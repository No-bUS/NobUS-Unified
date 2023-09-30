using NobUS.DataContract.Model;

namespace NobUS.Frontend.MAUI.Presentation.ViewModel
{
    public partial record GroupedArrivalEventsViewModel(string RouteName, IEnumerable<ArrivalEvent> ArrivalEvents)
    {

    }
}
