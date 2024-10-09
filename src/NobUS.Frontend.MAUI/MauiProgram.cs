using CommunityToolkit.Maui;
using MaterialColorUtilities.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Hosting;
using MoreLinq;
using NobUS.DataContract.Reader.OfficialAPI;
using NobUS.Frontend.MAUI.Presentation;
using NobUS.Frontend.MAUI.Service;
using NobUS.Infrastructure;

namespace NobUS.Frontend.MAUI;

public static class MauiProgram
{
    private static readonly string[] poppinsVariants = ["ExtraBold", "Regular", "SemiBold", "Bold"];
    private static readonly string[] miconFontVariants = ["Regular", "Outlined", "Round", "Sharp"];

    public static MauiApp CreateMauiApp()
    {
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
                poppinsVariants.ForEach(w => fonts.AddFont($"Poppins-{w}.ttf", $"{w}"));
                miconFontVariants.ForEach(w =>
                    fonts.AddFont($"Material-Icons-{w}.otf", $"MIcon-{w}")
                );
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
