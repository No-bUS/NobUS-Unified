using Microsoft.Maui.Graphics;
using NobUS.Frontend.MAUI.Presentation.Components;

namespace NobUS.Frontend.MAUI.Presentation.Pages;

internal class ToolsPage : Component, INavigationAware
{
    private readonly SportFacilityList _facilityList = new();

    public override VisualNode Render() =>
        new ScrollView
        {
            new VerticalStackLayout
            {
                new VerticalStackLayout
                {
                    new Label("Stay active").Large().Bold().TextColor(Styler.Scheme.OnSurface),
                    new Label("Live utilisation for campus gyms and courts.")
                        .Small()
                        .TextColor(Styler.Scheme.OnSurfaceVariant),
                }
                    .Spacing(4)
                    .Margin(0, 0, 0, 12),
                _facilityList,
            }
                .Spacing(24)
                .Padding(24, 0, 24, 120),
        }.Background(
            new Microsoft.Maui.Controls.LinearGradientBrush
            {
                GradientStops =
                {
                    new Microsoft.Maui.Controls.GradientStop(
                        Styler.Scheme.SurfaceVariant.WithAlpha(0.08f),
                        0f
                    ),
                    new Microsoft.Maui.Controls.GradientStop(Styler.Scheme.Surface, 1f),
                },
                EndPoint = new Point(0.5, 1),
            }
        );

    public void OnNavigatedTo()
    {
        _facilityList.OnNavigatedTo();
    }

    public void OnNavigatedFrom()
    {
        _facilityList.OnNavigatedFrom();
    }
}
