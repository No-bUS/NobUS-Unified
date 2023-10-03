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
            Loaded += LoadArrivalEvents;
            EtaListViewExpander.ExpandedChanged += LoadArrivalEvents;
        }

        public StationViewModel ViewModel
        {
            get => (StationViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private async void LoadArrivalEvents(object sender, EventArgs e) =>
            EtaListView.ItemsSource = (await ViewModel.ArrivalEvents)
                .GroupBy(ae => ae.RouteName)
                .Select(g => new GroupedArrivalEventsViewModel(g.Key, g.ToList()));
    }
}
