using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Plugin.Maui.SquircleContainer;

/// <summary>
/// A container control that renders Apple-style squircle (continuous-curvature) corners.
/// Similar to MAUI's Border control but with smooth superellipse corners instead of circular arcs.
/// </summary>
[ContentProperty(nameof(Content))]
public class SquircleContainer : Grid
{
    private readonly GraphicsView _graphicsView;
    private readonly SquircleDrawable _drawable;

    #region Bindable Properties

    /// <summary>
    /// Shadows Grid.Padding so that padding only applies to content, not the background GraphicsView.
    /// </summary>
    public static new readonly BindableProperty PaddingProperty = BindableProperty.Create(
        nameof(Padding), typeof(Thickness), typeof(SquircleContainer),
        new Thickness(0), propertyChanged: OnPaddingPropertyChanged);

    public static readonly BindableProperty ContentProperty = BindableProperty.Create(
        nameof(Content), typeof(View), typeof(SquircleContainer),
        null, propertyChanged: OnContentChanged);

    public static readonly BindableProperty CornerRadiusProperty = BindableProperty.Create(
        nameof(CornerRadius), typeof(CornerRadius), typeof(SquircleContainer),
        new CornerRadius(0), propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty CornerSmoothingProperty = BindableProperty.Create(
        nameof(CornerSmoothing), typeof(double), typeof(SquircleContainer),
        0.6, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty FillProperty = BindableProperty.Create(
        nameof(Fill), typeof(Brush), typeof(SquircleContainer),
        null, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty StrokeProperty = BindableProperty.Create(
        nameof(Stroke), typeof(Brush), typeof(SquircleContainer),
        null, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty StrokeThicknessProperty = BindableProperty.Create(
        nameof(StrokeThickness), typeof(double), typeof(SquircleContainer),
        0.0, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty StrokeDashPatternProperty = BindableProperty.Create(
        nameof(StrokeDashPattern), typeof(float[]), typeof(SquircleContainer),
        null, propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty StrokeDashOffsetProperty = BindableProperty.Create(
        nameof(StrokeDashOffset), typeof(double), typeof(SquircleContainer),
        0.0, propertyChanged: OnVisualPropertyChanged);

    #endregion

    #region Properties

    /// <summary>
    /// The child content to display inside the squircle container.
    /// </summary>
    public View? Content
    {
        get => (View?)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// The padding between the squircle border and the content.
    /// Unlike Grid.Padding, this only affects content — the background fills the entire container.
    /// </summary>
    public new Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    /// <summary>
    /// The corner radius for the squircle shape. Supports per-corner values.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// The smoothing factor for the corners. 0.0 = standard rounded corners, 1.0 = full squircle.
    /// Default is 0.6, which closely matches Apple's iOS corner smoothing.
    /// </summary>
    public double CornerSmoothing
    {
        get => (double)GetValue(CornerSmoothingProperty);
        set => SetValue(CornerSmoothingProperty, value);
    }

    /// <summary>
    /// The background fill brush. Supports solid colors, linear gradients, and radial gradients.
    /// </summary>
    public Brush? Fill
    {
        get => (Brush?)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>
    /// The stroke (border) brush.
    /// </summary>
    public Brush? Stroke
    {
        get => (Brush?)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    /// <summary>
    /// The thickness of the stroke (border).
    /// </summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>
    /// Dash pattern for the stroke. E.g., "8,4" for a dashed line.
    /// </summary>
    [System.ComponentModel.TypeConverter(typeof(FloatArrayTypeConverter))]
    public float[]? StrokeDashPattern
    {
        get => (float[]?)GetValue(StrokeDashPatternProperty);
        set => SetValue(StrokeDashPatternProperty, value);
    }

    /// <summary>
    /// Offset for the dash pattern.
    /// </summary>
    public double StrokeDashOffset
    {
        get => (double)GetValue(StrokeDashOffsetProperty);
        set => SetValue(StrokeDashOffsetProperty, value);
    }

    #endregion

    public SquircleContainer()
    {
        _drawable = new SquircleDrawable(this);
        _graphicsView = new GraphicsView
        {
            Drawable = _drawable,
            InputTransparent = true,
        };
        // GraphicsView is the background layer (index 0)
        Children.Add(_graphicsView);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        UpdateClipAndInvalidate();
    }

    private static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SquircleContainer container)
        {
            if (oldValue is View oldView)
                container.Children.Remove(oldView);

            if (newValue is View newView)
            {
                newView.Margin = container.Padding;
                container.Children.Add(newView);
            }
        }
    }

    private static void OnPaddingPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SquircleContainer container && container.Content is View content)
            content.Margin = (Thickness)newValue;
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SquircleContainer container)
        {
            container.UpdateClipAndInvalidate();
        }
    }

    private void UpdateClipAndInvalidate()
    {
        if (Width <= 0 || Height <= 0)
            return;

        var bounds = new RectF(0, 0, (float)Width, (float)Height);
        var path = SquirclePathBuilder.Build(bounds, CornerRadius, CornerSmoothing);

        // Update the clip for content clipping
        Clip = PathGeometryConverter.ToGeometry(path);

        // Invalidate the graphics view to redraw
        _graphicsView.Invalidate();
    }
}
