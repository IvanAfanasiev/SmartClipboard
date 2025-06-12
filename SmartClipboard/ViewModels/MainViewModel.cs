using SmartClipboard.Models;
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
    internal class MainViewModel: INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService = new();
        private string _lastText = string.Empty;
        private string? _lastImageHash;

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                SearchItems();
            }
        }

        public ObservableCollection<ClipboardItem> ClipboardItems { get; } = new();
        public ObservableCollection<ContentType> AvailableTypes { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand TogglePinCommand => new RelayCommand<ClipboardItem>(TogglePin);
        public ICommand FilterByTypeCommand => new RelayCommand<ContentType>(FilterByType);

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            LoadData();
            GetAvailableTypes();
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
            InsertItem(item);
            GetAvailableTypes();
        }
        public void SaveClipboardImage(BitmapSource image)
        {
            _lastText = String.Empty;
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
                Content = "screenshot",
                Timestamp = DateTime.Now,
                ImagePath = fullPath,
                Type = ContentType.Image,
            };
            InsertItem(item);
            GetAvailableTypes();
        }
        public void SaveClipboardFiles(IEnumerable<string> paths)
        {
            _lastText = String.Empty;
            
            foreach (var path in paths)
            {
                var item = new ClipboardItem
                {
                    Timestamp = DateTime.Now,
                    Content = string.Join(",\n", paths.Select(Path.GetFileName)),
                    FilePathList = string.Join(";\n", paths),
                    FilePath = path,
                    Type = ContentType.File
                };
                InsertItem(item);
            }
            GetAvailableTypes();
        }

        public void TogglePin(ClipboardItem item)
        {
            item.IsPinned = !item.IsPinned;
            _dbService.UpdateClipboardItem(item);
            SortClipboardItems(ClipboardItems);
        }

        void InsertItem(ClipboardItem item)
        {
            _dbService.InsertClipboardItem(item);
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                SearchItems();
                return;
            }
            int insertIndex = ClipboardItems.TakeWhile(i => i.IsPinned).Count();
            ClipboardItems.Insert(insertIndex, item);
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
            var filtered = _dbService
                .GetAllItems()
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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
