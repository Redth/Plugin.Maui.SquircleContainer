using System.Text;

namespace Plugin.Maui.SquircleContainer.Benchmarks;

public class BenchmarkPage : ContentPage
{
    private readonly Label _statusLabel;
    private readonly Editor _resultsEditor;
    private readonly Button _runButton;
    private readonly ActivityIndicator _spinner;

    public BenchmarkPage()
    {
        Title = "Squircle Benchmarks";

        _statusLabel = new Label
        {
            Text = "Ready to run benchmarks",
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10)
        };

        _spinner = new ActivityIndicator
        {
            IsRunning = false,
            IsVisible = false,
            HorizontalOptions = LayoutOptions.Center,
            Color = Colors.Blue
        };

        _runButton = new Button
        {
            Text = "Run Benchmarks",
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10),
            BackgroundColor = Colors.DodgerBlue,
            TextColor = Colors.White,
            CornerRadius = 8,
            Padding = new Thickness(20, 10)
        };
        _runButton.Clicked += OnRunBenchmarks;

        _resultsEditor = new Editor
        {
            IsReadOnly = true,
            FontFamily = "Courier New",
            FontSize = 11,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(10),
            Placeholder = "Benchmark results will appear here..."
        };

        Content = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            },
            Children =
            {
                _statusLabel,
                _spinner,
                _runButton,
                _resultsEditor
            }
        };

        Grid.SetRow(_statusLabel, 0);
        Grid.SetRow(_spinner, 1);
        Grid.SetRow(_runButton, 2);
        Grid.SetRow(_resultsEditor, 3);
    }

    private async void OnRunBenchmarks(object? sender, EventArgs e)
    {
        _runButton.IsEnabled = false;
        _spinner.IsRunning = true;
        _spinner.IsVisible = true;
        _statusLabel.Text = "Running benchmarks... this may take a few minutes.";
        _resultsEditor.Text = "";

        try
        {
            var results = await Task.Run(() =>
            {
                var output = new StringBuilder();
                var originalOut = Console.Out;
                var stringWriter = new StringWriter(output);
                Console.SetOut(stringWriter);

                try
                {
                    var formatted = BenchmarkRunner.RunAll();
                    return (output.ToString(), formatted);
                }
                finally
                {
                    Console.SetOut(originalOut);
                }
            });

            _resultsEditor.Text = results.Item2 + "\n\n--- Raw Output ---\n\n" + results.Item1;
            _statusLabel.Text = "Benchmarks complete!";
        }
        catch (Exception ex)
        {
            _resultsEditor.Text = $"Error: {ex.Message}\n\n{ex.StackTrace}";
            _statusLabel.Text = "Benchmark failed!";
        }
        finally
        {
            _spinner.IsRunning = false;
            _spinner.IsVisible = false;
            _runButton.IsEnabled = true;
        }
    }
}
