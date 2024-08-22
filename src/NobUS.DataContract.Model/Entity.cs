using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace NobUS.DataContract.Model;

public abstract record EntityBase<TId>(TId Id);

public record RouteStation(int Id, RouteStation.Type Variant) : EntityBase<int>(Id)
{
    public enum Type
    {
        To,
        From,
        Both,
        Twin
    }

    [JsonIgnore]
    public RouteStation? Opposite =>
        Variant == Type.Twin ? new RouteStation(Id / 10 + 10 - Id % 10, Variant) : null;
}

public record Route(string Name, List<RouteStation> Stations, Route.Type Variant, string Operator)
    : EntityBase<string>($"{Operator}-{Name}")
{
    public enum Type
    {
        Loop,
        Bidirectional,
        Unidirectional
    }

    [JsonIgnore]
    public RouteStation Origin =>
        Variant switch
        {
            Type.Loop or Type.Unidirectional => Stations[0],
            Type.Bidirectional => Stations.Find(x => FilterStations(true).Invoke(x))!,
            _ => throw new NotImplementedException()
        };

    [JsonIgnore]
    public RouteStation[] ToStations =>
        Variant switch
        {
            Type.Bidirectional
            or Type.Unidirectional
                => Stations.Where(FilterStations(true)).ToArray(),
            Type.Loop
                => Stations
                    .Where(FilterStations(true))
                    .Concat(
                        Stations
                            .Where(FilterStations(false))
                            .Reverse()
                            .Select(x => x.Variant is RouteStation.Type.Twin ? x.Opposite! : x)
                    )
                    .ToArray(),
            _ => throw new NotImplementedException()
        };

    [JsonIgnore]
    public RouteStation[] FromStations => Stations.Where(FilterStations(false)).ToArray();

    private static Func<RouteStation, bool> FilterStations(
        bool isTo,
        bool includeShared = true,
        bool includeTwin = true
    ) =>
        station =>
        {
            return station.Variant switch
            {
                RouteStation.Type.To => isTo,
                RouteStation.Type.From => !isTo,
                RouteStation.Type.Twin => includeTwin,
                RouteStation.Type.Both => includeShared,
                _ => throw new NotImplementedException()
            };
        };
}

public record Station(int Code, string Name, string Road, Coordinate Coordinate)
{
    public int? Opposite { get; }
}

public record ShuttleJob(int Id, Route Route, Vehicle Vehicle);

public record Vehicle(MassPoint? MassPoint, string Plate);
