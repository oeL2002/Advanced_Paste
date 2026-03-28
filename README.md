# Advanced Paste

A small Windows tray app that types the clipboard as keystrokes instead of pasting it.

Useful when pasting is blocked — remote desktop sessions, virtual machines, password fields, web-based KVM consoles, etc.

## Usage

Run the app. It sits in the system tray. Copy any text, then press **Ctrl+Alt+V** in the target window and the text will be typed character by character.

## How it works

Keystrokes are injected using the Win32 `SendInput` API with the `KEYEVENTF_UNICODE` flag, which sends raw UTF-16 code units directly to the foreground window without going through keyboard layout translation. This means it works regardless of which keyboard layout is active on either end.

## Building

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download) on Windows.

```
dotnet run
```

To produce a standalone `.exe`:

```
dotnet publish -c Release
```

The output will be in `bin/Release/net8.0-windows/win-x64/publish/`.

## Notes

- If **Ctrl+Alt+V** is already claimed by another application, the app will show an error on startup.
- Typing into elevated windows (Task Manager, UAC prompts) will not work unless the app itself is run as administrator — this is a Windows security restriction (UIPI).
- This is a personal vibe coded project. It works for my use case. No guarantees, no support, use at your own risk.
