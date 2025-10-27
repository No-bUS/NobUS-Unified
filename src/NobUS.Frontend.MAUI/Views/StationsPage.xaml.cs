using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using NobUS.Frontend.MAUI.ViewModels;

namespace NobUS.Frontend.MAUI.Views;

public partial class StationsPage : ContentPage
{
    public StationsPage()
        : this(App.Services.GetRequiredService<StationsPageViewModel>())
    {
    }

    public StationsPage(StationsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
