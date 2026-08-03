using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace CardEditor.Manager
{
    public class MarkSameWord : DocumentColorizingTransformer
    {
        private readonly string _selectedText;
        public MarkSameWord(string selectedText)
        {
            _selectedText = selectedText;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (string.IsNullOrEmpty(_selectedText)) return;

            int lineStartOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);
            int start = 0;
            int index;

            while ((index = text.IndexOf(_selectedText, start, StringComparison.Ordinal)) >= 0)
            {
                ChangeLinePart(
                    lineStartOffset + index,
                    lineStartOffset + index + _selectedText.Length,
                    element => element.TextRunProperties.SetBackgroundBrush(new SolidColorBrush(Color.FromArgb(100, 144, 238, 144)))
                );
                start = index + 1;
            }
        }
    }
}
