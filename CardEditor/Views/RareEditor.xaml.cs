using System;
using System.IO;
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
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MaterialDesignThemes.Wpf;
using CardEditor.Models;
using CardEditor.Manager;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for RareEditor.xaml
    /// </summary>
    public partial class RareEditor : Window
    {
        private RareViewModel rareViewModel;
        private SnackbarMessageQueue MessageNotifi;
        // private IImageViewerService ImageViewerService;
        public RareEditor()
        {
            InitializeComponent();
            rareViewModel = RareViewModel.CreateInstance();
            DataContext = rareViewModel;
            
            MessageNotifi = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
            NotifiSnackbar.MessageQueue = MessageNotifi;
        }

        #region Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();

            rareViewModel.LoadDataCallFromView();

            txtHeader.Text = CMess.RarityCardManager.ToText();

            rareViewModel.MessageBoxRequested += RareViewModel_MessageBoxRequested;
            rareViewModel.SnackbarRequested += RareViewModel_SnackbarRequested;
            rareViewModel.ImageViewerRequested += RareViewModel_ImageViewerRequested;
            rareViewModel.OpenFileRequested += RareViewModel_OpenFileRequested;
            rareViewModel.OnSettingSaved += LoadConfig;
        }

        private void RareViewModel_OnSettingSaved()
        {
            throw new NotImplementedException();
        }

        private void LoadConfig()
        {
            // int toolSort = SettingService.GetIntSetting("Arrange", 0);
            //int toolSort = CardEditor.ViewModels.ConfigViewModel.Instance.Arrange;
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
        private void ProgressBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            rareViewModel.ProgressBarWidth = e.NewSize.Width;
        }
        private void RareViewModel_MessageBoxRequested(object sender, Models.MessageBoxRequest e)
        {
            int result = CMSG.Show(e.Title, e.IconType, e.Message, e.Buttons);
            if (e.ResponseSource != null)
            {
                e.ResponseSource.TrySetResult(result);
            }
        }
        private void RareViewModel_SnackbarRequested(object sender, string message)
        {
            MessageNotifi.Enqueue(message);
        }
        private void RareViewModel_ImageViewerRequested(object sender, string imagePath)
        {
            var viewerService = new ImageViewerService();
            viewerService.ShowImage(imagePath);
        }
        private void RareViewModel_OpenFileRequested(object sender, string imagePath)
        {
            var fileExplorerService = new FileExplorerService();
            fileExplorerService.ShowFileInExplorer(imagePath);
        }
        #endregion

        #region Title bar
        private void blRarityManager_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnRareList_Checked(object sender, RoutedEventArgs e)
        {
            if (RareManager.IsLeftDrawerOpen == true)
            {
                RareManager.IsLeftDrawerOpen = false;
                btnRareFolder.IsChecked = false;
            }

            RareManager.IsRightDrawerOpen = true;
            txtHeader.Text = CMess.RarityListManager.ToText();
        }
        private void btnRareList_Unchecked(object sender, RoutedEventArgs e)
        {
            RareManager.IsRightDrawerOpen = false;
            txtHeader.Text = CMess.RarityCardManager.ToText();
        }
        private void btnRareFolder_Checked(object sender, RoutedEventArgs e)
        {
            if (RareManager.IsRightDrawerOpen == true)
            {
                RareManager.IsRightDrawerOpen = false;
                btnRareList.IsChecked = false;
            }
            RareManager.IsLeftDrawerOpen = true;
            txtHeader.Text = CMess.RarityCardManager.ToText();
        }
        private void btnRareFolder_Unchecked(object sender, RoutedEventArgs e)
        {
            RareManager.IsLeftDrawerOpen = false;
            txtHeader.Text = CMess.RarityCardManager.ToText();
        }
        #endregion

        //private void datagrRareCard_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var selectedItems = datagrRareCard?.SelectedItems.Cast<CardEditor.Models.RareCard>().ToList();
        //    RareViewModel.Instance.SelectedRareCards = new ObservableCollection<CardEditor.Models.RareCard>(selectedItems);
        //}

        private void btnCreateImagetest_Click(object sender, RoutedEventArgs e)
        {
            //RareViewModel.Instance.ProgressHeight = 65;
            //RareViewModel.Instance.ProgressMaximum = int.Parse(txtCardName.Text);
            //RareViewModel.Instance.ProgressValue = int.Parse(txtCardId.Text);
            //txtCardId.Text = progressBar.ActualWidth.ToString();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            datagrRareCard.ItemsSource = null;

            rareViewModel.MessageBoxRequested -= RareViewModel_MessageBoxRequested;
            rareViewModel.SnackbarRequested -= RareViewModel_SnackbarRequested;
            rareViewModel.ImageViewerRequested -= RareViewModel_ImageViewerRequested;
            rareViewModel.OpenFileRequested -= RareViewModel_OpenFileRequested;
            rareViewModel.OnSettingSaved -= LoadConfig;

            DataContext = null;
            rareViewModel?.Dispose();
            rareViewModel = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
