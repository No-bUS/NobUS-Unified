using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NobUS.DataContract.Model;
using NobUS.DataContract.Reader.OfficialAPI.Client;
using NobUS.Infrastructure;
using static NobUS.Infrastructure.DefinitionLoader;

namespace NobUS.DataContract.Reader.OfficialAPI;

public sealed class CongestedClient : IClient
{
    internal static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(40);

    private readonly SchemaClient _client;
    private readonly ConcurrentDictionary<int, ShuttleJob> _shuttleJobs = new();
    private readonly ConcurrentDictionary<string, Vehicle> _vehicles = new();

    private readonly SemaphoreSlim _vehicleInitLock = new(1, 1);
    private readonly SemaphoreSlim _shuttleInitLock = new(1, 1);
    private readonly SemaphoreSlim _fullInitLock = new(1, 1);

    private Task? _vehicleInitialization;
    private Task? _shuttleInitialization;
    private Task? _fullInitialization;

    public CongestedClient()
    {
        _client = new SchemaClient(Utility.GetHttpClientWithAuth());
    }

    internal async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_fullInitialization is null)
        {
            await _fullInitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _fullInitialization ??= InitializeAsync(cancellationToken);
            }
            finally
            {
                _fullInitLock.Release();
            }
        }

        await _fullInitialization!.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await EnsureVehiclesInitializedAsync(cancellationToken).ConfigureAwait(false);
        await EnsureShuttleJobsInitializedAsync(cancellationToken).ConfigureAwait(false);
    }

    internal async Task EnsureVehiclesInitializedAsync(CancellationToken cancellationToken)
    {
        if (_vehicleInitialization is null)
        {
            await _vehicleInitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _vehicleInitialization ??= LoadVehiclesAsync();
            }
            finally
            {
                _vehicleInitLock.Release();
            }
        }

        await _vehicleInitialization!.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    internal async Task EnsureShuttleJobsInitializedAsync(CancellationToken cancellationToken)
    {
        if (_shuttleInitialization is null)
        {
            await _shuttleInitLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _shuttleInitialization ??= LoadShuttleJobsAsync();
            }
            finally
            {
                _shuttleInitLock.Release();
            }
        }

        await _shuttleInitialization!.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task LoadVehiclesAsync()
    {
        var loadTasks = Stations.Values.Select(async station =>
        {
            var shuttleService = await _client
                .GetShuttleServiceAsync(station.QueryName())
                .ConfigureAwait(false);

            foreach (var plate in Utility
                .GetEtasFromShuttles(shuttleService.ShuttleServiceResult.Shuttles)
                .Select(eta => eta.Plate))
            {
                _vehicles.TryAdd(plate, new Vehicle(null, plate));
            }
        });

        await Task.WhenAll(loadTasks).ConfigureAwait(false);
    }

    private async Task LoadShuttleJobsAsync()
    {
        await EnsureVehiclesInitializedAsync(CancellationToken.None).ConfigureAwait(false);

        var stationTasks = Stations.Values.Select(InitShuttleJobsAsync);
        var routeTasks = Routes.Values.Select(InitShuttleJobsAsync);

        await Task.WhenAll(stationTasks.Concat(routeTasks)).ConfigureAwait(false);
    }

    internal async Task RefreshVehicleLocationsAsync(CancellationToken cancellationToken)
    {
        await EnsureVehiclesInitializedAsync(cancellationToken).ConfigureAwait(false);

        var updateTasks = Routes.Values.Select(async route =>
        {
            var activeBus = await _client.GetActiveBusAsync(route.Name).ConfigureAwait(false);
            foreach (var bus in activeBus.ActiveBusResult.Activebus)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _vehicles.AddOrUpdate(
                    bus.Vehplate,
                    plate => new Vehicle(Adapter.AdaptMassPoint(bus), plate),
                    (_, current) => current with { MassPoint = Adapter.AdaptMassPoint(bus) }
                );
            }
        });

        await Task.WhenAll(updateTasks).ConfigureAwait(false);
    }

    public async Task<IImmutableList<Station>> GetStationsAsync() =>
        await Task.FromResult(Stations.Values.ToImmutableList());

    public async Task<IImmutableList<Route>> GetRoutesAsync() =>
        await Task.FromResult(Routes.Values.ToImmutableList());

    public async Task<IImmutableList<Vehicle>> GetVehiclesAsync()
    {
        await EnsureVehiclesInitializedAsync(CancellationToken.None).ConfigureAwait(false);
        return _vehicles.Values.ToImmutableList();
    }

    public async Task<IImmutableList<ShuttleJob>> GetShuttleJobsAsync()
    {
        await EnsureShuttleJobsInitializedAsync(CancellationToken.None).ConfigureAwait(false);
        return _shuttleJobs.Values.ToImmutableList();
    }

    public async Task<IImmutableList<RouteStation>> GetStationsAsync(Route route) =>
        await Task.FromResult(route.Stations.ToImmutableList());

    public async Task<IImmutableList<ArrivalEvent>> GetArrivalEventsAsync(Station station)
    {
        await EnsureShuttleJobsInitializedAsync(CancellationToken.None).ConfigureAwait(false);

        var shuttleService = await _client
            .GetShuttleServiceAsync(station.QueryName())
            .ConfigureAwait(false);

        return Utility
            .GetRouteNameAndEtasFromShuttles(shuttleService.ShuttleServiceResult.Shuttles)
            .SelectMany(ss =>
                ss._etas.Select(eta => Adapter.AdaptArrivalEvent(station.Code, ss.RouteName, eta))
            )
            .ToImmutableList();
    }

    private async Task InitShuttleJobsAsync(Station station)
    {
        var shuttleService = await _client
            .GetShuttleServiceAsync(station.QueryName())
            .ConfigureAwait(false);

        foreach (var (routeName, etas) in Utility.GetRouteNameAndEtasFromShuttles(
            shuttleService.ShuttleServiceResult.Shuttles
        ))
        {
            foreach (var eta in etas)
            {
                if (!eta.Jobid.HasValue)
                {
                    continue;
                }

                var vehicle = _vehicles.AddOrUpdate(
                    eta.Plate,
                    plate => new Vehicle(null, plate),
                    (_, existing) => existing
                );

                _shuttleJobs.AddOrUpdate(
                    eta.Jobid.Value,
                    id => new ShuttleJob(id, Routes[routeName], vehicle),
                    (_, existing) => existing
                );
            }
        }
    }

    private async Task InitShuttleJobsAsync(Route route)
    {
        var activeBus = await _client.GetActiveBusAsync(route.Name).ConfigureAwait(false);
        foreach (var bus in activeBus.ActiveBusResult.Activebus)
        {
            var vehicle = _vehicles.AddOrUpdate(
                bus.Vehplate,
                plate => new Vehicle(Adapter.AdaptMassPoint(bus), plate),
                (_, existing) => existing with { MassPoint = Adapter.AdaptMassPoint(bus) }
            );

            var current = _shuttleJobs.FirstOrDefault(job => job.Value.Vehicle.Plate == vehicle.Plate);
            if (current.Equals(default(KeyValuePair<int, ShuttleJob>)))
            {
                continue;
            }

            _shuttleJobs.TryUpdate(
                current.Key,
                current.Value with { Vehicle = vehicle },
                current.Value
            );
        }
    }
}
