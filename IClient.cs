using System.Collections.Immutable;
using NobUS.DataContract.Model.Entity;
using NobUS.DataContract.Model.ValueObject.Snapshot;

namespace NobUS.DataContract.Reader.OfficialAPI
{
    public interface IClient
    {
        Task<IImmutableList<Station>> GetStationsAsync();
        Task<IImmutableList<Route>> GetRoutesAsync();
        Task<IImmutableList<Vehicle>> GetVehiclesAsync();
        Task<IImmutableList<ShuttleJob>> GetShuttleJobsAsync();
        Task<IImmutableList<Station>> GetStationsAsync(Route route);
        Task<IImmutableList<ArrivalEvent>> GetArrivalEventsAsync(Station station);
        Task<IImmutableList<TResult>> GetAsync<TResult>() where TResult : class;
        Task<IImmutableList<TResult>> GetAsync<TResult, TQuery>(TQuery query)
            where TResult : class where TQuery : class;
    }
}