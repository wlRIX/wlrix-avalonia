using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;

namespace Wlrix.Avalonia.Controls;

/// <summary>
/// Carries the path of the breadcrumb segment that was activated.
/// </summary>
public class PathSelectedEventArgs : RoutedEventArgs
{
    public PathSelectedEventArgs(RoutedEvent routedEvent, string path)
        : base(routedEvent) => Path = path;

    /// <summary>The full path up to and including the clicked segment.</summary>
    public string Path { get; }
}

/// <summary>
/// An SGI / IRIX file-manager style path bar: a row of small raised "nub" tabs sitting
/// above an editable path field. Each nub aligns with the start of a directory component
/// in the text and, when clicked, navigates to that component (raising
/// <see cref="PathSelected"/> and updating <see cref="Path"/>).
/// </summary>
[TemplatePart("PART_TextBox", typeof(TextBox))]
[TemplatePart("PART_Nubs", typeof(Canvas))]
public class PathBar : TemplatedControl
{
    /// <summary>The full path shown in the field and split into breadcrumb segments.</summary>
    public static readonly StyledProperty<string?> PathProperty =
        AvaloniaProperty.Register<PathBar, string?>(
            nameof(Path), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>The directory separator used to split <see cref="Path"/>. Defaults to '/'.</summary>
    public static readonly StyledProperty<char> SeparatorProperty =
        AvaloniaProperty.Register<PathBar, char>(nameof(Separator), '/');

    /// <summary>
    /// Horizontal offset, in pixels, of the text inside the field (border + outline +
    /// left padding). Nubs are positioned relative to this so they line up with the text.
    /// </summary>
    public static readonly StyledProperty<double> TextInsetProperty =
        AvaloniaProperty.Register<PathBar, double>(nameof(TextInset), 7.0);

    /// <summary>Optional command invoked with the selected path string when a nub is clicked.</summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<PathBar, ICommand?>(nameof(Command));

    /// <summary>Raised when a breadcrumb segment is clicked.</summary>
    public static readonly RoutedEvent<PathSelectedEventArgs> PathSelectedEvent =
        RoutedEvent.Register<PathBar, PathSelectedEventArgs>(
            nameof(PathSelected), RoutingStrategies.Bubble);

    private TextBox? _textBox;
    private Canvas? _nubs;

    static PathBar()
    {
        PathProperty.Changed.AddClassHandler<PathBar>((x, _) => x.RebuildNubs());
        SeparatorProperty.Changed.AddClassHandler<PathBar>((x, _) => x.RebuildNubs());
        TextInsetProperty.Changed.AddClassHandler<PathBar>((x, _) => x.RebuildNubs());
    }

    public string? Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public char Separator
    {
        get => GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    public double TextInset
    {
        get => GetValue(TextInsetProperty);
        set => SetValue(TextInsetProperty, value);
    }

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public event EventHandler<PathSelectedEventArgs>? PathSelected
    {
        add => AddHandler(PathSelectedEvent, value);
        remove => RemoveHandler(PathSelectedEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (_textBox is { } oldBox)
            oldBox.KeyDown -= OnTextBoxKeyDown;
        
        _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
        
        // Commit-on-Enter: typing a path and pressing Enter navigates, the same as clicking
        // the matching nub would.
        if (_textBox is { } box)
            box.KeyDown += OnTextBoxKeyDown;

        if (_nubs is { } old)
            old.SizeChanged -= OnNubsSizeChanged;

        _nubs = e.NameScope.Find<Canvas>("PART_Nubs");

        // The width of the last nub depends on the laid-out field width, so rebuild whenever
        // the nub strip is (re)sized as well as when the path changes.
        if (_nubs is { } canvas)
            canvas.SizeChanged += OnNubsSizeChanged;

        RebuildNubs();
    }

    private void OnNubsSizeChanged(object? sender, SizeChangedEventArgs e) => RebuildNubs();
    
    private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        // The field text is already two-way bound to Path; committing just (re)navigates to it.
        var text = (sender as TextBox)?.Text;
        if (!string.IsNullOrEmpty(text))
        {
            Navigate(text);
            e.Handled = true;
        }
    }

    /// <summary>
    /// Updates <see cref="Path"/>, invokes <see cref="Command"/> and raises
    /// <see cref="PathSelected"/> for the given target. Shared by nub clicks and Enter.
    /// </summary>
    private void Navigate(string target)
    {
        SetCurrentValue(PathProperty, target);

        if (Command is { } cmd && cmd.CanExecute(target))
            cmd.Execute(target);

        RaiseEvent(new PathSelectedEventArgs(PathSelectedEvent, target));
    }

    private void RebuildNubs()
    {
        if (_nubs is null)
            return;

        _nubs.Children.Clear();

        var path = Path;
        if (string.IsNullOrEmpty(path))
            return;

        var inset = TextInset;

        // Field bevel thickness (outline + raised edge), so the first and last nubs sit just
        // inside the path field's frame rather than over it.
        const double fieldEdge = 3;
        var rightEdge = _nubs.Bounds.Width - fieldEdge;

        // Left pixel position of each crumb's segment within the field.
        var crumbs = new List<Crumb>(BuildCrumbs(path, Separator));
        var lefts = new double[crumbs.Count];
        for (var i = 0; i < crumbs.Count; i++)
            lefts[i] = inset + MeasureWidth(path.Substring(0, crumbs[i].NameStart));

        for (var i = 0; i < crumbs.Count; i++)
        {
            var nub = CreateNub(crumbs[i].FullPath);

            // Nubs are contiguous tabs: each spans from its own segment to the start of the
            // next one, and the final nub stretches to the end of the path field.
            var left = i == 0 ? fieldEdge : lefts[i];
            var right = i + 1 < crumbs.Count ? lefts[i + 1] : Math.Max(rightEdge, left + 1);

            nub.Width = Math.Max(1, right - left);
            Canvas.SetLeft(nub, left);
            Canvas.SetTop(nub, 0);
            _nubs.Children.Add(nub);
        }
    }

    /// <summary>
    /// Splits an absolute path into crumbs. Each crumb records where its name begins in the
    /// original string (so a nub can be placed above it) and the full path it navigates to.
    /// </summary>
    private static IEnumerable<Crumb> BuildCrumbs(string path, char sep)
    {
        var i = 0;

        // Leading-separator root (e.g. "/" in "/usr/people").
        if (path.Length > 0 && path[0] == sep)
        {
            yield return new Crumb(0, sep.ToString());
            i = 1;
        }

        while (i < path.Length)
        {
            if (path[i] == sep)
            {
                i++;
                continue;
            }

            var start = i;
            var end = i;
            while (end < path.Length && path[end] != sep)
                end++;

            yield return new Crumb(start, path.Substring(0, end));
            i = end;
        }
    }

    private double MeasureWidth(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        var typeface = new Typeface(FontFamily, FontStyle.Normal, FontWeight.Normal);
        var ft = new FormattedText(
            text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            typeface, FontSize, null);
        return ft.WidthIncludingTrailingWhitespace;
    }

    private Button CreateNub(string fullPath)
    {
        var nub = new Button { Tag = fullPath };

        if (this.TryFindResource("WlrixPathNub", out var theme) && theme is ControlTheme ct)
            nub.Theme = ct;

        ToolTip.SetTip(nub, $"Display the {fullPath} directory");

        nub.Click += (_, _) => Navigate(fullPath);

        return nub;
    }

    private readonly struct Crumb
    {
        public Crumb(int nameStart, string fullPath)
        {
            NameStart = nameStart;
            FullPath = fullPath;
        }

        /// <summary>Index in the path string where this segment's name begins.</summary>
        public int NameStart { get; }

        /// <summary>The full path this crumb navigates to.</summary>
        public string FullPath { get; }
    }
}
