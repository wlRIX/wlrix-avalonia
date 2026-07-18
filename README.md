# wlrix-avalonia

The wlRIX Avalonia theme: an Avalonia control theme/library implementing the
IRIX **Indigo Magic** visual language (chiseled bevels, the Scheme colour
palette, Motif-flavoured widgets). Consumed by the apps in `wlrix-apps` and
packaged as a NuGet (`Wlrix.Avalonia`).

- **Language:** C# / Avalonia
- **License:** MIT (freely reusable as a library)
- **Palette / assets source:** `wlrix-assets`

## Status

Scaffold. `Wlrix.Avalonia` is a plain class library so it builds without NuGet
access; Avalonia package references and the actual theme resources are added
when the existing theme is imported (see the commented block in the `.csproj`).

## Next steps

1. Import the existing Avalonia theme into `src/Wlrix.Avalonia`.
2. Add the `Avalonia` package references and expose the theme as an
   `Avalonia.Styling.Styles` / `IStyle` resource dictionary.
3. Pull colours from the shared `wlrix-assets` palette so Rust and C# sides match.
4. Publish as the `Wlrix.Avalonia` NuGet package for `wlrix-apps` to consume.

## Build

```sh
dotnet build
```
