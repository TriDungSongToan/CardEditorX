using System;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Commands;
using CardEditor.Collections;
using CardAppContent = CardEditor.Models.AppContext;

namespace CardEditor.ViewModels
{
    public class OpenHistoryViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<OpenHistoryViewModel> _instance = new Lazy<OpenHistoryViewModel>(() => new OpenHistoryViewModel());
        public static OpenHistoryViewModel Instance => _instance.Value;

        #region Variable
        private readonly string RecentDBPath;
        private const int MaxItems = 10;
        #endregion

        #region Property
        public BulkObservableCollection<RecentItem> Archives { get; } = new();
        public BulkObservableCollection<RecentItem> Databases { get; } = new();
        public BulkObservableCollection<RecentItem> Scripts { get; } = new();
        public BulkObservableCollection<RecentItem> Decks { get; } = new();
        public BulkObservableCollection<RecentItem> BanLists { get; } = new();
        private BulkObservableCollection<RecentItem> GetCollection(RecentFileType type) => type switch
        {
            RecentFileType.Archive => Archives,
            RecentFileType.Database => Databases,
            RecentFileType.Script => Scripts,
            RecentFileType.Deck => Decks,
            RecentFileType.BanList => BanLists,
            _ => Databases
        };
        public ICommand ClearCommand { get; }
        #endregion

        #region Constructor
        private OpenHistoryViewModel()
        {
            string OpenHistoryPath = System.IO.Path.Combine(CardAppContent.Instance.DataFolderPath, "RecentOpen");
            if(!System.IO.Directory.Exists(OpenHistoryPath))
                Directory.CreateDirectory(OpenHistoryPath);
            RecentDBPath = Path.Combine(OpenHistoryPath, "RecentOpen.db");

            if (!IsDatabaseValid())
            {
                try
                {
                    InitializeDatabase();
                }
                catch { }
            }
            ClearCommand = new RelayCommand(p => { var t = (RecentFileType)p; ClearRecentItems(t); Reload(t); });
            
        }
        private bool IsDatabaseValid()
        {
            if (!File.Exists(RecentDBPath)) return false;

            try
            {
                using (var connection = new SQLiteConnection($"Data Source={RecentDBPath};Version=3;"))
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
            using (var connection = new SQLiteConnection($"Data Source={RecentDBPath};Version=3;"))
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
        #endregion

        #region Methods
        public void UpdateRecentItem(string url, RecentFileType typeItem)
        {
            if (!File.Exists(url)) return; // Kiểm tra file tồn tại

            using (var connection = new SQLiteConnection($"Data Source={RecentDBPath};Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    string table = typeItem switch
                    {
                        RecentFileType.Archive => "archive",
                        RecentFileType.Database => "database",
                        RecentFileType.Script => "script",
                        RecentFileType.Deck => "deck",
                        RecentFileType.BanList => "banlist",
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
            Reload(typeItem);
        }
        public List<(DateTime Time, string Url)> GetRecentItems(RecentFileType typeItem)
        {
            var items = new List<(DateTime Time, string Url)>();
            using (var connection = new SQLiteConnection($"Data Source={RecentDBPath};Version=3;"))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    string table = typeItem switch
                    {
                        RecentFileType.Archive => "archive",
                        RecentFileType.Database => "database",
                        RecentFileType.Script => "script",
                        RecentFileType.Deck => "deck",
                        RecentFileType.BanList => "banlist",
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
        public void ClearRecentItems(RecentFileType typeItem)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={RecentDBPath};Version=3;"))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand(connection))
                    {
                        string table = typeItem switch
                        {
                            RecentFileType.Archive => "archive",
                            RecentFileType.Database => "database",
                            RecentFileType.Script => "script",
                            RecentFileType.Deck => "deck",
                            RecentFileType.BanList => "banlist",
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
        public void Reload(RecentFileType type)
        {
            GetCollection(type).ReplaceAll(GetRecentItems(type).Where(x => File.Exists(x.Url)).Select(x => new RecentItem(type, x.Time, x.Url)));
        }
        #endregion

        #region Event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {

        }
        #endregion

    }
}
