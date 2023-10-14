using CommunityToolkit.Maui;
using Material.Components.Maui.Extensions;
using MaterialColorUtilities.Maui;
using MauiReactor;
using Microsoft.Extensions.Logging;
using NobUS.Frontend.MAUI.Presentation.View;

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
                .UseMauiApp<App>()
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
                })
                .UseMaterialComponents(list.Select(w => $"Poppins-{w}.ttf").ToList());
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
