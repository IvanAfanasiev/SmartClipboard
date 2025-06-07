using SmartClipboard.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClipboard.Models
{
    internal class ClipboardItem
    {
        public long Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public ContentType Type { get; set; } = ContentType.Text;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public ClipboardItem() { }

        public ClipboardItem(string content, ContentType type)
        {
            Content = content;
            Type = type;
            Timestamp = DateTime.Now;
        }

    }
}
