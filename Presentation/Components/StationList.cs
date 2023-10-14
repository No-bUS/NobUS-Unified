using MauiReactor;
using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Service;
using static NobUS.Frontend.MAUI.Service.DefinitionLoader;
using ActivityIndicator = MauiReactor.ActivityIndicator;
using Grid = Microsoft.Maui.Controls.Grid;
using ListView = MauiReactor.ListView;
using SwipeView = MauiReactor.SwipeView;
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

        private static SwipeItems SwipeItems =>
            new SwipeItems(
                new Microsoft.Maui.Controls.ISwipeItem[]
                {
#if WINDOWS
                    new SwipeItem { Text = "Pin", BackgroundColor = Styler.Scheme.Surface, }
#else
                    new SwipeItemView
                    {
                        Content = new Grid
                        {
                            new Microsoft.Maui.Controls.Label
                            {
                                Text = "Pin",
                                FontFamily = "SemiBold",
                                FontSize = Styles.Sizes.Medium
                            }
                        },
                    }
#endif
                }
            )
            {
                Mode = SwipeMode.Execute
            };

        public override VisualNode Render() =>
            new ListView()
                .ItemsSource(
                    State.HasLocated
                        ? _stations.OrderBy(s => s.Coordinate.DistanceTo(State.Location)).ToList()
                        : _stations,
                    s =>
                        new ViewCell()
                        {
                            new StationCard()
                                .Station(s)
                                .Distance(
                                    State.HasLocated
                                        ? s.Coordinate.DistanceTo(State.Location)
                                        : 1.453
                                )
                        }
                )
                .SelectionMode(ListViewSelectionMode.None)
                .VerticalScrollBarVisibility(ScrollBarVisibility.Never)
                .SeparatorVisibility(SeparatorVisibility.None)
                .HasUnevenRows(true)
                .BackgroundColor(Styler.Scheme.Surface);

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
