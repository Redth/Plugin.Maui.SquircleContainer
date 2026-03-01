using Foundation;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform.MacOS.Hosting;

namespace Plugin.Maui.SquircleContainer.Sample;

[Register("MauiMacOSApp")]
public class MauiMacOSApp : MacOSMauiApplication
{
    protected override MauiApp CreateMauiApp() => MacOSMauiProgram.CreateMauiApp();
}
