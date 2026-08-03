using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.ComponentModel;
using CardEditor.Services;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl, INotifyPropertyChanged
    {
        private IMainWindowService MainWindowService;
        private string _mainWindowTitle = string.Empty;
        public string MainWindowTitle
        {
            get => _mainWindowTitle;
            set
            {
                if (_mainWindowTitle != value)
                {
                    _mainWindowTitle = value;
                    OnPropertyChanged(nameof(MainWindowTitle));
                    UpdateWindowTitle();
                }
            }
        }

        public bool IsSaved { get; set; } = true;
        public Home()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public Home(IMainWindowService service) : this()
        {
            MainWindowService = service;
        }

        public void LoadConfig()
        {
            MainWindowTitle = CMess.Home.ToText();
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

            MainWindowTitle = CMess.Home.ToText();
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateWindowTitle()
        {
            if (MainWindowService != null)
            {
                MainWindowService.UpdateWindowTitle(MainWindowTitle);
                MainWindowService.UpdateTabItemHeader(MainWindowTitle);
            }
        }
        private void UpdateWindowSavedFlag()
        {
            if (MainWindowService != null)
            {
                MainWindowService.UpdateWindowSavedFlag(IsSaved);
            }
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
