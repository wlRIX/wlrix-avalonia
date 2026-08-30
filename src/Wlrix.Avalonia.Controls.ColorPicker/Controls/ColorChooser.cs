using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Wlrix.Avalonia.Controls.ColorPicker.Controls;

/// <summary>
/// An SGI / IRIX "Color Browser" style color chooser: two swatches (current and stored), an
/// HSV hue/saturation hexagon, component sliders and numeric fields for H/S/V (and R/G/B), an
/// Options menu (find white) and a Sliders menu (HSV / RGB / Both). OK/Cancel/Help are left to
/// the host; the control exposes <see cref="Color"/>/<see cref="HsvColor"/> and raises
/// <see cref="ColorChanged"/>.
/// </summary>
[TemplatePart("PART_Hexagon", typeof(ColorHexagon))]
[TemplatePart("PART_StoreButton", typeof(Button))]
[TemplatePart("PART_RecallButton", typeof(Button))]
[TemplatePart("PART_OptionsWhite", typeof(MenuItem))]
[TemplatePart("PART_SlidersHsv", typeof(MenuItem))]
[TemplatePart("PART_SlidersRgb", typeof(MenuItem))]
[TemplatePart("PART_SlidersBoth", typeof(MenuItem))]
public class ColorChooser : TemplatedControl
{
    public static readonly StyledProperty<Color> ColorProperty =
        AvaloniaProperty.Register<ColorChooser, Color>(nameof(Color), Colors.Blue,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<HsvColor> HsvColorProperty =
        AvaloniaProperty.Register<ColorChooser, HsvColor>(nameof(HsvColor), Colors.Blue.ToHsv(),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<Color> StoredColorProperty =
        AvaloniaProperty.Register<ColorChooser, Color>(nameof(StoredColor), Colors.White,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<ColorSliderSet> VisibleSlidersProperty =
        AvaloniaProperty.Register<ColorChooser, ColorSliderSet>(nameof(VisibleSliders), ColorSliderSet.Hsv);

    // Brush projections of the two colors so the swatches bind without a converter.
    public static readonly StyledProperty<IBrush> ColorBrushProperty =
        AvaloniaProperty.Register<ColorChooser, IBrush>(nameof(ColorBrush), new SolidColorBrush(Colors.Blue));

    public static readonly StyledProperty<IBrush> StoredColorBrushProperty =
        AvaloniaProperty.Register<ColorChooser, IBrush>(nameof(StoredColorBrush), new SolidColorBrush(Colors.White));

    // Component "views" the sliders and numeric fields bind to (TwoWay). H/S/V are 0..1 to
    // match the IRIX readout (e.g., blue hue = 0.667); R/G/B are 0..255.
    public static readonly StyledProperty<double> HueValueProperty =
        AvaloniaProperty.Register<ColorChooser, double>(nameof(HueValue), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> SaturationValueProperty =
        AvaloniaProperty.Register<ColorChooser, double>(nameof(SaturationValue),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> ValueValueProperty =
        AvaloniaProperty.Register<ColorChooser, double>(nameof(ValueValue), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> RedValueProperty =
        AvaloniaProperty.Register<ColorChooser, double>(nameof(RedValue), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> GreenValueProperty =
        AvaloniaProperty.Register<ColorChooser, double>(nameof(GreenValue), defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> BlueValueProperty =
        AvaloniaProperty.Register<ColorChooser, double>(nameof(BlueValue), defaultBindingMode: BindingMode.TwoWay);

    // Canonical model: hue 0..360, saturation 0..1, value 0..1. Held separately from Color so
    // hue/saturation survive a trip through grayscale (S=0 or V=0).
    private double _h = 240, _s = 1, _v = 1;
    private bool _updating;

    private ColorHexagon? _hexagon;
    private Button? _storeButton;
    private Button? _recallButton;
    private MenuItem? _optionsWhite;
    private MenuItem? _slidersHsv;
    private MenuItem? _slidersRgb;
    private MenuItem? _slidersBoth;

    static ColorChooser()
    {
        ColorProperty.Changed.AddClassHandler<ColorChooser>((x, _) => x.OnColorChanged());
        HsvColorProperty.Changed.AddClassHandler<ColorChooser>((x, _) => x.OnHsvColorChanged());
        StoredColorProperty.Changed.AddClassHandler<ColorChooser>((x, _) =>
            x.SetCurrentValue(StoredColorBrushProperty, new SolidColorBrush(x.StoredColor)));
        HueValueProperty.Changed.AddClassHandler<ColorChooser>((x, _) => x.OnHsvFieldChanged());
        SaturationValueProperty.Changed.AddClassHandler<ColorChooser>((x, _) => x.OnHsvFieldChanged());
        ValueValueProperty.Changed.AddClassHandler<ColorChooser>((x, _) => x.OnHsvFieldChanged());
        RedValueProperty.Changed.AddClassHandler<ColorChooser>((x, _) => x.OnRgbFieldChanged());
        GreenValueProperty.Changed.AddClassHandler<ColorChooser>((x, _) => x.OnRgbFieldChanged());
        BlueValueProperty.Changed.AddClassHandler<ColorChooser>((x, _) => x.OnRgbFieldChanged());
    }

    public ColorChooser()
    {
        // Publish the initial model into every view.
        SyncAll(regenerateHexagon: false);
    }

    /// <summary>
    /// Raised whenever the chosen color changes (any source).
    /// </summary>
    public event EventHandler<ColorChangedEventArgs>? ColorChanged;

    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public HsvColor HsvColor
    {
        get => GetValue(HsvColorProperty);
        set => SetValue(HsvColorProperty, value);
    }

    public Color StoredColor
    {
        get => GetValue(StoredColorProperty);
        set => SetValue(StoredColorProperty, value);
    }

    public ColorSliderSet VisibleSliders
    {
        get => GetValue(VisibleSlidersProperty);
        set => SetValue(VisibleSlidersProperty, value);
    }

    public IBrush ColorBrush
    {
        get => GetValue(ColorBrushProperty);
        set => SetValue(ColorBrushProperty, value);
    }

    public IBrush StoredColorBrush
    {
        get => GetValue(StoredColorBrushProperty);
        set => SetValue(StoredColorBrushProperty, value);
    }

    public double HueValue
    {
        get => GetValue(HueValueProperty);
        set => SetValue(HueValueProperty, value);
    }

    public double SaturationValue
    {
        get => GetValue(SaturationValueProperty);
        set => SetValue(SaturationValueProperty, value);
    }

    public double ValueValue
    {
        get => GetValue(ValueValueProperty);
        set => SetValue(ValueValueProperty, value);
    }

    public double RedValue
    {
        get => GetValue(RedValueProperty);
        set => SetValue(RedValueProperty, value);
    }

    public double GreenValue
    {
        get => GetValue(GreenValueProperty);
        set => SetValue(GreenValueProperty, value);
    }

    public double BlueValue
    {
        get => GetValue(BlueValueProperty);
        set => SetValue(BlueValueProperty, value);
    }

    /// <summary>
    /// Canonical hue, 0..360. Read by the hexagon and component sliders.
    /// </summary>
    internal double Hue => _h;

    /// <summary>
    /// Canonical saturation, 0..1.
    /// </summary>
    internal double Saturation => _s;

    /// <summary>
    /// Canonical value/brightness, 0..1.
    /// </summary>
    internal double Brightness => _v;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_storeButton is { } sb) sb.Click -= OnStore;
        if (_recallButton is { } rb) rb.Click -= OnRecall;
        if (_optionsWhite is { } ow) ow.Click -= OnWhite;
        if (_slidersHsv is { } sh) sh.Click -= OnSlidersHsv;
        if (_slidersRgb is { } sr) sr.Click -= OnSlidersRgb;
        if (_slidersBoth is { } sbo) sbo.Click -= OnSlidersBoth;

        _hexagon = e.NameScope.Find<ColorHexagon>("PART_Hexagon");
        if (_hexagon is { } hex)
            hex.Owner = this;

        _storeButton = e.NameScope.Find<Button>("PART_StoreButton");
        if (_storeButton is { } s) s.Click += OnStore;

        _recallButton = e.NameScope.Find<Button>("PART_RecallButton");
        if (_recallButton is { } r) r.Click += OnRecall;

        _optionsWhite = e.NameScope.Find<MenuItem>("PART_OptionsWhite");
        if (_optionsWhite is { } o) o.Click += OnWhite;

        _slidersHsv = e.NameScope.Find<MenuItem>("PART_SlidersHsv");
        if (_slidersHsv is { } a) a.Click += OnSlidersHsv;
        _slidersRgb = e.NameScope.Find<MenuItem>("PART_SlidersRgb");
        if (_slidersRgb is { } b) b.Click += OnSlidersRgb;
        _slidersBoth = e.NameScope.Find<MenuItem>("PART_SlidersBoth");
        if (_slidersBoth is { } c) c.Click += OnSlidersBoth;

        SyncAll(regenerateHexagon: true);
    }

    /// <summary>
    /// Sets the canonical HSV from a drag on the hexagon (hue 0..360, sat 0..1).
    /// </summary>
    internal void SetHueSaturation(double hue, double saturation)
    {
        if (_updating)
            return;
        _h = ((hue % 360) + 360) % 360;
        _s = Math.Clamp(saturation, 0, 1);
        SyncAll(regenerateHexagon: false);
    }

    private void OnColorChanged()
    {
        if (_updating)
            return;
        var hsv = Color.ToHsv();
        _v = hsv.V;
        _s = hsv.S;
        if (_s > 1e-6 && _v > 1e-6)
            _h = hsv.H;
        SyncAll(regenerateHexagon: true);
    }

    private void OnHsvColorChanged()
    {
        if (_updating)
            return;
        var hsv = HsvColor;
        _v = hsv.V;
        _s = hsv.S;
        if (_s > 1e-6 && _v > 1e-6)
            _h = hsv.H;
        SyncAll(regenerateHexagon: true);
    }

    private void OnHsvFieldChanged()
    {
        if (_updating)
            return;
        _h = Math.Clamp(HueValue, 0, 1) * 360;
        _s = Math.Clamp(SaturationValue, 0, 1);
        _v = Math.Clamp(ValueValue, 0, 1);
        SyncAll(regenerateHexagon: true);
    }

    private void OnRgbFieldChanged()
    {
        if (_updating)
            return;
        var rgb = Color.FromRgb(ToByte(RedValue), ToByte(GreenValue), ToByte(BlueValue));
        var hsv = rgb.ToHsv();
        _v = hsv.V;
        _s = hsv.S;
        if (_s > 1e-6 && _v > 1e-6)
            _h = hsv.H;
        SyncAll(regenerateHexagon: true);
    }

    /// <summary>
    /// Pushes the canonical model into every bindable view (guarded against recursion).
    /// </summary>
    private void SyncAll(bool regenerateHexagon)
    {
        if (_updating)
            return;
        _updating = true;
        try
        {
            var rgb = HsvColor.FromHsv(_h, _s, _v).ToRgb();
            SetCurrentValue(ColorProperty, rgb);
            SetCurrentValue(ColorBrushProperty, new SolidColorBrush(rgb));
            SetCurrentValue(HsvColorProperty, new HsvColor(1, _h, _s, _v));
            SetCurrentValue(HueValueProperty, _h / 360.0);
            SetCurrentValue(SaturationValueProperty, _s);
            SetCurrentValue(ValueValueProperty, _v);
            SetCurrentValue(RedValueProperty, rgb.R);
            SetCurrentValue(GreenValueProperty, rgb.G);
            SetCurrentValue(BlueValueProperty, rgb.B);
        }
        finally
        {
            _updating = false;
        }

        if (regenerateHexagon)
            _hexagon?.InvalidateSpectrum();
        _hexagon?.InvalidateVisual();
        ColorChanged?.Invoke(this, new ColorChangedEventArgs(Color));
    }

    private void OnStore(object? sender, RoutedEventArgs e) => SetCurrentValue(StoredColorProperty, Color);

    private void OnRecall(object? sender, RoutedEventArgs e) => SetCurrentValue(ColorProperty, StoredColor);

    private void OnWhite(object? sender, RoutedEventArgs e) => SetCurrentValue(ColorProperty, Colors.White);

    private void OnSlidersHsv(object? sender, RoutedEventArgs e) =>
        SetCurrentValue(VisibleSlidersProperty, ColorSliderSet.Hsv);

    private void OnSlidersRgb(object? sender, RoutedEventArgs e) =>
        SetCurrentValue(VisibleSlidersProperty, ColorSliderSet.Rgb);

    private void OnSlidersBoth(object? sender, RoutedEventArgs e) =>
        SetCurrentValue(VisibleSlidersProperty, ColorSliderSet.Both);

    private static byte ToByte(double v) => (byte)Math.Clamp(Math.Round(v), 0, 255);
}
