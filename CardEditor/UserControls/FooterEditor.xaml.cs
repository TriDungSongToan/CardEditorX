using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CardEditor.Models;
using System.Windows.Controls.Primitives;
using static ICSharpCode.AvalonEdit.Rendering.TextViewWeakEventManager;
using System.Runtime.InteropServices;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for FooterEditor.xaml
    /// </summary>
    public partial class FooterEditor : UserControl, INotifyPropertyChanged
    {
        public List<string> fontSizeList { get; set; }
        public List<CmbItems> indentOptions { get; set; }
        public List<CmbItems> newLineOptions { get; set; }

        private bool _isOverWrite = false;
        public bool IsOverWrite
        {
            get => _isOverWrite;
            set
            {
                if (_isOverWrite != value)
                {
                    _isOverWrite = value;
                    OnPropertyChanged(nameof(IsOverWrite));
                }
            }
        }
        public FooterEditor()
        {
            InitializeComponent();
            InitializeCombobox();

            DataContext = this;
        }
        private void InitializeCombobox()
        {
            fontSizeList = new List<string>
            {
                "8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48"
            };
            indentOptions = new List<CmbItems>
            {
                new CmbItems {Name = "Spaces", ShortName = "SPC" },
                new CmbItems {Name = "Tabs", ShortName = "TAB" }
            };
            newLineOptions = new List<CmbItems>
            {
                new CmbItems {Name = "CRLF", ShortName = "CRLF" },
                new CmbItems {Name = "Line Feed", ShortName = "LF" },
                new CmbItems {Name = "Carriage Return", ShortName = "CR" }
            };
        }

        private double _fontSizeText = 12;
        public double FontSizeText
        {
            get => _fontSizeText;
            set
            {
                if (_fontSizeText != value)
                {
                    _fontSizeText = value;
                    OnPropertyChanged(nameof(FontSizeText));
                    FontSizeChanged?.Invoke(this, value);
                }
            }
        }

        private int _lineNumber = 0;
        private int _charNumber = 0;
        public int LineNumber
        {
            get => _lineNumber;
            set
            {
                if (_lineNumber != value)
                {
                    _lineNumber = value;
                    OnPropertyChanged(nameof(LineNumber));
                }
            }
        }
        public int CharNumber
        {
            get => _charNumber;
            set
            {
                if (_charNumber != value)
                {
                    _charNumber = value;
                    OnPropertyChanged(nameof(CharNumber));
                }
            }
        }
        ///////////////
        private double _scrollOffset;
        private double _maxScroll;
        private double _viewportSize;
        public double ScrollOffset
        {
            get => _scrollOffset;
            set
            {
                if (_scrollOffset != value)
                {
                    _scrollOffset = value;
                    OnPropertyChanged(nameof(ScrollOffset));
                    // Trigger event để parent biết ScrollBar đã thay đổi
                    ScrollOffsetChanged?.Invoke(this, value);
                }
            }
        }
        public double MaxScroll
        {
            get => _maxScroll;
            set
            {
                if (_maxScroll != value)
                {
                    _maxScroll = value;
                    OnPropertyChanged(nameof(MaxScroll));
                }
            }
        }
        public double ViewportSize
        {
            get => _viewportSize;
            set
            {
                if (_viewportSize != value)
                {
                    _viewportSize = value;
                    OnPropertyChanged(nameof(ViewportSize));
                }
            }
        }
        ///////////////
        private int _selectedIndentOption;
        public int SelectedIndentOption
        {
            get => _selectedIndentOption;
            set
            {
                if (_selectedIndentOption != value)
                {
                    _selectedIndentOption = value;
                    OnPropertyChanged(nameof(SelectedIndentOption));
                    TabSpcChanged?.Invoke(this, value);
                }
            }
        }
        private int _selectedNewLineOption;
        public int SelectedNewLineOption
        {
            get => _selectedNewLineOption;
            set
            {
                if (_selectedNewLineOption != value)
                {
                    _selectedNewLineOption = value;
                    OnPropertyChanged(nameof(SelectedNewLineOption));
                    NewLineChanged?.Invoke(this, value);
                }
            }
        }
        ///////////////
        public void UpdateScrollBar(double offset, double maximum, double viewportSize)
        {
            MaxScroll = maximum;
            ScrollOffset = offset;
            ViewportSize = viewportSize;
        }
        public void UpdateCaretPosition(int line, int column)
        {
            LineNumber = line;
            CharNumber = column;
        }

        public void ChangeTabSpcOptionFooter(int newValue)
        {
            SelectedIndentOption = newValue;
        }
        public void ChangeNewLineOptionFooter(int newValue)
        {
            SelectedNewLineOption = newValue;
        }

        public event EventHandler<double> FontSizeChanged;
        public event EventHandler<double> ScrollOffsetChanged;
        public event EventHandler<int> TabSpcChanged;
        public event EventHandler<int> NewLineChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
