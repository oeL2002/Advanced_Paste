using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AdvancedPaste;

/// <summary>
/// Reads the clipboard and injects its text as keystroke events using SendInput.
/// Uses KEYEVENTF_UNICODE so characters are sent as Unicode code units, which is
/// completely independent of the active keyboard layout.
/// </summary>
internal static class ClipboardTyper
{
    // Maximum inputs per SendInput call. Chunking prevents the message pump from
    // being blocked on very large pastes and keeps individual calls manageable.
    private const int ChunkSize = 2000; // 1000 characters × 2 events each

    /// <summary>
    /// Must be called on the STA thread (the Application.Run thread) because
    /// System.Windows.Forms.Clipboard requires STA.
    /// </summary>
    public static void TypeClipboard()
    {
        string? text = Clipboard.GetText();
        if (string.IsNullOrEmpty(text))
            return;

        NativeMethods.INPUT[] inputs = BuildInputArray(text);
        if (inputs.Length == 0)
            return;

        int cbSize = Marshal.SizeOf<NativeMethods.INPUT>();

        for (int offset = 0; offset < inputs.Length; offset += ChunkSize)
        {
            int count = Math.Min(ChunkSize, inputs.Length - offset);
            NativeMethods.INPUT[] chunk = inputs[offset..(offset + count)];
            _ = NativeMethods.SendInput((uint)count, chunk, cbSize);
        }
    }

    private static NativeMethods.INPUT[] BuildInputArray(string text)
    {
        // Pre-allocate assuming 2 events per char (keydown + keyup).
        // Surrogate pairs are two chars in a C# string, so they naturally produce 4 events.
        var list = new List<NativeMethods.INPUT>(text.Length * 2);

        foreach (char c in text)
        {
            switch (c)
            {
                case '\r':
                    // Skip CR; \n below handles line breaks.
                    break;

                case '\n':
                    list.Add(MakeVkInput(NativeMethods.VK_RETURN, keyUp: false));
                    list.Add(MakeVkInput(NativeMethods.VK_RETURN, keyUp: true));
                    break;

                case '\t':
                    list.Add(MakeVkInput(NativeMethods.VK_TAB, keyUp: false));
                    list.Add(MakeVkInput(NativeMethods.VK_TAB, keyUp: true));
                    break;

                default:
                    list.Add(MakeUnicodeInput(c, keyUp: false));
                    list.Add(MakeUnicodeInput(c, keyUp: true));
                    break;
            }
        }

        return list.ToArray();
    }

    private static NativeMethods.INPUT MakeUnicodeInput(char scanCode, bool keyUp)
    {
        uint flags = NativeMethods.KEYEVENTF_UNICODE;
        if (keyUp)
            flags |= NativeMethods.KEYEVENTF_KEYUP;

        return new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            ki = new NativeMethods.KEYBDINPUT
            {
                wVk = 0,
                wScan = scanCode,
                dwFlags = flags,
                time = 0,
                dwExtraInfo = NativeMethods.GetMessageExtraInfo(),
            },
        };
    }

    private static NativeMethods.INPUT MakeVkInput(uint vk, bool keyUp)
    {
        uint flags = keyUp ? NativeMethods.KEYEVENTF_KEYUP : 0;

        return new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            ki = new NativeMethods.KEYBDINPUT
            {
                wVk = (ushort)vk,
                wScan = 0,
                dwFlags = flags,
                time = 0,
                dwExtraInfo = NativeMethods.GetMessageExtraInfo(),
            },
        };
    }
}
