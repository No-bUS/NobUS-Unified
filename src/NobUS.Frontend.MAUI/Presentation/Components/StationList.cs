using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Markup;
using DynamicData;
using Microsoft.Maui.Dispatching;
using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Service;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using static NobUS.Infrastructure.DefinitionLoader;

namespace NobUS.Frontend.MAUI.Presentation.Components
{
    internal class StationList : Component
    {
        private ObservableCollection<Station> _stations = GetAllStations.ToObservableCollection();
        private readonly ILocationProvider _locationProvider =
            CommonServiceLocator.ServiceLocator.Current.GetInstance<ILocationProvider>();
        private readonly IDispatcher dispatcher = Dispatcher.GetForCurrentThread();
        private IDisposable _locationSubscription;

        private readonly List<WeakReference<StationCard>> cardRefs = new();
        private WeakReference<ListView>? listViewRef;

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
                    s =>
                        new ViewCell()
                        {
                            new StationCard().Station(s).Invoke(c => cardRefs.Add(new(c)))
                        }
                )
                .SelectionMode(ListViewSelectionMode.None)
                .VerticalScrollBarVisibility(ScrollBarVisibility.Never)
                .SeparatorVisibility(SeparatorVisibility.None)
                .HasUnevenRows(true)
                .BackgroundColor(Styler.Scheme.Surface)
                .Invoke(lv => listViewRef = new(lv));

        protected override void OnMounted()
        {
            _locationSubscription = _locationProvider
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
                });
            base.OnMounted();
        }

        protected override void OnWillUnmount()
        {
            Task.Run(() =>
            {
                foreach (var card in cardRefs)
                {
                    if (card.TryGetTarget(out var c))
                    {
                        c.Dispose();
                    }
                }

                if (listViewRef != null && listViewRef.TryGetTarget(out var l))
                {
                    l.ItemsSource(Array.Empty<int>());
                }
                _locationSubscription.Dispose();
            });
            base.OnWillUnmount();
        }
    }
}
