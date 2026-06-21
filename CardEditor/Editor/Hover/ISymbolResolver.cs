using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardEditor.Models;
using CardEditor.ViewModels;
using ICSharpCode.AvalonEdit.Document;

namespace CardEditor.Editor.Hover
{
    public interface ISymbolResolver
    {
        CompletionSymbol ResolveExpression(string expression);
        CompletionSymbol Resolve(TextDocument document, int offset);

    }
}
