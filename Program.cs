using System.Windows.Forms;

namespace AdvancedPaste;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        HotkeyWindow? hotkeyWindow = null;
        try
        {
            hotkeyWindow = new HotkeyWindow();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Advanced Paste — startup error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using (hotkeyWindow)
        using (var context = new TrayContext(hotkeyWindow))
        {
            Application.Run(context);
        }
    }
}

internal sealed class TrayContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;

    public TrayContext(HotkeyWindow hotkeyWindow)
    {
        hotkeyWindow.HotkeyTriggered += (_, _) => ClipboardTyper.TypeClipboard();

        var menu = new ContextMenuStrip();
        menu.Items.Add("Advanced Paste", null).Enabled = false;
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) => Application.Exit());

        _trayIcon = new NotifyIcon
        {
            Icon             = SystemIcons.Paste,
            Text             = "Advanced Paste  (Ctrl+Alt+V)",
            Visible          = true,
            ContextMenuStrip = menu,
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}
