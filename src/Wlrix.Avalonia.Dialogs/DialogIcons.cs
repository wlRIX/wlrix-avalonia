using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Wlrix.Avalonia.Dialogs;

/// <summary>
/// Loads the message-dialog icons embedded under <c>Assets/Icons</c>. Returns <c>null</c> when
/// an asset is not present (icons are added in a later step) so the dialog still lays out with
/// an empty icon slot until the real PNGs are dropped in — no code change required then.
/// </summary>
internal static class DialogIcons
{
    public static Bitmap? Load(DialogType type)
    {
        var name = type switch
        {
            DialogType.Error => "Error",
            DialogType.Critical => "Critical",
            DialogType.Question => "Question",
            DialogType.Warning => "Warning",
            DialogType.Information => "Information",
            _ => null
        };
        if (name is null)
            return null;

        var uri = new Uri($"avares://Wlrix.Avalonia.Dialogs/Assets/Icons/{name}.png");
        if (!AssetLoader.Exists(uri))
            return null;

        using var stream = AssetLoader.Open(uri);
        return new Bitmap(stream);
    }
}
