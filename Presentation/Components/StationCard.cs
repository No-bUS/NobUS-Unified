using CommonServiceLocator;
using CommunityToolkit.Maui.Markup;
using MauiReactor;
using NobUS.DataContract.Model;
using NobUS.DataContract.Reader.OfficialAPI;
using NobUS.Frontend.MAUI.Service;
using Border = MauiReactor.Border;
using CollectionView = MauiReactor.CollectionView;
using Grid = MauiReactor.Grid;
using Label = MauiReactor.Label;
using VerticalStackLayout = MauiReactor.VerticalStackLayout;

namespace NobUS.Frontend.MAUI.Presentation.Components
{
    internal class StationCardState
    {
        public IList<IGrouping<string, ArrivalEvent>> ArrivalEvents { get; set; }
        public bool Expanded { get; set; }
    }

    internal class StationCard : Component<StationCardState>
    {
        private double _distance;
        private Station _station;
        private Task _refreshTask;
        private CancellationTokenSource _tokenSource = new();

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
                        .OnTapped(async () => await LoadAsync())
                        .TextColor(textColor),
                    !State.Expanded
                        ? null
                        : new Border
                        {
                            new CollectionView()
                                .ItemSizingStrategy(ItemSizingStrategy.MeasureFirstItem)
                                .SelectionMode(SelectionMode.None)
                                .VerticalScrollBarVisibility(ScrollBarVisibility.Never)
                                .ItemsSource(State.ArrivalEvents, RenderGroup)
                                .BackgroundColor(this.UseScheme().SecondaryContainer)
                        }.ToCard(20),
                }
                    .BackgroundColor(backgroundColor)
                    .Padding(5)
            }
                .ToCard(20)
                .Margin(1, 5);
        }

        private static VisualNode RenderArrivalEvents(ArrivalEvent ae)
        {
            var timeToWait = ae.CurrentTime + ae.EstimatedArrivalSpan - DateTime.Now;
            var tier = timeToWait switch
            {
                var t when t < TimeSpan.FromMinutes(5) => ETATiers.f0t5,
                var t when t < TimeSpan.FromMinutes(30) => ETATiers.f5t30,
                var t when t < TimeSpan.FromMinutes(60) => ETATiers.f30t60,
                _ => ETATiers.f60
            };

            return new Label()
                .Text($"{timeToWait.TotalMinutes:0}m{(timeToWait.Seconds > 30 ? "+" : "")}")
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
                .TextColor(Styler.Scheme.OnSecondaryContainer);
        }

        private static VisualNode RenderGroup(IGrouping<string, ArrivalEvent> grouping) =>
            new Grid("auto", "auto,*")
            {
                new Label(grouping.Key)
                    .GridColumn(0)
                    .SemiBold()
                    .Base()
                    .TextColor(Styler.Scheme.OnSecondaryContainer),
                new CollectionView()
                    .GridColumn(1)
                    .ItemsSource(grouping, RenderArrivalEvents)
                    .ItemSizingStrategy(ItemSizingStrategy.MeasureAllItems)
                    .SelectionMode(SelectionMode.None)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Never)
                    .ItemsLayout(new HorizontalLinearItemsLayout().ItemSpacing(5))
            }.Padding(10);

        private async Task<IList<IGrouping<string, ArrivalEvent>>> FetchArrivalEventsAsync(
            Station station
        ) =>
            (await ServiceLocator.Current.GetInstance<IClient>().GetArrivalEventsAsync(station))
                .OrderBy(ae => ae.RouteName)
                .GroupBy(ae => ae.RouteName)
                .ToList();

        private async Task LoadAsync()
        {
            if (State.Expanded)
            {
                _tokenSource.Cancel();
                SetState(s => s.Expanded = false);
                return;
            }

            var ae = await FetchArrivalEventsAsync(_station);

            if (ae.Count > 0)
                SetState(s =>
                {
                    s.Expanded = true;
                    s.ArrivalEvents = ae;
                });

            // Start the background refresh task
            _tokenSource = new CancellationTokenSource();
            _refreshTask = RefreshDataInBackground(_tokenSource.Token);
        }

        private async Task RefreshDataInBackground(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(10 * 1000, token);
                var aes = await FetchArrivalEventsAsync(_station);
                SetState(s => s.ArrivalEvents = aes);
            }
        }

        protected override void OnWillUnmount()
        {
            _tokenSource.Cancel();
            base.OnWillUnmount();
        }
    }
}
