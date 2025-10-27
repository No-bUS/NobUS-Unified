namespace NobUS.Frontend.Avalonia.ViewModels;

public static class DesignData
{
    public static MainViewModel Main { get; } = Create();

    private static MainViewModel Create()
    {
        var service = new Services.DemoBusArrivalService();
        RouteArrivalGroupViewModel.Factory factory = (routeName, arrivals) =>
            new RouteArrivalGroupViewModel(routeName, arrivals);
        return new MainViewModel(service, factory);
    }
}
