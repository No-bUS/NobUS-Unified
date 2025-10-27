using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using NobUS.Extra.Campus.Facility.Sports;
using ReactiveUI;
using FacilityType = NobUS.Extra.Campus.Facility.Sports.Type;

namespace NobUS.Frontend.MAUI.ViewModels;

public sealed partial class SportsPageViewModel : ObservableObject
{
    private readonly ObservableCollection<FacilityGroup> _facilityGroups = new();

    public SportsPageViewModel()
    {
        FacilityGroups = new ReadOnlyObservableCollection<FacilityGroup>(_facilityGroups);
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadAsync);
        RefreshCommand.IsExecuting.Subscribe(isExecuting => IsRefreshing = isExecuting);
        RefreshCommand.Execute().Subscribe();
    }

    public ReadOnlyObservableCollection<FacilityGroup> FacilityGroups { get; }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    [ObservableProperty]
    private bool isRefreshing;

    private async Task LoadAsync()
    {
        try
        {
            var facilities = await Parser.GetAllAsync();
            var grouped = facilities
                .GroupBy(f => f.Type)
                .Select(g => new FacilityGroup(g.Key, new ObservableCollection<Facility>(g)))
                .OrderBy(g => g.Type)
                .ToList();

            _facilityGroups.Clear();
            foreach (var group in grouped)
            {
                _facilityGroups.Add(group);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load facilities: {ex.Message}");
        }
    }

    public sealed record FacilityGroup(FacilityType Type, ObservableCollection<Facility> Facilities);
}
