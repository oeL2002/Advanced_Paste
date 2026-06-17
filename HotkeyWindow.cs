using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AdvancedPaste;

/// <summary>
/// A message-only window (HWND_MESSAGE) that owns the global hotkey registration.
/// Using a message-only window means nothing appears in the taskbar or alt-tab list.
/// </summary>
internal sealed class HotkeyWindow : NativeWindow, IDisposable
{
    private const int HotkeyId = 1;

    private static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

    private readonly uint _modifiers = NativeMethods.MOD_CONTROL | NativeMethods.MOD_ALT | NativeMethods.MOD_NOREPEAT;
    private readonly uint _vk = NativeMethods.VK_V;

    public bool IsEnabled { get; private set; }

    public event EventHandler? HotkeyTriggered;

    public HotkeyWindow()
    {
        var cp = new CreateParams { Parent = HWND_MESSAGE };
        CreateHandle(cp);

        bool registered = NativeMethods.RegisterHotKey(Handle, HotkeyId, _modifiers, _vk);

        if (!registered)
        {
            int error = Marshal.GetLastWin32Error();
            DestroyHandle();
            throw new InvalidOperationException(
                $"RegisterHotKey failed (Win32 error {error}). " +
                "Ctrl+Alt+V may already be claimed by another application.");
        }

        IsEnabled = true;
    }

    public void Enable()
    {
        if (IsEnabled) return;
        NativeMethods.RegisterHotKey(Handle, HotkeyId, _modifiers, _vk);
        IsEnabled = true;
    }

    public void Disable()
    {
        if (!IsEnabled) return;
        NativeMethods.UnregisterHotKey(Handle, HotkeyId);
        IsEnabled = false;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == NativeMethods.WM_HOTKEY && m.WParam.ToInt32() == HotkeyId)
            HotkeyTriggered?.Invoke(this, EventArgs.Empty);
        else
            base.WndProc(ref m);
    }

    public void Dispose()
    {
        NativeMethods.UnregisterHotKey(Handle, HotkeyId);
        DestroyHandle();
    }
}
