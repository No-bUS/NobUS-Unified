namespace NobUS.Frontend.Avalonia;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        _serviceProvider = ConfigureServices();

        if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
        {
            singleViewLifetime.MainView = new Views.MainView
            {
                DataContext = _serviceProvider.GetRequiredService<ViewModels.MainViewModel>(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<Services.IBusArrivalService, Services.DemoBusArrivalService>();
        services.AddSingleton<ViewModels.RouteArrivalGroupViewModel.Factory>(
            _ => (routeName, arrivals) => new ViewModels.RouteArrivalGroupViewModel(routeName, arrivals)
        );
        services.AddSingleton<ViewModels.MainViewModel>();

        return services.BuildServiceProvider();
    }
}
