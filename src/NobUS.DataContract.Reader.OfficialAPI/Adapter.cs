using System;
using NobUS.DataContract.Model;
using NobUS.DataContract.Reader.OfficialAPI.Client;

namespace NobUS.DataContract.Reader.OfficialAPI;

internal static class Adapter
{
    internal static ArrivalEvent AdaptArrivalEvent(
        int stationCode,
        string routeName,
        _etas rawEta
    ) =>
        new(
            stationCode,
            rawEta.Jobid ?? 0,
            routeName,
            TimeSpan.FromSeconds(Convert.ToDouble(rawEta.Eta_s ?? rawEta.Eta ?? 0)),
            DateTime.Now
        );

    internal static MassPoint AdaptMassPoint(Activebus rawMassPoint) =>
        new(
            new Coordinate(rawMassPoint.Lng ?? 0, rawMassPoint.Lat ?? 0),
            new Velocity(rawMassPoint.Speed ?? 0, rawMassPoint.Direction ?? 0)
        );
}
