namespace Wlrix.Avalonia;

/// <summary>
/// The 3D bevel styles used by the SGI / IRIS IM (Motif) look.
/// </summary>
public enum WlrixBevel
{
    /// <summary>No bevel is drawn.</summary>
    None,

    /// <summary>
    /// A raised button-like edge: light on the top/left, dark on the bottom/right.
    /// Used for push buttons, tool buttons, and raised panels.
    /// </summary>
    Raised,

    /// <summary>
    /// A sunken edge: dark on the top/left, light on the bottom/right.
    /// Used for pressed buttons, text fields, and scrollbar troughs.
    /// </summary>
    Sunken,

    /// <summary>
    /// A grooved "etched-in" frame (a dark line followed by a light line).
    /// Used for separators and group-box frames.
    /// </summary>
    Etched,

    /// <summary>
    /// A ridged "etched-out" frame (a light line followed by a dark line).
    /// </summary>
    Ridge
}
