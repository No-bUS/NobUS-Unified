using NobUS.DataContract.Model.ValueObject;

namespace NobUS.DataContract.Model.Entity
{
  public sealed partial record Vehicle(MassPoint? MassPoint, string Plate);
}