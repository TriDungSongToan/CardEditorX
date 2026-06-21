using System;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using System.ComponentModel;
namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl, INotifyPropertyChanged
    {
        public bool IsSaved { get; set; } = true;
        public Home()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string exeFilePath = Assembly.GetExecutingAssembly().Location;
            string dataFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(exeFilePath), "data");
            string imagePath = System.IO.Path.Combine(dataFolderPath, @"CardData\Images\MainLogo.png");
            BitmapImage bitmap = new BitmapImage();
            
            if (System.IO.File.Exists(imagePath))
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            else
            {
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("pack://application:,,,/CardEditor;component/Images/MainLogo.png", UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            bitmap.Freeze();
            imgmainbg.Source = bitmap;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        #region Event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
