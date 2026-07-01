using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace CardEditor.Converter
{
    public class ColorWithOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                double opacity = 1.0;
                if (parameter is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
                    opacity = parsed;

                var color = brush.Color;
                color.A = (byte)(opacity * 255);
                return new SolidColorBrush(color);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
