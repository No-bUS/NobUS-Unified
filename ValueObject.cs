namespace NobUS.DataContract.Model
{
    public sealed partial record Coordinate(double Longitude, double Latitude)
    {
        public override string ToString() => $"({Longitude}, {Latitude})";

        /// <summary>
        /// Credit: https://stackoverflow.com/a/60899418
        /// </summary>
        /// <returns>Distance in Km</returns>
        public double DistanceTo(Coordinate other)
        {
            double rad(double angle) => angle * 0.017453292519943295769236907684886127d; // = angle * Math.Pi / 180.0d
            double sinSquareHalf(double diff) => Math.Pow(Math.Sin(rad(diff) / 2d), 2); // = sin²(diff / 2)
            return 12745.6
                * Math.Asin(
                    Math.Sqrt(
                        sinSquareHalf(Latitude - other.Latitude)
                            + Math.Cos(rad(Latitude))
                                * Math.Cos(rad(other.Latitude))
                                * sinSquareHalf(other.Longitude - Longitude)
                    )
                ); // earth radius 6.372,8Km x 2 = 12745.6
        }
    }

    public sealed partial record Velocity(double Value, double Direction);

    public sealed partial record MassPoint(Coordinate Coordinate, Velocity Velocity);

    public sealed partial record ArrivalEvent(
        Station Station,
        ShuttleJob ShuttleJob,
        TimeSpan EstimatedArrivalSpan,
        DateTime CurrentTime
    );
}
