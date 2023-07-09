namespace NobUS.DataContract.Model.ValueObject
{
    public sealed partial record Coordinate(double Longitude, double Latitude)
    {
        public override string ToString() => $"({Longitude}, {Latitude})";
    }
}
