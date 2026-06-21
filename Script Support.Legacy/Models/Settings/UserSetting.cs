using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ScriptSupport.Legacy.Collections;

namespace ScriptSupport.Legacy.Models.Settings
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
        private string _browserPath = string.Empty;
        public string BrowserPath
        {
            get => _browserPath;
            set
            {
                if (_browserPath != value)
                {
                    _browserPath = value;
                    OnPropertyChanged(nameof(BrowserPath));
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