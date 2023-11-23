using CommonServiceLocator;
using CommunityToolkit.Maui.Markup;
using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Service;
using NobUS.Infrastructure;
using ReactiveUI;
using static NobUS.Infrastructure.ArrivalEventListener;

namespace NobUS.Frontend.MAUI.Presentation.Components
{
    internal class StationCardState
    {
        public bool Expanded { get; set; }
        public List<ArrivalEventGroup> ArrivalEvents { get; set; }
    }

    internal class StationCard : DisposableComponent<StationCardState>
    {
        private double _distance = 1.453;
        private Station _station;
        private readonly ArrivalEventListener arrivalEventListener =
            ServiceLocator.Current.GetInstance<ArrivalEventListener>();
        private readonly ILocationProvider locationProvider =
            ServiceLocator.Current.GetInstance<ILocationProvider>();

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
            Color textColor = State.Expanded
                ? this.UseScheme().OnSecondary
                : Styler.Scheme.OnSurface;
            Color backgroundColor = State.Expanded
                ? this.UseScheme().Secondary
                : Styler.Scheme.SurfaceContainer;
            return new Border
            {
                new VerticalStackLayout
                {
                    new Grid("auto", "*,auto")
                    {
                        new Label($"{_station.Code} | {_station.Road}")
                            .GridColumn(0)
                            .Regular()
                            .Small()
                            .HorizontalOptions(LayoutOptions.Start)
                            .TextColor(textColor),
                        new Label($"{_distance * 1000:F2}m")
                            .Regular()
                            .Small()
                            .GridColumn(1)
                            .HorizontalOptions(LayoutOptions.End)
                            .TextColor(textColor),
                    },
                    new Label(_station.Name)
                        .Medium()
                        .ExtraBold()
                        .OnTapped(Load)
                        .TextColor(textColor),
                    !State.Expanded
                        ? null
                        : new Border
                        {
                            new VerticalStackLayout { State.ArrivalEvents.Select(RenderGroup) }
                                .HFill()
                                .VFill()
                                .Margin(5)
                        }
                            .BackgroundColor(this.UseScheme().SecondaryContainer)
                            .ToCard(20),
                }
                    .BackgroundColor(backgroundColor)
                    .Padding(5)
            }
                .ToCard(20)
                .Margin(1, 5);
        }

        private static VisualNode RenderArrivalEvents(ArrivalEvent ae)
        {
            var tier = ae.TimeToWait switch
            {
                var t when t < TimeSpan.FromMinutes(5) => ETATiers.f0t5,
                var t when t < TimeSpan.FromMinutes(30) => ETATiers.f5t30,
                var t when t < TimeSpan.FromMinutes(60) => ETATiers.f30t60,
                _ => ETATiers.f60
            };

            return new Label()
                .Text($"{ae.TimeToWait.TotalMinutes:0}m{(ae.TimeToWait.Seconds > 30 ? "+" : "")}")
                .TextDecorations(
                    tier switch
                    {
                        ETATiers.f0t5 => TextDecorations.Underline,
                        ETATiers.f60 => TextDecorations.Strikethrough,
                        _ => TextDecorations.None
                    }
                )
                .FontAttributes(
                    tier switch
                    {
                        ETATiers.f30t60 => FontAttributes.Italic,
                        ETATiers.f60 => FontAttributes.Italic,
                        _ => FontAttributes.None
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
                    .ItemsLayout(new HorizontalLinearItemsLayout().ItemSpacing(5))
            }
                .Spacing(5)
                .HeightRequest(Styles.Sizes.Base * 2);

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
}
