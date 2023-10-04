using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonServiceLocator;
using NobUS.Frontend.MAUI.Presentation.ViewModel;

namespace NobUS.Frontend.MAUI.Presentation.View
{
    public partial class RouteListView : ContentView
    {
        private readonly RouteListViewModel _viewModel =
            ServiceLocator.Current.GetInstance<RouteListViewModel>();

        public RouteListView()
        {
            InitializeComponent();
            ListView.BindingContext = this;

            _viewModel
                .GetAll()
                .ContinueWith(
                    r => ListView.ItemsSource = r.Result,
                    TaskScheduler.FromCurrentSynchronizationContext()
                );
        }
    }
}
