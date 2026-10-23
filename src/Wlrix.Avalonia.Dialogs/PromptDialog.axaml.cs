using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Wlrix.Avalonia.Dialogs;

/// <summary>Asks for a single line of text.</summary>
/// <remarks>
/// Moved here from wlRIX Files once the icon catalog wanted one too, for naming its pages.
/// Like <see cref="MessageDialog"/>, this library has no strings of its own to translate, so
/// the caller passes the button text in its own language.
/// </remarks>
public partial class PromptDialog : Window
{
    /// <summary>Asking for a name is a question, and sounds like one.</summary>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        DialogSound.Play(DialogType.Question);
    }

    public PromptDialog()
    {
        InitializeComponent();
        Accept.Click += (_, _) => Close(Input.Text);
        Reject.Click += (_, _) => Close(null);
    }

    /// <summary>Asks for a name, returning null if the user canceled or left it blank.</summary>
    /// <param name="owner">The window the dialog is modal to.</param>
    /// <param name="title">The dialog's title.</param>
    /// <param name="prompt">The text above the field.</param>
    /// <param name="initial">What the field starts with, all of it selected.</param>
    /// <param name="selectExtension">
    /// Whether the extension is part of the initial selection. False when renaming a file, so
    /// typing replaces the stem and leaves <c>.txt</c> alone -- which is what a rename almost
    /// always means.
    /// </param>
    /// <param name="okText">OK button text. When null, the default ("OK") is used.</param>
    /// <param name="cancelText">Cancel button text. When null, the default ("Cancel") is used.</param>
    public static async Task<string?> ShowAsync(
        Window owner, string title, string prompt, string initial = "", bool selectExtension = true,
        string? okText = null, string? cancelText = null)
    {
        var dialog = new PromptDialog { Title = title };
        dialog.Prompt.Text = prompt;
        dialog.Input.Text = initial;
        if (okText is not null)
            dialog.Accept.Content = okText;
        if (cancelText is not null)
            dialog.Reject.Content = cancelText;

        dialog.Opened += (_, _) =>
        {
            dialog.Input.Focus();
            var dot = initial.LastIndexOf('.');
            var end = !selectExtension && dot > 0 ? dot : initial.Length;
            dialog.Input.SelectionStart = 0;
            dialog.Input.SelectionEnd = end;
        };

        var answer = await dialog.ShowDialog<string?>(owner);
        return string.IsNullOrWhiteSpace(answer) ? null : answer.Trim();
    }
}
