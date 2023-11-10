using NobUS.DataContract.Model;
using NobUS.DataContract.Reader.OfficialAPI.Client;

namespace NobUS.DataContract.Reader.OfficialAPI
{
    internal static class Adapter
    {
        internal static ArrivalEvent AdaptArrivalEvent(
            int stationCode,
            string routeName,
            _etas rawEta
        ) =>
            new(
                stationCode,
                rawEta.Jobid,
                routeName,
                TimeSpan.FromSeconds(rawEta.Eta_s),
                DateTime.Now
            );

        internal static MassPoint AdaptMassPoint(Activebus rawMassPoint) =>
            new(
                new Coordinate(rawMassPoint.Lng, rawMassPoint.Lat),
                new Velocity(rawMassPoint.Speed, rawMassPoint.Direction)
            );
    }
}
