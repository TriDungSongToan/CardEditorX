using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CardEditor.Models.Settings
{
    public class DeckEditSetting : INotifyPropertyChanged
    {
        #region Limit
        private int? _mainDeckLimit = 60;
        public int? MainDeckLimit
        {
            get => _mainDeckLimit;
            set
            {
                if (_mainDeckLimit != value)
                {
                    _mainDeckLimit = value;
                    OnPropertyChanged(nameof(MainDeckLimit));
                }
            }
        }
        private int? _extraDeckLimit = 15;
        public int? ExtraDeckLimit
        {
            get => _extraDeckLimit;
            set
            {
                if (_extraDeckLimit != value)
                {
                    _extraDeckLimit = value;
                    OnPropertyChanged(nameof(ExtraDeckLimit));
                }
            }
        }
        private int? _sideDeckLimit = 15;
        public int? SideDeckLimit
        {
            get => _sideDeckLimit;
            set
            {
                if (_sideDeckLimit != value)
                {
                    _sideDeckLimit = value;
                    OnPropertyChanged(nameof(SideDeckLimit));
                }
            }
        }
        #endregion

        #region Options
        private bool _alternate = false;
        public bool Alternate
        {
            get => _alternate;
            set
            {
                if (_alternate != value)
                {
                    _alternate = value;
                    OnPropertyChanged(nameof(Alternate));
                }
            }
        }
        private bool _listMode = true;
        public bool ListMode
        {
            get => _listMode;
            set
            {
                if (_listMode != value)
                {
                    _listMode = value;
                    OnPropertyChanged(nameof(ListMode));
                }
            }
        }

        private bool _displayID = true;
        public bool DisplayID
        {
            get => _displayID;
            set
            {
                if (_displayID != value)
                {
                    _displayID = value;
                    OnPropertyChanged(nameof(DisplayID));
                }
            }
        }
        private bool _displayArtchetype = true;
        public bool DisplayArtchetype
        {
            get => _displayArtchetype;
            set
            {
                if (_displayArtchetype != value)
                {
                    _displayArtchetype = value;
                    OnPropertyChanged(nameof(DisplayArtchetype));
                }
            }
        }

        private bool _displayGPoint = true;
        public bool DisplayGPoint
        {
            get => _displayGPoint;
            set
            {
                if (_displayGPoint != value)
                {
                    _displayGPoint = value;
                    OnPropertyChanged(nameof(DisplayGPoint));
                }
            }
        }
        private bool _displayScopeImg = true;
        public bool DisplayScopeImg
        {
            get => _displayScopeImg;
            set
            {
                if (_displayScopeImg != value)
                {
                    _displayScopeImg = value;
                    OnPropertyChanged(nameof(DisplayScopeImg));
                }
            }
        }

        private bool _ritualPlaceExtra = false;
        public bool RitualPlaceExtra
        {
            get => _ritualPlaceExtra;
            set
            {
                if (_ritualPlaceExtra != value)
                {
                    _ritualPlaceExtra = value;
                    OnPropertyChanged(nameof(RitualPlaceExtra));
                }
            }
        }
        private bool _saveCardName = false;
        public bool SaveCardName
        {
            get => _saveCardName;
            set
            {
                if (_saveCardName != value)
                {
                    _saveCardName = value;
                    OnPropertyChanged(nameof(SaveCardName));
                }
            }
        }

        private bool _ignoreDeckSize = false;
        public bool IgnoreDeckSize
        {
            get => _ignoreDeckSize;
            set
            {
                if (_ignoreDeckSize != value)
                {
                    _ignoreDeckSize = value;
                    OnPropertyChanged(nameof(IgnoreDeckSize));
                }
            }
        }
        private bool _ignoreDeckContent = false;
        public bool IgnoreDeckContent
        {
            get => _ignoreDeckContent;
            set
            {
                if (_ignoreDeckContent != value)
                {
                    _ignoreDeckContent = value;
                    OnPropertyChanged(nameof(IgnoreDeckContent));
                }
            }
        }
        #endregion

        public DeckEditSetting Clone()
        {
            var json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<DeckEditSetting>(json)!;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
