using System;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using ConfigViewModel = CardEditor.ViewModels.ConfigViewModel;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for ChatConfig.xaml
    /// </summary>
    public partial class ChatConfig : Window
    {
        public ChatConfig()
        {
            InitializeComponent();
            DataContext = UIConfigViewModel.Instance;
        }

        #region Load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeContentMenu();
            LoadSetting();
        }
        private void InitializeContentMenu()
        {
            ControlContextMenuService.Attach(txtUserName);
            ControlContextMenuService.Attach(txtApiEndpoint);
            ControlContextMenuService.Attach(txtApiKey);
        }
        private void LoadSetting()
        {
            txtUserName.Text = ConfigViewModel.Instance.userSetting.UserName;
            txtApiEndpoint.Text = ConfigViewModel.Instance.userSetting.ApiEndpoint;
            txtApiKey.Text = ConfigViewModel.Instance.userSetting.ApiKey;
        }
        #endregion

        #region Save
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CheckForm())
            {
                try
                {
                    ConfigViewModel.Instance.userSetting.UserName = txtUserName.Text;
                    ConfigViewModel.Instance.userSetting.ApiEndpoint = txtApiEndpoint.Text;
                    ConfigViewModel.Instance.userSetting.ApiKey = txtApiKey.Text;

                    var (result, message) = ConfigViewModel.Instance.SaveSingleSettingFile("UserSetting", ConfigViewModel.Instance.userSetting);
                    if (result)
                    {
                        await Task.Delay(100);
                        ChatBotViewModel.Instance.LoadChatConfig();
                        this.Close();
                    }
                    else throw new Exception(message);
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.Config.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
        }
        private bool CheckForm()
        {
            try
            {
                if(string.IsNullOrWhiteSpace(txtUserName.Text))
                    throw new ArgumentNullException($"{CMess.UserName.ToText()} {CMess.cannotEmpty.ToText()}");
                if (string.IsNullOrWhiteSpace(txtApiEndpoint.Text))
                    throw new ArgumentNullException($"{CMess.UserName.ToText()} {CMess.ApiEndpoint.ToText()}");
                if (string.IsNullOrWhiteSpace(txtApiKey.Text))
                    throw new ArgumentNullException($"{CMess.UserName.ToText()} {CMess.ApiKey.ToText()}");
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Button
        private void blChatSetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

    }
}
