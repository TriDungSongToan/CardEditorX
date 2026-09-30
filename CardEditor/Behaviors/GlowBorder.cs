using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CardEditor.Behaviors
{
    public static class GlowBorder
    {
        // Bật/tắt hiệu ứng
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(GlowBorder),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);

        // Tốc độ chạy (px/giây)
        public static readonly DependencyProperty SpeedProperty =
            DependencyProperty.RegisterAttached("Speed", typeof(double), typeof(GlowBorder), new PropertyMetadata(80.0));
        public static void SetSpeed(DependencyObject obj, double value) => obj.SetValue(SpeedProperty, value);
        public static double GetSpeed(DependencyObject obj) => (double)obj.GetValue(SpeedProperty);

        // Màu lửa
        public static readonly DependencyProperty GlowColorProperty =
            DependencyProperty.RegisterAttached("GlowColor", typeof(Color), typeof(GlowBorder),
                new PropertyMetadata(Color.FromRgb(0xFF, 0x45, 0x00))); // OrangeRed mặc định
        public static void SetGlowColor(DependencyObject obj, Color value) => obj.SetValue(GlowColorProperty, value);
        public static Color GetGlowColor(DependencyObject obj) => (Color)obj.GetValue(GlowColorProperty);

        // Độ dày nét viền cháy
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.RegisterAttached("StrokeThickness", typeof(double), typeof(GlowBorder), new PropertyMetadata(2.0));
        public static void SetStrokeThickness(DependencyObject obj, double value) => obj.SetValue(StrokeThicknessProperty, value);
        public static double GetStrokeThickness(DependencyObject obj) => (double)obj.GetValue(StrokeThicknessProperty);

        // Độ dài đoạn sáng / khoảng trống
        public static readonly DependencyProperty DashLengthProperty =
            DependencyProperty.RegisterAttached("DashLength", typeof(double), typeof(GlowBorder), new PropertyMetadata(20.0));
        public static void SetDashLength(DependencyObject obj, double value) => obj.SetValue(DashLengthProperty, value);
        public static double GetDashLength(DependencyObject obj) => (double)obj.GetValue(DashLengthProperty);

        public static readonly DependencyProperty GapLengthProperty =
            DependencyProperty.RegisterAttached("GapLength", typeof(double), typeof(GlowBorder), new PropertyMetadata(100.0));
        public static void SetGapLength(DependencyObject obj, double value) => obj.SetValue(GapLengthProperty, value);
        public static double GetGapLength(DependencyObject obj) => (double)obj.GetValue(GapLengthProperty);

        private static readonly System.Collections.Generic.Dictionary<Border, FireBorderAdorner> _adorners = new();

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Border border) return;

            if ((bool)e.NewValue)
            {
                if (border.IsLoaded) Attach(border);
                else border.Loaded += Border_Loaded;
            }
            else
            {
                Detach(border);
            }
        }

        private static void Border_Loaded(object sender, RoutedEventArgs e)
        {
            var border = (Border)sender;
            border.Loaded -= Border_Loaded;
            Attach(border);
        }

        private static void Attach(Border border)
        {
            var layer = AdornerLayer.GetAdornerLayer(border);
            if (layer == null) return; // xem lưu ý bên dưới nếu null

            var adorner = new FireBorderAdorner(border);
            layer.Add(adorner);
            _adorners[border] = adorner;

            border.Unloaded += Border_Unloaded;
        }

        private static void Border_Unloaded(object sender, RoutedEventArgs e) => Detach((Border)sender);

        private static void Detach(Border border)
        {
            if (_adorners.TryGetValue(border, out var adorner))
            {
                AdornerLayer.GetAdornerLayer(border)?.Remove(adorner);
                adorner.Dispose();
                _adorners.Remove(border);
            }
            border.Unloaded -= Border_Unloaded;
        }
    }
}
