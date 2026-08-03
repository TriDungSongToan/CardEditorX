using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Document;

namespace CardEditor.Manager
{
    public class BracketHighlightRenderer : DocumentColorizingTransformer
    {
        private readonly TextDocument _document;
        private readonly int _openBracketOffset;
        private readonly int _closeBracketOffset;

        public BracketHighlightRenderer(TextDocument document, int openBracketOffset, int closeBracketOffset)
        {
            _document = document;
            _openBracketOffset = openBracketOffset;
            _closeBracketOffset = closeBracketOffset;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            int lineStartOffset = line.Offset;
            int lineEndOffset = line.EndOffset;

            // Kiểm tra tính hợp lệ của các offset
            if (_openBracketOffset < 0 || _closeBracketOffset < 0 || _openBracketOffset >= _document.TextLength || _closeBracketOffset >= _document.TextLength)
                return;

            // Tô sáng ngoặc mở
            if (_openBracketOffset >= lineStartOffset && _openBracketOffset < lineEndOffset)
            {
                ChangeLinePart(
                    _openBracketOffset,
                    _openBracketOffset + 1,
                    element => element.TextRunProperties.SetBackgroundBrush(new SolidColorBrush(Color.FromArgb(128, 255, 165, 0)))
                );
            }

            // Tô sáng ngoặc đóng
            if (_closeBracketOffset >= lineStartOffset && _closeBracketOffset < lineEndOffset)
            {
                ChangeLinePart(
                    _closeBracketOffset,
                    _closeBracketOffset + 1,
                    element => element.TextRunProperties.SetBackgroundBrush(new SolidColorBrush(Color.FromArgb(128, 255, 165, 0)))
                );
            }
        }
    }
}
