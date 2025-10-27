namespace NobUS.Frontend.Avalonia.Views;

public partial class MainView : ReactiveUserControl<ViewModels.MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
