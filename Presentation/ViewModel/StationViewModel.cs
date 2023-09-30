using NobUS.DataContract.Model;
using NobUS.DataContract.Reader.OfficialAPI;
using NobUS.Frontend.MAUI.Façade;
using NobUS.Frontend.MAUI.Service;
using System.Collections.Immutable;

namespace NobUS.Frontend.MAUI.Presentation.ViewModel
{
    public partial record StationViewModel(Station Station)
    {
        public List<Route> Routes { get; } = new List<Route>();
        public Task<IImmutableList<ArrivalEvent>> ArrivalEvents { get; } = CommonServiceLocator.ServiceLocator.Current.GetInstance<IClient>().GetAsync<ArrivalEvent, Station>(Station);

        public readonly Task<double> DistanceTask = CommonServiceLocator.ServiceLocator.Current.GetInstance<ILocationProvider>().GetLocationAsync().ContinueWith(t => t.Result.DistanceTo(Station.Coordinate));
    }
}
