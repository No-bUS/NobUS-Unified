using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NobUS.Extra.Campus.Facility.Sports;

namespace NobUS.Frontend.MAUI.Controls;

public sealed class FacilityOccupancyView : GraphicsView
{
    private readonly ProgressArcDrawable _drawable = new();

    public FacilityOccupancyView()
    {
        HeightRequest = 64;
        WidthRequest = 64;
        Drawable = _drawable;
    }

    public static readonly BindableProperty FacilityProperty = BindableProperty.Create(
        nameof(Facility),
        typeof(Facility),
        typeof(FacilityOccupancyView),
        propertyChanged: OnFacilityChanged
    );

    public Facility? Facility
    {
        get => (Facility?)GetValue(FacilityProperty);
        set => SetValue(FacilityProperty, value);
    }

    private static void OnFacilityChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is FacilityOccupancyView view)
        {
            view._drawable.Facility = newValue as Facility;
            view.Invalidate();
        }
    }

    private sealed class ProgressArcDrawable : IDrawable
    {
        public Facility? Facility { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Facility is null)
            {
                return;
            }

            var arcThickness = dirtyRect.Width / 10f;
            var progress = Math.Clamp(Facility.Occupancy, 0, 1);
            var endAngle = (float)(progress * 360); // degrees
            var color = progress switch
            {
                < 0.25 => Color.FromArgb("#8ecd1e"),
                < 0.5 => Color.FromArgb("#fffb1d"),
                < 0.75 => Color.FromArgb("#ffca00"),
                _ => Color.FromArgb("#ff5252"),
            };

            canvas.SaveState();
            canvas.StrokeColor = color;
            canvas.StrokeSize = arcThickness;
            canvas.DrawArc(
                arcThickness / 2,
                arcThickness / 2,
                dirtyRect.Width - arcThickness,
                dirtyRect.Height - arcThickness,
                0,
                endAngle,
                false,
                false
            );
            canvas.RestoreState();
        }
    }
}
