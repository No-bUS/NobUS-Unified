namespace NobUS.DataContract.Model
{

    public abstract record EntityBase<TId>(TId Id);

    public abstract record Node<TOther, TOtherId, TId>(TId Id, List<TOtherId> OtherIDs) : EntityBase<TId>(Id);

    public abstract record RouteStation(int Id) : EntityBase<int>(Id)
    {
        public record SoloStation(int Id, bool IsTo) : RouteStation(Id);
        public record TwinStation(int Id) : RouteStation(Id)
        {
            public TwinStation GetCounterpart() => Id % 10 == 1 ? new TwinStation(Id + 8) : new TwinStation(Id - 8);
        }
        public record SharedStation(int Id) : RouteStation(Id);
    }

    public abstract partial record Route(string Name, List<RouteStation> Stations) : Node<Station, int, string>(Name, Stations.Select(x => x.Id).ToList())
    {
        public abstract RouteStation Origin { get; }

        protected Func<RouteStation, bool> FilterStations(bool isTo, bool includeShared = true, bool includeTwin = true) =>
            station =>
            {
                return station switch
                {
                    RouteStation.SoloStation solo => solo.IsTo == isTo,
                    RouteStation.TwinStation _ => includeTwin,
                    RouteStation.SharedStation _ => includeShared,
                    _ => false
                };
            };

        public record LoopRoute(string Name, List<RouteStation> Stations) : Route(Name, Stations)
        {
            public override RouteStation Origin => Stations[0];
            public List<RouteStation> RoundTripStations =>
                Stations.Where(FilterStations(true))
                    .Concat(Stations.Where(FilterStations(false)).Reverse().Select(x => x is RouteStation.TwinStation station ? station.GetCounterpart() : x)).ToList();
        }
        public record BidirectionalRoute(string Name, List<RouteStation> Stations) : Route(Name, Stations)
        {
            public override RouteStation Origin => Stations.Find(x => FilterStations(isTo: true).Invoke(x))!;
        }
        public record UnidirectionalRoute(string Name, List<RouteStation> Stations) : Route(Name, Stations)
        {
            public override RouteStation Origin => Stations[0];
        }
    }
    
    public abstract record Station(int Code, string Name, string Road, Coordinate Coordinate, List<string> RouteIDs)
    {
        public record SoloStation(int Code, string Name, string Road, Coordinate Coordinate, List<string> RouteIDs) : Station(Code, Name, Road, Coordinate, RouteIDs);
        public record TwinStation(int Code, string Name, string Road, Coordinate Coordinate, int OppositeCode, List<string> RouteIDs) : Station(Code, Name, Road, Coordinate, RouteIDs);
    }

    public record ShuttleJob(int Id, Route Route, Vehicle Vehicle);
    public record Vehicle(MassPoint? MassPoint, string Plate);
}