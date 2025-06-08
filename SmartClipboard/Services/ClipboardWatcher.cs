using SmartClipboard.Models;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

public class ClipboardWatcher : IDisposable
{
    private const int WM_CLIPBOARDUPDATE = 0x031D;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool AddClipboardFormatListener(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

    private readonly HwndSource _hwndSource;
    private readonly Action<string> _onTextCopied;
    private readonly Action<BitmapSource>? _onImageCopied;
    private readonly Action<IEnumerable<string>>? _onFilesCopied;

    private readonly string _imageSaveDirectory =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartClipboard", "Images");

    private void EnsureImageDirectoryExists()
    {
        if (!Directory.Exists(_imageSaveDirectory))
            Directory.CreateDirectory(_imageSaveDirectory);
    }


    public ClipboardWatcher(
                        Window window, 
                        Action<string>? onTextCopied,
                        Action<BitmapSource>? onImageCopied,
                        Action<IEnumerable<string>>? onFilesCopied
    ){
        _onTextCopied = onTextCopied ?? throw new ArgumentNullException(nameof(onTextCopied));
        _onImageCopied = onImageCopied ?? throw new ArgumentNullException(nameof(onImageCopied));
        _onFilesCopied = onFilesCopied ?? throw new ArgumentNullException(nameof(onFilesCopied));

        var helper = new WindowInteropHelper(window);
        _hwndSource = HwndSource.FromHwnd(helper.Handle);
        _hwndSource.AddHook(WndProc);

        AddClipboardFormatListener(helper.Handle);
        EnsureImageDirectoryExists();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_CLIPBOARDUPDATE)
        {
            if (Clipboard.ContainsText())
            {
                HandleText();
            }
            else if (Clipboard.ContainsFileDropList())
            {
                HandleFiles();
            }
            else if (Clipboard.ContainsImage())
            {
                HandleImage();
            }
            handled = true;
        }
        return IntPtr.Zero;
    }

    void HandleText()
    {
        string text = Clipboard.GetText();
        _onTextCopied?.Invoke(text);
    }
    void HandleFiles()
    {
        var fileList = Clipboard.GetFileDropList();
        var paths = fileList.Cast<string>();
        _onFilesCopied?.Invoke(paths);
    }
    void HandleImage()
    {
        var image = Clipboard.GetImage();
        if (image != null)
        {
            _onImageCopied?.Invoke(image);
        }
    }

    private string SaveBitmapToFile(BitmapSource bitmap)
    {
        string filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmssfff}.png";
        string fullPath = Path.Combine(_imageSaveDirectory, filename);

        using var fileStream = new FileStream(fullPath, FileMode.Create);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        encoder.Save(fileStream);

        return fullPath;
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
