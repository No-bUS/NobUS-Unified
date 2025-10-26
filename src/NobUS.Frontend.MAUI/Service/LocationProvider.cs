using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Devices.Sensors;
using NobUS.DataContract.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NobUS.Frontend.MAUI.Service;

internal sealed class LocationProvider : ReactiveObject, ILocationProvider
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(30);
    private readonly IGeolocation _geolocation;
    private readonly PeriodicTimer _timer = new(RefreshInterval);
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _backgroundTask;

    [Reactive]
    public Coordinate? Location { get; private set; }

    public LocationProvider()
        : this(Geolocation.Default) { }

    public LocationProvider(IGeolocation geolocation)
    {
        _geolocation = geolocation;
        _backgroundTask = RunAsync(_cancellationTokenSource.Token);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            await UpdateLocationAsync(cancellationToken).ConfigureAwait(false);
            while (await _timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                await UpdateLocationAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task UpdateLocationAsync(CancellationToken cancellationToken)
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.High, RefreshInterval);
            var location = await _geolocation
                .GetLocationAsync(request, cancellationToken)
                .ConfigureAwait(false);
            if (location is not null)
            {
                Location = new Coordinate(location.Longitude, location.Latitude);
            }
        }
        catch (FeatureNotEnabledException)
        {
            Location = null;
        }
        catch (PermissionException)
        {
            Location = null;
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _timer.Dispose();
        try
        {
            _backgroundTask.Wait();
        }
        catch (AggregateException ex)
            when (ex.InnerExceptions.All(e => e is OperationCanceledException)) { }
        _cancellationTokenSource.Dispose();
    }
}
