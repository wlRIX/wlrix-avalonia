using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;

namespace Wlrix.Avalonia;

/// <summary>
/// The wlRIX theme for Avalonia: chiselled bevels, hard outlines, and the
/// Classic palette. Add a single instance to <c>Application.Styles</c>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Scheme"/> is how an application changes color scheme, at startup
/// or at any point afterwards. It swaps the one merged dictionary this theme
/// owns and leaves everything else alone — including whatever the application
/// has merged into <c>Application.Resources</c> itself, which several wlRIX
/// apps do for the controls the theme does not cover.
/// </para>
/// <para>
/// Every color in the theme is reached through <c>DynamicResource</c>: the
/// scheme dictionaries define <c>Color</c> keys, <c>Brushes.axaml</c> defines
/// one brush per key pointing at it dynamically, and the control themes bind to
/// the brushes. So replacing the scheme dictionary repoints the whole theme, and
/// there is nothing else to invalidate.
/// </para>
/// </remarks>
public partial class WlrixTheme : Styles
{
    private string _scheme = WlrixSchemes.Default.Id;

    public WlrixTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// The color scheme this theme is drawing in, as a scheme id — see
    /// <see cref="WlrixSchemes"/> for the ids this build ships.
    /// </summary>
    /// <remarks>
    /// An id naming no shipped scheme falls back to <see cref="WlrixSchemes.Default"/>
    /// rather than throwing, and the property then reads back as the default. A
    /// mistyped scheme name in a config file must not leave somebody looking at
    /// an unpainted window, which is the same call every Rust component makes.
    /// </remarks>
    public string Scheme
    {
        get => _scheme;
        set
        {
            var scheme = WlrixSchemes.ById(value) ?? WlrixSchemes.Default;
            if (scheme.Id == _scheme)
                return;
            if (Apply(scheme))
                _scheme = scheme.Id;
        }
    }

    /// <summary>
    /// The theme instance in an application's styles, or <c>null</c> if it has none.
    /// </summary>
    /// <remarks>
    /// A search rather than a static set in the constructor: the headless
    /// screenshot tool builds more than one theme in a process, and a "last one
    /// wins" static would hand back whichever happened to be built last.
    /// </remarks>
    public static WlrixTheme? From(Application? application) =>
        application?.Styles.OfType<WlrixTheme>().FirstOrDefault();

    /// <summary>
    /// Replace the scheme dictionary in place.
    /// </summary>
    /// <remarks>
    /// Returns false when there is no slot to write, which would mean the theme
    /// XAML no longer merges a scheme at all — better to keep drawing in the one
    /// already loaded than to leave the theme half-merged.
    /// </remarks>
    private bool Apply(WlrixScheme scheme)
    {
        if (Resources is not ResourceDictionary resources)
            return false;

        var merged = resources.MergedDictionaries;
        var slot = SchemeSlot(merged);
        if (slot < 0)
            return false;

        // Assigning through the indexer is a remove followed by an add, which is what tells the
        // owning application its resources moved; every DynamicResource in the theme re-resolves
        // from there. Nothing else has to be invalidated by hand.
        merged[slot] = new ResourceInclude((Uri?)null) { Source = new Uri(scheme.ResourceUri) };
        return true;
    }

    /// <summary>
    /// Which merged dictionary is the scheme.
    /// </summary>
    /// <remarks>
    /// By the <see cref="WlrixSchemes.IdKey"/> every scheme dictionary carries, not by index and
    /// not by <c>ResourceInclude.Source</c>. Index would let a reordered include silently swap a
    /// control theme for a scheme; <c>Source</c> does not survive the build, because the XAML
    /// compiler resolves a <c>ResourceInclude</c> written in XAML down to the dictionary itself
    /// and there is no include left to read a URI off.
    /// </remarks>
    private static int SchemeSlot(IList<IResourceProvider> merged)
    {
        for (var i = 0; i < merged.Count; i++)
        {
            if (merged[i].TryGetResource(WlrixSchemes.IdKey, null, out _))
                return i;
        }

        return -1;
    }
}
