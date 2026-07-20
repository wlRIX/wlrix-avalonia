using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace Wlrix.Avalonia.Controls;

/// <summary>
/// An SGI / IRIX thumbwheel: the user changes <see cref="RangeBase.Value"/> by clicking and
/// dragging a ridged wheel surface, as if rolling the exposed edge of a cylinder. It works in
/// a bounded range (the default, e.g., zoom) or, when <see cref="IsContinuous"/> is set, an
/// infinite range (e.g., rotating a 3D object). An optional "home button"
/// (<see cref="ShowHomeButton"/>) resets the value to <see cref="HomeValue"/>. Both vertical
/// and horizontal orientations are supported.
/// </summary>
[TemplatePart("PART_Wheel", typeof(ThumbwheelSurface))]
[TemplatePart("PART_HomeButton", typeof(Button))]
[PseudoClasses(":horizontal", ":vertical")]
public class Thumbwheel : RangeBase
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Thumbwheel, Orientation>(nameof(Orientation), Orientation.Vertical);

    public static readonly StyledProperty<bool> IsDirectionReversedProperty =
        AvaloniaProperty.Register<Thumbwheel, bool>(nameof(IsDirectionReversed));

    /// <summary>When true the value is not clamped to the range and the wheel spins freely.</summary>
    public static readonly StyledProperty<bool> IsContinuousProperty =
        AvaloniaProperty.Register<Thumbwheel, bool>(nameof(IsContinuous));

    public static readonly StyledProperty<bool> ShowHomeButtonProperty =
        AvaloniaProperty.Register<Thumbwheel, bool>(nameof(ShowHomeButton));

    /// <summary>The value the home button resets to.</summary>
    public static readonly StyledProperty<double> HomeValueProperty =
        AvaloniaProperty.Register<Thumbwheel, double>(nameof(HomeValue));

    /// <summary>Value units changed per device pixel of drag.</summary>
    public static readonly StyledProperty<double> SensitivityProperty =
        AvaloniaProperty.Register<Thumbwheel, double>(nameof(Sensitivity), 0.5);

    /// <summary>Number of ridges across the visible half-turn of the wheel.</summary>
    public static readonly StyledProperty<int> RidgeCountProperty =
        AvaloniaProperty.Register<Thumbwheel, int>(nameof(RidgeCount), 18);

    /// <summary>Value span that corresponds to one ridge step rolling past.</summary>
    public static readonly StyledProperty<double> RidgePeriodProperty =
        AvaloniaProperty.Register<Thumbwheel, double>(nameof(RidgePeriod), 10.0);

    public static readonly StyledProperty<IBrush?> LightBrushProperty =
        AvaloniaProperty.Register<Thumbwheel, IBrush?>(nameof(LightBrush));

    public static readonly StyledProperty<IBrush?> DarkBrushProperty =
        AvaloniaProperty.Register<Thumbwheel, IBrush?>(nameof(DarkBrush));

    private ThumbwheelSurface? _surface;
    private Button? _homeButton;

    static Thumbwheel()
    {
        OrientationProperty.Changed.AddClassHandler<Thumbwheel>((x, _) => x.UpdatePseudoClasses());
        ValueProperty.Changed.AddClassHandler<Thumbwheel>((x, _) => x.OnValueChangedExternally());
    }

    public Thumbwheel()
    {
        UpdatePseudoClasses();
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool IsDirectionReversed
    {
        get => GetValue(IsDirectionReversedProperty);
        set => SetValue(IsDirectionReversedProperty, value);
    }

    public bool IsContinuous
    {
        get => GetValue(IsContinuousProperty);
        set => SetValue(IsContinuousProperty, value);
    }

    public bool ShowHomeButton
    {
        get => GetValue(ShowHomeButtonProperty);
        set => SetValue(ShowHomeButtonProperty, value);
    }

    public double HomeValue
    {
        get => GetValue(HomeValueProperty);
        set => SetValue(HomeValueProperty, value);
    }

    public double Sensitivity
    {
        get => GetValue(SensitivityProperty);
        set => SetValue(SensitivityProperty, value);
    }

    public int RidgeCount
    {
        get => GetValue(RidgeCountProperty);
        set => SetValue(RidgeCountProperty, value);
    }

    public double RidgePeriod
    {
        get => GetValue(RidgePeriodProperty);
        set => SetValue(RidgePeriodProperty, value);
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

    /// <summary>
    /// Phase of the wheel in value units. In continuous mode this accumulates without limit;
    /// in bounded mode it tracks <see cref="RangeBase.Value"/>. The render reads this so both
    /// modes animate identically.
    /// </summary>
    internal double Phase { get; private set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (_homeButton is { } oldButton)
            oldButton.Click -= OnHomeClick;

        _surface = e.NameScope.Find<ThumbwheelSurface>("PART_Wheel");
        if (_surface is { } s)
            s.Owner = this;

        _homeButton = e.NameScope.Find<Button>("PART_HomeButton");
        if (_homeButton is { } b)
            b.Click += OnHomeClick;
    }

    private void OnHomeClick(object? sender, RoutedEventArgs e)
        => SetCurrentValue(ValueProperty, HomeValue);

    private void OnValueChangedExternally()
    {
        // Keep the phase in step with externally-set values in bounded mode so the wheel
        // reflects bindings/home resets. In continuous mode the phase is owned by the drag.
        if (!IsContinuous)
            Phase = Value;
        _surface?.InvalidateVisual();
    }

    /// <summary>Applies a drag of <paramref name="pixels"/> along the wheel axis to the value.</summary>
    internal void ApplyDelta(double pixels)
    {
        var delta = pixels * Sensitivity * (IsDirectionReversed ? -1 : 1);
        if (delta == 0)
            return;

        if (IsContinuous)
        {
            Phase += delta;
            _surface?.InvalidateVisual();
        }
        else
        {
            // SetCurrentValue + the Value change handler updates Phase and repaints.
            SetCurrentValue(ValueProperty, Math.Clamp(Value + delta, Minimum, Maximum));
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":vertical", Orientation == Orientation.Vertical);
        PseudoClasses.Set(":horizontal", Orientation == Orientation.Horizontal);
    }
}
