using NobUS.DataContract.Model.ValueObject;

namespace NobUS.DataContract.Model.Entity
{
  public sealed partial record Station(int Code, string Name, string Caption, Coordinate Coordinate);
}