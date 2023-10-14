using MauiReactor;
using NobUS.Frontend.MAUI.Service;
using NobUS.Frontend.MAUI.Presentation.Components;
using NobUS.Frontend.MAUI.Presentation.Pages;
using ContentPage = MauiReactor.ContentPage;
using HorizontalStackLayout = MauiReactor.HorizontalStackLayout;
using Label = MauiReactor.Label;
using Grid = MauiReactor.Grid;
using VerticalStackLayout = MauiReactor.VerticalStackLayout;

namespace NobUS.Frontend.MAUI.Presentation
{
    internal class PageContainer : Component
    {
        public override VisualNode Render() =>
            new ContentPage
            {
                new Grid("auto,*", "*")
                {
                    new Label("NobUS")
                        .Large()
                        .Regular()
                        .TextColor(Styler.Scheme.OnSurface)
                        .VCenter()
                        .GridRow(0)
                        .Padding(5)
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
