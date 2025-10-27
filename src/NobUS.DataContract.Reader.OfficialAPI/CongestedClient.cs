using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NobUS.DataContract.Model;
using NobUS.DataContract.Reader.OfficialAPI.Client;
using NobUS.Infrastructure;
using static NobUS.Infrastructure.DefinitionLoader;

namespace NobUS.DataContract.Reader.OfficialAPI;

public sealed class CongestedClient : IClient, IAsyncDisposable
{
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(40);

    private readonly SchemaClient _client;
    private readonly ConcurrentDictionary<int, ShuttleJob> _shuttleJobs = new();
    private readonly ConcurrentDictionary<string, Vehicle> _vehicles = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly PeriodicTimer _timer = new(UpdateInterval);
    private readonly Task _initializationTask;
    private readonly Task _backgroundUpdater;

    public CongestedClient(SchemaClient client)
    {
        _client = client;
        _initializationTask = InitializeAsync(_cts.Token);
        _backgroundUpdater = RunUpdaterAsync(_cts.Token);
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        foreach (var station in Stations.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await EnsureVehiclesForStationAsync(station, cancellationToken).ConfigureAwait(false);
        }

        foreach (var station in Stations.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await EnsureShuttleJobsForStationAsync(station, cancellationToken)
                .ConfigureAwait(false);
        }

        foreach (var route in Routes.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await EnsureShuttleJobsForRouteAsync(route, cancellationToken).ConfigureAwait(false);
        }

        await UpdateVehicleLocationsAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureVehiclesForStationAsync(
        Station station,
        CancellationToken cancellationToken
    )
    {
        var response = await _client
            .GetShuttleServiceAsync(station.QueryName(), cancellationToken)
            .ConfigureAwait(false);

        var plates = Utility
            .GetEtasFromShuttles(response.ShuttleServiceResult.Shuttles)
            .Select(eta => eta.Plate)
            .Where(plate => !string.IsNullOrWhiteSpace(plate))
            .Select(plate => plate!);

        foreach (var plate in plates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _vehicles.TryAdd(plate, new Vehicle(null, plate));
        }
    }

    private async Task EnsureShuttleJobsForStationAsync(
        Station station,
        CancellationToken cancellationToken
    )
    {
        var response = await _client
            .GetShuttleServiceAsync(station.QueryName(), cancellationToken)
            .ConfigureAwait(false);

        foreach (
            var (routeName, etas) in Utility.GetRouteNameAndEtasFromShuttles(
                response.ShuttleServiceResult.Shuttles
            )
        )
        {
            if (!Routes.TryGetValue(routeName, out var route))
            {
                continue;
            }

            foreach (var eta in etas.Where(eta => eta.Plate is not null && eta.Jobid.HasValue))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var plate = eta.Plate!;
                var vehicle = _vehicles.GetOrAdd(plate, _ => new Vehicle(null, plate));
                _shuttleJobs.AddOrUpdate(
                    eta.Jobid!.Value,
                    _ => new ShuttleJob(eta.Jobid.Value, route, vehicle),
                    (_, existing) => existing with { Vehicle = vehicle }
                );
            }
        }
    }

    private async Task EnsureShuttleJobsForRouteAsync(
        Route route,
        CancellationToken cancellationToken
    )
    {
        var response = await _client
            .GetActiveBusAsync(route.Name, cancellationToken)
            .ConfigureAwait(false);

        var activeBuses = response.ActiveBusResult.Activebus ?? Enumerable.Empty<Activebus>();
        foreach (var bus in activeBuses.Where(bus => !string.IsNullOrWhiteSpace(bus?.Vehplate)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var plate = bus!.Vehplate!;
            var massPoint = Adapter.AdaptMassPoint(bus);
            var vehicle = _vehicles.AddOrUpdate(
                plate,
                _ => new Vehicle(massPoint, plate),
                (_, existing) => existing with { MassPoint = massPoint }
            );

            foreach (
                var (jobId, job) in _shuttleJobs.Where(kvp => kvp.Value.Vehicle.Plate == plate)
            )
            {
                _shuttleJobs.TryUpdate(jobId, job with { Vehicle = vehicle }, job);
            }
        }
    }

    private async Task RunUpdaterAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _initializationTask.ConfigureAwait(false);
            while (await _timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                await UpdateVehicleLocationsAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }

    private async Task UpdateVehicleLocationsAsync(CancellationToken cancellationToken)
    {
        foreach (var route in Routes.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await _client
                .GetActiveBusAsync(route.Name, cancellationToken)
                .ConfigureAwait(false);

            var activeBuses = response.ActiveBusResult.Activebus ?? Enumerable.Empty<Activebus>();
            foreach (var bus in activeBuses.Where(bus => !string.IsNullOrWhiteSpace(bus?.Vehplate)))
            {
                var plate = bus!.Vehplate!;
                var massPoint = Adapter.AdaptMassPoint(bus);
                _vehicles.AddOrUpdate(
                    plate,
                    _ => new Vehicle(massPoint, plate),
                    (_, existing) => existing with { MassPoint = massPoint }
                );
            }
        }
    }

    public Task<IImmutableList<Station>> GetStationsAsync() =>
        Task.FromResult<IImmutableList<Station>>(Stations.Values.ToImmutableList());

    public Task<IImmutableList<Route>> GetRoutesAsync() =>
        Task.FromResult<IImmutableList<Route>>(Routes.Values.ToImmutableList());

    public async Task<IImmutableList<Vehicle>> GetVehiclesAsync()
    {
        await _initializationTask.ConfigureAwait(false);
        return _vehicles.Values.ToImmutableList();
    }

    public async Task<IImmutableList<ShuttleJob>> GetShuttleJobsAsync()
    {
        await _initializationTask.ConfigureAwait(false);
        return _shuttleJobs.Values.ToImmutableList();
    }

    public Task<IImmutableList<RouteStation>> GetStationsAsync(Route route) =>
        Task.FromResult<IImmutableList<RouteStation>>(route.Stations.ToImmutableList());

    public async Task<IImmutableList<ArrivalEvent>> GetArrivalEventsAsync(Station station)
    {
        await _initializationTask.ConfigureAwait(false);
        var response = await _client
            .GetShuttleServiceAsync(station.QueryName())
            .ConfigureAwait(false);

        return Utility
            .GetRouteNameAndEtasFromShuttles(response.ShuttleServiceResult.Shuttles)
            .SelectMany(ss =>
                ss._etas.Select(eta => Adapter.AdaptArrivalEvent(station.Code, ss.RouteName, eta))
            )
            .ToImmutableList();
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _timer.Dispose();
        try
        {
            await _backgroundUpdater.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        _cts.Dispose();
    }
}
