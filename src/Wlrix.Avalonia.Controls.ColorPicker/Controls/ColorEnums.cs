using Avalonia.Media;

namespace Wlrix.Avalonia.Controls.ColorPicker.Controls;

/// <summary>
/// Which set of component sliders/fields the chooser shows.
/// </summary>
public enum ColorSliderSet
{
    /// <summary>
    /// Hue, Saturation, and Value rows only.
    /// </summary>
    Hsv,

    /// <summary>
    /// Red, Green, and Blue rows only.
    /// </summary>
    Rgb,

    /// <summary>
    /// All six rows (HSV and RGB).
    /// </summary>
    Both
}

/// <summary>
/// The color component a <see cref="ColorComponentSlider"/> edits.
/// </summary>
public enum ColorComponent
{
    Hue,
    Saturation,
    Value,
    Red,
    Green,
    Blue
}

/// <summary>
/// Carries the color after a committed change to <see cref="ColorChooser"/>.
/// </summary>
public sealed class ColorChangedEventArgs : EventArgs
{
    public ColorChangedEventArgs(Color color) => Color = color;

    public Color Color { get; }
}
