namespace Plugin.Maui.SquircleContainer.Sample;

public partial class PlaygroundPage : ContentPage
{
    public PlaygroundPage()
    {
        InitializeComponent();
    }

    void OnRadiusChanged(object? sender, ValueChangedEventArgs e)
    {
        var v = (int)e.NewValue;
        PreviewContainer.CornerRadius = v;
        RadiusLabel.Text = v.ToString();
    }

    void OnSmoothingChanged(object? sender, ValueChangedEventArgs e)
    {
        PreviewContainer.CornerSmoothing = e.NewValue;
        SmoothingLabel.Text = e.NewValue.ToString("F2");
    }

    void OnPaddingChanged(object? sender, ValueChangedEventArgs e)
    {
        var v = (int)e.NewValue;
        PreviewContainer.Padding = v;
        PaddingLabel.Text = v.ToString();
    }

    void OnMarginChanged(object? sender, ValueChangedEventArgs e)
    {
        var v = (int)e.NewValue;
        PreviewContainer.Margin = v;
        MarginLabel.Text = v.ToString();
    }

    void OnStrokeChanged(object? sender, ValueChangedEventArgs e)
    {
        var v = Math.Round(e.NewValue, 1);
        PreviewContainer.StrokeThickness = v;
        PreviewContainer.Stroke ??= new SolidColorBrush(Colors.White);
        StrokeLabel.Text = v.ToString("F1");
    }

    void OnWidthChanged(object? sender, ValueChangedEventArgs e)
    {
        var v = (int)e.NewValue;
        PreviewContainer.WidthRequest = v;
        WidthLabel.Text = v.ToString();
    }

    void OnHeightChanged(object? sender, ValueChangedEventArgs e)
    {
        var v = (int)e.NewValue;
        PreviewContainer.HeightRequest = v;
        HeightLabel.Text = v.ToString();
    }

    void OnFillColorClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn)
            PreviewContainer.Fill = new SolidColorBrush(Color.FromArgb(btn.ClassId));
    }

    void OnStrokeColorClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            PreviewContainer.Stroke = new SolidColorBrush(Color.FromArgb(btn.ClassId));
            if (PreviewContainer.StrokeThickness < 1)
            {
                PreviewContainer.StrokeThickness = 3;
                StrokeSlider.Value = 3;
            }
        }
    }
}
