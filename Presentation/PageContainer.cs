using NobUS.Frontend.MAUI.Service;
using NobUS.Frontend.MAUI.Presentation.Components;
using NobUS.Frontend.MAUI.Presentation.Pages;

namespace NobUS.Frontend.MAUI.Presentation
{
    internal class PageContainer : Component
    {
        public override VisualNode Render()
        {
#if !WINDOWS
            CommunityToolkit.Maui.Core.Platform.StatusBar.SetColor(Styler.Scheme.SurfaceContainer);
#endif
            return new ContentPage
            {
                new Grid("auto,*", "*")
                {
                    new Label("NobUS")
                        .Large()
                        .Bold()
                        .TextColor(Styler.Scheme.OnSurface)
                        .VCenter()
                        .GridRow(0)
                        .Padding(10, 5)
                        .Background(Styler.Scheme.SurfaceContainer),
                    new NavigationBar
                    {
                        new NavigationBarItem(
                            "Stations",
                            MaterialIcons.PinDrop,
                            () => new StationList()
                        ),
                        new NavigationBarItem(
                            "Sports",
                            MaterialIcons.FitnessCenter,
                            () => new ToolsPage()
                        ),
                    }.GridRow(1)
                }
            };
        }
    }
}
