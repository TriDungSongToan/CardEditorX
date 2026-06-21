using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ScriptSupport.Legacy.Theming
{
    public class LineHighlightRenderer : IBackgroundRenderer
    {
        private readonly TextEditor _editor;
        private int _lineToHighlight = -1;

        public LineHighlightRenderer(TextEditor editor)
        {
            _editor = editor;
        }

        public void HighlightLine(int lineNumber)
        {
            _lineToHighlight = lineNumber;
            _editor.TextArea.TextView.InvalidateVisual();
        }

        public void ClearHighlight()
        {
            _lineToHighlight = -1;
            _editor.TextArea.TextView.InvalidateVisual();
        }

        public KnownLayer Layer => KnownLayer.Background;

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_lineToHighlight < 1 || _lineToHighlight > _editor.Document.LineCount)
                return;

            textView.EnsureVisualLines();
            var line = _editor.Document.GetLineByNumber(_lineToHighlight);
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, line))
            {
                drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(100, 255, 255, 0)), null, rect);
            }
        }

    }
}
