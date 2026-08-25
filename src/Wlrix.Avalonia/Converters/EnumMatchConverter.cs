using System.Globalization;
using Avalonia.Data.Converters;

namespace Wlrix.Avalonia.Converters;

/// <summary>
/// True when the bound value's name matches the converter parameter, for showing one of several
/// overlaid glyphs from a single enum.
/// </summary>
/// <remarks>
/// Compiled bindings have no equality operator, so a template that wants "this shape when the
/// item is a folder, that one when it is a file" needs a converter or a view model property per
/// case. This is the converter, kept deliberately stringly-typed so one instance serves every
/// enum rather than needing a generic per type.
/// </remarks>
public sealed class EnumMatchConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not null
        && parameter is string expected
        && string.Equals(value.ToString(), expected, StringComparison.Ordinal);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
