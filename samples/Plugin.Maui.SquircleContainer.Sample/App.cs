using Microsoft.Maui.Controls;

namespace Plugin.Maui.SquircleContainer.Sample;

public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var tabbedPage = new TabbedPage
        {
            Title = "Squircle Container",
            Children =
            {
                new MainPage(),
                new ComplexLayoutsPage(),
                new PlaygroundPage(),
            }
        };

        return new Window(tabbedPage);
    }
}
