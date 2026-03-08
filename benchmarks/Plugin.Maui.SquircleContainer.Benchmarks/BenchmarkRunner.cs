using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using BenchmarkDotNet.Validators;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Plugin.Maui.SquircleContainer.Benchmarks;

/// <summary>
/// No-op exporter that prevents BenchmarkDotNet from adding default exporters
/// which call Process.Start() to detect the dotnet SDK (unsupported on iOS).
/// </summary>
internal class MobileExporter : IExporter
{
    public static readonly MobileExporter Instance = new();
    public string Name => "Mobile";
    public void ExportToLog(Summary summary, ILogger logger) { }
    public IEnumerable<string> ExportToFiles(Summary summary, ILogger consoleLogger)
        => Enumerable.Empty<string>();
}

/// <summary>
/// Runs BenchmarkDotNet benchmarks in-process on Android,
/// falls back to a manual Stopwatch-based runner on iOS where
/// Process.Start() is not supported.
/// </summary>
public static class BenchmarkRunner
{
    public static IConfig CreateMobileConfig()
    {
        return ManualConfig.CreateEmpty()
            .AddJob(Job.ShortRun
                .WithToolchain(InProcessNoEmitToolchain.Instance)
                .WithStrategy(RunStrategy.Throughput)
                .WithWarmupCount(3)
                .WithIterationCount(10))
            .AddColumnProvider(DefaultColumnProviders.Instance)
            .AddExporter(MobileExporter.Instance)
            .AddLogger(NullLogger.Instance)
            .AddValidator(JitOptimizationsValidator.DontFailOnError)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator)
            .WithSummaryStyle(SummaryStyle.Default);
    }

    public static string RunAll()
    {
        // On iOS, Process.Start() throws PlatformNotSupportedException.
        // BenchmarkDotNet's PrintSummary calls HostEnvironmentInfo.IsDotNetCliInstalled()
        // which calls Process.Start(). Pre-populate the lazy property via reflection.
        try { PreventDotNetCliDetection(); } catch { }

        // Try BenchmarkDotNet first (works on Android, and iOS with the reflection fix).
        // If it fails, fall back to manual Stopwatch-based runner.
        try
        {
            var config = CreateMobileConfig();
            var summaries = new List<Summary>();

            foreach (var type in BenchmarkTypes)
            {
                var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run(type, config);
                if (summary != null)
                    summaries.Add(summary);
            }

            if (summaries.Count > 0)
                return FormatBdnResults(summaries);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BenchmarkDotNet failed ({ex.GetType().Name}), using manual runner.");
        }

        // Fallback: manual Stopwatch-based benchmarks
        return RunManual();
    }

    /// <summary>
    /// Replaces BenchmarkDotNet's lazy DotNetSdkVersion/IsMonoInstalled properties
    /// so they never call Process.Start() (throws on iOS).
    /// These are auto-properties with protected setters, so backing fields use the
    /// compiler-generated pattern: &lt;PropertyName&gt;k__BackingField.
    /// </summary>
    private static void PreventDotNetCliDetection()
    {
        var hostEnvType = typeof(BenchmarkDotNet.Environments.HostEnvironmentInfo);
        var currentProp = hostEnvType.GetProperty("Current",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var hostEnv = currentProp?.GetValue(null);
        if (hostEnv == null) return;

        // Replace ALL Lazy<string> and Lazy<bool> fields to prevent any Process.Start()
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        foreach (var field in hostEnvType.GetFields(flags))
        {
            if (field.FieldType == typeof(Lazy<string>))
                field.SetValue(hostEnv, new Lazy<string>(() => ""));
            else if (field.FieldType == typeof(Lazy<bool>))
                field.SetValue(hostEnv, new Lazy<bool>(() => false));
        }
    }

    private static readonly Type[] BenchmarkTypes =
    {
        typeof(PathBuildingBenchmarks),
        typeof(GeometryConversionBenchmarks),
        typeof(LayoutBenchmarks),
    };

    #region BenchmarkDotNet formatter

    private static string FormatBdnResults(List<Summary> summaries)
    {
        var sb = new StringBuilder();

        foreach (var summary in summaries)
        {
            if (summary?.Table?.FullContent == null) continue;

            sb.AppendLine($"## {summary.Title}");
            sb.AppendLine();

            var columns = summary.Table.Columns;
            sb.AppendLine("| " + string.Join(" | ", columns.Select(c => c.Header)) + " |");
            sb.AppendLine("| " + string.Join(" | ", columns.Select(_ => "---")) + " |");

            foreach (var row in summary.Table.FullContent)
                sb.AppendLine("| " + string.Join(" | ", row) + " |");

            sb.AppendLine();
        }

        return sb.ToString();
    }

    #endregion

    #region Manual benchmark runner (iOS fallback)

    private const int WarmupIterations = 5;
    private const int MeasureIterations = 20;
    private const int InnerLoopCount = 1000;

    private static string RunManual()
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Manual Benchmark Results (Stopwatch)");
        sb.AppendLine($"Platform: {DeviceInfo.Platform}, Device: {DeviceInfo.Model}");
        sb.AppendLine($"Warmup: {WarmupIterations}, Iterations: {MeasureIterations}, Inner loop: {InnerLoopCount}");
        sb.AppendLine();

        RunPathBuildingBenchmarks(sb);
        RunGeometryConversionBenchmarks(sb);
        RunLayoutBenchmarks(sb);

        return sb.ToString();
    }

    private static void RunPathBuildingBenchmarks(StringBuilder sb)
    {
        sb.AppendLine("## Path Building");
        sb.AppendLine("| Method | CornerRadius | Mean (μs) | StdDev (μs) | Ratio |");
        sb.AppendLine("| --- | --- | --- | --- | --- |");

        var bounds = new Microsoft.Maui.Graphics.RectF(0, 0, 300, 200);

        foreach (double cr in new[] { 0.0, 16.0, 32.0 })
        {
            var baselineNs = MeasureNs(() =>
            {
                var path = new Microsoft.Maui.Graphics.PathF();
                path.AppendRoundedRectangle(bounds, (float)cr);
                return path;
            });

            var squircle06Ns = MeasureNs(() =>
                SquirclePathBuilder.Build(bounds, new Microsoft.Maui.CornerRadius(cr), 0.6));

            var squircle00Ns = MeasureNs(() =>
                SquirclePathBuilder.Build(bounds, new Microsoft.Maui.CornerRadius(cr), 0.0));

            var squircle10Ns = MeasureNs(() =>
                SquirclePathBuilder.Build(bounds, new Microsoft.Maui.CornerRadius(cr), 1.0));

            var perCornerNs = MeasureNs(() =>
                SquirclePathBuilder.Build(bounds, new Microsoft.Maui.CornerRadius(cr, cr / 2, cr, cr / 2), 0.6));

            sb.AppendLine($"| RoundRectangle (Border baseline) | {cr} | {baselineNs.Mean / 1000:F2} | {baselineNs.StdDev / 1000:F2} | 1.00 |");
            sb.AppendLine($"| Squircle (0.6 smoothing) | {cr} | {squircle06Ns.Mean / 1000:F2} | {squircle06Ns.StdDev / 1000:F2} | {squircle06Ns.Mean / baselineNs.Mean:F2} |");
            sb.AppendLine($"| Squircle (0.0 smoothing) | {cr} | {squircle00Ns.Mean / 1000:F2} | {squircle00Ns.StdDev / 1000:F2} | {squircle00Ns.Mean / baselineNs.Mean:F2} |");
            sb.AppendLine($"| Squircle (1.0 smoothing) | {cr} | {squircle10Ns.Mean / 1000:F2} | {squircle10Ns.StdDev / 1000:F2} | {squircle10Ns.Mean / baselineNs.Mean:F2} |");
            sb.AppendLine($"| Squircle (per-corner) | {cr} | {perCornerNs.Mean / 1000:F2} | {perCornerNs.StdDev / 1000:F2} | {perCornerNs.Mean / baselineNs.Mean:F2} |");
        }

        sb.AppendLine();
    }

    private static void RunGeometryConversionBenchmarks(StringBuilder sb)
    {
        sb.AppendLine("## Geometry Conversion");
        sb.AppendLine("| Method | CornerRadius | Mean (μs) | StdDev (μs) | Ratio |");
        sb.AppendLine("| --- | --- | --- | --- | --- |");

        var bounds = new Microsoft.Maui.Graphics.RectF(0, 0, 300, 200);

        foreach (double cr in new[] { 0.0, 16.0, 32.0 })
        {
            var squirclePath = SquirclePathBuilder.Build(bounds, new Microsoft.Maui.CornerRadius(cr), 0.6);
            var roundRectPath = new Microsoft.Maui.Graphics.PathF();
            roundRectPath.AppendRoundedRectangle(bounds, (float)cr);

            var baselineNs = MeasureNs(() =>
                new Microsoft.Maui.Controls.Shapes.RoundRectangleGeometry(
                    new Microsoft.Maui.CornerRadius(cr), new Microsoft.Maui.Graphics.Rect(0, 0, 300, 200)));

            var squircleGeoNs = MeasureNs(() => PathGeometryConverter.ToGeometry(squirclePath));
            var roundRectGeoNs = MeasureNs(() => PathGeometryConverter.ToGeometry(roundRectPath));

            sb.AppendLine($"| RoundRectangleGeometry (Border baseline) | {cr} | {baselineNs.Mean / 1000:F2} | {baselineNs.StdDev / 1000:F2} | 1.00 |");
            sb.AppendLine($"| Squircle PathF → Geometry | {cr} | {squircleGeoNs.Mean / 1000:F2} | {squircleGeoNs.StdDev / 1000:F2} | {squircleGeoNs.Mean / baselineNs.Mean:F2} |");
            sb.AppendLine($"| RoundRect PathF → Geometry | {cr} | {roundRectGeoNs.Mean / 1000:F2} | {roundRectGeoNs.StdDev / 1000:F2} | {roundRectGeoNs.Mean / baselineNs.Mean:F2} |");
        }

        sb.AppendLine();
    }

    private static void RunLayoutBenchmarks(StringBuilder sb)
    {
        sb.AppendLine("## Layout (Measure)");
        sb.AppendLine("| Method | Mean (μs) | StdDev (μs) | Ratio |");
        sb.AppendLine("| --- | --- | --- | --- |");

        var squircle = new SquircleContainer
        {
            CornerRadius = new Microsoft.Maui.CornerRadius(16),
            CornerSmoothing = 0.6,
            Fill = new Microsoft.Maui.Controls.SolidColorBrush(Microsoft.Maui.Graphics.Colors.Blue),
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Microsoft.Maui.Graphics.Colors.Red),
            StrokeThickness = 2,
            Content = new Microsoft.Maui.Controls.Label { Text = "Benchmark content" }
        };

        var border = new Microsoft.Maui.Controls.Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = new Microsoft.Maui.CornerRadius(16)
            },
            Background = new Microsoft.Maui.Controls.SolidColorBrush(Microsoft.Maui.Graphics.Colors.Blue),
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Microsoft.Maui.Graphics.Colors.Red),
            StrokeThickness = 2,
            Content = new Microsoft.Maui.Controls.Label { Text = "Benchmark content" }
        };

        var borderNs = MeasureNs(() => border.Measure(300, 200));
        var squircleNs = MeasureNs(() => squircle.Measure(300, 200));

        sb.AppendLine($"| Border.Measure (baseline) | {borderNs.Mean / 1000:F2} | {borderNs.StdDev / 1000:F2} | 1.00 |");
        sb.AppendLine($"| SquircleContainer.Measure | {squircleNs.Mean / 1000:F2} | {squircleNs.StdDev / 1000:F2} | {squircleNs.Mean / borderNs.Mean:F2} |");

        var createBorderNs = MeasureNs(() => new Microsoft.Maui.Controls.Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new Microsoft.Maui.CornerRadius(16) },
            Background = new Microsoft.Maui.Controls.SolidColorBrush(Microsoft.Maui.Graphics.Colors.Blue),
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Microsoft.Maui.Graphics.Colors.Red),
            StrokeThickness = 2,
            Content = new Microsoft.Maui.Controls.Label { Text = "Content" }
        });

        var createSquircleNs = MeasureNs(() => new SquircleContainer
        {
            CornerRadius = new Microsoft.Maui.CornerRadius(16),
            CornerSmoothing = 0.6,
            Fill = new Microsoft.Maui.Controls.SolidColorBrush(Microsoft.Maui.Graphics.Colors.Blue),
            Stroke = new Microsoft.Maui.Controls.SolidColorBrush(Microsoft.Maui.Graphics.Colors.Red),
            StrokeThickness = 2,
            Content = new Microsoft.Maui.Controls.Label { Text = "Content" }
        });

        sb.AppendLine($"| Border create+configure (baseline) | {createBorderNs.Mean / 1000:F2} | {createBorderNs.StdDev / 1000:F2} | 1.00 |");
        sb.AppendLine($"| SquircleContainer create+configure | {createSquircleNs.Mean / 1000:F2} | {createSquircleNs.StdDev / 1000:F2} | {createSquircleNs.Mean / createBorderNs.Mean:F2} |");
        sb.AppendLine();
    }

    private record struct BenchResult(double Mean, double StdDev);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static BenchResult MeasureNs<T>(Func<T> action)
    {
        // Warmup
        for (int i = 0; i < WarmupIterations * InnerLoopCount; i++)
            action();

        var timings = new double[MeasureIterations];
        var sw = new Stopwatch();

        for (int i = 0; i < MeasureIterations; i++)
        {
            sw.Restart();
            for (int j = 0; j < InnerLoopCount; j++)
                action();
            sw.Stop();
            timings[i] = (double)sw.ElapsedTicks / Stopwatch.Frequency * 1_000_000_000.0 / InnerLoopCount;
        }

        double mean = timings.Average();
        double stdDev = Math.Sqrt(timings.Select(t => (t - mean) * (t - mean)).Average());
        return new BenchResult(mean, stdDev);
    }

    #endregion
}
