using System;
using System.Collections.Generic;
using CommunityToolkit.Maui.Markup;
using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Service;
using NobUS.Infrastructure;
using ReactiveUI;
using static NobUS.Infrastructure.ArrivalEventListener;

namespace NobUS.Frontend.MAUI.Presentation.Components;

internal class StationCardState
{
    public bool Expanded { get; set; }
    public List<ArrivalEventGroup>? ArrivalEvents { get; set; }
}

internal partial class StationCard : DisposableComponent<StationCardState>
{
    private double? _distance;
    private Station? _station;

    [Inject]
    private ILocationProvider locationProvider = null!;

    [Inject]
    private ArrivalEventListener arrivalEventListener = null!;

    [Inject]
    private StationViewStateStore stationViewStateStore = null!;

    private enum ETATiers
    {
        f0t5,
        f5t30,
        f30t60,
        f60,
    }

    public StationCard Station(Station station)
    {
        _station = station;
        return this;
    }

    public StationCard Distance(double? distance)
    {
        _distance = distance;
        return this;
    }

    public override VisualNode Render()
    {
        var station = RequireStation();
        var textColor = State.Expanded ? this.UseScheme().OnSecondary : Styler.Scheme.OnSurface;
        var backgroundColor = State.Expanded
            ? this.UseScheme().Secondary
            : Styler.Scheme.SurfaceContainer;
        var distance = _distance.HasValue ? $"{_distance.Value * 1000:F0}m" : "—";

        return new Border
        {
            new VerticalStackLayout
            {
                new Grid("auto", "*,auto")
                {
                    new Label($"{station.Code} | {station.Road}")
                        .GridColumn(0)
                        .Regular()
                        .Small()
                        .HorizontalOptions(LayoutOptions.Start)
                        .TextColor(textColor),
                    new Label(distance)
                        .Regular()
                        .Small()
                        .GridColumn(1)
                        .HorizontalOptions(LayoutOptions.End)
                        .TextColor(textColor),
                },
                new Label(station.Name)
                    .Medium()
                    .ExtraBold()
                    .OnTapped(ToggleExpansion)
                    .TextColor(textColor),
                !State.Expanded
                    ? null
                    : new Border
                    {
                        new VerticalStackLayout
                        {
                            (State.ArrivalEvents ?? new List<ArrivalEventGroup>()).Select(
                                RenderGroup
                            ),
                        }
                            .HFill()
                            .VFill()
                            .Margin(5),
                    }
                        .BackgroundColor(this.UseScheme().SecondaryContainer)
                        .ToCard(20),
            }
                .BackgroundColor(backgroundColor)
                .Padding(5),
        }
            .ToCard(20)
            .Margin(1, 5);
    }

    private static VisualNode RenderArrivalEvents(ArrivalEvent arrivalEvent)
    {
        var tier = arrivalEvent.TimeToWait switch
        {
            var time when time < TimeSpan.FromMinutes(5) => ETATiers.f0t5,
            var time when time < TimeSpan.FromMinutes(30) => ETATiers.f5t30,
            var time when time < TimeSpan.FromMinutes(60) => ETATiers.f30t60,
            _ => ETATiers.f60,
        };

        return new Label()
            .Text(
                $"{arrivalEvent.TimeToWait.TotalMinutes:0}m{(arrivalEvent.TimeToWait.Seconds > 30 ? "+" : "")}"
            )
            .TextDecorations(
                tier switch
                {
                    ETATiers.f0t5 => TextDecorations.Underline,
                    ETATiers.f60 => TextDecorations.Strikethrough,
                    _ => TextDecorations.None,
                }
            )
            .FontAttributes(
                tier switch
                {
                    ETATiers.f30t60 => FontAttributes.Italic,
                    ETATiers.f60 => FontAttributes.Italic,
                    _ => FontAttributes.None,
                }
            )
            .Regular()
            .Base()
            .VStart()
            .HeightRequest(Styles.Sizes.Base * 1.5)
            .TextColor(Styler.Scheme.OnSecondaryContainer);
    }

    private static VisualNode RenderGroup(ArrivalEventGroup grouping) =>
        new HorizontalStackLayout
        {
            new Label(grouping.RouteName)
                .SemiBold()
                .Base()
                .TextColor(Styler.Scheme.OnSecondaryContainer),
            new CollectionView()
                .ItemsSource(grouping.Events, RenderArrivalEvents)
                .ItemSizingStrategy(ItemSizingStrategy.MeasureAllItems)
                .SelectionMode(SelectionMode.None)
                .VerticalScrollBarVisibility(ScrollBarVisibility.Never)
                .ItemsLayout(new HorizontalLinearItemsLayout().ItemSpacing(5)),
        }
            .Spacing(5)
            .HeightRequest(Styles.Sizes.Base * 2);

    private void ToggleExpansion()
    {
        var station = RequireStation();
        if (State.Expanded)
        {
            stationViewStateStore.SetExpanded(station.Code, false);
            arrivalEventListener.Cancel(station, this);
            SetState(state =>
            {
                state.Expanded = false;
                state.ArrivalEvents = null;
            });
        }
        else
        {
            var groups = arrivalEventListener.GetArrivalEventGroups(station, this);
            stationViewStateStore.SetExpanded(station.Code, true);
            SetState(state =>
            {
                state.Expanded = true;
                state.ArrivalEvents = groups;
            });
        }
    }

    protected override void OnMounted()
    {
        base.OnMounted();
        var station = RequireStation();
        var storedDistance = stationViewStateStore.GetDistance(station.Code);
        if (storedDistance.HasValue)
        {
            _distance = storedDistance.Value;
        }

        var locationSubscription = locationProvider
            .WhenAnyValue(provider => provider.Location)
            .WhereNotNull()
            .Subscribe(location =>
            {
                _distance = station.Coordinate.DistanceTo(location);
                Invalidate();
            });
        RegisterResource(locationSubscription);

        if (stationViewStateStore.IsExpanded(station.Code) && !State.Expanded)
        {
            ToggleExpansion();
        }
    }

    protected override void OnWillUnmount()
    {
        var station = _station;
        if (station is not null)
        {
            arrivalEventListener.Cancel(station, this);
        }
        base.OnWillUnmount();
    }

    private Station RequireStation() =>
        _station ?? throw new InvalidOperationException("Station must be set before rendering.");
}
