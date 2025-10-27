using System.Collections.ObjectModel;
using System.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using NobUS.DataContract.Model;
using NobUS.Frontend.MAUI.Service;
using NobUS.Infrastructure;
using ReactiveUI;

namespace NobUS.Frontend.MAUI.ViewModels;

public sealed partial class StationsPageViewModel : ObservableObject, IDisposable
{
    private readonly ILocationProvider _locationProvider;
    private readonly ArrivalEventListener _arrivalEventListener;
    private readonly ObservableCollection<StationItemViewModel> _stations;
    private readonly IDisposable _locationSubscription;

    public StationsPageViewModel(
        ILocationProvider locationProvider,
        ArrivalEventListener arrivalEventListener
    )
    {
        _locationProvider = locationProvider;
        _arrivalEventListener = arrivalEventListener;

        _stations = new ObservableCollection<StationItemViewModel>(
            DefinitionLoader.GetAllStations
                .Select(station => new StationItemViewModel(station, _arrivalEventListener))
        );
        Stations = new ReadOnlyObservableCollection<StationItemViewModel>(_stations);

        RefreshDistancesCommand = ReactiveCommand.Create(ResortStations);
        _locationSubscription = _locationProvider.LocationChanges.Subscribe(UpdateDistances);

        if (_locationProvider.Location is { } coordinate)
        {
            UpdateDistances(coordinate);
        }
    }

    public ReadOnlyObservableCollection<StationItemViewModel> Stations { get; }

    public ReactiveCommand<Unit, Unit> RefreshDistancesCommand { get; }

    private void UpdateDistances(Coordinate coordinate)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            foreach (var station in _stations)
            {
                station.UpdateDistance(coordinate);
            }

            ResortStations();
        });
    }

    private void ResortStations()
    {
        var ordered = _stations
            .OrderBy(s => s.DistanceInKilometers)
            .ToList();

        for (var index = 0; index < ordered.Count; index++)
        {
            var current = ordered[index];
            if (!ReferenceEquals(_stations.ElementAtOrDefault(index), current))
            {
                var existingIndex = _stations.IndexOf(current);
                if (existingIndex >= 0 && existingIndex != index)
                {
                    _stations.Move(existingIndex, index);
                }
            }
        }
    }

    public void Dispose()
    {
        _locationSubscription.Dispose();
        foreach (var station in _stations)
        {
            station.Dispose();
        }
    }
}
