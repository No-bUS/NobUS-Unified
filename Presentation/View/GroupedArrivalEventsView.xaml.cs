using NobUS.Frontend.MAUI.Presentation.ViewModel;

namespace NobUS.Frontend.MAUI.Presentation.View
{
    public partial class GroupedArrivalEventsView : ContentView
    {
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(
            nameof(ViewModel),
            typeof(GroupedArrivalEventsViewModel),
            typeof(GroupedArrivalEventsView),
            null,
            BindingMode.OneTime
        );

        public GroupedArrivalEventsView()
        {
            InitializeComponent();
        }

        public GroupedArrivalEventsViewModel ViewModel
        {
            get => (GroupedArrivalEventsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
    }
}
