namespace NobUS.Frontend.Avalonia.ViewModels;

public sealed class RouteArrivalGroupViewModel : ReactiveObject
{
    public delegate RouteArrivalGroupViewModel Factory(string routeName, IEnumerable<ArrivalPredictionViewModel> arrivals);

    public RouteArrivalGroupViewModel(string routeName, IEnumerable<ArrivalPredictionViewModel> arrivals)
    {
        RouteName = routeName;
        Arrivals = new ObservableCollection<ArrivalPredictionViewModel>(arrivals);
    }

    public string RouteName { get; }

    public ObservableCollection<ArrivalPredictionViewModel> Arrivals { get; }

    public sealed class ArrivalPredictionViewModel(TimeSpan waitTime, string vehiclePlate) : ReactiveObject
    {
        public TimeSpan WaitTime { get; } = waitTime;

        public string VehiclePlate { get; } = vehiclePlate;

        public string DisplayWait => WaitTime <= TimeSpan.Zero
            ? "Arriving"
            : WaitTime < TimeSpan.FromMinutes(1)
                ? $"{WaitTime.Seconds}s"
                : $"{WaitTime.Minutes}m {WaitTime.Seconds:D2}s";
    }
}
