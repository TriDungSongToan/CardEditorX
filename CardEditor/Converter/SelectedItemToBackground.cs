using System;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

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
