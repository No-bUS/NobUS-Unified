using CommunityToolkit.Maui.Markup;

namespace NobUS.Frontend.MAUI.Presentation.Components
{
    internal class NavigationShell : Component
    {
        public override VisualNode Render() =>
            new Shell
            {
                new ShellContent("Stations")
                    .Icon(MaterialIcons.PinDrop.ToFontImageSource())
                    .RenderContent(() => new ContentPage { new StationList() }),
                new ShellContent("Sports")
                    .Icon(MaterialIcons.FitnessCenter.ToFontImageSource())
                    .RenderContent(() => new ContentPage { new SportFacilityList() }),
            }
                .Set(Microsoft.Maui.Controls.Shell.BackgroundColorProperty, Styler.Scheme.Surface)
                .Set(
                    Microsoft.Maui.Controls.Shell.FlyoutBackgroundProperty,
                    Styler.Scheme.SurfaceContainer
                )
                .Set(
                    Microsoft.Maui.Controls.Shell.TabBarBackgroundColorProperty,
                    Styler.Scheme.SurfaceContainer
                )
                .Set(
                    Microsoft.Maui.Controls.Shell.TabBarTitleColorProperty,
                    Styler.Scheme.OnSurface
                )
                .Set(
                    Microsoft.Maui.Controls.Shell.TitleViewProperty,
                    new Microsoft.Maui.Controls.HorizontalStackLayout
                    {
                        new Microsoft.Maui.Controls.Label()
                        {
                            Text = "NobUS",
                            TextColor = Styler.Scheme.OnSurface,
                            FontFamily = "SemiBold",
                            HorizontalOptions = LayoutOptions.Start,
                            FontSize = Styles.Sizes.Medium,
                            VerticalOptions = LayoutOptions.Center,
                            FontAttributes = FontAttributes.Bold,
                        }
                    }.Padding(5)
                )
                .Set(Microsoft.Maui.Controls.Shell.TitleColorProperty, Styler.Scheme.OnSurface)
                .FlyoutHeader(
                    new Label("NobUS 🚀")
                        .SemiBold()
                        .Medium()
                        .Margin(5)
                        .TextColor(Styler.Scheme.OnSurface)
                        .HCenter()
                )
                .FlyoutHeaderBehavior(Microsoft.Maui.Controls.FlyoutHeaderBehavior.CollapseOnScroll)
                .FlyoutBackgroundColor(Styler.Scheme.SurfaceContainer)
                .ItemTemplate(RenderItem);

        public static VisualNode RenderItem(Microsoft.Maui.Controls.BaseShellItem item) =>
            new HorizontalStackLayout
            {
                new Label((item.Icon as FontImageSource).Glyph)
                    .FontFamily((item.Icon as FontImageSource).FontFamily)
                    .VCenter()
                    .Medium(),
                new Label()
                    .Text(item.Title)
                    .Medium()
                    .VCenter()
                    .Regular()
                    .TextColor(Styler.Scheme.OnSurface),
            }
                .Margin(5)
                .Spacing(5);
    }
}
