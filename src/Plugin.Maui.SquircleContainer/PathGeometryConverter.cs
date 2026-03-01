using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Plugin.Maui.SquircleContainer;

/// <summary>
/// Converts a MAUI Graphics PathF to a Microsoft.Maui.Controls.Shapes.Geometry
/// suitable for use with View.Clip.
/// </summary>
internal static class PathGeometryConverter
{
    /// <summary>
    /// Converts a PathF into a PathGeometry that can be used for clipping.
    /// </summary>
    public static Microsoft.Maui.Controls.Shapes.Geometry ToGeometry(PathF path)
    {
        var geometry = new PathGeometry();
        PathFigure? currentFigure = null;

        for (int i = 0; i < path.OperationCount; i++)
        {
            var segmentType = path.GetSegmentType(i);
            var points = path.GetPointsForSegment(i);

            switch (segmentType)
            {
                case PathOperation.Move:
                {
                    currentFigure = new PathFigure
                    {
                        StartPoint = new Point(points[0].X, points[0].Y),
                        IsClosed = false
                    };
                    geometry.Figures.Add(currentFigure);
                    break;
                }

                case PathOperation.Line:
                {
                    currentFigure?.Segments.Add(
                        new LineSegment(new Point(points[0].X, points[0].Y)));
                    break;
                }

                case PathOperation.Cubic:
                {
                    currentFigure?.Segments.Add(new BezierSegment(
                        new Point(points[0].X, points[0].Y),
                        new Point(points[1].X, points[1].Y),
                        new Point(points[2].X, points[2].Y)));
                    break;
                }

                case PathOperation.Quad:
                {
                    currentFigure?.Segments.Add(new QuadraticBezierSegment(
                        new Point(points[0].X, points[0].Y),
                        new Point(points[1].X, points[1].Y)));
                    break;
                }

                case PathOperation.Close:
                {
                    if (currentFigure != null)
                        currentFigure.IsClosed = true;
                    break;
                }
            }
        }

        return geometry;
    }
}
