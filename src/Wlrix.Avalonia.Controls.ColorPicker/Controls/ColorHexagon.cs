using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Wlrix.Avalonia.Controls.ColorPicker.Controls;

/// <summary>
/// The hue/saturation hexagon used inside a <see cref="ColorChooser"/>. Hue is the angle around
/// the center (the six corners are R/Y/G/C/B/M at 60° steps), and saturation is the normalized
/// distance from the center to the hexagon edge. The visible Value (brightness) follows the
/// owner. The spectrum is rendered into a cached <see cref="WriteableBitmap"/> regenerated only
/// when the Value or size changes; the selector circle is drawn on top each frame.
/// </summary>
public sealed class ColorHexagon : Control
{
    private WriteableBitmap? _bitmap;
    private double _bitmapValue = -1;
    private PixelSize _bitmapSize;
    private bool _dragging;

    internal ColorChooser? Owner { get; set; }

    public ColorHexagon()
    {
        UseLayoutRounding = true;
        ClipToBounds = false;
        Cursor = new Cursor(StandardCursorType.Cross);
    }

    /// <summary>
    /// Forces the cached spectrum bitmap to be rebuilt on the next render.
    /// </summary>
    internal void InvalidateSpectrum()
    {
        _bitmapValue = -1;
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        _dragging = true;
        e.Pointer.Capture(this);
        PickFrom(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_dragging)
            PickFrom(e.GetPosition(this));
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _dragging = false;
        e.Pointer.Capture(null);
    }

    private void PickFrom(Point p)
    {
        if (Owner is null)
            return;
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0)
            return;

        var cx = w / 2.0;
        var cy = h / 2.0;
        var radius = Math.Min(w, h) / 2.0 - 1;

        var dx = p.X - cx;
        var dy = p.Y - cy;

        // Angle → hue (0° = red at +X, counter-clockwise as Y is down).
        var angle = Math.Atan2(-dy, dx);
        var hue = angle * 180.0 / Math.PI;
        hue = (hue % 360 + 360) % 360;

        var dist = Math.Sqrt(dx * dx + dy * dy);
        var edge = HexEdgeRadius(angle, radius);
        var sat = edge <= 0 ? 0 : Math.Clamp(dist / edge, 0, 1);

        Owner.SetHueSaturation(hue, sat);
    }

    /// <summary>
    /// Distance from center to the hexagon edge along <paramref name="angle"/> (radians), for a
    /// pointy-top hexagon whose corners are at 0/60/120/… degrees and circumradius
    /// <paramref name="radius"/>. Uses the apothem/|cos| form folded into 60° sectors.
    /// </summary>
    private static double HexEdgeRadius(double angle, double radius)
    {
        const double sector = Math.PI / 3.0; // 60°
        var a = angle % sector;
        if (a < 0) a += sector;
        a -= sector / 2.0; // distance from sector center (the edge midpoint)
        var apothem = radius * Math.Cos(Math.PI / 6.0);
        return apothem / Math.Cos(a);
    }

    public override void Render(DrawingContext context)
    {
        var owner = Owner;
        if (owner is null)
            return;

        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 2 || h <= 2)
            return;

        EnsureBitmap((int)Math.Round(w), (int)Math.Round(h), owner.Brightness);
        if (_bitmap is { } bmp)
            context.DrawImage(bmp, new Rect(0, 0, w, h));

        // Selector circle at the current hue/saturation.
        var cx = w / 2.0;
        var cy = h / 2.0;
        var radius = Math.Min(w, h) / 2.0 - 1;
        var angle = owner.Hue * Math.PI / 180.0;
        var r = owner.Saturation * HexEdgeRadius(angle, radius);
        var px = cx + r * Math.Cos(angle);
        var py = cy - r * Math.Sin(angle);

        var outer = new Pen(Brushes.Black, 2);
        var inner = new Pen(Brushes.White, 1);
        context.DrawEllipse(null, outer, new Point(px, py), 5, 5);
        context.DrawEllipse(null, inner, new Point(px, py), 5, 5);
    }

    private void EnsureBitmap(int w, int h, double value)
    {
        if (w <= 0 || h <= 0)
            return;
        var size = new PixelSize(w, h);
        if (_bitmap is not null && _bitmapSize == size && Math.Abs(_bitmapValue - value) < 1e-6)
            return;

        _bitmap?.Dispose();
        _bitmap = new WriteableBitmap(size, new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
        _bitmapSize = size;
        _bitmapValue = value;

        var cx = w / 2.0;
        var cy = h / 2.0;
        var radius = Math.Min(w, h) / 2.0 - 1;

        using var fb = _bitmap.Lock();
        unsafe
        {
            var basePtr = (byte*)fb.Address;
            var stride = fb.RowBytes;
            for (var y = 0; y < h; y++)
            {
                var row = basePtr + y * stride;
                for (var x = 0; x < w; x++)
                {
                    var dx = x + 0.5 - cx;
                    var dy = y + 0.5 - cy;
                    var angle = Math.Atan2(-dy, dx);
                    var dist = Math.Sqrt(dx * dx + dy * dy);
                    var edge = HexEdgeRadius(angle, radius);

                    byte b, g, r, a;
                    if (dist <= edge)
                    {
                        var hue = (angle * 180.0 / Math.PI % 360 + 360) % 360;
                        var sat = edge <= 0 ? 0 : Math.Clamp(dist / edge, 0, 1);
                        var rgb = HsvColor.FromHsv(hue, sat, value).ToRgb();
                        // Premultiplied BGRA (alpha = 255, so premultiplication is a no-op).
                        b = rgb.B;
                        g = rgb.G;
                        r = rgb.R;
                        a = 255;
                    }
                    else
                    {
                        b = g = r = a = 0;
                    }

                    var px = row + x * 4;
                    px[0] = b;
                    px[1] = g;
                    px[2] = r;
                    px[3] = a;
                }
            }
        }
    }
}
