using System.Collections.Immutable;
using CommonServiceLocator;
using NobUS.DataContract.Model;
using NobUS.DataContract.Reader.OfficialAPI;
using NobUS.Frontend.MAUI.Service;

namespace NobUS.Frontend.MAUI.Presentation.ViewModel
{
    public record StationViewModel(Station Station)
    {
        public readonly Task<double> DistanceTask = ServiceLocator.Current
            .GetInstance<ILocationProvider>()
            .GetLocationAsync()
            .ContinueWith(t => t.Result.DistanceTo(Station.Coordinate));

        public List<Route> Routes { get; } = new();

        public double Distance { get; set; }

        public Task<IImmutableList<ArrivalEvent>> ArrivalEvents =>
            ServiceLocator.Current.GetInstance<IClient>().GetArrivalEventsAsync(Station);
    }
}
