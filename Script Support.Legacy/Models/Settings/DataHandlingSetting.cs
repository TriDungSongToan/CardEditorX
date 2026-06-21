using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScriptSupport.Legacy.Models.Settings
{
    public class DataHandlingSetting : INotifyPropertyChanged
    {
        private bool _autoSearch = true;
        public bool AutoSearch
        {
            get => _autoSearch;
            set
            {
                if (_autoSearch != value)
                {
                    _autoSearch = value;
                    OnPropertyChanged(nameof(AutoSearch));
                }
            }
        }
        private bool _allowSave = false;
        public bool AllowSave
        {
            get => _allowSave;
            set
            {
                if (_allowSave != value)
                {
                    _allowSave = value;
                    OnPropertyChanged(nameof(AllowSave));
                }
            }
        }
        private bool _allowNew = false;
        public bool AllowNew
        {
            get => _allowNew;
            set
            {
                if (_allowNew != value)
                {
                    _allowNew = value;
                    OnPropertyChanged(nameof(AllowNew));
                }
            }
        }

        private int _advanced = 5;
        public int Advanced
        {
            get => _advanced;
            set
            {
                if (_advanced != value)
                {
                    _advanced = value;
                    OnPropertyChanged(nameof(Advanced));
                }
            }
        }

        private int _filterMode = 0;
        public int FilterMode
        {
            get => _filterMode;
            set
            {
                if (_filterMode != value)
                {
                    _filterMode = value;
                    OnPropertyChanged(nameof(FilterMode));
                }
            }
        }

        public DataHandlingSetting Clone()
        {
            var json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<DataHandlingSetting>(json)!;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}