using System.Collections.Concurrent;
using System.Collections.Immutable;
using NobUS.DataContract.Model.Entity;
using NobUS.DataContract.Model.ValueObject.Snapshot;
using NobUS.DataContract.Reader.OfficialAPI.Client;

namespace NobUS.DataContract.Reader.OfficialAPI
{
    public record CongestedClient : IClient
    {
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromMinutes(3);
        private readonly SchemaClient _client;

        private readonly ConcurrentDictionary<string, Route> _routes = new();
        private readonly ConcurrentDictionary<int, ShuttleJob> _shuttleJobs = new();
        private readonly ConcurrentDictionary<string, Station> _stations = new();
        private readonly ConcurrentDictionary<string, Vehicle> _vehicles = new();

        public CongestedClient() => _client = new SchemaClient(Utility.GetHttpClientWithAuth());

        private Task InitAsync => Task.WhenAll(InitStations, InitRoutes, InitVehicles)
            .ContinueWith(_ => _routes.Values.Select(InitShuttleJobs))
            .ContinueWith(_ => BackgroundUpdater);

        private Task BackgroundUpdater => Task.Run(async () =>
        {
            while (true)
            {
                await UpdateVehicleLocations();
                await Task.Delay(UpdateInterval);
            }
            // ReSharper disable once FunctionNeverReturns
        });

        private Task InitStations => Task.Run(_client.GetListOfBusStopsAsync)
            .ContinueWith(ss =>
                ss.Result.BusStopsResult.Busstops.Select(Adapter.AdaptStation).Select(s => _stations.TryAdd(s.Name, s))
                    .ToList());

        private Task InitRoutes =>
            Task.Run(_client.GetServiceDescriptionAsync)
                .ContinueWith(rs => rs.Result.ServiceDescriptionResult.ServiceDescription.Select(r =>
                        Adapter.AdaptRoute(r, _client.PickupPointAsync(r.Route).Result.PickupPointResult.Pickuppoint))
                    .Select(r => _routes.TryAdd(r.Name, r)));

        private Task InitVehicles =>
            InitStations.ContinueWith(_ => _stations.Values
                .Select(s => _client.GetShuttleServiceAsync(s.Name).Result.ShuttleServiceResult.Shuttles)
                .Select(Utility.GetEtasFromShuttles)
                .Select(etas => etas.Select(eta => eta.Plate))
                .Select(p => p.Where(key => !_vehicles.ContainsKey(key)).Select(plate => new Vehicle(null, plate)))
                .Select(x => x.Select(vehicle => _vehicles.TryAdd(vehicle.Plate, vehicle)))
                .ToList());

        public async Task<IImmutableList<Station>> GetStationsAsync() =>
            await InitStations.ContinueWith(_ => _stations.Values.ToImmutableList());

        public async Task<IImmutableList<Route>> GetRoutesAsync() =>
            await InitRoutes.ContinueWith(_ => _routes.Values.ToImmutableList());

        public async Task<IImmutableList<Vehicle>> GetVehiclesAsync() =>
            await InitVehicles.ContinueWith(_ => _vehicles.Values.ToImmutableList());

        public async Task<IImmutableList<ShuttleJob>> GetShuttleJobsAsync() =>
            await InitAsync.ContinueWith(_ => _shuttleJobs.Values.ToImmutableList());

        public async Task<IImmutableList<Station>> GetStationsAsync(Route route) =>
            await InitRoutes.ContinueWith(_ => route.Stations.ToImmutableList());

        public async Task<IImmutableList<ArrivalEvent>> GetArrivalEventsAsync(Station station) =>
            await Task.Run(() =>
                _client.GetShuttleServiceAsync(station.Name).Result.ShuttleServiceResult.Shuttles
                    .SelectMany(ss => ss._etas).Select(eta => Adapter.AdaptArrivalEvent(station, eta, _shuttleJobs))
                    .ToImmutableList());

        public async Task<IImmutableList<TResult>> GetAsync<TResult>() where TResult : class
        {
            return Type.GetTypeCode(typeof(TResult)) switch
            {
                TypeCode.Object => typeof(TResult) switch
                {
                    { } t when t == typeof(Station) => (IImmutableList<TResult>)await GetStationsAsync(),
                    { } t when t == typeof(Route) => (IImmutableList<TResult>)await GetRoutesAsync(),
                    { } t when t == typeof(Vehicle) => (IImmutableList<TResult>)await GetVehiclesAsync(),
                    { } t when t == typeof(ShuttleJob) => (IImmutableList<TResult>)await GetShuttleJobsAsync(),
                    _ => ImmutableList<TResult>.Empty
                },
                _ => ImmutableList<TResult>.Empty
            };
        }

        public async Task<IImmutableList<TResult>> GetAsync<TResult, TQuery>(TQuery query)
            where TResult : class
            where TQuery : class
        {
            return Type.GetTypeCode(typeof(TResult)) switch
            {
                TypeCode.Object => typeof(TResult) switch
                {
                    { } t when t == typeof(ArrivalEvent) && query is Station station =>
                        (IImmutableList<TResult>)await GetArrivalEventsAsync(station),
                    { } t when t == typeof(Station) && query is Route route =>
                        (IImmutableList<TResult>)await GetStationsAsync(route),
                    _ => ImmutableList<TResult>.Empty
                },
                _ => ImmutableList<TResult>.Empty
            };
        }

        private Task InitShuttleJobs(Route route) =>
            Task.Run(() => (_client.GetActiveBusAsync(route.Name).Result.ActiveBusResult.Activebus, _stations.Values
                    .Select(station =>
                        Utility.GetEtasFromShuttles(_client.GetShuttleServiceAsync(station.Name).Result
                            .ShuttleServiceResult
                            .Shuttles))
                    .SelectMany(e => e)))
                .ContinueWith(tuple => tuple.Result.Activebus
                    .Where(b => tuple.Result.Item2.All(e => e.Plate != b.Vehplate))
                    .Select(b => new ShuttleJob(tuple.Result.Item2.First(e => e.Plate == b.Vehplate).Jobid, route,
                        _vehicles[b.Vehplate]))
                    .Select(j => _shuttleJobs.TryAdd(j.Id, j)).ToList());

        private Task UpdateVehicleLocations() => InitVehicles.ContinueWith(_ => _routes.Values.Select(r => _client
            .GetActiveBusAsync(r.Name).Result.ActiveBusResult.Activebus.Select(
                b => _vehicles[b.Vehplate] = _vehicles[b.Vehplate] with { MassPoint = Adapter.AdaptMassPoint(b) })
            .ToList()).ToList());
    }
}