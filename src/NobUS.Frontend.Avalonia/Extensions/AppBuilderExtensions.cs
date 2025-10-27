namespace NobUS.Frontend.Avalonia.Extensions;

public static class AppBuilderExtensions
{
    extension(AppBuilder builder)
    {
        public AppBuilder UseNobUSDefaults() => builder
            .UseReactiveUI()
            .WithInterFont();
    }
}
