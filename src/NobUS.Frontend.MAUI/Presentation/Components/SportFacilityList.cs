using Microsoft.Maui.Graphics;
using NobUS.Extra.Campus.Facility.Sports;
using Type = NobUS.Extra.Campus.Facility.Sports.Type;

namespace NobUS.Frontend.MAUI.Presentation.Components
{
    internal class SportFacilityListState
    {
        public Facility[] Facilities { get; set; } = Array.Empty<Facility>();
    }

    internal class SportFacilityList : Component<SportFacilityListState>
    {
        private readonly double _ringSize = 50;

        private VisualNode RenderFacility(Facility f) =>
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
                        .Margin(0, 0, 0, _ringSize / 10)
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
                }.WidthRequest(100)
            }
                .ToCard(20)
                .Padding(20)
                .BackgroundColor(Styler.Scheme.SurfaceContainerHigh);

        private VisualNode RenderType(Type t) =>
            new VerticalStackLayout
            {
                new Label(t.ToString())
                    .Large()
                    .SemiBold()
                    .GridRow(0)
                    .TextColor(Styler.Scheme.OnSurface)
                    .Margin(0, 0, 0, 5),
                new CollectionView()
                    .ItemsSource(State.Facilities.Where(f => f.Type == t), RenderFacility)
                    .ItemsLayout(new HorizontalLinearItemsLayout().ItemSpacing(5))
            };

        public override VisualNode Render()
        {
            return new Border
            {
                new CollectionView()
                    .ItemsSource(
                        Enum.GetValues(typeof(Type))
                            .Cast<Type>()
                            .Where(t => State.Facilities.Where(f => f.Type == t).Any()),
                        RenderType
                    )
                    .ItemsLayout(new VerticalLinearItemsLayout().ItemSpacing(5))
            }
                .ToCard(20)
                .Padding(20)
                .Background(Styler.Scheme.SurfaceContainer)
                .VerticalOptions(LayoutOptions.Start)
                .HorizontalOptions(LayoutOptions.Start);
        }

        protected async Task Load()
        {
            var facilities = await Parser.GetAllAsync();
            SetState(s => s.Facilities = facilities);
        }

        protected override async void OnMounted()
        {
            await Load();
            base.OnMounted();
        }

        private class ProgressArc : IDrawable
        {
            public double Progress { get; set; }
            public string Name { get; set; }
            public Type Type { get; set; }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                var endAngle = (float)Math.Min(Progress * 360 + 10, 359.999);
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
