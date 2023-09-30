using NobUS.Frontend.MAUI.Presentation.ViewModel;

namespace NobUS.Frontend.MAUI.Presentation.View
{
    public partial class StationView : ContentView
    {
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(
            nameof(ViewModel),
            typeof(StationViewModel),
            typeof(StationView),
            null,
            BindingMode.OneTime
        );

        public StationView()
        {
            InitializeComponent();
            InfoCard.BindingContext = this;
        }

        public StationViewModel ViewModel
        {
            get => (StationViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private async void ShowArrivalEventList(object sender, EventArgs e)
        {
            var groupedArrivalEventsViewModels = (await ViewModel.ArrivalEvents)
                .OrderBy(ae => ae.ShuttleJob.Route.Name)
                .GroupBy(ae => ae.ShuttleJob.Route.Name)
                .Select(g => new GroupedArrivalEventsViewModel(g.Key, g))
                .ToList();

            EtaListView.IsVisible = EtaListViewExpander.IsExpanded;
            EtaListView.ItemsSource = groupedArrivalEventsViewModels;
            EtaListView.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
        }
    }
}
