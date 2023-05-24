using NobUS.DataContract.Model.Entity;

namespace NobUS.DataContract.Model.ValueObject.Snapshot
{
  public sealed partial record ArrivalEvent(Station Station, ShuttleJob ShuttleJob, TimeSpan EstimatedArrivalSpan, DateTime CurrentTime);
}
