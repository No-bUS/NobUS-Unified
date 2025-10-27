using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Maui.Devices.Sensors;
using NobUS.DataContract.Model;
using ReactiveUI;

namespace NobUS.Frontend.MAUI.Service;

internal sealed class LocationProvider : ReactiveObject, ILocationProvider
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(30);

    private readonly BehaviorSubject<Coordinate?> _locationSubject = new(null);
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _backgroundTask;

    public LocationProvider()
    {
        _backgroundTask = Task.Run(UpdateLoop, _cancellationTokenSource.Token);
        _ = FetchInitialLocationAsync();
    }

    public Coordinate? Location { get; private set; }

    public IObservable<Coordinate> LocationChanges =>
        _locationSubject
            .Where(coordinate => coordinate is not null)
            .Select(coordinate => coordinate!);

    private async Task FetchInitialLocationAsync()
    {
        try
        {
            var location = await GetLocationAsync();
            UpdateLocation(location);
        }
        catch (Exception)
        {
            // Intentionally swallow - geolocation may not be permitted yet.
        }
    }

    private async Task UpdateLoop()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(RefreshInterval, _cancellationTokenSource.Token);
                var location = await GetLocationAsync();
                UpdateLocation(location);
            }
            catch (TaskCanceledException)
            {
                return;
            }
            catch (Exception)
            {
                // Ignore intermittent failures and retry on the next loop.
            }
        }
    }

    private static Task<Location?> GetLocationAsync() =>
        Geolocation.Default.GetLocationAsync(
            new GeolocationRequest(GeolocationAccuracy.High, RefreshInterval)
        );

    private void UpdateLocation(Location? location)
    {
        if (location is null)
        {
            return;
        }

        var coordinate = new Coordinate(location.Longitude, location.Latitude);
        Location = coordinate;
        _locationSubject.OnNext(coordinate);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _backgroundTask.Dispose();
        _locationSubject.Dispose();
    }
}
