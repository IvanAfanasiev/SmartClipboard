using SmartClipboard.Models;
using SmartClipboard.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartClipboard.ViewModels
{
    internal class MainViewModel: INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService = new();
        private string _lastText = string.Empty;

        public ObservableCollection<ClipboardItem> ClipboardItems { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            LoadData();
        }

        public void SaveClipboardText(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || text == _lastText)
                return;

            _lastText = text;

            var item = new ClipboardItem
            {
                Content = text,
                Timestamp = DateTime.Now,
                Type = ClassificationService.Classify(text)
            };

            _dbService.InsertClipboardItem(item);
            ClipboardItems.Insert(0, item);
        }

        public void LoadData()
        {
            var items = _dbService.GetAllItems();
            ClipboardItems.Clear();
            foreach (var item in items)
                ClipboardItems.Add(item);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
