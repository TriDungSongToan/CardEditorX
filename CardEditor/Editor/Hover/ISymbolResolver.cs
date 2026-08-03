using CardEditor.Models;
using ICSharpCode.AvalonEdit.Document;

namespace CardEditor.Editor.Hover
{
    public interface ISymbolResolver
    {
        CompletionSymbol ResolveExpression(string expression);
        CompletionSymbol Resolve(TextDocument document, int offset);

    }
}
