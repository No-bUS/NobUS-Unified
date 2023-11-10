using NobUS.DataContract.Model;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace NobUS.Infrastructure
{
    public class ArrivalEventListener : IDisposable
    {
        public class ArrivalEventGroup
        {
            public required string RouteName { get; set; }
            public required ObservableCollection<ArrivalEvent> Events { get; set; }
        }

        public record StationData(
            List<ArrivalEventGroup> EventGroups,
            CancellationTokenSource Cts,
            Dictionary<int, ArrivalEvent> Lookup,
            List<WeakReference> Subscribers
        );

        private readonly Func<Station, Task<IImmutableList<ArrivalEvent>>> _source;
        private readonly Dictionary<string, StationData> _dataDict = new();

        public ArrivalEventListener(Func<Station, Task<IImmutableList<ArrivalEvent>>> source)
        {
            _source = source;
        }

        public List<ArrivalEventGroup> this[Station station, object subscriber]
        {
            get
            {
                var queryName = station.QueryName();
                if (!_dataDict.TryGetValue(queryName, out var stationData))
                {
                    var events = Task.Run(async () => await _source(station)).Result;
                    var groupedEvents = events
                        .Where(e => e.TimeToWait >= TimeSpan.Zero)
                        .GroupBy(e => e.RouteName)
                        .Select(g => new ArrivalEventGroup() { RouteName = g.Key, Events = new(g) })
                        .ToList();

                    stationData = new StationData(
                        groupedEvents,
                        new(),
                        events.ToDictionary(e => e.ShuttleJobId),
                        new()
                    );
                    _dataDict[queryName] = stationData;
                    StartBackgroundTask(station, queryName, stationData.Cts.Token);
                }
                stationData.Subscribers.Add(new WeakReference(subscriber));
                return stationData.EventGroups;
            }
        }

        public void Cancel(Station station, object subscriber)
        {
            var queryName = station.QueryName();
            if (_dataDict.TryGetValue(queryName, out var stationData))
            {
                stationData.Subscribers.RemoveAll(wr => !wr.IsAlive || wr.Target == subscriber);
                CleanUpIfNoSubscribers(queryName);
            }
        }

        private void StartBackgroundTask(Station station, string queryName, CancellationToken token)
        {
            Task.Run(
                async () =>
                {
                    while (!token.IsCancellationRequested && _dataDict.ContainsKey(queryName))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), token);
                        var events = await _source(station);
                        var groupedEvents = events
                            .Where(e => e.TimeToWait >= TimeSpan.Zero)
                            .GroupBy(e => e.RouteName)
                            .ToList();

                        foreach (var group in groupedEvents)
                        {
                            var groupItem = _dataDict[queryName].EventGroups.FirstOrDefault(
                                eg => eg.RouteName == group.Key
                            );
                            if (groupItem == null)
                            {
                                groupItem = new ArrivalEventGroup()
                                {
                                    RouteName = group.Key,
                                    Events = new(group)
                                };
                                _dataDict[queryName].EventGroups.Add(groupItem);
                            }

                            foreach (var e in group)
                            {
                                if (
                                    !_dataDict[queryName].Lookup.TryGetValue(
                                        e.ShuttleJobId,
                                        out var existingEvent
                                    )
                                )
                                {
                                    groupItem.Events.Add(e);
                                    _dataDict[queryName].Lookup[e.ShuttleJobId] = e;
                                }
                                else
                                {
                                    if (!existingEvent.Equals(e))
                                    {
                                        // Update the existing item in lookup and in the list for UI updates
                                        int index = groupItem.Events.IndexOf(existingEvent);
                                        if (index != -1)
                                        {
                                            groupItem.Events[index] = e;
                                        }
                                        _dataDict[queryName].Lookup[e.ShuttleJobId] = e;
                                    }
                                    if (e.TimeToWait < TimeSpan.Zero)
                                    {
                                        groupItem.Events.Remove(e);
                                        _dataDict[queryName].Lookup.Remove(e.ShuttleJobId);
                                    }
                                }
                            }
                        }

                        CleanUpIfNoSubscribers(queryName);
                    }
                },
                token
            );
        }

        private void CleanUpIfNoSubscribers(string queryName)
        {
            if (_dataDict.TryGetValue(queryName, out var stationData))
            {
                // Remove dead weak references
                stationData.Subscribers.RemoveAll(wr => !wr.IsAlive);

                // If no live subscribers
                if (!stationData.Subscribers.Any(wr => wr.IsAlive))
                {
                    stationData.Cts.Cancel();
                    _dataDict.Remove(queryName);
                }
            }
        }

        public void Dispose()
        {
            foreach (var stationData in _dataDict.Values)
            {
                stationData.Cts.Cancel();
            }
        }
    }
}
