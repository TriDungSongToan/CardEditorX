using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace CardEditor.Converter
{
    public class FontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // String -> FontFamily (cho ComboBox hiển thị)
            if (value is string fontName && !string.IsNullOrEmpty(fontName))
            {
                try
                {
                    return new FontFamily(fontName);
                }
                catch
                {
                    return new FontFamily("Consolas"); // Fallback
                }
            }
            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // FontFamily -> String (khi user chọn)
            if (value is FontFamily fontFamily)
            {
                return fontFamily.Source;
            }
            return "Consolas"; // Default
        }
    }
}
