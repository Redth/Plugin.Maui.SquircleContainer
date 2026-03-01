using Microsoft.Maui.Graphics;

namespace Plugin.Maui.SquircleContainer;

/// <summary>
/// Draws the squircle background fill and border stroke for a SquircleContainer.
/// </summary>
internal class SquircleDrawable : IDrawable
{
    private readonly SquircleContainer _container;

    public SquircleDrawable(SquircleContainer container)
    {
        _container = container;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (dirtyRect.Width <= 0 || dirtyRect.Height <= 0)
            return;

        float strokeThickness = (float)_container.StrokeThickness;

        // Inset the drawing rect by half the stroke thickness so the stroke doesn't clip
        float inset = strokeThickness / 2f;
        var drawRect = new RectF(
            dirtyRect.X + inset,
            dirtyRect.Y + inset,
            dirtyRect.Width - strokeThickness,
            dirtyRect.Height - strokeThickness);

        if (drawRect.Width <= 0 || drawRect.Height <= 0)
            return;

        var path = SquirclePathBuilder.Build(
            drawRect,
            _container.CornerRadius,
            _container.CornerSmoothing);

        // Fill background
        if (_container.Fill is Brush fillBrush)
        {
            var paint = fillBrush.ToPaint();
            if (paint != null)
            {
                canvas.SetFillPaint(paint, drawRect);
                canvas.FillPath(path);
            }
        }

        // Draw stroke
        if (_container.Stroke is Brush strokeBrush && strokeThickness > 0)
        {
            var strokePaint = strokeBrush.ToPaint();
            if (strokePaint != null)
            {
                canvas.StrokeSize = strokeThickness;
                canvas.StrokeLineCap = LineCap.Round;
                canvas.StrokeLineJoin = LineJoin.Round;

                // Dash pattern (stretch goal)
                if (_container.StrokeDashPattern is { Length: > 0 } dashPattern)
                {
                    canvas.StrokeDashPattern = dashPattern;
                    canvas.StrokeDashOffset = (float)_container.StrokeDashOffset;
                }

                // ICanvas doesn't have SetStrokePaint; set StrokeColor directly
                if (strokePaint is SolidPaint sp)
                    canvas.StrokeColor = sp.Color;
                canvas.DrawPath(path);
            }
        }
    }
}

/// <summary>
/// Extension to convert MAUI Brush to Microsoft.Maui.Graphics Paint.
/// </summary>
internal static class BrushExtensions
{
    public static Paint? ToPaint(this Brush brush)
    {
        return brush switch
        {
            SolidColorBrush scb => new SolidPaint(scb.Color),
            LinearGradientBrush lgb => ToLinearGradientPaint(lgb),
            RadialGradientBrush rgb => ToRadialGradientPaint(rgb),
            _ => null
        };
    }

    private static LinearGradientPaint ToLinearGradientPaint(LinearGradientBrush brush)
    {
        var stops = new PaintGradientStop[brush.GradientStops.Count];
        for (int i = 0; i < brush.GradientStops.Count; i++)
        {
            var gs = brush.GradientStops[i];
            stops[i] = new PaintGradientStop(gs.Offset, gs.Color);
        }

        return new LinearGradientPaint(
            stops,
            new Point(brush.StartPoint.X, brush.StartPoint.Y),
            new Point(brush.EndPoint.X, brush.EndPoint.Y));
    }

    private static RadialGradientPaint ToRadialGradientPaint(RadialGradientBrush brush)
    {
        var stops = new PaintGradientStop[brush.GradientStops.Count];
        for (int i = 0; i < brush.GradientStops.Count; i++)
        {
            var gs = brush.GradientStops[i];
            stops[i] = new PaintGradientStop(gs.Offset, gs.Color);
        }

        return new RadialGradientPaint(
            stops,
            new Point(brush.Center.X, brush.Center.Y),
            brush.Radius);
    }
}
