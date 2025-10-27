using System.Collections.ObjectModel;
using System.Reactive;
using CommunityToolkit.Mvvm.ComponentModel;
using NobUS.DataContract.Model;
using NobUS.Infrastructure;
using ReactiveUI;

namespace NobUS.Frontend.MAUI.ViewModels;

public sealed partial class StationItemViewModel : ObservableObject, IDisposable
{
    private readonly ArrivalEventListener _arrivalEventListener;
    private readonly object _subscriptionKey;

    [ObservableProperty]
    private bool isExpanded;

    [ObservableProperty]
    private double distanceInKilometers;

    [ObservableProperty]
    private IReadOnlyList<ArrivalEventListener.ArrivalEventGroup>? arrivalGroups;

    public StationItemViewModel(Station station, ArrivalEventListener arrivalEventListener)
    {
        Station = station;
        _arrivalEventListener = arrivalEventListener;
        _subscriptionKey = new object();

        distanceInKilometers = double.PositiveInfinity;
        ToggleExpandedCommand = ReactiveCommand.Create(ToggleExpanded);
    }

    public Station Station { get; }

    public string StationTitle => $"{Station.Code} | {Station.Road}";

    public string DistanceDisplay => double.IsInfinity(DistanceInKilometers)
        ? string.Empty
        : $"{DistanceInKilometers * 1000:F2} m";

    public ReactiveCommand<Unit, Unit> ToggleExpandedCommand { get; }

    public void UpdateDistance(Coordinate? coordinate)
    {
        if (coordinate is null)
        {
            DistanceInKilometers = double.PositiveInfinity;
            return;
        }

        DistanceInKilometers = Station.Coordinate.DistanceTo(coordinate);
    }

    private void ToggleExpanded()
    {
        if (IsExpanded)
        {
            ArrivalGroups = null;
            IsExpanded = false;
            _arrivalEventListener.Cancel(Station, _subscriptionKey);
        }
        else
        {
            var groups = _arrivalEventListener.GetArrivalEventGroups(Station, _subscriptionKey);
            ArrivalGroups = new ReadOnlyCollection<ArrivalEventListener.ArrivalEventGroup>(groups);
            IsExpanded = true;
        }

        OnPropertyChanged(nameof(DistanceDisplay));
    }

    partial void OnDistanceInKilometersChanged(double value) =>
        OnPropertyChanged(nameof(DistanceDisplay));

    public void Dispose()
    {
        _arrivalEventListener.Cancel(Station, _subscriptionKey);
    }
}
