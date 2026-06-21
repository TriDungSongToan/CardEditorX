using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScriptSupport.Legacy.Views
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            LoadImage();
        }
        private void LoadImage()
        {
            string imagePath = "pack://application:,,,/Images/SSS.ico";
            if (Uri.IsWellFormedUriString(imagePath, UriKind.Absolute))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
                    AboutImage.Source = bitmap;
                }
                catch (Exception)
                {
                    // Nếu không thể tải hình ảnh, thay thế bằng hình ảnh mặc định.
                    AboutImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/About.ico"));
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
