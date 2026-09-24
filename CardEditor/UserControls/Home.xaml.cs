using System.Windows;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using CardEditor.Models;
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
        private AppTitle _mainWindowTitle = new();
        public AppTitle MainWindowTitle
        {
            get => _mainWindowTitle;
            set
            {
                if (_mainWindowTitle != value)
                {
                    _mainWindowTitle = value;
                    OnPropertyChanged(nameof(MainWindowTitle));

                    if (MainWindowService == null) return;
                    MainWindowService.UpdateWindowTitle(MainWindowTitle);
                    MainWindowService.UpdateTabItemHeader(MainWindowTitle.DisplayTabItemHeader);
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
        }

        public void LoadConfig()
        {
            RebuildWindowTitle();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void RebuildWindowTitle()
        {
            AppTitle WindowTitle = new AppTitle
            {
                DisplayTitle = CMess.Home.ToText(),
                DisplayTabItemHeader = CMess.Home.ToText(),
                PhysicalFullPath = string.Empty
            };
            MainWindowTitle = MainWindowTitle;
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
