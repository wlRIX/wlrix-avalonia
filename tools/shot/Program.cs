using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Wlrix.Avalonia;
using Wlrix.Avalonia.Controls;
using Wlrix.Avalonia.Controls.ColorPicker;
using Wlrix.Avalonia.Controls.ColorPicker.Controls;

// Dev tool: renders the gallery to a PNG for each color scheme using the real Skia
// backend (no display required). Usage: dotnet run -- <outputDir>
internal static class Program
{
    public static int Main(string[] args)
    {
        AppBuilder.Configure<ShotApp>()
            .UseSkia()
            .WithInterFont()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .SetupWithoutStarting();

        var outDir = args.Length > 0 ? args[0] : ".";
        System.IO.Directory.CreateDirectory(outDir);

        // Every scheme the theme ships, from the generated catalog, so a new palette JSON gets
        // a screenshot without this list being edited.
        foreach (var scheme in WlrixSchemes.All)
        {
            ApplyScheme(scheme);
            var window = new Window { Width = 520, Height = 1340 };
            window.Content = BuildGallery(scheme.Name);
            window.Show();
            for (int i = 0; i < 14; i++) Dispatcher.UIThread.RunJobs();

            var frame = window.CaptureRenderedFrame();
            if (frame is null) { Console.WriteLine("NULL FRAME for " + scheme.Id); return 2; }
            var path = System.IO.Path.Combine(outDir, FileName(scheme) + ".png");
            frame.Save(path);
            Console.WriteLine("SAVED " + path + " " + frame.PixelSize.Width + "x" + frame.PixelSize.Height);
        }
        return 0;
    }

    // Through the theme, like an application does, rather than by overriding
    // Application.Resources: these screenshots are meant to be what an app renders.
    private static void ApplyScheme(WlrixScheme scheme)
    {
        if (WlrixTheme.From(Application.Current) is { } theme)
            theme.Scheme = scheme.Id;
    }

    // The artifact names the README already links: Classic.png, ClassicG10.png, ...
    private static string FileName(WlrixScheme scheme) => string.Concat(
        scheme.Id.Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

    private static Control BuildGallery(string scheme)
    {
        var root = new StackPanel { Margin = new Thickness(12) };
        root.Children.Add(WrapEtched(scheme + " scheme",
            Row(new Button { Content = "OK", IsDefault = true },
                new Button { Content = "Apply" },
                new Button { Content = "Cancel" },
                new Button { Content = "Off", IsEnabled = false })));

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
        var checks = Col(
            new CheckBox { Content = "Emulate tabs", IsChecked = true },
            new CheckBox { Content = "Use tabs" },
            new CheckBox { Content = "Mixed", IsChecked = null });
        var radios = Col(
            new RadioButton { Content = "Unix", IsChecked = true, GroupName = "g" },
            new RadioButton { Content = "DOS", GroupName = "g" },
            new RadioButton { Content = "Macintosh", GroupName = "g" });
        Grid.SetColumn(checks, 0); Grid.SetColumn(radios, 1);
        grid.Children.Add(checks); grid.Children.Add(radios);
        root.Children.Add(WrapEtched("Check & radio", grid));

        var combo = new ComboBox { Width = 160 };
        combo.Items.Add(new ComboBoxItem { Content = "Unix" });
        combo.Items.Add(new ComboBoxItem { Content = "DOS" });
        combo.SelectedIndex = 0;
        root.Children.Add(WrapEtched("Text field & option menu",
            Col(new TextBox { Text = "/usr/people/guest" }, combo)));

        root.Children.Add(WrapEtched("Path bar",
            new PathBar { Path = "/usr/people/debbie" }));

        var slider = new Slider { Minimum = 0, Maximum = 100, Value = 60, Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
        var progress = new ProgressBar { Minimum = 0, Maximum = 100, Value = 60, Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
        var progressText = new ProgressBar { Minimum = 0, Maximum = 100, Value = 20, Width = 240, ShowProgressText = true, ProgressTextFormat = "{0:0}", HorizontalAlignment = HorizontalAlignment.Left };
        root.Children.Add(WrapEtched("Slider & gauge", Col(slider, progress, progressText)));

        root.Children.Add(WrapEtched("Tabs (top)", MakeTabs(Dock.Top)));
        root.Children.Add(WrapEtched("Tabs (bottom)", MakeTabs(Dock.Bottom)));

        var wheelV = new Thumbwheel
        {
            Orientation = Orientation.Vertical, Minimum = 0, Maximum = 100, Value = 40,
            ShowHomeButton = true, HomeValue = 50, Height = 126,   // standard IRIX size
            VerticalAlignment = VerticalAlignment.Top
        };
        var wheelH = new Thumbwheel
        {
            Orientation = Orientation.Horizontal, IsContinuous = true, Width = 126,
            VerticalAlignment = VerticalAlignment.Center
        };
        root.Children.Add(WrapEtched("Thumbwheels", Row(wheelV, wheelH)));

        root.Children.Add(WrapEtched("Color chooser",
            new ColorChooser { Color = Color.FromRgb(0x30, 0x30, 0xD0) }));

        root.Children.Add(WrapEtched("LED buttons", Row(
            Led("toggle", isChecked: true),
            Led("toggle", isChecked: false))));

        return root;
    }
    
    private static TabControl MakeTabs(Dock placement)
    {
        var t = new TabControl { TabStripPlacement = placement, Height = 96 };
        foreach (var h in new[] { "Applications", "Collaboration", "ControlPanels", "DesktopTools" })
            t.Items.Add(new TabItem { Header = h, Content = new TextBlock { Text = h + " page", Margin = new Thickness(2) } });
        t.SelectedIndex = 1;
        return t;
    }

    private static ToggleButton Led(string content, bool isChecked)
    {
        var tb = new ToggleButton { Content = content, IsChecked = isChecked };
        if (Application.Current!.TryFindResource("WlrixLedButton", out var theme)
            && theme is ControlTheme ct)
            tb.Theme = ct;
        return tb;
    }

    private static StackPanel Row(params Control[] kids)
    {
        var p = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        foreach (var k in kids) p.Children.Add(k);
        return p;
    }

    private static StackPanel Col(params Control[] kids)
    {
        var p = new StackPanel { Spacing = 6 };
        foreach (var k in kids) p.Children.Add(k);
        return p;
    }

    private static BevelBorder WrapEtched(string title, Control body)
    {
        var inner = new StackPanel { Spacing = 8 };
        inner.Children.Add(new TextBlock { Text = title, FontWeight = FontWeight.Bold });
        inner.Children.Add(body);

        var app = Application.Current!;
        IBrush? R(string k) { app.TryGetResource(k, null, out var o); return o as IBrush; }

        return new BevelBorder
        {
            Bevel = WlrixBevel.Etched,
            BevelThickness = 2,
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 10),
            Child = inner,
            Background = R("WlrixFace"),
            LightBrush = R("WlrixTopShadow"),
            DarkBrush = R("WlrixBottomShadow")
        };
    }
}

internal sealed class ShotApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new WlrixTheme());
        Styles.Add(new ColorPickerTheme());
    }
}
