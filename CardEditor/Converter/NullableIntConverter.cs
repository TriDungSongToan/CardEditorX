using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace CardEditor.Converter
{
    public class NullableIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // int? -> string
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // string -> int?
            var text = value as string;

            if (string.IsNullOrWhiteSpace(text))
                return null; // TextBox rỗng → property = null

            if (int.TryParse(text, out int number))
                return number;

            // Nếu người dùng nhập không phải số, không cập nhật property
            return DependencyProperty.UnsetValue;
        }
    }
}
