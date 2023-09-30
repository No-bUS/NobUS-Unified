using NobUS.DataContract.Model;

namespace NobUS.Frontend.MAUI.Service
{
    internal interface ILocationProvider
    {
        Task<Coordinate> GetLocationAsync();
    }
}
