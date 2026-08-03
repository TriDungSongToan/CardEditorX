using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using CardEditor.Collections;

namespace CardEditor.Models.Settings
{
    public class DisplaySettingSource : INotifyPropertyChanged
    {
        private BulkObservableCollection<string> _themes;
        public BulkObservableCollection<string> Themes
        {
            get => _themes;
            set
            {
                if (!ReferenceEquals(_themes, value))
                {
                    _themes = value ?? new BulkObservableCollection<string>();
                    OnPropertyChanged(nameof(Themes));
                }
            }
        }
        public BulkObservableCollection<FontFamily> _fontFamilys;
        public BulkObservableCollection<FontFamily> FontFamilys
        {
            get => _fontFamilys;
            set
            {
                if (!ReferenceEquals(_fontFamilys, value))
                {
                    _fontFamilys = value ?? new BulkObservableCollection<FontFamily>();
                    OnPropertyChanged(nameof(FontFamilys));
                }
            }
        }
        private BulkObservableCollection<string> _highLights;
        public BulkObservableCollection<string> HighLights
        {
            get => _highLights;
            set
            {
                if (!ReferenceEquals(_highLights, value))
                {
                    _highLights = value ?? new BulkObservableCollection<string>();
                    OnPropertyChanged(nameof(HighLights));
                }
            }
        }

        public DisplaySettingSource()
        {
            _themes = new BulkObservableCollection<string>();
            _fontFamilys = new BulkObservableCollection<FontFamily>();
            _highLights = new BulkObservableCollection<string>();
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            _themes ??= new BulkObservableCollection<string>();
            _fontFamilys ??= new BulkObservableCollection<FontFamily>();
            _highLights ??= new BulkObservableCollection<string>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class DisplaySetting : INotifyPropertyChanged
    {
        private string _background = "#FF000000";
        public string Background
        {
            get => _background;
            set
            {
                if (_background != value)
                {
                    _background = value;
                    OnPropertyChanged(nameof(Background));

                    try
                    {
                        var colorObj = ColorConverter.ConvertFromString(_background);
                        if (colorObj is Color color && color != _selectedBackground)
                        {
                            _selectedBackground = color;
                            OnPropertyChanged(nameof(SelectedBackground));
                        }
                    }
                    catch { }
                }
            }
        }
        private Color _selectedBackground = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
        [JsonIgnore]
        public Color SelectedBackground
        {
            get => _selectedBackground;
            set
            {
                if (_selectedBackground != value)
                {
                    _selectedBackground = value;
                    OnPropertyChanged(nameof(SelectedBackground));

                    var colorString = _selectedBackground.ToString();
                    if (_background != colorString)
                    {
                        _background = colorString;
                        OnPropertyChanged(nameof(Background));
                    }
                }
            }
        }

        private string _foreground = "#FFFFFFFF";
        public string Foreground
        {
            get => _foreground;
            set
            {
                if (_foreground != value)
                {
                    _foreground = value;
                    OnPropertyChanged(nameof(Foreground));

                    try
                    {
                        var colorObj = ColorConverter.ConvertFromString(_foreground);
                        if (colorObj is Color color && color != _selectedForeground)
                        {
                            _selectedForeground = color;
                            OnPropertyChanged(nameof(SelectedForeground));
                        }
                    }
                    catch { }
                }
            }
        }
        private Color _selectedForeground = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF");
        [JsonIgnore]
        public Color SelectedForeground
        {
            get => _selectedForeground;
            set
            {
                if (_selectedForeground != value)
                {
                    _selectedForeground = value;
                    OnPropertyChanged(nameof(SelectedForeground));

                    var colorString = _selectedForeground.ToString();
                    if (_foreground != colorString)
                    {
                        _foreground = colorString;
                        OnPropertyChanged(nameof(Foreground));
                    }
                }
            }
        }

        private string _theme = "DeepPurple";
        public string Theme
        {
            get => _theme;
            set
            {
                if (_theme != value)
                {
                    _theme = value;
                    OnPropertyChanged(nameof(Theme));
                }
            }
        }

        private string _fontFamily = "Consolas";
        public string FontFamily
        {
            get => _fontFamily;
            set
            {
                if (_fontFamily != value)
                {
                    _fontFamily = value;
                    OnPropertyChanged(nameof(FontFamily));
                }
            }
        }

        private int? _fontSize = 14;
        public int? FontSize
        {
            get => _fontSize;
            set
            {
                if (_fontSize != value)
                {
                    _fontSize = value;
                    OnPropertyChanged(nameof(FontSize));
                }
            }
        }

        private string _highLight = "Default";
        public string HighLight
        {
            get => _highLight;
            set
            {
                if (_highLight != value)
                {
                    _highLight = value;
                    OnPropertyChanged(nameof(HighLight));
                }
            }
        }

        private int _flowDirectionC = 0;
        public int FlowDirectionC
        {
            get => _flowDirectionC;
            set
            {
                if (_flowDirectionC != value)
                {
                    _flowDirectionC = value;
                    OnPropertyChanged(nameof(FlowDirectionC));
                }
            }
        }

        private int _textAlignmentC = 0;
        public int TextAlignmentC
        {
            get => _textAlignmentC;
            set
            {
                if (_textAlignmentC != value)
                {
                    _textAlignmentC = value;
                    OnPropertyChanged(nameof(TextAlignmentC));
                }
            }
        }

        private string _buttonChat = "2,2";
        public string ButtonChat
        {
            get => _buttonChat;
            set
            {
                if (_buttonChat != value)
                {
                    _buttonChat = value;
                    OnPropertyChanged(nameof(ButtonChat));
                }
            }
        }
        private int _widthChat = 545;
        public int WidthChat
        {
            get => _widthChat;
            set
            {
                if(_widthChat != value)
                {
                    _widthChat = value;
                    OnPropertyChanged(nameof(WidthChat));
                }
            }
        }
        private int _widthMiniMap = 50;
        public int WidthMiniMap
        {
            get => _widthMiniMap;
            set
            {
                if (_widthMiniMap != value)
                {
                    _widthMiniMap = value;
                    OnPropertyChanged(nameof(WidthMiniMap));
                }
            }
        }

        public DisplaySetting Clone()
        {
            var json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<DisplaySetting>(json)!;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
