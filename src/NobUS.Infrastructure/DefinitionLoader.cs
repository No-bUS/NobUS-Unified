using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using NobUS.DataContract.Model;
using NobUS.Resources.Definitions;

namespace NobUS.Infrastructure
{
    [JsonObject]
    public record QueryNameMap(string QueryName, int Code);

    public static class StationExtension
    {
        public static string QueryName(this Station station) =>
            DefinitionLoader.QueryNameMapping.GetValueOrDefault(station.Code, "UTOWN");
    }

    public static class DefinitionLoader
    {
        public static readonly ReadOnlyDictionary<int, string> QueryNameMapping = JsonConvert
            .DeserializeObject<QueryNameMap[]>(StationsNRoutes.NUS_Mapping)!
            .ToDictionary(m => m.Code, m => m.QueryName)
            .AsReadOnly();
        public static readonly ReadOnlyDictionary<string, Station> Stations = JsonConvert
            .DeserializeObject<Station[]>(StationsNRoutes.NUS_Stations)!
            .Concat(JsonConvert.DeserializeObject<Station[]>(StationsNRoutes.Public_Stations)!)
            .Where(s => QueryNameMapping.ContainsKey(s.Code))
            .ToDictionary(s => s.Name)
            .AsReadOnly();
        public static readonly ReadOnlyDictionary<string, Route> Routes = JsonConvert
            .DeserializeObject<Route[]>(StationsNRoutes.NUS_Routes)!
            .ToDictionary(r => r.Name)
            .AsReadOnly();

        public static IImmutableList<Station> GetAllStations => Stations.Values.ToImmutableList();

        public static IImmutableList<Route> GetAllRoutes => Routes.Values.ToImmutableList();

        public static Route GetRouteByName(string name) => Routes[name];

        public static Station? GetStationByCode(int code) =>
            Stations.Values.FirstOrDefault(s => s.Code == code);
    }
}
