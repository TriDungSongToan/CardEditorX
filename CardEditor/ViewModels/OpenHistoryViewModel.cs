using System;
using System.IO;
using System.Data.SQLite;
using System.Collections.Generic;

namespace CardEditor.ViewModels
{
    public class OpenHistoryViewModel
    {
        private static OpenHistoryViewModel _instance;
        private string DataFolderPath => CardEditor.Models.AppContext.Instance.DataFolderPath;
        private readonly string _dbPath;
        private const int MaxItems = 10;

        public static OpenHistoryViewModel Instance
        {
            get => _instance ??= new OpenHistoryViewModel();
        }

        private OpenHistoryViewModel()
        {
            string OpenHistoryPath = System.IO.Path.Combine(DataFolderPath, "RecentOpen");
            if(!System.IO.Directory.Exists(OpenHistoryPath))
                Directory.CreateDirectory(OpenHistoryPath);
            _dbPath = Path.Combine(OpenHistoryPath, "RecentOpen.db");

            if (!IsDatabaseValid())
            {
                try
                {
                    InitializeDatabase();
                }
                catch
                {
                    ///
                }
            }    
                
        }
        private bool IsDatabaseValid()
        {
            if (!File.Exists(_dbPath)) return false;

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='database';", connection))
                    {
                        return command.ExecuteScalar() != null;
                    }
                }
            }
            catch (SQLiteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database không hợp lệ: {ex.Message}");
                return false;
            }
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS archive (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            time DATETIME NOT NULL,
                            url TEXT NOT NULL UNIQUE
                        );
                        CREATE TABLE IF NOT EXISTS database (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            time DATETIME NOT NULL,
                            url TEXT NOT NULL UNIQUE
                        );
                        CREATE TABLE IF NOT EXISTS script (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            time DATETIME NOT NULL,
                            url TEXT NOT NULL UNIQUE
                        );
                        CREATE TABLE IF NOT EXISTS deck (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            time DATETIME NOT NULL,
                            url TEXT NOT NULL UNIQUE
                        );
                        CREATE TABLE IF NOT EXISTS banlist (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            time DATETIME NOT NULL,
                            url TEXT NOT NULL UNIQUE
                        );
                        CREATE INDEX IF NOT EXISTS idx_database_time ON database (time);
                        CREATE INDEX IF NOT EXISTS idx_script_time ON script (time);
                        CREATE INDEX IF NOT EXISTS idx_deck_time ON deck (time);
                        CREATE INDEX IF NOT EXISTS idx_banlist_time ON banlist (time);";
                        
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateRecentItem(string url, int typeItem)
        {
            if (!File.Exists(url)) return; // Kiểm tra file tồn tại

            using (var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    string table = typeItem switch
                    {
                        0 => "archive",
                        1 => "database",
                        2 => "script",
                        3 => "deck",
                        4 => "banlist",
                        _ => "database"
                    };

                    // Kiểm tra url có tồn tại không
                    command.CommandText = $"SELECT COUNT(*) FROM {table} WHERE url = @url";
                    command.Parameters.AddWithValue("@url", url);
                    long exists = (long)command.ExecuteScalar();

                    if (exists > 0)
                    {
                        // Cập nhật thời gian
                        command.CommandText = $"UPDATE {table} SET time = @time WHERE url = @url";
                        command.Parameters.AddWithValue("@time", DateTime.Now);
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        // Thêm dòng mới
                        command.CommandText = $"INSERT INTO {table} (time, url) VALUES (@time, @url)";
                        command.Parameters.AddWithValue("@time", DateTime.Now);
                        command.ExecuteNonQuery();

                        // Xóa dòng cũ nhất nếu vượt quá MaxItems
                        command.CommandText = $"DELETE FROM {table} WHERE id IN (SELECT id FROM {table} ORDER BY time DESC LIMIT -1 OFFSET @max)";
                        command.Parameters.AddWithValue("@max", MaxItems);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<(DateTime Time, string Url)> GetRecentItems(int typeItem)
        {
            var items = new List<(DateTime Time, string Url)>();
            using (var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    string table = typeItem switch
                    {
                        0 => "archive",
                        1 => "database",
                        2 => "script",
                        3 => "deck",
                        4 => "banlist",
                        _ => "database"
                    };

                    command.CommandText = $"SELECT time, url FROM {table} ORDER BY time DESC LIMIT @max";
                    command.Parameters.AddWithValue("@max", MaxItems);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add((reader.GetDateTime(0), reader.GetString(1)));
                        }
                    }
                }
            }
            return items;
        }

        public void ClearRecentItems(int typeItem)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(connection))
                    {
                        string table = typeItem switch
                        {
                            0 => "archive",
                            1 => "database",
                            2 => "script",
                            3 => "deck",
                            4 => "banlist",
                            _ => "database"
                        };

                        command.CommandText = $"DELETE FROM {table}";
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SQLiteException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xóa dữ liệu RecentOpen.db: {ex.Message}");
            }
        }
    }
}
