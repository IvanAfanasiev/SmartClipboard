using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartClipboard.Services
{
    public class SettingsService : INotifyPropertyChanged
    {
        public bool AutoStart
        {
            get => Properties.Settings.Default.AutoStart;
            set
            {
                Properties.Settings.Default.AutoStart = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged();
                AutoStartService.SetAutoStart(AutoStart);
            }
        }
        public int MaxItems
        {
            get => Properties.Settings.Default.MaxItems;
            set
            {
                Properties.Settings.Default.MaxItems = value;
                Properties.Settings.Default.Save();
            }
        }
        public bool ClearClipboardOnStartup
        {
            get => Properties.Settings.Default.ClearClipboardOnStartup;
            set
            {
                Properties.Settings.Default.ClearClipboardOnStartup = value;
                Properties.Settings.Default.Save();
            }
        }
        public bool DarkTheme
        {
            get => Properties.Settings.Default.DarkTheme;
            set
            {
                if (DarkTheme != value)
                {
                    Properties.Settings.Default.DarkTheme = value;
                    Properties.Settings.Default.Save();
                    ApplyTheme();
                    OnPropertyChanged();
                }
            }
        }

        public void ApplyTheme()
        {
            string themeFile = DarkTheme ? "Styles/Dark.xaml" : "Styles/Light.xaml";
            var uri = new Uri(themeFile, UriKind.Relative);

            var dictionaries = Application.Current.Resources.MergedDictionaries;

            var existingTheme = dictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("Dark.xaml") == true || d.Source?.OriginalString.Contains("Light.xaml") == true);

            if (existingTheme != null)
                dictionaries.Remove(existingTheme);
            dictionaries.Add(new ResourceDictionary { Source = uri });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
