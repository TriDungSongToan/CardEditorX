using System.IO;
using System.Windows.Media;
using System.ComponentModel;
using ScriptSupport.Models;
using ScriptSupport.Stores;
using ScriptSupport.States;
using ScriptSupport.Commands;
using ScriptSupport.Interfaces;
using ScriptSupport.Environment;
using ScriptSupport.Localization;
using CMess = ScriptSupport.Localization.Language;

namespace ScriptSupport.ViewModels
{
    public class SettingViewModel : BaseViewModel, IDisposable
    {
        #region Fields
        public UIConfigState UIConfig { get; }
        private readonly AppEnvironment _appEnvironment;
        private readonly ConfigStore _configStore;
        private readonly IConfigInterface _configInterface;
        private readonly IDialogInterface _dialogService;
        #endregion

        #region Propertys
        public ScriptSupport.Models.Settings.UserSettingSource userSettingSource { get; set; } = new();
        public ScriptSupport.Models.Settings.UserSetting userSetting { get; set; } = new();
        public ScriptSupport.Models.Settings.DisplaySettingSource displaySettingSource { get; set; } = new();
        public ScriptSupport.Models.Settings.DisplaySetting displaySetting { get; set; } = new();
        public ScriptSupport.Models.Settings.DataHandlingSetting dataHandlingSetting { get; set; } = new();
        public ScriptSupport.Models.Settings.FilterSetting filterSetting { get; set; } = new();
        public ScriptSupport.Models.Settings.CodeEditSetting codeEditSetting { get; set; } = new();
        #endregion

        #region Command
        public RelayCommand? BrowseDataSourceCommand { get; private set; }
        public RelayCommand? BrowseWebPathCommand { get; private set; }
        public RelayCommand? ResetSettingCommand { get; private set; }
        public RelayCommand? ReloadSettingCommand { get; private set; }
        public RelayCommand? SaveSettingCommand { get; private set; }
        #endregion

        #region Constructor
        public SettingViewModel(UIConfigState uIConfig, AppEnvironment appEnvironment, ConfigStore configStore,
            IConfigInterface configInterface, IDialogInterface dialogService)
        {
            UIConfig = uIConfig;
            _appEnvironment = appEnvironment;
            _configStore = configStore;
            _configInterface = configInterface;
            _dialogService = dialogService;

            InitializeEvent();
            InitializeCommands();
            InitializeSettingSource();
            if (LoadSettingSource())
            {
                LoadSetting();
            }
        }
        private void InitializeEvent()
        {
            if (userSetting != null) userSetting.PropertyChanged += Setting_PropertyChanged;
            if (displaySetting != null) displaySetting.PropertyChanged += Setting_PropertyChanged;
            if (dataHandlingSetting != null) dataHandlingSetting.PropertyChanged += Setting_PropertyChanged;
            if (filterSetting != null) filterSetting.PropertyChanged += Setting_PropertyChanged;
            if (codeEditSetting != null) codeEditSetting.PropertyChanged += Setting_PropertyChanged;
        }
        private void UnsubEvent()
        {
            if(userSetting != null) userSetting.PropertyChanged -= Setting_PropertyChanged;
            if(displaySetting != null) displaySetting.PropertyChanged -= Setting_PropertyChanged;
            if(dataHandlingSetting != null) dataHandlingSetting.PropertyChanged -= Setting_PropertyChanged;
            if(filterSetting != null) filterSetting.PropertyChanged -= Setting_PropertyChanged;
            if(codeEditSetting != null) codeEditSetting.PropertyChanged -= Setting_PropertyChanged;
        }
        private void Setting_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SaveSettingCommand?.RaiseCanExecuteChanged();
        }

        private void InitializeCommands()
        {
            BrowseDataSourceCommand = new ScriptSupport.Commands.RelayCommand(_ => BrowseDataSourcePath());
            BrowseWebPathCommand = new ScriptSupport.Commands.RelayCommand(_ => BrowseWebPath());
            ResetSettingCommand = new ScriptSupport.Commands.RelayCommand(async _ => await ResetSetting());
            ReloadSettingCommand = new ScriptSupport.Commands.RelayCommand(async _ => await ReloadSetting());
            SaveSettingCommand = new ScriptSupport.Commands.RelayCommand(async _ => await SaveSetting(), _ => CanSaveSettingCommand());
        }
        private void InitializeSettingSource()
        {
            userSettingSource = new Models.Settings.UserSettingSource();
            userSetting = new Models.Settings.UserSetting();
            displaySettingSource = new Models.Settings.DisplaySettingSource();
            displaySetting = new Models.Settings.DisplaySetting();
            filterSetting = new Models.Settings.FilterSetting();
            dataHandlingSetting = new Models.Settings.DataHandlingSetting();
            codeEditSetting = new Models.Settings.CodeEditSetting();
        }
        private bool LoadSettingSource()
        {
            try
            {
                #region User Settings
                string LanguagePath = System.IO.Path.Combine(_appEnvironment.DataFolderPath, @"CardData\Language");
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

                string gamePath = System.IO.Path.Combine(_appEnvironment.DataFolderPath, @"CardData\Game\Game.txt");
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
                #endregion

                #region Display Settings
                List<string> itemstheme = new List<string> { "Amber", "Blue", "BlueGrey", "Brown", "Cyan", "DeepOrange", "DeepPurple", "Green", "Grey", "Indigo", "LightBlue", "LightGreen", "Lime", "Orange", "Pink", "Purple", "Red", "Teal", "Yellow" };
                displaySettingSource.Themes.AddRange(itemstheme);

                var fontList = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
                displaySettingSource.FontFamilys.AddRange(fontList);

                string highLightFolder = _appEnvironment.HighLightFolderPath;
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
                #endregion

                return true;
            }
            catch
            {
                return false;
            }
        }
        private void LoadSetting()
        {
            UnsubEvent();
            userSetting = _configStore.UserSetting.Clone() ?? new Models.Settings.UserSetting();
            displaySetting = _configStore.DisplaySetting.Clone() ?? new Models.Settings.DisplaySetting();
            dataHandlingSetting = _configStore.DataHandlingSetting.Clone() ?? new Models.Settings.DataHandlingSetting();
            filterSetting = _configStore.FilterSetting.Clone() ?? new Models.Settings.FilterSetting();
            codeEditSetting = _configStore.CodeEditSetting.Clone() ?? new Models.Settings.CodeEditSetting();
            InitializeEvent();
        }
        #endregion

        #region Command
        private void BrowseDataSourcePath()
        {
            string path = _dialogService.OpenFolder(CMess.DataSource.ToText());
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            userSetting.DataSource = path;
        }
        private void BrowseWebPath()
        {
            string path = _dialogService.OpenFolder(CMess.browserPath.ToText());
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            userSetting.BrowserPath = path;
        }
        private async Task ResetSetting()
        {
            var resetQuest = new MessageBoxRequest
            {
                Title = CMess.questi.ToText(),
                IconType = MessageBoxIconType.Question,
                Message = CMess.confirmResetSetting.ToText(),
                Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                DefaultButtonIndex = 1,
                ResponseSource = new TaskCompletionSource<int>()
            };
            int resultQuest = await _dialogService.ShowMessage(resetQuest);
            if (resultQuest != 0) return;

            var (resultReset, messageReset) = await _configInterface.ResetConfigAsync();
            if (resultReset)
            {
                var notifiRestart = new MessageBoxRequest
                {
                    Title = CMess.notifi.ToText(),
                    IconType = MessageBoxIconType.Notification,
                    Message = CMess.settingReset.ToText(),
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = new TaskCompletionSource<int>()
                };
                int resultNotifi = await _dialogService.ShowMessage(notifiRestart);
                if (resultNotifi == 0) System.Windows.Application.Current.Shutdown();
            }
            else
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = MessageBoxIconType.Error,
                    Message = $"{CMess.errorOcc.ToText()} {messageReset}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                await _dialogService.ShowMessage(request);
            }
        }
        private async Task ReloadSetting()
        {
            var requestReset  = new MessageBoxRequest
            {
                Title = CMess.questi.ToText(),
                IconType = MessageBoxIconType.Question,
                Message = string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.tlReload.ToText(), CMess.Setting.ToText()),
                Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                DefaultButtonIndex = 1,
                ResponseSource = new TaskCompletionSource<int>()
            };
            int userChoice = await _dialogService.ShowMessage(requestReset);
            if (userChoice != 0) return;

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
                _configStore.UserSetting = userSetting.Clone() ?? new Models.Settings.UserSetting();
                _configStore.DisplaySetting = displaySetting.Clone() ?? new Models.Settings.DisplaySetting();
                _configStore.FilterSetting = filterSetting.Clone() ?? new Models.Settings.FilterSetting();
                _configStore.DataHandlingSetting = dataHandlingSetting.Clone() ?? new Models.Settings.DataHandlingSetting();
                _configStore.CodeEditSetting = codeEditSetting.Clone() ?? new Models.Settings.CodeEditSetting();

                var (resultSave, messageSave) = await _configInterface.SaveConfigAsync();
                if (!resultSave) throw new IOException(messageSave);
                var (resultApply, messageApply) = await _configInterface.ApplyConfigAsync();
                if (resultSave && resultApply)
                {
                    await _dialogService.ShowMessage(new MessageBoxRequest
                    {
                        Title = CMess.notifi.ToText(),
                        IconType = MessageBoxIconType.Notification,
                        Message = string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Save.ToText(), CMess.Setting.ToText()),
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    });
                }
                else throw new IOException(messageApply);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowMessage(new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = MessageBoxIconType.Error,
                    Message = $"{CMess.errorOcc.ToText()} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                });
            }
        }

        private bool CanSaveSettingCommand()
        {
            if (string.IsNullOrWhiteSpace(userSetting.DataSource) ||
                userSetting.DataSource.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0 ||
                !System.IO.Path.IsPathRooted(userSetting.DataSource) ||
                !Directory.Exists(userSetting.DataSource)) return false;

            if (string.IsNullOrEmpty(userSetting.Language) ||
                string.IsNullOrWhiteSpace(userSetting.Game))
                return false;

            if (string.IsNullOrWhiteSpace(displaySetting.Background) ||
                string.IsNullOrWhiteSpace(displaySetting.Foreground))
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
        #endregion

        #region IDisposable
        public void Dispose()
        {
            UnsubEvent();
            BrowseDataSourceCommand = null;
            BrowseWebPathCommand = null;
            ResetSettingCommand = null;
            ReloadSettingCommand = null;
            SaveSettingCommand = null;
        }
        #endregion
    }
}