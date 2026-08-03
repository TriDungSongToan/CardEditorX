using System.Collections.Generic;
using CardEditor.Editor.Analysis;
using CardEditor.Models;

namespace CardEditor.Services
{
    public static class ScriptSymbolProcessor
    {
        public static void Process(List<CompletionSymbol> symbols)
        {
            BuildSemanticGraph(symbols);
        }
        private static void BuildSemanticGraph(List<CompletionSymbol> symbols)
        {
            SymbolGraphBuilder.Build(symbols);
        }
    }
}
