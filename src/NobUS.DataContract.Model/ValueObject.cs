using System;

namespace NobUS.DataContract.Model;

public sealed partial record Coordinate(double Longitude, double Latitude)
{
    public override string ToString() => $"({Longitude}, {Latitude})";

    /// <summary>
    /// Credit: https://stackoverflow.com/a/60899418
    /// </summary>
    /// <returns>Distance in Km</returns>
    public double DistanceTo(Coordinate other) =>
        12745.6
        * Math.Asin(
            Math.Sqrt(
                SinSquareHalf(Latitude - other.Latitude)
                    + Math.Cos(Rad(Latitude))
                        * Math.Cos(Rad(other.Latitude))
                        * SinSquareHalf(other.Longitude - Longitude)
            )
        ); // earth radius 6.372,8Km x 2 = 12745.6

    static double Rad(double angle) => angle * 0.017453292519943295769236907684886127d; // = angle * Math.Pi / 180.0d

    static double SinSquareHalf(double diff) => Math.Pow(Math.Sin(Rad(diff) / 2d), 2); // = sin²(diff / 2)
}

public sealed partial record Velocity(double Value, double Direction);

public sealed partial record MassPoint(Coordinate Coordinate, Velocity Velocity);

public sealed partial record ArrivalEvent(
    int StationCode,
    int ShuttleJobId,
    string RouteName,
    TimeSpan EstimatedArrivalSpan,
    DateTime CurrentTime
)
{
    public TimeSpan TimeToWait => CurrentTime + EstimatedArrivalSpan - DateTime.Now;
}
