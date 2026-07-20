using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace Wlrix.Avalonia.Controls;

/// <summary>
/// The drawable, draggable wheel surface used as the <c>PART_Wheel</c> of a
/// <see cref="Thumbwheel"/>. It renders the ridged cylinder edge and translates pointer drags
/// into value changes on its <see cref="Owner"/>. Kept as a separate control, so the ridges are
/// clipped to the sunken trough, and pointer capture is scoped to the wheel (not the home button).
/// </summary>
public sealed class ThumbwheelSurface : Control
{
    /// <summary>
    /// The recessed base color painted behind the ridges (the cylinder body).
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<ThumbwheelSurface, IBrush?>(nameof(Background));

    private Point _last;
    private bool _dragging;

    /// <summary>
    /// The thumbwheel this surface belongs to; set by <see cref="Thumbwheel.OnApplyTemplate"/>.
    /// </summary>
    internal Thumbwheel? Owner { get; set; }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public ThumbwheelSurface()
    {
        UseLayoutRounding = true;
        ClipToBounds = true;
        Cursor = new Cursor(StandardCursorType.SizeAll);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        _dragging = true;
        _last = e.GetPosition(this);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (!_dragging || Owner is null)
            return;

        var p = e.GetPosition(this);
        // The wheel surface follows the pointer: dragging down (vertical) or right
        // (horizontal) rolls the ridges that way and increases the value.
        var pixels = Owner.Orientation == Orientation.Vertical
            ? p.Y - _last.Y
            : p.X - _last.X;
        _last = p;
        Owner.ApplyDelta(pixels);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragging = false;
        e.Pointer.Capture(null);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (Owner is null)
            return;

        // One notch nudges by a ridge step's worth of pixels.
        var step = Owner.RidgePeriod / Math.Max(Owner.Sensitivity, 0.0001);
        Owner.ApplyDelta(e.Delta.Y * step * 0.25);
        e.Handled = true;
    }

    public override void Render(DrawingContext context)
    {
        var owner = Owner;
        if (owner is null)
            return;

        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0)
            return;

        // Recessed cylinder base behind the ridges.
        if (Background is { } bg)
            context.FillRectangle(bg, new Rect(0, 0, w, h));

        var vertical = owner.Orientation == Orientation.Vertical;
        var span = vertical ? h : w; // length along the rolling axis
        var cross = vertical ? w : h; // width across the wheel
        var center = span / 2.0;
        var radius = span / 2.0;

        var light = owner.LightBrush ?? Brushes.White;
        var dark = owner.DarkBrush ?? Brushes.Black;

        var ridgeCount = Math.Max(2, owner.RidgeCount);
        var angleStep = Math.PI / ridgeCount;

        // Map the wheel phase (value units) into an angular offset. RidgePeriod value units
        // advance the wheel by one ridge step (angleStep radians). The ridge pattern repeats
        // every angleStep, so wrap the phase into one step — otherwise an unbounded phase
        // (continuous mode) eventually shifts the fixed draw window out of the visible arc and
        // the ridges vanish until scrolled back.
        var period = Math.Abs(owner.RidgePeriod) < 1e-6 ? 1.0 : owner.RidgePeriod;
        var phaseAngle = (owner.Phase / period) * angleStep % angleStep;

        const double halfPi = Math.PI / 2.0;

        // Draw enough ridge copies to cover the visible front arc as the phase shifts.
        for (var i = -ridgeCount - 1; i <= ridgeCount + 1; i++)
        {
            var theta = i * angleStep + phaseAngle;
            // Normalize into the visible front half-turn.
            if (theta <= -halfPi || theta >= halfPi)
                continue;

            var pos = center + radius * Math.Sin(theta);
            var y = Math.Round(pos);

            // Two abutting 1px lines (light then dark) read as a small raised notch. The
            // sin() spacing already crowds ridges toward the edges, giving the curvature.
            if (vertical)
            {
                if (y >= 1 && y < h)
                    context.FillRectangle(light, new Rect(0, y - 1, cross, 1));
                if (y >= 0 && y < h)
                    context.FillRectangle(dark, new Rect(0, y, cross, 1));
            }
            else
            {
                if (y >= 1 && y < w)
                    context.FillRectangle(light, new Rect(y - 1, 0, 1, cross));
                if (y >= 0 && y < w)
                    context.FillRectangle(dark, new Rect(y, 0, 1, cross));
            }
        }

        // Cylinder shading: a soft highlight near the top and a deep shadow toward the far
        // end, so the wheel reads as a rounded cylinder with real depth.
        var fade = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(vertical ? 0 : 1, vertical ? 1 : 0, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.FromArgb(110, 0, 0, 0), 0.0),
                new GradientStop(Color.FromArgb(0, 255, 255, 255), 0.18),
                new GradientStop(Color.FromArgb(60, 255, 255, 255), 0.34),
                new GradientStop(Color.FromArgb(0, 0, 0, 0), 0.5),
                new GradientStop(Color.FromArgb(150, 0, 0, 0), 1.0)
            }
        };
        context.FillRectangle(fade, new Rect(0, 0, w, h));
    }
}
