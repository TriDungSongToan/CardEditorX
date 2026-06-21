using System.Windows;
using Dragablz;
using MahApps.Metro.Controls;

namespace CardEditor.Models
{
    public class CustomInterTabClient : IInterTabClient
    {
        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            // Tạo cửa sổ mới
            var newWindow = new MetroWindow
            {
                Width = 1110,
                Height = 800,
                Title = "CardEditor X"
            };

            // Tạo TabablzControl mới cho cửa sổ
            var newTabControl = new TabablzControl
            {
                ShowDefaultCloseButton = true,
                ShowDefaultAddButton = true,
                Style = (Style)Application.Current.FindResource("MaterialDesignAlternateTabablzControlStyle"),
                InterTabController = new InterTabController { InterTabClient = this }
            };

            newWindow.Content = newTabControl;
            return new NewTabHost<Window>(newWindow, newTabControl);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            // return TabEmptiedResponse.DoNothing;
            // Nếu cửa sổ không chứa Tab nào thì đóng cửa sổ
            return window is MetroWindow ? TabEmptiedResponse.CloseWindowOrLayoutBranch : TabEmptiedResponse.DoNothing;
        }

    }
}
