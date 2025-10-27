namespace NobUS.Frontend.Avalonia.ViewModels;

public sealed class StationViewModel(Station station) : ReactiveObject
{
    public Station Station { get; } = station;

    public string DisplayName => Station.Name;

    public DateTimeOffset? LastUpdated
    {
        get;
        internal set
        {
            if (value == field)
            {
                return;
            }

            field = value;
            this.RaisePropertyChanged();
        }
    }
}
