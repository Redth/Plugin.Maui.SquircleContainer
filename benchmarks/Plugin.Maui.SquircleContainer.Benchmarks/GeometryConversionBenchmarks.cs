using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Plugin.Maui.SquircleContainer.Benchmarks;

/// <summary>
/// Benchmarks the cost of converting a PathF to a Geometry (used for content clipping).
/// Compares squircle geometry conversion against creating a RoundRectangleGeometry (Border equivalent).
/// </summary>
[MemoryDiagnoser]
public class GeometryConversionBenchmarks
{
    private PathF _squirclePath = null!;
    private PathF _roundRectPath = null!;

    [Params(0, 16, 32)]
    public double CornerRadius { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var bounds = new RectF(0, 0, 300, 200);
        _squirclePath = SquirclePathBuilder.Build(bounds, new CornerRadius(CornerRadius), 0.6);

        _roundRectPath = new PathF();
        _roundRectPath.AppendRoundedRectangle(bounds, (float)CornerRadius);
    }

    [Benchmark(Description = "Squircle PathF → Geometry")]
    public Microsoft.Maui.Controls.Shapes.Geometry SquircleToGeometry()
    {
        return PathGeometryConverter.ToGeometry(_squirclePath);
    }

    [Benchmark(Baseline = true, Description = "RoundRectangleGeometry (Border equivalent)")]
    public Microsoft.Maui.Controls.Shapes.Geometry RoundRectGeometry()
    {
        return new RoundRectangleGeometry(new CornerRadius(CornerRadius), new Rect(0, 0, 300, 200));
    }

    [Benchmark(Description = "RoundRect PathF → Geometry")]
    public Microsoft.Maui.Controls.Shapes.Geometry RoundRectPathToGeometry()
    {
        return PathGeometryConverter.ToGeometry(_roundRectPath);
    }
}
