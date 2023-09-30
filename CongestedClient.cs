﻿using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;
using Newtonsoft.Json;
using NobUS.DataContract.Model;
using NobUS.DataContract.Reader.OfficialAPI.Client;
using NobUS.DataContract.Reader.OfficialAPI.Properties;

namespace NobUS.DataContract.Reader.OfficialAPI
{
    [JsonObject]
    internal record QueryNameMapping(string QueryName, string Name);

    public record CongestedClient : IClient
    {
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromMinutes(3);
        private readonly Task _backgroundUpdater;
        private readonly SchemaClient _client;

        private readonly Task _initAsync;

        private readonly Task _initShuttleJobsAll;
        private readonly Task _initVehicles;

        private readonly ConcurrentDictionary<int, ShuttleJob> _shuttleJobs = new();

        private static readonly ReadOnlyDictionary<string, Station> Stations = JsonConvert
            .DeserializeObject<Station[]>(Encoding.UTF8.GetString(Resources.NUS_Stations))!
            .ToDictionary(s => s.Name)
            .AsReadOnly();
        private static readonly ReadOnlyDictionary<string, string> QueryNameMapping = JsonConvert
            .DeserializeObject<QueryNameMapping[]>(Encoding.UTF8.GetString(Resources.NUS_Mapping))!
            .ToDictionary(m => m.Name, m => m.QueryName)
            .AsReadOnly();
        private static readonly ReadOnlyDictionary<string, Route> Routes = JsonConvert
            .DeserializeObject<Route[]>(Encoding.UTF8.GetString(Resources.NUS_Routes))!
            .ToDictionary(r => r.Name)
            .AsReadOnly();

        private readonly ConcurrentDictionary<string, Vehicle> _vehicles = new();

        public CongestedClient()
        {
            _client = new SchemaClient(Utility.GetHttpClientWithAuth());
            _initVehicles = InitVehicles;
            _initShuttleJobsAll = InitShuttleJobsAll;

            _initAsync = InitAsync;
            _backgroundUpdater = BackgroundUpdater;
        }

        private Task InitAsync => Task.WhenAll(_initVehicles, _initShuttleJobsAll);

        private Task BackgroundUpdater =>
            Task.Run(async () =>
            {
                while (true)
                {
                    await UpdateVehicleLocations;
                    await Task.Delay(UpdateInterval);
                }
                // ReSharper disable once FunctionNeverReturns
            });

        private Task InitVehicles =>
            Task.Run(
                () =>
                    Stations.Values
                        .Select(
                            s =>
                                _client
                                    .GetShuttleServiceAsync(
                                        QueryNameMapping.GetValueOrDefault(s.Name)
                                    )
                                    .Result.ShuttleServiceResult.Shuttles
                        )
                        .Select(Utility.GetEtasFromShuttles)
                        .Select(etas => etas.Select(eta => eta.Plate))
                        .Select(
                            p =>
                                p.Where(key => !_vehicles.ContainsKey(key))
                                    .Select(plate => new Vehicle(null, plate))
                        )
                        .Select(x => x.Select(vehicle => _vehicles.TryAdd(vehicle.Plate, vehicle)))
                        .ToList()
            );

        private Task InitShuttleJobsAll =>
            Task.Run(() =>
            {
                Stations.Values.Select(InitShuttleJobs).ToList();
                Routes.Values.Select(InitShuttleJobs).ToList();
            });

        private Task UpdateVehicleLocations =>
            _initVehicles.ContinueWith(
                _ =>
                    Routes.Values
                        .Select(
                            r =>
                                _client
                                    .GetActiveBusAsync(r.Name)
                                    .Result.ActiveBusResult.Activebus.Select(
                                        b =>
                                            _vehicles.AddOrUpdate(
                                                b.Vehplate,
                                                key => new Vehicle(Adapter.AdaptMassPoint(b), key),
                                                (_, value) =>
                                                    value with
                                                    {
                                                        MassPoint = Adapter.AdaptMassPoint(b)
                                                    }
                                            )
                                    )
                                    .ToList()
                        )
                        .ToList()
            );

        public async Task<IImmutableList<Station>> GetStationsAsync() =>
            await Task.FromResult(Stations.Values.ToImmutableList());

        public async Task<IImmutableList<Route>> GetRoutesAsync() =>
            await Task.FromResult(Routes.Values.ToImmutableList());

        public async Task<IImmutableList<Vehicle>> GetVehiclesAsync() =>
            await _initVehicles.ContinueWith(_ => _vehicles.Values.ToImmutableList());

        public async Task<IImmutableList<ShuttleJob>> GetShuttleJobsAsync() =>
            await _initAsync.ContinueWith(_ => _shuttleJobs.Values.ToImmutableList());

        public async Task<IImmutableList<RouteStation>> GetStationsAsync(Route route) =>
            await Task.FromResult(route.Stations.ToImmutableList());

        public async Task<IImmutableList<ArrivalEvent>> GetArrivalEventsAsync(Station station) =>
            await _initShuttleJobsAll.ContinueWith(
                _ =>
                    _client
                        .GetShuttleServiceAsync(QueryNameMapping.GetValueOrDefault(station.Name))
                        .Result.ShuttleServiceResult.Shuttles.Where(ss => ss != null)
                        .Where(ss => ss._etas != null)
                        .SelectMany(ss => ss._etas)
                        .Select(eta => Adapter.AdaptArrivalEvent(station, eta, _shuttleJobs))
                        .ToImmutableList()
            );

        private Task InitShuttleJobs(Station station) =>
            Task.Run(
                () =>
                    Utility
                        .GetRouteNameAndEtasFromShuttles(
                            _client
                                .GetShuttleServiceAsync(
                                    QueryNameMapping.GetValueOrDefault(station.Name)
                                )
                                .Result.ShuttleServiceResult.Shuttles
                        )
                        .Select(
                            x =>
                                x._etas
                                    .Select(
                                        e =>
                                            _shuttleJobs.AddOrUpdate(
                                                e.Jobid,
                                                k =>
                                                    new ShuttleJob(
                                                        k,
                                                        Routes[x.RouteName],
                                                        _vehicles.AddOrUpdate(
                                                            e.Plate,
                                                            key => new Vehicle(null, key),
                                                            (_, oldValue) => oldValue
                                                        )
                                                    ),
                                                (_, oldValue) => oldValue
                                            )
                                    )
                                    .ToList()
                        )
                        .ToList()
            );

        private Task InitShuttleJobs(Route route) =>
            Task.Run(
                () =>
                    _client
                        .GetActiveBusAsync(route.Name)
                        .Result.ActiveBusResult.Activebus.Select(
                            x =>
                                (
                                    _vehicles.AddOrUpdate(
                                        x.Vehplate,
                                        key => new Vehicle(Adapter.AdaptMassPoint(x), key),
                                        (_, value) =>
                                            value with
                                            {
                                                MassPoint = Adapter.AdaptMassPoint(x)
                                            }
                                    ),
                                    _shuttleJobs.Any(s => s.Value.Vehicle.Plate == x.Vehplate)
                                )
                        )
                        .Where(t => t.Item2)
                        .Select(x =>
                        {
                            var (shuttleJobId, shuttleJob) = _shuttleJobs.First(
                                s => s.Value.Vehicle.Plate == x.Item1.Plate
                            );
                            return _shuttleJobs.TryUpdate(
                                shuttleJobId,
                                shuttleJob with
                                {
                                    Vehicle = x.Item1
                                },
                                shuttleJob
                            );
                        })
                        .ToList()
            );
    }
}
