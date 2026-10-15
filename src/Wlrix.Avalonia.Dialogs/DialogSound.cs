using System.Net.Sockets;
using System.Text;

namespace Wlrix.Avalonia.Dialogs;

/// <summary>
/// The sound a message dialog makes as it opens, played by <c>wlrix-sound</c>.
/// </summary>
/// <remarks>
/// <para>
/// IRIX's dialogs each had a sound: Error, Fatal error, Question, Information, and the File
/// arrival warning for warnings. The wlRIX sound theme has them under the freedesktop names below,
/// and the System &amp; Desktop Sounds panel's <i>System Alerts Sounds</i> box switches them.
/// </para>
/// <para>
/// The request is the one every wlRIX component sends: one datagram, <c>play &lt;name&gt;</c>,
/// to <c>$XDG_RUNTIME_DIR/wlrix-sound.sock</c>. Written out here rather than borrowed from
/// <c>Wlrix.Common.SystemSound</c> in wlrix-apps, which this library cannot reference; kept in
/// step with it, and with <c>wlrix-sound/src/client.rs</c>, by hand. A datagram returns at once
/// whatever the daemon is doing, so a dialog never waits on its own sound, and every failure --
/// no daemon, no runtime directory, not on Linux -- is silence.
/// </para>
/// </remarks>
internal static class DialogSound
{
    private const string SocketName = "wlrix-sound.sock";

    /// <summary>The sound name for each kind of dialog.</summary>
    public static string NameFor(DialogType type) => type switch
    {
        DialogType.Error => "dialog-error",
        DialogType.Critical => "dialog-error-serious",
        DialogType.Question => "dialog-question",
        DialogType.Warning => "dialog-warning",
        _ => "dialog-information",
    };

    /// <summary>Ask for the sound of <paramref name="type"/>. Returns at once; never throws.</summary>
    public static void Play(DialogType type)
    {
        var runtime = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
        if (string.IsNullOrEmpty(runtime) || !Path.IsPathRooted(runtime))
            return;
        try
        {
            using var socket = new Socket(AddressFamily.Unix, SocketType.Dgram, ProtocolType.Unspecified)
            {
                Blocking = false,
            };
            socket.SendTo(Encoding.UTF8.GetBytes("play " + NameFor(type)),
                new UnixDomainSocketEndPoint(Path.Combine(runtime, SocketName)));
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException or PlatformNotSupportedException)
        {
            // No daemon listening: this dialog opens quietly.
        }
    }
}
