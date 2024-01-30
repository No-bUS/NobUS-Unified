using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using NobUS.DataContract.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace NobUS.Frontend.MAUI.Service
{
    internal class LocationProvider : ReactiveObject, ILocationProvider
    {
        [Reactive]
        public Coordinate Location { get; private set; }
        private readonly Task _backgroundTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly CancellationToken _cancellationToken;

        public LocationProvider()
        {
            _cancellationToken = _cancellationTokenSource.Token;
            _backgroundTask = BackgroundTask();
            LocationTask.ContinueWith(t =>
                Location = new Coordinate(t.Result.Longitude, t.Result.Latitude)
            );
        }

        private static Task<Location> LocationTask =>
            Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(30))
            );

        private Task BackgroundTask() =>
            Task.Run(
                async () =>
                {
                    while (!_cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30));
                        var location = await LocationTask;
                        Location = new Coordinate(location.Longitude, location.Latitude);
                    }
                },
                _cancellationToken
            );

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _backgroundTask.Dispose();
        }
    }
}
