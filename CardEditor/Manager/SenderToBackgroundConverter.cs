using System;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CardEditor.Manager
{
    public class SenderToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string sender && sender == "User")
            {
                return new SolidColorBrush(Color.FromRgb(144, 238, 144)); // Xanh nhạt cho User
            }
            return new SolidColorBrush(Color.FromRgb(220, 220, 220)); // Xám cho AI/Discord
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
