﻿using SmartClipboard.Models;
using SmartClipboard.Services;
using SmartClipboard.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SmartClipboard.ViewModels
{
    public class MainViewModel: INotifyPropertyChanged
    {
        private readonly IDatabaseService _dbService;
        public SettingsService Settings { get; }
        private string? _lastImageHash;

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value.Trim();
                OnPropertyChanged(nameof(SearchQuery));
                SearchItems();
            }
        }
        object? _selectedClipboardItem;
        public object? SelectedClipboardItem
        {
            get => _selectedClipboardItem;
            set
            {
                _selectedClipboardItem = value;
                SetClipboardItemCommand.Execute(value);
            }
        }

        bool _suppressClipboardUpdate = false;

        public ObservableCollection<ClipboardItem> ClipboardItems { get; } = new();
        public ObservableCollection<ContentType> AvailableTypes { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand TogglePinCommand => new RelayTypedCommand<ClipboardItem>(TogglePin);
        public ICommand FilterByTypeCommand => new RelayTypedCommand<ContentType>(FilterByType);
        public ICommand DeleteItemCommand => new RelayTypedCommand<ClipboardItem>(DeleteItem);
        public ICommand ClearDatabaseCommand => new RelayCommand(ClearClipboard);
        public ICommand SetClipboardItemCommand => new RelayTypedCommand<ClipboardItem>(SetClipboardItem);

        public MainViewModel(IDatabaseService dbService, SettingsService settingsService)
        {
            Settings = settingsService;
            _dbService = dbService;
            LoadData();
            GetAvailableTypes();
            _selectedClipboardItem = null;
        }

        public void SaveClipboardText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            ClipboardItem? existingItem = ClipboardItems.FirstOrDefault(item => item.Content == text);
            if (existingItem != null)
            {
                ClipboardItems.Remove(existingItem);
                ClipboardItems.Add(existingItem);
                return;
            }

            var item = new ClipboardItem
            {
                Content = text,
                Timestamp = DateTime.Now,
                Type = ClassificationService.Classify(text)
            };
            InsertItem(item);
            GetAvailableTypes();
        }
        public void SaveClipboardImage(BitmapSource image)
        {
            var hash = ImageUtils.GetImageHash(image);

            if (hash == _lastImageHash)
                return;

            _lastImageHash = hash;

            string filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmssfff}.png";
            string folder = 
                Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "SmartClipboard", 
                        "Images"
                    );

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fullPath = Path.Combine(folder, filename);

            using var fileStream = new FileStream(fullPath, FileMode.Create);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(fileStream);

            var item = new ClipboardItem
            {
                Content = filename,
                Timestamp = DateTime.Now,
                ImagePath = fullPath,
                Type = ContentType.Image,
            };
            InsertItem(item);
            GetAvailableTypes();
        }
        public void SaveClipboardFiles(IEnumerable<string> paths)
        {
            var item = new ClipboardItem
            {
                Timestamp = DateTime.Now,
                Content = string.Join(",\n", paths.Select(Path.GetFileName)),
                FilePathList = string.Join(";\n", paths),
                Type = ContentType.File
            };
            InsertItem(item);
            GetAvailableTypes();
        }

        public void TogglePin(ClipboardItem item)
        {
            item.IsPinned = !item.IsPinned;
            _dbService.UpdateClipboardItem(item);
            SortClipboardItems(ClipboardItems);
        }

        public void DeleteItem(ClipboardItem item)
        {
            _dbService.DeleteClipboardItem(item);
            ClipboardItems.Remove(item);
            GetAvailableTypes();
        }

        void InsertItem(ClipboardItem item)
        {
            if (_suppressClipboardUpdate)
                return;
            _dbService.InsertClipboardItem(item);
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchItems();
                return;
            }
            int insertIndex = ClipboardItems.TakeWhile(i => i.IsPinned).Count();
            ClipboardItems.Insert(insertIndex, item);
            _selectedClipboardItem = ClipboardItems.Last();
            GetAvailableTypes();
        }

        public void LoadData()
        {
            var items = _dbService.GetAllItems();
            SortClipboardItems(items);
        }

        private void SortClipboardItems(IEnumerable<ClipboardItem> items)
        {
            var sortedItems = items
                .OrderByDescending(i => i.IsPinned)
                .ThenByDescending(i => i.Timestamp)
                .ToList();
            UpdateClipboard(sortedItems);
        }

        public void SearchItems()
        {
            var items = _dbService.GetAllItems();
            var filteredItems = items
               .Where(i => i.Content.ToLower().Contains(_searchQuery.ToLower()))
               .ToList();
            SortClipboardItems(filteredItems);
        }

        void FilterByType(ContentType selectedType)
        {
            var items = _dbService.GetAllItems();
            var filtered = items
                .Where(i => selectedType == ContentType.All || i.Type == selectedType)
                .ToList();
            SortClipboardItems(filtered);
        }

        void GetAvailableTypes()
        {
            var items = _dbService.GetAllItems();
            var types = new[] { ContentType.All }
                .Concat(items.Select(x => x.Type).Distinct())
                .ToList();

            AvailableTypes.Clear();
            foreach (var t in types)
                AvailableTypes.Add(t);
        }

        void UpdateClipboard(IEnumerable<ClipboardItem> items)
        {
            ClipboardItems.Clear();
            foreach (var item in items)
                ClipboardItems.Add(item);
        }

        public void ClearClipboard()
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete all entries from the clipboard?",
                "Clipboard Cleanup",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;
            Clipboard.Clear();
            _dbService.ClearAllItems();
            LoadData();
            GetAvailableTypes();
        }

        public void ClearClipboardWithoutAsking()
        {
            Clipboard.Clear();
            _dbService.ClearAllItems();
            LoadData();
            GetAvailableTypes();
        }

        void SetClipboardItem(ClipboardItem item)
        {
            _suppressClipboardUpdate = true;
            if (item.Type == ContentType.Image)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Clipboard.SetImage(item.ImagePreview);
                    _suppressClipboardUpdate = false;
                });
            }
            else if (item.Type == ContentType.File)
            {
                var filePaths = item.FilePathList;
                var data = new DataObject(DataFormats.FileDrop, filePaths?.ToArray());
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Clipboard.SetDataObject(data, true);
                    _suppressClipboardUpdate = false;
                });
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Clipboard.SetText(item.Content);
                    _suppressClipboardUpdate = false;
                });
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _dbService.VacuumDatabase();
        }
    }

    public static class ImageUtils
    {
        public static string GetImageHash(BitmapSource bitmap)
        {
            using var ms = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(ms);
            ms.Position = 0;

            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(ms);
            return Convert.ToBase64String(hashBytes);
        }

        public static bool ImagesAreEqual(BitmapSource a, BitmapSource b)
        {
            return GetImageHash(a) == GetImageHash(b);
        }
    }
}
