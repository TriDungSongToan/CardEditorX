using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using CardEditor.Theming;
using CardEditor.Services;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.ViewModels
{
    public class UIConfigViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<UIConfigViewModel> _instance = new Lazy<UIConfigViewModel>(() =>  new UIConfigViewModel());
        public static UIConfigViewModel Instance => _instance.Value;

        private Brush _background = Brushes.White;
        private Brush _foreground = Brushes.Black;
        private Brush _themeColor = Brushes.Purple;
        private IHighlightingDefinition _syntaxHighlighting;
        private FontFamily _fontFamily = new FontFamily("Consolas");
        private int _fontSize = 12;

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        private TextAlignment _textAlignment = TextAlignment.Left;
        private HorizontalAlignment _hintAlignment;

        private UIConfigViewModel()
        {
            LoadConfig();
        }

        public Brush Background
        {
            get => _background;
            set
            {
                if (_background != value)
                {
                    _background = value;
                    OnPropertyChanged(nameof(Background));
                }
            }
        }
        public Brush Foreground
        {
            get => _foreground;
            set
            {
                if (_foreground != value)
                {
                    _foreground = value;
                    OnPropertyChanged(nameof(Foreground));
                }
            }
        }
        public Brush ThemeColor
        {
            get => _themeColor;
            set
            {
                if (_themeColor != value)
                {
                    _themeColor = value;
                    OnPropertyChanged(nameof(ThemeColor));
                }
            }
        }
        public IHighlightingDefinition SyntaxHighlighting
        {
            get => _syntaxHighlighting;
            set
            {
                if (_syntaxHighlighting != value)
                {
                    _syntaxHighlighting = value;
                    OnPropertyChanged(nameof(SyntaxHighlighting));
                }
            }
        }

        public FontFamily FontFamily
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
        public int FontSize
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

        public FlowDirection FlowDirectionC
        {
            get => _flowDirection;
            set
            {
                if (_flowDirection != value)
                {
                    _flowDirection = value;
                    OnPropertyChanged(nameof(FlowDirectionC));
                }
            }
        }
        public TextAlignment TextAlignmentC
        {
            get => _textAlignment;
            set
            {
                if (_textAlignment != value)
                {
                    _textAlignment = value;
                    OnPropertyChanged(nameof(TextAlignmentC));
                }
            }
        }
        public HorizontalAlignment HintAlignment
        {
            get => _hintAlignment;
            set
            {
                if (_hintAlignment != value)
                {
                    _hintAlignment = value;
                    OnPropertyChanged(nameof(HintAlignment));
                }
            }
        }
        private Color _miniMapBackgroundColor { get; set; } = Color.FromArgb(100, 40, 40, 40);
        private Color _miniMapForegroundColor { get; set; } = Color.FromArgb(255, 212, 212, 212);
        private Color _miniMapSliderColor { get; set; } = Color.FromArgb(150, 100, 100, 100);
        public Color MiniMapBackgroundColor
        {
            get => _miniMapBackgroundColor;
            set
            {
                if (_miniMapBackgroundColor != value)
                {
                    _miniMapBackgroundColor = value;
                    OnPropertyChanged(nameof(MiniMapBackgroundColor));
                }
            }
        }
        public Color MiniMapForegroundColor
        {
            get => _miniMapForegroundColor;
            set
            {
                if (_miniMapForegroundColor != value)
                {
                    _miniMapForegroundColor = value;
                    OnPropertyChanged(nameof(MiniMapForegroundColor));
                }
            }
        }
        public Color MiniMapSliderColor
        {
            get => _miniMapSliderColor;
            set
            {
                if (_miniMapSliderColor != value)
                {
                    _miniMapSliderColor = value;
                    OnPropertyChanged(nameof(MiniMapSliderColor));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        // public event EventHandler<string> ThemeChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void LoadConfig()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var appResources = Application.Current.Resources;
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigViewModel.Instance.displaySetting.Background));
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigViewModel.Instance.displaySetting.Foreground));
                FontFamily = new FontFamily(ConfigViewModel.Instance.displaySetting.FontFamily);
                FontSize = ConfigViewModel.Instance.displaySetting.FontSize.Value;
                FlowDirectionC = ConfigViewModel.Instance.displaySetting.FlowDirectionC == 0 ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;

                switch (ConfigViewModel.Instance.displaySetting.TextAlignmentC)
                {
                    case 0:
                        TextAlignmentC = TextAlignment.Left;
                        HintAlignment = HorizontalAlignment.Left;
                        break;
                    case 1:
                        TextAlignmentC = TextAlignment.Right;
                        HintAlignment = HorizontalAlignment.Right;
                        break;
                    case 2:
                        TextAlignmentC = TextAlignment.Justify;
                        HintAlignment = HorizontalAlignment.Left;
                        break;
                    case 3:
                        TextAlignmentC = TextAlignment.Center;
                        HintAlignment = HorizontalAlignment.Left;
                        break;
                    default:
                        TextAlignmentC = TextAlignment.Left;
                        HintAlignment = HorizontalAlignment.Left;
                        break;
                }

                string[] itemstheme = { "Amber", "Blue", "BlueGrey", "Brown", "Cyan", "DeepOrange", "DeepPurple", "Green", "Grey", "Indigo", "LightBlue", "LightGreen", "Lime", "Orange", "Pink", "Purple", "Red", "Teal", "Yellow" };
                string themeSet = ConfigViewModel.Instance.displaySetting.Theme;
                if (!itemstheme.Contains(themeSet, StringComparer.OrdinalIgnoreCase))
                    themeSet = "DeepPurple";

                var oldTheme = appResources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("MaterialDesignColor"));
                if (oldTheme != null) appResources.MergedDictionaries.Remove(oldTheme);

                Uri themeUri = new Uri($"pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.{themeSet}.xaml", UriKind.Absolute);
                ResourceDictionary newResource = new ResourceDictionary { Source = themeUri };
                appResources.MergedDictionaries.Add(newResource);

                ColorDictionary colorDict = new ColorDictionary();
                string hexCode = colorDict.GetHexCode(themeSet);
                try
                {
                    Color color = (Color)ColorConverter.ConvertFromString(hexCode);
                    ThemeColor = new SolidColorBrush(color);
                }
                catch
                {
                    ThemeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#673AB7"));
                }

                MiniMapBackgroundColor = BrushToColor(Background, _miniMapBackgroundColor);
                MiniMapForegroundColor = BrushToColor(Foreground, _miniMapForegroundColor);
                MiniMapSliderColor = BrushToColor(ThemeColor, _miniMapSliderColor);

                var (result, highlightFilePath, message) = HighLightService.HighLightFilePath();
                if (!result) CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, message, new[] { CMess.ok.ToText() });
                if (!string.IsNullOrEmpty(highlightFilePath) && System.IO.File.Exists(highlightFilePath))
                {
                    using var stream = File.OpenRead(highlightFilePath);
                    using var reader = new XmlTextReader(stream);
                    SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            });
        }
        private static Color BrushToColor(Brush brush, Color fallback)
        {
            return brush is SolidColorBrush solid ? solid.Color : fallback;
        }
        public void Dispose()
        {
            Background = null;
            Foreground = null;
            ThemeColor = null;
            FontFamily = null;
            SyntaxHighlighting = null;

            PropertyChanged = null;
        }
    }
}
