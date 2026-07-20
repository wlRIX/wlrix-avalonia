using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Wlrix.Avalonia.Converters;

/// <summary>
/// Converts a <see cref="Avalonia.Controls.TreeViewItem"/> nesting level (int) into a left
/// margin <see cref="Thickness"/>, indenting each level by <see cref="Indent"/> pixels.
/// </summary>
public sealed class IndentConverter : IValueConverter
{
    /// <summary>
    /// Pixels of left indent per nesting level.
    /// </summary>
    public double Indent { get; set; } = 16;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var level = value is int i ? i : 0;
        return new Thickness(level * Indent, 0, 0, 0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
