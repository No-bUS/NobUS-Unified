using NobUS.Frontend.MAUI.Presentation.ViewModel;

namespace NobUS.Frontend.MAUI.Presentation.View
{
    public partial class StationView : ContentView
    {
        public static readonly BindableProperty ViewModelProperty
            = BindableProperty.Create(nameof(ViewModel), typeof(StationViewModel), typeof(StationView), null);

        public StationViewModel ViewModel
        {
            get => (StationViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public StationView()
        {
            InitializeComponent();
            InfoCard.BindingContext = this;
        }
    }
}