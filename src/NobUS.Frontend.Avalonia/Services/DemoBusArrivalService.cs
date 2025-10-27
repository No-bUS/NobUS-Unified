namespace NobUS.Frontend.Avalonia.Services;

public sealed class DemoBusArrivalService : IBusArrivalService
{
    private readonly Random _random = new(0x4E5655);

    public IReadOnlyList<Station> GetStations() => DefinitionLoader.GetAllStations.OrderBy(s => s.Name).ToList();

    public Task<IReadOnlyList<IBusArrivalService.RouteArrivals>> GetArrivalsAsync(
        Station station,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var arrivals = DefinitionLoader.Routes
            .Values
            .Where(route => route.ToStations.Any(routeStation => routeStation.Id == station.Code))
            .Select(route => CreateGroup(route.Name))
            .Where(group => group.Arrivals.Count > 0)
            .ToList();

        return Task.FromResult<IReadOnlyList<IBusArrivalService.RouteArrivals>>(arrivals);
    }

    private IBusArrivalService.RouteArrivals CreateGroup(string routeName)
    {
        var arrivalCount = _random.Next(1, 4);
        var arrivals = new List<IBusArrivalService.RouteArrivals.Arrival>(arrivalCount);

        for (var index = 0; index < arrivalCount; index++)
        {
            arrivals.Add(
                new IBusArrivalService.RouteArrivals.Arrival(
                    $"SH{_random.Next(100, 999)}",
                    TimeSpan.FromMinutes(_random.NextDouble() * 15)
                )
            );
        }

        return new IBusArrivalService.RouteArrivals(routeName, arrivals);
    }
}
