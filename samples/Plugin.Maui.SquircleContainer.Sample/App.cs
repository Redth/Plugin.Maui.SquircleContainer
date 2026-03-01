using Microsoft.Maui.Controls;

namespace Plugin.Maui.SquircleContainer.Sample;

public class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
#if MACOS
        return new Window(new SimplePage());
#else
        return new Window(new MainPage());
#endif
    }
}

/// <summary>
/// Simple page for testing on platforms where SquircleContainer may not be fully supported yet.
/// </summary>
public class SimplePage : ContentPage
{
    public SimplePage()
    {
        Title = "Squircle Sample";
        Content = new VerticalStackLayout
        {
            Spacing = 20,
            Padding = 20,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "Squircle Container - macOS",
                    FontSize = 24,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Label
                {
                    Text = "Hello from macOS!",
                    HorizontalTextAlignment = TextAlignment.Center
                },
                new Plugin.Maui.SquircleContainer.SquircleContainer
                {
                    CornerRadius = 30,
                    CornerSmoothing = 0.6,
                    HeightRequest = 120,
                    WidthRequest = 280,
                    Fill = new SolidColorBrush(Color.FromArgb("#6366F1")),
                    HorizontalOptions = LayoutOptions.Center,
                    Content = new Label
                    {
                        Text = "Hello Squircle!",
                        TextColor = Colors.White,
                        FontSize = 20,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                },
            }
        };
    }
}
