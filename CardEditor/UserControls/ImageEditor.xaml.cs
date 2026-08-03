using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using CardEditor.Helpers;
using CardEditor.Services;
using CardEditor.Localization;
using CardEditor.ViewModels;
using CMess = CardEditor.Localization.Language;
using System.Diagnostics;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for ImageEditor.xaml
    /// </summary>
    public partial class ImageEditor : UserControl, INotifyPropertyChanged
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
        private string _currentUrl = string.Empty;
        public ImageEditor()
        {
            InitializeComponent();
            this.DataContext = this;
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
            WebViewControl.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;

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
        private void CoreWebView2_DocumentTitleChanged(object sender, object e)
        {
            MainWindowTitle = string.IsNullOrWhiteSpace(WebViewControl.CoreWebView2.DocumentTitle)
                ? CMess.ImageEdit.ToText()
                : WebViewControl.CoreWebView2.DocumentTitle;
            Debug.WriteLine($"Document Title Changed: {WebViewControl.CoreWebView2.DocumentTitle}");
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

        private void UpdateWindowTitle()
        {
            Debug.WriteLine($"ImageEditor.MainWindowTitle Changed: {MainWindowTitle}");
            if (MainWindowService == null) Debug.WriteLine($"MainWindowService Null");
            else Debug.WriteLine($"MainWindowService Not Null");

            if (MainWindowService != null)
            {
                string TabHeader = TrimStringHelper.ShortenTitle(MainWindowTitle, maxLength: 30);
                MainWindowService.UpdateWindowTitle(MainWindowTitle);
                MainWindowService.UpdateTabItemHeader(TabHeader);
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
