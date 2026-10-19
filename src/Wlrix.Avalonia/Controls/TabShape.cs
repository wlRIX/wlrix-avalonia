using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Wlrix.Avalonia.Controls;

/// <summary>
/// The rounded-trapezoid background of an IRIX / SGI tab: a wide base (the edge that meets the
/// page) and a narrower opposite edge with chamfered corners and slanted sides. Used behind the
/// header in the themed <see cref="TabItem"/> template. <see cref="FlipVertical"/>
/// orients the wide base toward the page for top- (base down) or bottom- (base up) placed strips.
/// The base is left open: the page draws that edge, and breaks it where the selected tab sits.
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

    /// <summary>
    /// Width (px) of the light and dark bevel inside the outline.
    /// </summary>
    public static readonly StyledProperty<double> BevelThicknessProperty =
        AvaloniaProperty.Register<TabShape, double>(nameof(BevelThickness), 2.0);

    static TabShape()
    {
        AffectsRender<TabShape>(BackgroundProperty, SelectedBackgroundProperty, IsSelectedProperty, LightBrushProperty,
            DarkBrushProperty, OuterLineBrushProperty, FlipVerticalProperty, SlantWidthProperty, CornerRadiusProperty,
            OutlineThicknessProperty, BevelThicknessProperty);
        AffectsMeasure<TabShape>(SlantWidthProperty, CornerRadiusProperty);
    }

    public TabShape()
    {
        UseLayoutRounding = true;
        // Slanted edges one or two pixels wide turn to gray smears when antialiased; IRIX drew
        // them as stepped pixels.
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
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

    public double BevelThickness
    {
        get => GetValue(BevelThicknessProperty);
        set => SetValue(BevelThicknessProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        var face = (IsSelected ? SelectedBackground : Background) ?? Brushes.Gray;
        Draw(context, new Rect(Bounds.Size), FlipVertical, face, LightBrush, DarkBrush,
            OuterLineBrush ?? Brushes.Black, SlantWidth, CornerRadius, OutlineThickness, BevelThickness);
    }

    /// <summary>
    /// Draws one tab into <paramref name="rect"/>: hard outline, then the bevel, then the face.
    /// </summary>
    /// <remarks>
    /// Shared with <see cref="NotebookTabPanel"/>, which draws the slanted edges of hidden
    /// tabs at the ends of the strip with the same strokes.
    /// The base (the edge against the page) gets no outline or bevel. The page draws that
    /// edge itself, and leaves a gap where the selected tab sits, so the selected tab's face
    /// runs on into the page. The leading slant is lit. On a top tab the narrow edge is lit
    /// too; on a bottom tab it is shaded, as are both trailing edges.
    /// </remarks>
    public static void Draw(DrawingContext context, Rect rect, bool flip, IBrush face, IBrush? light,
        IBrush? dark, IBrush outline, double slant, double corner, double outlineThickness, double bevel)
    {
        var w = rect.Width;
        var h = rect.Height;
        if (w <= 0 || h <= 0)
            return;

        var s = Math.Min(slant, w / 2);
        var r = Math.Min(corner, Math.Min(s, h / 2));
        var t = outlineThickness;
        var b = bevel;

        var outer = Outline(w, h, s, r, 0);
        var inner = Outline(w, h, s, r, t);
        var faceEdge = Outline(w, h, s, r, t + b);

        context.DrawGeometry(outline, null, Build(outer, rect, h, flip));
        if (b > 0)
        {
            context.DrawGeometry(dark ?? outline, null, Build(inner, rect, h, flip));
            if (light is not null)
            {
                // The lit band: from the base up the leading slant and round its corner, then
                // along the narrow edge as well when that edge faces up.
                var lastLit = flip ? 2 : 3;
                var band = new List<Point>();
                for (var i = 0; i <= lastLit; i++)
                    band.Add(inner[i]);
                for (var i = lastLit; i >= 0; i--)
                    band.Add(faceEdge[i]);
                context.DrawGeometry(light, null, Build(band, rect, h, flip));
            }
        }

        context.DrawGeometry(face, null, Build(faceEdge, rect, h, flip));
    }

    /// <summary>
    /// The silhouette of a top tab (narrow edge at y=0, wide base at y=h), inset by
    /// <paramref name="d"/> on every side but the base. Clockwise from the bottom-left.
    /// </summary>
    private static List<Point> Outline(double w, double h, double s, double r, double d) =>
    [
        new(d, h), // wide base, left
        new(s + d, r + d), // up the leading slant
        new(s + r + d, d), // corner into the narrow edge
        new(w - s - r - d, d), // along the narrow edge
        new(w - s - d, r + d), // corner out of it
        new(w - d, h), // down the trailing slant
    ];

    /// <summary>
    /// A closed path through <paramref name="points"/>, turned upside down for a bottom tab
    /// and moved to <paramref name="rect"/>.
    /// </summary>
    private static StreamGeometry Build(IReadOnlyList<Point> points, Rect rect, double h, bool flip)
    {
        var geo = new StreamGeometry();
        using var gc = geo.Open();
        for (var i = 0; i < points.Count; i++)
        {
            var p = points[i];
            var pt = new Point(rect.X + p.X, rect.Y + (flip ? h - p.Y : p.Y));
            if (i == 0)
                gc.BeginFigure(pt, true);
            else
                gc.LineTo(pt);
        }

        gc.EndFigure(true);
        return geo;
    }
}
