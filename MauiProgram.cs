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
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    new[]
                    {
                        "Black",
                        "ExtraBold",
                        "SemiBold",
                        "Bold",
                        "Medium",
                        "Regular",
                        "Light",
                        "ExtraLight",
                        "Thin",
                        "ExtraThin"
                    }
                        .SelectMany(w => new[] { "Italic", "" }, (w, s) => $"{w}{s}")
                        .ToList()
                        .ForEach(w => fonts.AddFont($"Poppins-{w}.ttf", $"{w}"));
                })
                .UseMaterialComponents(new List<string>());
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
