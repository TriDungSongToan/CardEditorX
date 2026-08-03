using System;
using System.Windows.Data;
using System.Globalization;

namespace CardEditor.Converter
{
    public class IntSafeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value?.ToString(), out int result))
                return result;

            return -1;
        }
    }
}
