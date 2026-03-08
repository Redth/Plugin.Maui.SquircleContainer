---
name: squircle-container
description: "Add Apple-style squircle (continuous-curvature) corners to .NET MAUI apps using the Plugin.Maui.SquircleContainer NuGet package. Use this skill whenever someone wants smooth iOS-style rounded corners, mentions squircle or superellipse shapes, wants to replace MAUI Border controls with better-looking corners, asks about Figma-style corner smoothing in MAUI, or wants their app's cards/buttons/containers to look more like native iOS. Also use when someone mentions 'Plugin.Maui.SquircleContainer', asks about continuous curvature corners, or wants to migrate their existing Border controls to use squircle shapes. Even if they just say 'I want my app to look more like iOS' or 'the corners on my cards look wrong', this skill applies."
---

# Plugin.Maui.SquircleContainer

A .NET MAUI container control with Apple-style **squircle** (continuous-curvature) corners. Drop-in alternative to MAUI's `Border` control with smooth superellipse corners instead of circular arcs. Uses only MAUI Graphics — no SkiaSharp or other dependencies.

## Why squircle corners matter

Standard `Border` rounded corners use circular arcs, producing a visible "kink" where the arc meets the straight edge. Apple's iOS uses **continuous-curvature corners** (superellipse/squircle) where curvature transitions smoothly. This is the same algorithm behind Figma's "corner smoothing" slider. The difference is subtle but immediately noticeable — squircle corners look more natural and polished.

## Installation

No special setup beyond installing the NuGet package. There is NO `MauiProgram.cs` registration step — the control works immediately after adding the package reference.

```
dotnet add package Plugin.Maui.SquircleContainer
```

## XAML Setup

Add the namespace to any XAML page that uses the control:

```xml
xmlns:squircle="clr-namespace:Plugin.Maui.SquircleContainer;assembly=Plugin.Maui.SquircleContainer"
```

## Basic Usage

```xml
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
```

## C# Usage

```csharp
var container = new SquircleContainer
{
    CornerRadius = new CornerRadius(30),
    CornerSmoothing = 0.6,
    Fill = new SolidColorBrush(Colors.Indigo),
    Content = new Label { Text = "Hello!", TextColor = Colors.White }
};
```

## Properties

| Property | Type | Default | Description |
|---|---|---|---|
| `Content` | `View` | `null` | The child view displayed inside the container |
| `CornerRadius` | `CornerRadius` | `0` | Per-corner radius values. Single value (`"30"`) or four values (`"30,0,30,0"`) |
| `CornerSmoothing` | `double` | `0.6` | Controls the curvature transition. `0.0` = standard circular arcs (same as Border), `0.6` = Apple iOS style (default), `1.0` = full superellipse squircle |
| `Fill` | `Brush` | `null` | Background fill — supports `SolidColorBrush`, `LinearGradientBrush`, `RadialGradientBrush` |
| `Stroke` | `Brush` | `null` | Border stroke brush |
| `StrokeThickness` | `double` | `0` | Border width in device-independent pixels |
| `StrokeDashPattern` | `float[]` | `null` | Dash pattern for the stroke, e.g. `"8,4"` |
| `StrokeDashOffset` | `double` | `0` | Starting offset for the dash pattern |
| `Padding` | `Thickness` | `0` | Space between the squircle border and the content. The background fill extends to the full container edge regardless of padding |

## Common Patterns

### Card with gradient fill and border
```xml
<squircle:SquircleContainer CornerRadius="24"
                            CornerSmoothing="0.6"
                            StrokeThickness="2"
                            Padding="20">
    <squircle:SquircleContainer.Fill>
        <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
            <GradientStop Color="#8B5CF6" Offset="0" />
            <GradientStop Color="#EC4899" Offset="1" />
        </LinearGradientBrush>
    </squircle:SquircleContainer.Fill>
    <squircle:SquircleContainer.Stroke>
        <SolidColorBrush Color="White" />
    </squircle:SquircleContainer.Stroke>
    <VerticalStackLayout Spacing="8">
        <Label Text="Card Title" FontSize="20" FontAttributes="Bold" TextColor="White" />
        <Label Text="Card content goes here" FontSize="14" TextColor="#E0D0FF" />
    </VerticalStackLayout>
</squircle:SquircleContainer>
```

### Pill-shaped tag/chip
```xml
<squircle:SquircleContainer CornerRadius="16" CornerSmoothing="0.6" Padding="12,8">
    <squircle:SquircleContainer.Fill>
        <SolidColorBrush Color="#DBEAFE" />
    </squircle:SquircleContainer.Fill>
    <Label Text="Tag" FontSize="13" TextColor="#1E40AF" />
</squircle:SquircleContainer>
```

### App icon shape (square → squircle)
```xml
<squircle:SquircleContainer CornerRadius="30" CornerSmoothing="0.6"
                            HeightRequest="120" WidthRequest="120">
    <squircle:SquircleContainer.Fill>
        <SolidColorBrush Color="#3B82F6" />
    </squircle:SquircleContainer.Fill>
    <Label Text="App" TextColor="White" FontSize="28" FontAttributes="Bold"
           HorizontalOptions="Center" VerticalOptions="Center" />
</squircle:SquircleContainer>
```

### Chat bubble with asymmetric corners
```xml
<!-- Sent message (tail bottom-right) -->
<squircle:SquircleContainer CornerRadius="18,18,4,18" CornerSmoothing="0.6"
                            Padding="14,10" HorizontalOptions="End">
    <squircle:SquircleContainer.Fill>
        <SolidColorBrush Color="#6366F1" />
    </squircle:SquircleContainer.Fill>
    <Label Text="Hello!" TextColor="White" />
</squircle:SquircleContainer>

<!-- Received message (tail bottom-left) -->
<squircle:SquircleContainer CornerRadius="18,18,18,4" CornerSmoothing="0.6"
                            Padding="14,10" HorizontalOptions="Start">
    <squircle:SquircleContainer.Fill>
        <SolidColorBrush Color="#E2E8F0" />
    </squircle:SquircleContainer.Fill>
    <Label Text="Hi there!" TextColor="#1E293B" />
</squircle:SquircleContainer>
```

### Nested squircles (card with avatar)
```xml
<squircle:SquircleContainer CornerRadius="20" CornerSmoothing="0.6" Padding="16">
    <squircle:SquircleContainer.Fill>
        <SolidColorBrush Color="White" />
    </squircle:SquircleContainer.Fill>
    <squircle:SquircleContainer.Stroke>
        <SolidColorBrush Color="#E2E8F0" />
    </squircle:SquircleContainer.Stroke>
    <squircle:SquircleContainer.StrokeThickness>1</squircle:SquircleContainer.StrokeThickness>
    <HorizontalStackLayout Spacing="12">
        <squircle:SquircleContainer CornerRadius="12" CornerSmoothing="0.6"
                                    HeightRequest="44" WidthRequest="44">
            <squircle:SquircleContainer.Fill>
                <SolidColorBrush Color="#6366F1" />
            </squircle:SquircleContainer.Fill>
            <Label Text="JD" TextColor="White" FontSize="14" FontAttributes="Bold"
                   HorizontalOptions="Center" VerticalOptions="Center" />
        </squircle:SquircleContainer>
        <VerticalStackLayout VerticalOptions="Center">
            <Label Text="Jane Doe" FontSize="15" FontAttributes="Bold" />
            <Label Text="Developer" FontSize="12" TextColor="#64748B" />
        </VerticalStackLayout>
    </HorizontalStackLayout>
</squircle:SquircleContainer>
```

## Migrating from Border to SquircleContainer

SquircleContainer is designed as a drop-in replacement for `Border`. The property mapping is:

| Border Property | SquircleContainer Equivalent | Notes |
|---|---|---|
| `<Border>` | `<squircle:SquircleContainer>` | Change the element name |
| `StrokeShape="RoundRectangle CornerRadius=..."` | `CornerRadius="..."` | Move the corner radius value directly to the container. Add `CornerSmoothing="0.6"` for the iOS look |
| `Background` | `Fill` | Rename the property |
| `Stroke` | `Stroke` | Same property name |
| `StrokeThickness` | `StrokeThickness` | Same property name |
| `Content` | `Content` | Same — child elements work identically |
| `Padding` | `Padding` | Same property name |

### Before (Border)
```xml
<Border StrokeThickness="2"
        Padding="16">
    <Border.StrokeShape>
        <RoundRectangle CornerRadius="20" />
    </Border.StrokeShape>
    <Border.Background>
        <SolidColorBrush Color="#6366F1" />
    </Border.Background>
    <Border.Stroke>
        <SolidColorBrush Color="White" />
    </Border.Stroke>
    <Label Text="Content" TextColor="White" />
</Border>
```

### After (SquircleContainer)
```xml
<squircle:SquircleContainer CornerRadius="20"
                            CornerSmoothing="0.6"
                            StrokeThickness="2"
                            Padding="16">
    <squircle:SquircleContainer.Fill>
        <SolidColorBrush Color="#6366F1" />
    </squircle:SquircleContainer.Fill>
    <squircle:SquircleContainer.Stroke>
        <SolidColorBrush Color="White" />
    </squircle:SquircleContainer.Stroke>
    <Label Text="Content" TextColor="White" />
</squircle:SquircleContainer>
```

### Migration steps for an existing project

1. Install the package: `dotnet add package Plugin.Maui.SquircleContainer`
2. Add the XML namespace to each XAML file that uses Border:
   ```xml
   xmlns:squircle="clr-namespace:Plugin.Maui.SquircleContainer;assembly=Plugin.Maui.SquircleContainer"
   ```
3. For each `<Border>` you want to convert:
   - Replace `<Border>` with `<squircle:SquircleContainer>`
   - Move `CornerRadius` from `<RoundRectangle>` to the container element directly
   - Add `CornerSmoothing="0.6"` (or your preferred value)
   - Rename `Background` to `Fill`
   - Remove the `StrokeShape` element entirely
   - Everything else (Stroke, StrokeThickness, Padding, Content) stays the same

### Batch migration approach

When migrating many Border controls at once, work file-by-file:
1. Search the project for `<Border` to find all usages
2. In each file, add the squircle xmlns if not already present
3. Convert each Border following the property mapping above
4. Build and visually verify — the shapes will look slightly different (smoother) which is the point

If you want to keep some controls with standard round corners (e.g., where you need exact circular arcs), set `CornerSmoothing="0.0"` on those — it renders identically to Border's RoundRectangle.

## Architecture Notes

- `SquircleContainer` extends `Grid` (not `Border`), so it supports all Grid layout features
- Content clipping uses a `PathGeometry` clip applied to the Grid, so child content is properly clipped to the squircle shape
- The background is drawn via a `GraphicsView` with a custom `IDrawable` — this is how it achieves the smooth curves without SkiaSharp
- No platform-specific code — works identically on iOS, Android, macOS, Mac Catalyst
- Performance: benchmarks show the overall rendering pipeline is actually ~35% faster than Border because PathF→Geometry conversion is more efficient than RoundRectangleGeometry creation
