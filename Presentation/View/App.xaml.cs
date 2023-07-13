﻿using Autofac;
using Autofac.Extras.CommonServiceLocator;
using NobUS.DataContract.Model.Entity;
using NobUS.DataContract.Reader.OfficialAPI;
using NobUS.Frontend.MAUI.Presentation.ViewModel;
using NobUS.Frontend.MAUI.Repository;

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
            autofacContainerBuilder.RegisterType<StaticRepository<Station>>().As<IRepository<Station>>().SingleInstance();
            autofacContainerBuilder.RegisterType<StationListViewModel>().As<StationListViewModel>().SingleInstance();

            var container = autofacContainerBuilder.Build();
            CommonServiceLocator.ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(container));
        }
    }
}
