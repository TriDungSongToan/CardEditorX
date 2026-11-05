using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;

namespace CardEditor.Behaviors
{
    public class InsertionAdorner : Adorner
    {
        private readonly bool _isBelow;
        private readonly Pen _pen;

        public InsertionAdorner(UIElement adorned, bool isBelow) : base(adorned)
        {
            _isBelow = isBelow;
            _pen = new Pen(new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)), 2);
            _pen.Freeze();
            IsHitTestVisible = false;   // không chặn chuột
        }

        protected override void OnRender(DrawingContext dc)
        {
            double w = AdornedElement.RenderSize.Width;
            double h = AdornedElement.RenderSize.Height;
            double y = _isBelow ? h : 0;
            dc.DrawLine(_pen, new Point(0, y), new Point(w, y));
        }
    }
    public class DropTargetAdorner : Adorner
    {
        private readonly Pen _pen;
        private readonly Brush _fill;

        public DropTargetAdorner(UIElement adorned) : base(adorned)
        {
            _pen = new Pen(new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)), 2);
            _pen.Freeze();
            _fill = new SolidColorBrush(Color.FromArgb(0x33, 0x3B, 0x82, 0xF6));
            _fill.Freeze();
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(_fill, _pen, new Rect(0, 0, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height));
        }
    }
}
