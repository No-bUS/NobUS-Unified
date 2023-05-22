using NobUS.DataContract.Attribute;

namespace NobUS.DataContract.Entity
{
  public sealed record Station(string Name, string Caption, Coordinate Coordinate);
}