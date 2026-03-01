using System;
using Microsoft.Maui.Graphics;

namespace Plugin.Maui.SquircleContainer;

/// <summary>
/// Generates a squircle (continuous-curvature) path for a rectangle with given corner radii and smoothing.
/// Based on the Figma squircle algorithm: https://www.figma.com/blog/desperately-seeking-squircles/
/// </summary>
public static class SquirclePathBuilder
{
    /// <summary>
    /// Builds a squircle PathF for the given bounds, corner radii, and smoothing factor.
    /// </summary>
    /// <param name="bounds">The rectangle to draw the squircle in.</param>
    /// <param name="cornerRadius">Per-corner radius values.</param>
    /// <param name="smoothing">0.0 = standard rounded corners, 1.0 = full squircle. iOS-like ≈ 0.6.</param>
    public static PathF Build(RectF bounds, CornerRadius cornerRadius, double smoothing)
    {
        smoothing = Math.Clamp(smoothing, 0.0, 1.0);

        var path = new PathF();

        if (bounds.Width <= 0 || bounds.Height <= 0)
            return path;

        float shortestSide = Math.Min(bounds.Width, bounds.Height);

        var tl = new CornerParams(cornerRadius.TopLeft, smoothing, shortestSide);
        var tr = new CornerParams(cornerRadius.TopRight, smoothing, shortestSide);
        var br = new CornerParams(cornerRadius.BottomRight, smoothing, shortestSide);
        var bl = new CornerParams(cornerRadius.BottomLeft, smoothing, shortestSide);

        float left = bounds.Left;
        float top = bounds.Top;
        float right = bounds.Right;
        float bottom = bounds.Bottom;
        float width = bounds.Width;
        float height = bounds.Height;

        // Start at top center
        float centerX = left + width / 2f;
        path.MoveTo(centerX, top);

        // --- Top-right corner ---
        path.LineTo(left + Math.Max(width / 2f, width - tr.P), top);
        DrawCorner(path, tr,
            edgeStart: new PointF(right - (tr.P - tr.A), top),
            bezier1End: new PointF(right - (tr.P - tr.A - tr.B - tr.C), top + tr.D),
            arcCenter: new PointF(right - tr.Radius, top + tr.Radius),
            arcStartAngleDeg: 270 + tr.AngleBezier,
            arcSweepDeg: 90 - 2 * tr.AngleBezier,
            bezier2End: new PointF(right, top + Math.Min(height / 2f, tr.P)),
            cp1: new PointF(right - (tr.P - tr.A - tr.B), top),
            cp2_post: new PointF(right, top + (tr.P - tr.A - tr.B)),
            cp3_post: new PointF(right, top + (tr.P - tr.A)));

        // --- Bottom-right corner ---
        path.LineTo(right, top + Math.Max(height / 2f, height - br.P));
        DrawCorner(path, br,
            edgeStart: new PointF(right, bottom - (br.P - br.A)),
            bezier1End: new PointF(right - br.D, bottom - (br.P - br.A - br.B - br.C)),
            arcCenter: new PointF(right - br.Radius, bottom - br.Radius),
            arcStartAngleDeg: br.AngleBezier,
            arcSweepDeg: 90 - 2 * br.AngleBezier,
            bezier2End: new PointF(left + Math.Max(width / 2f, width - br.P), bottom),
            cp1: new PointF(right, bottom - (br.P - br.A - br.B)),
            cp2_post: new PointF(right - (br.P - br.A - br.B), bottom),
            cp3_post: new PointF(right - (br.P - br.A), bottom));

        // --- Bottom-left corner ---
        path.LineTo(left + Math.Min(width / 2f, bl.P), bottom);
        DrawCorner(path, bl,
            edgeStart: new PointF(left + (bl.P - bl.A), bottom),
            bezier1End: new PointF(left + (bl.P - bl.A - bl.B - bl.C), bottom - bl.D),
            arcCenter: new PointF(left + bl.Radius, bottom - bl.Radius),
            arcStartAngleDeg: 90 + bl.AngleBezier,
            arcSweepDeg: 90 - 2 * bl.AngleBezier,
            bezier2End: new PointF(left, top + Math.Max(height / 2f, height - bl.P)),
            cp1: new PointF(left + (bl.P - bl.A - bl.B), bottom),
            cp2_post: new PointF(left, bottom - (bl.P - bl.A - bl.B)),
            cp3_post: new PointF(left, bottom - (bl.P - bl.A)));

        // --- Top-left corner ---
        path.LineTo(left, top + Math.Min(height / 2f, tl.P));
        DrawCorner(path, tl,
            edgeStart: new PointF(left, top + (tl.P - tl.A)),
            bezier1End: new PointF(left + tl.D, top + (tl.P - tl.A - tl.B - tl.C)),
            arcCenter: new PointF(left + tl.Radius, top + tl.Radius),
            arcStartAngleDeg: 180 + tl.AngleBezier,
            arcSweepDeg: 90 - 2 * tl.AngleBezier,
            bezier2End: new PointF(left + Math.Min(width / 2f, tl.P), top),
            cp1: new PointF(left, top + (tl.P - tl.A - tl.B)),
            cp2_post: new PointF(left + (tl.P - tl.A - tl.B), top),
            cp3_post: new PointF(left + (tl.P - tl.A), top));

        path.Close();
        return path;
    }

    private static void DrawCorner(
        PathF path,
        CornerParams corner,
        PointF edgeStart,
        PointF bezier1End,
        PointF arcCenter,
        float arcStartAngleDeg,
        float arcSweepDeg,
        PointF bezier2End,
        PointF cp1,
        PointF cp2_post,
        PointF cp3_post)
    {
        if (corner.Radius <= 0)
            return;

        // First cubic Bézier: transition from straight edge into the arc
        path.CurveTo(edgeStart, cp1, bezier1End);

        // Circular arc in the middle of the corner
        if (arcSweepDeg > 0.01f)
        {
            float arcStartRad = arcStartAngleDeg * MathF.PI / 180f;
            float arcEndRad = (arcStartAngleDeg + arcSweepDeg) * MathF.PI / 180f;

            // Calculate arc start and end points, then approximate with a Bézier
            AddArcAsBeziers(path, arcCenter, corner.Radius, arcStartRad, arcEndRad);
        }

        // Second cubic Bézier: transition from arc back to straight edge
        path.CurveTo(cp2_post, cp3_post, bezier2End);
    }

    /// <summary>
    /// Adds a circular arc approximated with cubic Bézier curves.
    /// This avoids using PathF.AddArc which has inconsistent behavior across platforms.
    /// </summary>
    private static void AddArcAsBeziers(PathF path, PointF center, float radius, float startAngle, float endAngle)
    {
        float sweep = endAngle - startAngle;
        if (Math.Abs(sweep) < 0.001f)
            return;

        // For small arcs (< 90°), a single Bézier approximation is sufficient
        int segments = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweep) / (MathF.PI / 2f)));
        float segmentSweep = sweep / segments;

        for (int i = 0; i < segments; i++)
        {
            float a1 = startAngle + i * segmentSweep;
            float a2 = a1 + segmentSweep;
            AddArcSegmentAsBezier(path, center, radius, a1, a2);
        }
    }

    private static void AddArcSegmentAsBezier(PathF path, PointF center, float radius, float startAngle, float endAngle)
    {
        float sweep = endAngle - startAngle;
        // Compute the optimal handle length for the Bézier approximation of an arc
        float alpha = 4f / 3f * MathF.Tan(sweep / 4f);

        float cosStart = MathF.Cos(startAngle);
        float sinStart = MathF.Sin(startAngle);
        float cosEnd = MathF.Cos(endAngle);
        float sinEnd = MathF.Sin(endAngle);

        // Control point 1
        float cp1x = center.X + radius * (cosStart - alpha * sinStart);
        float cp1y = center.Y + radius * (sinStart + alpha * cosStart);

        // Control point 2
        float cp2x = center.X + radius * (cosEnd + alpha * sinEnd);
        float cp2y = center.Y + radius * (sinEnd - alpha * cosEnd);

        // End point
        float ex = center.X + radius * cosEnd;
        float ey = center.Y + radius * sinEnd;

        path.CurveTo(cp1x, cp1y, cp2x, cp2y, ex, ey);
    }

    /// <summary>
    /// Pre-computed corner parameters for the squircle algorithm.
    /// </summary>
    private readonly struct CornerParams
    {
        public readonly float Radius;
        public readonly float P;
        public readonly float A;
        public readonly float B;
        public readonly float C;
        public readonly float D;
        public readonly float AngleBezier;
        public readonly float AngleCircle;

        public CornerParams(double radiusValue, double smoothness, float shortestSide)
        {
            Radius = (float)Math.Min(Math.Max(0, radiusValue), shortestSide / 2.0);

            if (Radius <= 0 || smoothness <= 0)
            {
                P = Radius;
                A = B = C = D = AngleBezier = 0;
                AngleCircle = 90;
                return;
            }

            P = (float)Math.Min(shortestSide / 2.0, (1 + smoothness) * Radius);

            // Adjust smoothness if the corner is larger than 1/4 of the shortest side
            float effectiveSmoothing = (float)smoothness;
            if (Radius > shortestSide / 4f)
            {
                float changePercentage = (Radius - shortestSide / 4f) / (shortestSide / 4f);
                AngleCircle = 90f * (1f - effectiveSmoothing * (1f - changePercentage));
                AngleBezier = 45f * effectiveSmoothing * (1f - changePercentage);
            }
            else
            {
                AngleCircle = 90f * (1f - effectiveSmoothing);
                AngleBezier = 45f * effectiveSmoothing;
            }

            float angleBezierRad = AngleBezier * MathF.PI / 180f;
            float angleCircleRad = AngleCircle * MathF.PI / 180f;

            float dToC = MathF.Tan(angleBezierRad);
            float longest = Radius * MathF.Tan(angleBezierRad / 2f);
            float arcLen = MathF.Sin(angleCircleRad / 2f) * Radius * MathF.Sqrt(2f);

            C = longest * MathF.Cos(angleBezierRad);
            D = C * dToC;
            B = ((P - arcLen) - (1f + dToC) * C) / 3f;
            A = 2f * B;

            // Clamp to avoid negative values in degenerate cases
            if (A < 0) A = 0;
            if (B < 0) B = 0;
        }
    }
}
