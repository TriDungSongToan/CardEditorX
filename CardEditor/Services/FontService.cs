using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CardEditor.Services
{
    public class FontService
    {
        // Hàm để thay đổi font family
        public void SetFontFamily(FontFamily fontFamily)
        {
            // Cập nhật FontFamily cho toàn bộ ứng dụng
            foreach (Window window in Application.Current.Windows)
            {
                ApplyFontFamily(window, fontFamily);
            }
        }
        // Hàm áp dụng FontFamily cho cửa sổ
        private void ApplyFontFamily(Window window, FontFamily fontFamily)
        {
            window.FontFamily = fontFamily;
            // Duyệt tất cả các control con trong cửa sổ
            foreach (var control in FindLogicalChildren<FrameworkElement>(window))
            {
                // Kiểm tra nếu control có thể áp dụng FontFamily
                if (control is Control controlWithFontFamily)
                {
                    controlWithFontFamily.FontFamily = fontFamily;
                }
            }
        }

        // Hàm để thay đổi font size
        public void SetFontSize(int fontSize)
        {
            // Cập nhật FontSize cho tất cả các cửa sổ trong ứng dụng
            foreach (Window window in Application.Current.Windows)
            {
                ApplyFontSize(window, fontSize);
            }
        }
        // Hàm áp dụng Font Size cho cửa sổ
        private void ApplyFontSize(Window window, int fontSize)
        {
            window.FontSize = fontSize;
            // Duyệt qua tất cả các control con trong cửa sổ
            foreach (var control in FindLogicalChildren<FrameworkElement>(window))
            {
                // Kiểm tra nếu control có thể thay đổi FontSize
                if (control is Control controlWithFontSize)
                {
                    controlWithFontSize.FontSize = fontSize;
                }
            }
        }
        // Hàm tìm tất cả các control con trong một cửa sổ
        private static System.Collections.Generic.IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (var child in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (child is T) yield return (T)child;

                    if (child is DependencyObject childObj)
                    {
                        foreach (var nestedChild in FindLogicalChildren<T>(childObj))
                        {
                            yield return nestedChild;
                        }
                    }
                }
            }
        }
    }
}
