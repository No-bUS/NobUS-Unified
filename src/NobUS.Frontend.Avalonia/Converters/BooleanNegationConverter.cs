namespace NobUS.Frontend.Avalonia.Converters;

public sealed class BooleanNegationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo? culture) =>
        value is bool flag ? !flag : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo? culture) =>
        value is bool flag ? !flag : value;
}
