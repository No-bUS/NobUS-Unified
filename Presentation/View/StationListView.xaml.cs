using CommonServiceLocator;
using NobUS.Frontend.MAUI.Presentation.ViewModel;

namespace NobUS.Frontend.MAUI.Presentation.View
{
    public partial class StationListView : ContentView
    {
        public StationListViewModel ViewModel = ServiceLocator.Current.GetInstance<StationListViewModel>();

        public StationListView()
        {
            InitializeComponent();
            ViewModel.GetAll().ContinueWith(
                async r => ListView.ItemsSource = await r,
                TaskScheduler.FromCurrentSynchronizationContext()
            );
        }
    }
}