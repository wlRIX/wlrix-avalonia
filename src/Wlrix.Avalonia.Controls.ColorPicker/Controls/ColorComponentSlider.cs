using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;

namespace Wlrix.Avalonia.Controls.ColorPicker.Controls;

/// <summary>
/// A horizontal slider whose track shows the gradient of a single color
/// <see cref="Component"/> (e.g., the full hue spectrum, or red 0→255 at the current G/B). The
/// raised thumb is dragged to set <see cref="Value"/>. Used by <see cref="ColorChooser"/> for
/// the IRIX component sliders; reuses the theme's bevel palette for chrome.
/// </summary>
public sealed class ColorComponentSlider : Control
{
    public static readonly StyledProperty<ColorComponent> ComponentProperty = AvaloniaProperty.Register<ColorComponentSlider, ColorComponent>(nameof(Component));

    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<ColorComponentSlider, double>(nameof(Minimum));

    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<ColorComponentSlider, double>(nameof(Maximum), 1.0);

    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<ColorComponentSlider, double>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// The color the gradient is computed against (the chooser's current color).
    /// </summary>
    public static readonly StyledProperty<Color> BaseColorProperty = AvaloniaProperty.Register<ColorComponentSlider, Color>(nameof(BaseColor), Colors.Blue);

    public static readonly StyledProperty<IBrush?> FaceBrushProperty = AvaloniaProperty.Register<ColorComponentSlider, IBrush?>(nameof(FaceBrush));

    public static readonly StyledProperty<IBrush?> LightBrushProperty = AvaloniaProperty.Register<ColorComponentSlider, IBrush?>(nameof(LightBrush));

    public static readonly StyledProperty<IBrush?> DarkBrushProperty = AvaloniaProperty.Register<ColorComponentSlider, IBrush?>(nameof(DarkBrush));

    public static readonly StyledProperty<IBrush?> OuterLineBrushProperty = AvaloniaProperty.Register<ColorComponentSlider, IBrush?>(nameof(OuterLineBrush));

    private bool _dragging;

    static ColorComponentSlider()
    {
        AffectsRender<ColorComponentSlider>(
            ComponentProperty, MinimumProperty, MaximumProperty, ValueProperty,
            BaseColorProperty, FaceBrushProperty, LightBrushProperty, DarkBrushProperty,
            OuterLineBrushProperty);
    }

    public ColorComponentSlider()
    {
        UseLayoutRounding = true;
        Height = 18;
        Cursor = new Cursor(StandardCursorType.Hand);
    }

    public ColorComponent Component { get => GetValue(ComponentProperty); set => SetValue(ComponentProperty, value); }
    public double Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public Color BaseColor { get => GetValue(BaseColorProperty); set => SetValue(BaseColorProperty, value); }
    public IBrush? FaceBrush { get => GetValue(FaceBrushProperty); set => SetValue(FaceBrushProperty, value); }
    public IBrush? LightBrush { get => GetValue(LightBrushProperty); set => SetValue(LightBrushProperty, value); }
    public IBrush? DarkBrush { get => GetValue(DarkBrushProperty); set => SetValue(DarkBrushProperty, value); }
    public IBrush? OuterLineBrush { get => GetValue(OuterLineBrushProperty); set => SetValue(OuterLineBrushProperty, value); }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        _dragging = true;
        e.Pointer.Capture(this);
        SetFrom(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragging)
            SetFrom(e.GetPosition(this));
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragging = false;
        e.Pointer.Capture(null);
    }

    private void SetFrom(Point p)
    {
        var w = Bounds.Width;
        if (w <= 0)
            return;
        var t = Math.Clamp(p.X / w, 0, 1);
        SetCurrentValue(ValueProperty, Minimum + t * (Maximum - Minimum));
    }

    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0)
            return;

        // Sunken trough frame (1px outline + 1px bevel), gradient track inside.
        var line = OuterLineBrush ?? Brushes.Black;
        var light = LightBrush ?? Brushes.White;
        var dark = DarkBrush ?? Brushes.Black;

        // Outline
        context.FillRectangle(line, new Rect(0, 0, w, h));
        var inner = new Rect(1, 1, Math.Max(0, w - 2), Math.Max(0, h - 2));

        // Sunken bevel: dark top/left, light bottom/right.
        context.FillRectangle(dark, new Rect(inner.X, inner.Y, inner.Width, 1));
        context.FillRectangle(dark, new Rect(inner.X, inner.Y, 1, inner.Height));
        context.FillRectangle(light, new Rect(inner.X, inner.Bottom - 1, inner.Width, 1));
        context.FillRectangle(light, new Rect(inner.Right - 1, inner.Y, 1, inner.Height));

        var track = new Rect(inner.X + 1, inner.Y + 1, Math.Max(0, inner.Width - 2), Math.Max(0, inner.Height - 2));
        if (track.Width <= 0 || track.Height <= 0)
            return;

        var gradient = BuildGradient();
        context.FillRectangle(gradient, track);

        // Thumb: a thin raised marker at the current value.
        var range = Maximum - Minimum;
        var t = range <= 0 ? 0 : Math.Clamp((Value - Minimum) / range, 0, 1);
        var x = track.X + t * track.Width;
        var thumb = new Rect(Math.Round(x) - 2, 0, 4, h);
        context.FillRectangle(FaceBrush ?? Brushes.Gray, thumb);
        context.FillRectangle(light, new Rect(thumb.X, thumb.Y, 1, thumb.Height));
        context.FillRectangle(dark, new Rect(thumb.Right - 1, thumb.Y, 1, thumb.Height));
        context.FillRectangle(line, new Rect(thumb.X, thumb.Y, thumb.Width, 1));
        context.FillRectangle(line, new Rect(thumb.X, thumb.Bottom - 1, thumb.Width, 1));
    }

    private LinearGradientBrush BuildGradient()
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative)
        };

        void Stop(double offset, Color c) => brush.GradientStops.Add(new GradientStop(c, offset));

        var bc = BaseColor;
        switch (Component)
        {
            case ColorComponent.Hue:
                for (var i = 0; i <= 6; i++)
                    Stop(i / 6.0, HsvColor.FromHsv(i * 60.0 % 360, 1, 1).ToRgb());
                break;
            case ColorComponent.Saturation:
            {
                var hsv = bc.ToHsv();
                Stop(0, HsvColor.FromHsv(hsv.H, 0, hsv.V).ToRgb());
                Stop(1, HsvColor.FromHsv(hsv.H, 1, hsv.V).ToRgb());
                break;
            }
            case ColorComponent.Value:
            {
                var hsv = bc.ToHsv();
                Stop(0, HsvColor.FromHsv(hsv.H, hsv.S, 0).ToRgb());
                Stop(1, HsvColor.FromHsv(hsv.H, hsv.S, 1).ToRgb());
                break;
            }
            case ColorComponent.Red:
                Stop(0, Color.FromRgb(0, bc.G, bc.B));
                Stop(1, Color.FromRgb(255, bc.G, bc.B));
                break;
            case ColorComponent.Green:
                Stop(0, Color.FromRgb(bc.R, 0, bc.B));
                Stop(1, Color.FromRgb(bc.R, 255, bc.B));
                break;
            case ColorComponent.Blue:
                Stop(0, Color.FromRgb(bc.R, bc.G, 0));
                Stop(1, Color.FromRgb(bc.R, bc.G, 255));
                break;
        }

        return brush;
    }
}
