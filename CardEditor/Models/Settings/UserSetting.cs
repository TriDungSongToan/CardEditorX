using System.Text.Json;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CardEditor.Collections;

namespace CardEditor.Models.Settings
{
    public class UserSettingSource : INotifyPropertyChanged
    {
        private BulkObservableCollection<string> _languages;
        public BulkObservableCollection<string> Languages
        {
            get => _languages;
            set
            {
                if (!ReferenceEquals(_languages, value))
                {
                    _languages = value ?? new BulkObservableCollection<string>();
                    OnPropertyChanged();
                }
            }
        }
        private BulkObservableCollection<string> _games;
        public BulkObservableCollection<string> Games
        {
            get => _games;
            set
            {
                if (!ReferenceEquals(_games, value))
                {
                    _games = value ?? new BulkObservableCollection<string>();
                    OnPropertyChanged();
                }
            }
        }

        public UserSettingSource()
        {
            _languages = new BulkObservableCollection<string>();
            _games = new BulkObservableCollection<string>();
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            _languages ??= new BulkObservableCollection<string>();
            _games ??= new BulkObservableCollection<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class UserSetting : INotifyPropertyChanged
    {
        private string _userName = "User";
        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }
        private string _apiKey = string.Empty;
        public string ApiKey
        {
            get => _apiKey;
            set
            {
                if (_apiKey != value)
                {
                    _apiKey = value;
                    OnPropertyChanged(nameof(ApiKey));
                }
            }
        }
        private string _apiEndpoint = string.Empty;
        public string ApiEndpoint
        {
            get => _apiEndpoint;
            set
            {
                if (_apiEndpoint != value)
                {
                    _apiEndpoint = value;
                    OnPropertyChanged(nameof(ApiEndpoint));
                }
            }
        }
        private string _language = "English";
        public string Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    OnPropertyChanged(nameof(Language));
                }
            }
        }
        private string _dataSource = string.Empty;
        public string DataSource
        {
            get => _dataSource;
            set
            {
                if (_dataSource != value)
                {
                    _dataSource = value;
                    OnPropertyChanged(nameof(DataSource));
                }
            }
        }
        private string _game = "EDOPro";
        public string Game
        {
            get => _game;
            set
            {
                if (_game != value)
                {
                    _game = value;
                    OnPropertyChanged(nameof(Game));
                }
            }
        }

        public UserSetting Clone()
        {
            var json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<UserSetting>(json)!;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
