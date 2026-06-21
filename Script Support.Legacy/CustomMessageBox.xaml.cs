using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScriptSupport.Legacy
{
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string title, string message)
        {
            InitializeComponent();
            tbMessagetitle.Text = title;
            Messageconten.Text = message;
            LoadConfig();
        }
        private void LoadConfig()
        {
            //string backgroundHex = SettingService.GetStringSetting("backgroundset", "#FF000000");
            //string foregroundHex = SettingService.GetStringSetting("foregroundset", "#FFFFFFFF");

            //Color backgroundColor = (Color)ColorConverter.ConvertFromString(backgroundHex);
            //Color foregroundColor = (Color)ColorConverter.ConvertFromString(foregroundHex);

            //maingrid.Background = new SolidColorBrush(backgroundColor);
            //this.Foreground = new SolidColorBrush(foregroundColor);
        }
        // Hàm tĩnh để hiển thị cửa sổ thông báo
        public static void Show(string title, string message, Brush themeColor)
        {
            var customMessageBox = new CustomMessageBox(title, message);
            customMessageBox.border.Background = themeColor;
            customMessageBox.ShowDialog();
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

    }
}
