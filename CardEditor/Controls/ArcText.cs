using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Globalization;

namespace CardEditor.Controls
{
    public enum ArcFitMode { Spacing, FontSize, SpacingThenFontSize }
    public enum ArcAnchor { Center, Start, End }
    public enum ArcSide { Auto, Inside, Outside }
    public class ArcText : FrameworkElement
    {
        private const FrameworkPropertyMetadataOptions Render = FrameworkPropertyMetadataOptions.AffectsRender;
        private const double FallbackSize = 100;   // khi cha không giới hạn kích thước (StackPanel, ScrollViewer...)

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(ArcText), new FrameworkPropertyMetadata(string.Empty, Render));

        // NaN = tự động (vừa khít bên trong control)
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            nameof(Radius), typeof(double), typeof(ArcText), new FrameworkPropertyMetadata(double.NaN, Render));

        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
            nameof(Spacing), typeof(double), typeof(ArcText), new FrameworkPropertyMetadata(0.0, Render));

        public static readonly DependencyProperty IsOverlayProperty = DependencyProperty.Register(
            nameof(IsOverlay), typeof(bool), typeof(ArcText), new FrameworkPropertyMetadata(true, Render));

        public static readonly DependencyProperty MaxSweepAngleProperty = DependencyProperty.Register(
            nameof(MaxSweepAngle), typeof(double), typeof(ArcText), new FrameworkPropertyMetadata(300.0, Render));

        public static readonly DependencyProperty FitModeProperty = DependencyProperty.Register(
            nameof(FitMode), typeof(ArcFitMode), typeof(ArcText),
            new FrameworkPropertyMetadata(ArcFitMode.SpacingThenFontSize, Render));

        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(
            nameof(Angle), typeof(double), typeof(ArcText), new FrameworkPropertyMetadata(180.0, Render));

        public static readonly DependencyProperty AnchorProperty = DependencyProperty.Register(
            nameof(Anchor), typeof(ArcAnchor), typeof(ArcText), new FrameworkPropertyMetadata(ArcAnchor.Center, Render));

        public static readonly DependencyProperty SideProperty = DependencyProperty.Register(
            nameof(Side), typeof(ArcSide), typeof(ArcText), new FrameworkPropertyMetadata(ArcSide.Auto, Render));

        public static readonly DependencyProperty FontFamilyProperty =
            TextElement.FontFamilyProperty.AddOwner(typeof(ArcText),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, Render | FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(typeof(ArcText),
                new FrameworkPropertyMetadata(12.0, Render | FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty FontWeightProperty =
            TextElement.FontWeightProperty.AddOwner(typeof(ArcText),
                new FrameworkPropertyMetadata(FontWeights.Normal, Render | FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty ForegroundProperty =
            TextElement.ForegroundProperty.AddOwner(typeof(ArcText),
                new FrameworkPropertyMetadata(Brushes.Black, Render | FrameworkPropertyMetadataOptions.Inherits));

        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public double Radius { get => (double)GetValue(RadiusProperty); set => SetValue(RadiusProperty, value); }
        public double Spacing { get => (double)GetValue(SpacingProperty); set => SetValue(SpacingProperty, value); }
        public bool IsOverlay { get => (bool)GetValue(IsOverlayProperty); set => SetValue(IsOverlayProperty, value); }
        public double MaxSweepAngle { get => (double)GetValue(MaxSweepAngleProperty); set => SetValue(MaxSweepAngleProperty, value); }
        public ArcFitMode FitMode { get => (ArcFitMode)GetValue(FitModeProperty); set => SetValue(FitModeProperty, value); }
        public double Angle { get => (double)GetValue(AngleProperty); set => SetValue(AngleProperty, value); }
        public ArcAnchor Anchor { get => (ArcAnchor)GetValue(AnchorProperty); set => SetValue(AnchorProperty, value); }
        public ArcSide Side { get => (ArcSide)GetValue(SideProperty); set => SetValue(SideProperty, value); }
        public FontFamily FontFamily { get => (FontFamily)GetValue(FontFamilyProperty); set => SetValue(FontFamilyProperty, value); }
        public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
        public FontWeight FontWeight { get => (FontWeight)GetValue(FontWeightProperty); set => SetValue(FontWeightProperty, value); }
        public Brush Foreground { get => (Brush)GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }

        public ArcText()
        {
            ClipToBounds = true;   // chữ tràn khỏi control thì bị cắt, đối xứng vì tâm vòng = tâm control
        }

        // ---------------- Layout: kích thước không phụ thuộc nội dung ----------------

        protected override Size MeasureOverride(Size available)
        {
            double w = double.IsInfinity(available.Width) ? FallbackSize : available.Width;
            double h = double.IsNaN(Height)
                ? w                                                    // không đặt Height -> hình vuông
                : (double.IsInfinity(available.Height) ? w : available.Height);
            return new Size(w, h);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double w = finalSize.Width;
            double h = double.IsNaN(Height) ? w : finalSize.Height;    // giữ hình vuông theo chiều rộng
            return new Size(w, h);
        }

        // ---------------- Tính toán hình học ----------------

        private sealed class ArcLayout
        {
            public List<FormattedText> Glyphs = new List<FormattedText>();
            public double Spacing, Radius, Rm, LineH, Cx, Cy, StartTheta, Dir;
            public bool Outside;
        }

        private List<FormattedText> MakeGlyphs(double size)
        {
            var typeface = new Typeface(FontFamily, FontStyles.Normal, FontWeight, FontStretches.Normal);
            double ppd = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var brush = Foreground ?? Brushes.Black;

            var list = new List<FormattedText>();
            var it = StringInfo.GetTextElementEnumerator(Text ?? string.Empty);   // giữ nguyên dấu tiếng Việt
            while (it.MoveNext())
                list.Add(new FormattedText((string)it.Current, CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight, typeface, size, brush, ppd));
            return list;
        }

        private ArcLayout Build(Size box)
        {
            double half = Math.Min(box.Width, box.Height) / 2;
            var glyphs = MakeGlyphs(FontSize);
            if (glyphs.Count == 0 || half <= 0) return new ArcLayout();

            bool auto = double.IsNaN(Radius);
            double fs = FontSize, spacing = Spacing;
            int gaps = glyphs.Count - 1;
            double sumW = glyphs.Sum(g => g.WidthIncludingTrailingWhitespace);
            double lineH = glyphs[0].Height;

            // Bán kính giữa dòng chữ theo hệ số co s:  rm(s) = a + b*s
            //   Radius cố định: rm = Radius + lineH/2·s       Radius tự động: rm = half − lineH/2·s
            double a = auto ? half : Radius;
            double b = auto ? -lineH / 2 : lineH / 2;

            if (!IsOverlay)
            {
                double maxRad = Math.Max(1.0, Math.Min(MaxSweepAngle, 360.0)) * Math.PI / 180;
                double rm0 = Math.Max(1.0, a + b);

                if (sumW + spacing * gaps > maxRad * rm0)          // vượt giới hạn -> phải co
                {
                    bool fitted = false;

                    if (FitMode != ArcFitMode.FontSize && gaps > 0)   // Bước 1: thu hẹp khoảng cách
                    {
                        double fit = (maxRad * rm0 - sumW) / gaps;
                        if (fit >= 0 || FitMode == ArcFitMode.Spacing) { spacing = fit; fitted = true; }
                        else spacing = 0;
                    }

                    if (!fitted && FitMode != ArcFitMode.Spacing)     // Bước 2: thu nhỏ cỡ chữ (công thức đóng)
                    {
                        double W = sumW + spacing * gaps;
                        double denom = W - maxRad * b;
                        if (denom > 0 && a > 0)
                        {
                            double s = Math.Min(1.0, maxRad * a / denom);
                            fs *= s; spacing *= s;
                            glyphs = MakeGlyphs(fs);
                            sumW = glyphs.Sum(g => g.WidthIncludingTrailingWhitespace);
                            lineH = glyphs[0].Height;
                        }
                    }
                }
            }

            double radius = auto ? Math.Max(1.0, half - lineH) : Radius;   // auto: chữ nằm trọn trong control
            double rm = radius + lineH / 2;
            double total = sumW + spacing * gaps;
            double alpha = total / rm / 2;                                 // nửa góc quét (rad)

            // Góc neo: 0° = đỉnh, chiều kim đồng hồ (y hướng xuống)
            double anchor = (Angle - 90) * Math.PI / 180;
            bool outside = Side == ArcSide.Outside ||
                           (Side == ArcSide.Auto && Math.Sin(anchor) < -1e-9);
            double dir = outside ? 1 : -1;

            double center = Anchor == ArcAnchor.Start ? anchor + dir * alpha
                          : Anchor == ArcAnchor.End ? anchor - dir * alpha
                          : anchor;

            return new ArcLayout
            {
                Glyphs = glyphs,
                Spacing = spacing,
                Radius = radius,
                Rm = rm,
                LineH = lineH,
                StartTheta = center - dir * alpha,
                Dir = dir,
                Outside = outside,
                Cx = box.Width / 2,
                Cy = box.Height / 2                    // tâm vòng tròn = tâm control
            };
        }

        // ---------------- Vẽ ----------------

        protected override void OnRender(DrawingContext dc)
        {
            var l = Build(RenderSize);
            if (l.Glyphs.Count == 0) return;

            double theta = l.StartTheta;

            foreach (var g in l.Glyphs)
            {
                double w = g.WidthIncludingTrailingWhitespace;
                double mid = theta + l.Dir * (w / 2) / l.Rm;

                double px = l.Cx + l.Radius * Math.Cos(mid);
                double py = l.Cy + l.Radius * Math.Sin(mid);
                double rot = l.Outside ? mid + Math.PI / 2 : mid - Math.PI / 2;

                var m = Matrix.Identity;
                m.Rotate(rot * 180 / Math.PI);
                m.Translate(px, py);

                dc.PushTransform(new MatrixTransform(m));
                dc.DrawText(g, new Point(-w / 2, l.Outside ? -l.LineH : 0));
                dc.Pop();

                theta += l.Dir * (w + l.Spacing) / l.Rm;
            }
        }
    }
}
