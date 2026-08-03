using System.IO;
using System.Windows;
using System.Windows.Media;
using CardEditor.Localization;
using CardEditor.ViewModels;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services
{
    public static class HighLightService
    {
        public static(bool, string, string) HighLightFilePath()
        {
            string highlightFilePath = Path.Combine(Path.GetDirectoryName(CardEditor.Models.AppContext.Instance.ExeFilePath),
                $@"HighLight\{ConfigViewModel.Instance.displaySetting.HighLight}.xshd");

            if (File.Exists(highlightFilePath)) return (true, highlightFilePath, string.Empty);
            else
            {
                highlightFilePath = Path.Combine(Path.GetDirectoryName(CardEditor.Models.AppContext.Instance.ExeFilePath),
                    $@"HighLight\Default.xshd");
                return (false, highlightFilePath, CMess.useDefault.ToText());
            }
        }
    }

    public class CurrentLineHighlighter : IBackgroundRenderer
    {
        private readonly TextEditor _editor;
        private readonly SolidColorBrush _highlightBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
        public KnownLayer Layer => KnownLayer.Selection;
        public CurrentLineHighlighter(TextEditor editor)
        {
            _editor = editor;
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (_editor.Document == null || !_editor.TextArea.TextView.VisualLinesValid) return;

            textView.EnsureVisualLines();
            var caretLine = _editor.Document.GetLineByOffset(_editor.CaretOffset);
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, caretLine))
            {
                drawingContext.DrawRectangle(_highlightBrush, null, new Rect(rect.Location, new Size(textView.ActualWidth, rect.Height)));
            }
        }
    }
}
