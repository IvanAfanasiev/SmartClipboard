using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public class ClipboardWatcher : IDisposable
{
    private const int WM_CLIPBOARDUPDATE = 0x031D;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    private readonly HwndSource _hwndSource;
    private readonly Action<string> _onTextCopied;

    public ClipboardWatcher(Window window, Action<string> onTextCopied)
    {
        _onTextCopied = onTextCopied ?? throw new ArgumentNullException(nameof(onTextCopied));

        var helper = new WindowInteropHelper(window);
        _hwndSource = HwndSource.FromHwnd(helper.Handle);
        _hwndSource.AddHook(WndProc);

        AddClipboardFormatListener(helper.Handle);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_CLIPBOARDUPDATE)
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                _onTextCopied?.Invoke(text);
            }
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_hwndSource != null)
        {
            RemoveClipboardFormatListener(_hwndSource.Handle);
            _hwndSource.RemoveHook(WndProc);
        }
    }
}
