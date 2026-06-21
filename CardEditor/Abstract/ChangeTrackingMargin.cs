using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using static ICSharpCode.AvalonEdit.Rendering.TextViewWeakEventManager;
using static ICSharpCode.AvalonEdit.TextEditorWeakEventManager;


namespace CardEditor.Abstract
{
    public enum LineStatus
    {
        Unchanged,
        Modified,
        Saved
    }
    public class ChangeTrackingMargin : AbstractMargin
    {
        private const double MarginWidth = 6.0;
        private readonly SolidColorBrush modifiedBrush;
        private readonly SolidColorBrush savedBrush;
        private readonly ChangeTracker changeTracker;

        public ChangeTrackingMargin(ChangeTracker tracker)
        {
            changeTracker = tracker ?? throw new ArgumentNullException(nameof(tracker));

            // Màu vàng cho dòng đã sửa nhưng chưa lưu
            modifiedBrush = new SolidColorBrush(Color.FromRgb(255, 238, 98));
            modifiedBrush.Freeze();

            // Màu xanh lá cho dòng đã lưu
            savedBrush = new SolidColorBrush(Color.FromRgb(155, 185, 85));
            savedBrush.Freeze();

            changeTracker.Changed += (s, e) => InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(MarginWidth, 0);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (TextView == null || !TextView.VisualLinesValid) return;

            var renderSize = RenderSize;

            foreach (var line in TextView.VisualLines)
            {
                var lineNumber = line.FirstDocumentLine.LineNumber;
                var status = changeTracker.GetLineStatus(lineNumber);

                if (status == LineStatus.Unchanged) continue;

                var brush = status == LineStatus.Modified ? modifiedBrush : savedBrush;
                var lineTop = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop) - TextView.ScrollOffset.Y;
                var lineBottom = line.GetTextLineVisualYPosition(line.TextLines[line.TextLines.Count - 1], VisualYPosition.TextBottom) - TextView.ScrollOffset.Y;

                var rect = new Rect(0, lineTop, MarginWidth, lineBottom - lineTop);
                drawingContext.DrawRectangle(brush, null, rect);
            }
        }

        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
            {
                oldTextView.VisualLinesChanged -= OnVisualLinesChanged;
            }
            base.OnTextViewChanged(oldTextView, newTextView);
            if (newTextView != null)
            {
                newTextView.VisualLinesChanged += OnVisualLinesChanged;
            }
            InvalidateVisual();
        }

        private void OnVisualLinesChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }
    }

    public class ChangeTracker
    {
        private readonly Dictionary<int, LineStatus> lineStatuses = new Dictionary<int, LineStatus>();
        private readonly ICSharpCode.AvalonEdit.Document.TextDocument document;
        private string originalText;

        public event EventHandler Changed;

        public ChangeTracker(ICSharpCode.AvalonEdit.Document.TextDocument doc)
        {
            document = doc ?? throw new ArgumentNullException(nameof(doc));
            originalText = doc.Text;
            document.Changed += Document_Changed;
        }

        private void Document_Changed(object sender, ICSharpCode.AvalonEdit.Document.DocumentChangeEventArgs e)
        {
            try
            {
                // Đảm bảo offset không vượt quá document length
                int offset = Math.Min(e.Offset, document.TextLength);
                if (offset < 0 || offset > document.TextLength) return;

                var startLine = document.GetLineByOffset(offset);

                // Tính endLine dựa trên insertion length (sau khi insert)
                // hoặc dựa trên offset (nếu là deletion)
                int endOffset;
                if (e.InsertionLength > 0)
                {
                    // Có text được insert
                    endOffset = Math.Min(offset + e.InsertionLength, document.TextLength);
                }
                else
                {
                    // Chỉ có deletion, dùng offset hiện tại
                    endOffset = offset;
                }

                var endLine = document.GetLineByOffset(endOffset);

                // Đánh dấu các dòng bị ảnh hưởng
                for (int lineNum = startLine.LineNumber; lineNum <= endLine.LineNumber; lineNum++)
                {
                    if (lineNum > 0 && lineNum <= document.LineCount)
                    {
                        lineStatuses[lineNum] = LineStatus.Modified;
                    }
                }

                // Nếu có dòng bị xóa, cần shift các dòng sau lên
                if (e.RemovalLength > 0 && e.InsertionLength == 0)
                {
                    // Tính số dòng bị xóa
                    int removedLines = 0;
                    int tempOffset = offset;
                    for (int i = 0; i < e.RemovalLength && tempOffset < document.TextLength; i++)
                    {
                        if (i > 0 && tempOffset > 0)
                        {
                            // Ước tính số dòng bị xóa (không chính xác 100% nhưng an toàn)
                            removedLines = e.RemovalLength / 50; // Giả sử trung bình 50 ký tự/dòng
                            break;
                        }
                    }

                    if (removedLines > 0)
                    {
                        var newStatuses = new Dictionary<int, LineStatus>();
                        foreach (var kvp in lineStatuses)
                        {
                            if (kvp.Key < startLine.LineNumber)
                            {
                                newStatuses[kvp.Key] = kvp.Value;
                            }
                            else if (kvp.Key > startLine.LineNumber + removedLines)
                            {
                                int newLineNum = kvp.Key - removedLines;
                                if (newLineNum > 0 && newLineNum <= document.LineCount)
                                {
                                    newStatuses[newLineNum] = kvp.Value;
                                }
                            }
                        }
                        lineStatuses.Clear();
                        foreach (var kvp in newStatuses)
                        {
                            lineStatuses[kvp.Key] = kvp.Value;
                        }
                    }
                }

                Changed?.Invoke(this, EventArgs.Empty);
            }
            catch (ArgumentOutOfRangeException)
            {
                // Bỏ qua lỗi nếu offset không hợp lệ
                // Có thể xảy ra trong một số trường hợp edge case
            }
        }

        public LineStatus GetLineStatus(int lineNumber)
        {
            return lineStatuses.TryGetValue(lineNumber, out var status) ? status : LineStatus.Unchanged;
        }

        public void MarkAsSaved()
        {
            var modifiedLines = new List<int>();
            foreach (var kvp in lineStatuses)
            {
                if (kvp.Value == LineStatus.Modified)
                {
                    modifiedLines.Add(kvp.Key);
                }
            }

            foreach (var lineNum in modifiedLines)
            {
                lineStatuses[lineNum] = LineStatus.Saved;
            }

            originalText = document.Text;
            Changed?.Invoke(this, EventArgs.Empty);
        }
        public void Reset()
        {
            lineStatuses.Clear();
            originalText = document.Text;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        public void ClearSavedMarkers()
        {
            var savedLines = new List<int>();
            foreach (var kvp in lineStatuses)
            {
                if (kvp.Value == LineStatus.Saved)
                {
                    savedLines.Add(kvp.Key);
                }
            }

            foreach (var lineNum in savedLines)
            {
                lineStatuses.Remove(lineNum);
            }

            Changed?.Invoke(this, EventArgs.Empty);
        }

    }
}
