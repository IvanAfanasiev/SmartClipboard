using SmartClipboard.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SmartClipboard.Models
{
    internal class ClipboardItem: INotifyPropertyChanged
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? FilePathList { get; set; }
        public string? ImagePath { get; set; }
        public BitmapImage? ImagePreview => string.IsNullOrEmpty(ImagePath)
            ? null
            : new BitmapImage(new Uri(ImagePath));
        public string? FilePath { get; set; }
        public ContentType Type { get; set; } = ContentType.Text;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        private bool _isPinned = false;
        public bool IsPinned
        {
            get => _isPinned;
            set
            {
                if (_isPinned != value)
                {
                    _isPinned = value;
                    OnPropertyChanged(nameof(IsPinned));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ClipboardItem() { }

        public ClipboardItem(string content, ContentType type)
        {
            Content = content;
            Type = type;
            Timestamp = DateTime.Now;
        }

    }
}
