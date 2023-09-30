using NobUS.DataContract.Model;

namespace NobUS.Frontend.MAUI.Service
{
    internal class LocationProvider : ILocationProvider
    {
        private readonly Task<Location> _locationTask = Geolocation.Default.GetLocationAsync(
            new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(30))
        );

        private Coordinate _location;

        public async Task<Coordinate> GetLocationAsync() =>
            _location ??= await _locationTask.ContinueWith(
                t => new Coordinate(t.Result.Longitude, t.Result.Latitude)
            );
    }
}