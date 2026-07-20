using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Wlrix.Avalonia.Controls;

/// <summary>
/// The rounded-trapezoid background of an IRIX / SGI tab: a wide base (the edge that meets the
/// page) and a narrower opposite edge with chamfered corners and slanted sides. Used behind the
/// header in the themed <see cref="TabItem"/> template. <see cref="FlipVertical"/>
/// orients the wide base toward the page for top- (base down) or bottom- (base up) placed strips.
/// </summary>
public sealed class TabShape : Control
{
    /// <summary>
    /// Face when unselected (the recessed tab color).
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<TabShape, IBrush?>(nameof(Background));

    /// <summary>
    /// Face when <see cref="IsSelected"/> (the page color, so the tab merges in).
    /// </summary>
    public static readonly StyledProperty<IBrush?> SelectedBackgroundProperty =
        AvaloniaProperty.Register<TabShape, IBrush?>(nameof(SelectedBackground));

    /// <summary>
    /// True when the owning tab is selected; chooses the face and is bound from TabItem.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<TabShape, bool>(nameof(IsSelected));

    public static readonly StyledProperty<IBrush?> LightBrushProperty =
        AvaloniaProperty.Register<TabShape, IBrush?>(nameof(LightBrush));

    public static readonly StyledProperty<IBrush?> DarkBrushProperty =
        AvaloniaProperty.Register<TabShape, IBrush?>(nameof(DarkBrush));

    public static readonly StyledProperty<IBrush?> OuterLineBrushProperty =
        AvaloniaProperty.Register<TabShape, IBrush?>(nameof(OuterLineBrush));

    /// <summary>
    /// False = wide base at the bottom (top strip); true = wide base at the top.
    /// </summary>
    public static readonly StyledProperty<bool> FlipVerticalProperty =
        AvaloniaProperty.Register<TabShape, bool>(nameof(FlipVertical));

    /// <summary>
    /// Horizontal inset (px) of each slanted side at the narrow edge.
    /// </summary>
    public static readonly StyledProperty<double> SlantWidthProperty =
        AvaloniaProperty.Register<TabShape, double>(nameof(SlantWidth), 10.0);

    /// <summary>
    /// Chamfer (px) on the two narrow-edge corners.
    /// </summary>
    public static readonly StyledProperty<double> CornerRadiusProperty =
        AvaloniaProperty.Register<TabShape, double>(nameof(CornerRadius), 4.0);

    /// <summary>
    /// Thickness (px) of the hard outline.
    /// </summary>
    public static readonly StyledProperty<double> OutlineThicknessProperty =
        AvaloniaProperty.Register<TabShape, double>(nameof(OutlineThickness), 1.0);

    static TabShape()
    {
        AffectsRender<TabShape>(BackgroundProperty, SelectedBackgroundProperty, IsSelectedProperty, LightBrushProperty,
            DarkBrushProperty, OuterLineBrushProperty, FlipVerticalProperty, SlantWidthProperty, CornerRadiusProperty,
            OutlineThicknessProperty);
        AffectsMeasure<TabShape>(SlantWidthProperty, CornerRadiusProperty);
    }

    public TabShape()
    {
        UseLayoutRounding = true;
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public IBrush? SelectedBackground
    {
        get => GetValue(SelectedBackgroundProperty);
        set => SetValue(SelectedBackgroundProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
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

    public bool FlipVertical
    {
        get => GetValue(FlipVerticalProperty);
        set => SetValue(FlipVerticalProperty, value);
    }

    public double SlantWidth
    {
        get => GetValue(SlantWidthProperty);
        set => SetValue(SlantWidthProperty, value);
    }

    public double CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public double OutlineThickness
    {
        get => GetValue(OutlineThicknessProperty);
        set => SetValue(OutlineThicknessProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0)
            return;

        var s = Math.Min(SlantWidth, w / 2);
        var r = Math.Min(CornerRadius, Math.Min(s, h / 2));
        var flip = FlipVertical;

        var outline = OuterLineBrush ?? Brushes.Black;
        var face = (IsSelected ? SelectedBackground : Background) ?? Brushes.Gray;
        var t = OutlineThickness;

        // Hard outline silhouette, then the face inset by the outline thickness.
        context.DrawGeometry(outline, null, BuildTab(w, h, s, r, flip));
        context.DrawGeometry(face, null,
            BuildTab(w - 2 * t, h - 2 * t, Math.Max(0, s - t), Math.Max(0, r), flip, t, t));
    }

    /// <summary>
    /// Builds the trapezoid path. <paramref name="ox"/>/<paramref name="oy"/> offset the
    /// whole shape (used to inset the face inside the outline).
    /// </summary>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="s"></param>
    /// <param name="r"></param>
    /// <param name="flip"></param>
    /// <param name="ox"></param>
    /// <param name="oy"></param>
    /// <returns></returns>
    private static StreamGeometry BuildTab(double w, double h, double s, double r, bool flip, double ox = 0,
        double oy = 0)
    {
        // Top-tab points (narrow edge at y=0, wide base at y=h), clockwise from bottom-left.
        var pts = new[]
        {
            new Point(0, h), // P0 bottom-left (wide base)
            new Point(s, r), // P1 up the left slant
            new Point(s + r, 0), // P2 chamfer top-left
            new Point(w - s - r, 0), // P3 narrow top
            new Point(w - s, r), // P4 chamfer top-right
            new Point(w, h), // P5 down the right slant
        };

        var geo = new StreamGeometry();
        using var gc = geo.Open();
        for (var i = 0; i < pts.Length; i++)
        {
            var p = pts[i];
            var y = flip ? h - p.Y : p.Y;
            var pt = new Point(p.X + ox, y + oy);
            if (i == 0)
                gc.BeginFigure(pt, true);
            else
                gc.LineTo(pt);
        }

        gc.EndFigure(true);
        return geo;
    }
}
