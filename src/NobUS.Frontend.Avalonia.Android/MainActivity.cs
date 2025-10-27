using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using NobUS.Frontend.Avalonia.Extensions;

namespace NobUS.Frontend.Avalonia.Android;

[Activity(
    Label = "NobUS.Avalonia",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/appicon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode
)]
public sealed class MainActivity : AvaloniaMainActivity<NobUS.Frontend.Avalonia.App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) =>
        base.CustomizeAppBuilder(builder).UseNobUSDefaults();
}
