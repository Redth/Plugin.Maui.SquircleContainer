using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Plugin.Maui.SquircleContainer.Benchmarks;

/// <summary>
/// Benchmarks layout (Measure/Arrange) performance of SquircleContainer vs Border.
/// Uses the cross-platform Measure method to test layout computation cost.
/// </summary>
[MemoryDiagnoser]
public class LayoutBenchmarks
{
    private SquircleContainer _squircle = null!;
    private Border _border = null!;

    [GlobalSetup]
    public void Setup()
    {
        _squircle = new SquircleContainer
        {
            CornerRadius = new CornerRadius(16),
            CornerSmoothing = 0.6,
            Fill = new SolidColorBrush(Colors.Blue),
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2,
            Content = new Label { Text = "Benchmark content" }
        };

        _border = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
            Background = new SolidColorBrush(Colors.Blue),
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2,
            Content = new Label { Text = "Benchmark content" }
        };
    }

    [Benchmark(Description = "SquircleContainer.Measure")]
    public Size MeasureSquircle()
    {
        return _squircle.Measure(300, 200);
    }

    [Benchmark(Baseline = true, Description = "Border.Measure")]
    public Size MeasureBorder()
    {
        return _border.Measure(300, 200);
    }

    [Benchmark(Description = "SquircleContainer create + configure")]
    public SquircleContainer CreateSquircle()
    {
        return new SquircleContainer
        {
            CornerRadius = new CornerRadius(16),
            CornerSmoothing = 0.6,
            Fill = new SolidColorBrush(Colors.Blue),
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2,
            Content = new Label { Text = "Content" }
        };
    }

    [Benchmark(Description = "Border create + configure")]
    public Border CreateBorder()
    {
        return new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
            Background = new SolidColorBrush(Colors.Blue),
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2,
            Content = new Label { Text = "Content" }
        };
    }
}
