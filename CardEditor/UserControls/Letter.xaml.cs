using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using CardEditor.ViewModels;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for Letter.xaml
    /// </summary>
    public partial class Letter : UserControl
    {
        private Storyboard _rotateStoryboard;
        private bool _isPaused = false;

        public Letter()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!HomeImageViewModel.Instance.IsLoadedLogo)
            {
                var (resultLoad, messageLoad) = await HomeImageViewModel.Instance.LoadLogo();
                if (!resultLoad) return;
            }

            ImgLogoCircle.Source = HomeImageViewModel.Instance.LogoCircle;
            ImgLogoX.Source = HomeImageViewModel.Instance.LogoX;

            _rotateStoryboard = (Storyboard)RotatingGrid.Resources["RotateStoryboard"];
            _rotateStoryboard.Begin(RotatingGrid, true); // true = isControllable
        }
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_rotateStoryboard != null)
            {
                _rotateStoryboard.Stop(RotatingGrid);
                _rotateStoryboard.Remove(RotatingGrid);
            }
        }

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_rotateStoryboard != null && !_isPaused)
            {
                _rotateStoryboard.Pause(RotatingGrid);
                _isPaused = true;
            }
        }
        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_rotateStoryboard != null && _isPaused)
            {
                _rotateStoryboard.Resume(RotatingGrid);
                _isPaused = false;
            }
        }
    }
}
