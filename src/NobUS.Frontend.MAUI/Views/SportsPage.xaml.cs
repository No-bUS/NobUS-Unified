using Microsoft.Maui.Controls;
using NobUS.Frontend.MAUI.ViewModels;

namespace NobUS.Frontend.MAUI.Views;

public partial class SportsPage : ContentPage
{
    public SportsPage(SportsPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
