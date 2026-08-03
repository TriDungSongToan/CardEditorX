using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

namespace CardEditor.Converter
{
    public class SelectedItemToBackground : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                switch (count)
                {
                    case 0: return Brushes.Red;
                    case 1: return Brushes.Orange;
                    case 2: return Brushes.Yellow;
                    case 3: return Brushes.Green;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
