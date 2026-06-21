using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;

namespace CardEditor.Helpers
{
    public class LuaFoldingStrategy
    {
        // Regex patterns cho các cấu trúc Lua có thể fold
        private static readonly Regex localEffectStartPattern = new Regex(@"local\s+(.*?)\s*=\s*Effect\.CreateEffect.*", RegexOptions.Compiled);
        private static readonly Regex functionPattern = new Regex(@"function\s+.*?\s*\(.*?\)", RegexOptions.Compiled);

        private static readonly Regex ifPattern = new Regex(@"\bif\b", RegexOptions.Compiled);
        private static readonly Regex whilePattern = new Regex(@"\bwhile\b", RegexOptions.Compiled);
        private static readonly Regex forPattern = new Regex(@"\bfor\b", RegexOptions.Compiled);
        // private static readonly Regex doPattern = new Regex(@"\bdo\b", RegexOptions.Compiled);
        private static readonly Regex repeatPattern = new Regex(@"\brepeat\b", RegexOptions.Compiled);
        private static readonly Regex tablePattern = new Regex(@"\{", RegexOptions.Compiled);

        private static readonly Regex registerEffectEndPattern = new Regex(@".*RegisterEffect\((.*?)\)", RegexOptions.Compiled);
        private static readonly Regex endPattern = new Regex(@"\bend\b", RegexOptions.Compiled);
        private static readonly Regex untilPattern = new Regex(@"\buntil\b", RegexOptions.Compiled);
        private static readonly Regex closeBracePattern = new Regex(@"\}", RegexOptions.Compiled);

        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            if (manager == null || document == null)
                return;
            IEnumerable<NewFolding> newFoldings = CreateNewFoldings(document, out int firstErrorOffset);
            manager.UpdateFoldings(newFoldings, firstErrorOffset);
        }

        public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;
            List<NewFolding> newFoldings = new List<NewFolding>();
            Stack<FoldingBlock> stack = new Stack<FoldingBlock>();

            // Quét qua từng dòng trong document
            for (int lineNumber = 1; lineNumber <= document.LineCount; lineNumber++)
            {
                string line = document.GetText(document.GetLineByNumber(lineNumber));
                int offset = document.GetLineByNumber(lineNumber).Offset;

                // Kiểm tra các điểm bắt đầu có thể fold
                CheckStartFold(line, offset, stack, localEffectStartPattern, "effect");
                CheckStartFold(line, offset, stack, functionPattern, "function");
                CheckStartFold(line, offset, stack, ifPattern, "if");
                CheckStartFold(line, offset, stack, whilePattern, "while");
                CheckStartFold(line, offset, stack, forPattern, "for");
                //CheckStartFold(line, offset, stack, doPattern, "do");
                CheckStartFold(line, offset, stack, repeatPattern, "repeat");
                CheckStartFold(line, offset, stack, tablePattern, "table");

                // Kiểm tra các điểm kết thúc fold
                CheckEndFold(line, offset, stack, newFoldings, registerEffectEndPattern);
                CheckEndFold(line, offset, stack, newFoldings, endPattern);
                CheckEndFold(line, offset, stack, newFoldings, untilPattern);
                CheckEndFold(line, offset, stack, newFoldings, closeBracePattern);
            }

            // Sắp xếp foldings theo offset
            newFoldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return newFoldings;
        }

        private void CheckStartFold(string line, int offset, Stack<FoldingBlock> stack, Regex pattern, string type)
        {
            Match match = pattern.Match(line);
            if (match.Success)
            {
                int startOffset = offset + match.Index;
                stack.Push(new FoldingBlock { StartOffset = startOffset, Type = type });
            }
        }

        private void CheckEndFold(string line, int offset, Stack<FoldingBlock> stack, List<NewFolding> newFoldings, Regex pattern)
        {
            Match match = pattern.Match(line);
            if (match.Success && stack.Count > 0)
            {
                FoldingBlock block = stack.Pop();
                int endOffset = offset + match.Index + match.Length;
                string endLineContent = line;
                string endText = string.Empty;
                // Chỉ tạo folding nếu có đủ khoảng cách giữa start và end
                if (endOffset > block.StartOffset + 5)
                {
                    if (block.Type.ToLower() == "effect")
                        endText = endLineContent.Replace("RegisterEffect", "");
                    string name = GetFoldingName(block.Type, endText);
                    newFoldings.Add(new NewFolding(block.StartOffset, endOffset) { Name = name });
                }
            }
        }

        private string GetFoldingName(string type, string endText)
        {
            switch (type.ToLower())
            {
                case "effect":
                    return $"{endText}";
                case "function":
                    return "function...end";
                case "if":
                    return "if...end";
                case "while":
                    return "while...end";
                case "for":
                    return "for...end";
                case "do":
                    return "do...end";
                case "repeat":
                    return "repeat...until";
                case "table":
                    return "{...}";
                default:
                    return "...";
            }
        }

        private class FoldingBlock
        {
            public int StartOffset { get; set; }
            public string Type { get; set; }
        }
    }

    // Helper class để quản lý folding
    public class LuaFoldingManager
    {
        private readonly TextEditor textEditor;
        private FoldingManager foldingManager;
        private readonly LuaFoldingStrategy foldingStrategy;
        private bool isEnabled;

        public LuaFoldingManager(TextEditor editor)
        {
            textEditor = editor;
            foldingStrategy = new LuaFoldingStrategy();
            isEnabled = false;
        }

        public void Enable()
        {
            if (!isEnabled)
            {
                if (foldingManager == null)
                {
                    try
                    {
                        foldingManager = FoldingManager.Install(textEditor.TextArea);
                    }
                    catch (ArgumentException)
                    {
                        foldingManager = GetExistingFoldingManagerByReflection(textEditor.TextArea);
                    }
                }


                // foldingManager = FoldingManager.Install(textEditor.TextArea);
                textEditor.TextChanged -= TextEditor_TextChanged;
                textEditor.TextChanged += TextEditor_TextChanged;
                if (foldingManager != null)
                {
                    UpdateFoldings();
                }
                // UpdateFoldings();
                isEnabled = true;
            }
        }
        private FoldingManager GetExistingFoldingManagerByReflection(TextArea textArea)
        {
            var field = typeof(FoldingManager).GetField("installedManagers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (field != null)
            {
                var installedManagers = field.GetValue(null) as System.Collections.IDictionary;
                if (installedManagers != null && installedManagers.Contains(textArea))
                {
                    return installedManagers[textArea] as FoldingManager;
                }
            }
            return null;
        }


        public void Disable()
        {
            if (isEnabled)
            {
                textEditor.TextChanged -= TextEditor_TextChanged;
                FoldingManager.Uninstall(foldingManager);
                isEnabled = false;
            }
        }

        public void UpdateFoldings()
        {
            if (isEnabled)
            {
                foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);
            }
        }

        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            UpdateFoldings();
        }

        public void CollapseAll()
        {
            if (isEnabled)
            {
                foreach (var folding in foldingManager.AllFoldings)
                {
                    folding.IsFolded = true;
                }
            }
        }

        public void ExpandAll()
        {
            if (isEnabled)
            {
                foreach (var folding in foldingManager.AllFoldings)
                {
                    folding.IsFolded = false;
                }
            }
        }
    }
}
