using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Plugin.Maui.SquircleContainer.Benchmarks;

/// <summary>
/// End-to-end benchmark: measures the full rendering pipeline cost of
/// SquircleContainer vs Border. Combines path/geometry creation and layout
/// into a single operation so the overall cost can be compared directly.
/// </summary>
[MemoryDiagnoser]
public class OverallBenchmarks
{
    private RectF _bounds;
    private SquircleContainer _squircle = null!;
    private Border _border = null!;

    [Params(16, 32)]
    public double CornerRadius { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _bounds = new RectF(0, 0, 300, 200);

        _squircle = new SquircleContainer
        {
            CornerRadius = new CornerRadius(CornerRadius),
            CornerSmoothing = 0.6,
            Fill = new SolidColorBrush(Colors.Blue),
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2,
            Content = new Label { Text = "Content" }
        };

        _border = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(CornerRadius) },
            Background = new SolidColorBrush(Colors.Blue),
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2,
            Content = new Label { Text = "Content" }
        };
    }

    /// <summary>
    /// Squircle full pipeline: build path + convert to clip geometry + measure layout.
    /// This is the work SquircleContainer does on every render pass.
    /// </summary>
    [Benchmark(Description = "SquircleContainer full pipeline")]
    public (Geometry, Size) SquircleFullPipeline()
    {
        var path = SquirclePathBuilder.Build(_bounds, new CornerRadius(CornerRadius), 0.6);
        var geometry = PathGeometryConverter.ToGeometry(path);
        var size = _squircle.Measure(300, 200);
        return (geometry, size);
    }

    /// <summary>
    /// Border full pipeline: create RoundRectangleGeometry + measure layout.
    /// This is the equivalent work Border does.
    /// </summary>
    [Benchmark(Baseline = true, Description = "Border full pipeline")]
    public (Geometry, Size) BorderFullPipeline()
    {
        var geometry = new RoundRectangleGeometry(new CornerRadius(CornerRadius), new Rect(0, 0, 300, 200));
        var size = _border.Measure(300, 200);
        return (geometry, size);
    }

    /// <summary>
    /// SquircleContainer full lifecycle: create, configure, build path, clip geometry, measure.
    /// </summary>
    [Benchmark(Description = "SquircleContainer full lifecycle")]
    public (SquircleContainer, Geometry, Size) SquircleFullLifecycle()
    {
        var sc = new SquircleContainer
        {
            CornerRadius = new CornerRadius(CornerRadius),
            CornerSmoothing = 0.6,
            Fill = new SolidColorBrush(Colors.Blue),
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2,
            Content = new Label { Text = "Content" }
        };
        var path = SquirclePathBuilder.Build(_bounds, new CornerRadius(CornerRadius), 0.6);
        var geometry = PathGeometryConverter.ToGeometry(path);
        var size = sc.Measure(300, 200);
        return (sc, geometry, size);
    }

    /// <summary>
    /// Border full lifecycle: create, configure, create geometry, measure.
    /// </summary>
    [Benchmark(Description = "Border full lifecycle")]
    public (Border, Geometry, Size) BorderFullLifecycle()
    {
        var b = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(CornerRadius) },
            Background = new SolidColorBrush(Colors.Blue),
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 2,
            Content = new Label { Text = "Content" }
        };
        var geometry = new RoundRectangleGeometry(new CornerRadius(CornerRadius), new Rect(0, 0, 300, 200));
        var size = b.Measure(300, 200);
        return (b, geometry, size);
    }
}
