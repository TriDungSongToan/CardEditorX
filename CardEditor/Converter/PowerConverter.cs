using System;
using System.Windows.Data;
using System.Globalization;

namespace CardEditor.Converter
{
    public class PowerConverter : IValueConverter
    {
        // Convert từ long? -> string để hiển thị trong TextBox
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is long atk)
                return atk.ToString();
            return string.Empty; // null sẽ hiển thị ""
        }

        // ConvertBack từ string trong TextBox -> long?
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;

            if (string.IsNullOrWhiteSpace(text))
                return null; // TextBox rỗng -> null

            if (!long.TryParse(text, out long result))
                return -2; // Không phải số -> -2

            if (result < 0)
                return -2; // Số âm -> -2

            return result; // Số hợp lệ >=0
        }

    }
}
