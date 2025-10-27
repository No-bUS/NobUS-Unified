using System.Collections.Generic;
using System.Linq;
using NobUS.DataContract.Model;
using NobUS.Infrastructure;

namespace NobUS.Frontend.MAUI.Service;

internal sealed class StationViewStateStore
{
    private readonly object _sync = new();
    private readonly Dictionary<int, bool> _expandedStations = new();
    private readonly Dictionary<int, double> _distances = new();
    private IReadOnlyList<int> _orderedStationCodes = DefinitionLoader
        .GetAllStations.Select(station => station.Code)
        .ToArray();

    public IReadOnlyList<Station> OrderStations(IEnumerable<Station> stations)
    {
        var stationLookup = stations.ToDictionary(station => station.Code);
        lock (_sync)
        {
            var ordered = new List<Station>();
            foreach (var code in _orderedStationCodes)
            {
                if (stationLookup.Remove(code, out var station))
                {
                    ordered.Add(station);
                }
            }

            ordered.AddRange(stationLookup.Values);
            return ordered;
        }
    }

    public void UpdateOrdering(IEnumerable<Station> orderedStations)
    {
        lock (_sync)
        {
            _orderedStationCodes = orderedStations.Select(station => station.Code).ToArray();
        }
    }

    public bool IsExpanded(int stationCode)
    {
        lock (_sync)
        {
            return _expandedStations.TryGetValue(stationCode, out var expanded) && expanded;
        }
    }

    public void SetExpanded(int stationCode, bool expanded)
    {
        lock (_sync)
        {
            if (expanded)
            {
                _expandedStations[stationCode] = true;
            }
            else
            {
                _expandedStations.Remove(stationCode);
            }
        }
    }

    public double? GetDistance(int stationCode)
    {
        lock (_sync)
        {
            return _distances.TryGetValue(stationCode, out var distance) ? distance : null;
        }
    }

    public void SetDistances(IEnumerable<(int StationCode, double Distance)> distances)
    {
        lock (_sync)
        {
            foreach (var (code, distance) in distances)
            {
                _distances[code] = distance;
            }
        }
    }
}
