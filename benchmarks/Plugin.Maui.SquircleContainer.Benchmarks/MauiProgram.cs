using Microsoft.Maui.Hosting;
using MauiDevFlow.Agent;

namespace Plugin.Maui.SquircleContainer.Benchmarks;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

#if DEBUG
        builder.AddMauiDevFlowAgent();
#endif

        return builder.Build();
    }
}
