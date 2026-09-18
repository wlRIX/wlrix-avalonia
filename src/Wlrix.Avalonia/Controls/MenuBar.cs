using System;
using Avalonia.Controls;

namespace Wlrix.Avalonia.Controls;

/// <summary>
/// An application's menu bar: one row, with Help at the right end.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="Menu"/> whose items are laid out in a <c>DockPanel</c> rather than the default
/// wrapping row, so an item carrying <c>DockPanel.Dock="Right"</c> sits against the right edge
/// the way every IRIX application put Help. Everything else needs no annotation: a DockPanel
/// docks to the left by default, in order.
/// </para>
/// <para>
/// <b>Help stays an ordinary item of this menu</b> rather than becoming a property of its own.
/// Avalonia's menu drives its children as containers of its <c>Items</c> — opening on hover
/// once the bar is active, access keys, and moving between top-level menus with the arrow keys
/// all come from that. A separate slot would draw in the right place and behave like a
/// different control.
/// </para>
/// <para>
/// This exists because doing it by hand went wrong in two directions at once. Laying the bar
/// out as <i>two</i> menus gives Help a raised bevel of its own, so a seam appears in the
/// middle of the bar; writing the items panel per application means one that forgets it has no
/// right-aligned Help at all. Both were in the tree.
/// </para>
/// <para>
/// It also paints <c>WlrixMenubarBackground</c> — a colour every scheme has carried from the
/// start, that nothing but one hand-rolled wrapper was using. A menu bar is lighter than a
/// panel face on IRIX.
/// </para>
/// </remarks>
public class MenuBar : Menu
{
    protected override Type StyleKeyOverride => typeof(MenuBar);
}
