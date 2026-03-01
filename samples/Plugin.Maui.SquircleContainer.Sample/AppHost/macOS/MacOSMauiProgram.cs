using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform.MacOS.Hosting;

namespace Plugin.Maui.SquircleContainer.Sample;

public static class MacOSMauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp
            .CreateBuilder()
            .UseMauiAppMacOS<App>();

        return builder.Build();
    }
}
