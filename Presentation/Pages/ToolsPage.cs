using MauiReactor;
using NobUS.Frontend.MAUI.Service;
using Grid = MauiReactor.Grid;
using NobUS.Extra.Campus.Facility.Sports;
using Label = MauiReactor.Label;
using VerticalStackLayout = MauiReactor.VerticalStackLayout;
using CollectionView = MauiReactor.CollectionView;
using GraphicsView = MauiReactor.GraphicsView;
using Border = MauiReactor.Border;
using ScrollView = MauiReactor.ScrollView;

namespace NobUS.Frontend.MAUI.Presentation.Pages
{
    internal class ToolsPage : Component
    {
        private readonly double _ringSize = 100;

        public override VisualNode Render()
        {
            Facility[] facilities = Parser.GetAll();
            return new ScrollView
            {
                new VerticalStackLayout
                {
                    Enum.GetValues(typeof(Extra.Campus.Facility.Sports.Type))
                        .Cast<Extra.Campus.Facility.Sports.Type>()
                        .Where(t => facilities.Where(f => f.Type == t).Any())
                        .Select(
                            t =>
                                new VerticalStackLayout
                                {
                                    new Label(t.ToString())
                                        .Large()
                                        .SemiBold()
                                        .TextColor(Styler.Scheme.OnSurface)
                                        .Margin(20),
                                    new CollectionView()
                                        .ItemsSource(
                                            facilities.Where(f => f.Type == t),
                                            f =>
                                                new Border
                                                {
                                                    new Grid("auto,*,auto", "*")
                                                    {
                                                        new GraphicsView()
                                                            .HeightRequest(_ringSize)
                                                            .WidthRequest(_ringSize)
                                                            .Drawable(
                                                                new ProgressArc
                                                                {
                                                                    Progress = f.Occupancy,
                                                                    Name = f.Name,
                                                                    Type = f.Type
                                                                }
                                                            )
                                                            .Margin(0,0,0,_ringSize/10)
                                                            .GridRow(0),
                                                        new Label(f.Name)
                                                            .Base()
                                                            .VerticalOptions(LayoutOptions.Center)
                                                            .TextColor(Styler.Scheme.OnSurface)
                                                            .SemiBold()
                                                            .GridRow(1),
                                                        new Label($"{f.Load} / {f.Capacity}")
                                                            .Regular()
                                                            .TextColor(Styler.Scheme.OnSurface)
                                                            .SemiBold()
                                                            .GridRow(2)
                                                    }
                                                        .WidthRequest(120)
                                                        .Margin(20)
                                                }
                                                    .ToCard(20)
                                                    .BackgroundColor(Styler.Scheme.SurfaceContainer)
                                        )
                                        .ItemsLayout(
                                            new HorizontalLinearItemsLayout().ItemSpacing(10)
                                        )
                                }
                        ),
                }
            };
        }

        private class ProgressArc : IDrawable
        {
            public double Progress { get; set; }
            public string Name { get; set; }
            public Extra.Campus.Facility.Sports.Type Type { get; set; }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                var endAngle = (float)
                    Math.Min(Progress * 360 + 10, 359.999);
                canvas.StrokeColor = Progress switch
                {
                    < 0.25 => Color.FromArgb("#8ecd1e"),
                    < 0.5 => Color.FromArgb("#fffb1d"),
                    < 0.75 => Color.FromArgb("#ffca00"),
                    _ => Color.FromArgb("#ff5252"),

                };
                float thickness = dirtyRect.Width / 10;
                canvas.StrokeSize = thickness;

                canvas.DrawArc(
                    thickness / 2,
                    thickness / 2,
                    dirtyRect.Width - thickness,
                    dirtyRect.Width - thickness,
                    0,
                    endAngle,
                    false,
                    false
                );
            }
        }
    }
}
