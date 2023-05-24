using NobUS.DataContract.ValueObject;

namespace NobUS.DataContract.Entity
{
  public sealed partial record Vehicle(MassPoint? MassPoint, string Plate);
}