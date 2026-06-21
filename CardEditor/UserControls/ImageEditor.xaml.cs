using System;
using System.Windows;
using CardEditor.ViewModels;
using System.Windows.Controls;
using CardEditor.Helpers;
using System.IO;
using CardEditor.Localization;
using CardEditor.Services;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for ImageEditor.xaml
    /// </summary>
    public partial class ImageEditor : UserControl
    {
        private IMainWindowService MainWindowService;
        public bool IsSaved { get; set; } = true;
        private string _currentUrl = string.Empty;
        public ImageEditor()
        {
            InitializeComponent();
            DataContext = UIConfigViewModel.Instance;
        }
        public ImageEditor(IMainWindowService service) : this()
        {
            MainWindowService = service;
        }
        private void RootGrid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var env = await WebViewEnvironment.GetAsync();
            await WebViewControl.EnsureCoreWebView2Async(env);
            WebViewControl.CoreWebView2.DownloadStarting += CoreWebView2_DownloadStarting;

            LoadConfig();
        }

        private void CoreWebView2_DownloadStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2DownloadStartingEventArgs e)
        {
            string downloadFolder = ConfigViewModel.Instance.imageSetting.DownloadedCardFolder;
            if (string.IsNullOrEmpty(downloadFolder) || !Directory.Exists(downloadFolder))
            {
                string folderDiaLog = FileDiaLogHelper.OpenFolder(CardEditor.Localization.Language.DownloadFolder.ToText());
                if (string.IsNullOrEmpty(folderDiaLog)) return;
                ConfigViewModel.Instance.imageSetting.DownloadedCardFolder = folderDiaLog;
                downloadFolder = folderDiaLog;
            }

            string fileName = Path.GetFileName(e.ResultFilePath);
            e.ResultFilePath = Path.Combine(downloadFolder, fileName);
            e.Handled = true;
        }

        public void LoadConfig()
        {
            string linkWeb = ConfigViewModel.Instance.imageSetting.SelectedCardMaker.Link;
            if (string.IsNullOrWhiteSpace(linkWeb)) return;

            if (_currentUrl != linkWeb)
            {
                _currentUrl = linkWeb;
                WebViewControl.Source = new Uri(_currentUrl);
            }

            if (MainWindowService != null)
            {
                MainWindowService.UpdateTabItemHeader(ConfigViewModel.Instance.imageSetting.SelectedCardMaker.DisplayName);
            }
        }
    }
}
