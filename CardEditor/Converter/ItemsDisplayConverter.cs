using System;
using System.Windows.Data;
using System.Globalization;
using CardEditor.Models;

namespace CardEditor.Converter
{
    public class CmbItemsDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CmbItems item)
            {
                return item.ShortName;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
