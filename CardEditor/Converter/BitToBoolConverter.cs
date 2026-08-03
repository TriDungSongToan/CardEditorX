using System;
using System.Windows.Data;
using System.Globalization;

namespace CardEditor.Converter
{
    public class BitToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is ulong flags && values[1] is int index)
                return (flags & (1UL << index)) != 0;

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}