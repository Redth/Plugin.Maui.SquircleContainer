using Microsoft.Maui.Controls;

namespace Plugin.Maui.SquircleContainer.Sample;

public class App : Application
{
    public App()
    {
        MainPage = new NavigationPage(new MainPage());
    }
}
