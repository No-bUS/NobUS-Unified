namespace NobUS.Frontend.Avalonia;

public sealed class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var name = data.GetType().FullName?.Replace("ViewModel", "View", StringComparison.Ordinal);
        if (name is null)
        {
            return null;
        }

        var type = Type.GetType(name);
        return type is null ? new TextBlock { Text = name } : Activator.CreateInstance(type) as Control;
    }

    public bool Match(object? data) => data is ReactiveObject;
}
