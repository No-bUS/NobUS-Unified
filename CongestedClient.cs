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
        private readonly Task _backgroundUpdater;
        private readonly SchemaClient _client;

        private readonly Task _initAsync;
        private readonly Task _initRoutes;
        private readonly Task _initShuttleJobsAll;
        private readonly Task _initStations;
        private readonly Task _initVehicles;

        private readonly ConcurrentDictionary<string, Route> _routes = new();
        private readonly ConcurrentDictionary<int, ShuttleJob> _shuttleJobs = new();
        private readonly ConcurrentDictionary<string, Station> _stations = new();
        private readonly ConcurrentDictionary<string, Vehicle> _vehicles = new();

        public CongestedClient()
        {
            _client = new SchemaClient(Utility.GetHttpClientWithAuth());
            _initStations = InitStations;
            _initRoutes = InitRoutes;
            _initVehicles = InitVehicles;
            _initShuttleJobsAll = InitShuttleJobsAll;

            _initAsync = InitAsync;
            _backgroundUpdater = BackgroundUpdater;
        }

        private Task InitAsync => Task.WhenAll(_initStations, _initRoutes, _initVehicles, _initShuttleJobsAll);

        private Task BackgroundUpdater => Task.Run(async () =>
        {
            while (true)
            {
                await UpdateVehicleLocations;
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
                .ContinueWith(
                    rs => rs.Result.ServiceDescriptionResult.ServiceDescription.Select(r =>
                            Adapter.AdaptRoute(r,
                                _client.PickupPointAsync(r.Route).Result.PickupPointResult.Pickuppoint))
                        .Select(r => _routes.TryAdd(r.Name, r)).ToList());

        private Task InitVehicles =>
            _initStations.ContinueWith(_ => _stations.Values
                .Select(s => _client.GetShuttleServiceAsync(s.Name).Result.ShuttleServiceResult.Shuttles)
                .Select(Utility.GetEtasFromShuttles)
                .Select(etas => etas.Select(eta => eta.Plate))
                .Select(p => p.Where(key => !_vehicles.ContainsKey(key)).Select(plate => new Vehicle(null, plate)))
                .Select(x => x.Select(vehicle => _vehicles.TryAdd(vehicle.Plate, vehicle)))
                .ToList());

        private Task InitShuttleJobsAll =>
            Task.WhenAll(_initRoutes, _initStations)
                .ContinueWith(_ => _stations.Values.Select(InitShuttleJobs).ToList())
                .ContinueWith(_ => _routes.Values.Select(InitShuttleJobs).ToList());

        private Task UpdateVehicleLocations => _initVehicles.ContinueWith(_ => _routes.Values.Select(r => _client
            .GetActiveBusAsync(r.Name).Result.ActiveBusResult.Activebus.Select(
                b => _vehicles.AddOrUpdate(b.Vehplate, key => new Vehicle(Adapter.AdaptMassPoint(b), key),
                    (_, value) => value with { MassPoint = Adapter.AdaptMassPoint(b) }))
            .ToList()).ToList());

        public async Task<IImmutableList<Station>> GetStationsAsync() =>
            await _initStations.ContinueWith(_ => _stations.Values.ToImmutableList());

        public async Task<IImmutableList<Route>> GetRoutesAsync() =>
            await _initRoutes.ContinueWith(_ => _routes.Values.ToImmutableList());

        public async Task<IImmutableList<Vehicle>> GetVehiclesAsync() =>
            await _initVehicles.ContinueWith(_ => _vehicles.Values.ToImmutableList());

        public async Task<IImmutableList<ShuttleJob>> GetShuttleJobsAsync() =>
            await _initAsync.ContinueWith(_ => _shuttleJobs.Values.ToImmutableList());

        public async Task<IImmutableList<Station>> GetStationsAsync(Route route) =>
            await _initRoutes.ContinueWith(_ => route.Stations.ToImmutableList());

        public async Task<IImmutableList<ArrivalEvent>> GetArrivalEventsAsync(Station station) =>
            await _initShuttleJobsAll.ContinueWith(_ =>
                _client.GetShuttleServiceAsync(station.Name).Result.ShuttleServiceResult.Shuttles
                    .SelectMany(ss => ss._etas).Select(eta => Adapter.AdaptArrivalEvent(station, eta, _shuttleJobs))
                    .ToImmutableList());

        private Task InitShuttleJobs(Station station) => Task.Run(() =>
            Utility.GetRouteNameAndEtasFromShuttles(_client.GetShuttleServiceAsync(station.Name).Result
                    .ShuttleServiceResult.Shuttles)
                .Select(x => x._etas.Select(e =>
                    _shuttleJobs.AddOrUpdate(e.Jobid, k => new ShuttleJob(k, _routes[x.RouteName],
                        _vehicles.AddOrUpdate(e.Plate, key => new Vehicle(null, key),
                            (_, oldValue) => oldValue)), (_, oldValue) => oldValue)).ToList()).ToList()
        );

        private Task InitShuttleJobs(Route route) =>
            Task.Run(() => _client.GetActiveBusAsync(route.Name).Result.ActiveBusResult.Activebus
                .Select(x => (
                    _vehicles.AddOrUpdate(x.Vehplate, key => new Vehicle(Adapter.AdaptMassPoint(x), key),
                        (_, value) => value with { MassPoint = Adapter.AdaptMassPoint(x) }),
                    _shuttleJobs.Any(s => s.Value.Vehicle.Plate == x.Vehplate)
                ))
                .Where(t => t.Item2)
                .Select(x =>
                    {
                        var (shuttleJobId, shuttleJob) =
                            _shuttleJobs.First(s => s.Value.Vehicle.Plate == x.Item1.Plate);
                        return _shuttleJobs.TryUpdate(shuttleJobId, shuttleJob with { Vehicle = x.Item1 }, shuttleJob);
                    }
                ).ToList());
    }
}