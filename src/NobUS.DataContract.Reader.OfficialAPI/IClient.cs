using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using NobUS.DataContract.Model;

namespace NobUS.DataContract.Reader.OfficialAPI;

public interface IClient
{
    Task<IImmutableList<Station>> GetStationsAsync();
    Task<IImmutableList<Route>> GetRoutesAsync();
    Task<IImmutableList<Vehicle>> GetVehiclesAsync();
    Task<IImmutableList<ShuttleJob>> GetShuttleJobsAsync();
    Task<IImmutableList<RouteStation>> GetStationsAsync(Route route);
    Task<IImmutableList<ArrivalEvent>> GetArrivalEventsAsync(Station station);

    async Task<IImmutableList<TResult>> GetAsync<TResult>()
        where TResult : class =>
        Type.GetTypeCode(typeof(TResult)) switch
        {
            TypeCode.Object => typeof(TResult) switch
            {
                { } t when t == typeof(Station) => (IImmutableList<TResult>)
                    await GetStationsAsync(),
                { } t when t == typeof(Route) => (IImmutableList<TResult>)await GetRoutesAsync(),
                { } t when t == typeof(Vehicle) => (IImmutableList<TResult>)
                    await GetVehiclesAsync(),
                { } t when t == typeof(ShuttleJob) => (IImmutableList<TResult>)
                    await GetShuttleJobsAsync(),
                _ => ImmutableList<TResult>.Empty,
            },
            _ => ImmutableList<TResult>.Empty,
        };

    async Task<IImmutableList<TResult>> GetAsync<TResult, TQuery>(TQuery query)
        where TResult : class
        where TQuery : class =>
        Type.GetTypeCode(typeof(TResult)) switch
        {
            TypeCode.Object => typeof(TResult) switch
            {
                { } t when t == typeof(ArrivalEvent) && query is Station station =>
                    (IImmutableList<TResult>)await GetArrivalEventsAsync(station),
                { } t when t == typeof(Station) && query is Route route => (IImmutableList<TResult>)
                    await GetStationsAsync(route),
                _ => ImmutableList<TResult>.Empty,
            },
            _ => ImmutableList<TResult>.Empty,
        };
}
