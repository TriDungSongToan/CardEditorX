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
using MaterialDesignThemes.Wpf;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for GenesysEditor.xaml
    /// </summary>
    public partial class GenesysEditor : Window
    {
        private GenesysViewModel genesysViewModel;
        private SnackbarMessageQueue MessageNotifi;

        public GenesysEditor()
        {
            InitializeComponent();
            genesysViewModel = GenesysViewModel.CreateInstance();
            DataContext = genesysViewModel;

            MessageNotifi = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
            NotifiSnackbar.MessageQueue = MessageNotifi;
        }

        #region Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();

            genesysViewModel.MessageBoxRequested += GenesysViewModel_MessageBoxRequested;
            genesysViewModel.SnackbarRequested += GenesysViewModel_SnackbarRequested;
            genesysViewModel.ImageViewerRequested += GenesysViewModel_ImageViewerRequested;
            genesysViewModel.OpenFileRequested += GenesysViewModel_OpenFileRequested;
        }
        private void GenesysViewModel_MessageBoxRequested(object sender, Models.MessageBoxRequest e)
        {
            int result = CMSG.Show(e.Title, e.IconType, e.Message, e.Buttons);
            if (e.ResponseSource != null)
            {
                e.ResponseSource.TrySetResult(result);
            }
        }
        private void GenesysViewModel_SnackbarRequested(object sender, string message)
        {
            MessageNotifi.Enqueue(message);
        }
        private void GenesysViewModel_ImageViewerRequested(object sender, string imagePath)
        {
            var viewerService = new ImageViewerService();
            viewerService.ShowImage(imagePath);
        }
        private void GenesysViewModel_OpenFileRequested(object sender, string imagePath)
        {
            var fileExplorerService = new FileExplorerService();
            fileExplorerService.ShowFileInExplorer(imagePath);
        }
        private void LoadConfig()
        {
            //int toolSort = ConfigViewModel.Instance.Arrange;
            string sortType = string.Empty;
            //switch (toolSort)
            //{
            //    case 1:
            //        {
            //            sortType = CMess.id_asc.ToText();
            //            iconSort.Kind = MaterialDesignThemes.Wpf.PackIconKind.SortNumericAscending;
            //        }
            //        break;
            //    case 2:
            //        {
            //            sortType = CMess.id_desc.ToText();
            //            iconSort.Kind = MaterialDesignThemes.Wpf.PackIconKind.SortNumericDescending;
            //        }
            //        break;
            //    case 3:
            //        {
            //            sortType = CMess.name_asc.ToText();
            //            iconSort.Kind = MaterialDesignThemes.Wpf.PackIconKind.SortAlphabeticalAscending;
            //        }
            //        break;

            //    case 4:
            //        {
            //            sortType = CMess.name_desc.ToText();
            //            iconSort.Kind = MaterialDesignThemes.Wpf.PackIconKind.SortAlphabeticalDescending;
            //        }
            //        break;
            //    default:
            //        {
            //            sortType = CMess.id_asc.ToText();
            //            iconSort.Kind = MaterialDesignThemes.Wpf.PackIconKind.SortNumericAscending;
            //        }
            //        break;
            //}
            //btnSortCard.ToolTip = $"{CMess.toolSort.ToText()} {sortType}\n{CMess.clearSort.ToText()}";
        }
        #endregion

        #region Title bar
        private void blGenesysManager_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            datagrGenesysCard.ItemsSource = null;

            genesysViewModel.MessageBoxRequested -= GenesysViewModel_MessageBoxRequested;
            genesysViewModel.SnackbarRequested -= GenesysViewModel_SnackbarRequested;
            genesysViewModel.ImageViewerRequested -= GenesysViewModel_ImageViewerRequested;
            genesysViewModel.OpenFileRequested -= GenesysViewModel_OpenFileRequested;

            DataContext = null;
            genesysViewModel?.Dispose();
            genesysViewModel=null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        #endregion

    }
}
