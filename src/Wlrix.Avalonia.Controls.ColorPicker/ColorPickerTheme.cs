using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Wlrix.Avalonia.Controls.ColorPicker;

/// <summary>
/// Styles for the SGI/IRIX color chooser. Add a single instance to <c>Application.Styles</c>
/// after <c>WlrixTheme</c> so the <see cref="Controls.ColorChooser"/> picks up its template and
/// the SGI scheme brushes.
/// </summary>
public partial class ColorPickerTheme : Styles
{
    public ColorPickerTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
