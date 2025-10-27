using System.Globalization;
using Microsoft.Maui.Controls;

namespace NobUS.Frontend.MAUI.Converters;

public sealed class TimeSpanToEtaConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimeSpan timeSpan)
        {
            return string.Empty;
        }

        var minutes = Math.Max(0, Math.Floor(timeSpan.TotalMinutes));
        var suffix = timeSpan.Seconds > 30 ? "+" : string.Empty;
        return $"{minutes:0}m{suffix}";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class TimeSpanToTextDecorationsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimeSpan timeSpan)
        {
            return TextDecorations.None;
        }

        if (timeSpan < TimeSpan.FromMinutes(5))
        {
            return TextDecorations.Underline;
        }

        if (timeSpan >= TimeSpan.FromMinutes(60))
        {
            return TextDecorations.Strikethrough;
        }

        return TextDecorations.None;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class TimeSpanToFontAttributesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not TimeSpan timeSpan)
        {
            return FontAttributes.None;
        }

        if (timeSpan >= TimeSpan.FromMinutes(30))
        {
            return FontAttributes.Italic;
        }

        return FontAttributes.None;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
