using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Wlrix.Avalonia.Controls;

/// <summary>
/// The stacks of slanted tab edges at the ends of a notebook strip, one edge for each tab
/// left out on that side, up to a limit. It sits behind the strip, so the end tabs overlap
/// the stacks. It only draws: <see cref="NotebookTabPanel"/> says where the stacks go and
/// handles clicks on them.
/// </summary>
public sealed class NotebookOverflowEdges : Control
{
    /// <summary>Face of a hidden tab (the recessed color of an unselected tab).</summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<NotebookOverflowEdges, IBrush?>(nameof(Background));

    public static readonly StyledProperty<IBrush?> LightBrushProperty =
        AvaloniaProperty.Register<NotebookOverflowEdges, IBrush?>(nameof(LightBrush));

    public static readonly StyledProperty<IBrush?> DarkBrushProperty =
        AvaloniaProperty.Register<NotebookOverflowEdges, IBrush?>(nameof(DarkBrush));

    public static readonly StyledProperty<IBrush?> OuterLineBrushProperty =
        AvaloniaProperty.Register<NotebookOverflowEdges, IBrush?>(nameof(OuterLineBrush));

    private int _before;
    private Rect _leading;
    private int _after;
    private Rect _trailing;
    private bool _flip;
    private double _slant;
    private double _overlap;
    private double _spacing;

    static NotebookOverflowEdges()
    {
        AffectsRender<NotebookOverflowEdges>(BackgroundProperty, LightBrushProperty, DarkBrushProperty,
            OuterLineBrushProperty);
    }

    public NotebookOverflowEdges()
    {
        IsHitTestVisible = false;
        UseLayoutRounding = true;
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
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

    /// <summary>
    /// Sets the stacks from the strip's latest layout. The rects are the space reserved
    /// for each stack, in this control's coordinates.
    /// </summary>
    internal void Update(int before, Rect leading, int after, Rect trailing, bool flip, double slant,
        double overlap, double spacing)
    {
        if (before == _before && leading == _leading && after == _after && trailing == _trailing &&
            flip == _flip && slant == _slant && overlap == _overlap && spacing == _spacing)
            return;

        (_before, _leading, _after, _trailing) = (before, leading, after, trailing);
        (_flip, _slant, _overlap, _spacing) = (flip, slant, overlap, spacing);
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        var face = Background ?? Brushes.Gray;
        var outline = OuterLineBrush ?? Brushes.Black;

        // Each hidden tab shows as one slanted edge peeking out from behind the next, the
        // farthest drawn first. Each is drawn as a full-width tab reaching well under its
        // neighbor, so that its slants run parallel to the strip's and only the outer one
        // shows; the nearest is covered in turn by the end tab of the strip.
        var reach = _overlap + 3 * _slant;
        for (var k = _before - 1; k >= 0; k--)
        {
            var left = _leading.Right - _slant - _spacing * (k + 1);
            var rect = new Rect(left, _leading.Y, _leading.Right - left + reach, _leading.Height);
            TabShape.Draw(context, rect, _flip, face, LightBrush, DarkBrush, outline, _slant, 4, 1, 2);
        }

        for (var k = _after - 1; k >= 0; k--)
        {
            var right = _trailing.Left + _slant + _spacing * (k + 1);
            var rect = new Rect(_trailing.Left - reach, _trailing.Y, right - _trailing.Left + reach,
                _trailing.Height);
            TabShape.Draw(context, rect, _flip, face, LightBrush, DarkBrush, outline, _slant, 4, 1, 2);
        }
    }
}
