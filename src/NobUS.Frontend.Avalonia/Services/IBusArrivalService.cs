namespace NobUS.Frontend.Avalonia.Services;

public interface IBusArrivalService
{
    IReadOnlyList<Station> GetStations();

    Task<IReadOnlyList<RouteArrivals>> GetArrivalsAsync(Station station, CancellationToken cancellationToken);

    public sealed record RouteArrivals(string RouteName, IReadOnlyList<RouteArrivals.Arrival> Arrivals)
    {
        public readonly record struct Arrival(string VehiclePlate, TimeSpan WaitTime);
    }
}
