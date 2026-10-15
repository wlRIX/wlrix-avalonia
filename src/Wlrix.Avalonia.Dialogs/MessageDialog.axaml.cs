using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Wlrix.Avalonia.Dialogs;

/// <summary>
/// A reusable SGI / IRIX message dialog: an icon, a framed message, and a configurable button
/// row. The <see cref="DialogType"/> selects the title and icon; <see cref="Buttons"/> chooses
/// which of OK / Cancel / Help appear. The width is given by the caller and the height
/// auto-sizes to the message. OK and Cancel close the dialog with a <see cref="DialogResult"/>;
/// Help raises <see cref="HelpRequested"/> and leaves it open.
/// <para>
/// Opening plays the type's IRIX dialog sound through <c>wlrix-sound</c> (see
/// <see cref="DialogSound"/>), unless <see cref="PlaysSound"/> is turned off. Whether it is
/// heard is the user's to decide, in the Sounds panel.
/// </para>
/// </summary>
public partial class MessageDialog : Window
{
    public static readonly StyledProperty<DialogType> DialogTypeProperty =
        AvaloniaProperty.Register<MessageDialog, DialogType>(nameof(DialogType));

    public static readonly StyledProperty<string?> MessageProperty =
        AvaloniaProperty.Register<MessageDialog, string?>(nameof(Message));

    public static readonly StyledProperty<IImage?> IconImageProperty =
        AvaloniaProperty.Register<MessageDialog, IImage?>(nameof(IconImage));

    public static readonly StyledProperty<string> OkTextProperty =
        AvaloniaProperty.Register<MessageDialog, string>(nameof(OkText), "OK");

    public static readonly StyledProperty<string> CancelTextProperty =
        AvaloniaProperty.Register<MessageDialog, string>(nameof(CancelText), "Cancel");

    public static readonly StyledProperty<string> HelpTextProperty =
        AvaloniaProperty.Register<MessageDialog, string>(nameof(HelpText), "Help");

    /// <summary>
    /// Which buttons are shown. Defaults to OK + Cancel + Help.
    /// </summary>
    public static readonly StyledProperty<DialogButtons> ButtonsProperty =
        AvaloniaProperty.Register<MessageDialog, DialogButtons>(nameof(Buttons), DialogButtons.OkCancelHelp);

    // Per-button visibility derived from Buttons; the template binds to these.
    public static readonly StyledProperty<bool> IsOkVisibleProperty =
        AvaloniaProperty.Register<MessageDialog, bool>(nameof(IsOkVisible), true);

    public static readonly StyledProperty<bool> IsCancelVisibleProperty =
        AvaloniaProperty.Register<MessageDialog, bool>(nameof(IsCancelVisible), true);

    /// <summary>
    /// Whether opening the dialog plays its sound. On by default; off for a dialog that opens
    /// often enough, or in answer to something already noisy enough, that a sound each time
    /// would be one too many.
    /// </summary>
    public static readonly StyledProperty<bool> PlaysSoundProperty =
        AvaloniaProperty.Register<MessageDialog, bool>(nameof(PlaysSound), true);

    public static readonly StyledProperty<bool> IsHelpVisibleProperty =
        AvaloniaProperty.Register<MessageDialog, bool>(nameof(IsHelpVisible), true);

    // Tracks whether the caller set Title themselves (so the type-derived default doesn't
    // clobber a localized title). _applyingDefaultTitle guards our own default assignment.
    private bool _explicitTitle;
    private bool _applyingDefaultTitle;

    static MessageDialog()
    {
        DialogTypeProperty.Changed.AddClassHandler<MessageDialog>((x, _) => x.OnDialogTypeChanged());
        ButtonsProperty.Changed.AddClassHandler<MessageDialog>((x, _) => x.OnButtonsChanged());
    }

    public MessageDialog()
    {
        InitializeComponent();
        OnDialogTypeChanged();
        OnButtonsChanged();

        this.FindControl<Button>("PART_OkButton")!.Click += (_, _) => Close(DialogResult.Ok);
        this.FindControl<Button>("PART_CancelButton")!.Click += (_, _) => Close(DialogResult.Cancel);
        this.FindControl<Button>("PART_HelpButton")!.Click += OnHelpClick;
    }

    /// <summary>
    /// Raised when the user clicks Help. The dialog stays open.
    /// </summary>
    public event EventHandler? HelpRequested;

    public DialogType DialogType
    {
        get => GetValue(DialogTypeProperty);
        set => SetValue(DialogTypeProperty, value);
    }

    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public IImage? IconImage
    {
        get => GetValue(IconImageProperty);
        set => SetValue(IconImageProperty, value);
    }

    public string OkText
    {
        get => GetValue(OkTextProperty);
        set => SetValue(OkTextProperty, value);
    }

    public string CancelText
    {
        get => GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    public string HelpText
    {
        get => GetValue(HelpTextProperty);
        set => SetValue(HelpTextProperty, value);
    }

    public bool PlaysSound
    {
        get => GetValue(PlaysSoundProperty);
        set => SetValue(PlaysSoundProperty, value);
    }

    public DialogButtons Buttons
    {
        get => GetValue(ButtonsProperty);
        set => SetValue(ButtonsProperty, value);
    }

    public bool IsOkVisible
    {
        get => GetValue(IsOkVisibleProperty);
        private set => SetValue(IsOkVisibleProperty, value);
    }

    public bool IsCancelVisible
    {
        get => GetValue(IsCancelVisibleProperty);
        private set => SetValue(IsCancelVisibleProperty, value);
    }

    public bool IsHelpVisible
    {
        get => GetValue(IsHelpVisibleProperty);
        private set => SetValue(IsHelpVisibleProperty, value);
    }

    /// <summary>
    /// Shows a modal message dialog of the given <paramref name="type"/> and returns the result.
    /// </summary>
    /// <param name="owner">The owner window the dialog is centered on and modal to.</param>
    /// <param name="type">Selects the icon and the default title.</param>
    /// <param name="message">The message text (wraps within <paramref name="width"/>).</param>
    /// <param name="width">The fixed dialog width; height auto-sizes to the message.</param>
    /// <param name="onHelp">Optional handler invoked when Help is clicked (dialog stays open).</param>
    /// <param name="buttons">
    /// Which buttons to show. Defaults to OK + Cancel. Help is only shown when it is included
    /// here AND <paramref name="onHelp"/> is supplied (a Help button with no handler would do
    /// nothing), so by default Help appears exactly when a handler is given.
    /// </param>
    /// <param name="title">Window title. When null, the type-derived default is used. Pass a
    /// localized string to override it.</param>
    /// <param name="okText">OK button text. When null, the default ("OK") is used.</param>
    /// <param name="cancelText">Cancel button text. When null, the default ("Cancel") is used.</param>
    /// <param name="helpText">Help button text. When null, the default ("Help") is used.</param>
    /// <param name="playSound">Whether the dialog plays its sound as it opens. See
    /// <see cref="PlaysSound"/>.</param>
    /// <returns>
    /// A task containing the button result. Null if the dialog was closed without a button
    /// press (caller must handle this case).
    /// </returns>
    public static Task<DialogResult?> ShowAsync(Window owner, DialogType type, string message, double width = 360,
        EventHandler? onHelp = null, DialogButtons buttons = DialogButtons.OkCancel, string? title = null,
        string? okText = null, string? cancelText = null, string? helpText = null, bool playSound = true)
    {
        // Help is meaningless without a handler — drop it if none was provided.
        if (onHelp is null)
            buttons &= ~DialogButtons.Help;
        else
            buttons |= DialogButtons.Help;

        var dialog = new MessageDialog
        {
            DialogType = type,
            Message = message,
            Buttons = buttons,
            Width = width,
            MinWidth = width,
            MaxWidth = width,
            PlaysSound = playSound,
        };
        if (title is not null)
            dialog.Title = title;
        if (okText is not null)
            dialog.OkText = okText;
        if (cancelText is not null)
            dialog.CancelText = cancelText;
        if (helpText is not null)
            dialog.HelpText = helpText;
        if (onHelp is not null)
            dialog.HelpRequested += onHelp;

        return dialog.ShowDialog<DialogResult?>(owner);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        // Any Title set that isn't our own default assignment counts as caller-provided and
        // pins the title against the type-derived default.
        if (change.Property == TitleProperty && !_applyingDefaultTitle)
            _explicitTitle = true;
    }

    /// <summary>
    /// The sound goes with the dialog appearing, as IRIX's did, rather than with its creation:
    /// a dialog built and never shown says nothing.
    /// </summary>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (PlaysSound)
            DialogSound.Play(DialogType);
    }

    private void OnHelpClick(object? sender, RoutedEventArgs e) => HelpRequested?.Invoke(this, EventArgs.Empty);

    private void OnButtonsChanged()
    {
        IsOkVisible = Buttons.HasFlag(DialogButtons.Ok);
        IsCancelVisible = Buttons.HasFlag(DialogButtons.Cancel);
        IsHelpVisible = Buttons.HasFlag(DialogButtons.Help);
    }

    private void OnDialogTypeChanged()
    {
        // The type provides a default title; a caller-set Title always wins (e.g. localized).
        if (!_explicitTitle)
        {
            _applyingDefaultTitle = true;
            Title = DialogType switch
            {
                DialogType.Error => "Error",
                DialogType.Critical => "Critical",
                DialogType.Question => "Question",
                DialogType.Warning => "Warning",
                DialogType.Information => "Information",
                _ => "Message"
            };
            _applyingDefaultTitle = false;
        }

        IconImage = DialogIcons.Load(DialogType);
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
