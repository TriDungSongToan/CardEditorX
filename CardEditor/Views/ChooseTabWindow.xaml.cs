using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for ChooseTabWindow.xaml
    /// </summary>
    public partial class ChooseTabWindow : Window
    {
        public string SelectedOption { get; private set; }
        public ChooseTabWindow()
        {
            InitializeComponent();
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.ShowInTaskbar = false;

            Point mousePosition = Mouse.GetPosition(null);
            this.Left = mousePosition.X;
            this.Top = mousePosition.Y + 40;
        }

        private void btndata_Click(object sender, RoutedEventArgs e)
        {
            SelectedOption = "Data";
            this.DialogResult = true;
            this.Close();
        }
        private void btndeck_Click(object sender, RoutedEventArgs e)
        {
            SelectedOption = "Deck";
            this.DialogResult = true;
            this.Close();
        }
        private void btncode_Click(object sender, RoutedEventArgs e)
        {
            SelectedOption = "Code";
            this.DialogResult = true;
            this.Close();
        }
        private void btnimage_Click(object sender, RoutedEventArgs e)
        {
            SelectedOption = "Image";
            this.DialogResult = true;
            this.Close();
        }
        private void btnbanlist_Click(object sender, RoutedEventArgs e)
        {
            SelectedOption = "BanList";
            this.DialogResult = true;
            this.Close();
        }
    }
}
