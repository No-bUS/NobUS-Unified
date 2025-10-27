using Microsoft.Maui.Controls;
using NobUS.Frontend.MAUI.Views;

namespace NobUS.Frontend.MAUI;

public partial class AppShell : Shell
{
    public AppShell(StationsPage stationsPage, SportsPage sportsPage)
    {
        InitializeComponent();

        StationsShellContent.Content = stationsPage;
        SportsShellContent.Content = sportsPage;
    }
}
