using System;
using System.Text;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.Generic;
using CardEditor.ViewModels;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        private readonly string exeFilePath;
        private const string DiscordURL = "";
        private const string GithubURL = "https://github.com/TriDungSongToan/CardEditorX.git";
        public About()
        {
            InitializeComponent();
            exeFilePath = Assembly.GetExecutingAssembly().Location;
            DataContext = UIConfigViewModel.Instance;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string CreatorPath = "pack://application:,,,/Images/Creator.png";
                if (Uri.IsWellFormedUriString(CreatorPath, UriKind.Absolute))
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage(new Uri(CreatorPath));
                        Creator.ImageSource = bitmap;
                    }
                    catch (Exception)
                    {
                        Creator.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/Logo.ico"));
                    }
                }


                string imagePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(exeFilePath), @"data\CardData\Images\Logo.ico");
                if (System.IO.File.Exists(imagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgLogo.Source = bitmap;
                }
                else
                {
                    imgLogo.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Logo.png"));
                }
            }
            catch
            {
                imgLogo.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Logo.png"));
            }
        }

        #region Button
        private void blsetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void btnGithub_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = GithubURL,
                UseShellExecute = true
            });
        }
        private void btnok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
