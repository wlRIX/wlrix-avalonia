using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Wlrix.Avalonia.Controls;

/// <summary>
/// The tab strip of an IRIX / Motif notebook: overlapping tabs in one row, the selected one in
/// front with the rest stepping back the farther they are from it. When the tabs do not all
/// fit, the ones that are left out show as a stack of slanted tab edges at that end of the
/// strip. Clicking a stack opens a menu that jumps to any tab.
/// </summary>
/// <remarks>
/// <para>
/// This is the items panel of the themed <see cref="TabControl"/>, so it works for the stock
/// control as well as for <see cref="NotebookTabControl"/>. It finds its owner by walking up
/// from itself.
/// </para>
/// <para>
/// The selected tab joins its page through a gap in the page's edge. The tab does not
/// overlap the page. After each layout pass the panel finds the page (the
/// <see cref="BevelBorder"/> named <c>PART_Page</c> in the owner's template) and sets the
/// gap under the selected tab.
/// </para>
/// <para>
/// A panel cannot paint under its own children, so the stacks of hidden-tab edges are drawn
/// by a <see cref="NotebookOverflowEdges"/> (<c>PART_OverflowEdges</c>) laid behind the strip.
/// The panel tells it where the stacks go after each layout pass, and handles the clicks on
/// them itself.
/// </para>
/// <para>
/// Only top and bottom strips get this treatment. A left or right strip is a plain column
/// with every tab in it and no gap, because no wlRIX app uses one.
/// </para>
/// </remarks>
public class NotebookTabPanel : Panel
{
    /// <summary>Slant of a tab's side, matching the tabs' own <see cref="TabShape.SlantWidth"/>.</summary>
    public static readonly StyledProperty<double> SlantWidthProperty =
        AvaloniaProperty.Register<NotebookTabPanel, double>(nameof(SlantWidth), 10.0);

    /// <summary>How far each tab slides under its neighbor, in pixels.</summary>
    public static readonly StyledProperty<double> OverlapProperty =
        AvaloniaProperty.Register<NotebookTabPanel, double>(nameof(Overlap), 8.0);

    /// <summary>Spacing, in pixels, between the stacked edges of hidden tabs.</summary>
    public static readonly StyledProperty<double> EdgeSpacingProperty =
        AvaloniaProperty.Register<NotebookTabPanel, double>(nameof(EdgeSpacing), 8.0);

    /// <summary>
    /// Edges drawn in a stack of hidden tabs. The stack is a fixed marker, as in IRIX, not a
    /// count: one hidden tab or twenty, it shows the same edges.
    /// </summary>
    public static readonly StyledProperty<int> OverflowEdgesProperty =
        AvaloniaProperty.Register<NotebookTabPanel, int>(nameof(OverflowEdges), 3);

    /// <summary>
    /// Width, in pixels, of the page's edge (outline and bevel) the selected tab breaks
    /// through. It is also the width of the tab's own side, so the page's edge runs on as far
    /// as the tab's outline and bevel do.
    /// </summary>
    public static readonly StyledProperty<double> SeamInsetProperty =
        AvaloniaProperty.Register<NotebookTabPanel, double>(nameof(SeamInset), 3.0);

    private TabControl? _owner;
    private BevelBorder? _page;
    private NotebookOverflowEdges? _edges;
    private int _first;
    private int _last = -1;
    private double _edgeReserve;
    private Rect _leadingEdges;
    private Rect _trailingEdges;

    static NotebookTabPanel()
    {
        AffectsMeasure<NotebookTabPanel>(OverlapProperty, EdgeSpacingProperty, OverflowEdgesProperty,
            SlantWidthProperty);
    }

    public NotebookTabPanel()
    {
        // The stacks of hidden-tab edges are drawn behind the panel, by NotebookOverflowEdges,
        // but the clicks on them are the panel's: it has to be hit-testable where no tab is.
        Background = Brushes.Transparent;
    }

    public double SlantWidth
    {
        get => GetValue(SlantWidthProperty);
        set => SetValue(SlantWidthProperty, value);
    }

    public double Overlap
    {
        get => GetValue(OverlapProperty);
        set => SetValue(OverlapProperty, value);
    }

    public double EdgeSpacing
    {
        get => GetValue(EdgeSpacingProperty);
        set => SetValue(EdgeSpacingProperty, value);
    }

    public int OverflowEdges
    {
        get => GetValue(OverflowEdgesProperty);
        set => SetValue(OverflowEdgesProperty, value);
    }

    public double SeamInset
    {
        get => GetValue(SeamInsetProperty);
        set => SetValue(SeamInsetProperty, value);
    }

    /// <summary>Tabs left out before the first one shown.</summary>
    public int HiddenBefore => Children.Count == 0 ? 0 : _first;

    /// <summary>Tabs left out after the last one shown.</summary>
    public int HiddenAfter => Children.Count == 0 ? 0 : Children.Count - 1 - _last;

    private bool IsHorizontal => _owner?.TabStripPlacement is not (Dock.Left or Dock.Right);

    private bool IsBottom => _owner?.TabStripPlacement == Dock.Bottom;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _owner = this.FindAncestorOfType<TabControl>();
        if (_owner is not null)
            _owner.PropertyChanged += OnOwnerPropertyChanged;
        LayoutUpdated += OnLayoutUpdated;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (_owner is not null)
            _owner.PropertyChanged -= OnOwnerPropertyChanged;
        _owner = null;
        _page = null;
        _edges = null;
        LayoutUpdated -= OnLayoutUpdated;
    }

    private void OnOwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        // The selection decides which tabs are shown and how they stack, and the placement
        // which way up they are drawn.
        if (e.Property == SelectingItemsControl.SelectedIndexProperty ||
            e.Property == TabControl.TabStripPlacementProperty)
        {
            InvalidateMeasure();
            InvalidateVisual();
        }
    }

    /// <summary>
    /// The selected tab's index among the children, or -1. Read from the containers rather
    /// than the owner, because the containers are what is laid out here.
    /// </summary>
    private int SelectedChild()
    {
        for (var i = 0; i < Children.Count; i++)
            if (Children[i] is TabItem { IsSelected: true })
                return i;
        return -1;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var children = Children;
        var n = children.Count;
        foreach (var child in children)
            child.Measure(new Size(double.PositiveInfinity, availableSize.Height));

        if (!IsHorizontal)
        {
            _first = 0;
            _last = n - 1;
            double cw = 0, ch = 0;
            foreach (var child in children)
            {
                cw = Math.Max(cw, child.DesiredSize.Width);
                ch += child.DesiredSize.Height;
            }

            return new Size(cw, ch);
        }

        double height = 0;
        foreach (var child in children)
            height = Math.Max(height, child.DesiredSize.Height);

        _edgeReserve = SlantWidth + EdgeSpacing * Math.Max(0, OverflowEdges);
        var full = RowWidth(0, n - 1);

        if (n == 0)
        {
            _first = 0;
            _last = -1;
        }
        else if (double.IsInfinity(availableSize.Width) || full <= availableSize.Width)
        {
            _first = 0;
            _last = n - 1;
        }
        else
        {
            FitWindow(availableSize.Width, SelectedChild());
        }

        var width = double.IsInfinity(availableSize.Width) ? full : Math.Min(full, availableSize.Width);
        return new Size(width, height);
    }

    /// <summary>
    /// Width of tabs <paramref name="first"/> to <paramref name="last"/> laid out together,
    /// including room for the stacks of whatever is left out on either side.
    /// </summary>
    private double RowWidth(int first, int last)
    {
        if (last < first)
            return 0;
        double w = 0;
        for (var i = first; i <= last; i++)
            w += Children[i].DesiredSize.Width;
        w -= Overlap * (last - first);
        if (first > 0)
            w += _edgeReserve;
        if (last < Children.Count - 1)
            w += _edgeReserve;
        return w;
    }

    /// <summary>
    /// Picks which tabs to show. The window moves as little as it can: it stays where it was
    /// unless the selected tab has gone out of it, and only then slides just far enough to
    /// bring that tab back. When there is room to spare at the end it slides back to fill it.
    /// </summary>
    private void FitWindow(double available, int selected)
    {
        var n = Children.Count;
        var first = Math.Clamp(_first, 0, n - 1);
        if (selected >= 0 && selected < first)
            first = selected;

        var last = Extend(first, available);
        if (selected > last)
        {
            // Smallest move forward that still reaches the selected tab.
            while (first < selected && RowWidth(first, selected) > available)
                first++;
            last = Extend(first, available);
        }

        // Grown (or tabs closed): take back tabs from the left while they still fit.
        while (first > 0 && RowWidth(first - 1, last) <= available)
            first--;
        last = Extend(first, available);

        _first = first;
        _last = last;
    }

    /// <summary>The last tab that fits when the row starts at <paramref name="first"/>.</summary>
    private int Extend(int first, double available)
    {
        var last = first;
        while (last + 1 < Children.Count && RowWidth(first, last + 1) <= available)
            last++;
        return last;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var children = Children;
        var n = children.Count;

        if (!IsHorizontal)
        {
            double y = 0;
            foreach (var child in children)
            {
                child.Arrange(new Rect(0, y, finalSize.Width, child.DesiredSize.Height));
                y += child.DesiredSize.Height;
            }

            _leadingEdges = _trailingEdges = default;
            return finalSize;
        }

        var selected = SelectedChild();
        var h = finalSize.Height;
        var x = 0.0;

        _leadingEdges = HiddenBefore > 0 ? new Rect(0, 0, _edgeReserve, h) : default;
        if (HiddenBefore > 0)
            x = _edgeReserve - Overlap;

        for (var i = 0; i < n; i++)
        {
            var child = children[i];
            if (i < _first || i > _last)
            {
                // Out of the window. Arranged to nothing (the tab clips to its bounds), not
                // hidden, because changing IsVisible during arrange would start another pass.
                child.Arrange(default);
                continue;
            }

            var w = child.DesiredSize.Width;
            child.Arrange(new Rect(x, 0, w, h));
            x += w - Overlap;
        }

        _trailingEdges = HiddenAfter > 0 ? new Rect(x, 0, _edgeReserve, h) : default;

        // Front to back: the selected tab, then outward from it.
        for (var i = 0; i < n; i++)
        {
            var z = selected < 0 ? n - i : i == selected ? n + 1 : n - Math.Abs(i - selected);
            if (children[i].ZIndex != z)
                children[i].ZIndex = z;
        }

        return finalSize;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Handled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;

        var p = e.GetPosition(this);
        var leading = HiddenBefore > 0 && _leadingEdges.Contains(p);
        var trailing = HiddenAfter > 0 && _trailingEdges.Contains(p);
        if (!leading && !trailing)
            return;

        // A tab overlapping the stack is above the panel and takes its own clicks, so a press
        // that arrives here is on the visible part of the stack.
        e.Handled = true;
        OpenJumpMenu(leading ? _leadingEdges : _trailingEdges, trailing);
    }

    /// <summary>
    /// Opens the quick-jump menu: every tab, with separators either side of the ones on show
    /// (so a separator appears only when tabs are hidden on that side).
    /// </summary>
    private void OpenJumpMenu(Rect from, bool trailing)
    {
        if (_owner is not { } owner)
            return;

        var items = new List<Control>();
        for (var i = 0; i < Children.Count; i++)
        {
            if (i == _first && i > 0 || i == _last + 1 && i < Children.Count)
                items.Add(new Separator());

            if (Children[i] is not TabItem tab)
                continue;
            var index = i;
            // A string header goes in as a TextBlock: the menu reads a bare string as an
            // access key label, and an underscore in a tab's name would vanish.
            var item = new MenuItem
            {
                Header = tab.Header is string text ? new TextBlock { Text = text } : tab.Header,
                HeaderTemplate = tab.Header is string ? null : tab.HeaderTemplate,
                IsEnabled = tab.IsEnabled,
            };
            item.Click += (_, _) => owner.SelectedIndex = index;
            items.Add(item);
        }

        // The menu rises from the stack it was opened from, toward the page, and keeps to
        // that stack's end of the strip.
        var bottom = IsBottom;
        var menu = new ContextMenu
        {
            ItemsSource = items,
            PlacementTarget = this,
            Placement = PlacementMode.AnchorAndGravity,
            PlacementRect = from,
            PlacementAnchor = (bottom, trailing) switch
            {
                (true, true) => PopupAnchor.TopRight,
                (true, false) => PopupAnchor.TopLeft,
                (false, true) => PopupAnchor.BottomRight,
                _ => PopupAnchor.BottomLeft,
            },
            PlacementGravity = (bottom, trailing) switch
            {
                (true, true) => PopupGravity.TopLeft,
                (true, false) => PopupGravity.TopRight,
                (false, true) => PopupGravity.BottomLeft,
                _ => PopupGravity.BottomRight,
            },
        };
        menu.Open(this);
    }

    /// <summary>
    /// Points the page's gap at the selected tab. Done after layout, not during arrange,
    /// because the page is laid out after the strip and the two only line up once both are.
    /// </summary>
    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (_owner is not { } owner)
            return;

        _page = FindPart(owner, _page, "PART_Page");
        _edges = FindPart(owner, _edges, "PART_OverflowEdges");

        if (_edges is { } edges)
        {
            var horizontal = IsHorizontal;
            var origin = this.TranslatePoint(default, edges) ?? default;
            edges.Update(
                horizontal && HiddenBefore > 0 ? OverflowEdges : 0, _leadingEdges.Translate(origin),
                horizontal && HiddenAfter > 0 ? OverflowEdges : 0, _trailingEdges.Translate(origin),
                IsBottom, SlantWidth, Overlap, EdgeSpacing);
        }

        if (_page is not { } page)
            return;

        double start = 0, length = 0;
        var edge = IsBottom ? Dock.Bottom : Dock.Top;
        if (IsHorizontal && IsEffectivelyVisible && SelectedChild() is var s and >= 0 &&
            Children[s] is { Bounds: { Width: > 0 } bounds })
        {
            var inset = SeamInset;
            if (this.TranslatePoint(new Point(bounds.X + inset, 0), page) is { } a)
            {
                start = a.X;
                length = Math.Max(0, bounds.Width - 2 * inset);
            }
        }

        if (page.GapEdge != edge)
            page.GapEdge = edge;
        if (page.GapStart != start)
            page.GapStart = start;
        if (page.GapLength != length)
            page.GapLength = length;
    }

    /// <summary>
    /// The owner's template part of that name, kept from the last call while it still
    /// belongs to the owner's current template.
    /// </summary>
    private static T? FindPart<T>(TabControl owner, T? cached, string name) where T : Control
    {
        if (cached is not null && cached.TemplatedParent == owner && cached.IsAttachedToVisualTree())
            return cached;
        return owner.GetVisualDescendants()
            .OfType<T>()
            .FirstOrDefault(c => c.Name == name && c.TemplatedParent == owner);
    }
}
