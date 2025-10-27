using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using NobUS.Extra.Campus.Facility.Sports;
using Type = NobUS.Extra.Campus.Facility.Sports.Type;

namespace NobUS.Frontend.MAUI.Presentation.Components;

internal class SportFacilityListState
{
    public Facility[] Facilities { get; set; } = Array.Empty<Facility>();
    public bool IsLoading { get; set; }
}

internal partial class SportFacilityList : DisposableComponent<SportFacilityListState>
{
    private readonly double _ringSize = 50;

    [Inject]
    private IFacilityParser facilityParser = null!;

    private VisualNode RenderFacility(Facility facility) =>
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
                            Progress = facility.Occupancy,
                            Name = facility.Name,
                            Type = facility.Type,
                        }
                    )
                    .Margin(0, 0, 0, _ringSize / 10)
                    .GridRow(0),
                new Label(facility.Name)
                    .Base()
                    .VerticalOptions(LayoutOptions.Center)
                    .TextColor(Styler.Scheme.OnSurface)
                    .SemiBold()
                    .GridRow(1),
                new Label($"{facility.Load} / {facility.Capacity}")
                    .Regular()
                    .TextColor(Styler.Scheme.OnSurface)
                    .SemiBold()
                    .GridRow(2),
            }.WidthRequest(100),
        }
            .ToCard(20)
            .Padding(20)
            .BackgroundColor(Styler.Scheme.SurfaceContainerHigh);

    private VisualNode RenderType(Type type) =>
        new VerticalStackLayout
        {
            new Label(type.ToString())
                .Large()
                .SemiBold()
                .GridRow(0)
                .TextColor(Styler.Scheme.OnSurface)
                .Margin(0, 0, 0, 5),
            new CollectionView()
                .ItemsSource(
                    State.Facilities.Where(facility => facility.Type == type),
                    RenderFacility
                )
                .ItemsLayout(new HorizontalLinearItemsLayout().ItemSpacing(5)),
        };

    public override VisualNode Render()
    {
        VisualNode content = State.IsLoading
            ? new ActivityIndicator()
                .IsRunning(true)
                .IsVisible(true)
                .HorizontalOptions(LayoutOptions.Center)
                .VerticalOptions(LayoutOptions.Center)
            : new CollectionView()
                .ItemsSource(
                    Enum.GetValues(typeof(Type))
                        .Cast<Type>()
                        .Where(type => State.Facilities.Any(facility => facility.Type == type)),
                    RenderType
                )
                .ItemsLayout(new VerticalLinearItemsLayout().ItemSpacing(5));

        return new Border { content }
            .ToCard(20)
            .Padding(20)
            .Background(Styler.Scheme.SurfaceContainer)
            .VerticalOptions(LayoutOptions.Start)
            .HorizontalOptions(LayoutOptions.Start);
    }

    protected async Task LoadAsync(CancellationToken cancellationToken)
    {
        SetState(state => state.IsLoading = true);
        try
        {
            var facilities = await facilityParser
                .GetAllAsync(cancellationToken)
                .ConfigureAwait(false);
            SetState(state =>
            {
                state.Facilities = facilities.ToArray();
                state.IsLoading = false;
            });
        }
        catch (OperationCanceledException)
        {
            SetState(state => state.IsLoading = false);
        }
    }

    protected override async void OnMounted()
    {
        var cts = new CancellationTokenSource();
        RegisterResource(cts);
        await LoadAsync(cts.Token);
        base.OnMounted();
    }

    private class ProgressArc : IDrawable
    {
        public double Progress { get; init; }

        public required string Name { get; init; }

        public required Type Type { get; init; }

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
