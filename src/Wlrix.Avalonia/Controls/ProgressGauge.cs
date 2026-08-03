using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Wlrix.Avalonia.Controls;

/// <summary>
/// The drawable face of the SGI / IRIX progress bar: a sunken trough with an accent-colored
/// fill that has an angled (sheared) leading edge, plus an optional value label above the
/// trough that follows the end of the fill. Used as the content of the themed
/// <see cref="ProgressBar"/> template; the bar's Minimum/Maximum/Value and
/// text options are pushed in via template bindings.
/// </summary>
public sealed class ProgressGauge : Control
{
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<ProgressGauge, double>(nameof(Minimum));

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<ProgressGauge, double>(nameof(Maximum), 100.0);

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<ProgressGauge, double>(nameof(Value));

    /// <summary>
    /// Whether the value label is drawn above the trough.
    /// </summary>
    public static readonly StyledProperty<bool> ShowValueTextProperty =
        AvaloniaProperty.Register<ProgressGauge, bool>(nameof(ShowValueText));

    /// <summary>
    /// Composite format for the label, matching <c>ProgressBar.ProgressTextFormat</c>: the
    /// arguments are {0}=value, {1}=percentage, {2}=minimum, {3}=maximum. The default shows the
    /// percentage with a trailing "%".
    /// </summary>
    public static readonly StyledProperty<string?> TextFormatProperty =
        AvaloniaProperty.Register<ProgressGauge, string?>(nameof(TextFormat), "{1:0}%");

    /// <summary>
    /// Height (px) reserved above the trough for the value label.
    /// </summary>
    public static readonly StyledProperty<double> LabelHeightProperty =
        AvaloniaProperty.Register<ProgressGauge, double>(nameof(LabelHeight), 16.0);

    /// <summary>
    /// Horizontal shear (px) of the fill's leading edge — the angled end.
    /// </summary>
    public static readonly StyledProperty<double> ShearProperty =
        AvaloniaProperty.Register<ProgressGauge, double>(nameof(Shear), 8.0);

    public static readonly StyledProperty<IBrush?> FillBrushProperty =
        AvaloniaProperty.Register<ProgressGauge, IBrush?>(nameof(FillBrush));

    public static readonly StyledProperty<IBrush?> TroughBrushProperty =
        AvaloniaProperty.Register<ProgressGauge, IBrush?>(nameof(TroughBrush));

    public static readonly StyledProperty<IBrush?> LightBrushProperty =
        AvaloniaProperty.Register<ProgressGauge, IBrush?>(nameof(LightBrush));

    public static readonly StyledProperty<IBrush?> DarkBrushProperty =
        AvaloniaProperty.Register<ProgressGauge, IBrush?>(nameof(DarkBrush));

    public static readonly StyledProperty<IBrush?> OuterLineBrushProperty =
        AvaloniaProperty.Register<ProgressGauge, IBrush?>(nameof(OuterLineBrush));

    public static readonly StyledProperty<IBrush?> TextBrushProperty =
        AvaloniaProperty.Register<ProgressGauge, IBrush?>(nameof(TextBrush));

    static ProgressGauge()
    {
        AffectsRender<ProgressGauge>(MinimumProperty, MaximumProperty, ValueProperty, ShowValueTextProperty,
            TextFormatProperty, LabelHeightProperty, ShearProperty, FillBrushProperty, TroughBrushProperty,
            LightBrushProperty, DarkBrushProperty, OuterLineBrushProperty, TextBrushProperty);
        AffectsMeasure<ProgressGauge>(ShowValueTextProperty, LabelHeightProperty);
    }

    public ProgressGauge()
    {
        UseLayoutRounding = true;
    }

    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool ShowValueText
    {
        get => GetValue(ShowValueTextProperty);
        set => SetValue(ShowValueTextProperty, value);
    }

    public string? TextFormat
    {
        get => GetValue(TextFormatProperty);
        set => SetValue(TextFormatProperty, value);
    }

    public double LabelHeight
    {
        get => GetValue(LabelHeightProperty);
        set => SetValue(LabelHeightProperty, value);
    }

    public double Shear
    {
        get => GetValue(ShearProperty);
        set => SetValue(ShearProperty, value);
    }

    public IBrush? FillBrush
    {
        get => GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }

    public IBrush? TroughBrush
    {
        get => GetValue(TroughBrushProperty);
        set => SetValue(TroughBrushProperty, value);
    }

    public IBrush? LightBrush
    {
        get => GetValue(LightBrushProperty);
        set => SetValue(LightBrushProperty, value);
    }

    public IBrush? DarkBrush
    {
        get => GetValue(DarkBrushProperty);
        set => SetValue(DarkBrushProperty, value);
    }

    public IBrush? OuterLineBrush
    {
        get => GetValue(OuterLineBrushProperty);
        set => SetValue(OuterLineBrushProperty, value);
    }

    public IBrush? TextBrush
    {
        get => GetValue(TextBrushProperty);
        set => SetValue(TextBrushProperty, value);
    }

    private double Fraction
    {
        get
        {
            var range = Maximum - Minimum;
            if (range <= 0)
                return 0;
            return Math.Clamp((Value - Minimum) / range, 0, 1);
        }
    }

    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0)
            return;

        var labelH = ShowValueText ? LabelHeight : 0;
        var troughTop = labelH;
        var troughH = h - labelH;
        if (troughH <= 0)
            return;

        var line = OuterLineBrush ?? Brushes.Black;
        var light = LightBrush ?? Brushes.White;
        var dark = DarkBrush ?? Brushes.Black;

        // Sunken trough: 1px hard outline, then a 1px bevel (dark top/left, light bottom/right).
        var trough = new Rect(0, troughTop, w, troughH);
        context.FillRectangle(TroughBrush ?? Brushes.Gray, trough);
        DrawFrame(context, line, trough, 1);
        var inner = trough.Deflate(1);
        if (inner.Width <= 0 || inner.Height <= 0)
            return;

        context.FillRectangle(dark, new Rect(inner.X, inner.Y, inner.Width, 1));
        context.FillRectangle(dark, new Rect(inner.X, inner.Y, 1, inner.Height));
        context.FillRectangle(light, new Rect(inner.X, inner.Bottom - 1, inner.Width, 1));
        context.FillRectangle(light, new Rect(inner.Right - 1, inner.Y, 1, inner.Height));

        // The fill area is inside the bevel.
        var fillArea = inner.Deflate(1);
        if (fillArea.Width <= 0 || fillArea.Height <= 0)
            return;

        var frac = Fraction;
        var fillLen = fillArea.Width * frac;
        var fill = FillBrush ?? Brushes.RoyalBlue;
        double tipX = fillArea.X;

        if (fillLen > 0.5)
        {
            // Angled (sheared) leading edge: the bottom of the fill reaches further right than
            // the top. Shear is clamped so it never exceeds the fill length.
            var shear = Math.Min(Shear, fillLen);
            var x0 = fillArea.X;
            var bottomRight = x0 + fillLen;
            var topRight = x0 + fillLen - shear;
            tipX = topRight;

            var geo = new StreamGeometry();
            using (var gc = geo.Open())
            {
                gc.BeginFigure(new Point(x0, fillArea.Y), true);
                gc.LineTo(new Point(topRight, fillArea.Y));
                gc.LineTo(new Point(bottomRight, fillArea.Bottom));
                gc.LineTo(new Point(x0, fillArea.Bottom));
                gc.EndFigure(true);
            }

            context.DrawGeometry(fill, null, geo);
        }

        // Value label above the trough, centered on the fill tip.
        if (ShowValueText && labelH > 0)
        {
            var text = FormatLabel();
            if (!string.IsNullOrEmpty(text))
            {
                var ft = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    Typeface.Default, 11, TextBrush ?? Brushes.Black);

                // Center the label over the fill tip, clamped to stay within the control.
                var x = tipX - ft.Width / 2;
                x = Math.Clamp(x, 0, Math.Max(0, w - ft.Width));
                var y = (labelH - ft.Height) / 2;
                context.DrawText(ft, new Point(x, y));
            }
        }
    }

    private string FormatLabel()
    {
        var format = string.IsNullOrEmpty(TextFormat) ? "{1:0}%" : TextFormat!;
        var percent = Fraction * 100;
        try
        {
            // Matches ProgressBar.ProgressTextFormat args: {0}=value {1}=percent {2}=min {3}=max.
            return string.Format(CultureInfo.CurrentCulture, format, Value, percent, Minimum, Maximum);
        }
        catch (FormatException)
        {
            return ((int)Math.Round(Value)).ToString(CultureInfo.CurrentCulture);
        }
    }

    private static void DrawFrame(DrawingContext context, IBrush brush, Rect r, double t)
    {
        context.FillRectangle(brush, new Rect(r.X, r.Y, r.Width, t));
        context.FillRectangle(brush, new Rect(r.X, r.Bottom - t, r.Width, t));
        context.FillRectangle(brush, new Rect(r.X, r.Y + t, t, r.Height - 2 * t));
        context.FillRectangle(brush, new Rect(r.Right - t, r.Y + t, t, r.Height - 2 * t));
    }
}
