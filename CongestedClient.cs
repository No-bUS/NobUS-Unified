using NobUS.DataContract.Model.Entity;
using NobUS.DataContract.Model.ValueObject.Snapshot;
using NobUS.DataContract.Reader.OfficialAPI.Client;
using System.Collections.Immutable;

namespace NobUS.DataContract.Reader.OfficialAPI;

public partial record CongestedClient : IClient
{
    public CongestedClient(SchemaClient? client, HttpClient? httpClient)
    {
        var _httpClient = httpClient ?? Utility.GetHttpClientWithAuth();
        _client = client ?? new SchemaClient(_httpClient);
        InitStations();
        InitRoutes();
        InitVehicles();
        foreach (var route in _routeSet)
        {
            InitShuttleJobs(route);
        }
    }

    public CongestedClient()
    {
        _client = new SchemaClient(Utility.GetHttpClientWithAuth());
        InitStations();
        InitRoutes();
        InitVehicles();
        foreach (var route in _routeSet)
        {
            InitShuttleJobs(route);
        }
    }

    private readonly SchemaClient _client;
    private readonly Dictionary<string, Route> _routes = new();
    private readonly HashSet<Route> _routeSet = new();
    private readonly Dictionary<string, Station> _stations = new();
    private readonly HashSet<Station> _stationSet = new();
    private readonly Dictionary<int, ShuttleJob> _shuttleJobs = new();
    private readonly HashSet<ShuttleJob> _shuttleJobSet = new();
    private readonly Dictionary<string, Vehicle> _vehicles = new();
    private readonly HashSet<Vehicle> _vehicleSet = new();

    private void InitStations()
    {
        var rawStations = _client.GetListOfBusStopsAsync();
        ICollection<Busstops> stations = rawStations.Result.BusStopsResult.Busstops;
        foreach (var station in stations)
        {
            var adaptedStation = Adapter.AdaptStation(station);
            _stations.Add(station.Name, adaptedStation);
            _stationSet.Add(adaptedStation);
        }
    }

    private void InitRoutes()
    {
        var rawRoutes = _client.GetServiceDescriptionAsync();
        ICollection<ServiceDescription> routes = rawRoutes.Result.ServiceDescriptionResult.ServiceDescription;
        foreach (var route in routes)
        {
            var rawRouteStops = _client.PickupPointAsync(route.Route);
            var adaptedRoute = Adapter.AdaptRoute(route, rawRouteStops.Result.PickupPointResult.Pickuppoint);
            _routes.Add(route.Route, adaptedRoute);
            _routeSet.Add(adaptedRoute);
        }
    }

    private void InitVehicles()
    {
        IEnumerable<string> StationToPlatesConverter(Station station) =>
          Utility.GetEtasFromShuttles(_client.GetShuttleServiceAsync(station.Name)
            .Result.ShuttleServiceResult.Shuttles).Select(eta => eta.Plate);

        var plates = _stationSet.Select(StationToPlatesConverter);
        var runningVehicles = plates
          .SelectMany(p => p)
          .Where(p => !_vehicles.ContainsKey(p))
          .Select(p => new Vehicle(null, p));
        foreach (var runningVehicle in runningVehicles)
        {
            _vehicles.Add(runningVehicle.Plate, runningVehicle);
            _vehicleSet.Add(runningVehicle);
        }

    }

    public void UpdateVehicleLocations()
    {
        foreach (var route in _routeSet)
        {
            var activeBuses = _client.GetActiveBusAsync(route.Name).Result.ActiveBusResult.Activebus;
            foreach (var activeBus in activeBuses)
            {
                var vehicle = _vehicles[activeBus.Vehplate];
                var updatedVehicle = vehicle with { MassPoint = Adapter.AdaptMassPoint(activeBus) };
                _vehicles[activeBus.Vehplate] = updatedVehicle;
                _vehicleSet.Remove(vehicle);
                _vehicleSet.Add(updatedVehicle);
            }
        }
    }

    private void InitShuttleJobs(Route route)
    {
        var rawShuttleJobs = _client.GetActiveBusAsync(route.Name);
        var rawOngoingShuttleJobs = _stationSet.Select(station =>
          Utility.GetEtasFromShuttles(_client.GetShuttleServiceAsync(station.Name).Result.ShuttleServiceResult.Shuttles))
          .SelectMany(e => e).ToList();
        ICollection<Activebus> shuttleJobs = rawShuttleJobs.Result.ActiveBusResult.Activebus;
        if (rawOngoingShuttleJobs.Count < 1) return;
        foreach (var shuttleJob in shuttleJobs)
        {
            if (!rawOngoingShuttleJobs.Any(e => e.Plate == shuttleJob.Vehplate))
            {
                continue;
            }
            var adaptedShuttleJob = new ShuttleJob(rawOngoingShuttleJobs.First(e => e.Plate == shuttleJob.Vehplate).Jobid,
              route, _vehicles[shuttleJob.Vehplate]);
            _shuttleJobs.Add(adaptedShuttleJob.Id, adaptedShuttleJob);
            _shuttleJobSet.Add(adaptedShuttleJob);
        }
    }

    public IEnumerable<Station> GetStations() => _stationSet.ToImmutableList();
    public async Task<IImmutableList<Station>> GetStationsAsync() => await Task.FromResult(_stationSet.ToImmutableList());
    public async Task<IImmutableList<Station>> GetStationsAsync(CancellationToken ct) => await Task.FromResult(_stationSet.ToImmutableList());
    public IEnumerable<Route> GetRoutes() => _routeSet.ToImmutableList();
    public async Task<IImmutableList<Route>> GetRoutesAsync() => await Task.FromResult(_routeSet.ToImmutableList());

    public IEnumerable<Vehicle> GetVehicles()
    {
        UpdateVehicleLocations();
        return _vehicleSet.ToImmutableList();
    }
    public async Task<IImmutableList<Vehicle>> GetVehiclesAsync()
    {
        UpdateVehicleLocations();
        return await Task.FromResult(_vehicleSet.ToImmutableList());
    }

    public IEnumerable<ShuttleJob> GetShuttleJobs() => _shuttleJobSet.ToImmutableList();
    public async Task<IImmutableList<ShuttleJob>> GetShuttleJobsAsync() => await Task.FromResult(_shuttleJobSet.ToImmutableList());
    public ImmutableDictionary<string, Station> GetStationsDictionary() => _stations.ToImmutableDictionary();
    public ImmutableDictionary<string, Route> GetRoutesDictionary() => _routes.ToImmutableDictionary();

    public ImmutableDictionary<string, Vehicle> GetVehiclesDictionary()
    {
        UpdateVehicleLocations();
        return _vehicles.ToImmutableDictionary();
    }

    public ImmutableDictionary<int, ShuttleJob> GetShuttleJobsDictionary() => _shuttleJobs.ToImmutableDictionary();

    public IEnumerable<Station> GetStations(Route route) => route.Stations.ToImmutableList();
    public async Task<IImmutableList<Station>> GetStationsAsync(Route route) => await Task.FromResult(route.Stations.ToImmutableList());

    public IEnumerable<ArrivalEvent> GetArrivalEvents(Station station)
      => _client.GetShuttleServiceAsync(station.Name).Result.ShuttleServiceResult.Shuttles.SelectMany(ss => ss._etas)
        .Select(eta => Adapter.AdaptArrivalEvent(station, eta, _shuttleJobs));
    public async Task<IImmutableList<ArrivalEvent>> GetArrivalEventsAsync(Station station)
      => await Task.FromResult(_client.GetShuttleServiceAsync(station.Name).Result.ShuttleServiceResult.Shuttles.SelectMany(ss => ss._etas)
                 .Select(eta => Adapter.AdaptArrivalEvent(station, eta, _shuttleJobs)).ToImmutableList());

    public async Task<IImmutableList<T>> GetAsync<T>() where T : class
    {
        switch (Type.GetTypeCode(typeof(T)))
        {
            case TypeCode.Object:
                switch (typeof(T))
                {
                    case Type t when t == typeof(Station):
                        return (IImmutableList<T>)await GetStationsAsync();
                    case Type t when t == typeof(Route):
                        return (IImmutableList<T>)await GetRoutesAsync();
                    case Type t when t == typeof(Vehicle):
                        return (IImmutableList<T>)await GetVehiclesAsync();
                    case Type t when t == typeof(ShuttleJob):
                        return (IImmutableList<T>)await GetShuttleJobsAsync();

                }
                return await Task.FromResult(ImmutableList<T>.Empty);
            default:
                return await Task.FromResult(ImmutableList<T>.Empty);
        }
    }
}
