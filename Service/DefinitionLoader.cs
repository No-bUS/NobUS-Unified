using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Resources = NobUS.Frontend.MAUI.Resources.Definitions.Resources;
using NobUS.DataContract.Model;

namespace NobUS.Frontend.MAUI.Service
{
    [JsonObject]
    internal record QueryNameMap(string QueryName, int Code);

    internal static class DefinitionLoader
    {
        internal static readonly ReadOnlyDictionary<int, string> QueryNameMapping = JsonConvert
            .DeserializeObject<QueryNameMap[]>(Resources.Definitions.Resources.NUS_Mapping)!
            .ToDictionary(m => m.Code, m => m.QueryName)
            .AsReadOnly();
        private static readonly ReadOnlyDictionary<string, Station> Stations = JsonConvert
            .DeserializeObject<Station[]>(Resources.Definitions.Resources.NUS_Stations)!
            .Concat(
                JsonConvert.DeserializeObject<Station[]>(
                    Resources.Definitions.Resources.Public_Stations
                )!
            )
            .Where(s => QueryNameMapping.ContainsKey(s.Code))
            .ToDictionary(s => s.Name)
            .AsReadOnly();
        private static readonly ReadOnlyDictionary<string, Route> Routes = JsonConvert
            .DeserializeObject<Route[]>(Resources.Definitions.Resources.NUS_Routes)!
            .ToDictionary(r => r.Name)
            .AsReadOnly();

        public static IImmutableList<Station> GetAllStations => Stations.Values.ToImmutableList();

        public static IImmutableList<Route> GetAllRoutes => Routes.Values.ToImmutableList();

        public static Route GetRouteByName(string name) => Routes[name];

        public static Station GetStationByCode(int code) =>
            Stations.Values.FirstOrDefault(s => s.Code == code);
    }
}
