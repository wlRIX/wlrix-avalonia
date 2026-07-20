using Avalonia.Metadata;

// Reuse the same XAML namespace URI as the base theme, so consumers keep a single
// xmlns:wlrix prefix for both the theme controls and the color chooser. Avalonia allows
// multiple assemblies to contribute types to one xmlns URI.
[assembly: XmlnsDefinition("https://vic485.xyz/Wlrix.Avalonia", "Wlrix.Avalonia.Controls.ColorPicker")]
[assembly: XmlnsDefinition("https://vic485.xyz/Wlrix.Avalonia", "Wlrix.Avalonia.Controls.ColorPicker.Controls")]
