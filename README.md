# Plugin.Maui.SquircleContainer

A .NET MAUI container control with Apple-style **squircle** (continuous-curvature) corners. Drop-in alternative to MAUI's `Border` control, but with smooth superellipse corners instead of circular arcs.

Uses only **MAUI Graphics** — no SkiaSharp or other drawing dependencies.

## What is a Squircle?

Standard rounded rectangles use circular arcs at corners, producing visible curvature discontinuities where the arc meets the straight edge. Apple's iOS uses **continuous-curvature corners** (superellipse/squircle shape) where curvature transitions smoothly, producing a more organic, visually pleasing shape. This is the same algorithm used by Figma's "corner smoothing" feature.

## Installation

```
dotnet add package Plugin.Maui.SquircleContainer
```

## Usage

### XAML

```xml
<ContentPage xmlns:squircle="clr-namespace:Plugin.Maui.SquircleContainer;assembly=Plugin.Maui.SquircleContainer">

    <!-- Basic squircle container -->
    <squircle:SquircleContainer CornerRadius="30"
                                CornerSmoothing="0.6"
                                HeightRequest="120"
                                WidthRequest="280">
        <squircle:SquircleContainer.Fill>
            <SolidColorBrush Color="#6366F1" />
        </squircle:SquircleContainer.Fill>
        <Label Text="Hello Squircle!"
               TextColor="White"
               HorizontalOptions="Center"
               VerticalOptions="Center" />
    </squircle:SquircleContainer>

    <!-- With gradient fill and border -->
    <squircle:SquircleContainer CornerRadius="40"
                                CornerSmoothing="0.6"
                                StrokeThickness="2">
        <squircle:SquircleContainer.Fill>
            <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                <GradientStop Color="#8B5CF6" Offset="0" />
                <GradientStop Color="#EC4899" Offset="1" />
            </LinearGradientBrush>
        </squircle:SquircleContainer.Fill>
        <squircle:SquircleContainer.Stroke>
            <SolidColorBrush Color="White" />
        </squircle:SquircleContainer.Stroke>
        <Label Text="Gradient!" TextColor="White" />
    </squircle:SquircleContainer>

</ContentPage>
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `CornerRadius` | `CornerRadius` | `0` | Per-corner radius values |
| `CornerSmoothing` | `double` | `0.6` | 0.0 = standard round, 1.0 = full squircle. 0.6 ≈ iOS |
| `Fill` | `Brush` | `null` | Background fill (solid, linear gradient, radial gradient) |
| `Stroke` | `Brush` | `null` | Border stroke brush |
| `StrokeThickness` | `double` | `0` | Border width |
| `StrokeDashPattern` | `float[]` | `null` | Dash pattern, e.g. `"8,4"` |
| `StrokeDashOffset` | `double` | `0` | Dash pattern offset |

## Corner Smoothing Values

| Value | Effect |
|---|---|
| `0.0` | Standard circular-arc rounded corners (same as `Border`) |
| `0.6` | Apple iOS-style continuous curvature (default) |
| `1.0` | Full superellipse squircle |

## Algorithm

Based on the [Figma squircle algorithm](https://www.figma.com/blog/desperately-seeking-squircles/):
each corner is drawn as **two cubic Bézier curves** flanking a **circular arc**, with the arc sweep decreasing as smoothing increases. This produces G2-continuous (curvature-continuous) transitions between straight edges and rounded corners.

## License

MIT
