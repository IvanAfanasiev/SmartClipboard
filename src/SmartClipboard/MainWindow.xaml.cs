using SmartClipboard.Models;
using SmartClipboard.Services;
using SmartClipboard.ViewModels;
using SmartClipboard.Views;
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
        private readonly SettingsService _settingsService;

        public MainWindow()
        {
            InitializeComponent();

            _settingsService = new SettingsService();
            _settingsService.ApplyTheme();
            if (_settingsService.AutoStart)
            {
                AutoStartService.EnsureAutoStartEntry();
            }
            _mainViewModel = new MainViewModel(new DatabaseService(_settingsService), _settingsService);
            DataContext = _mainViewModel;
            
            Loaded += (_, _) =>
            {
                if (_mainViewModel.Settings.ClearClipboardOnStartup)
                {
                    _mainViewModel.ClearClipboardWithoutAsking();
                }
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

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_settingsService);
            settingsWindow.Owner = this;
            settingsWindow.ShowDialog();
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