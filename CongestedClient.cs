using NobUS.DataContract.Model.Entity;
using NobUS.DataContract.Model.ValueObject.Snapshot;
using NobUS.DataContract.Reader.OfficialAPI.Client;
using System.Collections.Immutable;

namespace NobUS.DataContract.Reader.OfficialAPI;

public class CongestedClient
{
  public CongestedClient(SchemaClient? client, HttpClient? httpClient)
  {
    _client = client ?? new SchemaClient(httpClient ?? new HttpClient());
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
      _client.GetShuttleServiceAsync(station.Name)
        .Result.ShuttleServiceResult.Shuttles.SelectMany(ss => ss._etas)
        .Select(eta => eta.Plate);

    var runningVehicles = _stationSet.Select(StationToPlatesConverter).SelectMany(p => p)
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
        _client.GetShuttleServiceAsync(station.Name).Result.ShuttleServiceResult.Shuttles.SelectMany(ss => ss._etas))
      .SelectMany(e => e);
    ICollection<Activebus> shuttleJobs = rawShuttleJobs.Result.ActiveBusResult.Activebus;
    foreach (var shuttleJob in shuttleJobs)
    {
      var adaptedShuttleJob = new ShuttleJob(rawOngoingShuttleJobs.First(e => e.Plate == shuttleJob.Vehplate).Jobid,
        route, _vehicles[shuttleJob.Vehplate]);
      _shuttleJobs.Add(adaptedShuttleJob.Id, adaptedShuttleJob);
      _shuttleJobSet.Add(adaptedShuttleJob);
    }
  }

  public IEnumerable<Station> GetStations() => _stationSet.ToImmutableList();
  public IEnumerable<Route> GetRoutes() => _routeSet.ToImmutableList();

  public IEnumerable<Vehicle> GetVehicles()
  {
    UpdateVehicleLocations();
    return _vehicleSet.ToImmutableList();
  }

  public IEnumerable<ShuttleJob> GetShuttleJobs() => _shuttleJobSet.ToImmutableList();
  public ImmutableDictionary<string, Station> GetStationsDictionary() => _stations.ToImmutableDictionary();
  public ImmutableDictionary<string, Route> GetRoutesDictionary() => _routes.ToImmutableDictionary();

  public ImmutableDictionary<string, Vehicle> GetVehiclesDictionary()
  {
    UpdateVehicleLocations();
    return _vehicles.ToImmutableDictionary();
  }

  public ImmutableDictionary<int, ShuttleJob> GetShuttleJobsDictionary() => _shuttleJobs.ToImmutableDictionary();

  public IEnumerable<Station> GetStations(Route route) => route.Stations;

  public IEnumerable<ArrivalEvent> GetArrivalEvents(Station station)
    => _client.GetShuttleServiceAsync(station.Name).Result.ShuttleServiceResult.Shuttles.SelectMany(ss => ss._etas)
      .Select(eta => Adapter.AdaptArrivalEvent(station, eta, _shuttleJobs));
}