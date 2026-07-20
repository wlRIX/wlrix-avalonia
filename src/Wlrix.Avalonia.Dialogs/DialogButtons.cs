using System;

namespace Wlrix.Avalonia.Dialogs;

/// <summary>
/// Which buttons a <see cref="MessageDialog"/> shows. Combine with bitwise OR.
/// </summary>
[Flags]
public enum DialogButtons
{
    None = 0,
    Ok = 1,
    Cancel = 2,
    Help = 4,

    /// <summary>
    /// OK and Cancel (the default pair).
    /// </summary>
    OkCancel = Ok | Cancel,

    /// <summary>
    /// OK, Cancel and Help.
    /// </summary>
    OkCancelHelp = Ok | Cancel | Help
}
