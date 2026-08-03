using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;

namespace CardEditor.Services
{
    public class ThemeService
    {
        public void SetTheme(Brush background, Brush foreground, Brush theme)
        {
            // Cập nhật FontFamily cho toàn bộ ứng dụng
            foreach (Window window in Application.Current.Windows)
            {
                ApplyTheme(window, background, foreground, theme);

                var userControls = FindVisualChildren<UserControl>(window);
                foreach (var control in userControls)
                {
                    ApplyTheme(control, background, foreground, theme);
                }
            }
        }
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                if (child is T t)
                {
                    yield return t;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
        private void ApplyTheme(DependencyObject target, Brush background, Brush foreground, Brush theme)
        {
            if (target == null) return;

            // Duyệt tất cả các phần tử con trong cửa sổ
            foreach (var element in FindVisualChildren<FrameworkElement>(target))
            {
                if(element is Menu menu)
                {
                    menu.Foreground = foreground;
                    menu.Background = background;
                }
                if(element is MenuItem menuItem)
                {
                    menuItem.Foreground = foreground;
                    menuItem.Background = background;

                    foreach (var subElement in FindVisualChildren<MenuItem>(menuItem))
                    {
                        subElement.Foreground = foreground;
                        subElement.Background = background;
                    }
                    //continue;
                }
                if(element is Popup popup)
                {
                    var abc = popup.Child as MenuItem;
                    if (abc != null)
                    {
                        abc.Foreground = foreground;
                        abc.Background = background;

                    }
                    if (popup.Child is FrameworkElement popupChild)
                    {
                        foreach (var childElement in FindVisualChildren<FrameworkElement>(popupChild))
                        {
                            //childElement.
                        }
                    }

                    var subMenuBorder = popup.Child as Border;
                    if (subMenuBorder != null)
                    {
                        subMenuBorder.Background = background;
                    }
                    foreach (var popupControl in FindLogicalChildren<MenuItem>(popup))
                    {
                        popupControl.Background = background;
                        popupControl.Foreground = foreground;
                    }
                    //continue;
                }
            }
        }
        // Hàm thay đổi màu nền của các MenuItem con (SubMenuItem)
        private void SetSubMenuItemColors(MenuItem menuItem, Brush background, Brush foreground)
        {
            foreach (var subItem in menuItem.Items)
            {
                if (subItem is MenuItem subMenuItem)
                {
                    subMenuItem.Background = background;
                    subMenuItem.Foreground = foreground;
                    SetSubMenuItemColors(subMenuItem, background, foreground);
                }
            }
        }

        // Hàm tìm tất cả các control con trong một cửa sổ
        private static System.Collections.Generic.IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                // Duyệt qua các children của đối tượng logic (logical tree)
                foreach (var child in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (child is T) yield return (T)child;
                    // Nếu là một container (một đối tượng có thể chứa các đối tượng con)
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
