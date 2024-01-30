using System.Collections.ObjectModel;
using System.Reactive.Linq;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Markup;
using DynamicData;
using Microsoft.Maui.Dispatching;
using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Service;
using ReactiveUI;
using static NobUS.Infrastructure.DefinitionLoader;

namespace NobUS.Frontend.MAUI.Presentation.Components
{
    internal class StationList : DisposableComponent
    {
        private ObservableCollection<Station> _stations = GetAllStations.ToObservableCollection();
        private readonly ILocationProvider _locationProvider =
            CommonServiceLocator.ServiceLocator.Current.GetInstance<ILocationProvider>();
        private readonly IDispatcher dispatcher = Dispatcher.GetForCurrentThread();

        public StationList Stations(IList<Station> stations)
        {
            _stations = stations.ToObservableCollection();
            return this;
        }

        private static SwipeItems SwipeItems =>
            new(
                new Microsoft.Maui.Controls.ISwipeItem[]
                {
#if WINDOWS
                    new SwipeItem { Text = "Pin", BackgroundColor = Styler.Scheme.Surface, },
#else
                    new SwipeItemView
                    {
                        Content = new Microsoft.Maui.Controls.Grid
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
                    _stations,
                    s => new ViewCell() { new StationCard().Station(s).Invoke(RegisterResource) }
                )
                .SelectionMode(ListViewSelectionMode.None)
                .VerticalScrollBarVisibility(ScrollBarVisibility.Never)
                .SeparatorVisibility(SeparatorVisibility.None)
                .HasUnevenRows(true)
                .BackgroundColor(Styler.Scheme.Surface);

        protected override void OnMounted()
        {
            _locationProvider
                .WhenAnyValue(x => x.Location)
                .WhereNotNull()
                .Subscribe(loc =>
                {
                    dispatcher.Dispatch(() =>
                    {
                        var sorted = _stations.OrderBy(s => s.Coordinate.DistanceTo(loc)).ToList();
                        _stations.Clear();
                        _stations.AddRange(sorted);
                    });
                })
                .Invoke(RegisterResource);
            base.OnMounted();
        }
    }
}
