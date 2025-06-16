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
using System.Text.Json;
using SmartClipboard.Utilities;

namespace SmartClipboard.Services
{
    internal class DatabaseService: IDatabaseService
    {
        private readonly string _dbPath = "Data Source=clipboard.db";

        public DatabaseService()
        {
            using var conn = new SQLiteConnection(_dbPath);
            conn.Execute(@"
            CREATE TABLE IF NOT EXISTS ClipboardItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Content TEXT NOT NULL,
                FilePathList TEXT,
                FilePath TEXT,
                ImagePath TEXT,
                Timestamp TEXT NOT NULL,
                Type TEXT NOT NULL,
                IsPinned INTEGER NOT NULL DEFAULT 0
            );");
        }
        public void InsertClipboardItem(ClipboardItem item)
        {
            using var conn = new SQLiteConnection(_dbPath);
            conn.Execute("INSERT INTO ClipboardItems " +
                "(Content, Timestamp, Type, FilePath, FilePathList, ImagePath) " +
                "VALUES " +
                "(@Content, @Timestamp, @Type, @FilePath, @FilePathList, @ImagePath)",
                new
                {
                    Content = item.Content,
                    FilePath = item.FilePath,
                    FilePathList = JsonSerializer.Serialize(item.FilePathList),
                    ImagePath = item.ImagePath,
                    Timestamp = DateTime.Now.ToString("s"),
                    Type = item.Type
                });
        }

        public List<ClipboardItem> GetAllItems()
        {
            using var conn = new SQLiteConnection(_dbPath);
            string query = "SELECT * FROM ClipboardItems ORDER BY IsPinned DESC, Timestamp DESC";
            var rows = conn.Query(query);

            var result = new List<ClipboardItem>();
            foreach (var row in rows)
            {
                result.Add(new ClipboardItem
                {
                    Id = row.Id,
                    Content = row.Content,
                    FilePathList = string.IsNullOrWhiteSpace((string?)row.FilePathList)
                        ? String.Empty
                        : JsonSerializer.Deserialize<string>((string)row.FilePathList),
                    ImagePath = row.ImagePath,
                    FilePath = row.FilePath,
                    Timestamp = DateTime.Parse(row.Timestamp),
                    Type = Enum.TryParse<ContentType>((string)row.Type, out var type) ? type : ContentType.Unknown,
                    IsPinned = row.IsPinned == 1
                });
            }

            return result;
        }

        public void UpdateClipboardItem(ClipboardItem item)
        {
            using var conn = new SQLiteConnection(_dbPath);
            conn.Execute("UPDATE ClipboardItems SET IsPinned = @IsPinned WHERE Id = @Id", item);
        }

        public void DeleteClipboardItem(ClipboardItem item)
        {
            using var conn = new SQLiteConnection(_dbPath);
            conn.Execute("DELETE FROM ClipboardItems WHERE Id = @Id", item);
        }

        public void ClearAllItems()
        {
            using var conn = new SQLiteConnection(_dbPath);

            conn.Execute("DELETE FROM ClipboardItems WHERE IsPinned != 1;" +
                        "DELETE FROM sqlite_sequence WHERE name='ClipboardItems';");
        }

    }
}
