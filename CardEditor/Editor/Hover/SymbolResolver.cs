using System;
using System.Linq;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using CardEditor.Models;
using CardEditor.ViewModels;

namespace CardEditor.Editor.Hover
{
    public sealed class SymbolResolver : ISymbolResolver
    {
        public CompletionSymbol Resolve(TextDocument document, int offset)
        {
            if (document == null || offset < 0 || offset >= document.TextLength) return null;

            var token = GetTokenAtOffset(document, offset);
            if (string.IsNullOrEmpty(token)) return null;

            return ResolveSymbol(token);
        }
        private static string GetTokenAtOffset(TextDocument document, int offset)
        {
            int start = offset;
            int end = offset;

            // Nếu đang đứng giữa chữ, lùi lại 1
            if (start > 0 && IsIdentifierChar(document.GetCharAt(start - 1))) start--;

            // Scan backward
            while (start > 0 && IsIdentifierChar(document.GetCharAt(start - 1))) start--;

            // Scan forward
            while (end < document.TextLength && IsIdentifierChar(document.GetCharAt(end))) end++;

            if (start == end) return string.Empty;

            return document.GetText(start, end - start);
        }

        private static bool IsIdentifierChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private static CompletionSymbol ResolveSymbol(string token)
        {
            var vm = ScriptViewModel.Instance;
            var key = token.ToLowerInvariant();

            if (!vm.SymbolByName.TryGetValue(key, out var candidates)) return null;

            if (candidates.Count == 1) return candidates[0];

            // Nhiều symbol trùng tên → xử lý tiếp
            return ResolveBestCandidate(candidates);
        }

        private static CompletionSymbol ResolveBestCandidate(IReadOnlyList<CompletionSymbol> candidates)
        {
            // Ưu tiên function > constant > enum member
            return candidates.OrderByDescending(GetKindPriority).First();
        }

        private static int GetKindPriority(CompletionSymbol s)
        {
            return s.Kind switch
            {
                SymbolKind.Function => 100,
                SymbolKind.Constant => 80,
                SymbolKind.EnumMember => 60,
                SymbolKind.Enum => 50,
                _ => 0
            };
        }

        public CompletionSymbol ResolveExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression)) return null;

            var parts = expression.Split('.');
            if (parts.Length == 0) return null;

            CompletionSymbol current = ResolveRoot(parts[0]);
            if (current == null) return null;

            for (int i = 1; i < parts.Length; i++)
            {
                current = ResolveMember(current, parts[i]);
                if (current == null) return null;
            }

            return current;
        }

        private CompletionSymbol ResolveRoot(string name)
        {
            var key = name.ToLowerInvariant();

            if (!ScriptViewModel.Instance.SymbolByName.TryGetValue(key, out var list)) return null;

            return list.Count == 1 ? list[0] : ResolveBestCandidate(list);
        }

        private CompletionSymbol ResolveMember(CompletionSymbol parent, string name)
        {
            if (parent == null) return null;

            // 1. direct members (enum, namespace, graph)
            if (parent.Members != null)
            {
                var direct = parent.Members
                    .FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (direct != null)
                    return direct;
            }

            // 2. TYPE-AWARE: function return type
            if (parent.Kind == SymbolKind.Function &&
                parent.Overloads != null)
            {
                foreach (var ov in parent.Overloads)
                {
                    foreach (var ret in ov.Returns)
                    {
                        foreach (var typeName in ret.Types)
                        {
                            var match = ResolveRoot(typeName);

                            if (match?.Members != null)
                            {
                                var member = match.Members
                                    .FirstOrDefault(m =>
                                        m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                                if (member != null)
                                    return member;
                            }
                        }
                    }
                }
            }

            return null;





            //////////////////










            if (parent.Members == null || parent.Members.Count == 0) return null;

            var matches = parent.Members
                .Where(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            //var key = name.ToLowerInvariant();

            //var matches = parent.Members
            //    .Where(m => m.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
            //    .ToList();

            if (matches.Count == 0) return null;

            return matches.Count == 1 ? matches[0] : ResolveBestCandidate(matches);
        }
    }
}
