using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NobUS.DataContract.Model;
using static NobUS.Infrastructure.DefinitionLoader;

namespace NobUS.Infrastructure;

public sealed class ArrivalEventListener : IDisposable
{
    public record ArrivalEventGroup(string RouteName, IReadOnlyList<ArrivalEvent> Events);

    private readonly Func<Station, Task<IImmutableList<ArrivalEvent>>> _source;
    private readonly ConcurrentDictionary<string, IObservable<IReadOnlyList<ArrivalEventGroup>>> _streams = new();

    public ArrivalEventListener(Func<Station, Task<IImmutableList<ArrivalEvent>>> source)
    {
        _source = source;
    }

    public IObservable<IReadOnlyList<ArrivalEventGroup>> ObserveArrivalEvents(Station station)
    {
        var queryName = station.QueryName();
        return _streams.GetOrAdd(queryName, _ => CreateStream(station));
    }

    private IObservable<IReadOnlyList<ArrivalEventGroup>> CreateStream(Station station) =>
        Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(10))
            .SelectMany(_ => Observable.FromAsync(() => _source(station)))
            .Select(events =>
                events
                    .Where(e => e.TimeToWait >= TimeSpan.Zero)
                    .GroupBy(e => e.RouteName)
                    .Select(group =>
                        new ArrivalEventGroup(
                            group.Key,
                            group.OrderBy(e => e.TimeToWait).ToList()
                        )
                    )
                    .OrderBy(group => group.RouteName)
                    .ToList()
                    .AsReadOnly()
            )
            .Replay(1)
            .RefCount();

    public void Dispose()
    {
        _streams.Clear();
    }
}
