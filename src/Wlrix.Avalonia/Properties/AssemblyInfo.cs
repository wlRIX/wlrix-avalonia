using Avalonia.Metadata;

// Expose the theme's CLR namespaces under a single, stable XAML namespace URI so consumers
// can write xmlns:wlrix="https://vic485.xyz/Wlrix.Avalonia" instead of a clr-namespace:/assembly
// pair. Both the controls (BevelBorder) and the WlrixTheme style live behind this one prefix.
[assembly: XmlnsDefinition("https://vic485.xyz/Wlrix.Avalonia", "Wlrix.Avalonia")]
[assembly: XmlnsDefinition("https://vic485.xyz/Wlrix.Avalonia", "Wlrix.Avalonia.Controls")]
