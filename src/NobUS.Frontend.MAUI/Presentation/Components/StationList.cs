using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Maui.Dispatching;
using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Service;
using ReactiveUI;
using static NobUS.Infrastructure.DefinitionLoader;

namespace NobUS.Frontend.MAUI.Presentation.Components;

internal partial class StationList : DisposableComponent
{
    private readonly ObservableCollection<Station> _stations = new(GetAllStations);
    private readonly Dictionary<int, double> _distanceLookup = new();
    private readonly IDispatcher _dispatcher = Dispatcher.GetForCurrentThread()!;

    [Inject]
    private ILocationProvider locationProvider = null!;

    [Inject]
    private StationViewStateStore stationViewStateStore = null!;

    public StationList Stations(IList<Station> stations)
    {
        var ordered = stationViewStateStore.OrderStations(stations);
        _stations.Clear();
        _stations.AddRange(ordered);
        stationViewStateStore.UpdateOrdering(ordered);
        return this;
    }

    public override VisualNode Render() =>
        new ListView()
            .ItemsSource(
                _stations,
                station => new ViewCell() { CreateCard(station).Invoke(RegisterResource) }
            )
            .SelectionMode(ListViewSelectionMode.None)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Never)
            .SeparatorVisibility(SeparatorVisibility.None)
            .HasUnevenRows(true)
            .BackgroundColor(Styler.Scheme.Surface);

    protected override void OnMounted()
    {
        base.OnMounted();
        ApplyStoredOrdering();
        SubscribeToLocationUpdates();
    }

    private StationCard CreateCard(Station station)
    {
        var card = new StationCard().Station(station);
        if (_distanceLookup.TryGetValue(station.Code, out var distance))
        {
            card = card.Distance(distance);
        }
        return card;
    }

    private void ApplyStoredOrdering()
    {
        var ordered = stationViewStateStore.OrderStations(_stations);
        if (!ordered.SequenceEqual(_stations))
        {
            _stations.Clear();
            _stations.AddRange(ordered);
        }

        foreach (var station in ordered)
        {
            if (stationViewStateStore.GetDistance(station.Code) is { } distance)
            {
                _distanceLookup[station.Code] = distance;
            }
        }
    }

    private void SubscribeToLocationUpdates()
    {
        locationProvider
            .WhenAnyValue(provider => provider.Location)
            .WhereNotNull()
            .Subscribe(location =>
            {
                _dispatcher.Dispatch(() =>
                {
                    var ordered = _stations
                        .OrderBy(station => station.Coordinate.DistanceTo(location))
                        .ToList();

                    foreach (var station in ordered)
                    {
                        var distance = station.Coordinate.DistanceTo(location);
                        _distanceLookup[station.Code] = distance;
                    }

                    stationViewStateStore.SetDistances(
                        ordered.Select(station => (station.Code, _distanceLookup[station.Code]))
                    );
                    stationViewStateStore.UpdateOrdering(ordered);

                    if (!ordered.SequenceEqual(_stations))
                    {
                        _stations.Clear();
                        _stations.AddRange(ordered);
                    }
                });
            })
            .Invoke(RegisterResource);
    }
}
