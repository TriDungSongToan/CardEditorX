using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.ComponentModel;
using CardEditor.Models;
using CardEditor.Manager;
using CardEditor.Helpers;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using TextBox = System.Windows.Controls.TextBox;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for ConfigEditor.xaml
    /// </summary>
    public partial class ConfigEditor : Window, INotifyPropertyChanged
    {
        #region Variable
        public MainWindow MainWindowReference { get; set; }
        private SettingViewModel settingViewModel;
        private bool gridrowsave = false;

        private int _failedAttempts = 0;
        private DateTime _lockUntil = DateTime.MinValue;
        private const int MaxAttempts = 5;
        private static readonly TimeSpan LockDuration = TimeSpan.FromSeconds(30);
        private string DeveloperEncrypted;

        public event Action ConfigChanged;
        #endregion

        public ConfigEditor()
        {
            InitializeComponent();
            this.Opacity = 0;

            DeveloperEncrypted = ConfigViewModel.Instance.DeveloperEncrypted;

            settingViewModel = SettingViewModel.CreateInstance();
            DataContext = settingViewModel;
            grDev.DataContext = this;

            LoadDevModeState();
            settingViewModel.CallConfigChanged += SettingViewModel_CallConfigChanged;
            settingViewModel.RequestOpenBrowseDialog += SettingViewModel_RequestOpenBrowseDialog;
            settingViewModel.MessageBoxRequested += SettingViewModel_MessageBoxRequested;
        }

        #region Load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeContentMenu();
            this.Opacity = 1;
        }
        private void InitializeContentMenu()
        {
            ControlContextMenuService.Attach(txtUserName);
            ControlContextMenuService.Attach(txtdatasource);

            ControlContextMenuService.Attach(txtbackground);
            ControlContextMenuService.Attach(txtforeground);
            ControlContextMenuService.Attach(txtfontsize);

            ControlContextMenuService.Attach(txtArtWorkPath);
            ControlContextMenuService.Attach(txtOutPutPath);
            ControlContextMenuService.Attach(txtOriginalPath);
            ControlContextMenuService.Attach(txtDownloadPath);

            ControlContextMenuService.Attach(txtimgsize);
            ControlContextMenuService.Attach(txtstampsize);
            ControlContextMenuService.Attach(txtstampmargin);

            ControlContextMenuService.Attach(txtColumnRuler);
            ControlContextMenuService.Attach(txtIndentationSize);
            ControlContextMenuService.Attach(txtWordWrapIndentation);

            ControlContextMenuService.Attach(txtMaxMainDeck);
            ControlContextMenuService.Attach(txtMaxExtraDeck);
            ControlContextMenuService.Attach(txtMaxSideDeck);
        }
        private void LoadDevModeState()
        {

            DeveloperEncrypted = ConfigViewModel.Instance.DeveloperEncrypted;
            bool devModeUnlocked = SettingsEncryption.DecryptBoolSetting(
                ConfigViewModel.Instance.DeveloperEncrypted);
            tbtndeveloper.IsChecked = devModeUnlocked;
        }
        #endregion

        #region Event
        private void SettingViewModel_CallConfigChanged()
        {
            ConfigChanged?.Invoke();
        }
        private void SettingViewModel_MessageBoxRequested(object sender, MessageBoxRequest e)
        {
            int result = CMSG.Show(e.Title, e.IconType, e.Message, e.Buttons);
            if (e.ResponseSource != null)
            {
                e.ResponseSource.TrySetResult(result);
            }
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
        private void blsetting_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void txtSize_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!e.Text.All(c => char.IsDigit(c) || c == ','))
            {
                e.Handled = true;
            }
            if (e.Text == ",")
            {
                var textBox = sender as TextBox;
                if (textBox.SelectionStart == 0)
                {
                    e.Handled = true;
                    return;
                }
                if (textBox.Text.Contains(","))
                {
                    e.Handled = true;
                    return;
                }
            }
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gridrowsave)
            {
                gridrowsave = false;
                grDev.Height = 0;
                blLogin.Visibility = Visibility.Collapsed;
                blReSetPass.Visibility = Visibility.Collapsed;
            }
            else
            {
                gridrowsave = true;
                grDev.Height = Double.NaN;
                blLogin.Visibility = Visibility.Collapsed;
                blReSetPass.Visibility = Visibility.Collapsed;
            }
        }

        private void btnsave_Click(object sender, RoutedEventArgs e)
        {
            ConfigViewModel.Instance.DeveloperEncrypted = DeveloperEncrypted;
            ConfigViewModel.Instance.UpdateSettingsProperties();
            ConfigViewModel.Instance.SaveSettingsProperties();
        }
        private void btncacels_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (settingViewModel != null)
            {
                settingViewModel.CallConfigChanged -= SettingViewModel_CallConfigChanged;
                settingViewModel.RequestOpenBrowseDialog -= SettingViewModel_RequestOpenBrowseDialog;
                settingViewModel.MessageBoxRequested -= SettingViewModel_MessageBoxRequested;
                settingViewModel?.Dispose();
                settingViewModel = null;
            }
            DataContext = null;
            settingViewModel = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        #endregion

        #region List View
        private void ListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!(sender is System.Windows.Controls.ListView listView)) return;

            // Tìm ScrollViewer bên trong ListView
            var innerScrollViewer = FindVisualChild<ScrollViewer>(listView);
            if (innerScrollViewer == null) return;

            bool atTop = innerScrollViewer.VerticalOffset == 0;
            bool atBottom = innerScrollViewer.VerticalOffset >= innerScrollViewer.ScrollableHeight;

            if ((e.Delta > 0 && atTop) || (e.Delta < 0 && atBottom))
            {
                // Tìm ScrollViewer cha, bất kể sâu bao nhiêu lớp
                var parentScrollViewer = FindParentScrollViewer(listView);
                if (parentScrollViewer != null)
                {
                    parentScrollViewer.ScrollToVerticalOffset(parentScrollViewer.VerticalOffset - e.Delta);
                    e.Handled = true;
                }
            }
        }
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }
        private static ScrollViewer FindParentScrollViewer(DependencyObject child)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is ScrollViewer sv) return sv;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
        #endregion

        #region PassWord

        #region Propertys
        private string _passWord = string.Empty;
        public string PassWord
        {
            get => _passWord;
            set
            {
                if (_passWord != value)
                {
                    _passWord = value;
                    Debug.WriteLine($"[PassWord setter] '{value}'");
                    OnPropertyChanged(nameof(PassWord));
                }
            }
        }
        private string _oldPassWord = string.Empty;
        public string OldPassWord
        {
            get => _oldPassWord;
            set
            {
                if (_oldPassWord != value)
                {
                    _oldPassWord = value;
                    OnPropertyChanged(nameof(OldPassWord));
                }
            }
        }
        private string _newPassWord = string.Empty;
        public string NewPassWord
        {
            get => _newPassWord;
            set
            {
                if (_newPassWord != value)
                {
                    _newPassWord = value;
                    OnPropertyChanged(nameof(NewPassWord));
                }
            }
        }
        private string _confirmPassWord = string.Empty;
        public string ConfirmPassWord
        {
            get => _confirmPassWord;
            set
            {
                if (_confirmPassWord != value)
                {
                    _confirmPassWord = value;
                    OnPropertyChanged(nameof(ConfirmPassWord));
                }
            }
        }

        private string _loginResult = string.Empty;
        public string LoginResult
        {
            get => _loginResult;
            set
            {
                if (_loginResult != value)
                {
                    _loginResult = value;
                    OnPropertyChanged(nameof(LoginResult));
                }
            }
        }
        private string _oldPassWordResult = string.Empty;
        public string OldPassWordResult
        {
            get => _oldPassWordResult;
            set
            {
                if (_oldPassWordResult != value)
                {
                    _oldPassWordResult = value;
                    OnPropertyChanged(nameof(OldPassWordResult));
                }
            }
        }
        private string _newPassWordResult = string.Empty;
        public string NewPassWordResult
        {
            get => _newPassWordResult;
            set
            {
                if (_newPassWordResult != value)
                {
                    _newPassWordResult = value;
                    OnPropertyChanged(nameof(NewPassWordResult));
                }
            }
        }
        private string _confirmpasswordResult = string.Empty;
        public string ConfirmpasswordResult
        {
            get => _confirmpasswordResult;
            set
            {
                if (_confirmpasswordResult != value)
                {
                    _confirmpasswordResult = value;
                    OnPropertyChanged(nameof(ConfirmpasswordResult));
                }
            }
        }
        #endregion

        #region Events
        private void txtPassWordBox_EnterPressed(object sender, RoutedEventArgs e)
        {
            btnAccept_Click(sender, e);
        }
        private void txtOldPassWordBox_EnterPressed(object sender, RoutedEventArgs e)
        {
            btnAcceptReset_Click(sender, e);
        }
        private void txtNewPassWordBox_EnterPressed(object sender, RoutedEventArgs e)
        {
            btnAcceptReset_Click(sender, e);
        }
        private void txtConfirmPassWordBox_EnterPressed(object sender, RoutedEventArgs e)
        {
            btnAcceptReset_Click(sender, e);
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (DateTime.Now < _lockUntil)
            {
                int secondsLeft = (int)(_lockUntil - DateTime.Now).TotalSeconds;
                LoginResult = $"Đã nhập sai quá nhiều lần. Thử lại sau {secondsLeft}s.";
                return;
            }

            string hash = SettingsEncryption.DecryptString(
                CardEditor.Properties.Settings.Default.EncryptedPasswordHash);

            string enteredPassword = PassWord;

            if (hash != null && PasswordHasher.Verify(enteredPassword, hash))
            {
                _failedAttempts = 0;
                tbtndeveloper.IsChecked = true;
                SaveDevModeUnlocked(true);

                blLogin.Visibility = Visibility.Collapsed;
                grToggleButtonDev.IsEnabled = true;

                PassWord = string.Empty;
                LoginResult = string.Empty;
            }
            else
            {
                _failedAttempts++;
                if (_failedAttempts >= MaxAttempts)
                {
                    _lockUntil = DateTime.Now.Add(LockDuration);
                    _failedAttempts = 0;
                    LoginResult = $"Sai quá {MaxAttempts} lần. Khóa {LockDuration.TotalSeconds}s.";
                }
                else
                {
                    LoginResult = "Sai mật khẩu!";
                }
            }
        }
        private void btnAcceptReset_Click(object sender, RoutedEventArgs e)
        {
            OldPassWordResult = string.Empty;
            NewPassWordResult = string.Empty;
            ConfirmpasswordResult = string.Empty;

            string currentHash = SettingsEncryption.DecryptString(
                CardEditor.Properties.Settings.Default.EncryptedPasswordHash);

            if (currentHash == null || !PasswordHasher.Verify(OldPassWord, currentHash))
            {
                OldPassWordResult = "Old password is incorrect!";
                return;
            }

            if (NewPassWord != ConfirmPassWord)
            {
                ConfirmpasswordResult = "Password confirmation does not match";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassWord) || NewPassWord.Length < 4)
            {
                NewPassWordResult = "New password must be at least 4 characters long!";
                return;
            }

            string newHash = PasswordHasher.Hash(NewPassWord);
            CardEditor.Properties.Settings.Default.EncryptedPasswordHash = SettingsEncryption.EncryptString(newHash);
            CardEditor.Properties.Settings.Default.Save();

            OldPassWord = string.Empty;
            NewPassWord = string.Empty;
            ConfirmPassWord = string.Empty;

            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                "Password changed successfully!", new[] { CMess.ok.ToText() });

            grToggleButtonDev.IsEnabled = true;
            blReSetPass.Visibility = Visibility.Collapsed;
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            PassWord = string.Empty;
            LoginResult = string.Empty;
            blReSetPass.Visibility = Visibility.Collapsed;
            blLogin.Visibility = Visibility.Collapsed;
            grToggleButtonDev.IsEnabled = true;
        }
        private void btnCancelReset_Click(object sender, RoutedEventArgs e)
        {
            OldPassWord = string.Empty;
            NewPassWord = string.Empty;
            ConfirmPassWord = string.Empty;

            LoginResult = string.Empty;
            OldPassWordResult = string.Empty;
            NewPassWordResult = string.Empty;
            ConfirmpasswordResult = string.Empty;

            grToggleButtonDev.IsEnabled = true;
            blLogin.Visibility = Visibility.Collapsed;
            blReSetPass.Visibility = Visibility.Collapsed;
        }
        #endregion

        private void tbtndeveloper_Click(object sender, RoutedEventArgs e)
        {
            if (tbtndeveloper.IsChecked == true)
            {
                // Vừa được check -> luôn yêu cầu nhập lại password
                tbtndeveloper.IsChecked = false;
                grToggleButtonDev.IsEnabled = false;
                OpenLoginDialog();
            }
            else
            {
                SaveDevModeUnlocked(false);
            }
        }
        private async void btnIconReset_Click(object sender, RoutedEventArgs e)
        {
            OldPassWord = string.Empty;
            NewPassWord = string.Empty;
            ConfirmPassWord = string.Empty;

            LoginResult = string.Empty;
            OldPassWordResult = string.Empty;
            NewPassWordResult = string.Empty;
            ConfirmpasswordResult = string.Empty;

            grToggleButtonDev.IsEnabled = false;
            blLogin.Visibility = Visibility.Collapsed;
            blReSetPass.Visibility = Visibility.Visible;
        }
        private void OpenLoginDialog()
        {
            PassWord = string.Empty;
            LoginResult = string.Empty;
            blReSetPass.Visibility = Visibility.Collapsed;
            blLogin.Visibility = Visibility.Visible;
        }

        private void SaveDevModeUnlocked(bool unlocked)
        {
            DeveloperEncrypted = SettingsEncryption.EncryptBoolSetting(unlocked);
        }

        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
