using SmartClipboard.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartClipboard.ViewModels
{
    class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settings;

        public SettingsViewModel(SettingsService settings)
        {
            _settings = settings;

            AutoStart = _settings.AutoStart;
            MaxItems = _settings.MaxItems;
            ClearClipboardOnStartup = _settings.ClearClipboardOnStartup;
            DarkTheme = _settings.DarkTheme;
        }

        private bool _autoStart;
        public bool AutoStart
        {
            get => _autoStart;
            set
            {
                if (_autoStart != value)
                {
                    _autoStart = value;
                    OnPropertyChanged();
                    _settings.AutoStart = value;
                }
            }
        }

        private int _maxItems;
        public int MaxItems
        {
            get => _maxItems;
            set
            {
                if (_maxItems != value)
                {
                    _maxItems = value;
                    OnPropertyChanged();
                    _settings.MaxItems = value;
                }
            }
        }

        private bool _clearClipboardOnStartup;
        public bool ClearClipboardOnStartup
        {
            get => _clearClipboardOnStartup;
            set
            {
                if (_clearClipboardOnStartup != value)
                {
                    _clearClipboardOnStartup = value;
                    OnPropertyChanged();
                    _settings.ClearClipboardOnStartup = value;
                }
            }
        }

        private bool _darkTheme;
        public bool DarkTheme
        {
            get => _darkTheme;
            set
            {
                if (_darkTheme != value)
                {
                    _darkTheme = value;
                    OnPropertyChanged();
                    _settings.DarkTheme = value;
                    ApplyTheme(value);
                }
            }
        }

        private void ApplyTheme(bool dark)
        {
            var themePath = dark ? "Styles/Dark.xaml" : "Styles/Light.xaml";
            var themeDict = new ResourceDictionary { Source = new Uri(themePath, UriKind.Relative) };
            var mergedDicts = Application.Current.Resources.MergedDictionaries;

            var existing = mergedDicts.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Dark") || d.Source.OriginalString.Contains("Light"));
            if (existing != null)
                mergedDicts.Remove(existing);

            mergedDicts.Add(themeDict);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
