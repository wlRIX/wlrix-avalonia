using Avalonia.Controls;
using Avalonia.Input;

namespace Wlrix.Avalonia.Controls;

/// <summary>
/// A <see cref="TabControl"/> for documents and views that come and go: Ctrl+PageUp and
/// Ctrl+PageDown step through the tabs, wrapping at the ends.
/// </summary>
/// <remarks>
/// It looks and lays out the same as a themed <see cref="TabControl"/> (it is styled as one), so the overlapping
/// tabs, the page that the selected tab opens into and the quick-jump stacks for tabs that
/// do not fit all come from <see cref="NotebookTabPanel"/> and the shared template.
/// </remarks>
public class NotebookTabControl : TabControl
{
    protected override Type StyleKeyOverride => typeof(TabControl);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!e.Handled && e.KeyModifiers == KeyModifiers.Control &&
            e.Key is Key.PageUp or Key.PageDown && ItemCount > 1)
        {
            var step = e.Key == Key.PageDown ? 1 : -1;
            SelectedIndex = ((SelectedIndex + step) % ItemCount + ItemCount) % ItemCount;
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }
}
