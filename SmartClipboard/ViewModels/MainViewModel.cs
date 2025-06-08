using SmartClipboard.Models;
using SmartClipboard.Services;
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
using System.Windows.Media.Imaging;

namespace SmartClipboard.ViewModels
{
    internal class MainViewModel: INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService = new();
        private string _lastText = string.Empty;
        private string? _lastImageHash;

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
        public void SaveClipboardImage(BitmapSource image)
        {
            _lastText = String.Empty;
            var hash = ImageUtils.GetImageHash(image);

            if (hash == _lastImageHash)
                return;

            _lastImageHash = hash;

            string filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmssfff}.png";
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartClipboard", "Images");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fullPath = Path.Combine(folder, filename);

            using var fileStream = new FileStream(fullPath, FileMode.Create);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.Save(fileStream);

            var item = new ClipboardItem
            {
                Timestamp = DateTime.Now,
                ImagePath = fullPath,
                Type = ContentType.Image,
            };

            _dbService.InsertClipboardItem(item);
            ClipboardItems.Insert(0, item);
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

                _dbService.InsertClipboardItem(item);
                ClipboardItems.Insert(0, item);
            }
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
