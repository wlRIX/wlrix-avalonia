# Wlrix.Avalonia

A full Avalonia **application theme** that gives applications the chiselled, 3D
look of **SGI IRIX** (IRIS IM / Motif) desktop applications — raised and inset
bevels, hard outlines, and period-correct color schemes. The default scheme is
the classic **Indigo Magic** blue-gray.

It is self-contained: `WlrixTheme` is a complete theme and does **not** depend on
`FluentTheme` (or any other base theme). Add it alone to `Application.Styles`.

IRIX baked the same scheme for three display gammas. All three ship; **gamma 1.7**
is the default, as it reads closest to the original on an sRGB display.

## Projects

| Path                                      | Description                                                                                                                  |
|-------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|
| `src/Wlrix.Avalonia`                      | The theme: the `BevelBorder` primitive, the `PathBar` control, control templates, color schemes, and the `WlrixTheme` style. |
| `src/Wlrix.Avalonia.Controls.ColorPicker` | A separate control library with the IRIX `ColorChooser` (hexagon color picker) and its `ColorPickerTheme`.                   |
| `src/Wlrix.Avalonia.Dialogs`              | Reusable SGI/IRIX dialog windows — the `MessageDialog` (Error / Question / Warning / Information).                           |
| `samples/Wlrix.Demo`                      | A gallery app that shows the controls and lets you switch color schemes at runtime.                                          |
| `tools/shot`                              | A headless (display-free) Skia renderer used to regenerate the screenshots above.                                            |

Built against Avalonia 12 / .NET 10.

## Usage

Add the theme to your `App.axaml` styles:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:sgi="https://vic485.xyz/Wlrix.Avalonia">
    <Application.Styles>
        <sgi:WlrixTheme />
    </Application.Styles>
</Application>
```

`WlrixTheme` is the only style you need — there is no `FluentTheme` underneath.
The following controls are templated and pick up the SGI look:

- Buttons: `Button`, `RepeatButton`, `ToggleButton` (plus an `SgiLedButton`
  `ToggleButton` variant with a sunken indicator light)
- Selection: `CheckBox`, `RadioButton`, `ComboBox`, `ListBox`, `TreeView`
- Text & numeric: `TextBox`, `NumericUpDown` (`ButtonSpinner`)
- Range: `Slider`, `ProgressBar`, `ScrollBar`
- Containers: `ScrollViewer`, `TabControl`, `Expander`, `GridSplitter`
- Menus & chrome: `Menu`/`MenuItem`, `ContextMenu`, `ToolTip`
- Custom: `BevelBorder`, `PathBar`, `Thumbwheel`

The date/calendar family (`Calendar`, `DatePicker`, `TimePicker`) is not yet
themed; an app that needs those can merge `FluentTheme` *before* `WlrixTheme` to
fill the gaps.

## The `PathBar` control

`Wlrix.Avalonia.Controls.PathBar` is a custom breadcrumb path bar for file
managers: an editable path field with small raised "nub" tabs above it, one
centered over each directory component. Clicking a nub navigates to that
component — it updates `Path`, raises `PathSelected`, and (optionally) invokes
`Command` with the selected path.

```xml
<sgi:PathBar Path="/usr/people/vic485"
             PathSelected="OnPathSelected" />
```

```csharp
private void OnPathSelected(object? sender, PathSelectedEventArgs e)
    => LoadDirectory(e.Path);   // e.Path is the full path up to the clicked nub
```

Navigation is raised both by clicking a nub and by editing the field and pressing
**Enter**. `Separator` (default `/`) controls how the path is split.

## The `Thumbwheel` control

`Wlrix.Avalonia.Controls.Thumbwheel` is the IRIX thumbwheel: the user changes
the value by clicking and dragging a ridged wheel surface, as if rolling the edge
of a cylinder. It derives from `RangeBase`, so `Minimum`/`Maximum`/`Value`/
`SmallChange`/`LargeChange` and `ValueChanged` work as on `Slider`.

```xml
<!-- Bounded (e.g. zoom), vertical, with a home button that resets to 50 -->
<sgi:Thumbwheel Orientation="Vertical" Minimum="0" Maximum="100" Value="40"
                ShowHomeButton="True" HomeValue="50" Height="110" />

<!-- Infinite rotation (e.g. 3D rotate), horizontal -->
<sgi:Thumbwheel Orientation="Horizontal" IsContinuous="True" Width="160" />
```

Key properties: `Orientation` (Vertical/Horizontal), `IsContinuous` (unbounded
spin via an internal phase accumulator — `Value` is not clamped), `ShowHomeButton`

+ `HomeValue`, `IsDirectionReversed`, `Sensitivity` (value units per drag pixel),
  `RidgeCount`, `RidgePeriod` (value units per ridge step). Dragging along the wheel
  axis or using the mouse wheel changes the value; the ridges scroll and crowd
  toward the edges for a cylinder-edge look.

## The `ColorChooser` control

`Wlrix.Avalonia.Controls.ColorPicker.Controls.ColorChooser` is the IRIX "Color
Browser": a hue/saturation **hexagon** (drag the selector circle), current +
stored swatches with copy arrows, gradient component sliders and numeric fields
for H/S/V and R/G/B, an **Options** menu (find white) and a **Sliders** menu
(HSV / RGB / Both). It ships in a **separate project**
(`Wlrix.Avalonia.Controls.ColorPicker`) the way Avalonia keeps its color picker
out of core, and reuses the SGI chrome and scheme brushes.

It needs its own styles in addition to `WlrixTheme`:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:sgi="https://vic485.xyz/Wlrix.Avalonia">
    <Application.Styles>
        <sgi:WlrixTheme />
        <sgi:ColorPickerTheme />   <!-- adds the ColorChooser template -->
    </Application.Styles>
</Application>
```

```xml
<sgi:ColorChooser Color="#3030D0" ColorChanged="OnColorChanged" />
```

`Color` (RGB) and `HsvColor` are TwoWay; `StoredColor` holds the reference
swatch; `VisibleSliders` (`Hsv`/`Rgb`/`Both`) chooses which rows show. OK/Cancel/
Help are intentionally left to the host dialog — the control raises `ColorChanged`
and exposes the color properties rather than owning windowing.

## Message dialogs

`Wlrix.Avalonia.Dialogs.MessageDialog` is a reusable SGI/IRIX message window: an
icon, a message in a light non-beveled frame, and a configurable button row. One
`DialogType` (`Error` / `Critical` / `Question` / `Warning` / `Information`)
selects the icon and a default title. The caller specifies the type and width; the
height auto-fits the (wrapped) message.

```csharp
// Default: OK + Cancel. Help appears only when an onHelp handler is supplied
// (a Help button with no handler would do nothing).
var result = await MessageDialog.ShowAsync(
    owner, DialogType.Warning, "Default warning message", width: 360,
    onHelp: (_, _) => ShowHelp());   // Help keeps the dialog open
// result is DialogResult.Ok or DialogResult.Cancel (Esc = Cancel)

// Pick the button set explicitly, e.g. an error pop-up with just OK:
await MessageDialog.ShowAsync(owner, DialogType.Error, "Disk full",
    buttons: DialogButtons.Ok);
```

`Buttons` is a `[Flags]` `DialogButtons` enum (`Ok` / `Cancel` / `Help`, with
`OkCancel` / `OkCancelHelp` shortcuts); `ShowAsync` adds `Help` when a handler is
given and strips it when not.

For localization, the title and button labels are overridable — pass `title`,
`okText`, `cancelText`, `helpText` to `ShowAsync` (or set `Title` / `OkText` /
`CancelText` / `HelpText` on the dialog). Any unset value falls back to the
type-derived title and the English defaults:

```csharp
await MessageDialog.ShowAsync(owner, DialogType.Warning, "Datei nicht gefunden",
    title: "Achtung", okText: "Ja", cancelText: "Nein", helpText: "Hilfe",
    onHelp: (_, _) => ShowHelp());
```

`Title`, `Message`, `OkText`, `CancelText`, `HelpText` and `DialogType` are all
styled properties, so instead of passing strings you can construct the dialog and
**bind** them to a localization source (e.g. a `ResourceManager` exposed through a
view model or a markup extension). This keeps the labels reactive to a language
change and is the natural fit for an MVVM app:

```xml
<!-- A localized dialog defined in XAML; {x:Static} or a loc markup extension
     supplies each string. -->
<dlg:MessageDialog xmlns:dlg="using:Wlrix.Avalonia.Dialogs"
                   DialogType="Warning"
                   Title="{x:Static res:Strings.FileNotFoundTitle}"
                   Message="{x:Static res:Strings.FileNotFoundBody}"
                   OkText="{x:Static res:Strings.Yes}"
                   CancelText="{x:Static res:Strings.No}"
                   HelpText="{x:Static res:Strings.Help}"
                   Buttons="OkCancelHelp" />
```

```csharp
// Or in code, binding to a view model that wraps the resource lookups:
var dialog = new MessageDialog { DialogType = DialogType.Warning };
dialog.Bind(MessageDialog.TitleProperty,   new Binding(nameof(vm.WarningTitle)));
dialog.Bind(MessageDialog.MessageProperty, new Binding(nameof(vm.WarningBody)));
dialog.Bind(MessageDialog.OkTextProperty,  new Binding(nameof(vm.YesLabel)));
dialog.DataContext = vm;
var result = await dialog.ShowDialog<DialogResult>(owner);
```

A bound `Title` is treated as caller-set, so it overrides the `DialogType`
default just like an explicit string. `ShowAsync` is the convenience layer over
these properties — use it for one-off calls and bindings for fully localized,
data-driven dialogs.

The dialogs are plain `Window`s that inherit the app-level `WlrixTheme`, so no extra
styles are required. Icons live under `Assets/Icons/{Type}.png`; until they are
added the icon slot stays empty (its space is reserved so layout doesn't shift).

## File dialogs

`WlrixTheme` templates Avalonia's `ManagedFileChooser`, so `TopLevel.StorageProvider`
opens a dialog in the wlRIX look with nothing extra to reference — the control lives in
`Avalonia.Dialogs.dll`, which ships inside the `Avalonia` package.

This is not a nicety. wlRIX has no FileChooser portal, and the managed chooser is
templated only by the Fluent and Simple themes; since this theme includes no base theme,
an untemplated chooser opens as an **empty window**, with no error and nothing in the log.
`ManagedFileChooserOverwritePrompt` is templated for the same reason — a save over an
existing file would otherwise raise a blank box.

The nub row across the top is the `PathBar` above, so clicking a path component navigates.

To open the wlRIX picker even where a working portal exists — a GTK dialog looks out of
place in this desktop — force the managed one at startup:

```csharp
AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .UseWayland()
    .UseManagedSystemDialogs()   // Avalonia.Dialogs
```

The chooser's own strings (`OK`, `Cancel`, the column headings) are declared alongside the
theme, because Fluent keeps its copies in a dictionary nothing here merges in. They are
American English only; there is no satellite resource mechanism in a theme dictionary.

## Color schemes

> **`Schemes/*.axaml` is generated — do not edit it.** The colors come from
> `wlrix-assets/palette/*.json`, which both this theme and the Rust compositor
> read, so an app's panel and the desktop behind it are the same gray by
> construction. Edit the palette JSON and run `just palette` from `wlrix-epoch`.

A scheme is a `ResourceDictionary` of `Color` keys (`SgiFaceColor`,
`SgiPanelColor`, `SgiTopShadowColor`, …). The matching `SolidColorBrush` keys
live once in `Schemes/Brushes.axaml` and point at the colors through
`DynamicResource`, so swapping the scheme repoints every brush.

The values are transcribed from the IRIX 6.5 X11 scheme files, and roles map
onto them the way `Base/Base` bound them to Motif widgets:

| Key                     | IRIX name                   | Gamma 1.7             |
|-------------------------|-----------------------------|-----------------------|
| `SgiPanel`              | `BasicBackground`           | `#c1c1c1`             |
| `SgiFace`               | `ButtonBackground`          | `#999999`             |
| `SgiTextBackground`     | `TextFieldBackground`       | `#b98e8e`             |
| `SgiViewBackground`     | `DrawingAreaBackground`     | `#608189`             |
| `SgiCheck` / `SgiRadio` | `CheckColor` / `RadioColor` | `#ff0000` / `#0000ff` |

Buttons sit deliberately darker than the panel behind them; that contrast is a
signature of the look.

IRIX stored no bevel shadows — Motif derived them per widget — so `SgiTopShadow`
and `SgiBottomShadow` are computed by the generator rather than authored.

### Switching at runtime

Because every control binds with `DynamicResource`, the scheme can change while
the app is running. Set `WlrixTheme.Scheme` to a scheme id:

```csharp
if (WlrixTheme.From(Application.Current) is { } theme)
    theme.Scheme = "gotham";
```

That swaps the one dictionary the theme owns and leaves everything else alone —
including anything the app has merged into `Application.Resources` itself, which
most wlRIX apps do for the controls this theme does not cover. An id naming no
shipped scheme falls back to the default rather than throwing.

**Do not merge a scheme into `Application.Resources`.** It is consulted *before*
a style's own resources, so a scheme there shadows the theme's and
`WlrixTheme.Scheme` appears to do nothing. The theme already carries the default.

The schemes themselves come from the generated catalog, so a picker lists
whatever this build ships rather than a copy of the list:

```csharp
foreach (var scheme in WlrixSchemes.All)
    Console.WriteLine($"{scheme.Id}\t{scheme.Name}\t{(scheme.IsDark ? "dark" : "light")}");

var current = WlrixSchemes.ById(id) ?? WlrixSchemes.Default;
```

`Schemes/SchemeCatalog.g.cs` is generated alongside the dictionaries by
`just palette`, from the same JSON — so adding a palette file adds an entry to
every scheme picker with no other edit. The demo's dropdown and wlRIX's Color
Schemes panel are both built from it.

## The `BevelBorder` primitive

`Wlrix.Avalonia.Controls.BevelBorder` is a single-child decorator that
paints pixel-snapped 3D edges. It is the building block of every templated
control and is reusable directly in your own layouts:

```xml
<sgi:BevelBorder Bevel="Raised" BevelThickness="2" OuterLineThickness="1"
                 Background="{DynamicResource SgiFace}"
                 LightBrush="{DynamicResource SgiTopShadow}"
                 DarkBrush="{DynamicResource SgiBottomShadow}"
                 OuterLineBrush="{DynamicResource SgiOuterLine}"
                 Padding="8">
    <!-- content -->
</sgi:BevelBorder>
```

`Bevel` supports `Raised`, `Sunken`, `Etched`, `Ridge`, and `None`.

## Building

```
dotnet build Wlrix.Avalonia.slnx
dotnet run --project samples/Wlrix.Demo
```

## Regenerating the screenshots

```
dotnet run --project tools/shot/shot.csproj -- artifacts
```
