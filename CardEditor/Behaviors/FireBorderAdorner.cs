using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CardEditor.Behaviors
{
    public class FireBorderAdorner : Adorner, IDisposable
    {
        private readonly Border _border;
        private readonly Rectangle _rect = new Rectangle
        {
            StrokeDashCap = PenLineCap.Round
        };
        private readonly VisualCollection _visuals;

        public FireBorderAdorner(Border adornedElement) : base(adornedElement)
        {
            _border = adornedElement;
            _visuals = new VisualCollection(this) { _rect };

            IsHitTestVisible = false; // không chặn click/hover lên Border gốc

            _rect.StrokeThickness = GlowBorder.GetStrokeThickness(_border);
            _rect.StrokeDashCap = PenLineCap.Round;
            _rect.Stroke = CreateGlowBrush(GlowBorder.GetGlowColor(_border));
            _rect.StrokeDashArray = new DoubleCollection
            {
                GlowBorder.GetDashLength(_border),
                GlowBorder.GetGapLength(_border)
            };

            _rect.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = GlowBorder.GetGlowColor(_border),
                BlurRadius = 8,
                ShadowDepth = 0,
                Opacity = 0.8
            };

            UpdateRadius();
            _border.SizeChanged += Border_SizeChanged;
            RestartAnimation();
        }

        private void Border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateArrange();
            RestartAnimation();
        }

        private void UpdateRadius()
        {
            // Lấy góc trên-trái làm chuẩn (đủ dùng nếu CornerRadius đồng đều)
            double radius = _border.CornerRadius.TopLeft;
            _rect.RadiusX = radius;
            _rect.RadiusY = radius;
        }

        private void RestartAnimation()
        {
            double w = _border.ActualWidth;
            double h = _border.ActualHeight;
            if (w <= 0 || h <= 0) return;

            double perimeter = 2 * (w + h);
            double speed = GlowBorder.GetSpeed(_border);

            var anim = new DoubleAnimation
            {
                From = 0,
                To = -perimeter,
                Duration = TimeSpan.FromSeconds(perimeter / speed),
                RepeatBehavior = RepeatBehavior.Forever
            };
            _rect.BeginAnimation(Shape.StrokeDashOffsetProperty, anim);
        }

        private static LinearGradientBrush CreateGradientBrush(Color color)
        {
            var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 0) };
            brush.GradientStops.Add(new GradientStop(Colors.Transparent, 0));
            brush.GradientStops.Add(new GradientStop(color, 0.4));
            brush.GradientStops.Add(new GradientStop(Color.FromRgb(0xFF, 0xD7, 0x00), 0.5)); // điểm sáng vàng
            brush.GradientStops.Add(new GradientStop(color, 0.6));
            brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));
            return brush;
        }
        private static Brush CreateGlowBrush(Color color)
        {
            return new SolidColorBrush(color);
        }

        protected override int VisualChildrenCount => _visuals?.Count ?? 0;
        protected override Visual GetVisualChild(int index) => _visuals[index];

        protected override Size ArrangeOverride(Size finalSize)
        {
            _rect.Arrange(new Rect(0, 0, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height));
            return finalSize;
        }

        public void Dispose()
        {
            _border.SizeChanged -= Border_SizeChanged;
            _rect.BeginAnimation(Shape.StrokeDashOffsetProperty, null);
        }

    }
}