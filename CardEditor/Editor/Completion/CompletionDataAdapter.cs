using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CardEditor.Models;
using CardEditor.Editor.Completion;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using CardEditor.Editor.Analysis;

namespace CardEditor.Editor.Completion
{
    public sealed class CompletionDataAdapter : ICompletionData
    {
        private static readonly ISymbolDescriptionPresenter _presenter
        = new ScriptDescriptionPresenter();
        public CompletionSymbol Symbol { get; }
        public CompletionDataAdapter(CompletionSymbol symbol)
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        }

        public string Text => Symbol.Name;
        public object Content => Symbol.Name;
        public object Description => _presenter.Create(Symbol);

        public double Priority => GetPriority();
        public ImageSource Image => IconProvider.GetIcon(Symbol.Kind);

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var document = textArea.Document;
            int caretOffset = textArea.Caret.Offset;
            // Tìm vị trí bắt đầu của prefix (sau dấu . nếu có)
            int start = caretOffset - 1;
            while (start >= 0)
            {
                char c = document.GetCharAt(start);
                if (!char.IsLetterOrDigit(c) && c != '_')
                {
                    start++;
                    break;
                }
                start--;
            }

            if (start < 0) start = 0;

            // Replace chỉ phần prefix, không động vào qualifier
            int length = caretOffset - start;
            document.Replace(start, length, Text);
        }

        private double GetPriority()
        {
            return Symbol.Kind switch
            {
                SymbolKind.Function => 100,
                SymbolKind.Constant => 80,
                SymbolKind.Enum => 70,
                SymbolKind.EnumMember => 60,
                _ => 0
            };
        }
    }
}
