using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace CardEditor.Helpers
{
    public class AutoCompleteData
    {
        // Lớp để lưu trữ thông tin gợi ý
        public string Text { get; set; }        // Từ gợi ý
        public string Description { get; set; }  // Mô tả
        public bool IsFunction { get; set; }     // Là hàm hay hằng số
        public string Parameters { get; set; }   // Tham số (nếu là hàm)
    }

    public class CustomCompletionWindow : CompletionWindow
    {
        public CustomCompletionWindow(TextArea textArea) : base(textArea)
        {
            // Tùy chỉnh giao diện của completion window
            this.Background = Brushes.White;
            this.BorderBrush = Brushes.Gray;
            this.BorderThickness = new Thickness(1);

            // Xử lý phím ESC
            this.PreviewKeyDown += (s, e) => {
                if (e.Key == Key.Escape)
                {
                    this.Close();
                }
            };
        }
    }

    public class CustomCompletionData : ICompletionData
    {
        private AutoCompleteData data;
        private TextEditor editor;
        public ImageSource Image => null;
        public string Text => data.Text;
        public object Content => data.Text;
        public object Description => data.Description;
        public double Priority => 0;

        public CustomCompletionData(AutoCompleteData data, TextEditor editor)
        {
            this.data = data;
            this.editor = editor;
        }
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            // Xác định từ đang gõ
            int caretOffset = textArea.Caret.Offset;
            var document = textArea.Document;

            // Tìm điểm bắt đầu của từ
            int wordStart = caretOffset;
            while (wordStart > 0)
            {
                char ch = document.GetCharAt(wordStart - 1);
                if (!char.IsLetterOrDigit(ch) && ch != '.' && ch != '_')
                    break;
                wordStart--;
            }

            // Xác định độ dài của từ hiện tại
            int wordLength = caretOffset - wordStart;

            // Thay thế đúng từ đang gõ
            document.Replace(wordStart, wordLength, this.Text);
        }
    }

    public class CompletionProvider
    {
        private TextEditor editor;
        private List<AutoCompleteData> completionData;
        private CustomCompletionWindow completionWindow;
        private ToolTip descriptionToolTip;
        private ParameterInfoToolTip parameterToolTip;

        public CompletionProvider(TextEditor editor)
        {
            this.editor = editor;
            this.completionData = LoadCompletionData();
            this.descriptionToolTip = new ToolTip
            {
                Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint,
                HasDropShadow = true,
                Background = Brushes.Black,
                Foreground = Brushes.White,
                PlacementTarget = editor.TextArea.TextView
            };
            this.parameterToolTip = new ParameterInfoToolTip();

            // Đăng ký các sự kiện
            editor.TextArea.TextEntered += TextArea_TextEntered;
            editor.TextArea.PreviewKeyDown += TextArea_PreviewKeyDown;
            editor.TextArea.MouseMove += TextArea_MouseMove;
            //editor.TextArea.TextView.MouseHover += TextView_MouseHover;
            //editor.TextArea.TextView.MouseHoverStopped += TextView_MouseHoverStopped;
            editor.TextArea.TextView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;
            editor.TextArea.MouseLeave += TextArea_MouseHoverStopped;
        }

        private void TextView_ScrollOffsetChanged(object sender, EventArgs e)
        {
            descriptionToolTip.IsOpen = false;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateTooltipAtCurrentMousePosition();
            }), DispatcherPriority.Background);
        }
        private void UpdateTooltipAtCurrentMousePosition()
        {
            try
            {
                // Kiểm tra xem chuột có còn trong vùng TextArea không
                if (!editor.TextArea.IsMouseOver) return;

                var textView = editor.TextArea.TextView;
                textView.EnsureVisualLines();

                // LẤY VỊ TRÍ CHUỘT HIỆN TẠI (không dùng lastMousePosition)
                var mousePos = Mouse.GetPosition(textView);

                // Chuyển vị trí chuột thành vị trí văn bản
                var position = textView.GetPosition(mousePos);

                if (position == null) return;

                int offset;
                try
                {
                    offset = editor.Document.GetOffset(position.Value.Location);
                }
                catch
                {
                    return;
                }

                var word = GetWordAtOffset(offset);
                if (string.IsNullOrEmpty(word)) return;

                var data = completionData.FirstOrDefault(d => d.Text == word);
                if (data == null) return;

                // Hiển thị tooltip
                descriptionToolTip.Content = data.Description;
                descriptionToolTip.PlacementTarget = editor.TextArea;
                descriptionToolTip.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
                descriptionToolTip.IsOpen = true;
            }
            catch
            {
                descriptionToolTip.IsOpen = false;
            }
        }
        private void TextView_MouseHoverStopped(object sender, MouseEventArgs e)
        {
            descriptionToolTip.IsOpen = false;
        }

        private void TextView_MouseHover(object sender, MouseEventArgs e)
        {
            var textView = editor.TextArea.TextView;
            textView.EnsureVisualLines();
            var point = e.GetPosition(textView);

            var position = textView.GetPosition(point);
            if (position == null)
            {
                descriptionToolTip.IsOpen = false;
                return;
            }

            int offset;
            try
            {
                offset = editor.Document.GetOffset(position.Value.Location);
            }
            catch
            {
                descriptionToolTip.IsOpen = false;
                return;
            }

            if (offset < 0 || offset >= editor.Document.TextLength)
            {
                descriptionToolTip.IsOpen = false;
                return;
            }

            string word = GetWordAtOffset(offset);
            if (string.IsNullOrEmpty(word))
            {
                descriptionToolTip.IsOpen = false;
                return;
            }

            var data = completionData.FirstOrDefault(d => d.Text == word);
            if (data == null)
            {
                descriptionToolTip.IsOpen = false;
                return;
            }

            descriptionToolTip.Content = data.Description;
            descriptionToolTip.PlacementTarget = editor.TextArea;
            descriptionToolTip.IsOpen = true;
        }

        private void TextArea_MouseMove(object sender, MouseEventArgs e)
        {
            //try
            //{
            //    Point mousePosition = e.GetPosition(editor.TextArea);
            //    int offset = editor.Document.GetOffset(editor.TextArea.Caret.Position.Line, editor.TextArea.Caret.Position.Column);
            //    if (offset >= 0)
            //    {
            //        var location = editor.Document.GetLocation(offset);
            //        var word = GetWordAtOffset(location);
            //        if (!string.IsNullOrEmpty(word))
            //        {
            //            var data = completionData.FirstOrDefault(d => d.Text == word);
            //            if (data != null)
            //            {
            //                descriptionToolTip.Content = data.Description;
            //                descriptionToolTip.IsOpen = true; return;
            //            }
            //        }
            //    }
            //}
            //catch
            //{ ///

            //}
            //descriptionToolTip.IsOpen = false;
        }

        private List<AutoCompleteData> LoadCompletionData()
        {
            string exeFilePath = Assembly.GetExecutingAssembly().Location;
            var result = new List<AutoCompleteData>();
            // string languageCode = SettingService.GetStringSetting("Language", "English");
            string languageCode = CardEditor.ViewModels.ConfigViewModel.Instance.userSetting.Language;


            // Đọc file constants.txt
            var constantsPath = Path.Combine(System.IO.Path.GetDirectoryName(exeFilePath), $@"data\CardData\Language\{languageCode}\scriptinfo\constants.txt");
            if (File.Exists(constantsPath))
            {
                var constants = File.ReadAllText(constantsPath).Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var constant in constants)
                {
                    var lines = constant.Trim().Split('\n');
                    if (lines.Length >= 2)
                    {
                        result.Add(new AutoCompleteData
                        {
                            Text = lines[0].Trim(),
                            Description = string.Join("\n", lines.Skip(1)).Trim(),
                            IsFunction = false
                        });
                    }
                }
            }

            // Đọc file keyword.txt
            var keywordsPath = Path.Combine(System.IO.Path.GetDirectoryName(exeFilePath), $@"data\CardData\Language\{languageCode}\scriptinfo\keywords.txt");
            if (File.Exists(keywordsPath))
            {
                var keywords = File.ReadAllText(keywordsPath).Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var keyword in keywords)
                {
                    var lines = keyword.Trim().Split('\n');
                    result.Add(new AutoCompleteData
                    {
                        Text = lines[0].Trim(),
                        Description = string.Join("\n", lines.Skip(1)).Trim(),
                        IsFunction = false
                    });
                }
            }

            // Đọc file functions.txt
            var functionsPath = Path.Combine(System.IO.Path.GetDirectoryName(exeFilePath), $@"data\CardData\Language\{languageCode}\scriptinfo\functions.txt");
            if (File.Exists(functionsPath))
            {
                var functions = File.ReadAllText(functionsPath).Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var function in functions)
                {
                    var lines = function.Trim().Split('\n');
                    if (lines.Length >= 2)
                    {
                        var functionDeclaration = lines[0].Trim();
                        var openParenIndex = functionDeclaration.IndexOf('(');
                        var closeParenIndex = functionDeclaration.IndexOf(')');

                        if (openParenIndex != -1 && closeParenIndex != -1)
                        {
                            var startDescriptionLine = 1; // Dòng bắt đầu của mô tả.
                            if (lines.Length >= startDescriptionLine)
                            {
                                result.Add(new AutoCompleteData
                                {
                                    Text = functionDeclaration.Substring(0, openParenIndex),
                                    Parameters = functionDeclaration.Substring(openParenIndex + 1, closeParenIndex - openParenIndex - 1),
                                    Description = string.Join("\n", lines.Skip(startDescriptionLine - 1)).Trim(),
                                    IsFunction = true
                                });
                            }
                        }
                    }
                }
            }
            return result;
        }

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '.')
                return;

            // var currentWord = GetCurrentWord();

            if (e.Text == "(")
            {
                // Hiển thị parameter info
                var function = GetFunctionAtCaret();
                if (function != null)
                {
                    ShowParameterInfo(function);
                }
            }

            else if (e.Text == ")")
            {
                // Ẩn parameter info
                HideParameterInfo();
            }
            else
            {
                var currentWord = GetCurrentWord();
                // Hiển thị completion window khi gõ chữ
                ShowCompletionWindow(currentWord);
            }
        }
        private void TextArea_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Hiển thị completion window khi nhấn Ctrl+Space
                var currentWord = GetCurrentWord();
                ShowCompletionWindow(currentWord);
                e.Handled = true;
            }
        }
        private void TextArea_MouseHoverStopped(object sender, MouseEventArgs e)
        {
            // Ẩn tooltip mô tả
            descriptionToolTip.IsOpen = false;
        }
        private string GetCurrentWord()
        {
            var caretOffset = editor.CaretOffset;
            var document = editor.Document;

            // Tìm điểm bắt đầu của từ
            var wordStart = caretOffset;
            while (wordStart > 0)
            {
                var ch = document.GetCharAt(wordStart - 1);
                if (!char.IsLetterOrDigit(ch) && ch != '.' && ch != '_')
                    break;
                wordStart--;
            }

            // Lấy từ hiện tại
            if (caretOffset > wordStart)
                return document.GetText(wordStart, caretOffset - wordStart);

            return string.Empty;
        }
        private void ShowCompletionWindow(string currentWord)
        {
            // Đóng cửa sổ completion cũ nếu đang mở
            if (completionWindow != null)
            {
                completionWindow.Close();
            }

            // Tạo danh sách gợi ý dựa trên từ hiện tại
            var completions = completionData
                .Where(d => string.IsNullOrEmpty(currentWord) || d.Text.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
                .Select(d => new CustomCompletionData(d, editor))
                .ToList();

            if (completions.Any())
            {
                completionWindow = new CustomCompletionWindow(editor.TextArea);
                foreach (var completion in completions)
                {
                    completionWindow.CompletionList.CompletionData.Add(completion);
                }
                completionWindow.CompletionList.SelectedItem = completionWindow.CompletionList.CompletionData.FirstOrDefault();
                completionWindow.Show();
            }
        }
        private AutoCompleteData GetFunctionAtCaret()
        {
            var currentWord = GetCurrentWord();
            return completionData.FirstOrDefault(d => d.IsFunction && d.Text == currentWord);
        }
        private void ShowParameterInfo(AutoCompleteData function)
        {
            parameterToolTip.Show(function.Parameters, editor.TextArea);
        }
        private void HideParameterInfo()
        {
            parameterToolTip.Hide();
        }
        //private string GetWordAtOffset(TextLocation location)
        //{
        //    // Chuyển TextLocation thành document offset
        //    int offset = editor.Document.GetOffset(location);

        //    // Tìm điểm bắt đầu và kết thúc của từ
        //    var document = editor.Document;
        //    var wordStart = offset;
        //    while (wordStart > 0)
        //    {
        //        var ch = document.GetCharAt(wordStart - 1);
        //        if (!char.IsLetterOrDigit(ch) && ch != '.' && ch != '_')
        //            break;
        //        wordStart--;
        //    }

        //    var wordEnd = offset;
        //    while (wordEnd < document.TextLength)
        //    {
        //        var ch = document.GetCharAt(wordEnd);
        //        if (!char.IsLetterOrDigit(ch) && ch != '.' && ch != '_')
        //            break;
        //        wordEnd++;
        //    }

        //    if (wordEnd > wordStart)
        //        return document.GetText(wordStart, wordEnd - wordStart);

        //    return string.Empty;
        //}
        private string GetWordAtOffset(int offset)
        {
            if (offset < 0 || offset >= editor.Document.TextLength)
                return string.Empty;

            var document = editor.Document;

            int start = offset;
            while (start > 0)
            {
                char ch = document.GetCharAt(start - 1);
                if (!char.IsLetterOrDigit(ch) && ch != '_' && ch != '.')
                    break;
                start--;
            }

            int end = offset;
            while (end < document.TextLength)
            {
                char ch = document.GetCharAt(end);
                if (!char.IsLetterOrDigit(ch) && ch != '_' && ch != '.')
                    break;
                end++;
            }

            return end > start ? document.GetText(start, end - start) : string.Empty;
        }
    }

    // Lớp hỗ trợ hiển thị thông tin tham số
    public class ParameterInfoToolTip : ToolTip
    {
        public ParameterInfoToolTip()
        {
            this.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        }
        public void Show(string parameters, TextArea textArea)
        {
            this.Content = parameters;
            this.PlacementTarget = textArea;
            this.IsOpen = true;
        }
        public void Hide()
        {
            this.IsOpen = false;
        }
    }
}
