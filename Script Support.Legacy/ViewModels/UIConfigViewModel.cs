using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ScriptSupport.Legacy.Services;
using System.Xml;
using CMess = ScriptSupport.Legacy.Localization.Language;
using ScriptSupport.Legacy.Localization;
using ScriptSupport.Legacy.Views;
using ScriptSupport.Legacy.Theming;

namespace ScriptSupport.Legacy.ViewModels
{
    public class UIConfigViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<UIConfigViewModel> _instance = new Lazy<UIConfigViewModel>(() => new UIConfigViewModel());
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


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
