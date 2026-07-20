using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Wlrix.Avalonia.Controls;

/// <summary>
/// A single-child decorator that paints a crisp, pixel-aligned SGI / IRIS IM (Motif) style
/// 3D bevel around its child. The bevel is drawn with filled rectangles snapped to whole
/// device pixels so the edges stay sharp at any DPI, matching the chiselled IRIX widget look.
/// </summary>
/// <remarks>
/// <see cref="Decorator.Child"/> holds the content and <see cref="Decorator.Padding"/> the
/// inner padding (in addition to the space taken by the bevel and optional outline).
/// </remarks>
public class BevelBorder : Decorator
{
    public static readonly StyledProperty<WlrixBevel> BevelProperty =
        AvaloniaProperty.Register<BevelBorder, WlrixBevel>(nameof(Bevel), WlrixBevel.Raised);

    /// <summary>Thickness, in pixels, of the light/dark shaded edge.</summary>
    public static readonly StyledProperty<double> BevelThicknessProperty =
        AvaloniaProperty.Register<BevelBorder, double>(nameof(BevelThickness), 2.0);

    /// <summary>Thickness of an optional hard outline drawn outside the bevel.</summary>
    public static readonly StyledProperty<double> OuterLineThicknessProperty =
        AvaloniaProperty.Register<BevelBorder, double>(nameof(OuterLineThickness), 0.0);

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<BevelBorder>();

    /// <summary>Brush for the lit edges (top/left when raised).</summary>
    public static readonly StyledProperty<IBrush?> LightBrushProperty =
        AvaloniaProperty.Register<BevelBorder, IBrush?>(nameof(LightBrush));

    /// <summary>Brush for the shaded edges (bottom/right when raised).</summary>
    public static readonly StyledProperty<IBrush?> DarkBrushProperty =
        AvaloniaProperty.Register<BevelBorder, IBrush?>(nameof(DarkBrush));

    /// <summary>Brush for the optional hard outline.</summary>
    public static readonly StyledProperty<IBrush?> OuterLineBrushProperty =
        AvaloniaProperty.Register<BevelBorder, IBrush?>(nameof(OuterLineBrush));

    static BevelBorder()
    {
        AffectsRender<BevelBorder>(
            BevelProperty, BevelThicknessProperty, OuterLineThicknessProperty,
            BackgroundProperty, LightBrushProperty, DarkBrushProperty, OuterLineBrushProperty);
        AffectsMeasure<BevelBorder>(
            BevelProperty, BevelThicknessProperty, OuterLineThicknessProperty);
    }

    public BevelBorder()
    {
        UseLayoutRounding = true;
    }

    public WlrixBevel Bevel
    {
        get => GetValue(BevelProperty);
        set => SetValue(BevelProperty, value);
    }

    public double BevelThickness
    {
        get => GetValue(BevelThicknessProperty);
        set => SetValue(BevelThicknessProperty, value);
    }

    public double OuterLineThickness
    {
        get => GetValue(OuterLineThicknessProperty);
        set => SetValue(OuterLineThicknessProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
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

    private double EdgeThickness =>
        (Bevel == WlrixBevel.None ? 0 : BevelThickness) + OuterLineThickness;

    private Thickness TotalInset
    {
        get
        {
            var e = EdgeThickness;
            var p = Padding;
            return new Thickness(p.Left + e, p.Top + e, p.Right + e, p.Bottom + e);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var inset = TotalInset;
        var child = Child;
        if (child != null)
        {
            child.Measure(availableSize.Deflate(inset));
            return child.DesiredSize.Inflate(inset);
        }

        return new Size(inset.Left + inset.Right, inset.Top + inset.Bottom);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Child?.Arrange(new Rect(finalSize).Deflate(TotalInset));
        return finalSize;
    }

    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0)
            return;

        // Face fills the whole control (so the bevel sits on top of the face color).
        if (Background is { } bg)
            context.FillRectangle(bg, new Rect(0, 0, w, h));

        var outer = OuterLineThickness;
        if (outer > 0 && OuterLineBrush is { } olb)
        {
            DrawFrame(context, olb, new Rect(0, 0, w, h), outer);
            w -= outer * 2;
            h -= outer * 2;
        }

        var offset = outer;
        var t = BevelThickness;
        if (Bevel == WlrixBevel.None || t <= 0)
            return;

        switch (Bevel)
        {
            case WlrixBevel.Raised:
                DrawBevel(context, offset, offset, w, h, t, LightBrush, DarkBrush);
                break;
            case WlrixBevel.Sunken:
                DrawBevel(context, offset, offset, w, h, t, DarkBrush, LightBrush);
                break;
            case WlrixBevel.Etched:
                // Dark groove on the outside, light on the inside.
                DrawBevel(context, offset, offset, w, h, t / 2, DarkBrush, LightBrush);
                DrawBevel(context, offset + t / 2, offset + t / 2, w - t, h - t, t / 2, LightBrush, DarkBrush);
                break;
            case WlrixBevel.Ridge:
                DrawBevel(context, offset, offset, w, h, t / 2, LightBrush, DarkBrush);
                DrawBevel(context, offset + t / 2, offset + t / 2, w - t, h - t, t / 2, DarkBrush, LightBrush);
                break;
        }
    }

    /// <summary>Draws a two-tone bevel: <paramref name="tl"/> on top/left, <paramref name="br"/> on bottom/right.</summary>
    private static void DrawBevel(DrawingContext context, double x, double y, double w, double h,
        double t, IBrush? tl, IBrush? br)
    {
        if (w <= 0 || h <= 0 || t <= 0)
            return;

        if (br is { } brb)
        {
            // Bottom edge and right edge (full extent).
            context.FillRectangle(brb, new Rect(x, y + h - t, w, t));
            context.FillRectangle(brb, new Rect(x + w - t, y, t, h));
        }

        if (tl is { } tlb)
        {
            // Top edge and left edge (inset so the corners read as mitred).
            context.FillRectangle(tlb, new Rect(x, y, w - t, t));
            context.FillRectangle(tlb, new Rect(x, y, t, h - t));
        }
    }

    /// <summary>Draws a uniform rectangular frame of the given thickness.</summary>
    private static void DrawFrame(DrawingContext context, IBrush brush, Rect r, double t)
    {
        context.FillRectangle(brush, new Rect(r.X, r.Y, r.Width, t)); // top
        context.FillRectangle(brush, new Rect(r.X, r.Bottom - t, r.Width, t)); // bottom
        context.FillRectangle(brush, new Rect(r.X, r.Y + t, t, r.Height - 2 * t)); // left
        context.FillRectangle(brush, new Rect(r.Right - t, r.Y + t, t, r.Height - 2 * t)); // right
    }
}
