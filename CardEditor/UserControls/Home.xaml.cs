using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Commands;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl, INotifyPropertyChanged
    {
        #region Variable
        private CancellationTokenSource _resizeCts;
        #endregion

        #region Property
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

        private string _welcome = string.Empty;
        public string Welcome
        {
            get => _welcome;
            set
            {
                if (_welcome != value)
                {
                    _welcome = value;
                    OnPropertyChanged(nameof(Welcome));
                }
            }
        }
        #endregion

        #region Commands
        public RelayCommand GetStartedCommand { get; set; }
        #endregion

        #region Constructor
        public Home()
        {
            InitializeComponent();

            this.DataContext = this;
            GridTop.AddHandler(UIElement.MouseWheelEvent,
                new MouseWheelEventHandler(GridTop_MouseWheel), true);
        }
        public Home(IMainWindowService service) : this()
        {
            MainWindowService = service;
        }
        #endregion

        #region Load
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _resizeCts?.Cancel();
            _resizeCts = null;
        }
        public void LoadConfig()
        {
            BuildHomeInterface();
            RebuildWindowTitle();
        }
        #endregion

        #region Scroll
        private void BuildHomeInterface()
        {
            Dispatcher.BeginInvoke(new Action(UpdateGridRowHeights),
                System.Windows.Threading.DispatcherPriority.Loaded);
        }
        private void UpdateGridRowHeights()
        {
            double gridRootHeight = GridRoot.ActualHeight;
            if (gridRootHeight <= 0) return;

            bool hasDataSource =
                !string.IsNullOrWhiteSpace(ConfigViewModel.Instance.userSetting.DataSource) &&
                System.IO.Directory.Exists(ConfigViewModel.Instance.userSetting.DataSource);

            if (!hasDataSource)
            {
                Welcome = "Welcome back to";
                GridTopRow.Height = new GridLength(240, GridUnitType.Pixel);
            }
            else
            {
                Welcome = "Welcome to";
                GridTopRow.Height = new GridLength(420, GridUnitType.Pixel);
            }
            GridRoot.UpdateLayout();
        }
        private async Task UpdateGridTopVisibilityAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(100, token);

                token.ThrowIfCancellationRequested();

                double height = GridTop.ActualHeight;

                GrGetStarted.Visibility =
                    height > 360 ? Visibility.Visible : Visibility.Collapsed;

                GrWelcome.Visibility =
                    height > 240 ? Visibility.Visible : Visibility.Collapsed;

                GrTitle.Visibility =
                    height > 200 ? Visibility.Visible : Visibility.Collapsed;

                GrLogo.Visibility =
                    height > 160 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (OperationCanceledException)
            {
                ///
            }
        }
        private void GridTop_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (CanInnerControlScroll(e.OriginalSource as DependencyObject, e.Delta))
                return;

            double current = GridTopRow.ActualHeight;
            double next = current + (e.Delta > 0 ? 20 : -20);

            next = Math.Max(10, Math.Min(420, next));
            GridTopRow.Height = new GridLength(next, GridUnitType.Pixel);

            e.Handled = true;
        }
        private bool CanInnerControlScroll(DependencyObject source, int delta)
        {
            while (source != null && source != GridRoot)
            {
                if (source is ScrollViewer sv)
                {
                    // delta > 0: cuộn lên (wheel up) -> còn scroll lên được không
                    if (delta > 0 && sv.VerticalOffset > 0) return true;
                    // delta < 0: cuộn xuống (wheel down) -> còn scroll xuống được không
                    if (delta < 0 && sv.VerticalOffset < sv.ScrollableHeight) return true;

                    return false; // đã chạm biên, trả quyền cho GridRoot
                }
                source = VisualTreeHelper.GetParent(source);
            }
            return false; // không nằm trong ScrollViewer nào
        }
        #endregion

        #region Main Window Title
        private void RebuildWindowTitle()
        {
            AppTitle WindowTitle = new AppTitle
            {
                DisplayTitle = CMess.Home.ToText(),
                DisplayTabItemHeader = CMess.Home.ToText(),
                PhysicalFullPath = string.Empty
            };
            MainWindowTitle = WindowTitle;
        }
        private void UpdateWindowSavedFlag()
        {
            if (MainWindowService != null)
            {
                MainWindowService.UpdateWindowSavedFlag(IsSaved);
            }
        }
        #endregion

        #region Event
        private void FeatureCard_ItemOpened(object sender, RecentItem item)
        {
            switch (item.FileType)
            {
                case RecentFileType.Archive: MainWindowService.OpenArchiveCommand(item.Url); break;
                case RecentFileType.Database: MainWindowService.OpenDatabaseCommand(item.Url); break;
                case RecentFileType.Script: MainWindowService.OpenScriptCommand(item.Url); break;
                case RecentFileType.Deck: MainWindowService.OpenDeckCommand(item.Url); break;
                case RecentFileType.BanList: MainWindowService.OpenBanListCommand(item.Url); break;
                default: return; break;
            }
        }
        private void FeatureCard_CardTypeSelected(object sender, CardEditor.Enums.RecentFileType e)
        {
            switch (e)
            {
                case RecentFileType.Archive: MainWindowService.OpenArchive(); break;
                case RecentFileType.Database: MainWindowService.OpenDataEditor(); break;
                case RecentFileType.Script: MainWindowService.OpenCodeEditor(); break;
                case RecentFileType.Deck: MainWindowService.OpenDeckEditor(); break;
                case RecentFileType.BanList: MainWindowService.OpenBanListEditor(); break;
                default: return; break;
            }
        }
        private async void GridTop_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _resizeCts?.Cancel();
            var cts = new CancellationTokenSource();
            _resizeCts = cts;

            try
            {
                await UpdateGridTopVisibilityAsync(cts.Token);
            }
            finally
            {
                if (ReferenceEquals(_resizeCts, cts))
                    _resizeCts = null;

                cts.Dispose();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
