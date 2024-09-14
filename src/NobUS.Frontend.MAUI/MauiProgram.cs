using CommunityToolkit.Maui;
using MaterialColorUtilities.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using NobUS.DataContract.Reader.OfficialAPI;
using NobUS.Frontend.MAUI.Presentation;
using NobUS.Frontend.MAUI.Service;
using NobUS.Infrastructure;

namespace NobUS.Frontend.MAUI;

public static class MauiProgram
{
    private static readonly string[] stringArray = [""];
    private static readonly string[] sourceArray = ["ExtraBold", "Regular", "SemiBold", "Bold"];

    public static MauiApp CreateMauiApp()
    {
        var list = sourceArray.SelectMany(w => stringArray, (w, s) => $"{w}{s}").ToList();
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiReactorApp<PageContainer>(
                (app) => IMaterialColorService.Current.Initialize(app.Resources)
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
        builder
            .Services.AddScoped<IClient, CongestedClient>()
            .AddScoped<ILocationProvider, LocationProvider>()
            .AddScoped(provider => new ArrivalEventListener(
                async (station) =>
                    await provider.GetRequiredService<IClient>().GetArrivalEventsAsync(station)
            ));

        return builder.Build();
    }
}
