using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CardEditor.Helpers
{
    public class MenuHelper
    {

    }

    public class MenuColorManager : INotifyPropertyChanged
    {
        private System.Windows.Media.Brush _background;
        private System.Windows.Media.Brush _foreground;

        public System.Windows.Media.Brush Background
        {
            get => _background;
            set
            {
                _background = value;
                OnPropertyChanged(nameof(Background));
            }
        }

        public System.Windows.Media.Brush Foreground
        {
            get => _foreground;
            set
            {
                _foreground = value;
                OnPropertyChanged(nameof(Foreground));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ColorBrightnessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush && parameter is string factor)
            {
                double brightnessScale = System.Convert.ToDouble(factor);
                System.Windows.Media.Color originalColor = brush.Color;

                byte r = (byte)Math.Min(255, originalColor.R * brightnessScale);
                byte g = (byte)Math.Min(255, originalColor.G * brightnessScale);
                byte b = (byte)Math.Min(255, originalColor.B * brightnessScale);

                return new System.Windows.Media.Color() { A = originalColor.A, R = r, G = g, B = b };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
