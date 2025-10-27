using CommunityToolkit.Maui;
using MaterialColorUtilities.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using MoreLinq;
using NobUS.DataContract.Reader.OfficialAPI;
using NobUS.Frontend.MAUI.Service;
using NobUS.Frontend.MAUI.ViewModels;
using NobUS.Frontend.MAUI.Views;
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
            .UseMauiApp<App>()
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
        builder.Services
            .AddSingleton<IClient, CongestedClient>()
            .AddSingleton<ILocationProvider, LocationProvider>()
            .AddSingleton(provider => new ArrivalEventListener(
                station => provider.GetRequiredService<IClient>().GetArrivalEventsAsync(station)
            ))
            .AddSingleton<AppShell>()
            .AddTransient<StationsPage>()
            .AddTransient<SportsPage>()
            .AddTransient<StationsPageViewModel>()
            .AddTransient<SportsPageViewModel>();

        return builder.Build();
    }
}
