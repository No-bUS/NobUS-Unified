namespace NobUS.Frontend.Avalonia.ViewModels;

public sealed class MainViewModel : ReactiveObject
{
    private readonly Services.IBusArrivalService _arrivalService;
    private readonly RouteArrivalGroupViewModel.Factory _groupFactory;
    private StationViewModel? _selectedStation;

    public MainViewModel(
        Services.IBusArrivalService arrivalService,
        RouteArrivalGroupViewModel.Factory groupFactory
    )
    {
        _arrivalService = arrivalService;
        _groupFactory = groupFactory;

        StatusMessage = "Pick a station to view simulated arrivals.";

        Stations = new ObservableCollection<StationViewModel>(
            arrivalService
                .GetStations()
                .Select(station => new StationViewModel(station))
                .OrderBy(vm => vm.DisplayName)
        );

        Refresh = ReactiveCommand.CreateFromTask(ExecuteRefreshAsync);
        Refresh.IsExecuting.Subscribe(isBusy => IsBusy = isBusy);
        Refresh.ThrownExceptions.Subscribe(ex => StatusMessage = ex.Message);

        this.WhenAnyValue(vm => vm.SelectedStation)
            .WhereNotNull()
            .InvokeCommand(Refresh);

        if (Stations.Count > 0)
        {
            SelectedStation = Stations[0];
        }
    }

    public ObservableCollection<StationViewModel> Stations { get; }

    public ObservableCollection<RouteArrivalGroupViewModel> ArrivalGroups { get; } = new();

    public ReactiveCommand<Unit, Unit> Refresh { get; }

    public StationViewModel? SelectedStation
    {
        get => _selectedStation;
        set => this.RaiseAndSetIfChanged(ref _selectedStation, value);
    }

    public bool IsBusy
    {
        get;
        private set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            this.RaisePropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => field ?? string.Empty;
        private set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            this.RaisePropertyChanged();
        }
    }

    private async Task ExecuteRefreshAsync()
    {
        if (SelectedStation is null)
        {
            return;
        }

        StatusMessage = $"Loading {SelectedStation.DisplayName} arrivals...";

        var groups = await _arrivalService
            .GetArrivalsAsync(SelectedStation.Station, CancellationToken.None)
            .ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(
            () =>
            {
                ArrivalGroups.Clear();

                foreach (var group in groups)
                {
                    var arrivalViewModels = group.Arrivals
                        .Select(arrival => new RouteArrivalGroupViewModel.ArrivalPredictionViewModel(
                            arrival.WaitTime,
                            arrival.VehiclePlate
                        ));

                    ArrivalGroups.Add(_groupFactory(group.RouteName, arrivalViewModels));
                }

                SelectedStation?.LastUpdated = DateTimeOffset.Now;
                StatusMessage = groups.Count == 0
                    ? "No shuttles scheduled in the demo feed."
                    : $"Showing {groups.Sum(g => g.Arrivals.Count)} arrival forecasts.";
            }
        );
    }
}
