using NobUS.DataContract.Model.Entity;
using NobUS.DataContract.Model.ValueObject.Snapshot;

namespace NobUS.Frontend.MAUI.Presentation.ViewModel
{
    public partial record StationViewModel(Station Station)
    {
        public List<Route> Routes { get; } = new List<Route>();
        public List<ArrivalEvent> ArrivalEvents { get; } = new List<ArrivalEvent>();
    }
}
