using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using NobUS.Frontend.MAUI.Presentation.Components;
using NobUS.Frontend.MAUI.Presentation.Pages;

namespace NobUS.Frontend.MAUI.Presentation;

internal class PageContainer : Component
{
    public override VisualNode Render()
    {
#if !WINDOWS
        CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(Styler.Scheme.Primary);
#endif
        string greeting = DateTime.Now.Hour switch
        {
            < 6 => "Night owl mode",
            < 9 => "Good morning",
            < 12 => "Plan the day",
            < 15 => "Midday momentum",
            < 18 => "Keep moving",
            < 21 => "Good evening",
            _ => "Late-night ride",
        };

        return new ContentPage
        {
            new Grid("auto,*", "*")
            {
                new Border
                {
                    new VerticalStackLayout
                    {
                        new Label("NobUS")
                            .FontFamily("ExtraBold")
                            .FontSize(30)
                            .TextColor(Styler.Scheme.OnPrimary),
                        new Label($"{greeting} ✨")
                            .FontFamily("SemiBold")
                            .FontSize(15)
                            .TextColor(Styler.Scheme.OnPrimary)
                            .Opacity(0.9f),
                        new Label("Your realtime campus companion")
                            .FontFamily("Regular")
                            .FontSize(13)
                            .TextColor(Styler.Scheme.OnPrimary)
                            .Opacity(0.78f)
                            .Margin(0, 4, 0, 0),
                    }.Spacing(2),
                }
                    .Padding(24, 28, 24, 36)
                    .Background(
                        new Microsoft.Maui.Controls.LinearGradientBrush
                        {
                            GradientStops =
                            {
                                new Microsoft.Maui.Controls.GradientStop(
                                    Styler.Scheme.Primary,
                                    0f
                                ),
                                new Microsoft.Maui.Controls.GradientStop(
                                    Styler.Scheme.Secondary,
                                    1f
                                ),
                            },
                            EndPoint = new Point(0.9, 1),
                        }
                    )
                    .StrokeThickness(0)
                    .Stroke(Colors.Transparent)
                    .GridRow(0),
                new ContentView
                {
                    new NavigationBar
                    {
                        new NavigationBarItem(
                            "Stations",
                            MaterialIcons.PinDrop,
                            static () => new StationList()
                        ),
                        new NavigationBarItem(
                            "Sports",
                            MaterialIcons.FitnessCenter,
                            static () => new ToolsPage()
                        ),
                    },
                }
                    .Margin(0, -16, 0, 0)
                    .GridRow(1),
            },
        }.Background(Styler.Scheme.Surface);
    }
}
