using NobUS.DataContract.Model.Entity;
using NobUS.DataContract.Model.ValueObject;
using NobUS.DataContract.Model.ValueObject.Snapshot;
using NobUS.DataContract.Reader.OfficialAPI.Client;

namespace NobUS.DataContract.Reader.OfficialAPI
{
  internal static class Adapter
  {
    internal static Route AdaptRoute(ServiceDescription rawRouteMetadata, IEnumerable<Pickuppoint> rawRouteStops) =>
      new(rawRouteMetadata.Route, rawRouteMetadata.Route, rawRouteStops.Select(AdaptStation).ToArray());

    internal static Station AdaptStation(Busstops rawStation) =>
      new(-1, rawStation.Name, rawStation.Caption, new Coordinate(rawStation.Latitude, rawStation.Longitude));

    internal static Station AdaptStation(Pickuppoint rawStation) =>
      new(-1, rawStation.Busstopcode, rawStation.Pickupname, new Coordinate(rawStation.Lng, rawStation.Lat));

    internal static ArrivalEvent AdaptArrivalEvent(Station station, _etas rawEta, IDictionary<int, ShuttleJob> shuttleJobDictionary) =>
      new(station, shuttleJobDictionary[rawEta.Jobid], new TimeSpan(0, 0, rawEta.Eta_s), DateTime.Now);

    internal static MassPoint AdaptMassPoint(Activebus rawMassPoint) =>
      new(new Coordinate(rawMassPoint.Lng, rawMassPoint.Lat), new Velocity(rawMassPoint.Speed, rawMassPoint.Direction));
  }
}