using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using System.Windows.Media.Media3D;
using System.Windows.Documents;

namespace ScriptSupport.Legacy.Services
{
    public class ThemeService
    {
        public void SetTheme(Brush background, Brush foreground, Brush theme)
        {
            // Cập nhật FontFamily cho toàn bộ ứng dụng
            foreach (Window window in Application.Current.Windows)
            {
                ApplyTheme(window, background, foreground, theme);
            }
        }
        private void ApplyTheme(Window window, Brush background, Brush foreground, Brush theme)
        {
            // Duyệt tất cả các phần tử con trong cửa sổ
            foreach (var element in FindLogicalChildren<FrameworkElement>(window))
            {
                if (element is Control controlWithColor)
                {
                    if (controlWithColor is Button || controlWithColor is ToggleButton)
                    {
                        continue;
                    }
                    else if (controlWithColor is MenuItem)
                    {
                        controlWithColor.Background = background;
                        controlWithColor.Foreground = foreground;
                        // SetSubMenuItemColors(menuItem, background, foreground); // Cập nhật SubMenuItem
                        continue;
                    }
                    else if (controlWithColor is TabControl tabControlWithColor)
                    {
                        tabControlWithColor.Background = background;
                        continue;
                    }
                    else if (controlWithColor is TabItem tabItemWithColor)
                    {
                        tabItemWithColor.Background = background;
                        if (tabItemWithColor.IsSelected)
                        {
                            tabItemWithColor.Background = theme;
                        }
                        continue;
                    }
                    else if (controlWithColor is ListBox)
                    {
                        controlWithColor.Background = background;
                        controlWithColor.Foreground = foreground;
                        continue;
                    }
                    else if (controlWithColor is TextBox)
                    {
                        controlWithColor.Background = background;
                        controlWithColor.Foreground = foreground;
                        continue;
                    }
                    else
                    {
                        controlWithColor.Background = background;
                        controlWithColor.Foreground = foreground;
                        continue;
                    }
                }
                else
                {
                    // PopUp cho Sub-Menu
                    if (element is Popup popup)
                    {
                        // Cập nhật màu nền của Popup (SubMenu)
                        var subMenuBorder = popup.Child as Border;
                        if (subMenuBorder != null)
                        {
                            subMenuBorder.Background = background;
                        }
                        // Duyệt qua các MenuItem con trong Popup
                        foreach (var popupControl in FindLogicalChildren<MenuItem>(popup))
                        {
                            popupControl.Background = background;
                            popupControl.Foreground = foreground;
                        }
                    }
                    else if (element is TextBlock textBlockWithColor)
                    {
                        textBlockWithColor.Foreground = foreground;
                        foreach (var inline in textBlockWithColor.Inlines)
                        {
                            if (inline is Hyperlink hyperlink)
                            {
                                hyperlink.Foreground = foreground;
                            }
                        }
                        continue;
                    }

                    //if (element is Border borderWithColor)
                    //{
                    //    borderWithColor.Background = foreground;
                    //    borderWithColor.BorderBrush = foreground;
                    //    borderWithColor.BorderThickness = new Thickness(2);
                    //}
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
