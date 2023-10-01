using CommonServiceLocator;
using NobUS.Frontend.MAUI.Presentation.ViewModel;
using NobUS.Frontend.MAUI.Service;

namespace NobUS.Frontend.MAUI.Presentation.View
{
    public partial class StationListView : ContentView
    {
        private readonly StationListViewModel _viewModel =
            ServiceLocator.Current.GetInstance<StationListViewModel>();

        private readonly ILocationProvider _locationProvider =
            ServiceLocator.Current.GetInstance<ILocationProvider>();

        public StationListView()
        {
            InitializeComponent();
            ListView.BindingContext = this;
            _viewModel
                .GetAll()
                .ContinueWith(
                    async r => ListView.ItemsSource = await r,
                    TaskScheduler.FromCurrentSynchronizationContext()
                );

            _locationProvider
                .GetLocationAsync()
                .ContinueWith(
                    async locationTask =>
                    {
                        var location = await locationTask;
                        var viewModels = await _viewModel.GetAll();
                        foreach (var viewModel in viewModels)
                            viewModel.Distance =
                                viewModel.Station.Coordinate.DistanceTo(location) * 1000;
                        viewModels.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                        ListView.ItemsSource = viewModels;
                    },
                    TaskScheduler.FromCurrentSynchronizationContext()
                );
        }
    }
}
