using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Threading.Tasks;
using System.Configuration;
using ScriptSupport.Legacy.Helper;
using ScriptSupport.Legacy.Commands;
using ScriptSupport.Legacy.ViewModels;
using ScriptSupport.Legacy.Localization;
using CMess = ScriptSupport.Legacy.Localization.Language;

namespace ScriptSupport.Legacy
{
    /// <summary>
    /// Interaction logic for ConfigEditor.xaml
    /// </summary>
    public partial class ConfigEditor : Window
    {
        #region Variable
        public MainWindow MainWindowReference { get; set; }
        private SettingViewModel settingViewModel;
        public event Action ConfigChanged;
        #endregion

        #region Command
        public RelayCommand CancelCommand;
        #endregion

        #region Constructor
        public ConfigEditor()
        {
            InitializeCommand();
            InitializeComponent();
            this.Opacity = 0;
            settingViewModel = SettingViewModel.CreateInstance();
            DataContext = settingViewModel;

            settingViewModel.CallConfigChanged += SettingViewModel_CallConfigChanged;
            settingViewModel.RequestOpenBrowseDialog += SettingViewModel_RequestOpenBrowseDialog;
            settingViewModel.MessageBoxRequested += SettingViewModel_MessageBoxRequested;
        }
        private void InitializeCommand()
        {
            CancelCommand = new ScriptSupport.Legacy.Commands.RelayCommand(_ => Cancel());
        }
        #endregion

        #region Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Opacity = 1;
        }
        #endregion

        #region Event
        private void SettingViewModel_CallConfigChanged()
        {
            ConfigChanged?.Invoke();
        }
        private (bool, string) SettingViewModel_RequestOpenBrowseDialog(string title)
        {
            try
            {
                string folderPath = FileDiaLogHelper.OpenFolder(title);

                if (!string.IsNullOrEmpty(folderPath))
                {
                    return (true, folderPath);
                }
                else throw new Exception();
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        private void SettingViewModel_MessageBoxRequested(object sender, Models.MessageBoxRequest e)
        {
            int result = CMSG.Show(e.Title, e.IconType, e.Message, e.Buttons);
            if (e.ResponseSource != null)
            {
                e.ResponseSource.TrySetResult(result);
            }
        }

        private void blsetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void Cancel()
        {
            this.Close();
        }
        #endregion

        #region Closing
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settingViewModel != null)
            {
                settingViewModel.CallConfigChanged -= SettingViewModel_CallConfigChanged;
                settingViewModel.RequestOpenBrowseDialog -= SettingViewModel_RequestOpenBrowseDialog;
                settingViewModel.MessageBoxRequested -= SettingViewModel_MessageBoxRequested;
                settingViewModel.Dispose();
                settingViewModel = null;
            }
            DataContext = null;
            settingViewModel = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        #endregion

    }
}
