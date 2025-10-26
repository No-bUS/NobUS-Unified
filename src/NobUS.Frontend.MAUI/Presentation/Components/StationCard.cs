using System;
using CommunityToolkit.Maui.Markup;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Presentation;
using NobUS.Frontend.MAUI.Service;
using NobUS.Infrastructure;
using ReactiveUI;
using static NobUS.Infrastructure.ArrivalEventListener;

namespace NobUS.Frontend.MAUI.Presentation.Components;

internal class StationCardState
{
    public bool Expanded { get; set; }
    public List<ArrivalEventGroup> ArrivalEvents { get; set; }
}

internal partial class StationCard : DisposableComponent<StationCardState>
{
    private double _distance = 1.453;
    private Station _station;

    [Inject]
    private ILocationProvider locationProvider;

    [Inject]
    private ArrivalEventListener arrivalEventListener;

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

    public StationCard Distance(double distance)
    {
        _distance = distance;
        return this;
    }

    public override VisualNode Render()
    {
        Color textColor = State.Expanded ? this.UseScheme().OnSecondary : Styler.Scheme.OnSurface;
        Color backgroundColor = State.Expanded
            ? this.UseScheme().Secondary
            : Styler.Scheme.SurfaceContainerHigh;

        string distanceLabel = _distance < 1
            ? $"{_distance * 1000:0} m"
            : $"{_distance:0.0} km";

        return new Border
        {
            new VerticalStackLayout
            {
                new Grid("auto", "*,auto")
                {
                    new HorizontalStackLayout
                    {
                        new Label(char.ConvertFromUtf32((int)MaterialIcons.Route))
                            .FontFamily("MIcon-Regular")
                            .FontSize(16)
                            .TextColor(textColor)
                            .Opacity(0.75f),
                        new Label($"{_station.Code} • {_station.Road}")
                            .FontFamily("SemiBold")
                            .FontSize(12)
                            .TextColor(textColor)
                            .Opacity(0.9f),
                    }
                        .Spacing(6)
                        .GridColumn(0),
                    new Border
                    {
                        new HorizontalStackLayout
                        {
                            new Label(char.ConvertFromUtf32((int)MaterialIcons.Place))
                                .FontFamily("MIcon-Regular")
                                .FontSize(14)
                                .TextColor(textColor)
                                .Opacity(0.85f),
                            new Label(distanceLabel)
                                .FontFamily("SemiBold")
                                .FontSize(12)
                                .TextColor(textColor),
                        }
                            .Spacing(4)
                            .Padding(0, 0, 2, 0),
                    }
                        .Padding(10, 4)
                        .BackgroundColor(textColor.WithAlpha(0.12f))
                        .StrokeShape(new RoundRectangle().CornerRadius(12))
                        .StrokeThickness(0)
                        .Stroke(Colors.Transparent)
                        .GridColumn(1)
                        .HorizontalOptions(LayoutOptions.End),
                }
                    .ColumnSpacing(12)
                    .OnTapped(Load),
                new Label(_station.Name)
                    .FontFamily("ExtraBold")
                    .FontSize(18)
                    .TextColor(textColor)
                    .LineBreakMode(LineBreakMode.TailTruncation)
                    .Margin(0, 8, 0, 0)
                    .OnTapped(Load),
                !State.Expanded
                    ? null
                    : new Border
                    {
                        new VerticalStackLayout { State.ArrivalEvents.Select(RenderGroup) }
                            .Spacing(10)
                            .Margin(4, 8)
                            .HFill(),
                    }
                        .BackgroundColor(this.UseScheme().SecondaryContainer)
                        .StrokeShape(new RoundRectangle().CornerRadius(18))
                        .StrokeThickness(0)
                        .Stroke(Colors.Transparent)
                        .Padding(12, 10),
            }
                .Spacing(6)
                .BackgroundColor(backgroundColor)
                .Padding(18, 16),
        }
            .ToCard(28)
            .Margin(4, 8)
            .Shadow(new Shadow
            {
                Brush = new SolidColorBrush(Color.FromArgb("#12000000")),
                Radius = 14,
                Opacity = 0.25f,
                Offset = new Point(0, 4),
            });
    }

    private static VisualNode RenderArrivalEvents(ArrivalEvent ae)
    {
        var tier = ae.TimeToWait switch
        {
            var t when t < TimeSpan.FromMinutes(5) => ETATiers.f0t5,
            var t when t < TimeSpan.FromMinutes(30) => ETATiers.f5t30,
            var t when t < TimeSpan.FromMinutes(60) => ETATiers.f30t60,
            _ => ETATiers.f60,
        };

        return new Label()
            .Text($"{ae.TimeToWait.TotalMinutes:0}m{(ae.TimeToWait.Seconds > 30 ? "+" : "")}")
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
        new VerticalStackLayout
        {
            new Border
            {
                new Label(grouping.RouteName)
                    .SemiBold()
                    .TextColor(Styler.Scheme.OnSecondary)
                    .FontSize(13)
                    .Margin(6, 2),
            }
                .Background(Styler.Scheme.SecondaryContainer)
                .StrokeShape(new RoundRectangle().CornerRadius(12))
                .StrokeThickness(0)
                .Stroke(Colors.Transparent),
            new CollectionView()
                .ItemsSource(grouping.Events, RenderArrivalEvents)
                .ItemSizingStrategy(ItemSizingStrategy.MeasureAllItems)
                .SelectionMode(SelectionMode.None)
                .VerticalScrollBarVisibility(ScrollBarVisibility.Never)
                .ItemsLayout(new HorizontalLinearItemsLayout().ItemSpacing(6))
                .Margin(0, 6, 0, 0),
        }
            .Spacing(6);

    private void Load()
    {
        if (State.Expanded)
        {
            SetState(s =>
            {
                s.Expanded = false;
                s.ArrivalEvents = null;
            });
            Dispose();
        }
        else
        {
            SetState(s =>
            {
                s.Expanded = true;
                s.ArrivalEvents = arrivalEventListener.GetArrivalEventGroups(_station, this);
            });
        }
    }

    protected override void OnMounted()
    {
        locationProvider
            .WhenAnyValue(x => x.Location)
            .WhereNotNull()
            .Subscribe(loc => _distance = _station.Coordinate.DistanceTo(loc))
            .Invoke(RegisterResource);
        base.OnMounted();
    }
}
