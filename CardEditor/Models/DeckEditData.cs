using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace CardEditor.Models
{
    public class DeckEditData : INotifyPropertyChanged
    {
        private int _limit = 5;
        public int Limit
        {
            get => _limit;
            set
            {
                if (_limit != value)
                {
                    _limit = value;
                    OnPropertyChanged(nameof(Limit));
                }
            }
        }
        private ulong _selectedType = 0;
        public ulong SelectedType
        {
            get => _selectedType;
            set
            {
                if (_selectedType != value)
                {
                    _selectedType = value;
                    OnPropertyChanged(nameof(SelectedType));
                }
            }
        }
        private ulong _selectedAttribute = 0;
        public ulong SelectedAttribute
        {
            get => _selectedAttribute;
            set
            {
                if (_selectedAttribute != value)
                {
                    _selectedAttribute = value;
                    OnPropertyChanged(nameof(SelectedAttribute));
                }
            }
        }
        private ulong _selectedRace = 0;
        public ulong SelectedRace
        {
            get => _selectedRace;
            set
            {
                if (_selectedRace != value)
                {
                    _selectedRace = value;
                    OnPropertyChanged(nameof(SelectedRace));
                }
            }
        }
        private ulong _selectedSetcode = 0;
        public ulong SelectedSetcode
        {
            get => _selectedSetcode;
            set
            {
                if (_selectedSetcode != value)
                {
                    _selectedSetcode = value;
                    OnPropertyChanged(nameof(SelectedSetcode));
                }
            }
        }
        private ulong _selectedCategory = 0;
        public ulong SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory));
                }
            }
        }
        private ulong _selectedRule = 0;
        public ulong SelectedRule
        {
            get => _selectedRule;
            set
            {
                if (_selectedRule != value)
                {
                    _selectedRule = value;
                    OnPropertyChanged(nameof(SelectedRule));
                }
            }
        }
        private ulong _selectedFlag = 0;
        public ulong SelectedFlag
        {
            get => _selectedFlag;
            set
            {
                if (_selectedFlag != value)
                {
                    _selectedFlag = value;
                    OnPropertyChanged(nameof(SelectedFlag));
                }
            }
        }
        private long _selectedRarity = 0;
        public long SelectedRarity
        {
            get => _selectedRarity;
            set
            {
                if (_selectedRarity != value)
                {
                    _selectedRarity = value;
                    OnPropertyChanged(nameof(SelectedRarity));
                }
            }
        }
        /// <summary>
        /// /////////////////
        /// </summary>
        private ulong? _cardID;
        public ulong? CardID
        {
            get => _cardID;
            set
            {
                if (_cardID != value)
                {
                    _cardID = value;
                    OnPropertyChanged(nameof(CardID));
                }
            }
        }
        private string _cardIDText = string.Empty;
        public string CardIDText
        {
            get => _cardIDText;
            set
            {
                if (_cardIDText != value)
                {
                    _cardIDText = value;
                    OnPropertyChanged(nameof(CardIDText));
                    if (string.IsNullOrWhiteSpace(value)) CardID = null;
                    else if (!ulong.TryParse(value, out ulong result) || result == 0) CardID = 0;
                    else CardID = result;
                }
            }
        }
        private int? _cardLevel;
        public int? CardLevel
        {
            get => _cardLevel;
            set
            {
                if (_cardLevel != value)
                {
                    _cardLevel = value;
                    OnPropertyChanged(nameof(CardLevel));
                }
            }
        }
        private string _cardLevelText = string.Empty;
        public string CardLevelText
        {
            get => _cardLevelText;
            set
            {
                if (_cardLevelText != value)
                {
                    _cardLevelText = value;
                    OnPropertyChanged(nameof(CardLevelText));
                    if (string.IsNullOrWhiteSpace(value)) CardLevel = null;
                    else if (!int.TryParse(value, out int result) || result < 0) CardLevel = 0;
                    else CardLevel = result;
                }
            }
        }
        private int? _linkRating;
        public int? LinkRating
        {
            get => _linkRating;
            set
            {
                if (_linkRating != value)
                {
                    _linkRating = value;
                    OnPropertyChanged(nameof(LinkRating));
                }
            }
        }
        private string _linkText = string.Empty;
        public string LinkText
        {
            get => _linkText;
            set
            {
                if (_linkText != value)
                {
                    _linkText = value;
                    OnPropertyChanged(nameof(LinkText));
                    if (string.IsNullOrWhiteSpace(value)) LinkRating = null;
                    else if (!int.TryParse(value, out int result) || result < 0) LinkRating = 0;
                    else LinkRating = result;
                }
            }
        }
        private int? _scaleLeft;
        public int? ScaleLeft
        {
            get => _scaleLeft;
            set
            {
                if (_scaleLeft != value)
                {
                    _scaleLeft = value;
                    OnPropertyChanged(nameof(ScaleLeft));
                }
            }
        }
        private string _scaleLeftText = string.Empty;
        public string ScaleLeftText
        {
            get => _scaleLeftText;
            set
            {
                if (_scaleLeftText != value)
                {
                    _scaleLeftText = value;
                    OnPropertyChanged(nameof(ScaleLeftText));
                    if (string.IsNullOrWhiteSpace(value)) ScaleLeft = null;
                    else if (!int.TryParse(value, out int result) || result < 0) ScaleLeft = 0;
                    else ScaleLeft = result;
                }
            }
        }
        private int? _scaleRight;
        public int? ScaleRight
        {
            get => _scaleRight;
            set
            {
                if (_scaleRight != value)
                {
                    _scaleRight = value;
                    OnPropertyChanged(nameof(ScaleRight));
                }
            }
        }
        private string _scaleRightText = string.Empty;
        public string ScaleRightText
        {
            get => _scaleRightText;
            set
            {
                if (_scaleRightText != value)
                {
                    _scaleRightText = value;
                    OnPropertyChanged(nameof(ScaleRightText));
                    if (string.IsNullOrWhiteSpace(value)) ScaleRight = null;
                    else if (!int.TryParse(value, out int result) || result < 0) ScaleRight = 0;
                    else ScaleRight = result;
                }
            }
        }
        /// <summary>
        /// /////////////////
        /// </summary>
        private long? _cardATK;
        public long? CardATK
        {
            get => _cardATK;
            set
            {
                if (_cardATK != value)
                {
                    _cardATK = value;
                    OnPropertyChanged(nameof(CardATK));
                }
            }
        }
        private string _cardATKText = string.Empty;
        public string CardATKText
        {
            get => _cardATKText;
            set
            {
                if (_cardATKText != value)
                {
                    _cardATKText = value;
                    OnPropertyChanged(nameof(CardATKText));
                    if (string.IsNullOrWhiteSpace(value)) CardATK = null;
                    else if (!long.TryParse(value, out long result) || result < 0) CardATK = -2;
                    else CardATK = result;
                }
            }
        }
        private long? _cardDEF;
        public long? CardDEF
        {
            get => _cardDEF;
            set
            {
                if (_cardDEF != value)
                {
                    _cardDEF = value;
                    OnPropertyChanged(nameof(CardDEF));
                }
            }
        }
        private string _cardDEFText = string.Empty;
        public string CardDEFText
        {
            get => _cardDEFText;
            set
            {
                if (_cardDEFText != value)
                {
                    _cardDEFText = value;
                    OnPropertyChanged(nameof(CardDEFText));
                    if (string.IsNullOrWhiteSpace(value)) CardDEF = null;
                    else if (!long.TryParse(value, out long result) || result < 0) CardDEF = -2;
                    else CardDEF = result;
                }
            }
        }
        private long _linkArrows;
        public long LinkArrows
        {
            get => _linkArrows;
            set
            {
                if (_linkArrows != value)
                {
                    _linkArrows = value;
                    OnPropertyChanged(nameof(LinkArrows));
                }
            }
        }
        private int? _gPoint;
        public int? GPoint
        {
            get => _gPoint;
            set
            {
                if (_gPoint != value)
                {
                    _gPoint = value;
                    OnPropertyChanged(nameof(GPoint));
                }
            }
        }
        private string _gPointText = string.Empty;
        public string GPointText
        {
            get => _gPointText;
            set
            {
                if (_gPointText != value)
                {
                    _gPointText = value;
                    OnPropertyChanged(nameof(GPointText));
                    if (string.IsNullOrWhiteSpace(value)) GPoint = null;
                    else if (!int.TryParse(value, out int result) || result < 0) GPoint = 0;
                    else GPoint = result;
                }
            }
        }

        private string _cardDesc;
        public string CardDesc
        {
            get => _cardDesc;
            set
            {
                if (_cardDesc != value)
                {
                    _cardDesc = value;
                    OnPropertyChanged(nameof(CardDesc));
                }
            }
        }

        public void Clear()
        {
            SelectedType = 0;
            SelectedAttribute = 0;
            SelectedRace = 0;
            SelectedSetcode = 0;
            SelectedCategory = 0;
            SelectedRule = 0;
            SelectedFlag = 0;
            SelectedRarity = 0;

            CardIDText = string.Empty;
            CardLevelText = string.Empty;
            LinkText = string.Empty;
            ScaleLeftText = string.Empty;
            ScaleRightText = string.Empty;

            CardATKText = string.Empty;
            CardDEFText = string.Empty;
            LinkArrows = 0;
            GPointText = string.Empty;
            CardDesc = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
