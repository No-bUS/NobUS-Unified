using NobUS.Frontend.MAUI.Presentation.ViewModel;

namespace NobUS.Frontend.MAUI.Presentation.View
{
    public partial class StationView : ContentView
    {
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(nameof(ViewModel),
            typeof(StationViewModel), typeof(StationView), null, BindingMode.OneTime);

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
            EtaListView.ItemsSource = await ViewModel.ArrivalEvents;
        }
    }
}