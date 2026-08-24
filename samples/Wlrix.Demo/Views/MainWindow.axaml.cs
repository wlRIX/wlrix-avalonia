using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Wlrix.Avalonia.Dialogs;

namespace Wlrix.Demo.Views;

public partial class MainWindow : Window
{
    private static readonly string[] SchemeFiles =
        { "Classic", "ClassicG10", "ClassicG24", "Gotham" };

    public MainWindow()
    {
        InitializeComponent();
        SchemeBox.SelectionChanged += (_, _) => ApplyScheme(SchemeBox.SelectedIndex);

        // Error: just an acknowledgement — OK only.
        ErrorDialogButton.Click += (_, _) =>
            MessageDialog.ShowAsync(this, DialogType.Error, "Default error message",
                buttons: DialogButtons.Ok);
        // Question: OK/Cancel, plus Help (a handler is supplied, so the Help button appears).
        QuestionDialogButton.Click += (_, _) =>
            MessageDialog.ShowAsync(this, DialogType.Question, "Default question",
                onHelp: (_, _) => { /* show help here */ });
        // Warning: the default OK/Cancel (no Help, since no handler).
        WarningDialogButton.Click += (_, _) =>
            MessageDialog.ShowAsync(this, DialogType.Warning, "Default warning message");
        // Information: OK only.
        InfoDialogButton.Click += (_, _) =>
            MessageDialog.ShowAsync(this, DialogType.Information, "Default message",
                buttons: DialogButtons.Ok);
    }

    private static void ApplyScheme(int index)
    {
        if (index < 0 || Application.Current is not { } app)
            return;

        var uri = new Uri($"avares://Wlrix.Avalonia/Schemes/{SchemeFiles[index]}.axaml");
        var scheme = new ResourceInclude((Uri?)null) { Source = uri };

        // `Clear()` is safe *here* and only here: the demo merges nothing of its own. A real
        // application does -- Wlrix.Toolchest merges its own menu dictionary into
        // Application.Resources -- so a switching API for apps has to replace one owned entry
        // rather than emptying the list. That API does not exist yet.
        app.Resources.MergedDictionaries.Clear();
        app.Resources.MergedDictionaries.Add(scheme);
    }
}
