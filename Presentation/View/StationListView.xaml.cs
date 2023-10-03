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
                    r =>
                    {
                        var viewModels = r.Result;
                        ListView.ItemsSource = viewModels;
                        return viewModels;
                    },
                    TaskScheduler.FromCurrentSynchronizationContext()
                )
                .ContinueWith(
                    async t =>
                    {
                        var location = await _locationProvider.GetLocationAsync();
                        var viewModels = t.Result;
                        foreach (var viewModel in viewModels)
                            viewModel.Distance =
                                viewModel.Station.Coordinate.DistanceTo(location) * 1000;
                        ListView.ItemsSource = viewModels.OrderBy(vm => vm.Distance);
                    },
                    TaskScheduler.FromCurrentSynchronizationContext()
                );
        }
    }
}
