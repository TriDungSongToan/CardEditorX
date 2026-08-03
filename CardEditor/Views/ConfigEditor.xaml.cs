using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Security.Cryptography;
using MaterialDesignThemes.Wpf;
using CardEditor.Models;
using CardEditor.Manager;
using CardEditor.Helpers;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Localization;
using TextBox = System.Windows.Controls.TextBox;
using CMess = CardEditor.Localization.Language;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for ConfigEditor.xaml
    /// </summary>
    public partial class ConfigEditor : Window
    {
        #region Variable
        public MainWindow MainWindowReference { get; set; }
        private SettingViewModel settingViewModel;

        private readonly string CedsFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CardEditor", "Ceds.dat");
        private readonly string KeyPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CardEditor", "Ceds.key");
        private readonly string IVPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CardEditor", "Ceds.iv");
        private bool gridrowsave = false;
        public event Action ConfigChanged;
        #endregion

        public ConfigEditor()
        {
            InitializeComponent();
            this.Opacity = 0;

            settingViewModel = SettingViewModel.CreateInstance();
            DataContext = settingViewModel;
            
            LoadDev();
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
        private void LoadDev()
        {
            string directorySecPath = System.IO.Path.GetDirectoryName(CedsFilePath);
            if (!Directory.Exists(directorySecPath))
            {
                Directory.CreateDirectory(directorySecPath);
            }
            if (!File.Exists(CedsFilePath))
            {
                if (!File.Exists(KeyPath) || !File.Exists(IVPath))
                {
                    tbtndeveloper.IsChecked = false;
                    tbtndeveloper.IsEnabled = false;
                    btnIconReset.IsEnabled = false;
                    grdev.ToolTip = "You do not have permission to use this mode.";
                    return;
                }

                string defaultP = SecretManager.GetSecret(KeyPath, IVPath);
                if (!string.IsNullOrEmpty(defaultP))
                    CreateInitial(defaultP);
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    "Decryption error. Invalid key file.", new[] { CMess.ok.ToText() });
            }
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
                MainDialogHost.Visibility = Visibility.Collapsed;
                ResetPassDiaLog.Visibility = Visibility.Collapsed;
                btnIconReset.Visibility = Visibility.Collapsed;
            }
            else
            {
                gridrowsave = true;
                grDev.Height = Double.NaN;
                MainDialogHost.Visibility = Visibility.Visible;
                ResetPassDiaLog.Visibility = Visibility.Visible;
                btnIconReset.Visibility = Visibility.Visible;
            }
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

        #region Button
        private void btncacels_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void tbtnShowPass_Checked(object sender, RoutedEventArgs e)
        {
            txtpassword.Visibility = Visibility.Collapsed;
            txtPlainPassword.Visibility = Visibility.Visible;

            txtPlainPassword.Text = txtpassword.Password;
            txtPlainPassword.Focus();
            txtPlainPassword.UpdateLayout();
            txtPlainPassword.CaretIndex = txtPlainPassword.Text.Length;
        }
        private void tbtnShowPass_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPlainPassword.Visibility = Visibility.Collapsed;
            txtpassword.Visibility = Visibility.Visible;
            txtpassword.Focus();
            txtpassword.Password = txtPlainPassword.Text;
            txtpassword.UpdateLayout();
        }
        private void txtpassword_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ValidatePassword();
            }
        }
        private void txtPlainPassword_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ValidatePassword();
            }
        }
        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            ValidatePassword();
        }
        private void btnAcceptReset_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = txtoldpassword.Text;
            string newPassword = txtnewpassword.Text;
            string confirmPassword = txtconfirmpassword.Text;

            // Ẩn các thông báo lỗi
            IncorrectPassword.Visibility = Visibility.Collapsed;
            UnMatchPassword.Visibility = Visibility.Collapsed;

            // Kiểm tra mật khẩu cũ
            if (!VerifyPassword(oldPassword))
            {
                IncorrectPassword.Visibility = Visibility.Visible;
                return;
            }

            // Kiểm tra mật khẩu mới và xác nhận có giống nhau không
            if (newPassword != confirmPassword)
            {
                UnMatchPassword.Visibility = Visibility.Visible;
                return;
            }

            // Mọi kiểm tra đều thành công, thay đổi mật khẩu
            // Tạo salt mới cho mật khẩu mới
            byte[] newSalt = GenerateRandomSalt();
            byte[] newPasswordHash = HashPassword(newPassword, newSalt);

            // Lưu salt và hash mới
            SavePasswordData(newSalt, newPasswordHash);
            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Information,
                "Password changed successfully!", new[] { CMess.ok.ToText() });

            // Xóa dữ liệu nhập để tăng tính bảo mật
            txtoldpassword.Text = string.Empty;
            txtnewpassword.Text = string.Empty;
            txtconfirmpassword.Text = string.Empty;

            DialogHost.CloseDialogCommand.Execute(true, ResetPassDiaLog);
        }
        private async void btnIconReset_Click(object sender, RoutedEventArgs e)
        {
            if (DialogHost.IsDialogOpen("ResetPass")) return;
            txtoldpassword.Text = string.Empty;
            txtnewpassword.Text = string.Empty;
            txtconfirmpassword.Text = string.Empty;

            object result = await ResetPassDiaLog.ShowDialog(ResetPassDiaLog.DialogContent);
        }
        private async void tbtndeveloper_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!tbtndeveloper.IsChecked.Value)
            {
                e.Handled = true;
                if (DialogHost.IsDialogOpen("MainDialog"))
                    return;

                txtpassword.Password = string.Empty;
                ErrorMessage.Visibility = Visibility.Collapsed;

                object result = await MainDialogHost.ShowDialog(MainDialogHost.DialogContent);
                if (result is bool dialogResult && dialogResult)
                {
                    tbtndeveloper.IsChecked = true;
                }
            }
        }

        #endregion

        #region Password
        private const int SaltSize = 16; // Kích thước salt - 16 bytes (128 bits)
        private const int Iterations = 10000; // Số vòng lặp cho PBKDF2
        private void CreateInitial(string password)
        {
            // Tạo salt ngẫu nhiên
            byte[] salt = GenerateRandomSalt();

            // Hash mật khẩu với salt
            byte[] passwordHash = HashPassword(password, salt);

            // Lưu salt và password hash
            SavePasswordData(salt, passwordHash);
        } // Phương thức tạo mật khẩu ban đầu
        private byte[] GenerateRandomSalt()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);
                return salt;
            }
        } // Tạo salt ngẫu nhiên
        private byte[] HashPassword(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(32); // 32 bytes = 256 bits
            }
        } // Hash mật khẩu sử dụng PBKDF2
        private void SavePasswordData(byte[] salt, byte[] passwordHash)
        {
            // Kết hợp salt và hash để lưu trữ
            byte[] dataToSave = new byte[salt.Length + passwordHash.Length];
            Array.Copy(salt, 0, dataToSave, 0, salt.Length);
            Array.Copy(passwordHash, 0, dataToSave, salt.Length, passwordHash.Length);

            // Lưu vào file (trong thực tế, nên lưu vào cơ sở dữ liệu an toàn)
            File.WriteAllBytes(CedsFilePath, dataToSave);
        } // Lưu salt và hash
        private (byte[] Salt, byte[] Hash) GetStoredPasswordData()
        {
            byte[] storedData = File.ReadAllBytes(CedsFilePath);

            byte[] salt = new byte[SaltSize];
            byte[] hash = new byte[storedData.Length - SaltSize];

            Array.Copy(storedData, 0, salt, 0, SaltSize);
            Array.Copy(storedData, SaltSize, hash, 0, storedData.Length - SaltSize);

            return (salt, hash);
        } // Đọc salt và hash từ storage
        private bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
            {
                diff |= (uint)(a[i] ^ b[i]);
            }
            return diff == 0;
        } // So sánh hai mảng byte cẩn thận để tránh timing attacks
        private bool VerifyPassword(string password)
        {
            try
            {
                var (salt, storedHash) = GetStoredPasswordData();

                // Tính toán hash của mật khẩu được nhập với salt đã lưu
                byte[] computedHash = HashPassword(password, salt);

                // So sánh byte-by-byte
                return SlowEquals(storedHash, computedHash);
            }
            catch
            {
                return false;
            }
        } // Kiểm tra mật khẩu
        private void ValidatePassword()
        {
            string enteredPassword = tbtnShowPass.IsChecked == true ? txtPlainPassword.Text : txtpassword.Password;

            if (VerifyPassword(enteredPassword))
            {
                // Xác thực thành công
                ErrorMessage.Visibility = Visibility.Collapsed;
                DialogHost.CloseDialogCommand.Execute(true, MainDialogHost);
            }
            else
            {
                // Xác thực thất bại
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }
        #endregion

    }
}
