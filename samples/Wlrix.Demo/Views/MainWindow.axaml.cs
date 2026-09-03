using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Wlrix.Avalonia;
using Wlrix.Avalonia.Dialogs;

namespace Wlrix.Demo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // The schemes come from the generated catalog, so a new palette JSON turns up here
        // without this file being touched.
        SchemeBox.ItemsSource = WlrixSchemes.All;
        SchemeBox.DisplayMemberBinding = new Binding(nameof(WlrixScheme.Name));
        SchemeBox.SelectedItem = WlrixSchemes.Default;
        SchemeBox.SelectionChanged += (_, _) =>
        {
            if (SchemeBox.SelectedItem is WlrixScheme scheme)
                ApplyScheme(scheme);
        };

        // Error: just an acknowledgment — OK only.
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

    // The theme swaps the one dictionary it owns and leaves Application.Resources alone, so
    // this is the same call a real application makes -- Wlrix.Toolchest merges its own menu
    // dictionary there and would have lost it to the Clear() this used to do.
    private static void ApplyScheme(WlrixScheme scheme)
    {
        if (WlrixTheme.From(Application.Current) is { } theme)
            theme.Scheme = scheme.Id;
    }
}
