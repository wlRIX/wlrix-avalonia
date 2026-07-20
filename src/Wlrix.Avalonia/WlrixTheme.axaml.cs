using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Wlrix.Avalonia;

/// <summary>
/// The wlRIX theme for Avalonia: chiselled bevels, hard outlines, and the
/// Classic palette. Add a single instance to <c>Application.Styles</c>. Color
/// schemes can be overridden by merging one of the dictionaries under
/// <c>avares://Wlrix.Avalonia/Schemes/</c> into <c>Application.Resources</c>.
/// </summary>
public partial class WlrixTheme : Styles
{
    public WlrixTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
