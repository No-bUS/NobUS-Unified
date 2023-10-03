using NobUS.DataContract.Model;

namespace NobUS.Frontend.MAUI.Presentation.ViewModel
{
    public record GroupedArrivalEventsViewModel(string RouteName, IList<ArrivalEvent> ArrivalEvents)
    {
        public List<ArrivalEvent> FirstArrivalEvents => ArrivalEvents.Take(4).ToList();
        public List<ArrivalEvent> LastArrivalEvents => ArrivalEvents.Skip(4).ToList();
    }
}
