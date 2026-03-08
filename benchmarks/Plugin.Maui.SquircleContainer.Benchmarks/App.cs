namespace Plugin.Maui.SquircleContainer.Benchmarks;

public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new BenchmarkPage());
    }
}
