using NobUS.Frontend.MAUI.Presentation.ViewModel;

namespace NobUS.Frontend.MAUI.Presentation.View
{
    public partial class RouteView : ContentView
    {
        public RouteView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(
            nameof(ViewModel),
            typeof(RouteViewModel),
            typeof(RouteView),
            null,
            BindingMode.OneTime
        );

        public RouteViewModel ViewModel
        {
            get => (RouteViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        // on loaded, call view model's get origin, and set to label's text
        public async void OnLoaded(object sender, EventArgs e)
        {
            RouteNameLabel.Text = $"{ViewModel.Route.Operator} | {await ViewModel.OriginName}";
        }
    }
}
