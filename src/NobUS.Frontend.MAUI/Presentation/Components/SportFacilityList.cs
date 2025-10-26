using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using NobUS.Extra.Campus.Facility.Sports;
using Type = NobUS.Extra.Campus.Facility.Sports.Type;

namespace NobUS.Frontend.MAUI.Presentation.Components;

internal class SportFacilityListState
{
    public Facility[] Facilities { get; set; } = Array.Empty<Facility>();
    public Dictionary<string, double> PreviousOccupancies { get; set; } = new();
    public bool IsLoading { get; set; }
    public bool IsRefreshing { get; set; }
    public bool IsBackgroundRefreshing { get; set; }
    public double AnimationProgress { get; set; } = 1;
    public DateTimeOffset? LastUpdated { get; set; }
    public string? ErrorMessage { get; set; }
}

internal class SportFacilityList : Component<SportFacilityListState>, INavigationAware
{
    private readonly double _ringSize = 48;
    private bool _isFetching;

    private enum RefreshTrigger
    {
        Initial,
        Manual,
        Background,
    }

    private VisualNode RenderFacility(Facility facility)
    {
        double previousOccupancy = State.PreviousOccupancies.TryGetValue(
            facility.Name,
            out double previous
        )
            ? previous
            : facility.Occupancy;

        double displayProgress =
            previousOccupancy + (facility.Occupancy - previousOccupancy) * State.AnimationProgress;
        displayProgress = Math.Clamp(displayProgress, 0, 1);

        bool highlightChange =
            Math.Abs(facility.Occupancy - previousOccupancy) > 0.015 && State.AnimationProgress < 1;

        return new Border
        {
            new Grid("auto,auto", "auto,*")
            {
                new GraphicsView()
                    .HeightRequest(_ringSize)
                    .WidthRequest(_ringSize)
                    .Drawable(new ProgressArc { Progress = displayProgress, Type = facility.Type })
                    .GridColumn(0)
                    .GridRowSpan(2)
                    .Margin(0, 0, 12, 0),
                new VerticalStackLayout
                {
                    new Label(facility.Name)
                        .Medium()
                        .SemiBold()
                        .TextColor(Styler.Scheme.OnSurface)
                        .LineBreakMode(Microsoft.Maui.LineBreakMode.TailTruncation),
                    new Label($"{facility.Load} / {facility.Capacity} in use")
                        .Small()
                        .Regular()
                        .TextColor(Styler.Scheme.OnSurfaceVariant),
                }
                    .GridColumn(1)
                    .Spacing(2)
                    .Margin(0, 0, 0, 2),
                new Label($"{displayProgress:P0}")
                    .FontFamily("SemiBold")
                    .Base()
                    .TextColor(Styler.Scheme.OnSurface)
                    .GridColumn(1)
                    .GridRow(1),
            }
                .ColumnSpacing(12)
                .RowSpacing(4),
        }
            .ToCard(24)
            .Padding(16)
            .Background(
                highlightChange
                    ? Styler.Scheme.SecondaryContainer
                    : Styler.Scheme.SurfaceContainerHigh
            )
            .Margin(0, 0, 10, 0);
    }

    private VisualNode RenderType(Type type) =>
        new VerticalStackLayout
        {
            new Label(type.ToString())
                .Medium()
                .SemiBold()
                .TextColor(Styler.Scheme.OnSurface)
                .Margin(4, 0, 0, 8),
            new CollectionView()
                .ItemsSource(State.Facilities.Where(f => f.Type == type), RenderFacility)
                .ItemsLayout(new HorizontalLinearItemsLayout().ItemSpacing(12))
                .HorizontalScrollBarVisibility(ScrollBarVisibility.Never),
        };

    public override VisualNode Render()
    {
        var sections = Enum.GetValues(typeof(Type))
            .Cast<Type>()
            .Where(t => State.Facilities.Any(f => f.Type == t))
            .ToList();

        return new Grid
        {
            new Border
            {
                new Grid("auto,auto,*", "*")
                {
                    new Grid("*", "*,auto")
                    {
                        new Label("Campus activity")
                            .Large()
                            .Bold()
                            .TextColor(Styler.Scheme.OnSurface),
                        new HorizontalStackLayout
                        {
                            State.IsBackgroundRefreshing
                                ? new Border
                                {
                                    new Label("Updating…")
                                        .Small()
                                        .SemiBold()
                                        .TextColor(Styler.Scheme.Primary),
                                }
                                    .Background(Styler.Scheme.SurfaceContainerHigh)
                                    .Padding(8, 4)
                                    .Margin(0, 0, 8, 0)
                                : null,
                            new Border
                            {
                                new Label(char.ConvertFromUtf32((int)MaterialIcons.Refresh))
                                    .FontFamily("MIcon-Regular")
                                    .FontSize(18)
                                    .TextColor(Styler.Scheme.OnSecondaryContainer)
                                    .HCenter()
                                    .VCenter(),
                            }
                                .HeightRequest(40)
                                .WidthRequest(40)
                                .Background(Styler.Scheme.SecondaryContainer)
                                .ToCard(16)
                                .OnTapped(async () => await LoadAsync(RefreshTrigger.Manual))
                                .InputTransparent(State.IsRefreshing)
                                .Opacity(State.IsRefreshing ? 0.6f : 1f),
                        }
                            .Spacing(8)
                            .HEnd(),
                    }.ColumnSpacing(12),
                    new Grid("*", "*,auto")
                    {
                        new Label(
                            State.LastUpdated is null
                                ? "Waiting for the first update"
                                : $"Last updated • {State.LastUpdated.Value.ToLocalTime():HH:mm:ss}"
                        )
                            .Small()
                            .TextColor(Styler.Scheme.OnSurfaceVariant)
                            .FontFamily("Regular"),
                        State.IsRefreshing
                            ? new ActivityIndicator()
                                .IsRunning(true)
                                .HeightRequest(18)
                                .WidthRequest(18)
                                .Color(Styler.Scheme.Primary)
                                .HEnd()
                                .VCenter()
                            : null,
                    }
                        .ColumnSpacing(8)
                        .GridRow(1),
                    sections.Any()
                        ? new CollectionView()
                            .ItemsSource(sections, RenderType)
                            .ItemsLayout(new VerticalLinearItemsLayout().ItemSpacing(18))
                            .GridRow(2)
                    : State.IsLoading ? null
                    : new Label("No live facility information right now.")
                        .TextColor(Styler.Scheme.OnSurfaceVariant)
                        .GridRow(2)
                        .FontFamily("Regular")
                        .Margin(0, 16, 0, 0),
                }.RowSpacing(16),
            }
                .Padding(20)
                .StrokeThickness(0)
                .Stroke(Colors.Transparent)
                .Background(
                    new Microsoft.Maui.Controls.LinearGradientBrush
                    {
                        GradientStops =
                        {
                            new Microsoft.Maui.Controls.GradientStop(
                                Styler.Scheme.SurfaceContainerHigh,
                                0f
                            ),
                            new Microsoft.Maui.Controls.GradientStop(
                                Styler.Scheme.SurfaceContainer,
                                1f
                            ),
                        },
                        EndPoint = new Point(1, 1),
                    }
                ),
            State.IsLoading
                ? new Grid
                {
                    new ActivityIndicator()
                        .IsRunning(true)
                        .Color(Styler.Scheme.Primary)
                        .HeightRequest(40)
                        .WidthRequest(40)
                        .HCenter()
                        .VCenter(),
                }
                    .BackgroundColor(Styler.Scheme.Surface.WithAlpha(0.65f))
                    .Padding(24)
                : null,
            !string.IsNullOrWhiteSpace(State.ErrorMessage)
                ? new Border
                {
                    new Label(State.ErrorMessage)
                        .TextColor(Styler.Scheme.Error)
                        .FontFamily("SemiBold")
                        .LineBreakMode(Microsoft.Maui.LineBreakMode.WordWrap)
                        .HCenter(),
                }
                    .Background(Styler.Scheme.ErrorContainer)
                    .Padding(10)
                    .ToCard(18)
                    .Margin(0, 8, 0, 0)
                    .VerticalOptions(LayoutOptions.End)
                : null,
        };
    }

    private async Task LoadAsync(RefreshTrigger trigger)
    {
        if (_isFetching)
        {
            return;
        }

        _isFetching = true;

        bool hasExistingData = State.Facilities.Any();

        SetState(s =>
        {
            s.ErrorMessage = null;
            s.IsLoading = trigger == RefreshTrigger.Initial && !hasExistingData;
            s.IsRefreshing = trigger == RefreshTrigger.Manual && hasExistingData;
            s.IsBackgroundRefreshing = trigger == RefreshTrigger.Background && hasExistingData;
            if (!hasExistingData)
            {
                s.AnimationProgress = 0;
            }
        });

        try
        {
            var facilities = await Parser.GetAllAsync();
            var previous = State.Facilities.ToDictionary(f => f.Name, f => f.Occupancy);

            SetState(s =>
            {
                s.PreviousOccupancies = previous;
                s.Facilities = facilities;
                s.LastUpdated = DateTimeOffset.Now;
                s.IsLoading = false;
                s.IsRefreshing = false;
                s.IsBackgroundRefreshing = false;
                s.AnimationProgress = previous.Count == 0 ? 1 : 0;
            });

            if (previous.Count > 0)
            {
                StartProgressAnimation();
            }
        }
        catch
        {
            SetState(s =>
            {
                s.IsLoading = false;
                s.IsRefreshing = false;
                s.IsBackgroundRefreshing = false;
                s.ErrorMessage = "Couldn't refresh facility data. Tap refresh to retry.";
            });
        }
        finally
        {
            _isFetching = false;
        }
    }

    private void StartProgressAnimation()
    {
        const double duration = 0.45;
        DateTimeOffset start = DateTimeOffset.Now;

        var dispatcher =
            Dispatcher.GetForCurrentThread()
            ?? Microsoft.Maui.Controls.Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            SetState(s =>
            {
                s.AnimationProgress = 1;
                s.PreviousOccupancies = new();
            });
            return;
        }

        dispatcher.StartTimer(
            TimeSpan.FromMilliseconds(16),
            () =>
            {
                double elapsed = (DateTimeOffset.Now - start).TotalSeconds;
                double progress = Math.Clamp(elapsed / duration, 0, 1);

                SetState(s =>
                {
                    s.AnimationProgress = progress;
                    if (progress >= 1)
                    {
                        s.PreviousOccupancies = new();
                    }
                });

                return progress < 1;
            }
        );
    }

    protected override async void OnMounted()
    {
        await LoadAsync(RefreshTrigger.Initial);
        base.OnMounted();
    }

    public void OnNavigatedTo()
    {
        _ = LoadAsync(RefreshTrigger.Background);
    }

    public void OnNavigatedFrom() { }

    private class ProgressArc : IDrawable
    {
        public double Progress { get; set; }
        public Type Type { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            float thickness = dirtyRect.Width / 10f;
            float sweep = (float)Math.Min(Progress * 360 + 6, 359.99);
            canvas.StrokeColor = Progress switch
            {
                < 0.25 => Color.FromArgb("#7BCF4A"),
                < 0.5 => Color.FromArgb("#FFDD4A"),
                < 0.75 => Color.FromArgb("#FFC043"),
                _ => Color.FromArgb("#FF6B6B"),
            };
            canvas.StrokeSize = thickness;
            canvas.StrokeLineCap = LineCap.Round;

            canvas.DrawArc(
                thickness / 2,
                thickness / 2,
                dirtyRect.Width - thickness,
                dirtyRect.Height - thickness,
                0,
                sweep,
                false,
                false
            );
        }
    }
}
