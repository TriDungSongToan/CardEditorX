using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardEditor.Models;

namespace CardEditor.Editor.Analysis
{
    public static class SymbolGraphBuilder
    {
        public static void Build(IReadOnlyList<CompletionSymbol> symbols)
        {
            BuildEnums(symbols);
            //BuildNamespaces(symbols);
            // sau này có thể thêm Type, Variable, …
        }

        private static void BuildEnums(IReadOnlyList<CompletionSymbol> symbols)
        {
            var enums = symbols.Where(s => s.Kind == SymbolKind.Enum).ToDictionary(e => e.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var member in symbols.Where(s => s.Kind == SymbolKind.EnumMember))
            {
                if (string.IsNullOrEmpty(member.OwnerEnum)) continue;
                if (!enums.TryGetValue(member.OwnerEnum, out var enumSymbol)) continue;
                member.Owner = enumSymbol;
                enumSymbol.AddMember(member);
            }
        }
    }
}
