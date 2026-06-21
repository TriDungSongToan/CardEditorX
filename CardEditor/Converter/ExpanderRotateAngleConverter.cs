using System;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CardEditor.Converter
{
    public class ExpanderRotateAngleConverter : IValueConverter
    {
        public static readonly ExpanderRotateAngleConverter Instance = new ExpanderRotateAngleConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ExpandDirection direction)
            {
                switch (direction)
                {
                    case ExpandDirection.Down:
                        return 0.0;
                    case ExpandDirection.Left:
                        return -90.0;
                    case ExpandDirection.Up:
                        return 180.0;
                    case ExpandDirection.Right:
                        return 90.0;
                    default:
                        return 0.0;
                }
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
