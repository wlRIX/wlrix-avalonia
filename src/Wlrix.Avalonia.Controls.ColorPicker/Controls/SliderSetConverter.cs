using System.Globalization;
using Avalonia.Data.Converters;

namespace Wlrix.Avalonia.Controls.ColorPicker.Controls;

/// <summary>
/// Returns true when a row should be visible for the current <see cref="ColorSliderSet"/>.
/// The converter parameter is "Hsv" or "Rgb" naming which family the row belongs to; HSV rows
/// show for <see cref="ColorSliderSet.Hsv"/>/<see cref="ColorSliderSet.Both"/> and RGB rows for
/// <see cref="ColorSliderSet.Rgb"/>/<see cref="ColorSliderSet.Both"/>.
/// </summary>
public sealed class SliderSetConverter : IValueConverter
{
    public static readonly SliderSetConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ColorSliderSet set || parameter is not string family)
            return false;

        return family switch
        {
            "Hsv" => set is ColorSliderSet.Hsv or ColorSliderSet.Both,
            "Rgb" => set is ColorSliderSet.Rgb or ColorSliderSet.Both,
            _ => false
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
