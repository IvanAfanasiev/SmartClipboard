using SmartClipboard.Models;
using SmartClipboard.Services;
using SmartClipboard.ViewModels;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SmartClipboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _mainViewModel;
        private ClipboardWatcher? _watcher;

        public MainWindow()
        {
            InitializeComponent();
            _mainViewModel = new MainViewModel(new DatabaseService());
            DataContext = _mainViewModel;
            
            Loaded += (_, _) =>
            {
                _watcher = new ClipboardWatcher(this,
                    onTextCopied: OnClipboardTextCopied,
                    onImageCopied: OnClipboardImageCopied,
                    onFilesCopied: OnClipboardFilesCopied);
                _mainViewModel.LoadData();
            };

            Unloaded += (_, _) =>
            {
                _watcher?.Dispose();
            };
        }

        private void OnClipboardTextCopied(string text)
        {
            Dispatcher.Invoke(() =>
            {
                _mainViewModel.SaveClipboardText(text);
            });
        }
        private void OnClipboardImageCopied(BitmapSource image)
        {
            Dispatcher.Invoke(() =>
            {
                _mainViewModel.SaveClipboardImage(image);
            });
        }
        private void OnClipboardFilesCopied(IEnumerable<string> paths)
        {
            Dispatcher.Invoke(() =>
            {
                _mainViewModel.SaveClipboardFiles(paths);
            });
        }
    }
}