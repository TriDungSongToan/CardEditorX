using System.Text.Json;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace CardEditor.Models.Settings
{
    public class DataHandlingSetting : INotifyPropertyChanged
    {
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
        private int _writeMode = 0;
        public int WriteMode
        {
            get => _writeMode;
            set
            {
                if (_writeMode != value)
                {
                    _writeMode = value;
                    OnPropertyChanged(nameof(WriteMode));
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

        private bool _confirmClear = true;
        public bool ConfirmClear
        {
            get => _confirmClear;
            set
            {
                if (_confirmClear != value)
                {
                    _confirmClear = value;
                    OnPropertyChanged(nameof(ConfirmClear));
                }
            }
        }
        private bool _confirmDelete = true;
        public bool ConfirmDelete
        {
            get => _confirmDelete;
            set
            {
                if (_confirmDelete != value)
                {
                    _confirmDelete = value;
                    OnPropertyChanged(nameof(ConfirmDelete));
                }
            }
        }
        private bool _confirmReSet = true;
        public bool ConfirmReSet
        {
            get => _confirmReSet;
            set
            {
                if (_confirmReSet != value)
                {
                    _confirmReSet = value;
                    OnPropertyChanged(nameof(ConfirmReSet));
                }
            }
        }
        private bool _confirmReLoad = true;
        public bool ConfirmReLoad
        {
            get => _confirmReLoad;
            set
            {
                if (_confirmReLoad != value)
                {
                    _confirmReLoad = value;
                    OnPropertyChanged(nameof(ConfirmReLoad));
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
