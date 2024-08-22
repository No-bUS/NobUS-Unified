using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NobUS.DataContract.Model;
using static NobUS.Infrastructure.DefinitionLoader;

namespace NobUS.Infrastructure;

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
        ConcurrentDictionary<int, ArrivalEvent> Lookup,
        List<WeakReference> Subscribers
    );

    private readonly Func<Station, Task<IImmutableList<ArrivalEvent>>> _source;
    private readonly ConcurrentDictionary<string, StationData> _dataDict = new();
    private readonly ConcurrentDictionary<string, Task> _taskDict = new();

    public ArrivalEventListener(Func<Station, Task<IImmutableList<ArrivalEvent>>> source)
    {
        _source = source;
    }

    public List<ArrivalEventGroup> GetArrivalEventGroups(Station station, object subscriber)
    {
        var queryName = station.QueryName();
        if (!_dataDict.TryGetValue(queryName, out var stationData))
        {
            stationData = new StationData(
                Routes
                    .Values.Where(r => r.ToStations.Select(s => s.Id).Contains(station.Code))
                    .Select(r => new ArrivalEventGroup { RouteName = r.Name, Events = new() })
                    .ToList(),
                new(),
                new(),
                new()
            );
            _dataDict[queryName] = stationData;
            _taskDict[queryName] = StartBackgroundTask(station, queryName, stationData.Cts.Token);
        }
        stationData.Subscribers.Add(new WeakReference(subscriber));
        return stationData.EventGroups;
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

    private Task StartBackgroundTask(Station station, string queryName, CancellationToken token) =>
        Task.Run(
            async () =>
            {
                while (!token.IsCancellationRequested && _dataDict.ContainsKey(queryName))
                {
                    var events = await _source(station);
                    var groupedEvents = events
                        .Where(e => e.TimeToWait >= TimeSpan.Zero)
                        .GroupBy(e => e.RouteName)
                        .ToList();

                    foreach (var group in groupedEvents)
                    {
                        var groupItem = _dataDict[queryName]
                            .EventGroups.FirstOrDefault(eg => eg.RouteName == group.Key);
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
                                !_dataDict[queryName]
                                    .Lookup.TryGetValue(e.ShuttleJobId, out var existingEvent)
                            )
                            {
                                groupItem.Events.Add(e);
                                _dataDict[queryName].Lookup.TryAdd(e.ShuttleJobId, e);
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
                                    _dataDict[queryName]
                                        .Lookup.TryUpdate(e.ShuttleJobId, e, existingEvent);
                                }
                                if (e.TimeToWait < TimeSpan.Zero)
                                {
                                    groupItem.Events.Remove(e);
                                    _dataDict[queryName]
                                        .Lookup.TryRemove(e.ShuttleJobId, out var _);
                                }
                            }
                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                    CleanUpIfNoSubscribers(queryName);
                }
            },
            token
        );

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
                _dataDict.Remove(queryName, out var _);
            }
        }

        if (_taskDict.TryGetValue(queryName, out var task))
        {
            if (task.IsCompleted)
            {
                task.Dispose();
            }
            _taskDict.Remove(queryName, out var _);
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
