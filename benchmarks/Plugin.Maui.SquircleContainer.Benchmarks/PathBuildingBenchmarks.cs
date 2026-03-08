using BenchmarkDotNet.Attributes;
using Microsoft.Maui.Graphics;

namespace Plugin.Maui.SquircleContainer.Benchmarks;

/// <summary>
/// Compares squircle path building against RoundRectangle geometry creation
/// (the equivalent operation for MAUI Border's rounded corners).
/// </summary>
[MemoryDiagnoser]
public class PathBuildingBenchmarks
{
    private RectF _bounds;

    [GlobalSetup]
    public void Setup()
    {
        _bounds = new RectF(0, 0, 300, 200);
    }

    [Params(0, 16, 32)]
    public double CornerRadius { get; set; }

    [Benchmark(Description = "SquirclePathBuilder.Build (0.6 smoothing)")]
    public PathF SquirclePath_DefaultSmoothing()
    {
        return SquirclePathBuilder.Build(_bounds, new CornerRadius(CornerRadius), 0.6);
    }

    [Benchmark(Description = "SquirclePathBuilder.Build (0.0 smoothing = round)")]
    public PathF SquirclePath_NoSmoothing()
    {
        return SquirclePathBuilder.Build(_bounds, new CornerRadius(CornerRadius), 0.0);
    }

    [Benchmark(Description = "SquirclePathBuilder.Build (1.0 smoothing = full)")]
    public PathF SquirclePath_FullSmoothing()
    {
        return SquirclePathBuilder.Build(_bounds, new CornerRadius(CornerRadius), 1.0);
    }

    [Benchmark(Baseline = true, Description = "RoundRectangle path (Border equivalent)")]
    public PathF RoundRectanglePath()
    {
        var path = new PathF();
        var cr = (float)CornerRadius;
        path.AppendRoundedRectangle(_bounds, cr);
        return path;
    }

    [Benchmark(Description = "SquirclePathBuilder.Build (per-corner radii)")]
    public PathF SquirclePath_PerCornerRadii()
    {
        return SquirclePathBuilder.Build(
            _bounds,
            new CornerRadius(CornerRadius, CornerRadius / 2, CornerRadius, CornerRadius / 2),
            0.6);
    }
}
