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
using System.Diagnostics;

namespace SmartClipboard.Services
{
    internal class DatabaseService: IDatabaseService
    {
        private readonly string _dbPath = $"Data Source={GetDatabasePath()}";
        
        private readonly SettingsService _settingsService;


        public DatabaseService(SettingsService settingsService)
        {
            using var conn = new SQLiteConnection(_dbPath);
            conn.Execute(@"
            CREATE TABLE IF NOT EXISTS ClipboardItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Content TEXT NOT NULL,
                FilePathList TEXT,
                ImagePath TEXT,
                Timestamp TEXT NOT NULL,
                Type TEXT NOT NULL,
                IsPinned INTEGER NOT NULL DEFAULT 0
            );");
            _settingsService = settingsService;
        }
        private static string GetDatabasePath()
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SmartClipboard"
            );
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "clipboard.db");
        }


        public void InsertClipboardItem(ClipboardItem item)
        {
            using var conn = new SQLiteConnection(_dbPath);

            const string insertQuery = @"INSERT INTO ClipboardItems " +
                "(Content, Timestamp, Type, FilePathList, ImagePath) " +
                "VALUES " +
                "(@Content, @Timestamp, @Type, @FilePathList, @ImagePath);";

            conn.Execute(insertQuery, item);

            if (_settingsService.MaxItems > 0)
            {
                const string countQuery = "SELECT COUNT(*) FROM ClipboardItems WHERE IsPinned = 0;";
                int count = conn.ExecuteScalar<int>(countQuery);

                if (count > _settingsService.MaxItems)
                {
                    const string deleteQuery = @"
                        DELETE FROM ClipboardItems
                        WHERE Id = (
                            SELECT Id FROM ClipboardItems
                            WHERE IsPinned = 0
                            ORDER BY Timestamp ASC
                            LIMIT 1
                        );";
                    conn.Execute(deleteQuery);
                }
            }
        }
        public List<ClipboardItem> GetAllItems()
        {
            using var conn = new SQLiteConnection(_dbPath);

            var query = new StringBuilder();
            query.Append("SELECT * FROM ClipboardItems ORDER BY IsPinned DESC, Timestamp DESC");

            if (_settingsService.MaxItems > 0)
                query.Append(" LIMIT ").Append(_settingsService.MaxItems);

            var rows = conn.Query(query.ToString());

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
        public void VacuumDatabase()
        {
            try
            {
                using var conn = new SQLiteConnection(_dbPath);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "VACUUM;";
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("VACUUM error: " + ex.Message);
            }
        }

    }
}
