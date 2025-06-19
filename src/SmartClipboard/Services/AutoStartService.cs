using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClipboard.Services
{
    class AutoStartService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "SmartClipboard";

        public static bool IsAutoStartEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            var value = key?.GetValue(AppName) as string;
            return !string.IsNullOrWhiteSpace(value);
        }

        public static void SetAutoStart(bool enable)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null) return;

            if (enable)
            {
                string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
                key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
                key.DeleteValue(AppName, false);
        }
    }
}
