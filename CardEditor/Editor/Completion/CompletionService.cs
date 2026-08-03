using System;
using System.Linq;
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
        private CompletionWindow _completionWindow;

        private string _lastPrefix = string.Empty;

        public CompletionService(TextEditor editor)
        {
            _editor = editor;
        }

        public void ShowCompletion()
        {
            CloseCompletion();

            string prefix = GetCurrentPrefix();
            _completionWindow = new CompletionWindow(_editor.TextArea);
            var data = _completionWindow.CompletionList.CompletionData;

            foreach (var symbol in ScriptViewModel.Instance.AllSymbols)
            {
                if (!string.IsNullOrEmpty(prefix) && !symbol.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                data.Add(new CompletionDataAdapter(symbol));
            }
            if (data.Count == 0) return;
            _completionWindow.Closed += (_, __) => _completionWindow = null;
            _completionWindow.Show();
        }

        private void CloseCompletion()
        {
            _completionWindow?.Close();
            _completionWindow = null;
            _lastPrefix = string.Empty;
        }

        private string GetCurrentPrefix()
        {
            var doc = _editor.Document;
            var offset = _editor.CaretOffset;

            if (offset == 0) return string.Empty;
            int start = offset - 1;
            while (start >= 0)
            {
                char c = doc.GetCharAt(start);
                if (!char.IsLetterOrDigit(c) && c != '_') break;
                start--;
            }
            start++;
            return doc.GetText(start, offset - start);
        }

        public void OnTextEntered(char enteredChar)
        {
            if (!IsIdentifierChar(enteredChar) && enteredChar != '.')
            {
                CloseCompletion();
                return;
            }

            var context = AnalyzeContext();

            if (string.IsNullOrEmpty(context.Prefix) && !context.IsDotCompletion)
            {
                CloseCompletion();
                return;
            }

            ShowOrUpdateCompletion(context);
        }

        private void ShowOrUpdateCompletion(CompletionContext context)
        {
            if (_completionWindow == null)
            {
                _completionWindow = new CompletionWindow(_editor.TextArea);
                _completionWindow.Closed += (_, __) => _completionWindow = null;
                _completionWindow.Show();
            }

            UpdateCompletionList(context);
        }

        private void UpdateCompletionList(CompletionContext context)
        {
            if (_completionWindow == null) return;

            var data = _completionWindow.CompletionList.CompletionData;
            data.Clear();

            if (context.IsDotCompletion)
            {
                AddDotCompletionItems(context, data);
            }
            else
            {
                AddGlobalCompletionItems(context, data);
            }
            if (data.Count > 0)
            {
                _completionWindow.CompletionList.SelectItem(context.Prefix);
            }
            else
            {
                CloseCompletion();
            }
        }
        private void AddGlobalCompletionItems(CompletionContext context, IList<ICompletionData> data)
        {
            foreach (var symbol in ScriptViewModel.Instance.AllSymbols)
            {
                if (!symbol.Name.StartsWith(context.Prefix, StringComparison.OrdinalIgnoreCase)) continue;
                data.Add(new CompletionDataAdapter(symbol));
            }
        }

        private readonly ISymbolResolver _resolver = new SymbolResolver();
        private void AddDotCompletionItems(CompletionContext context, IList<ICompletionData> data)
        {
            if (string.IsNullOrEmpty(context.Qualifier)) return;

            string qualifier = context.Qualifier;

            // 1. Resolve semantic symbol (QUAN TRỌNG NHẤT)
            var symbol = _resolver.ResolveExpression(qualifier);

            // 2. Fallback: nếu không resolve được → thử namespace lookup nhanh
            if (symbol == null)
            {
                var nsFallback = ScriptViewModel.Instance.AllSymbols
                    .Where(s =>
                        !string.IsNullOrEmpty(s.Namespace) &&
                        string.Equals(s.Namespace, qualifier, StringComparison.OrdinalIgnoreCase));

                foreach (var s in nsFallback)
                {
                    if (!IsMatchPrefix(s, context.Prefix)) continue;
                    data.Add(new CompletionDataAdapter(s));
                }

                return;
            }

            // 3. Enum members / graph members (PRIMARY PATH)
            if (symbol.Members != null && symbol.Members.Count > 0)
            {
                foreach (var member in symbol.Members)
                {
                    if (!IsMatchPrefix(member, context.Prefix)) continue;
                    data.Add(new CompletionDataAdapter(member));
                }

                return;
            }

            // 4. Fallback: nếu symbol không có graph → thử namespace children
            var namespaceSymbols = ScriptViewModel.Instance.AllSymbols
                .Where(s =>
                    string.Equals(s.Namespace, qualifier, StringComparison.OrdinalIgnoreCase));

            foreach (var s in namespaceSymbols)
            {
                if (!IsMatchPrefix(s, context.Prefix)) continue;
                data.Add(new CompletionDataAdapter(s));
            }




            //string qualifier = context.Qualifier!;

            //// EnumName.Value
            //var enumMembers = ScriptViewModel.Instance.AllSymbols
            //    .Where(s => s.Kind == SymbolKind.EnumMember && string.Equals(s.OwnerEnum, qualifier, StringComparison.OrdinalIgnoreCase));

            //foreach (var symbol in enumMembers)
            //{
            //    if (!symbol.Name.StartsWith(context.Prefix, StringComparison.OrdinalIgnoreCase)) continue;
            //    data.Add(new CompletionDataAdapter(symbol));
            //}

            //// Namespace.symbol
            //var namespaceSymbols = ScriptViewModel.Instance.AllSymbols
            //    .Where(s => !string.IsNullOrEmpty(s.Namespace) && string.Equals(s.Namespace, qualifier, StringComparison.OrdinalIgnoreCase));

            //foreach (var symbol in namespaceSymbols)
            //{
            //    if (!symbol.Name.StartsWith(context.Prefix, StringComparison.OrdinalIgnoreCase)) continue;
            //    data.Add(new CompletionDataAdapter(symbol));
            //}
        }

        private static bool IsMatchPrefix(CompletionSymbol symbol, string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return true;

            return symbol.Name.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase);
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseCompletion();
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                var context = AnalyzeContext();

                if (string.IsNullOrEmpty(context.Prefix) && !context.IsDotCompletion)
                {
                    CloseCompletion();
                    return;
                }

                ShowOrUpdateCompletion(context);
            }
        }

        private static bool IsIdentifierChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private CompletionContext AnalyzeContext()
        {
            var doc = _editor.Document;
            int offset = _editor.CaretOffset;

            if (offset == 0) return new CompletionContext(null, string.Empty);

            int i = offset - 1;

            // 1. Parse prefix (right side)
            while (i >= 0)
            {
                char c = doc.GetCharAt(i);
                if (!char.IsLetterOrDigit(c) && c != '_') break;
                i--;
            }

            int prefixStart = i + 1;
            string prefix = doc.GetText(prefixStart, offset - prefixStart);

            // 2. Check dot
            if (i < 0 || doc.GetCharAt(i) != '.') return new CompletionContext(null, prefix);

            // 3. Parse qualifier (left side)
            i--; // skip dot
            int end = i;

            while (i >= 0)
            {
                char c = doc.GetCharAt(i);
                if (!char.IsLetterOrDigit(c) && c != '_') break;
                i--;
            }

            int qualifierStart = i + 1;
            string qualifier = doc.GetText(qualifierStart, end - qualifierStart + 1);

            return new CompletionContext(qualifier, prefix);
        }
    }
}
