using CommunityToolkit.Maui;
using Material.Components.Maui.Extensions;
using Microsoft.Extensions.Logging;
using NobUS.Frontend.MAUI.Presentation.View;

namespace NobUS.Frontend.MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var list = new[] { "ExtraBold", "Regular" }
                .SelectMany(w => new[] { "" }, (w, s) => $"{w}{s}")
                .ToList();
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(
                    fonts => list.ForEach(w => fonts.AddFont($"Poppins-{w}.ttf", $"{w}"))
                )
                .UseMaterialComponents(list.Select(w => $"Poppins-{w}.ttf").ToList());
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
