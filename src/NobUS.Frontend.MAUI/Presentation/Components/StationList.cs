using System.Collections.ObjectModel;
using System.Reactive.Linq;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Markup;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Service;
using ReactiveUI;
using static NobUS.Infrastructure.DefinitionLoader;

namespace NobUS.Frontend.MAUI.Presentation.Components;

internal partial class StationList : DisposableComponent
{
    private ObservableCollection<Station> _stations = [.. GetAllStations];
    private readonly IDispatcher dispatcher = Dispatcher.GetForCurrentThread()!;

    [Inject]
    private ILocationProvider locationProvider;

    public StationList Stations(IList<Station> stations)
    {
        _stations = [.. stations];
        return this;
    }

    private static SwipeItems SwipeItems =>
        new(
            new Microsoft.Maui.Controls.ISwipeItem[]
            {
#if WINDOWS
                new SwipeItem { Text = "Pin", BackgroundColor = Styler.Scheme.Surface },
#else
                new SwipeItemView
                {
                    Content = new Microsoft.Maui.Controls.Grid
                    {
                        new Microsoft.Maui.Controls.Label
                        {
                            Text = "Pin",
                            FontFamily = "SemiBold",
                            FontSize = Styles.Sizes.Medium,
                        },
                    },
                }
#endif
            }
        )
        {
            Mode = SwipeMode.Execute,
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
            .BackgroundColor(Colors.Transparent)
            .Margin(24, 0, 24, 120)
            .RowHeight(-1)
            .Header(
                new Grid
                {
                    new Label("Nearby bus stops")
                        .FontFamily("ExtraBold")
                        .FontSize(22)
                        .TextColor(Styler.Scheme.OnSurface)
                        .Margin(0, 24, 0, 12),
                }
            );

    protected override void OnMounted()
    {
        locationProvider
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
