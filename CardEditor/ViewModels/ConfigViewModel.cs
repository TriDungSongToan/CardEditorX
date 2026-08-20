using System;
using System.IO;
using System.Text.Json;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using SkiaSharp;
using CardEditor.Models;
using CardEditor.Models.Settings;
using CardEditor.Services;
using CardAppContext = CardEditor.Models.AppContext;

namespace CardEditor.ViewModels
{
    public class ConfigViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<ConfigViewModel> _instance = new Lazy<ConfigViewModel>(() => new ConfigViewModel());
        public static ConfigViewModel Instance => _instance.Value;


        public string SettingFilePath { get; set; }

        private CardEditor.Models.Settings.UserSetting _userSetting;
        public CardEditor.Models.Settings.UserSetting userSetting
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

        private CardEditor.Models.Settings.DisplaySetting _displaySetting;
        public CardEditor.Models.Settings.DisplaySetting displaySetting
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

        private CardEditor.Models.Settings.DataHandlingSetting _dataHandlingSetting;
        public CardEditor.Models.Settings.DataHandlingSetting dataHandlingSetting
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

        private CardEditor.Models.Settings.ImageSetting _imageSetting;
        public CardEditor.Models.Settings.ImageSetting imageSetting
        {
            get => _imageSetting;
            set
            {
                if (_imageSetting != value)
                {
                    if (_imageSetting != null) _imageSetting.PropertyChanged -= ImageSetting_PropertyChanged;
                    _imageSetting = value;
                    if (_imageSetting != null) _imageSetting.PropertyChanged += ImageSetting_PropertyChanged;
                    OnPropertyChanged(nameof(imageSetting));
                }
            }
        }

        private CardEditor.Models.Settings.CodeEditSetting _codeEditSetting;
        public CardEditor.Models.Settings.CodeEditSetting codeEditSetting
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

        private CardEditor.Models.Settings.DeckEditSetting _deckEditSetting;
        public CardEditor.Models.Settings.DeckEditSetting deckEditSetting
        {
            get => _deckEditSetting;
            set
            {
                if (_deckEditSetting != value)
                {
                    _deckEditSetting = value;
                    OnPropertyChanged(nameof(deckEditSetting));
                }
            }
        }
        public ImageSize imageSize { get; set; }

        private int _imageSeries = 4;
        public int ImageSeries
        {
            get => _imageSeries;
            set
            {
                if (_imageSeries != value)
                {
                    _imageSeries = value;
                    OnPropertyChanged(nameof(ImageSeries));
                    if (imageSetting.Series != _imageSeries)
                    {
                        imageSetting.Series = _imageSeries;
                        GeneraImageViewModel.Instance.IsChangedSeries = true;
                    }
                }
            }
        }
        private int _imageRare = 0;
        public int ImageRare
        {
            get => _imageRare;
            set
            {
                if (_imageRare != value)
                {
                    _imageRare = value;
                    OnPropertyChanged(nameof(ImageRare));
                    if (imageSetting.Rare != _imageRare)
                    {
                        imageSetting.Rare = _imageRare;
                        GeneraImageViewModel.Instance.IsChangedRare = true;
                    }
                }
            }
        }
        public string DeveloperEncrypted { get; set; } = string.Empty;

        public ConfigViewModel()
        {
            SettingFilePath = Path.Combine(CardAppContext.Instance.ConfigFolderPath, "AppSetting.db");
            userSetting = new Models.Settings.UserSetting();
            displaySetting = new Models.Settings.DisplaySetting();
            dataHandlingSetting = new Models.Settings.DataHandlingSetting();
            imageSetting = new Models.Settings.ImageSetting();
            codeEditSetting = new Models.Settings.CodeEditSetting();
            deckEditSetting = new Models.Settings.DeckEditSetting();
            imageSize = new ImageSize();
            imageSize.PropertyChanged += ImageSize_PropertyChanged;
        }
        private void ImageSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(ImageSetting.Series)) && (ImageSeries != imageSetting.Series))
            {
                ImageSeries = imageSetting.Series;
                GeneraImageViewModel.Instance.IsChangedSeries = true;
            }
            if ((e.PropertyName == nameof(ImageSetting.ImageSize)) && (imageSize.Size != imageSetting.ImageSize))
            {
                imageSize.Size = imageSetting.ImageSize;
            }
            if ((e.PropertyName == nameof(ImageSetting.Rare)) && (ImageRare != imageSetting.Rare))
            {
                ImageRare = imageSetting.Rare;
                GeneraImageViewModel.Instance.IsChangedRare = true;
            }
            
            if (e.PropertyName == nameof(CardEditor.Models.Settings.ImageSetting.YAWFullTrans) ||
                e.PropertyName == nameof(CardEditor.Models.Settings.ImageSetting.YAWFullOpaque) ||
                e.PropertyName == nameof(CardEditor.Models.Settings.ImageSetting.HeightFullTrans) ||
                e.PropertyName == nameof(CardEditor.Models.Settings.ImageSetting.HeightFullOpaque))
            {
                SetArtWorkFullSquareRect();
            }
        }
        private void SetArtWorkFullSquareRect()
        {
            var opaqueRect = new SKRect(left: 46, top: imageSetting.YAWFullOpaque,
                right: 46 + 1296, bottom: imageSetting.YAWFullOpaque + imageSetting.HeightFullOpaque);
            var transparentRect = new SKRect(left: 0, top: imageSetting.YAWFullTrans,
                right: 1388, bottom: imageSetting.YAWFullTrans + imageSetting.HeightFullTrans);
            GeneraImageViewModel.Instance.CurrentImageInfo.AWFullSquareOpaqueRect = opaqueRect;
            GeneraImageViewModel.Instance.CurrentImageInfo.AWFullSquareTransparenRect = transparentRect;
        }
        private void ImageSize_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(ImageSize.Size)) && (imageSetting.ImageSize != imageSize.Size))
            {
                imageSetting.ImageSize = imageSize.Size;
            }
        }

        public void ReadSettingsPropertiesCommand()
        {
            DeveloperEncrypted = PropertiesSettingService.GetStringSetting("DeveloperEncrypted", string.Empty);
        }
        public void UpdateSettingsProperties()
        {
            Properties.Settings.Default.DeveloperEncrypted = DeveloperEncrypted;
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
                var (resultCreate, messageCreate) = CreateFileServices.CreateSetting(CardAppContext.Instance.ConfigFolderPath, "AppSetting.db");
                if (!resultCreate) return (false, messageCreate);
            }

            string connectionString = $"Data Source={SettingFilePath};Version=3;";

            try
            {
                userSetting = LoadSetting<UserSetting>("UserSetting", connectionString);
                displaySetting = LoadSetting<DisplaySetting>("DisplaySetting", connectionString);
                dataHandlingSetting = LoadSetting<DataHandlingSetting>("DataHandlingSetting", connectionString);
                imageSetting = LoadSetting<ImageSetting>("ImageSetting", connectionString);
                codeEditSetting = LoadSetting<CodeEditSetting>("CodeEditSetting", connectionString);
                deckEditSetting = LoadSetting<DeckEditSetting>("DeckEditSetting", connectionString);
                imageSize.Size = imageSetting.ImageSize;
                ImageRare = imageSetting.Rare;

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
                var (resultCreate, messageCreate) = CreateFileServices.CreateSetting(CardAppContext.Instance.ConfigFolderPath, "AppSetting.db");
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
        public (bool, string) SaveDisplaySettingFile()
        {
            return SaveSingleSettingFile("DisplaySetting", displaySetting);
        }
        public (bool, string) SaveAllSettingFile()
        {
            if (string.IsNullOrEmpty(SettingFilePath) || !System.IO.File.Exists(SettingFilePath))
            {
                var (resultCreate, messageCreate) = CreateFileServices.CreateSetting(CardAppContext.Instance.ConfigFolderPath, "AppSetting.db");
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
                InsertOrReplaceSetting(connection, "ImageSetting", imageSetting, jsonOptions);
                InsertOrReplaceSetting(connection, "CodeEditSetting", codeEditSetting, jsonOptions);
                InsertOrReplaceSetting(connection, "DeckEditSetting", deckEditSetting, jsonOptions);

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
                var (resultCreate, messageCreate) = CreateFileServices.CreateSetting(CardAppContext.Instance.ConfigFolderPath, "AppSetting.db");
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
            if (imageSize != null)
            {
                imageSize.PropertyChanged -= ImageSize_PropertyChanged;
                imageSize = null;
            }
            userSetting = null;
            displaySetting = null;
            dataHandlingSetting = null;
            imageSetting = null;
            codeEditSetting = null;
            deckEditSetting = null;
            imageSize = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
