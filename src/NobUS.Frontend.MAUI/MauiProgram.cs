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
    private static readonly (string File, string Alias)[] poppinsFonts =
    [
        ("Poppins-Regular.ttf", "Regular"),
        ("Poppins-SemiBold.ttf", "SemiBold"),
        ("Poppins-Bold.ttf", "Bold"),
        ("Poppins-ExtraBold.ttf", "ExtraBold"),
    ];

    private static readonly (string File, string Alias)[] materialIconFonts =
    [
        ("MaterialSymbolsOutlined.ttf", "MIcon-Regular"),
        ("MaterialSymbolsOutlined.otf", "MIcon-Outlined"),
        ("MaterialSymbolsRounded.otf", "MIcon-Round"),
        ("MaterialSymbolsSharp.otf", "MIcon-Sharp"),
    ];

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiReactorApp<PageContainer>(
                (app) => IMaterialColorService.Current.Initialize(app.Resources)
            )
            .UseMauiCommunityToolkit()
            .UseMaterialColors()
            .ConfigureFonts(fonts =>
            {
                poppinsFonts.ForEach(font => fonts.AddFont(font.File, font.Alias));
                materialIconFonts.ForEach(font => fonts.AddFont(font.File, font.Alias));
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
