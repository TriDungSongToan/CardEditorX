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
    public class CodeEditSetting : INotifyPropertyChanged
    {
        #region Display
        private bool _showLineNumber = true;
        public bool ShowLineNumber
        {
            get => _showLineNumber;
            set
            {
                if (_showLineNumber != value)
                {
                    _showLineNumber = value;
                    OnPropertyChanged(nameof(ShowLineNumber));
                }
            }
        }
        private bool _showSpace = true;
        public bool ShowSpace
        {
            get => _showSpace;
            set
            {
                if (_showSpace != value)
                {
                    _showSpace = value;
                    OnPropertyChanged(nameof(ShowSpace));
                }
            }
        }
        private bool _showTab = true;
        public bool ShowTab
        {
            get => _showTab;
            set
            {
                if (_showTab != value)
                {
                    _showTab = value;
                    OnPropertyChanged(nameof(ShowTab));
                }
            }
        }
        private bool _showEndLine = false;
        public bool ShowEndLine
        {
            get => _showEndLine;
            set
            {
                if (_showEndLine != value)
                {
                    _showEndLine = value;
                    OnPropertyChanged(nameof(ShowEndLine));
                }
            }
        }
        private bool _showControlChar = true;
        public bool ShowControlChar
        {
            get => _showControlChar;
            set
            {
                if (_showControlChar != value)
                {
                    _showControlChar = value;
                    OnPropertyChanged(nameof(ShowControlChar));
                }
            }
        }
        private bool _highLight = true;
        public bool HighLight
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
        private bool _highLightLine = true;
        public bool HighLightLine
        {
            get => _highLightLine;
            set
            {
                if (_highLightLine != value)
                {
                    _highLightLine = value;
                    OnPropertyChanged(nameof(HighLightLine));
                }
            }
        }
        private bool _hiddenCursor = true;
        public bool HiddenCursor
        {
            get => _hiddenCursor;
            set
            {
                if (_hiddenCursor != value)
                {
                    _hiddenCursor = value;
                    OnPropertyChanged(nameof(HiddenCursor));
                }
            }
        }
        #endregion

        #region Column Ruler
        private bool _showColumnRuler = false;
        public bool ShowColumnRuler
        {
            get => _showColumnRuler;
            set
            {
                if (_showColumnRuler != value)
                {
                    _showColumnRuler = value;
                    OnPropertyChanged(nameof(ShowColumnRuler));
                }
            }
        }
        private int _columnRulerPosition = 0;
        public int ColumnRulerPosition
        {
            get => _columnRulerPosition;
            set
            {
                if (_columnRulerPosition != value)
                {
                    _columnRulerPosition = value;
                    OnPropertyChanged(nameof(ColumnRulerPosition));
                }
            }
        }
        private string _columnRulerPositionString = string.Empty;
        public string ColumnRulerPositionString
        {
            get => _columnRulerPositionString;
            set
            {
                if (_columnRulerPositionString != value)
                {
                    _columnRulerPositionString = value;
                    OnPropertyChanged(nameof(ColumnRulerPositionString));
                    if (string.IsNullOrWhiteSpace(value)) ColumnRulerPosition = 0;
                    else if (!int.TryParse(value, out int result) || result < 0) ColumnRulerPosition = 0;
                    else ColumnRulerPosition = result;
                }
            }
        }
        #endregion

        #region Editing
        private bool _codeFolding = true;
        public bool CodeFolding
        {
            get => _codeFolding;
            set
            {
                if (_codeFolding != value)
                {
                    _codeFolding = value;
                    OnPropertyChanged(nameof(CodeFolding));
                }
            }
        }
        private bool _textDragDrop = true;
        public bool TextDragDrop
        {
            get => _textDragDrop;
            set
            {
                if (_textDragDrop != value)
                {
                    _textDragDrop = value;
                    OnPropertyChanged(nameof(TextDragDrop));
                }
            }
        }
        private bool _overstrikemode = false;
        public bool Overstrikemode
        {
            get => _overstrikemode;
            set
            {
                if (_overstrikemode != value)
                {
                    _overstrikemode = value;
                    OnPropertyChanged(nameof(Overstrikemode));

                }
            }
        }
        private bool _handleWholeLine = false;
        public bool HandleWholeLine
        {
            get => _handleWholeLine;
            set
            {
                if (_handleWholeLine != value)
                {
                    _handleWholeLine = value;
                    OnPropertyChanged(nameof(HandleWholeLine));
                }
            }
        }
        private bool _virtualSpace = false;
        public bool VirtualSpace
        {
            get => _virtualSpace;
            set
            {
                if (_virtualSpace != value)
                {
                    _virtualSpace = value;
                    OnPropertyChanged(nameof(VirtualSpace));
                }
            }
        }

        private bool _rectangularSelection = true;
        public bool RectangularSelection
        {
            get => _rectangularSelection;
            set
            {
                if (_rectangularSelection != value)
                {
                    _rectangularSelection = value;
                    OnPropertyChanged(nameof(RectangularSelection));
                }
            }
        }


        private bool _scrollBelowDocument = true;
        public bool ScrollBelowDocument
        {
            get => _scrollBelowDocument;
            set
            {
                if (_scrollBelowDocument != value)
                {
                    _scrollBelowDocument = value;
                    OnPropertyChanged(nameof(ScrollBelowDocument));
                }
            }
        }


        #endregion

        #region Link / InPut
        private bool _IMESupport = true;
        public bool IMESupport
        {
            get => _IMESupport;
            set
            {
                if (_IMESupport != value)
                {
                    _IMESupport = value;
                    OnPropertyChanged(nameof(IMESupport));
                }
            }
        }
        private bool _hyperLink = true;
        public bool HyperLink
        {
            get => _hyperLink;
            set
            {
                if (_hyperLink != value)
                {
                    _hyperLink = value;
                    OnPropertyChanged(nameof(HyperLink));
                }
            }
        }
        private bool _mailHyperLink = true;
        public bool MailHyperLink
        {
            get => _mailHyperLink;
            set
            {
                if(_mailHyperLink != value)
                {
                    _mailHyperLink = value;
                    OnPropertyChanged(nameof(MailHyperLink));
                }
            }
        }
        private bool _requireControlHyperLink = true;
        public bool RequireControlHyperLink
        {
            get => _requireControlHyperLink;
            set
            {
                if(_requireControlHyperLink != value)
                {
                    _requireControlHyperLink = value;
                    OnPropertyChanged(nameof(RequireControlHyperLink));
                }
            }
        }
        #endregion

        #region Indentation
        private bool _tabsToSpace = true; //Chuyển tab thành khoảng trắng khi thụt lề.
        public bool TabsToSpace
        {
            get => _tabsToSpace;
            set
            {
                if (_tabsToSpace != value)
                {
                    _tabsToSpace = value;
                    OnPropertyChanged(nameof(TabsToSpace));
                }
            }
        }

        private int _indentationSize = 4; //Độ rộng của một đơn vị thụt lề (số ký tự).
        public int IndentationSize
        {
            get => _indentationSize;
            set
            {
                if(_indentationSize != value)
                {
                    _indentationSize = value;
                    OnPropertyChanged(nameof(IndentationSize));
                }
            }
        }
        private string _indentationSizeString = string.Empty;
        public string IndentationSizeString
        {
            get => _indentationSizeString;
            set
            {
                if (_indentationSizeString != value)
                {
                    _indentationSizeString = value;
                    OnPropertyChanged(nameof(IndentationSizeString));
                    if (string.IsNullOrWhiteSpace(value)) IndentationSize = 0;
                    else if (!int.TryParse(value, out int result) || result < 0) IndentationSize = 0;
                    else IndentationSize = result;
                }
            }
        }

        private bool _wordWrap = false; //Ngắt dòng
        public bool WordWrap
        {
            get => _wordWrap;
            set
            {
                if (_wordWrap != value)
                {
                    _wordWrap = value;
                    OnPropertyChanged(nameof(WordWrap));
                }
            }
        }
        private int _wordWrapIndentation = 4; //Độ thụt lề cho các dòng bị ngắt(word wrap), trừ dòng đầu tiên.
        public int WordWrapIndentation
        {
            get => _wordWrapIndentation;
            set
            {
                if (_wordWrapIndentation != value)
                {
                    _wordWrapIndentation = value;
                    OnPropertyChanged(nameof(WordWrapIndentation));
                }
            }
        }
        private string _wordWrapIndentationString = string.Empty;
        public string WordWrapIndentationString
        {
            get => _wordWrapIndentationString;
            set
            {
                if (_wordWrapIndentationString != value)
                {
                    _wordWrapIndentationString = value;
                    OnPropertyChanged(nameof(WordWrapIndentationString));
                    if (string.IsNullOrWhiteSpace(value)) WordWrapIndentation = 0;
                    else if (!int.TryParse(value, out int result) || result < 0) WordWrapIndentation = 0;
                    else WordWrapIndentation = result;
                }
            }
        }

        private bool _inheritWordWrapIndentation = true; //Các dòng ngắt dòng có kế thừa thụt lề của dòng đầu tiên hay không.
        public bool InheritWordWrapIndentation
        {
            get => _inheritWordWrapIndentation;
            set
            {
                if(_inheritWordWrapIndentation != value)
                {
                    _inheritWordWrapIndentation = value;
                    OnPropertyChanged(nameof(InheritWordWrapIndentation));
                }
            }
        }
        #endregion

        public CodeEditSetting Clone()
        {
            var json = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<CodeEditSetting>(json)!;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
