using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace CardEditor.Converter
{
    public class BoolToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is true ? new GridLength(0) : GridLength.Auto;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
