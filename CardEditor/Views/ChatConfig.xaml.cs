using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Manager;
using CardEditor.Services;
using CardEditor.ImageGene;
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
            LoadTheme();
        }

        private void LoadTheme()
        {
            string[] itemstheme = new string[] { "Amber", "Blue", "BlueGrey", "Brown", "Cyan", "DeepOrange", "DeepPurple", "Green", "Grey", "Indigo", "LightBlue", "LightGreen", "Lime", "Orange", "Pink", "Purple", "Red", "Teal", "Yellow" };
            string themeSet = ConfigViewModel.Instance.displaySetting.Theme;
            if (!Array.Exists(itemstheme, theme => theme.Equals(themeSet, StringComparison.OrdinalIgnoreCase)))
            {
                themeSet = "DeepPurple";
            }
            Uri themeUri = new Uri($"pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.{themeSet}.xaml", UriKind.Absolute);
            ResourceDictionary resourceDict = new ResourceDictionary();
            resourceDict.Source = themeUri;
            this.Resources.MergedDictionaries.Add(resourceDict);
        }

        #region Load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSetting();
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
