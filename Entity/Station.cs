using NobUS.DataContract.ValueObject;

namespace NobUS.DataContract.Entity
{
  public sealed partial record Station(int Code, string Name, string Caption, Coordinate Coordinate);
}