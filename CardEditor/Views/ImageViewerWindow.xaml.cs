using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Diagnostics;
using System.Configuration;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for ImageViewerWindow.xaml
    /// </summary>
    public partial class ImageViewerWindow : Window
    {
        private string _imagePath;
        private bool isFullscreen = false;

        public ImageViewerWindow(string imagePath)
        {
            InitializeComponent();
            _imagePath = imagePath;
            DataContext = UIConfigViewModel.Instance;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadImageViewer();
        }

        public void LoadImageViewer()
        {
            imageViewer.LoadImage(_imagePath);
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            if(e.Key == Key.F)
            {
                FullScreen();
            }
        }

        #region Button
        private void Exit()
        {
            this.Close();
        }
        private void FullScreen()
        {
            if (isFullscreen)
            {
                tbtnFullscreen.IsChecked = false;
                WindowedMode();
            }
            else
            {
                tbtnFullscreen.IsChecked = true;
                FullscreenMode();
            }
        }
        private void FullscreenMode()
        {
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            isFullscreen = true;
        }
        private void WindowedMode()
        {
            this.WindowStyle = WindowStyle.None;
            this.WindowState = WindowState.Normal;
            this.ResizeMode = ResizeMode.CanResize;
            isFullscreen = false;
        }
        private void Web()
        {
            string url = ConfigurationManager.AppSettings["ygocarder"];
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ex.Message}",
                        new string[] { CMess.ok.ToText() });
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Exit();
        }
        private void tbtnFullscreen_Checked(object sender, RoutedEventArgs e)
        {
            FullscreenMode();
        }
        private void tbtnFullscreen_Unchecked(object sender, RoutedEventArgs e)
        {
            WindowedMode();
        }
        private void btnMore_Click(object sender, RoutedEventArgs e)
        {
            Web();
        }
        #endregion
    }
}
