using MauiReactor;
using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Service;
using static NobUS.Frontend.MAUI.Service.DefinitionLoader;
using ActivityIndicator = MauiReactor.ActivityIndicator;
using ListView = MauiReactor.ListView;
using ViewCell = MauiReactor.ViewCell;

namespace NobUS.Frontend.MAUI.Presentation.Components
{
    internal class State
    {
        public bool HasLocated { get; set; }
        public Coordinate Location { get; set; }
    }

    internal class StationList : Component<State>
    {
        private IList<Station> _stations = GetAllStations.ToList();

        public StationList Stations(IList<Station> stations)
        {
            _stations = stations;
            return this;
        }

        public override VisualNode Render() =>
            State.HasLocated
                ? new ListView()
                    .ItemsSource(
                        _stations.OrderBy(s => s.Coordinate.DistanceTo(State.Location)),
                        s =>
                            new ViewCell()
                            {
                                new StationCard()
                                    .Station(s)
                                    .Distance(s.Coordinate.DistanceTo(State.Location))
                            }
                    )
                    .SelectionMode(ListViewSelectionMode.None)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Never)
                    .SeparatorVisibility(SeparatorVisibility.None)
                    .HasUnevenRows(true)
                    .BackgroundColor(Styler.Scheme.Surface)
                : new ActivityIndicator().IsRunning(true);

        protected override async void OnMounted()
        {
            var loc = await CommonServiceLocator.ServiceLocator.Current
                .GetInstance<ILocationProvider>()
                .GetLocationAsync();
            SetState(s =>
            {
                s.HasLocated = true;
                s.Location = loc;
            });
            base.OnMounted();
        }
    }
}
