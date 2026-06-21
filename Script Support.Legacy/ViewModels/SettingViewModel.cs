using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ScriptSupport.Legacy.Models;
using ScriptSupport.Legacy.Commands;
using ScriptSupport.Legacy.Localization;
using CMess = ScriptSupport.Legacy.Localization.Language;
using ScriptAppContent = ScriptSupport.Legacy.Models.AppContext;

namespace ScriptSupport.Legacy.ViewModels
{
    public class SettingViewModel : INotifyPropertyChanged, IDisposable
    {
        public static SettingViewModel CreateInstance() => new SettingViewModel();

        private static readonly string _sizePattern = @"^\d+,\d+$";

        #region Propertys
        public ScriptSupport.Legacy.Models.Settings.UserSettingSource userSettingSource { get; set; }
        public ScriptSupport.Legacy.Models.Settings.UserSetting userSetting { get; set; }
        public ScriptSupport.Legacy.Models.Settings.DisplaySettingSource displaySettingSource { get; set; }
        public ScriptSupport.Legacy.Models.Settings.DisplaySetting displaySetting { get; set; }
        public ScriptSupport.Legacy.Models.Settings.DataHandlingSetting dataHandlingSetting { get; set; }
        public ScriptSupport.Legacy.Models.Settings.CodeEditSetting codeEditSetting { get; set; }
        #endregion

        #region Command
        public RelayCommand BrowseDataSourceCommand { get; private set; }
        public RelayCommand BrowseWebPathCommand { get; private set; }
        public RelayCommand ResetSettingCommand { get; private set; }
        public RelayCommand ReloadSettingCommand { get; private set; }
        public RelayCommand SaveSettingCommand { get; private set; }
        #endregion

        public SettingViewModel()
        {
            InitializeCommands();
            InitializeSettingSource();
            if (LoadSettingSource())
            {
                LoadSetting();
            }
        }
        private void InitializeCommands()
        {
            BrowseDataSourceCommand = new ScriptSupport.Legacy.Commands.RelayCommand(_ => BrowseDataSourcePath());
            BrowseWebPathCommand = new ScriptSupport.Legacy.Commands.RelayCommand(_ => BrowseWebPath());
            ResetSettingCommand = new ScriptSupport.Legacy.Commands.RelayCommand(async _ => await ResetSetting());
            ReloadSettingCommand = new ScriptSupport.Legacy.Commands.RelayCommand(async _ => await ReloadSetting());
            SaveSettingCommand = new ScriptSupport.Legacy.Commands.RelayCommand(async _ => await SaveSetting(), _ => CanSaveSettingCommand());
        }
        private void InitializeSettingSource()
        {
            userSettingSource = new Models.Settings.UserSettingSource();
            userSetting = new Models.Settings.UserSetting();
            displaySettingSource = new Models.Settings.DisplaySettingSource();
            displaySetting = new Models.Settings.DisplaySetting();
            dataHandlingSetting = new Models.Settings.DataHandlingSetting();
            codeEditSetting = new Models.Settings.CodeEditSetting();
        }

        private bool LoadSettingSource()
        {
            try
            {
                string LanguagePath = System.IO.Path.Combine(ScriptAppContent.Instance.DataFolderPath, @"CardData\Language");
                try
                {
                    if (Directory.Exists(LanguagePath))
                    {
                        var folders = Directory.GetDirectories(LanguagePath).Select(d => new DirectoryInfo(d).Name).Where(name => name != ".git").ToList();
                        userSettingSource.Languages.AddRange(folders);
                    }
                }
                catch
                {
                    userSettingSource.Languages.AddRange(new List<string> { "English" });
                }

                string gamePath = System.IO.Path.Combine(ScriptAppContent.Instance.DataFolderPath, @"CardData\Game\Game.txt");
                try
                {
                    if (File.Exists(gamePath))
                    {
                        string[] lines = File.ReadAllLines(gamePath);
                        userSettingSource.Games.AddRange(lines);
                    }
                }
                catch
                {
                    userSettingSource.Games.AddRange(new List<string> { "EDOPro" });
                }

                List<string> itemstheme = new List<string> { "Amber", "Blue", "BlueGrey", "Brown", "Cyan", "DeepOrange", "DeepPurple", "Green", "Grey", "Indigo", "LightBlue", "LightGreen", "Lime", "Orange", "Pink", "Purple", "Red", "Teal", "Yellow" };
                displaySettingSource.Themes.AddRange(itemstheme);

                var fontList = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
                displaySettingSource.FontFamilys.AddRange(fontList);

                string highLightFolder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ScriptAppContent.Instance.ExeFilePath), "HighLight");
                try
                {
                    if (Directory.Exists(highLightFolder))
                    {
                        string[] xshdFiles = Directory.GetFiles(highLightFolder, "*.xshd")
                            .Select(f => Path.GetFileNameWithoutExtension(f)).ToArray();
                        if (xshdFiles.Length > 0) displaySettingSource.HighLights.AddRange((IEnumerable<string>)xshdFiles);
                        else displaySettingSource.HighLights.AddRange(new List<string> { "Default" });
                    }
                }
                catch
                {
                    displaySettingSource.HighLights.AddRange(new List<string> { "Default" });
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        private void LoadSetting()
        {
            userSetting = ConfigViewModel.Instance.userSetting.Clone();
            displaySetting = ConfigViewModel.Instance.displaySetting.Clone();
            dataHandlingSetting = ConfigViewModel.Instance.dataHandlingSetting.Clone();
            codeEditSetting = ConfigViewModel.Instance.codeEditSetting.Clone();
        }

        private void BrowseDataSourcePath()
        {
            var handler = RequestOpenBrowseDialog;

            if (handler != null)
            {
                var result = handler.Invoke(CMess.DataSource.ToText());
                bool ok = result.Item1;
                string message = result.Item2;

                if (ok) userSetting.DataSource = message;
            }
        }
        private void BrowseWebPath()
        {
            var handler = RequestOpenBrowseDialog;

            if (handler != null)
            {
                var result = handler.Invoke(CMess.DataSource.ToText());
                bool ok = result.Item1;
                string message = result.Item2;

                if (ok) userSetting.BrowserPath = message;
            }
        }

        private async Task ResetSetting()
        {
            var requestReset = new MessageBoxRequest
            {
                Title = CMess.questi.ToText(),
                IconType = CMSG.MessageBoxIconType.Question,
                Message = CMess.confirmResetSetting.ToText(),
                Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                ResponseSource = new TaskCompletionSource<int>()
            };
            OnMessageBoxRequested(requestReset);
            int result = await requestReset.ResponseTask;
            if (result != 0) return;

            ConfigViewModel.Instance.ResetSettingProperties();
            var (resultFile, message) = ConfigViewModel.Instance.ResetSettingFile();
            if (resultFile)
            {
                var notifiRestart = new MessageBoxRequest
                {
                    Title = CMess.notifi.ToText(),
                    IconType = CMSG.MessageBoxIconType.Notification,
                    Message = CMess.settingReset.ToText(),
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = new TaskCompletionSource<int>()
                };
                OnMessageBoxRequested(notifiRestart);
                int resultRestart = await notifiRestart.ResponseTask;
                if (resultRestart == 0)
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }
            else
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{CMess.errorOcc.ToText()} {message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        private async Task ReloadSetting()
        {
            var requestReset = new MessageBoxRequest
            {
                Title = CMess.questi.ToText(),
                IconType = CMSG.MessageBoxIconType.Question,
                Message = string.Format(CMess.confirmReload.ToText(), CMess.Setting.ToText()),
                Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                ResponseSource = new TaskCompletionSource<int>()
            };
            OnMessageBoxRequested(requestReset);
            int result = await requestReset.ResponseTask;
            if (result != 0) return;

            try
            {
                LoadSetting();
            }
            catch { }
        }
        private async Task SaveSetting()
        {
            try
            {
                ConfigViewModel.Instance.userSetting = userSetting.Clone();
                ConfigViewModel.Instance.displaySetting = displaySetting.Clone();
                ConfigViewModel.Instance.dataHandlingSetting = dataHandlingSetting.Clone();
                ConfigViewModel.Instance.codeEditSetting = codeEditSetting.Clone();

                ConfigViewModel.Instance.UpdateSettingsProperties();
                ConfigViewModel.Instance.SaveSettingsProperties();

                //ConfigViewModel.Instance.UpdateSettingsFile();
                var (resultSetting, messageSetting) = ConfigViewModel.Instance.SaveAllSettingFile();

                if (resultSetting)
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.notifi.ToText(),
                        IconType = CMSG.MessageBoxIconType.Notification,
                        Message = CMess.saveSettingSuc.ToText(),
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(request);

                    CallConfigChanged?.Invoke();
                }
                else throw new IOException(messageSetting);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{CMess.errorOcc.ToText()} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        private bool CanSaveSettingCommand()
        {
            if (string.IsNullOrWhiteSpace(userSetting.DataSource) ||
                userSetting.DataSource.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0 ||
                !System.IO.Path.IsPathRooted(userSetting.DataSource) ||
                !Directory.Exists(userSetting.DataSource) ||
                !HasReadWritePermission(userSetting.DataSource)) return false;

            if (string.IsNullOrEmpty(userSetting.Language) ||
                string.IsNullOrWhiteSpace(userSetting.Game))
                return false;

            if (string.IsNullOrWhiteSpace(displaySetting.Background) ||
                string.IsNullOrWhiteSpace(displaySetting.Foreground) ||
                displaySetting.SelectedBackground == null ||
                displaySetting.SelectedForeground == null)
                return false;

            if (string.IsNullOrEmpty(displaySetting.Theme) ||
                string.IsNullOrEmpty(displaySetting.HighLight) ||
                displaySetting.FontFamily == null ||
                displaySetting.FontSize == null || displaySetting.FontSize.Value <= 0 || displaySetting.FontSize.Value >= 100 ||
                displaySetting.FlowDirectionC < 0 ||
                displaySetting.TextAlignmentC < 0)
                return false;

            return true;
        }
        private bool HasReadWritePermission(string folderPath)
        {
            string tempFilePath = System.IO.Path.Combine(folderPath, System.IO.Path.GetRandomFileName());
            try
            {
                using (var stream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write))
                {
                    stream.WriteByte(0x0);
                }
                using (var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                {
                    int b = stream.ReadByte();
                }
                File.Delete(tempFilePath);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region Event
        public event Action CallConfigChanged;
        public event Func<string, (bool, string)> RequestOpenBrowseDialog;
        public event EventHandler<MessageBoxRequest> MessageBoxRequested;
        //public event EventHandler<OpenFolderDialogEventArgs> OpenFolderDialogRequested;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnMessageBoxRequested(MessageBoxRequest request)
        {
            MessageBoxRequested?.Invoke(this, request);
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            BrowseDataSourceCommand = null;
            BrowseWebPathCommand = null;
            ResetSettingCommand = null;
            ReloadSettingCommand = null;
            SaveSettingCommand = null;

            userSettingSource = null;
            userSetting = null;
            displaySettingSource = null;
            displaySetting = null;
            dataHandlingSetting = null;
            codeEditSetting = null;

            CallConfigChanged = null;
            RequestOpenBrowseDialog = null;
            MessageBoxRequested = null;
            PropertyChanged = null;
        }
        #endregion

    }
}