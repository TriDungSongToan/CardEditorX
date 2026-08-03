using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace CardEditor.Converter
{
    public class FlowDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (FlowDirection)value == FlowDirection.LeftToRight;
        }
    }
    public class TextAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? TextAlignment.Left : TextAlignment.Right;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (TextAlignment)value == TextAlignment.Left;
        }
    }
}
