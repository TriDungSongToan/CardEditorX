using System;
using System.Windows.Data;
using System.Globalization;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Converter
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class AscDescConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? CMess.Ascending.ToText() : CMess.Descending.ToText();
            return CMess.Descending.ToText();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (string.Equals(s, CMess.Ascending.ToText(), StringComparison.OrdinalIgnoreCase)) return true;
                if (string.Equals(s, CMess.Descending.ToText(), StringComparison.OrdinalIgnoreCase)) return false;
            }
            return Binding.DoNothing;
        }
    }
}
