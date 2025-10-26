using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NobUS.Frontend.MAUI.Presentation.Components;

namespace NobUS.Frontend.MAUI.Presentation.Pages;

internal class ToolsPage : Component
{
    public override VisualNode Render() =>
        new ScrollView
        {
            new VerticalStackLayout
            {
                new SportFacilityList(),
            }
                .Spacing(24)
                .Padding(24, 0, 24, 120),
        }
            .Background(new LinearGradientBrush
            {
                GradientStops =
                {
                    new GradientStop(Styler.Scheme.SurfaceVariant.WithAlpha(0.08f), 0f),
                    new GradientStop(Styler.Scheme.Surface, 1f),
                },
                EndPoint = new Point(0.5, 1),
            });
}
