using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Rendering;
using CardEditor.Events;
using CardEditor.Helpers;
using CardEditor.Manager;
using CardEditor.Abstract;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using ICSharpCode.AvalonEdit.Editing;
using System.ComponentModel;
using Microsoft.Build.Tasks;
using System.Security.Policy;
using System.Web.UI.WebControls;
using CardEditor.Editor.Completion;
using System.Runtime.Remoting.Contexts;
using CardEditor.Editor.Hover;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for CodeEditorPane.xaml
    /// </summary>
    public partial class CodeEditorPane : UserControl, IDisposable
    {
        #region Variable
        
        //private CompletionProvider completionProvider;
        private ChangeTracker changeTracker;
        private ChangeTrackingMargin changeTrackingMargin;
        private CompletionService _completionService;
        private HoverService _hoverService;

        private bool isChangeTrackingInitialized = false;
        public bool isSyncingScroll = false;

        private ScrollViewer _textEditorScrollViewer;
        #endregion

        #region Constructor
        public CodeEditorPane()
        {
            InitializeComponent();
            
            textEditorPane.TextArea.SelectionChanged += TextArea_SelectionChanged;
            textEditorPane.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            //completionProvider = new CompletionProvider(textEditorPane);
        }

        #endregion

        #region Load
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            LoadCompletion();
            CreateContextMenu(textEditorPane);
            InitializeMiniMap();
            InitializeHighLight();
            if (!isChangeTrackingInitialized) InitializeChangeTracking();
            InitializeEvent();
            InitializeFooter();
        }
        public void LoadConfig()
        {
            textEditorPane.LineNumbersForeground = UIConfigViewModel.Instance.ThemeColor;
            textEditorPane.WordWrap = ConfigViewModel.Instance.codeEditSetting.WordWrap;

            textEditorPane.ShowLineNumbers = ConfigViewModel.Instance.codeEditSetting.ShowLineNumber; // Hiển thị số dòng
            textEditorPane.Options.ShowSpaces = ConfigViewModel.Instance.codeEditSetting.ShowSpace; //Hiển thị ký hiệu · cho các ký tự khoảng trắng (space).
            textEditorPane.Options.ShowTabs = ConfigViewModel.Instance.codeEditSetting.ShowTab; //Hiển thị ký hiệu » cho các ký tự tab.
            textEditorPane.Options.ShowEndOfLine = ConfigViewModel.Instance.codeEditSetting.ShowEndLine; //Hiển thị ký hiệu ¶ ở cuối mỗi dòng.
            textEditorPane.Options.ShowBoxForControlCharacters = ConfigViewModel.Instance.codeEditSetting.ShowControlChar; //Hiển thị hộp chứa mã hex cho các ký tự điều khiển không in được.
            textEditorPane.Options.HighlightCurrentLine = ConfigViewModel.Instance.codeEditSetting.HighLightLine; //Tô sáng dòng hiện tại chứa caret (con trỏ văn bản).
            textEditorPane.Options.HideCursorWhileTyping = ConfigViewModel.Instance.codeEditSetting.HiddenCursor; //Ẩn con trỏ chuột khi người dùng đang gõ phím.

            textEditorPane.Options.ShowColumnRuler = ConfigViewModel.Instance.codeEditSetting.ShowColumnRuler; //Hiển thị thanh thước dọc tại vị trí cột xác định.
            textEditorPane.Options.ColumnRulerPosition = ConfigViewModel.Instance.codeEditSetting.ColumnRulerPosition; //Vị trí (số cột) của thanh thước dọc khi ShowColumnRuler bật.

            
            textEditorPane.Options.EnableTextDragDrop = ConfigViewModel.Instance.codeEditSetting.TextDragDrop; //Cho phép kéo thả đoạn văn bản trong vùng soạn thảo.
            textEditorPane.Options.AllowToggleOverstrikeMode = ConfigViewModel.Instance.codeEditSetting.Overstrikemode; //Cho phép chuyển đổi chế độ ghi đè (overstrike mode).
            textEditorPane.Options.CutCopyWholeLine = ConfigViewModel.Instance.codeEditSetting.HandleWholeLine; //Khi không có vùng chọn, thao tác cắt/sao chép sẽ áp dụng cho toàn bộ dòng hiện tại.
            textEditorPane.Options.EnableVirtualSpace = ConfigViewModel.Instance.codeEditSetting.VirtualSpace; //Cho phép đặt caret (con trỏ) vượt ra ngoài cuối dòng (virtual space).
            textEditorPane.Options.EnableRectangularSelection = ConfigViewModel.Instance.codeEditSetting.RectangularSelection; //Cho phép chọn vùng hình chữ nhật (Alt + kéo chuột).
            textEditorPane.Options.AllowScrollBelowDocument = ConfigViewModel.Instance.codeEditSetting.ScrollBelowDocument; //Cho phép cuộn vượt quá cuối tài liệu(thêm không gian trống bên dưới).

            textEditorPane.Options.EnableImeSupport = ConfigViewModel.Instance.codeEditSetting.IMESupport; //Bật/tắt hỗ trợ IME (Input Method Editor) cho các ngôn ngữ như Trung, Nhật, Hàn.
            textEditorPane.Options.EnableHyperlinks = ConfigViewModel.Instance.codeEditSetting.HyperLink; //Cho phép nhận diện và click vào các liên kết(URL) trong văn bản.
            textEditorPane.Options.EnableEmailHyperlinks = ConfigViewModel.Instance.codeEditSetting.MailHyperLink; //Cho phép nhận diện và click vào các liên kết email trong văn bản.
            textEditorPane.Options.RequireControlModifierForHyperlinkClick = ConfigViewModel.Instance.codeEditSetting.RequireControlHyperLink; //Yêu cầu nhấn phím Ctrl khi click vào liên kết để mở liên kết đó.

            textEditorPane.Options.ConvertTabsToSpaces = ConfigViewModel.Instance.codeEditSetting.TabsToSpace; //Chuyển tab thành khoảng trắng khi thụt lề.
            textEditorPane.Options.IndentationSize = ConfigViewModel.Instance.codeEditSetting.IndentationSize; //Độ rộng của một đơn vị thụt lề (số ký tự).
            textEditorPane.WordWrap = ConfigViewModel.Instance.codeEditSetting.WordWrap;
            textEditorPane.Options.WordWrapIndentation = ConfigViewModel.Instance.codeEditSetting.WordWrapIndentation; //Độ thụt lề cho các dòng bị ngắt(word wrap), trừ dòng đầu tiên.
            textEditorPane.Options.InheritWordWrapIndentation = ConfigViewModel.Instance.codeEditSetting.InheritWordWrapIndentation; //Các dòng ngắt dòng có kế thừa thụt lề của dòng đầu tiên hay không.
            // textEditorPane.Options.GetIndentationString(); //Phương thức trả về chuỗi thụt lề phù hợp (tab hoặc số lượng space) tùy theo cấu hình.
        }
        #endregion

        #region ContextMenu
        private void CreateContextMenu(FrameworkElement control)
        {
            var contextMenu = new ContextMenu
            {
                Background = UIConfigViewModel.Instance.Background,
                Foreground = UIConfigViewModel.Instance.Foreground,
                FontFamily = UIConfigViewModel.Instance.FontFamily,
                FontSize = UIConfigViewModel.Instance.FontSize,
                Tag = control
            };
            control.ContextMenu = contextMenu;

            contextMenu.Opened += (s, e) =>
            {
                var target = contextMenu.Tag as FrameworkElement;
                Debug.WriteLine($"ContextMenu opened for control: {target?.GetType().Name} (Name: {(target as Control)?.Name})");
            };

            contextMenu.Items.Add(new System.Windows.Controls.MenuItem { Header = "Cut", Height = 25, Command = ApplicationCommands.Cut });
            contextMenu.Items.Add(new System.Windows.Controls.MenuItem { Header = "Copy", Height = 25, Command = ApplicationCommands.Copy });
            contextMenu.Items.Add(new System.Windows.Controls.MenuItem { Header = "Paste", Height = 25, Command = ApplicationCommands.Paste });

            var fullWidthItem = new System.Windows.Controls.MenuItem { Header = "To FullWidth", Height = 25 };
            fullWidthItem.Click += (s, e) => MenuConvertWidth_Click(control, true);
            contextMenu.Items.Add(fullWidthItem);

            var halfWidthItem = new System.Windows.Controls.MenuItem { Header = "To HalfWidth", Height = 25 };
            halfWidthItem.Click += (s, e) => MenuConvertWidth_Click(control, false);
            contextMenu.Items.Add(halfWidthItem);

            var toSuperScriptItem = new System.Windows.Controls.MenuItem { Header = "To SuperScript", Height = 25 };
            toSuperScriptItem.Click += (s, e) => MenuConvertSuper_Click(control, true);
            contextMenu.Items.Add(toSuperScriptItem);

            var fromSuperScriptItem = new System.Windows.Controls.MenuItem { Header = "From SuperScript", Height = 25 };
            fromSuperScriptItem.Click += (s, e) => MenuConvertSuper_Click(control, false);
            contextMenu.Items.Add(fromSuperScriptItem);

            var toSubScriptItem = new System.Windows.Controls.MenuItem { Header = "To SubScript", Height = 25 };
            toSubScriptItem.Click += (s, e) => MenuConvertSub_Click(control, true);
            contextMenu.Items.Add(toSubScriptItem);

            var fromSubScriptItem = new System.Windows.Controls.MenuItem { Header = "From SubScript", Height = 25 };
            fromSubScriptItem.Click += (s, e) => MenuConvertSub_Click(control, false);
            contextMenu.Items.Add(fromSubScriptItem);

            var specialCharactersItem = new System.Windows.Controls.MenuItem
            {
                Header = "Special Characters",
                Height = 25,
                Background = UIConfigViewModel.Instance.Background,
                Foreground = UIConfigViewModel.Instance.Foreground,
                FontFamily = UIConfigViewModel.Instance.FontFamily,
                FontSize = UIConfigViewModel.Instance.FontSize
            };
            specialCharactersItem.Click += (s, e) =>
            {
                var targetControl = contextMenu.Tag as FrameworkElement ?? contextMenu.PlacementTarget as FrameworkElement;
                Debug.WriteLine($"Opening SpecialCharactersWindow for control: {targetControl?.GetType().Name} (Name: {(targetControl as Control)?.Name})");
                if (targetControl != null)
                {
                    var window = new SpecialCharactersWindow(targetControl);
                    window.ShowDialog();
                }
                else
                {
                    Debug.WriteLine("Target control is null");
                }
            };
            contextMenu.Items.Add(specialCharactersItem);
        }
        private void MenuConvertWidth_Click(FrameworkElement control, bool toFullWidth)
        {
            switch (control)
            {
                case RichTextBox richTextBox:
                    TextRange selectedText = new TextRange(richTextBox.Selection.Start, richTextBox.Selection.End);
                    if (!string.IsNullOrEmpty(selectedText.Text))
                    {
                        string convertedText = toFullWidth ? ConvertString.ConvertToFullWidth(selectedText.Text) : ConvertString.ConvertToHalfWidth(selectedText.Text);
                        selectedText.Text = convertedText;
                    }
                    break;
                case System.Windows.Controls.TextBox textBox:
                    if (!string.IsNullOrEmpty(textBox.SelectedText))
                    {
                        string convertedText = toFullWidth ? ConvertString.ConvertToFullWidth(textBox.SelectedText) : ConvertString.ConvertToHalfWidth(textBox.SelectedText);
                        int selectionStart = textBox.SelectionStart;
                        textBox.Text = textBox.Text.Remove(selectionStart, textBox.SelectionLength).Insert(selectionStart, convertedText);
                        textBox.SelectionStart = selectionStart;
                        textBox.SelectionLength = convertedText.Length;
                    }
                    break;
                case ICSharpCode.AvalonEdit.TextEditor textEditor:
                    if (!string.IsNullOrEmpty(textEditor.SelectedText))
                    {
                        string convertedText = toFullWidth ? ConvertString.ConvertToFullWidth(textEditor.SelectedText) : ConvertString.ConvertToHalfWidth(textEditor.SelectedText);
                        textEditor.Document.Replace(textEditor.SelectionStart, textEditor.SelectionLength, convertedText);
                    }
                    break;
            }
        }
        private void MenuConvertSuper_Click(FrameworkElement control, bool ToSuper)
        {
            switch (control)
            {
                case RichTextBox richTextBox:
                    TextRange selectedText = new TextRange(richTextBox.Selection.Start, richTextBox.Selection.End);
                    if (!string.IsNullOrEmpty(selectedText.Text))
                    {
                        string convertedText = ToSuper ? ConvertString.ConvertToSuperscript(selectedText.Text) : ConvertString.ConvertFromSuperscript(selectedText.Text);
                        selectedText.Text = convertedText;
                    }
                    break;
                case System.Windows.Controls.TextBox textBox:
                    if (!string.IsNullOrEmpty(textBox.SelectedText))
                    {
                        string convertedText = ToSuper ? ConvertString.ConvertToSuperscript(textBox.SelectedText) : ConvertString.ConvertFromSuperscript(textBox.SelectedText);
                        int selectionStart = textBox.SelectionStart;
                        textBox.Text = textBox.Text.Remove(selectionStart, textBox.SelectionLength).Insert(selectionStart, convertedText);
                        textBox.SelectionStart = selectionStart;
                        textBox.SelectionLength = convertedText.Length;
                    }
                    break;
                case ICSharpCode.AvalonEdit.TextEditor textEditor:
                    if (!string.IsNullOrEmpty(textEditor.SelectedText))
                    {
                        string convertedText = ToSuper ? ConvertString.ConvertToSuperscript(textEditor.SelectedText) : ConvertString.ConvertFromSuperscript(textEditor.SelectedText);
                        textEditor.Document.Replace(textEditor.SelectionStart, textEditor.SelectionLength, convertedText);
                    }
                    break;
            }
        }
        private void MenuConvertSub_Click(FrameworkElement control, bool ToSub)
        {
            switch (control)
            {
                case RichTextBox richTextBox:
                    TextRange selectedText = new TextRange(richTextBox.Selection.Start, richTextBox.Selection.End);
                    if (!string.IsNullOrEmpty(selectedText.Text))
                    {
                        string convertedText = ToSub ? ConvertString.ConvertToSubscript(selectedText.Text) : ConvertString.ConvertFromSubscript(selectedText.Text);
                        selectedText.Text = convertedText;
                    }
                    break;
                case System.Windows.Controls.TextBox textBox:
                    if (!string.IsNullOrEmpty(textBox.SelectedText))
                    {
                        string convertedText = ToSub ? ConvertString.ConvertToSubscript(textBox.SelectedText) : ConvertString.ConvertFromSubscript(textBox.SelectedText);
                        int selectionStart = textBox.SelectionStart;
                        textBox.Text = textBox.Text.Remove(selectionStart, textBox.SelectionLength).Insert(selectionStart, convertedText);
                        textBox.SelectionStart = selectionStart;
                        textBox.SelectionLength = convertedText.Length;
                    }
                    break;
                case ICSharpCode.AvalonEdit.TextEditor textEditor:
                    if (!string.IsNullOrEmpty(textEditor.SelectedText))
                    {
                        string convertedText = ToSub ? ConvertString.ConvertToSubscript(textEditor.SelectedText) : ConvertString.ConvertFromSubscript(textEditor.SelectedText);
                        textEditor.Document.Replace(textEditor.SelectionStart, textEditor.SelectionLength, convertedText);
                    }
                    break;
            }
        }
        #endregion

        #region MiniMap
        private void InitializeMiniMap()
        {
            miniMap.TextEditor = textEditorPane;
            previewTooltip.Child = miniMap.PreviewBorder;
        }
        #endregion

        #region HighLight
        private void InitializeHighLight()
        {
            var highlighter = new CurrentLineHighlighter(textEditorPane);
            //textEditorPane.TextArea.TextView.BackgroundRenderers.Add(highlighter);
        }
        private void InitializeChangeTracking()
        {
            if (textEditorPane?.Document == null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (textEditorPane?.Document != null)
                    {
                        InitializeChangeTracking();
                    }
                }), System.Windows.Threading.DispatcherPriority.Loaded);
                return;
            }

            try
            {
                // Tạo ChangeTracker
                changeTracker = new ChangeTracker(textEditorPane.Document);
                // Tạo ChangeTrackingMargin
                changeTrackingMargin = new ChangeTrackingMargin(changeTracker);
                // Thêm margin vào TextEditor (bên trái line numbers)
                // Margin được add vào TextArea.LeftMargins
                textEditorPane.TextArea.LeftMargins.Insert(0, changeTrackingMargin);

                isChangeTrackingInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing change tracking: {ex.Message}");
            }
        }
        #endregion

        #region Change
        private void InitializeEvent()
        {
            textEditorPane.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            textEditorPane.TextArea.PreviewMouseWheel += TextArea_PreviewMouseWheel;
        }
        private void LoadCompletion()
        {
            _completionService = new CompletionService(textEditorPane);
            textEditorPane.TextArea.KeyDown += TextArea_KeyDown;
            textEditorPane.TextArea.TextEntered += TextArea_TextEntered;

            _hoverService = new HoverService(textEditorPane);
        }
        private void TextArea_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                _completionService?.ShowCompletion();
                e.Handled = true;
                return;
            }
            else if (e.Key == Key.Back)
            {
                _completionService?.OnKeyDown(e);
                _completionService?.ShowCompletion();
                //e.Handled = true;
                return;
            }
            _completionService?.OnKeyDown(e);
        }
        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length == 1)
            {
                _completionService?.OnTextEntered(e.Text[0]);
            }
        }
        private void TextArea_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                double currentFontSize = textEditorPane.FontSize;

                if (e.Delta > 0) currentFontSize += 1;
                else currentFontSize -= 1;

                currentFontSize = Math.Max(6, Math.Min(72, currentFontSize));
                FontSizeText = currentFontSize;

                e.Handled = true;
            }
        }
        private void TextArea_SelectionChanged(object sender, EventArgs e)
        {
            var existingWordTransformers = textEditorPane.TextArea.TextView.LineTransformers
            .OfType<MarkSameWord>().ToList();

            foreach (var transformer in existingWordTransformers)
            {
                textEditorPane.TextArea.TextView.LineTransformers.Remove(transformer);
            }

            if (!string.IsNullOrWhiteSpace(textEditorPane.SelectedText))
            {
                textEditorPane.TextArea.TextView.LineTransformers.Add(
                    new MarkSameWord(textEditorPane.SelectedText));
            }

            textEditorPane.TextArea.TextView.Redraw();
        }
        public void MarkAsSaved()
        {
            changeTracker?.MarkAsSaved();
        }
        public void ClearSavedMarker()
        {
            changeTracker?.ClearSavedMarkers();
        }
        public void ResetSavedMarker()
        {
            changeTracker?.Reset();
        }
        public void ChangeOVR(bool IsOverWrite)
        {
            textEditorPane.TextArea.OverstrikeMode = IsOverWrite;
            footerEditor.IsOverWrite = IsOverWrite;
        }

        #endregion

        #region Footer

        private void InitializeFooter()
        {
            InitializeFontSize();
            InitializeScroll();
            InitializeCombobox();
        }

        #region Font Size
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
                    OnFontSizeChange();
                }
            }
        }
        private void InitializeFontSize()
        {
            footerEditor.FontSizeChanged += FooterEditor_FontSizeChanged;
        }
        private void FooterEditor_FontSizeChanged(object sender, double e)
        {
            FontSizeText = e;
        }
        private void OnFontSizeChange()
        {
            textEditorPane.FontSize = FontSizeText;
            footerEditor.FontSizeText = FontSizeText;
        }
        #endregion

        #region Scroll
        private ScrollViewer GetScrollViewer(DependencyObject element)
        {
            if (element is ScrollViewer scrollViewer)
            {
                return scrollViewer;
            }

            int childCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                var result = GetScrollViewer(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void InitializeScroll()
        {
            // 1. Tìm ScrollViewer của TextEditor trong Visual Tree
            _textEditorScrollViewer = GetScrollViewer(textEditorPane);

            if (_textEditorScrollViewer != null)
            {
                // 2. Lắng nghe sự kiện scroll của TextEditor
                _textEditorScrollViewer.ScrollChanged += TextEditor_ScrollChanged;
            }

            // 3. Lắng nghe sự kiện khi user kéo ScrollBar trên FooterEditor
            footerEditor.ScrollOffsetChanged += FooterEditor_ScrollOffsetChanged;

            // 4. Khởi tạo giá trị ban đầu cho FooterEditor
            UpdateFooterScrollBar();
        }
        private void CleanupScrollBarSync()
        {
            if (_textEditorScrollViewer != null)
            {
                _textEditorScrollViewer.ScrollChanged -= TextEditor_ScrollChanged;
            }
            footerEditor.ScrollOffsetChanged -= FooterEditor_ScrollOffsetChanged;
        }
        private void TextEditor_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.HorizontalChange != 0 || e.ExtentWidthChange != 0)
            {
                UpdateFooterScrollBar();
            }
        }
        private void FooterEditor_ScrollOffsetChanged(object sender, double newOffset)
        {
            if (_textEditorScrollViewer != null)
            {
                // Tạm thời hủy event để tránh vòng lặp vô hạn
                _textEditorScrollViewer.ScrollChanged -= TextEditor_ScrollChanged;

                _textEditorScrollViewer.ScrollToHorizontalOffset(newOffset);

                // Đăng ký lại event
                _textEditorScrollViewer.ScrollChanged += TextEditor_ScrollChanged;
            }
        }
        private void UpdateFooterScrollBar()
        {
            if (_textEditorScrollViewer != null)
            {
                double currentOffset = _textEditorScrollViewer.HorizontalOffset;
                double maxScroll = _textEditorScrollViewer.ScrollableWidth;
                double viewportSize = _textEditorScrollViewer.ViewportWidth;

                // Tạm thời hủy event để tránh vòng lặp vô hạn
                footerEditor.ScrollOffsetChanged -= FooterEditor_ScrollOffsetChanged;

                footerEditor.UpdateScrollBar(currentOffset, maxScroll, viewportSize);

                // Đăng ký lại event
                footerEditor.ScrollOffsetChanged += FooterEditor_ScrollOffsetChanged;
            }
        }

        public int GetFirstVisibleLine()
        {
            var textView = textEditorPane.TextArea.TextView;
            if (!textView.IsInitialized) return 1;

            textView.EnsureVisualLines();
            if (!textView.VisualLines.Any()) return 1;

            int firstVisibleLine = textView.VisualLines[0].FirstDocumentLine.LineNumber;
            return firstVisibleLine;
        }
        public void ScrollToLine(int line)
        {
            var textView = textEditorPane?.TextArea.TextView;
            if (!textView.IsInitialized) return;

            var doc = textView.Document;
            if (doc == null || doc.LineCount == 0) return;

            if (line < 1) line = 1;
            if (line > doc.LineCount) line = doc.LineCount;

            textView.EnsureVisualLines();
            textEditorPane.TextArea.TextView.EnsureVisualLines();
            textEditorPane.ScrollTo(line, 0);
        }
        #endregion

        #region Combobox
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
        private void InitializeCombobox()
        {
            footerEditor.TabSpcChanged += FooterEditor_TabSpcChanged;
            footerEditor.NewLineChanged += FooterEditor_NewLineChanged;
        }
        private void FooterEditor_TabSpcChanged(object sender, int e)
        {
            SelectedIndentOption = e;
        }
        private void FooterEditor_NewLineChanged(object sender, int e)
        {
            SelectedNewLineOption = e;
        }

        public void ChangeTabSpcOptionPane(int newValue)
        {
            footerEditor.ChangeTabSpcOptionFooter(newValue);
        }
        public void ChangeNewLineOptionPane(int newValue)
        {
            footerEditor.ChangeNewLineOptionFooter(newValue);
        }
        #endregion

        #endregion

        #region Caret
        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            textEditorPane.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);

            var caret = textEditorPane.TextArea.Caret;
            if (caret != null)
            {
                int line = caret.Line;
                int column = caret.Column;

                footerEditor.UpdateCaretPosition(line, column);
            }

            // Xóa tất cả các transformer BracketHighlightRenderer cũ
            var existingBracketTransformers = textEditorPane.TextArea.TextView.LineTransformers
                .OfType<BracketHighlightRenderer>().ToList();
            foreach (var transformer in existingBracketTransformers)
            {
                textEditorPane.TextArea.TextView.LineTransformers.Remove(transformer);
            }

            // Tìm cặp ngoặc nếu con trỏ ở cạnh ngoặc
            var (openOffset, closeOffset) = FindMatchingBracket();
            if (openOffset >= 0 && closeOffset >= 0)
            {
                textEditorPane.TextArea.TextView.LineTransformers.Add(
                    new BracketHighlightRenderer(textEditorPane.Document, openOffset, closeOffset));
            }

            // Cập nhật giao diện
            textEditorPane.TextArea.TextView.Redraw();
        }
        #endregion

        #region Bracket
        private (int openOffset, int closeOffset) FindMatchingBracket()
        {
            int offset = textEditorPane.TextArea.Caret.Offset;
            var document = textEditorPane.Document;
            if (offset < 0 || offset > document.TextLength) return (-1, -1);

            char currentChar = offset < document.TextLength ? document.GetCharAt(offset) : '\0';
            char prevChar = offset > 0 ? document.GetCharAt(offset - 1) : '\0';

            // Kiểm tra nếu con trỏ ở cạnh ngoặc mở hoặc đóng
            char openBracket = '\0', closeBracket = '\0';
            int startOffset = -1;
            bool isForwardSearch = true;

            if (IsOpenBracket(currentChar)) // Con trỏ ngay sau ngoặc mở
            {
                openBracket = currentChar;
                closeBracket = GetMatchingCloseBracket(currentChar);
                startOffset = offset;
                isForwardSearch = true;
            }
            else if (IsCloseBracket(currentChar)) // Con trỏ ngay sau ngoặc đóng
            {
                closeBracket = currentChar;
                openBracket = GetMatchingOpenBracket(currentChar);
                startOffset = offset;
                isForwardSearch = false;
            }
            else if (IsOpenBracket(prevChar)) // Con trỏ ngay trước ngoặc mở
            {
                openBracket = prevChar;
                closeBracket = GetMatchingCloseBracket(prevChar);
                startOffset = offset - 1;
                isForwardSearch = true;
            }
            else if (IsCloseBracket(prevChar)) // Con trỏ ngay trước ngoặc đóng
            {
                closeBracket = prevChar;
                openBracket = GetMatchingOpenBracket(prevChar);
                startOffset = offset - 1;
                isForwardSearch = false;
            }
            else
            {
                return (-1, -1);
            }

            // Tìm ngoặc tương ứng
            int matchingOffset = FindMatchingBracketOffset(document, startOffset, openBracket, closeBracket, isForwardSearch);
            if (matchingOffset >= 0)
            {
                // Đảm bảo trả về đúng thứ tự: (openOffset, closeOffset)
                if (isForwardSearch)
                    return (startOffset, matchingOffset); // Từ ngoặc mở -> ngoặc đóng
                else
                    return (matchingOffset, startOffset); // Từ ngoặc đóng -> ngoặc mở
            }

            return (-1, -1);
        }
        private bool IsOpenBracket(char c) => c == '(' || c == '{' || c == '[';
        private bool IsCloseBracket(char c) => c == ')' || c == '}' || c == ']';
        private char GetMatchingCloseBracket(char openBracket) =>
        openBracket switch
        {
            '(' => ')',
            '{' => '}',
            '[' => ']',
            _ => '\0'
        };
        private char GetMatchingOpenBracket(char closeBracket) =>
        closeBracket switch
        {
            ')' => '(',
            '}' => '{',
            ']' => '[',
            _ => '\0'
        };
        private int FindMatchingBracketOffset(TextDocument document, int startOffset, char openBracket, char closeBracket, bool isForwardSearch)
        {
            int depth = 1;
            int step = isForwardSearch ? 1 : -1;
            int i = startOffset + step;

            while (i >= 0 && i < document.TextLength)
            {
                char c = document.GetCharAt(i);
                if (isForwardSearch)
                {
                    if (c == openBracket)
                        depth++;
                    else if (c == closeBracket)
                        depth--;
                }
                else
                {
                    if (c == closeBracket)
                        depth++;
                    else if (c == openBracket)
                        depth--;
                }

                if (depth == 0)
                    return i;

                i += step;
            }

            return -1;
        }
        #endregion

        #region Event
        public event EventHandler<int> TabSpcChanged;
        public event EventHandler<int> NewLineChanged;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }
        public void Dispose()
        {
            textEditorPane.SyntaxHighlighting = null;
            textEditorPane.TextArea.SelectionChanged -= TextArea_SelectionChanged;
            textEditorPane.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
            textEditorPane.TextArea.PreviewMouseWheel -= TextArea_PreviewMouseWheel;

            textEditorPane.TextArea.KeyDown -= TextArea_KeyDown;
            textEditorPane.TextArea.TextEntered -= TextArea_TextEntered;

            footerEditor.FontSizeChanged -= FooterEditor_FontSizeChanged;

            _textEditorScrollViewer.ScrollChanged -= TextEditor_ScrollChanged;
            footerEditor.ScrollOffsetChanged -= FooterEditor_ScrollOffsetChanged;
            textEditorPane.Document.Text = string.Empty;
            textEditorPane.TextArea.Caret.PositionChanged -= (s, e) =>
            {
                textEditorPane.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
            };
        }
        #endregion

    }
}
