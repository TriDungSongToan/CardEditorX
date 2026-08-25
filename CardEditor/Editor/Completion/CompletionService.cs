using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using CardEditor.Models;
using CardEditor.Editor.Hover;
using CardEditor.ViewModels;

namespace CardEditor.Editor.Completion
{
    public sealed class CompletionService
    {
        private readonly TextEditor _editor;
        private readonly ISymbolResolver _resolver = new SymbolResolver();

        private CompletionWindow _completionWindow;
        private readonly CompletionDescriptionPopup _descriptionPopup;

        public CompletionService(TextEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));

            _descriptionPopup = new CompletionDescriptionPopup(
                _editor.TextArea.TextView);

            _descriptionPopup.CloseRequested += (_, __) =>
            {
                CloseDescriptionAndReturnFocus();
            };
        }
        private void CloseDescriptionAndReturnFocus()
        {
            _descriptionPopup.Hide();

            // Escape/X đóng description; Escape tiếp theo sẽ đến editor
            // và có thể đóng Auto-suggestion.
            _editor.TextArea.Focus();
        }
        private void CloseCompletionOnly()
        {
            _completionWindow?.Close();
        }
        public void ShowCompletion()
        {
            CloseCompletion();

            string prefix = GetCurrentPrefix();

            EnsureCompletionWindow();

            var data = _completionWindow.CompletionList.CompletionData;

            foreach (var symbol in ScriptViewModel.Instance.AllSymbols)
            {
                if (!string.IsNullOrEmpty(prefix) &&
                    !symbol.Name.StartsWith(
                        prefix,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                data.Add(new CompletionDataAdapter(symbol));
            }

            if (data.Count == 0)
            {
                CloseCompletion();
                return;
            }

            // Dòng này sẽ chọn item phù hợp nhất.
            // Sau đó AvalonEdit phát event SelectionChanged.
            _completionWindow.CompletionList.SelectItem(prefix);
        }

        public void HandleOwnerPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (!_descriptionPopup.IsOpen)
            {
                return;
            }

            var textView = _editor.TextArea.TextView;
            Point position = e.GetPosition(textView);

            // Click nằm ngoài TextView.
            if (position.X < 0 ||
                position.Y < 0 ||
                position.X > textView.ActualWidth ||
                position.Y > textView.ActualHeight)
            {
                _descriptionPopup.Hide();
                return;
            }

            // Chuyển tọa độ chuột thành vị trí trong Document.
            var textViewPosition = textView.GetPosition(position);

            if (textViewPosition == null)
            {
                _descriptionPopup.Hide();
                return;
            }

            int caretLine = _editor.Document.GetLineByOffset(_editor.CaretOffset).LineNumber;
            int clickedLine = textViewPosition.Value.Location.Line;
            if (clickedLine != caretLine)
            {
                _descriptionPopup.Hide();
            }
        }
        public void OnTextEntered(char enteredChar)
        {
            // Người dùng tiếp tục gõ: mô tả cũ không còn phù hợp.
            _descriptionPopup.Hide();

            if (!IsIdentifierChar(enteredChar) && enteredChar != '.')
            {
                CloseCompletion();
                return;
            }

            var context = AnalyzeContext();

            if (string.IsNullOrEmpty(context.Prefix) &&
                !context.IsDotCompletion)
            {
                CloseCompletion();
                return;
            }

            ShowOrUpdateCompletion(context);
        }
        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_descriptionPopup.IsOpen)
                {
                    CloseDescriptionAndReturnFocus();
                }
                else if (_completionWindow != null)
                {
                    CloseCompletionOnly();
                }

                e.Handled = true;
                return;
            }

            if (e.Key == Key.Back)
            {
                _descriptionPopup.Hide();

                var context = AnalyzeContext();

                if (string.IsNullOrEmpty(context.Prefix) &&
                    !context.IsDotCompletion)
                {
                    CloseCompletion();
                    return;
                }

                ShowOrUpdateCompletion(context);
            }
        }

        private void EnsureCompletionWindow()
        {
            if (_completionWindow != null)
            {
                return;
            }
            _completionWindow = new CompletionWindow(_editor.TextArea);
            
            _completionWindow.CloseAutomatically = false;

            // Đây là nơi đăng ký event.
            // Event chạy mỗi khi người dùng đổi item đang được chọn
            // bằng chuột, ↑/↓, hoặc SelectItem(...).
            _completionWindow.CompletionList.SelectionChanged +=
                CompletionList_SelectionChanged;

            _completionWindow.Closed += CompletionWindow_Closed;

            _completionWindow.Show();
        }

        private void CompletionList_SelectionChanged(object sender, EventArgs e)
        {
            if (_completionWindow == null)
            {
                return;
            }

            var selectedItem =
                _completionWindow.CompletionList.SelectedItem
                as CompletionDataAdapter;

            if (selectedItem == null)
            {
                _descriptionPopup.Hide();
                return;
            }

            // Rect tính theo TextView, cũng là PlacementTarget
            // của CompletionDescriptionPopup.
            Rect caretRect =
                _editor.TextArea.Caret.CalculateCaretRectangle();

            var popupPosition = new Point(
                caretRect.Right + 12,
                caretRect.Bottom + 8);

            _descriptionPopup.Show(
                selectedItem.Symbol,
                popupPosition);
        }

        private void CompletionWindow_Closed(object sender, EventArgs e)
        {
            var closedWindow = sender as CompletionWindow;

            if (closedWindow != null)
            {
                
                closedWindow.CompletionList.SelectionChanged -=
                    CompletionList_SelectionChanged;

                closedWindow.Closed -= CompletionWindow_Closed;
            }

            if (ReferenceEquals(_completionWindow, closedWindow))
            {
                _completionWindow = null;
            }

            _descriptionPopup.Hide();
        }

        private void CloseCompletion()
        {
            if (_completionWindow == null)
            {
                _descriptionPopup.Hide();
                return;
            }

            // CompletionWindow_Closed sẽ tự unsubscribe event,
            // gán _completionWindow = null và đóng description.
            _completionWindow.Close();
        }

        private void ShowOrUpdateCompletion(CompletionContext context)
        {
            EnsureCompletionWindow();
            UpdateCompletionList(context);
        }

        private void UpdateCompletionList(CompletionContext context)
        {
            if (_completionWindow == null)
            {
                return;
            }

            var data = _completionWindow.CompletionList.CompletionData;

            // Khi Clear(), selection cũ có thể bị bỏ.
            // Handler sẽ tự Hide popup cũ.
            data.Clear();

            if (context.IsDotCompletion)
            {
                AddDotCompletionItems(context, data);
            }
            else
            {
                AddGlobalCompletionItems(context, data);
            }

            if (data.Count == 0)
            {
                CloseCompletionOnly();
                return;
            }

            // Đây là thao tác làm selection thay đổi,
            // nên CompletionList_SelectionChanged() sẽ được gọi.
            _completionWindow.CompletionList.SelectItem(context.Prefix);
        }

        private void AddGlobalCompletionItems(CompletionContext context, IList<ICompletionData> data)
        {
            foreach (var symbol in ScriptViewModel.Instance.AllSymbols)
            {
                if (!symbol.Name.StartsWith(
                    context.Prefix,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                data.Add(new CompletionDataAdapter(symbol));
            }
        }

        private void AddDotCompletionItems(CompletionContext context, IList<ICompletionData> data)
        {
            if (string.IsNullOrEmpty(context.Qualifier))
            {
                return;
            }

            var symbol = _resolver.ResolveExpression(context.Qualifier);

            if (symbol != null &&
                symbol.Members != null &&
                symbol.Members.Count > 0)
            {
                foreach (var member in symbol.Members)
                {
                    if (IsMatchPrefix(member, context.Prefix))
                    {
                        data.Add(new CompletionDataAdapter(member));
                    }
                }

                return;
            }

            var fallbackSymbols =
                ScriptViewModel.Instance.AllSymbols.Where(s =>
                    string.Equals(
                        s.Namespace,
                        context.Qualifier,
                        StringComparison.OrdinalIgnoreCase));

            foreach (var item in fallbackSymbols)
            {
                if (IsMatchPrefix(item, context.Prefix))
                {
                    data.Add(new CompletionDataAdapter(item));
                }
            }
        }

        private string GetCurrentPrefix()
        {
            var document = _editor.Document;
            int offset = _editor.CaretOffset;

            if (offset == 0)
            {
                return string.Empty;
            }

            int start = offset - 1;

            while (start >= 0)
            {
                char c = document.GetCharAt(start);

                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    break;
                }

                start--;
            }

            start++;

            return document.GetText(start, offset - start);
        }

        private CompletionContext AnalyzeContext()
        {
            var document = _editor.Document;
            int offset = _editor.CaretOffset;

            if (offset == 0)
            {
                return new CompletionContext(null, string.Empty);
            }

            int i = offset - 1;

            while (i >= 0)
            {
                char c = document.GetCharAt(i);

                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    break;
                }

                i--;
            }

            int prefixStart = i + 1;
            string prefix = document.GetText(
                prefixStart,
                offset - prefixStart);

            if (i < 0 || document.GetCharAt(i) != '.')
            {
                return new CompletionContext(null, prefix);
            }

            i--; // Bỏ qua dấu '.'
            int qualifierEnd = i;

            while (i >= 0)
            {
                char c = document.GetCharAt(i);

                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    break;
                }

                i--;
            }

            int qualifierStart = i + 1;

            string qualifier = document.GetText(
                qualifierStart,
                qualifierEnd - qualifierStart + 1);

            return new CompletionContext(qualifier, prefix);
        }

        private static bool IsIdentifierChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private static bool IsMatchPrefix(CompletionSymbol symbol, string prefix)
        {
            return string.IsNullOrEmpty(prefix) ||
                   symbol.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
    }
}
