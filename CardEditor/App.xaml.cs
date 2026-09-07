using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Generic;
using LibGit2Sharp;
using CardEditor.Manager;
using CardEditor.Services;
using CardEditor.ImageGene;
using CardEditor.ViewModels;
using CardEditor.Localization;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fielda
        private readonly string DataFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "data");
        private SingleInstanceManager _instanceManager;
        private readonly List<MainWindow> _openWindows = new List<MainWindow>();
        private MainWindow _activeWindow;
        #endregion

        #region Startup
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            UpgradeSettingsIfNeeded();
            EnsureDefaultPassword();

            _instanceManager = new SingleInstanceManager();
            _instanceManager.FileReceived += OnFileReceivedFromAnotherInstance;
            bool isFirstInstance = _instanceManager.TryStart(e.Args);
            if (!isFirstInstance)
            {
                // Đã forward file (nếu có) sang instance đang chạy, thoát ngay.
                Environment.Exit(0);
                return;
            }

            var result = await CheckDataFolder();
            await LoadSettings();

            // LanguageManager.Initialize();

            MainWindow mainWindow = e.Args.Length > 0 ? new MainWindow(e.Args) : new MainWindow();
            RegisterWindow(mainWindow);
            mainWindow.Show();
            Application.Current.MainWindow = mainWindow;

            UpdateApplicationIcon();
            if (result) ShowCmdNotification();
        }
        #endregion

        #region Constructor
        public App()
        {
            // PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
            // PresentationTraceSources.DataBindingSource.Listeners.Add(new BindingTraceListener());
        }
        #endregion

        #region Event
        private void OnFileReceivedFromAnotherInstance(string filePath)
        {
            // Callback này chạy trên background thread (từ Named Pipe), phải marshal vào UI thread.
            Dispatcher.Invoke(() =>
            {
                // Ưu tiên cửa sổ đang active; nếu vì lý do nào đó không có (đã đóng),
                // fallback sang cửa sổ đầu tiên còn tồn tại.
                var target = _activeWindow ?? _openWindows.FirstOrDefault();

                if (target == null)
                {
                    // Trường hợp hiếm: tất cả cửa sổ đã đóng nhưng process vẫn sống
                    // (chỉ xảy ra nếu bạn đổi ShutdownMode sang OnExplicitShutdown).
                    target = new MainWindow();
                    RegisterWindow(target);
                    target.Show();
                }

                if (target.WindowState == WindowState.Minimized)
                {
                    target.WindowState = WindowState.Normal;
                }
                target.Activate();
                target.Topmost = true;
                target.Topmost = false;
                target.Focus();

                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    target.HandleFileOpen(filePath);
                }
            });
        }
        protected override void OnExit(ExitEventArgs e)
        {
            Cleanup();
            base.OnExit(e);
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Cleanup();
        }
        #endregion

        #region Functions
        private void UpgradeSettingsIfNeeded()
        {
            if (CardEditor.Properties.Settings.Default.NeedsUpgrade)
            {
                CardEditor.Properties.Settings.Default.Upgrade();
                CardEditor.Properties.Settings.Default.NeedsUpgrade = false;
                CardEditor.Properties.Settings.Default.Save();
            }
        }
        private void EnsureDefaultPassword()
        {
            try
            {
                if (string.IsNullOrEmpty(CardEditor.Properties.Settings.Default.EncryptedPasswordHash))
                {
                    const string defaultHash = "yky1YY/H2TRQRFDHiPuK9A==.XlAjVXO4OmV7rIpqhg0wT9t+PVU7ELwx80SnErOkNlI=";

                    CardEditor.Properties.Settings.Default.EncryptedPasswordHash =
                        SettingsEncryption.EncryptString(defaultHash);
                    CardEditor.Properties.Settings.Default.Save();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error when setting the default password: {ex.Message}");
                MessageBox.Show("Unable to initialize security configuration. DEV Mode may not work.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        internal void RegisterWindow(MainWindow window)
        {
            _openWindows.Add(window);
            _activeWindow = window; // cửa sổ vừa tạo mặc định coi là active

            window.Activated += (s, e) => _activeWindow = window;
            window.Closed += (s, e) =>
            {
                _openWindows.Remove(window);
                if (ReferenceEquals(_activeWindow, window))
                {
                    // Cửa sổ active vừa đóng -> chuyển active sang cửa sổ còn lại gần nhất
                    _activeWindow = _openWindows.LastOrDefault();
                }
            };
        }

        private async Task LoadSettings()
        {
            try
            {
                ConfigViewModel.Instance.ReadSettingsPropertiesCommand();
                if (!Directory.Exists(CardEditor.Models.AppContext.Instance.ConfigFolderPath))
                {
                    Directory.CreateDirectory(CardEditor.Models.AppContext.Instance.ConfigFolderPath);
                    Thread.Sleep(100);
                }

                var (resultSetting, errorSetting) = ConfigViewModel.Instance.LoadSettingFIle();
                if (resultSetting) LoadLanguage();
                EnumsViewModel.Instance.InitializeEnumsList();
                var (resultSort, messageSort) = await SortsViewModel.Instance.ReadSortingFileCommand();
                if (!resultSetting) MessageBox.Show($"An Error when Read Setting File: {errorSetting}", "Error");
                if (!resultSort) MessageBox.Show($"An Error when Read Sorting File: {messageSort}", "Error");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void LoadLanguage()
        {
            #if DEBUG
            LanguageManager.ValidateLocalizationEnum();
            #endif
            string currentLanguage = ConfigViewModel.Instance.userSetting.Language;
            LanguageManager.Initialize();
            LanguageManager.Instance.LoadLanguage(currentLanguage);
        }

        private async Task<bool> CheckDataFolder()
        {
            try
            {
                if (!Directory.Exists(DataFolder))
                {
                    Directory.CreateDirectory(DataFolder);
                }
                string CardDatapath = Path.Combine(DataFolder, "CardData");
                if (!Directory.Exists(CardDatapath) || !Repository.IsValid(CardDatapath))
                {
                    string CardDataURL = ConfigurationManager.AppSettings["CardDataURL"];
                    var (hasUpdateCardData, message) = await GitHubService.CheckForUpdatesAsync(CardDatapath, CardDataURL);

                    return hasUpdateCardData;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking repository: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        private void ShowCmdNotification()
        {
            string line0 = "Notification";
            string line1 = "The Most Malicious Malware Has Been Downloaded Compelete!";
            string line2 = "Installing VIRUS.exe...🐧";
            try
            {
                string commandArgs = $"echo {line0} && echo {line1} && echo {line2}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {commandArgs} && pause",
                    CreateNoWindow = false,
                    UseShellExecute = false
                });
            }
            catch
            {
                MessageBox.Show($"{line1}\n{line2}", $"{line0}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void UpdateApplicationIcon()
        {
            try
            {
                string iconPath = Path.Combine(DataFolder, @"CardData\Images\Logo.ico");
                if (File.Exists(iconPath))
                {
                    using (var stream = new FileStream(iconPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var memoryStream = new MemoryStream();
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        var icon = BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        memoryStream.Close();

                        if (Application.Current.MainWindow != null)
                            Application.Current.MainWindow.Icon = icon;
                    }
                }
                else
                {
                    var defaultIconUri = new Uri("pack://application:,,,/Images/Logo.ico");
                    var defaultIcon = BitmapFrame.Create(defaultIconUri);
                    if (Application.Current.MainWindow != null)
                    {
                        Application.Current.MainWindow.Icon = defaultIcon;
                    }
                }
            }
            catch
            {

            }
        }

        private bool _cleaned = false;
        private void Cleanup()
        {
            if (_cleaned) return;
            _cleaned = true;
            BanListRawDataViewModel.Instance.Dispose();
            CardEXDataViewModel.Instance.Dispose();
            CardImageCacheViewModel.Instance.Dispose();
            ChatBotViewModel.Instance.Dispose();
            ConfigViewModel.Instance.Dispose();
            DeckViewModel.Instance.Dispose();
            EnumsViewModel.Instance.Dispose();
            GeneraImageViewModel.Instance.Dispose();
            ImageGenerator.Dispose();
            GenesysRawDataViewModel.Instance.Dispose();
            KonamiIDViewModel.Instance.Dispose();
            RareRawDataViewModel.Instance.Dispose();
            ScriptViewModel.Instance.Dispose();
            SortsViewModel.Instance.Dispose();
            CardDataViewModel.Instance.Dispose();
            PenLanguageViewModel.Instance.Dispose();
            MasterDuelAPIViewModel.Instance.Dispose();
            CreditsViewModel.Instance.Dispose();
            UIConfigViewModel.Instance.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        #endregion
    }
}
