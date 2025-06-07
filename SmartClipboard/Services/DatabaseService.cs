using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Dapper;
using SmartClipboard.Models;
using System.Windows;
using System.IO;

namespace SmartClipboard.Services
{
    internal class DatabaseService
    {
        private readonly string _dbPath = "Data Source=clipboard.db";

        public DatabaseService()
        {
            using var conn = new SQLiteConnection(_dbPath);
            conn.Execute(@"
            CREATE TABLE IF NOT EXISTS ClipboardItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Content TEXT NOT NULL,
                Timestamp TEXT NOT NULL,
                Type TEXT NOT NULL
            );");
        }
        public void InsertClipboardItem(ClipboardItem item)
        {
            using var conn = new SQLiteConnection(_dbPath);
            conn.Execute("INSERT INTO ClipboardItems (Content, Timestamp, Type) VALUES (@Content, @Timestamp, @Type)",
                new
                {
                    Content = item.Content,
                    Timestamp = DateTime.Now.ToString("s"),
                    Type = item.Type
                });
        }

        public List<ClipboardItem> GetAllItems()
        {
            using var conn = new SQLiteConnection(_dbPath);

            var rows = conn.Query("SELECT * FROM ClipboardItems ORDER BY Timestamp DESC");
            var result = new List<ClipboardItem>();

            foreach (var row in rows)
            {
                result.Add(new ClipboardItem
                {
                    Id = row.Id,
                    Content = row.Content,
                    Timestamp = DateTime.Parse(row.Timestamp),
                    Type = Enum.TryParse<ContentType>((string)row.Type, out var type) ? type : ContentType.Unknown
                });
            }

            return result;
        }
    }
}
