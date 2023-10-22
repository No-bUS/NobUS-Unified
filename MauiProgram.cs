using Autofac;
using Autofac.Extras.CommonServiceLocator;
using CommonServiceLocator;
using CommunityToolkit.Maui;
using MaterialColorUtilities.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using NobUS.DataContract.Reader.OfficialAPI;
using NobUS.Frontend.MAUI.Presentation;
using NobUS.Frontend.MAUI.Service;
using NobUS.Infrastructure;

namespace NobUS.Frontend.MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var list = new[] { "ExtraBold", "Regular", "SemiBold", "Bold" }
                .SelectMany(w => new[] { "" }, (w, s) => $"{w}{s}")
                .ToList();
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiReactorApp<PageContainer>(
                    (app) =>
                    {
                        IMaterialColorService.Current.Initialize(app.Resources);
                        var autofacContainerBuilder = new ContainerBuilder();
                        autofacContainerBuilder
                            .RegisterType<CongestedClient>()
                            .As<IClient>()
                            .SingleInstance();
                        autofacContainerBuilder
                            .RegisterType<LocationProvider>()
                            .As<ILocationProvider>()
                            .SingleInstance();

                        var container = autofacContainerBuilder.Build();
                        ServiceLocator.SetLocatorProvider(
                            () => new AutofacServiceLocator(container)
                        );
                    }
                )
                .UseMauiCommunityToolkit()
#if DEBUG
                .EnableMauiReactorHotReload()
#endif
                .UseMaterialColors()
                .ConfigureFonts(fonts =>
                {
                    list.ForEach(w => fonts.AddFont($"Poppins-{w}.ttf", $"{w}"));
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MIcon");
                    fonts.AddFont("MaterialIconsOutlined-Regular.ttf", "MIconOutlined");
                });
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
