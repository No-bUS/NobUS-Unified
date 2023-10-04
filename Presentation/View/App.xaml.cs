using Autofac;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using NobUS.DataContract.Model;
using NobUS.DataContract.Reader.OfficialAPI;
using NobUS.Frontend.MAUI.Presentation.ViewModel;
using NobUS.Frontend.MAUI.Repository;
using NobUS.Frontend.MAUI.Service;

namespace NobUS.Frontend.MAUI.Presentation.View
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            var autofacContainerBuilder = new ContainerBuilder();
            autofacContainerBuilder.RegisterType<CongestedClient>().As<IClient>().SingleInstance();
            autofacContainerBuilder
                .RegisterType<LocationProvider>()
                .As<ILocationProvider>()
                .SingleInstance();
            autofacContainerBuilder
                .RegisterType<StaticRepository<Station>>()
                .As<IRepository<Station>>()
                .SingleInstance();
            autofacContainerBuilder
                .RegisterType<StaticRepository<Route>>()
                .As<IRepository<Route>>()
                .SingleInstance();
            autofacContainerBuilder
                .RegisterType<StationListViewModel>()
                .As<StationListViewModel>()
                .SingleInstance();
            autofacContainerBuilder
                .RegisterType<RouteListViewModel>()
                .As<RouteListViewModel>()
                .SingleInstance();

            var container = autofacContainerBuilder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));
        }
    }
}
