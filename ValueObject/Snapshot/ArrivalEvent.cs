using NobUS.DataContract.Entity;

namespace NobUS.DataContract.ValueObject.Snapshot
{
  public sealed partial record ArrivalEvent(Station Station, ShuttleJob ShuttleJob, TimeSpan EstimatedArrivalSpan, DateTime CurrentTime);
}
