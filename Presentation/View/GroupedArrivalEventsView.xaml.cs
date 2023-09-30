using NobUS.Frontend.MAUI.Presentation.ViewModel;

namespace NobUS.Frontend.MAUI.Presentation.View;

public partial class GroupedArrivalEventsView : ContentView
{
    public static readonly BindableProperty GroupedArrivalEventsProperty = BindableProperty.Create(nameof(GroupedArrivalEvents), typeof(GroupedArrivalEventsViewModel), typeof(GroupedArrivalEventsView));

    public GroupedArrivalEventsViewModel GroupedArrivalEvents
    {
        get => (GroupedArrivalEventsViewModel)GetValue(GroupedArrivalEventsProperty);
        set => SetValue(GroupedArrivalEventsProperty, value);
    }


    public GroupedArrivalEventsView()
    {
        InitializeComponent();
        while (GroupedArrivalEvents == null) { };

        foreach (var arrival in GroupedArrivalEvents.ArrivalEvents)
        {
            ArrivalEventsPanel.Children.Add(new Label
            {
                Text = arrival.EstimatedArrivalSpan.ToString(),
            });
        }
    }
}