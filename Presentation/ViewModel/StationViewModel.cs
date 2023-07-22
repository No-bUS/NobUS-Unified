using System.Collections.Immutable;
using NobUS.DataContract.Model.Entity;
using NobUS.DataContract.Model.ValueObject.Snapshot;
using NobUS.Frontend.MAUI.Façade;

namespace NobUS.Frontend.MAUI.Presentation.ViewModel
{
    public partial record StationViewModel(Station Station)
    {
        public List<Route> Routes { get; } = new List<Route>();
        public Task<IImmutableList<ArrivalEvent>> ArrivalEvents { get; } = CommonServiceLocator.ServiceLocator.Current.GetInstance<IFaçade>().GetAsync<ArrivalEvent, Station>(Station);
    }
}
