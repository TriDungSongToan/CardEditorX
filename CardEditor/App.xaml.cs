using System;
using System.IO;
using System.IO.Pipes;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Diagnostics;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using LibGit2Sharp;
using OfficeOpenXml;
using CardEditor.Models;
using CardEditor.Manager;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CardEditor.ImageGene;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly string DataFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "data");
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ExcelPackage.License.SetNonCommercialOrganization("CardEditorX");
            var result = await CheckDataFolder();
            await LoadSettings();

            // LanguageManager.Initialize();
            MainWindow mainWindow;
            if (e.Args.Length > 0)
            {
                mainWindow = new MainWindow(e.Args);
            }
            else
            {
                mainWindow = new MainWindow();
            }
            // MainWindow = mainWindow;
            mainWindow.Show();
            Application.Current.MainWindow = mainWindow;
            UpdateApplicationIcon();
            if (result) ShowCmdNotification();
        }
        
        public App()
        {
            // PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
            // PresentationTraceSources.DataBindingSource.Listeners.Add(new BindingTraceListener());
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
        protected override void OnExit(ExitEventArgs e)
        {
            Cleanup();
            base.OnExit(e);
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Cleanup();
        }
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
            UIConfigViewModel.Instance.Dispose();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
