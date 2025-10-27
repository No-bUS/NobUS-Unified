using MaterialColorUtilities.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace NobUS.Frontend.MAUI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = default!;
    private readonly IServiceProvider _services;

    private static readonly string[] ColorResourceKeys =
    [
        "SurfaceColor",
        "SurfaceContainerColor",
        "SurfaceContainerHighColor",
        "SecondaryContainerColor",
        "SecondaryColor",
        "OnSurfaceColor",
        "OnSecondaryColor",
        "OnSecondaryContainerColor",
    ];

    public App(IServiceProvider services)
    {
        InitializeComponent();

        Services = services;
        _services = services;
        ApplyMaterialColors();
    }

    protected override Window CreateWindow(IActivationState? activationState) =>
        new()
        {
            Page = _services.GetRequiredService<AppShell>(),
        };

    private void ApplyMaterialColors()
    {
        if (IMaterialColorService.Current is not MaterialColorService materialService)
        {
            return;
        }

        materialService.Initialize(Resources);
        var scheme = materialService.SchemeMaui;

        var colorMap = new Dictionary<string, Color>
        {
            ["SurfaceColor"] = scheme.Surface,
            ["SurfaceContainerColor"] = scheme.SurfaceContainer,
            ["SurfaceContainerHighColor"] = scheme.SurfaceContainerHigh,
            ["SecondaryContainerColor"] = scheme.SecondaryContainer,
            ["SecondaryColor"] = scheme.Secondary,
            ["OnSurfaceColor"] = scheme.OnSurface,
            ["OnSecondaryColor"] = scheme.OnSecondary,
            ["OnSecondaryContainerColor"] = scheme.OnSecondaryContainer,
        };

        foreach (var key in ColorResourceKeys)
        {
            if (colorMap.TryGetValue(key, out var color))
            {
                Resources[key] = color;
            }
        }
    }
}
