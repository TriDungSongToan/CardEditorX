using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Data.SQLite;
using System.Windows;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ScriptSupport.Legacy.Models.Settings;
using ScriptSupport.Legacy.Services;
using ScriptAppContent = ScriptSupport.Legacy.Models.AppContext;


namespace ScriptSupport.Legacy.ViewModels
{
    public class ConfigViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<ConfigViewModel> _instance = new Lazy<ConfigViewModel>(() => new ConfigViewModel());
        public static ConfigViewModel Instance => _instance.Value;
        private readonly object _lock = new object();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public string SettingFilePath { get; set; }

        private ScriptSupport.Legacy.Models.Settings.UserSetting _userSetting;
        public ScriptSupport.Legacy.Models.Settings.UserSetting userSetting
        {
            get => _userSetting;
            set
            {
                if (_userSetting != value)
                {
                    _userSetting = value;
                    OnPropertyChanged(nameof(userSetting));
                }
            }
        }

        private ScriptSupport.Legacy.Models.Settings.DisplaySetting _displaySetting;
        public ScriptSupport.Legacy.Models.Settings.DisplaySetting displaySetting
        {
            get => _displaySetting;
            set
            {
                if (_displaySetting != value)
                {
                    _displaySetting = value;
                    OnPropertyChanged(nameof(displaySetting));
                }
            }
        }

        private ScriptSupport.Legacy.Models.Settings.DataHandlingSetting _dataHandlingSetting;
        public ScriptSupport.Legacy.Models.Settings.DataHandlingSetting dataHandlingSetting
        {
            get => _dataHandlingSetting;
            set
            {
                if (_dataHandlingSetting != value)
                {
                    _dataHandlingSetting = value;
                    OnPropertyChanged(nameof(dataHandlingSetting));
                }
            }
        }

        private ScriptSupport.Legacy.Models.Settings.CodeEditSetting _codeEditSetting;
        public ScriptSupport.Legacy.Models.Settings.CodeEditSetting codeEditSetting
        {
            get => _codeEditSetting;
            set
            {
                if (_codeEditSetting != value)
                {
                    _codeEditSetting = value;
                    OnPropertyChanged(nameof(codeEditSetting));
                }
            }
        }

        public ConfigViewModel()
        {
            SettingFilePath = Path.Combine(ScriptAppContent.Instance.ConfigFolderPath, "AppSetting.db");
            userSetting = new Models.Settings.UserSetting();
            displaySetting = new Models.Settings.DisplaySetting();
            dataHandlingSetting = new Models.Settings.DataHandlingSetting();
        }

        public void ReadSettingsPropertiesCommand()
        {
            displaySetting.Background = PropertiesSettingService.GetStringSetting("Background", "#FF000000");
            displaySetting.Foreground = PropertiesSettingService.GetStringSetting("Foreground", "#FFFFFFFF");
            displaySetting.Theme = PropertiesSettingService.GetStringSetting("Theme", "DeepPurple");
            displaySetting.FontFamily = PropertiesSettingService.GetStringSetting("FontFamily", "Consolas");
            displaySetting.FontSize = PropertiesSettingService.GetIntSetting("FontSize", 12, true, 50);
            displaySetting.HighLight = PropertiesSettingService.GetStringSetting("HighLight", "Default");
            displaySetting.FlowDirectionC = PropertiesSettingService.GetIntSetting("FlowDirectionC", 0);
            displaySetting.TextAlignmentC = PropertiesSettingService.GetIntSetting("TextAlignmentC", 0);
        }
        public void UpdateSettingsProperties()
        {
            Properties.Settings.Default.Background = displaySetting.Background;
            Properties.Settings.Default.Foreground = displaySetting.Foreground;
            Properties.Settings.Default.Theme = displaySetting.Theme;
            Properties.Settings.Default.FontFamily = displaySetting.FontFamily;
            Properties.Settings.Default.FontSize = displaySetting.FontSize.HasValue ? displaySetting.FontSize.Value : 12;
            Properties.Settings.Default.HighLight = displaySetting.HighLight;
            Properties.Settings.Default.FlowDirectionC = displaySetting.FlowDirectionC;
            Properties.Settings.Default.TextAlignmentC = displaySetting.TextAlignmentC;
        }
        public void SaveSettingsProperties()
        {
            Properties.Settings.Default.Save();
        }
        public void ResetSettingProperties()
        {
            Properties.Settings.Default.Reset();
        }
        /// <summary>
        /// Setting File
        /// </summary>
        public (bool, string) LoadSettingFIle()
        {
            if (string.IsNullOrEmpty(SettingFilePath) || !System.IO.File.Exists(SettingFilePath))
            {
                var (resultCreate, messageCreate) = CreateFileServices.CreateSetting(ScriptAppContent.Instance.ConfigFolderPath, "AppSetting.db");
                if (!resultCreate) return (false, messageCreate);
            }

            string connectionString = $"Data Source={SettingFilePath};Version=3;";

            try
            {
                userSetting = LoadSetting<UserSetting>("UserSetting", connectionString);
                dataHandlingSetting = LoadSetting<DataHandlingSetting>("DataHandlingSetting", connectionString);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public T LoadSetting<T>(string key, string _connectionString) where T : new()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            string query = "SELECT Value FROM Settings WHERE Key = @key";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@key", key);

            var result = command.ExecuteScalar();
            if (result == null) return new T(); // Trả về instance mới nếu chưa có

            return JsonSerializer.Deserialize<T>(result.ToString()) ?? new T();
        }

        public (bool, string) SaveSingleSettingFile<T>(string key, T settingObject)
        {
            if (string.IsNullOrEmpty(SettingFilePath) || !System.IO.File.Exists(SettingFilePath))
            {
                var (resultCreate, messageCreate) = CreateFileServices.CreateSetting(ScriptAppContent.Instance.ConfigFolderPath, "AppSetting.db");
                if (!resultCreate) return (false, messageCreate);
            }

            string connectionString = $"Data Source={SettingFilePath};Version=3;";

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false
            };

            try
            {
                using var connection = new SQLiteConnection(connectionString);
                connection.Open();

                string query = @"INSERT OR REPLACE INTO Settings
                     (Key, Value, LastModified)
                     VALUES (@key, @value, @modified)";

                using var command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@value", JsonSerializer.Serialize(settingObject, jsonOptions));
                command.Parameters.AddWithValue("@modified", DateTime.Now);

                command.ExecuteNonQuery();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (bool, string) SaveAllSettingFile()
        {
            if (string.IsNullOrEmpty(SettingFilePath) || !System.IO.File.Exists(SettingFilePath))
            {
                var (resultCreate, messageCreate) = CreateFileServices.CreateSetting(ScriptAppContent.Instance.ConfigFolderPath, "AppSetting.db");
                if (!resultCreate) return (false, messageCreate);
            }

            string connectionString = $"Data Source={SettingFilePath};Version=3;";
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false
            };

            using var connection = new SQLiteConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                InsertOrReplaceSetting(connection, "UserSetting", userSetting, jsonOptions);
                InsertOrReplaceSetting(connection, "DisplaySetting", displaySetting, jsonOptions);
                InsertOrReplaceSetting(connection, "DataHandlingSetting", dataHandlingSetting, jsonOptions);

                transaction.Commit();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return (false, ex.Message);
            }
        }
        private static void InsertOrReplaceSetting<T>(SQLiteConnection connection, string key,
            T settingObject, JsonSerializerOptions options)
        {
            string json = JsonSerializer.Serialize(settingObject, options);

            string query = @"INSERT OR REPLACE INTO Settings 
                     (Key, Value, LastModified)
                     VALUES (@key, @value, @modified)";

            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@value", json);
            command.Parameters.AddWithValue("@modified", DateTime.Now);

            command.ExecuteNonQuery();
        }

        public (bool, string) ResetSettingFile()
        {
            if (string.IsNullOrEmpty(SettingFilePath) || !System.IO.File.Exists(SettingFilePath))
            {
                var (resultCreate, messageCreate) = CreateFileServices.CreateSetting(ScriptAppContent.Instance.ConfigFolderPath, "AppSetting.db");
                if (!resultCreate) return (false, messageCreate);
            }

            string connectionString = $"Data Source={SettingFilePath};Version=3;";
            using var connection = new SQLiteConnection(connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                using (var deleteCmd = new SQLiteCommand("DELETE FROM Settings", connection))
                    deleteCmd.ExecuteNonQuery();

                CreateFileServices.CreateDefaultSettings(connection);
                transaction.Commit();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return (false, ex.Message);
            }
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
            userSetting = null;
            displaySetting = null;
            dataHandlingSetting = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
