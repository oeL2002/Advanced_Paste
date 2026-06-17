using System.Runtime.InteropServices;

namespace AdvancedPaste;

internal static class NativeMethods
{
    // ── Hotkey registration ───────────────────────────────────────────────

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    internal const uint MOD_ALT = 0x0001;
    internal const uint MOD_CONTROL = 0x0002;
    internal const uint MOD_NOREPEAT = 0x4000;

    internal const uint VK_V = 0x56;
    internal const uint VK_RETURN = 0x0D;
    internal const uint VK_TAB = 0x09;

    internal const int WM_HOTKEY = 0x0312;

    // ── SendInput ────────────────────────────────────────────────────────

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetMessageExtraInfo();

    internal const int INPUT_KEYBOARD = 1;
    internal const uint KEYEVENTF_UNICODE = 0x0004;
    internal const uint KEYEVENTF_KEYUP = 0x0002;
    internal const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

    // INPUT struct — explicit layout required because it contains a union.
    // On x64: type (4) + padding (4) + union (32) = 40 bytes total.
    // The keyboard variant (KEYBDINPUT) sits at the start of the union, offset 8.
    [StructLayout(LayoutKind.Explicit, Size = 40)]
    internal struct INPUT
    {
        [FieldOffset(0)]
        public uint type;

        [FieldOffset(8)]
        public KEYBDINPUT ki;
    }

    // KEYBDINPUT mirrors the Win32 struct exactly.
    // dwExtraInfo is a pointer-sized value; using IntPtr ensures correct sizing on x64.
    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }
}
