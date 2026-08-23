using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CardEditor.Models;
using CardEditor.Models.Settings;
using CardEditor.Helpers;
using CardEditor.Commands;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using CardAppContext = CardEditor.Models.AppContext;

namespace CardEditor.ViewModels
{
    public class SettingViewModel : INotifyPropertyChanged, IDisposable
    {
        public static SettingViewModel CreateInstance() => new SettingViewModel();

        private static readonly string _sizePattern = @"^\d+,\d+$";

        #region Propertys
        public CardEditor.Models.Settings.UserSetting userSetting { get; set; }
        public CardEditor.Models.Settings.UserSettingSource userSettingSource { get; set; }
        public CardEditor.Models.Settings.DisplaySettingSource displaySettingSource { get; set; }
        public CardEditor.Models.Settings.DisplaySetting displaySetting { get; set; }
        public CardEditor.Models.Settings.DataHandlingSetting dataHandlingSetting { get; set; }
        public CardEditor.Models.Settings.ImageSetting imageSetting { get; set; }
        public CardEditor.Models.Settings.ImageSettingSource imageSettingSource { get; set; }
        public CardEditor.Models.Settings.CodeEditSetting codeEditSetting { get; set; }
        public CardEditor.Models.Settings.DeckEditSetting deckEditSetting { get; set; }

        public string ListModeString => deckEditSetting.ListMode
            ? CMess.GridCardMode.ToText()
            : CMess.ListCardMode.ToText();
        #endregion

        #region Dev
        //private bool _developer = false;
        //public bool Developer
        //{
        //    get => _developer;
        //    set
        //    {
        //        if(_developer != value)
        //        {
        //            _developer = value;
        //            OnPropertyChanged(nameof(Developer));
        //        }
        //    }
        //}
        #endregion

        #region Command

        #region Sorting
        public System.Windows.Input.ICommand AddSortItemCommand { get; private set; }
        public System.Windows.Input.ICommand RemoveSortItemCommand { get; private set; }
        #endregion

        public RelayCommand BrowseDataSource { get; private set; }
        public RelayCommand BrowseArtWork {  get; private set; }
        public RelayCommand BrowsePutPut {  get; private set; }
        public RelayCommand BrowseOriginal { get; private set; }
        public RelayCommand BrowseDownload { get; private set; }
        public RelayCommand ResetSetting { get; private set; }
        public RelayCommand ReloadSetting { get; private set; }
        public RelayCommand SaveSetting { get; private set; }
        public RelayCommand CancelSetting { get; private set; }
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
        private void UnSubscribeEvent()
        {
            userSetting.PropertyChanged -= UserSetting_PropertyChanged;
            displaySetting.PropertyChanged -= DisplaySetting_PropertyChanged;
            dataHandlingSetting.PropertyChanged -= DataHandlingSetting_PropertyChanged;
            imageSetting.PropertyChanged -= ImageSetting_PropertyChanged;
            codeEditSetting.PropertyChanged -= CodeEditSetting_PropertyChanged;
            deckEditSetting.PropertyChanged -= DeckEditSetting_PropertyChanged;
        }
        private void InitializeEvent()
        {
            userSetting.PropertyChanged += UserSetting_PropertyChanged;
            displaySetting.PropertyChanged += DisplaySetting_PropertyChanged;
            dataHandlingSetting.PropertyChanged += DataHandlingSetting_PropertyChanged;
            imageSetting.PropertyChanged += ImageSetting_PropertyChanged;
            codeEditSetting.PropertyChanged += CodeEditSetting_PropertyChanged;
            deckEditSetting.PropertyChanged += DeckEditSetting_PropertyChanged;
        }

        private void InitializeCommands()
        {
            AddSortItemCommand = new RelayCommand(AddSortItem);
            RemoveSortItemCommand = new RelayCommand(RemoveSortItem);

            BrowseDataSource = new CardEditor.Commands.RelayCommand(_ => BrowseDataSourcePath());
            BrowseArtWork = new CardEditor.Commands.RelayCommand(_ => BrowseArtWorkPath());
            BrowsePutPut = new CardEditor.Commands.RelayCommand(_ => BrowsePutPutPath());
            BrowseOriginal = new CardEditor.Commands.RelayCommand(_ => BrowseOriginalPath());
            BrowseDownload = new CardEditor.Commands.RelayCommand(_ => BrowseDownloadPath());

            ResetSetting = new CardEditor.Commands.RelayCommand(async _ => await ResetSettingCommand());
            ReloadSetting = new CardEditor.Commands.RelayCommand(_ => ReloadSettingCommand());
            SaveSetting = new CardEditor.Commands.RelayCommand(async _ => await SaveSettingCommand(), _ => CanSaveSettingCommand());
        }
        private void InitializeSettingSource()
        {
            userSetting = new Models.Settings.UserSetting();
            userSettingSource = new Models.Settings.UserSettingSource();
            displaySettingSource = new Models.Settings.DisplaySettingSource();
            displaySetting = new Models.Settings.DisplaySetting();
            dataHandlingSetting = new Models.Settings.DataHandlingSetting();
            imageSetting = new Models.Settings.ImageSetting();
            imageSettingSource = new Models.Settings.ImageSettingSource();
            codeEditSetting = new Models.Settings.CodeEditSetting();
            deckEditSetting = new Models.Settings.DeckEditSetting();
        }

        private bool LoadSettingSource()
        {
            try
            {
                string LanguagePath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, @"CardData\Language");
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

                string gamePath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, @"CardData\Game\Game.txt");
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

                string highLightFolder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(CardAppContext.Instance.ExeFilePath), "HighLight");
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

                var (resultSeries, messageSeries) = GeneraImageViewModel.Instance.LoadSeriesList();

                //List<string> Series = new List<string>();
                //string seriesFilePath = Path.Combine(CardAppContext.Instance.DataFolderPath, $@"CardImage\Series.txt");
                //if (File.Exists(seriesFilePath))
                //{
                //    using (StreamReader sr = new StreamReader(seriesFilePath))
                //    {
                //        string line;
                //        while ((line = sr.ReadLine()) != null)
                //        {
                //            Series.Add(line);
                //        }
                //    }    
                //}
                //else Series.Add("Series 10");
                //imageSettingSource.Series.AddRange(Series);

                List<ImageFormat> imageFormats = Enum.GetValues(typeof(ImageFormat)).Cast<ImageFormat>().ToList();
                imageSettingSource.ImageFormats.AddRange(imageFormats);

                string FoildFolderPath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath,
                    $@"CardImage\{ConfigViewModel.Instance.imageSetting.Series}\Foild");
                var listFoild = FindNameHelper.LoadNameArtList(FoildFolderPath);
                imageSettingSource.FoildLists.AddRange(listFoild);

                string backgroundArtFolderPath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath,
                    $@"CardImage\{ConfigViewModel.Instance.imageSetting.Series}\BackgroundArt");
                var listBGArt = FindNameHelper.LoadNameArtList(backgroundArtFolderPath);
                imageSettingSource.BackgroundArts.AddRange(listBGArt);

                List<CardMaker> cardMakers = new List<CardMaker>();
                string cardMakerPath = System.IO.Path.Combine(CardAppContext.Instance.DataFolderPath, @"CardData\CardMaker\CardMaker.txt");
                try
                {
                    if (File.Exists(cardMakerPath))
                    {
                        foreach (string cardMakerline in File.ReadLines(cardMakerPath))
                        {
                            if (string.IsNullOrWhiteSpace(cardMakerline)) continue;
                            string[] cardMakerParts = cardMakerline.Split('\t');

                            if (cardMakerParts.Length >= 2)
                            {
                                cardMakers.Add(new CardMaker
                                {
                                    DisplayName = cardMakerParts[0],
                                    Link = cardMakerParts[1]
                                });
                            }
                        }
                        imageSettingSource.CardMakers.AddRange(cardMakers);
                    }
                }
                catch { }

                return true;
            }
            catch
            {
                return false;
            }
        }
        private void LoadSetting()
        {
            UnSubscribeEvent();

            userSetting = ConfigViewModel.Instance.userSetting.Clone();
            displaySetting = ConfigViewModel.Instance.displaySetting.Clone();
            dataHandlingSetting = ConfigViewModel.Instance.dataHandlingSetting.Clone();
            imageSetting = ConfigViewModel.Instance.imageSetting.Clone();
            codeEditSetting = ConfigViewModel.Instance.codeEditSetting.Clone();
            deckEditSetting = ConfigViewModel.Instance.deckEditSetting.Clone();

            var selectedCardMaker = imageSettingSource.CardMakers
                .FirstOrDefault(cm => cm.DisplayName == imageSetting.SelectedCardMaker?.DisplayName);
            if (selectedCardMaker != null) imageSetting.SelectedCardMaker = selectedCardMaker;

            string encryptedDevelop = ConfigViewModel.Instance.DeveloperEncrypted;
            // Developer = SettingsEncryption.DecryptBoolSetting(encryptedDevelop);
            InitializeEvent();
        }

        private void AddSortItem(object parameter)
        {
            if (parameter is SortItem sortItem)
            {
                // Kiểm tra xem đã có item với Sort type này chưa
                var existingItem = SortsViewModel.Instance.SelectedSortItems
                    .FirstOrDefault(x => x.SelectedItem?.Sort == sortItem.Sort);

                // Nếu chưa có thì thêm mới
                if (existingItem == null)
                {
                    var newSelectedItem = new SelectedSortItem
                    {
                        SelectedItem = sortItem,
                        OrderByAsc = true // Mặc định sắp xếp tăng dần
                    };

                    SortsViewModel.Instance.SelectedSortItems.Add(newSelectedItem);
                }
            }
        }
        private void RemoveSortItem(object parameter)
        {
            if (parameter is SelectedSortItem selectedSortItem)
            {
                SortsViewModel.Instance.SelectedSortItems.Remove(selectedSortItem);
            }
        }

        private void BrowseDataSourcePath()
        {
            var handler = RequestOpenBrowseDialog;

            if (handler != null)
            {
                var result = handler.Invoke(CMess.DataSource.ToText());
                bool ok = result.Item1;
                string message = result.Item2;

                if (ok)
                {
                    if (!HasReadPermission(message))
                    {
                        var request = new MessageBoxRequest
                        {
                            Title = CMess.error.ToText(),
                            IconType = CMSG.MessageBoxIconType.Error,
                            Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.Permission.ToText()),
                            Buttons = new[] { CMess.ok.ToText() },
                            ResponseSource = null
                        };
                        OnMessageBoxRequested(request);
                        return;
                    }
                    userSetting.DataSource = message;
                }
            }
        }
        private void BrowseArtWorkPath()
        {
            var handler = RequestOpenBrowseDialog;

            if (handler != null)
            {
                var result = handler.Invoke(CMess.ArtWorkFolder.ToText());
                bool ok = result.Item1;
                string message = result.Item2;

                if (ok)
                {
                    if (!HasWritePermission(message))
                    {
                        var request = new MessageBoxRequest
                        {
                            Title = CMess.error.ToText(),
                            IconType = CMSG.MessageBoxIconType.Error,
                            Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.Permission.ToText()),
                            Buttons = new[] { CMess.ok.ToText() },
                            ResponseSource = null
                        };
                        OnMessageBoxRequested(request);
                        return;
                    }
                    imageSetting.ArtworkFolder = message;
                }
            }
        }
        private void BrowsePutPutPath()
        {
            var handler = RequestOpenBrowseDialog;

            if (handler != null)
            {
                var result = handler.Invoke(CMess.OutPutFolder.ToText());
                bool ok = result.Item1;
                string message = result.Item2;

                if (ok)
                {
                    if (!HasWritePermission(message))
                    {
                        var request = new MessageBoxRequest
                        {
                            Title = CMess.error.ToText(),
                            IconType = CMSG.MessageBoxIconType.Error,
                            Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.Permission.ToText()),
                            Buttons = new[] { CMess.ok.ToText() },
                            ResponseSource = null
                        };
                        OnMessageBoxRequested(request);
                        return;
                    }
                    imageSetting.OutPutFolder = message;
                }
            }
        }
        private void BrowseOriginalPath()
        {
            var handler = RequestOpenBrowseDialog;

            if (handler != null)
            {
                var result = handler.Invoke(CMess.OriginalFolder.ToText());
                bool ok = result.Item1;
                string message = result.Item2;

                if (ok) imageSetting.OriginalCardFolder = message;
            }
        }
        private void BrowseDownloadPath()
        {
            var handler = RequestOpenBrowseDialog;

            if (handler != null)
            {
                var result = handler.Invoke(CMess.DownloadFolder.ToText());
                bool ok = result.Item1;
                string message = result.Item2;

                if (ok) imageSetting.DownloadedCardFolder = message;
            }
        }

        private async Task ResetSettingCommand()
        {
            if (dataHandlingSetting.ConfirmReSet)
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
            }

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
        private async void ReloadSettingCommand()
        {
            if (dataHandlingSetting.ConfirmReLoad)
            {
                var requestReset = new MessageBoxRequest
                {
                    Title = CMess.questi.ToText(),
                    IconType = CMSG.MessageBoxIconType.Question,
                    Message = string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.tlReload.ToText(), CMess.Setting.ToText()),
                    Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                    ResponseSource = new TaskCompletionSource<int>()
                };
                OnMessageBoxRequested(requestReset);
                int result = await requestReset.ResponseTask;
                if (result != 0) return;
            }

            try
            {
                LoadSetting();
            }
            catch { }
        }
        private async Task SaveSettingCommand()
        {
            try
            {
                ConfigViewModel.Instance.userSetting = userSetting.Clone();
                ConfigViewModel.Instance.displaySetting = displaySetting.Clone();
                ConfigViewModel.Instance.dataHandlingSetting = dataHandlingSetting.Clone();
                ConfigViewModel.Instance.imageSetting = imageSetting.Clone();
                ConfigViewModel.Instance.codeEditSetting = codeEditSetting.Clone();
                ConfigViewModel.Instance.deckEditSetting = deckEditSetting.Clone();

                //#region Develop
                //string encryptedDevelop = SettingsEncryption.EncryptBoolSetting(Developer);
                //ConfigViewModel.Instance.DeveloperEncrypted = encryptedDevelop;
                //#endregion

                ConfigViewModel.Instance.UpdateSettingsProperties();
                ConfigViewModel.Instance.SaveSettingsProperties();

                //ConfigViewModel.Instance.UpdateSettingsFile();
                var (resultSetting, messageSetting) = ConfigViewModel.Instance.SaveAllSettingFile();
                var (resultSorting, messageSorting) = await SortsViewModel.Instance.SaveSortingFile();

                if (resultSetting && resultSorting)
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.notifi.ToText(),
                        IconType = CMSG.MessageBoxIconType.Notification,
                        Message = string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Save.ToText(), CMess.Setting.ToText()),
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(request);

                    CallConfigChanged?.Invoke();
                }
                else throw new IOException(messageSetting + messageSorting);

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
            Debug.WriteLine("CanSaveSettingCommand called");
            if (string.IsNullOrWhiteSpace(userSetting.UserName) ||
                string.IsNullOrEmpty(userSetting.Language) ||
                string.IsNullOrWhiteSpace(userSetting.Game))
                return false;

            if (string.IsNullOrWhiteSpace(userSetting.DataSource) ||
                userSetting.DataSource.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0 ||
                !System.IO.Path.IsPathRooted(userSetting.DataSource) ||
                !Directory.Exists(userSetting.DataSource)) return false;

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

            if (SortsViewModel.Instance.SelectedSortItems.Count == 0) return false;

            if (!Regex.IsMatch(imageSetting.ImageSizeString, _sizePattern) ||
                !Regex.IsMatch(imageSetting.StampSizeString, _sizePattern) ||
                !Regex.IsMatch(imageSetting.StampMarrginString, _sizePattern))
                return false;

            if (string.IsNullOrWhiteSpace(imageSetting.OutPutFolder) ||
                imageSetting.OutPutFolder.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0 ||
                !System.IO.Path.IsPathRooted(imageSetting.OutPutFolder) ||
                !Directory.Exists(imageSetting.OutPutFolder))
                return false;

            if (imageSetting.SelectedCardMaker == null) return false;

            if (imageSetting.StampPosition < 0 ||
                string.IsNullOrEmpty(imageSetting.BackgroundArt) ||
                string.IsNullOrEmpty(imageSetting.Foild) ||
                imageSetting.Secret < 0 || imageSetting.Secret > 3)
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
        public static bool HasReadPermission(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath)) return false;
                Directory.EnumerateFileSystemEntries(folderPath).FirstOrDefault();
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
        public static bool HasWritePermission(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath)) return false;

                string testFile = Path.Combine(folderPath, Path.GetRandomFileName());

                using (var fs = new FileStream(testFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose))
                {
                    fs.WriteByte(0);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region Event
        private void DeckEditSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveSetting?.RaiseCanExecuteChanged();
        }
        private void CodeEditSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveSetting?.RaiseCanExecuteChanged();
        }
        private void ImageSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveSetting?.RaiseCanExecuteChanged();
        }
        private void DataHandlingSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveSetting?.RaiseCanExecuteChanged();
        }
        private void DisplaySetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveSetting?.RaiseCanExecuteChanged();
        }
        private void UserSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveSetting?.RaiseCanExecuteChanged();
        }

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
            userSetting.PropertyChanged -= UserSetting_PropertyChanged;
            displaySetting.PropertyChanged -= DisplaySetting_PropertyChanged;
            dataHandlingSetting.PropertyChanged -= DataHandlingSetting_PropertyChanged;
            imageSetting.PropertyChanged -= ImageSetting_PropertyChanged;
            codeEditSetting.PropertyChanged -= CodeEditSetting_PropertyChanged;
            deckEditSetting.PropertyChanged -= DeckEditSetting_PropertyChanged;

            AddSortItemCommand = null;
            RemoveSortItemCommand = null;

            BrowseDataSource = null;
            BrowseArtWork = null;
            BrowsePutPut = null;
            BrowseOriginal = null;
            ResetSetting = null;
            ReloadSetting = null;
            SaveSetting = null;
            CancelSetting = null;

            userSetting = null;
            userSettingSource = null;
            displaySettingSource = null;
            displaySetting = null;
            dataHandlingSetting = null;
            imageSetting = null;
            imageSettingSource = null;
            codeEditSetting = null;
            deckEditSetting = null;

            CallConfigChanged = null;
            RequestOpenBrowseDialog = null;
            MessageBoxRequested = null;
            PropertyChanged = null;
        }
        #endregion

    }
}
