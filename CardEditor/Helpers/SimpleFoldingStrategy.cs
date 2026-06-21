using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace CardEditor.Helpers
{
    public class SimpleFoldingStrategy
    {
        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            var newFoldings = CreateFoldings(document);
            manager.UpdateFoldings(newFoldings, -1);
        }

        private List<NewFolding> CreateFoldings(TextDocument document)
        {
            List<NewFolding> foldings = new List<NewFolding>();
            int start = -1;

            for (int i = 0; i < document.LineCount; i++)
            {
                DocumentLine line = document.GetLineByNumber(i + 1);
                string text = document.GetText(line);

                if (text.Trim().StartsWith("{"))  // Start of a block
                {
                    start = line.Offset;
                }
                else if (text.Trim().StartsWith("}")) // End of a block
                {
                    if (start != -1)
                    {
                        foldings.Add(new NewFolding(start, line.EndOffset));
                        start = -1;
                    }
                }
            }

            return foldings;
        }
    }
}
