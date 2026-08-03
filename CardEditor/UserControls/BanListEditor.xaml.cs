using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Specialized;
using MaterialDesignThemes.Wpf;
using CardEditor.Helpers;
using CardEditor.Services;
using CardEditor.Collections;
using CardEditor.Models;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using RelayCommand = CardEditor.Commands.RelayCommand;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for BanListEditor.xaml
    /// </summary>
    public partial class BanListEditor : UserControl, INotifyPropertyChanged, IDisposable, ISaveable
    {
        #region Variable
        private IMainWindowService MainWindowService;
        public string MainWindowTitle { get; set; }
        private bool _isSaved = true;
        public bool IsSaved
        {
            get => _isSaved;
            set
            {
                if (_isSaved != value)
                {
                    _isSaved = value;
                    OnPropertyChanged(nameof(IsSaved));
                    UpdateWindowSavedFlag();
                }
            }
        }
        private bool _isInternalUpdate = false;
        public string CurrentBanListPath = string.Empty;
        public string CurrentBanListName = string.Empty;
        private string ImageUrl = string.Empty;
        private SnackbarMessageQueue MessageNotifi = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
        #endregion

        #region Collection
        public BulkObservableCollection<CardBanList> ForbiddenCards { get; set; }
        public BulkObservableCollection<CardBanList> LimitedCards { get; set; }
        public BulkObservableCollection<CardBanList> SemiLimitedCards { get; set; }
        public BulkObservableCollection<CardBanList> UnLimitedCards { get; set; }

        public ICollectionView ForbiddenCardsView { get; set; }
        public ICollectionView LimitedCardsView { get; set; }
        public ICollectionView SemiLimitedCardsView { get; set; }
        public ICollectionView UnLimitedCardsView { get; set; }
        #endregion

        #region Property
        private BanList _selectedBanList;
        public BanList SelectedBanList
        {
            get => _selectedBanList;
            set
            {
                if (_selectedBanList != value)
                {
                    _selectedBanList = value;
                    OnPropertyChanged(nameof(SelectedBanList));
                    LoadCardList();
                    SaveBanListCommand?.RaiseCanExecuteChanged();
                    DeleteBanListCommand?.RaiseCanExecuteChanged();
                    ClearBanListCommand?.RaiseCanExecuteChanged();
                    ReNameBanListCommand?.RaiseCanExecuteChanged();
                    SaveAsBanListCommand?.RaiseCanExecuteChanged();
                    UpLoadCardCommand?.RaiseCanExecuteChanged();

                    CurrentBanListPath = _selectedBanList?.FilePath ?? string.Empty;
                    CurrentBanListName = _selectedBanList?.FileName ?? string.Empty;
                    NewFileName = CurrentBanListName;
                    NewFolderPath = (string.IsNullOrEmpty(CurrentBanListPath) || !System.IO.File.Exists(CurrentBanListPath))
                        ? string.Empty : System.IO.Path.GetDirectoryName(CurrentBanListPath);
                    UpdateWindowTitle();
                }
            }
        }
        private void cmbBanList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newBanList = cmbBanList.SelectedItem as CardEditor.Models.BanList;
            if (newBanList == SelectedBanList) return;

            if (!IsSaved)
            {
                var result = CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    $"{CMess.HasUnSaveData.ToText()} {CMess.QuestContinue.ToText()}",
                    new[] { CMess.yes.ToText(), CMess.no.ToText(), });

                if (result != 0)
                {
                    OnPropertyChanged(nameof(SelectedBanList));
                    return;
                }
            }
            SelectedBanList = newBanList;
        }
        private CardBanList _selectedCard { get ; set; }
        public CardBanList SelectedCard
        {
            get => _selectedCard;
            set
            {
                if (_selectedCard != value)
                {
                    _selectedCard = value;
                    OnPropertyChanged(nameof(SelectedCard));
                    OnSelectedCardChanged();
                    AddCardCommand.RaiseCanExecuteChanged();
                    ModifyCardCommand.RaiseCanExecuteChanged();

                    ViewImageCommand.RaiseCanExecuteChanged();
                    OpenFileImageCommand.RaiseCanExecuteChanged();
                    OpenDatabaseCommand.RaiseCanExecuteChanged();
                    OpenDatabaseCommand.RaiseCanExecuteChanged();
                }
            }
        }
        public List<CardBanList> SelectedCards { get; set; }
        /// <summary>
        /// ///// BanList Info /////
        /// </summary>
        private string _banListName = string.Empty;
        public string BanListName
        {
            get => _banListName;
            set
            {
                if (_banListName != value)
                {
                    _banListName = value;
                    OnPropertyChanged(nameof(BanListName));
                }
            }
        }

        private bool _WhiteList = false;
        public bool WhiteList
        {
            get => _WhiteList;
            set
            {
                if (_WhiteList != value)
                {
                    _WhiteList = value;
                    OnPropertyChanged(nameof(WhiteList));
                }
            }
        }
        private string _newFileName = string.Empty;
        public string NewFileName
        {
            get => _newFileName;
            set
            {
                if (_newFileName != value)
                {
                    _newFileName = value;
                    OnPropertyChanged(nameof(NewFileName));
                    CreateNewBanListCommand?.RaiseCanExecuteChanged();
                    ReNameBanListCommand?.RaiseCanExecuteChanged();
                    SaveAsBanListCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private string _newFolderPath = string.Empty;
        public string NewFolderPath
        {
            get => _newFolderPath;
            set
            {
                if (_newFolderPath != value)
                {
                    _newFolderPath = value;
                    OnPropertyChanged(nameof(NewFolderPath));
                    CreateNewBanListCommand?.RaiseCanExecuteChanged();
                    ReNameBanListCommand?.RaiseCanExecuteChanged();
                    SaveAsBanListCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private ulong? _cardID;
        public ulong? CardID
        {
            get => _cardID;
            set
            {
                if (_cardID != value)
                {
                    _cardID = value;
                    OnPropertyChanged(nameof(CardID));
                    AddCardCommand?.RaiseCanExecuteChanged();
                    ModifyCardCommand?.RaiseCanExecuteChanged();
                    FilterCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private string _cardIDString = string.Empty;
        public string CardIDString
        {
            get => _cardIDString;
            set
            {
                if (_cardIDString != value)
                {
                    _cardIDString = value;
                    OnPropertyChanged(nameof(CardIDString));
                    if (!string.IsNullOrWhiteSpace(CardIDString) &&
                        ulong.TryParse(CardIDString, out ulong cardid))
                    {
                        CardID = cardid;
                    }
                    else CardID = 0;
                }
            }
        }
        private string _cardName;
        public string CardName
        {
            get => _cardName;
            set
            {
                if (_cardName != value)
                {
                    _cardName = value;
                    OnPropertyChanged(nameof(CardName));
                    AddCardCommand?.RaiseCanExecuteChanged();
                    ModifyCardCommand?.RaiseCanExecuteChanged();
                    FilterCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private int _limitedCount;
        public int LimitedCount
        {
            get => _limitedCount;
            set
            {
                if (_limitedCount != value)
                {
                    _limitedCount = value;
                    OnPropertyChanged(nameof(LimitedCount));
                    AddCardCommand?.RaiseCanExecuteChanged();
                    ModifyCardCommand?.RaiseCanExecuteChanged();
                    FilterCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion

        #region Commands
        public RelayCommand NewBanListCommand { get; set; } 
        public RelayCommand OpenBanListCommand { get; set; }
        public RelayCommand BrowseCommand { get; set; }
        public RelayCommand SaveBanListCommand { get; set; }
        public RelayCommand DeleteBanListCommand { get; set; }
        public RelayCommand ClearBanListCommand { get; set; }
        public RelayCommand CreateNewBanListCommand { get; set; }
        public RelayCommand ReNameBanListCommand { get; set; }
        public RelayCommand SaveAsBanListCommand { get; set; }
        public RelayCommand BrowseNewPathCommand { get; set; }
        ////////////////
        public RelayCommand AddCardCommand { get; set; }
        public RelayCommand ModifyCardCommand { get; set; }
        public RelayCommand UpLoadCardCommand { get; set; }
        public RelayCommand SortCardCommand { get; set; }
        public RelayCommand FilterCardCommand { get; set; }
        public RelayCommand RefreshCardCommand { get; set; }
        ////////////////
        public RelayCommand ViewImageCommand { get; set; }
        public RelayCommand OpenFileImageCommand { get; set; }
        public RelayCommand OpenDatabaseCommand { get; set; }
        public RelayCommand OpenScriptCommand { get; set; }

        public RelayCommand OpenKonamiDBCommand { get; set; }
        public RelayCommand OpenYugipediaCommand { get; set; }
        public RelayCommand OpenYGOResourcesCommand { get; set; }
        #endregion

        #region Constructor
        public BanListEditor()
        {
            InitializeComponent();
            InitializeCommands();
            NotifiSnackbar.MessageQueue = MessageNotifi;
            ForbiddenCards = new BulkObservableCollection<CardBanList>();
            LimitedCards = new BulkObservableCollection<CardBanList>();
            SemiLimitedCards = new BulkObservableCollection<CardBanList>();
            UnLimitedCards = new BulkObservableCollection<CardBanList>();

            ForbiddenCards.CollectionChanged += OnCardsCollectionChanged;
            LimitedCards.CollectionChanged += OnCardsCollectionChanged;
            SemiLimitedCards.CollectionChanged += OnCardsCollectionChanged;
            UnLimitedCards.CollectionChanged += OnCardsCollectionChanged;

            ForbiddenCardsView = CollectionViewSource.GetDefaultView(ForbiddenCards);
            LimitedCardsView = CollectionViewSource.GetDefaultView(LimitedCards);
            SemiLimitedCardsView = CollectionViewSource.GetDefaultView(SemiLimitedCards);
            UnLimitedCardsView = CollectionViewSource.GetDefaultView(UnLimitedCards);
            SelectedCards = new List<CardBanList>();

            this.DataContext = this;
        }
        public BanListEditor(IMainWindowService service) : this()
        {
            MainWindowService = service;
        }
        private void InitializeCommands()
        {
            NewBanListCommand = new CardEditor.Commands.RelayCommand( _ => NewBanList());
            OpenBanListCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenBanList());
            BrowseCommand = new CardEditor.Commands.RelayCommand(async _ => await BrowseBanListFile());
            SaveBanListCommand = new CardEditor.Commands.RelayCommand(async _ => await SaveBanListFile(), _ => CanSaveBanListCommand());
            DeleteBanListCommand = new CardEditor.Commands.RelayCommand( _ => DeleteBanList(), _ => CanClearDeleteBanList());
            ClearBanListCommand = new CardEditor.Commands.RelayCommand( _ => ClearBanList(), _ => CanClearDeleteBanList());
            CreateNewBanListCommand = new CardEditor.Commands.RelayCommand(async _ => await CreateNewBanList(), _ => CanNewBanList());
            ReNameBanListCommand = new CardEditor.Commands.RelayCommand( _ => ReNameBanList(), _ => CanReNameBanList());
            SaveAsBanListCommand = new CardEditor.Commands.RelayCommand(async _ => await SaveAsBanList(), _ => CanReNameBanList());
            BrowseNewPathCommand = new CardEditor.Commands.RelayCommand(_ => BrowseNewPath());

            AddCardCommand = new CardEditor.Commands.RelayCommand( _ => AddCard(), _ => CanAddModifyCard());
            ModifyCardCommand = new CardEditor.Commands.RelayCommand( _ => ModifyCard(), _ => CanAddModifyCard());
            UpLoadCardCommand = new CardEditor.Commands.RelayCommand( _ => UpLoadCard(), _ => CanUpLoadCard());
            SortCardCommand = new CardEditor.Commands.RelayCommand( _ => SortCard(), _ => CanSortCard());
            FilterCardCommand = new CardEditor.Commands.RelayCommand( _ => FilterCard(), _ => CanFilterCard());
            RefreshCardCommand = new CardEditor.Commands.RelayCommand( _ => RefreshCard(), _ => CanSortCard());

            ViewImageCommand = new CardEditor.Commands.RelayCommand(_ => ViewImage(), _ => SelectedCardExist());
            OpenFileImageCommand = new CardEditor.Commands.RelayCommand(_ => OpenFileLocation(), _ => SelectedCardExist());
            OpenDatabaseCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenDataBase(), _ => SelectedCardExist());
            OpenScriptCommand = new CardEditor.Commands.RelayCommand(_ => OpenScript(), _ => SelectedCardExist());

            OpenKonamiDBCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenKonamiDB(), _ => SelectedCardExist());
            OpenYugipediaCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenYugipedia(), _ => SelectedCardExist());
            OpenYGOResourcesCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenYGOResources(), _ => SelectedCardExist());
        }
        private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();
        private static JsonSerializerOptions CreateSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }
        #endregion

        #region Loaded
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            InitializeContentMenu();
            if (!BanListRawDataViewModel.Instance.IsLoaded)
            {
                var(result, message) = await BanListRawDataViewModel.Instance.LoadBanLists();
                if (!result)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                }
            }
        }
        private void InitializeContentMenu()
        {
            ControlContextMenuService.Attach(txtBanListName);
            ControlContextMenuService.Attach(txtCardId);
            ControlContextMenuService.Attach(txtCardName);
            ControlContextMenuService.Attach(txtReName);
            ControlContextMenuService.Attach(txtNewPath);
        }
        public void LoadConfig()
        {

        }
        #endregion

        #region Save
        public async Task<bool> Save()
        {
            if (SelectedBanList == null || string.IsNullOrWhiteSpace(SelectedBanList.FilePath) ||
                !System.IO.File.Exists(SelectedBanList.FilePath))
            {
                string filePath = FileDiaLogHelper.SaveBanList();

                if (!string.IsNullOrEmpty(filePath))
                {
                    BanList saveBanList = new BanList()
                    {

                        Name = BanListName,
                        FileName = System.IO.Path.GetFileName(filePath),
                        FilePath = filePath,
                        CardList = BuildCardList(),
                        WhiteList = WhiteList,
                    };
                    var (resultSave, messageSave) = await BanListRawDataViewModel.Instance.SaveBanListFile(saveBanList);
                    if (resultSave)
                    {
                        IsSaved = true;
                        return true;
                    }
                    else
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {messageSave}", new[] { CMess.ok.ToText() });
                        return false;
                    }
                }
                else return false;
            }
            else
            {
                SelectedBanList.CardList = BuildCardList();
                var (resultSave, messageSave) = await BanListRawDataViewModel.Instance.SaveBanListFile(SelectedBanList);
                if (resultSave)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.ThreePlaceholderSuccess.ToText(), SelectedBanList.Name, CMess.BanList.ToText(), CMess.Save.ToText()),
                        // FileName BanList Save successfully!
                        new[] { CMess.ok.ToText() });
                    IsSaved = true;
                    return true;
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {messageSave}", new[] { CMess.ok.ToText() });
                    return false;
                }
            }
        }
        #endregion

        #region BanList
        private string GetNewFolderPath()
        {
            if (string.IsNullOrEmpty(NewFolderPath) || !Directory.Exists(NewFolderPath))
            {
                string folderPath = FileDiaLogHelper.OpenFolder();

                if (!string.IsNullOrEmpty(folderPath))
                {
                    NewFolderPath = folderPath;
                    return NewFolderPath;
                }
                else return string.Empty;
            }
            return NewFolderPath;
        }
        public record CardListGroup(
            List<CardBanList> Forbidden,
            List<CardBanList> Limited,
            List<CardBanList> SemiLimited,
            List<CardBanList> Unlimited);
        public void NewBanList()
        {
            popNewBanList.IsOpen = true;
        }
        private async Task OpenBanList(string filePath = null)
        {
            string openFilePath = (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                ? filePath : FileDiaLogHelper.OpenBanList();
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                BanList newBanList = await BanListRawDataViewModel.Instance.LoadFileBanList(filePath);
                if (newBanList != null)
                {
                    BanListRawDataViewModel.Instance.BanLists.Add(newBanList);
                    SelectedBanList = newBanList;
                }
            }
        }
        private async Task BrowseBanListFile()
        {
            try
            {
                string filePath = FileDiaLogHelper.OpenBanList();
                if (!string.IsNullOrEmpty(filePath))
                {
                    if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 0)
                    {
                        int resultBrowse = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question, CMess.confirmWriteData.ToText(),
                            new[] { CMess.OverwriteDupli.ToText(), CMess.Appendwrite.ToText(), CMess.CreateNew.ToText(), CMess.cancel.ToText() });
                        if (resultBrowse == 0)
                        {
                            var (result, message) = await BrowseBanListFileOverWrite(filePath);
                            if (result)
                                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                                    $"{string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Import.ToText(), CMess.Data.ToText())}  ({CMess.OverwriteDupli.ToText()})", new[] { CMess.ok.ToText() });
                            else
                                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                                    $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                        }
                        else if (resultBrowse == 1)
                        {
                            var (result, message) = await BrowseBanListFileAppendwrite(filePath);
                            if (result)
                                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                                    $"{string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Import.ToText(), CMess.Data.ToText())} ({CMess.Appendwrite.ToText()})", new[] { CMess.ok.ToText() });
                            else
                                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                                    $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                        }
                        else if (resultBrowse == 2)
                        {
                            await BrowseBanListFileCreateNew(filePath);
                        }
                        else return;
                    }
                    else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 1)
                    {
                        var (result, message) = await BrowseBanListFileOverWrite(filePath);
                        if (result)
                            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                                $"{string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Import.ToText(), CMess.Data.ToText())} ({CMess.OverwriteDupli.ToText()})", new[] { CMess.ok.ToText() });
                        else
                            CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                                $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                    }
                    else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 2)
                    {
                        var (result, message) = await BrowseBanListFileAppendwrite(filePath);
                        if (result)
                            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                                $"{string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Import.ToText(), CMess.Data.ToText())} ({CMess.Appendwrite.ToText()})", new[] { CMess.ok.ToText() });
                        else
                            CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                                $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                    }
                    else
                    {
                        await BrowseBanListFileCreateNew(filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task<(bool, string)> BrowseBanListFileOverWrite(string filePath)
        {
            try
            {
                BanList newBanList = await BanListRawDataViewModel.Instance.LoadFileBanList(filePath);
                if (newBanList != null)
                {
                    var (result, message, CardList) = GetListCard(newBanList.CardList);
                    if (result)
                    {
                        if (CardList.Forbidden != null) ForbiddenCards.ReplaceAll(CardList.Forbidden);
                        if (CardList.Limited != null) LimitedCards.ReplaceAll(CardList.Limited);
                        if (CardList.SemiLimited != null) SemiLimitedCards.ReplaceAll(CardList.SemiLimited);
                        if (CardList.Unlimited != null) UnLimitedCards.ReplaceAll(CardList.Unlimited);
                        return (true, string.Empty);
                    }
                    else return (false, message);
                }
                else return (false, "BanList vua chon bi null");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        private async Task<(bool, string)> BrowseBanListFileAppendwrite(string filePath)
        {
            try
            {
                BanList newBanList = await BanListRawDataViewModel.Instance.LoadFileBanList(filePath);
                if (newBanList != null)
                {
                    var (result, message, CardList) = GetListCard(newBanList.CardList);
                    if (result)
                    {
                        if (CardList.Forbidden != null) ForbiddenCards.AddRange(CardList.Forbidden);
                        if (CardList.Limited != null) LimitedCards.AddRange(CardList.Limited);
                        if (CardList.SemiLimited != null) SemiLimitedCards.AddRange(CardList.SemiLimited);
                        if (CardList.Unlimited != null) UnLimitedCards.AddRange(CardList.Unlimited);
                        return (true, string.Empty);
                    }
                    else return (false, message);
                }
                else return (false, "BanList vua chon bi null");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task BrowseBanListFileCreateNew(string filePath)
        {
            await OpenBanList(filePath);
        }

        public async Task SaveBanListFile()
        {
            if (SelectedBanList == null ||
                string.IsNullOrWhiteSpace(SelectedBanList.FilePath) ||
                !System.IO.File.Exists(SelectedBanList.FilePath))
            {
                string filePath = FileDiaLogHelper.SaveBanList();

                if (!string.IsNullOrEmpty(filePath))
                {
                    BanList saveBanList = new BanList()
                    {
                        
                        Name = BanListName,
                        FileName = System.IO.Path.GetFileName(filePath),
                        FilePath = filePath,
                        CardList = BuildCardList(),
                        WhiteList = WhiteList,
                    };
                    var (resultSave, messageSave) = await BanListRawDataViewModel.Instance.SaveBanListFile(saveBanList);
                    if (resultSave)
                    {
                        CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.ThreePlaceholderSuccess.ToText(), saveBanList.Name, CMess.BanList.ToText(), CMess.Save.ToText()),
                        // FileName BanList Save successfully!
                        new[] { CMess.ok.ToText() });
                        IsSaved = true;
                    }
                    else
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {messageSave}", new[] { CMess.ok.ToText() });
                    }
                }
                else return;
            }
            else
            {
                SelectedBanList.CardList = BuildCardList();
                var (resultSave, messageSave) = await BanListRawDataViewModel.Instance.SaveBanListFile(SelectedBanList);
                if (resultSave)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.ThreePlaceholderSuccess.ToText(), SelectedBanList.Name, CMess.BanList.ToText(), CMess.Save.ToText()),
                        // FileName BanList Save successfully!
                        new[] { CMess.ok.ToText() });
                    IsSaved = true;
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {messageSave}", new[] { CMess.ok.ToText() });
                }
            }
        }
        private bool CanSaveBanListCommand()
        {
            if (SelectedBanList == null) return false;
            if (ForbiddenCards == null || LimitedCards == null ||
                SemiLimitedCards == null || UnLimitedCards == null) return false;
            if (ForbiddenCards.Count != 0 ||
                LimitedCards.Count != 0 ||
                SemiLimitedCards.Count != 0 ||
                UnLimitedCards.Count != 0) return true;
            return false;
        }
        private Dictionary<ulong, CardBanList> BuildCardList()
        {
            return ForbiddenCards.Concat(LimitedCards).Concat(SemiLimitedCards).Concat(UnLimitedCards)
                .GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
        }
        private void DeleteBanList()
        {
            if (SelectedBanList == null) return;
            if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmDelete)
            {
                int result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    string.Format(CMess.confirmDelete.ToText(), CMess.SelectedBanList.ToText()),
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
            }
            string oldName = SelectedBanList.Name;
            var (resultDelete, messageDelete) = BanListRawDataViewModel.Instance.DeleteBanList(SelectedBanList);
            if (resultDelete)
            {
                SelectedBanList = null;
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.ThreePlaceholderSuccess.ToText(), oldName, CMess.BanList.ToText(), CMess.tlDelete.ToText()),
                    new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageDelete}", new[] { CMess.ok.ToText() });
            }
        }
        private void ClearBanList()
        {
            if (SelectedBanList == null) return;

            if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmClear)
            {
                int result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    string.Format(CMess.ConfirmClear.ToText(), CMess.SelectedBanList.ToText()),
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
            }
            BanListName = string.Empty;
            WhiteList = false;
            ForbiddenCards.Clear();
            LimitedCards.Clear();
            SemiLimitedCards.Clear();
            UnLimitedCards.Clear();
        }
        private bool CanClearDeleteBanList()
        {
            return SelectedBanList != null;
        }

        private async Task CreateNewBanList()
        {
            try
            {
                string banlistPath = GetNewFolderPath();
                if (string.IsNullOrEmpty(banlistPath)) return;

                string newName = JoinStringHelper.SanitizationString(NewFileName.Trim());
                var ext = System.IO.Path.GetExtension(newName);

                if (string.IsNullOrEmpty(ext) || ext.Length <= 1)
                {
                    if (newName.EndsWith("."))
                        newName = newName.TrimEnd('.');
                    newName += ".lflist.conf";
                }

                string filePath = System.IO.Path.Combine(banlistPath, newName);
                if (!Directory.Exists(banlistPath)) Directory.CreateDirectory(banlistPath);
                using (FileStream fs = File.Create(filePath)) { }

                BanList newBanList = new BanList()
                {
                    Name = BanListName,
                    WhiteList = WhiteList,
                    FileName = newName,
                    FilePath = filePath,
                    CardList = BuildCardList()
                };

                var (result, message) = await BanListRawDataViewModel.Instance.SaveBanListFile(newBanList);
                if (result)
                {
                    BanListRawDataViewModel.Instance.BanLists.Add(newBanList);
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.ThreePlaceholderSuccess.ToText(), BanListName, CMess.BanList.ToText(), CMess.tlAdd.ToText()),
                        new[] { CMess.ok.ToText() });
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private void ReNameBanList()
        {
            try
            {
                string banlistPath = GetNewFolderPath();
                if (string.IsNullOrEmpty(banlistPath)) return;

                string newName = JoinStringHelper.SanitizationString(NewFileName.Trim());
                var ext = System.IO.Path.GetExtension(newName);

                if (string.IsNullOrEmpty(ext) || ext.Length <= 1)
                {
                    if (newName.EndsWith("."))
                        newName = newName.TrimEnd('.');
                    newName += ".lflist.conf";
                }

                string oldFilePath = SelectedBanList.FilePath;
                string newFilePath = System.IO.Path.Combine(banlistPath, newName);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Move(oldFilePath, newFilePath);
                }
                SelectedBanList.Name = BanListName;
                SelectedBanList.FileName = NewFileName;
                SelectedBanList.FilePath = newFilePath;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        public async Task SaveAsBanList()
        {
            string banlistPath = GetNewFolderPath();
            if (string.IsNullOrEmpty(banlistPath)) return;

            string newName = JoinStringHelper.SanitizationString(NewFileName.Trim());
            var ext = System.IO.Path.GetExtension(newName);

            if (string.IsNullOrEmpty(ext) || ext.Length <= 1)
            {
                if (newName.EndsWith("."))
                    newName = newName.TrimEnd('.');
                newName += ".lflist.conf";
            }

            string newFilePath = System.IO.Path.Combine(banlistPath, newName);
            if (System.IO.File.Exists(newFilePath))
            {
                int result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    $"{CMess.filealreadyExit.ToText()} {CMess.QuestOverwrite.ToText()}",
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
                System.IO.File.Delete(newFilePath);
            }

            if (SelectedBanList == null)
            {
                BanList saveBanList = new BanList()
                {
                    Name = BanListName,
                    FileName = NewFileName,
                    FilePath = newFilePath,
                    CardList = BuildCardList(),
                    WhiteList = WhiteList,
                };
                var (resultSave, messageSave) = await BanListRawDataViewModel.Instance.SaveBanListFile(saveBanList);
                if (resultSave)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.ThreePlaceholderSuccess.ToText(), saveBanList.Name, CMess.BanList.ToText(), CMess.Save.ToText()),
                        new[] { CMess.ok.ToText() });
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {messageSave}", new[] { CMess.ok.ToText() });
                }
            }
            else
            {
                var (resultSave, messageSave) = await BanListRawDataViewModel.Instance.SaveBanListFile(SelectedBanList, newFilePath);
                if (resultSave)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.ThreePlaceholderSuccess.ToText(), SelectedBanList.Name, CMess.BanList.ToText(), CMess.Save.ToText()),
                        new[] { CMess.ok.ToText() });
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {messageSave}", new[] { CMess.ok.ToText() });
                }
            }
        }
        private bool CanNewBanList()
        {
            return (!string.IsNullOrWhiteSpace(NewFileName) &&
                !string.IsNullOrWhiteSpace(NewFolderPath) &&
                System.IO.Directory.Exists(NewFolderPath));
        }
        private bool CanReNameBanList()
        {
            return CanNewBanList() && SelectedBanList != null;
        }
        private void BrowseNewPath()
        {
            string selectedFolder = FileDiaLogHelper.OpenFolder();
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                NewFolderPath = selectedFolder;
            }

        }
        #endregion

        #region Card
        private void AddCard()
        {
            try
            {
                CardBanList newCard = new CardBanList()
                {
                    Id = CardID.Value,
                    Name = CardName,
                    LimitedCount = LimitedCount,
                };

                if (LimitedCount == 0) ForbiddenCards.Add(newCard);
                else if (LimitedCount == 1) LimitedCards.Add(newCard);
                else if (LimitedCount == 2) SemiLimitedCards.Add(newCard);
                else UnLimitedCards.Add(newCard);

                MessageNotifi.Enqueue(string.Format(CMess.ThreePlaceholderSuccess.ToText(), 1.ToString(), CMess.Card.ToText(), CMess.tlAdd.ToText()));
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private void ModifyCard()
        {
            try
            {
                CardBanList newCard = new CardBanList()
                {
                    Id = CardID.Value,
                    Name = CardName,
                    LimitedCount = LimitedCount,
                };

                int Forbidden = ModifyCollection(ForbiddenCards, newCard);
                int Limited = ModifyCollection(LimitedCards, newCard);
                int Semi = ModifyCollection(SemiLimitedCards, newCard);
                int UnLimited = ModifyCollection(UnLimitedCards, newCard);
                int Total = Forbidden + Limited + Semi + UnLimited;
                MessageNotifi.Enqueue(string.Format(CMess.TwoPlaceholderSuccess.ToText(), Total.ToString(), CMess.Card.ToText(), CMess.Update.ToText()));
                // 1 Card Update successfully!
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private bool CanAddModifyCard()
        {
            if (CardID == null) return false;
            if (CardID.Value <= 0 || CardID.Value > uint.MaxValue) return false;

            return true;
        }
        private int ModifyCollection(BulkObservableCollection<CardBanList> collection, CardBanList updated)
        {
            if (collection == null || updated == null) return 0;

            var match = collection.Search(
                x => x.Id == updated.Id, new CardEditor.Collections.SearchOptions
                {
                    UseCache = true,
                    CacheKey = $"Banlist_{updated.Id}",
                    UseParallel = true,
                    ParallelThreshold = 500
                }).FirstOrDefault();

            if (match != null)
            {
                match.Name = updated.Name;
                match.LimitedCount = updated.LimitedCount;

                return 1;
            }
            return 0;
        }
        private void UpLoadCard()
        {
            SelectedBanList.Name = BanListName;
            SelectedBanList.CardList = BuildCardList();
            SelectedBanList.WhiteList = WhiteList;

            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                string.Format(CMess.ThreePlaceholderSuccess.ToText(), CMess.Update.ToText(), SelectedBanList.Name, CMess.BanList.ToText()), new[] { CMess.ok.ToText() });
        }
        private bool CanUpLoadCard()
        {
            return SelectedBanList != null;
        }
        private void SortCard()
        {
            var selectedSorts = SortsViewModel.Instance.SelectedSortItems;
            if (selectedSorts == null || selectedSorts.Count <= 0) return;

            try
            {
                List<SortListCard> SortList = new List<SortListCard>();
                foreach (var sort in selectedSorts)
                {
                    if (sort?.SelectedItem == null) continue;

                    if (sort.SelectedItem.Name == "Id" || sort.SelectedItem.Name == "Name")
                    {
                        SortList.Add(new SortListCard { Name = sort.SelectedItem.Name, OrderByAsc = sort.OrderByAsc });
                    }
                }

                using (ForbiddenCardsView.DeferRefresh())
                {
                    ForbiddenCardsView.SortDescriptions.Clear();
                    foreach(var item in SortList)
                    {
                        var direction = item.OrderByAsc ? ListSortDirection.Ascending : ListSortDirection.Descending;
                        ForbiddenCardsView.SortDescriptions.Add(new SortDescription(item.Name, direction));
                    }
                }
                using (LimitedCardsView.DeferRefresh())
                {
                    LimitedCardsView.SortDescriptions.Clear();
                    foreach (var item in SortList)
                    {
                        var direction = item.OrderByAsc ? ListSortDirection.Ascending : ListSortDirection.Descending;
                        LimitedCardsView.SortDescriptions.Add(new SortDescription(item.Name, direction));
                    }
                }
                using (SemiLimitedCardsView.DeferRefresh())
                {
                    SemiLimitedCardsView.SortDescriptions.Clear();
                    foreach (var item in SortList)
                    {
                        var direction = item.OrderByAsc ? ListSortDirection.Ascending : ListSortDirection.Descending;
                        SemiLimitedCardsView.SortDescriptions.Add(new SortDescription(item.Name, direction));
                    }
                }
                using (UnLimitedCardsView.DeferRefresh())
                {
                    UnLimitedCardsView.SortDescriptions.Clear();
                    foreach (var item in SortList)
                    {
                        var direction = item.OrderByAsc ? ListSortDirection.Ascending : ListSortDirection.Descending;
                        UnLimitedCardsView.SortDescriptions.Add(new SortDescription(item.Name, direction));
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private bool CanSortCard()
        {
            if (ForbiddenCards == null || LimitedCards == null || SemiLimitedCards == null || UnLimitedCards == null) return false;
            if (ForbiddenCards.Count > 0 ||
                LimitedCards.Count > 0 ||
                SemiLimitedCards.Count > 0 ||
                UnLimitedCards.Count > 0) return true;
            else return false;
        }
        private class SortListCard
        {
            public string Name { get; set; }
            public bool OrderByAsc { get; set; }
        }
        private bool CanFilterCard()
        {
            return (CardID.HasValue || !string.IsNullOrWhiteSpace(CardName) || LimitedCount < 0);
        }
        private void RefreshCard()
        {
            try
            {
                List<CardBanList> AllCards = new List<CardBanList>();
                AllCards.AddRange(ForbiddenCards);
                AllCards.AddRange(LimitedCards);
                AllCards.AddRange(SemiLimitedCards);
                AllCards.AddRange(UnLimitedCards);

                var forbidden = new List<CardBanList>();
                var limited = new List<CardBanList>();
                var semiLimited = new List<CardBanList>();
                var unlimited = new List<CardBanList>();

                foreach (var card in AllCards)
                {
                    switch (card.LimitedCount)
                    {
                        case 0: forbidden.Add(card); break;
                        case 1: limited.Add(card); break;
                        case 2: semiLimited.Add(card); break;
                        default: unlimited.Add(card); break;
                    }
                }

                ForbiddenCards.ReplaceAll(forbidden);
                LimitedCards.ReplaceAll(limited);
                SemiLimitedCards.ReplaceAll(semiLimited);
                UnLimitedCards.ReplaceAll(unlimited);

                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.ThreePlaceholderSuccess.ToText(), CMess.Refresh.ToText(), BanListName, CMess.BanList.ToText()), new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #region Import
        public (bool, List<ulong>) CheckDuplicateIds()
        {
            if (ForbiddenCards == null || LimitedCards == null ||
                SemiLimitedCards == null || UnLimitedCards == null) return (false, null);
            var duplicates = ForbiddenCards.Concat(LimitedCards).Concat(SemiLimitedCards).Concat(UnLimitedCards)
                .GroupBy(c => c.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            return (duplicates.Count > 0, duplicates);
        }

        public void ImportCreateNew(IEnumerable<CardEditor.Models.CardBanList> importedCards)
        {
            SelectedBanList = null;
            List<CardBanList> ForBiddenList = new List<CardBanList>();
            List<CardBanList> LimitedList = new List<CardBanList>();
            List<CardBanList> SemiLimitedList = new List<CardBanList>();
            List<CardBanList> UnLimitedList = new List<CardBanList>();

            foreach (var card in importedCards)
            {
                if (card.LimitedCount <= 0) ForBiddenList.Add(card);
                else if (card.LimitedCount == 1) LimitedList.Add(card);
                else if (card.LimitedCount == 2) SemiLimitedList.Add(card);
                else UnLimitedList.Add(card);
            }
            ForbiddenCards.ReplaceAll(ForBiddenList);
            LimitedCards.ReplaceAll(LimitedList);
            SemiLimitedCards.ReplaceAll(SemiLimitedList);
            UnLimitedCards.ReplaceAll(UnLimitedList);

            Mouse.OverrideCursor = null;
        }
        public void ImportOverwrite(IEnumerable<CardEditor.Models.CardBanList> importedCards)
        {
            if (importedCards == null || !importedCards.Any()) return;

            List<CardBanList> ForBiddenList = new List<CardBanList>();
            List<CardBanList> LimitedList = new List<CardBanList>();
            List<CardBanList> SemiLimitedList = new List<CardBanList>();
            List<CardBanList> UnLimitedList = new List<CardBanList>();

            var ForBiddenDict = ForbiddenCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
            var LimitedDict = LimitedCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
            var SemiLimitedDict = SemiLimitedCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
            var UnLimitedDict = UnLimitedCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());

            foreach (var card in importedCards)
            {
                if (card.LimitedCount <= 0)
                {
                    if (ForBiddenDict.TryGetValue(card.Id, out var existingCard))
                    {
                        existingCard.Name = card.Name;
                        existingCard.LimitedCount = card.LimitedCount;
                    }
                    else
                    {
                        ForBiddenList.Add(card);
                    }
                }
                else if (card.LimitedCount == 1)
                {
                    if (LimitedDict.TryGetValue(card.Id, out var existingCard))
                    {
                        existingCard.Name = card.Name;
                        existingCard.LimitedCount = card.LimitedCount;
                    }
                    else
                    {
                        LimitedList.Add(card);
                    }
                }
                else if (card.LimitedCount == 2)
                {
                    if (SemiLimitedDict.TryGetValue(card.Id, out var existingCard))
                    {
                        existingCard.Name = card.Name;
                        existingCard.LimitedCount = card.LimitedCount;
                    }
                    else
                    {
                        SemiLimitedList.Add(card);
                    }
                }
                else
                {
                    if (UnLimitedDict.TryGetValue(card.Id, out var existingCard))
                    {
                        existingCard.Name = card.Name;
                        existingCard.LimitedCount = card.LimitedCount;
                    }
                    else
                    {
                        UnLimitedList.Add(card);
                    }
                }
            }
            ForbiddenCards.AddRange(ForBiddenList);
            LimitedCards.AddRange(LimitedList);
            SemiLimitedCards.AddRange(SemiLimitedList);
            UnLimitedCards.AddRange(UnLimitedList);

            Mouse.OverrideCursor = null;
        }
        public void ImportAppendwrite(IEnumerable<CardEditor.Models.CardBanList> importedCards)
        {
            List<CardBanList> ForBiddenList = new List<CardBanList>();
            List<CardBanList> LimitedList = new List<CardBanList>();
            List<CardBanList> SemiLimitedList = new List<CardBanList>();
            List<CardBanList> UnLimitedList = new List<CardBanList>();

            var ForBiddenDict = ForbiddenCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
            var LimitedDict = LimitedCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
            var SemiLimitedDict = SemiLimitedCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
            var UnLimitedDict = UnLimitedCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());

            foreach (var card in importedCards)
            {
                if (card.LimitedCount <= 0)
                {
                    if (!ForBiddenDict.ContainsKey(card.Id))
                    {
                        ForBiddenList.Add(card);
                    }
                }
                else if (card.LimitedCount == 1)
                {
                    if (!LimitedDict.ContainsKey(card.Id))
                    {
                        LimitedList.Add(card);
                    }
                }
                else if (card.LimitedCount == 2)
                {
                    if (!SemiLimitedDict.ContainsKey(card.Id))
                    {
                        SemiLimitedList.Add(card);
                    }
                }
                else
                {
                    if (!UnLimitedDict.ContainsKey(card.Id))
                    {
                        UnLimitedList.Add(card);
                    }
                }
            }
            ForbiddenCards.AddRange(ForBiddenList);
            LimitedCards.AddRange(LimitedList);
            SemiLimitedCards.AddRange(SemiLimitedList);
            UnLimitedCards.AddRange(UnLimitedList);
            Mouse.OverrideCursor = null;
        }

        public (bool, string) ImportDataCommand(IEnumerable<CardEditor.Models.CardBanList> cardList, ulong flags)
        {
            if (cardList == null) return (false, CMess.noCardReplace.ToText());

            IEnumerable<Action<CardEditor.Models.CardBanList, CardEditor.Models.CardBanList>> updaters = 
                Enumerable.Empty<Action<CardEditor.Models.CardBanList, CardEditor.Models.CardBanList>>();

            if (flags != 0)
            {
                var selected = (CardFieldBanList)flags;
                updaters = CardFieldBanListUpdate.FieldUpdaters.Where(kv => selected.HasFlag(kv.Key)).Select(kv => kv.Value).ToList();
            }

            return ImportDataField(cardList, updaters);
        }
        public (bool, string) ImportDataField(IEnumerable<CardEditor.Models.CardBanList> cardList,
            IEnumerable<Action<CardEditor.Models.CardBanList, CardEditor.Models.CardBanList>> updaters)
        {
            if (cardList == null || ForbiddenCards == null || LimitedCards == null ||
                SemiLimitedCards == null || UnLimitedCards == null)
                return (false, CMess.noCardReplace.ToText());
            try
            {
                var existingForbiddenCards = ForbiddenCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
                var existingLimitedCards = LimitedCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
                var existingSemiLimitedCards = SemiLimitedCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
                var existingUnLimitedCards = UnLimitedCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
                var updaterList = updaters?.ToList();
                bool allowUpdate = updaterList?.Count > 0;
                int count = 0;

                ForbiddenCards.BeginUpdate();
                LimitedCards.BeginUpdate();
                SemiLimitedCards.BeginUpdate();
                UnLimitedCards.BeginUpdate();
                foreach (var newCard in cardList)
                {
                    switch (newCard.LimitedCount)
                    {
                        case <= 0:
                            if (existingForbiddenCards.TryGetValue(newCard.Id, out var existingForbidden))
                            {
                                if (allowUpdate) existingForbidden.UpdateFrom(newCard, updaterList);
                            }
                            else ForbiddenCards.Add(newCard);
                            break;
                        case 1:
                            if (existingLimitedCards.TryGetValue(newCard.Id, out var existingLimited))
                            {
                                if (allowUpdate) existingLimited.UpdateFrom(newCard, updaterList);
                            }
                            else LimitedCards.Add(newCard);
                            break;
                        case 2:
                            if (existingSemiLimitedCards.TryGetValue(newCard.Id, out var existingSemiLimited))
                            {
                                if (allowUpdate) existingSemiLimited.UpdateFrom(newCard, updaterList);
                            }
                            else SemiLimitedCards.Add(newCard);
                            break;
                        default:
                            if (existingUnLimitedCards.TryGetValue(newCard.Id, out var existingUnLimited))
                            {
                                if (allowUpdate) existingUnLimited.UpdateFrom(newCard, updaterList);
                            }
                            else UnLimitedCards.Add(newCard);
                            break;
                    }
                    count++;
                }
                ForbiddenCards.EndUpdate();
                LimitedCards.EndUpdate();
                SemiLimitedCards.EndUpdate();
                UnLimitedCards.EndUpdate();

                return (true, count.ToString());
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (bool, string) ImportDataField(IEnumerable<CardEditor.Models.CardBanList> cardList,
            params Action<CardEditor.Models.CardBanList, CardEditor.Models.CardBanList>[] updaters)
        {
            return ImportDataField(cardList, updaters.AsEnumerable());
        }
        #endregion

        #region Replace
        public (bool Success, int ReplacedCard, int TotalCard, string Message) ReplaceDataCommand(IEnumerable<CardEditor.Models.CardBanList> cardList, ulong flags, bool isAddNew)
        {
            if (cardList == null || flags == 0) return (false, 0, 0, CMess.noCardReplace.ToText());

            IEnumerable<Action<CardEditor.Models.CardBanList, CardEditor.Models.CardBanList>> updaters =
                Enumerable.Empty<Action<CardEditor.Models.CardBanList, CardEditor.Models.CardBanList>>();

            if (flags != 0)
            {
                var selected = (CardFieldBanList)flags;
                updaters = CardFieldBanListUpdate.FieldUpdaters.Where(kv => selected.HasFlag(kv.Key)).Select(kv => kv.Value).ToList();
            }

            return ReplaceDataField(cardList, isAddNew, updaters);
        }
        public (bool Success, int ReplacedCard, int TotalCard, string Message) ReplaceDataField(IEnumerable<CardEditor.Models.CardBanList> cardList, bool isAddNew,
            IEnumerable<Action<CardEditor.Models.CardBanList, CardEditor.Models.CardBanList>> updaters)
        {
            if (cardList == null || ForbiddenCards == null || LimitedCards == null ||
                SemiLimitedCards == null || UnLimitedCards == null)
                return (false, 0, 0, CMess.noCardReplace.ToText());

            try
            {
                var existingForbiddenCards = ForbiddenCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
                var existingLimitedCards = LimitedCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
                var existingSemiLimitedCards = SemiLimitedCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
                var existingUnLimitedCards = UnLimitedCards.GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());
                var updaterList = updaters?.ToList();
                bool allowUpdate = updaterList?.Count > 0;
                int replaced = 0;

                ForbiddenCards.BeginUpdate();
                LimitedCards.BeginUpdate();
                SemiLimitedCards.BeginUpdate();
                UnLimitedCards.BeginUpdate();
                foreach (var newCard in cardList)
                {
                    switch (newCard.LimitedCount)
                    {
                        case <= 0:
                            if (existingForbiddenCards.TryGetValue(newCard.Id, out var existingForbidden))
                            {
                                if (allowUpdate)
                                {
                                    existingForbidden.UpdateFrom(newCard, updaterList);
                                    replaced++;
                                }
                            }
                            else if (isAddNew)
                            {
                                ForbiddenCards.Add(newCard);
                                replaced++;
                            }
                            break;
                        case 1:
                            if (existingLimitedCards.TryGetValue(newCard.Id, out var existingLimited))
                            {
                                if (allowUpdate)
                                {
                                    existingLimited.UpdateFrom(newCard, updaterList);
                                    replaced++;
                                }
                            }
                            else if (isAddNew)
                            {
                                LimitedCards.Add(newCard);
                                replaced++;
                            }
                            break;
                        case 2:
                            if (existingSemiLimitedCards.TryGetValue(newCard.Id, out var existingSemiLimited))
                            {
                                if (allowUpdate)
                                {
                                    existingSemiLimited.UpdateFrom(newCard, updaterList);
                                    replaced++;
                                }
                            }
                            else if (isAddNew)
                            {
                                SemiLimitedCards.Add(newCard);
                                replaced++;
                            }
                            break;
                        default:
                            if (existingUnLimitedCards.TryGetValue(newCard.Id, out var existingUnLimited))
                            {
                                if (allowUpdate)
                                {
                                    existingUnLimited.UpdateFrom(newCard, updaterList);
                                    replaced++;
                                }
                            }
                            else if (isAddNew)
                            {
                                UnLimitedCards.Add(newCard);
                                replaced++;
                            }
                            break;
                    }
                }
                return (true, replaced, ForbiddenCards.Count() + LimitedCards.Count() + SemiLimitedCards.Count() + UnLimitedCards.Count(), string.Empty);
            }
            catch (Exception ex)
            {
                return (false, 0, 0, ex.Message);
            }
            finally
            {
                ForbiddenCards.EndUpdate();
                LimitedCards.EndUpdate();
                SemiLimitedCards.EndUpdate();
                UnLimitedCards.EndUpdate();
            }
        }
        public (bool Success, int ReplacedCard, int TotalCard, string Message) ReplaceDataField(IEnumerable<CardEditor.Models.CardBanList> cardList, bool isAddNew,
            params Action<CardEditor.Models.CardBanList, CardEditor.Models.CardBanList>[] updaters)
        {
            return ReplaceDataField(cardList, isAddNew, updaters.AsEnumerable());
        }
        #endregion

        #region Copy
        public async Task CopySelectedCard()
        {
            if (SelectedCards.Any())
            {
                try
                {
                    await CopyCards(SelectedCards);
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Copy.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
            else
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    CMess.noCardCopy.ToText(), new[] { CMess.ok.ToText() });
            }
        }
        public async Task CopyAllFilterCard()
        {
            var allFilteredCards = new List<CardEditor.Models.CardBanList>();
            allFilteredCards.AddRange(ForbiddenCardsView.Cast<CardBanList>());
            allFilteredCards.AddRange(LimitedCardsView.Cast<CardBanList>());
            allFilteredCards.AddRange(SemiLimitedCardsView.Cast<CardBanList>());
            allFilteredCards.AddRange(UnLimitedCardsView.Cast<CardBanList>());

            if (allFilteredCards.Any())
            {
                try
                {
                    await CopyCards(allFilteredCards);
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Copy.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
            else
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    CMess.noCardCopy.ToText(), new[] { CMess.ok.ToText() });
            }
        }
        public async Task CopyAllCard()
        {
            var allCards = new List<CardEditor.Models.CardBanList>();
            allCards.AddRange(ForbiddenCards);
            allCards.AddRange(LimitedCards);
            allCards.AddRange(SemiLimitedCards);
            allCards.AddRange(UnLimitedCards);


            if (allCards != null && allCards.Any())
            {
                try
                {
                    await CopyCards(allCards);
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Copy.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
            else
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    CMess.noCardCopy.ToText(), new[] { CMess.ok.ToText() });
            }
        }
        private async Task CopyCards(IEnumerable<CardEditor.Models.CardBanList> SelectedItems)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                string json = await Task.Run(() =>
                    JsonSerializer.Serialize(SelectedItems, SerializerOptions)
                );

                Clipboard.SetText(json);

                MessageNotifi.Enqueue(string.Format(CMess.ThreePlaceholderSuccess.ToText(), SelectedItems.Count(), CMess.Card.ToText(), CMess.Copy.ToText()));
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Copy.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        #endregion

        #region Filter
        private Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();
        private Regex GetOrCreateRegex(string pattern, bool matchCase)
        {
            string key = $"{pattern}|{matchCase}";
            if (!_regexCache.TryGetValue(key, out Regex regex))
            {
                var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                regex = new Regex(pattern, options | RegexOptions.Compiled); // Compiled để tăng tốc
                _regexCache[key] = regex;
            }
            return regex;
        }
        private string BuildRegexPattern(string pattern, bool matchPrefix, bool matchSuffix, bool wholeWords)
        {
            string regexPattern = Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".");

            if (matchPrefix) regexPattern = "^" + regexPattern;
            if (matchSuffix) regexPattern += "$";
            if (wholeWords) regexPattern = $@"\b{regexPattern}\b";

            return regexPattern;
        }
        private void FilterCard()
        {
            int advancedSettings = ConfigViewModel.Instance.dataHandlingSetting.Advanced;
            bool isAdvancedFind = (advancedSettings & 0x01) == 0x01;
            bool matchCase = (advancedSettings & 0x02) == 0x02;
            bool useWildcards = (advancedSettings & 0x04) == 0x04;
            bool matchPrefix = (advancedSettings & 0x08) == 0x08;
            bool matchSuffix = (advancedSettings & 0x10) == 0x10;
            bool wholeWords = (advancedSettings & 0x20) == 0x20;
            bool ignorePunctuation = (advancedSettings & 0x40) == 0x40;
            bool ignoreWhitespace = (advancedSettings & 0x80) == 0x80;

            string normalizedFilterName = CardName;
            if (isAdvancedFind)
            {
                if (!string.IsNullOrWhiteSpace(CardName))
                {
                    if (ignorePunctuation)
                        normalizedFilterName = new string(normalizedFilterName.Where(c => !char.IsPunctuation(c)).ToArray());
                    if (ignoreWhitespace)
                        normalizedFilterName = Regex.Replace(normalizedFilterName, @"\s+", "");
                }
            }
            Regex nameRegex = null;
            if (isAdvancedFind && !string.IsNullOrWhiteSpace(normalizedFilterName) && (useWildcards || wholeWords))
            {
                string pattern = useWildcards
                    ? BuildRegexPattern(normalizedFilterName, matchPrefix, matchSuffix, wholeWords)
                    : wholeWords ? $@"\b{Regex.Escape(normalizedFilterName)}\b" : null;

                if (pattern != null)
                    nameRegex = GetOrCreateRegex(pattern, matchCase);
            }
            StringComparison comparison = (isAdvancedFind && matchCase)
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            bool MatchName(string name)
            {
                if (string.IsNullOrWhiteSpace(CardName)) return true;
                if (string.IsNullOrWhiteSpace(name)) return false;

                string normalizedName = name;
                if (isAdvancedFind)
                {
                    if (ignorePunctuation)
                        normalizedName = new string(normalizedName.Where(c => !char.IsPunctuation(c)).ToArray());
                    if (ignoreWhitespace)
                        normalizedName = Regex.Replace(normalizedName, @"\s+", "");
                }

                if (nameRegex != null)
                    return nameRegex.IsMatch(normalizedName);

                int index = normalizedName.IndexOf(normalizedFilterName, comparison);
                if (index < 0) return false;
                if (matchPrefix && index != 0) return false;
                if (matchSuffix && (index + normalizedFilterName.Length) != normalizedName.Length) return false;

                return true;
            }

            ForbiddenCardsView.Filter = item =>
            {
                var card = (CardBanList)item;
                if (CardID.HasValue && card.Id != CardID.Value) return false;
                if (!MatchName(card.Name)) return false;
                return true;
            };
            LimitedCardsView.Filter = item =>
            {
                var card = (CardBanList)item;
                if (CardID.HasValue && card.Id != CardID.Value) return false;
                if (!MatchName(card.Name)) return false;
                return true;
            };
            SemiLimitedCardsView.Filter = item =>
            {
                var card = (CardBanList)item;
                if (CardID.HasValue && card.Id != CardID.Value) return false;
                if (!MatchName(card.Name)) return false;
                return true;
            };
            UnLimitedCardsView.Filter = item =>
            {
                var card = (CardBanList)item;
                if (CardID.HasValue && card.Id != CardID.Value) return false;
                if (!MatchName(card.Name)) return false;
                return true;
            };
        }

        public async Task FilterDuplicateDataFromYdkFile(bool isDuplicate)
        {
            if (ForbiddenCards == null || LimitedCards == null || SemiLimitedCards == null || UnLimitedCards == null)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.noCardFilter.ToText(), new[] { CMess.ok.ToText() });
                return;
            }
            if (!ForbiddenCards.Any() && !LimitedCards.Any() && !SemiLimitedCards.Any() && !UnLimitedCards.Any())
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.noCardFilter.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            string filePath = FileDiaLogHelper.OpenDeck();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                var cardIds = new HashSet<ulong>();
                const int bufferSize = 4096;

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true))
                using (var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line)) continue;
                        if (!Regex.IsMatch(line, @"^\d+$")) continue;

                        if (ulong.TryParse(line, out ulong id))
                        {
                            cardIds.Add(id);
                        }
                    }
                }

                if (isDuplicate)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ForbiddenCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return cardIds.Contains(card.id);
                        };
                        LimitedCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return cardIds.Contains(card.id);
                        };
                        SemiLimitedCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return cardIds.Contains(card.id);
                        };
                        UnLimitedCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return cardIds.Contains(card.id);
                        };
                    });
                }
                else
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ForbiddenCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return !cardIds.Contains(card.id);
                        };
                        LimitedCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return !cardIds.Contains(card.id);
                        };
                        SemiLimitedCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return !cardIds.Contains(card.id);
                        };
                        UnLimitedCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return !cardIds.Contains(card.id);
                        };
                    });
                }

                int totalFilted =
                    ForbiddenCardsView.Cast<CardEditor.Models.Card>().Count() +
                    LimitedCardsView.Cast<CardEditor.Models.Card>().Count() +
                    SemiLimitedCardsView.Cast<CardEditor.Models.Card>().Count() +
                    UnLimitedCardsView.Cast<CardEditor.Models.Card>().Count();

                int totalCards =
                    ForbiddenCards.Count +
                    LimitedCards.Count +
                    SemiLimitedCards.Count +
                    UnLimitedCards.Count;

                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.filterSuc.ToText(), totalFilted.ToString(), totalCards.ToString()),
                    new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        public async Task FilterDuplicateDataFromCdbFile(bool isDuplicate)
        {
            if (ForbiddenCards == null || LimitedCards == null || SemiLimitedCards == null || UnLimitedCards == null)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.noCardFilter.ToText(), new[] { CMess.ok.ToText() });
                return;
            }
            if (!ForbiddenCards.Any() && !LimitedCards.Any() && !SemiLimitedCards.Any() && !UnLimitedCards.Any())
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.noCardFilter.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            string filePath = FileDiaLogHelper.OpenDataBase();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                var cardIds = new HashSet<ulong>();
                using (var connection = new SQLiteConnection($"Data Source={filePath};Version=3;"))
                {
                    await connection.OpenAsync();
                    using (var command = new SQLiteCommand("SELECT id FROM datas", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                cardIds.Add((ulong)reader.GetInt64(0));
                        }
                    }
                }

                if (isDuplicate)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ForbiddenCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return cardIds.Contains(card.id);
                        };
                        LimitedCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return cardIds.Contains(card.id);
                        };
                        SemiLimitedCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return cardIds.Contains(card.id);
                        };
                        UnLimitedCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return cardIds.Contains(card.id);
                        };
                    });
                }
                else
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ForbiddenCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return !cardIds.Contains(card.id);
                        };
                        LimitedCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return !cardIds.Contains(card.id);
                        };
                        SemiLimitedCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return !cardIds.Contains(card.id);
                        };
                        UnLimitedCardsView.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return !cardIds.Contains(card.id);
                        };
                    });
                }

                int totalFilted =
                    ForbiddenCardsView.Cast<CardEditor.Models.Card>().Count() +
                    LimitedCardsView.Cast<CardEditor.Models.Card>().Count() +
                    SemiLimitedCardsView.Cast<CardEditor.Models.Card>().Count() +
                    UnLimitedCardsView.Cast<CardEditor.Models.Card>().Count();

                int totalCards =
                    ForbiddenCards.Count +
                    LimitedCards.Count +
                    SemiLimitedCards.Count +
                    UnLimitedCards.Count;

                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.filterSuc.ToText(), totalFilted.ToString(), totalCards.ToString()),
                    new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }


        public async Task<ResultItem> FilterCardByCDBFile(string filePath, bool isDuplicate)
        {
            int totalCards =
                ForbiddenCards.Count +
                LimitedCards.Count +
                SemiLimitedCards.Count +
                UnLimitedCards.Count;

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return new ResultItem
                {
                    Succeeded = false,
                    FilteredCount = 0,
                    TotalCount = totalCards,
                    Message = CMess.noFileFound.ToText()
                };
            }

            if (ForbiddenCards == null || !ForbiddenCards.Any() ||
                LimitedCards == null || !LimitedCards.Any() ||
                SemiLimitedCards == null || !SemiLimitedCards.Any() ||
                UnLimitedCards == null || !UnLimitedCards.Any())
            {
                return new ResultItem
                {
                    Succeeded = false,
                    FilteredCount = 0,
                    TotalCount = 0,
                    Message = CMess.noCardFilter.ToText()
                };
            }

            try
            {
                var cardIds = new HashSet<ulong>();
                using (var connection = new SQLiteConnection($"Data Source={filePath};Version=3;"))
                {
                    await connection.OpenAsync();
                    using (var command = new SQLiteCommand("SELECT id FROM datas", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                                cardIds.Add((ulong)reader.GetInt64(0));
                        }
                    }
                }
                return FilterCardByListID(cardIds, isDuplicate);
            }
            catch (Exception ex)
            {
                return new ResultItem
                {
                    Succeeded = false,
                    FilteredCount = 0,
                    TotalCount = totalCards,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResultItem> FilterCardByYDKFile(string filePath, bool isDuplicate)
        {
            int totalCards =
                ForbiddenCards.Count +
                LimitedCards.Count +
                SemiLimitedCards.Count +
                UnLimitedCards.Count;

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return new ResultItem
                {
                    Succeeded = false,
                    FilteredCount = 0,
                    TotalCount = totalCards,
                    Message = CMess.noFileFound.ToText()
                };
            }

            if (ForbiddenCards == null || !ForbiddenCards.Any() ||
                LimitedCards == null || !LimitedCards.Any() ||
                SemiLimitedCards == null || !SemiLimitedCards.Any() ||
                UnLimitedCards == null || !UnLimitedCards.Any())
            {
                return new ResultItem
                {
                    Succeeded = false,
                    FilteredCount = 0,
                    TotalCount = 0,
                    Message = CMess.noCardFilter.ToText()
                };
            }

            try
            {
                var cardIds = new HashSet<ulong>();
                const int bufferSize = 4096;

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true))
                using (var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false))
                {
                    string line;
                    while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line)) continue;
                        if (!Regex.IsMatch(line, @"^\d+$")) continue;

                        if (ulong.TryParse(line, out ulong id))
                        {
                            cardIds.Add(id);
                        }
                    }
                }

                return FilterCardByListID(cardIds, isDuplicate);
            }
            catch (Exception ex)
            {
                return new ResultItem
                {
                    Succeeded = false,
                    FilteredCount = 0,
                    TotalCount = totalCards,
                    Message = ex.Message
                };
            }
        }
        private ResultItem FilterCardByListID(HashSet<ulong> ids, bool isDuplicate)
        {
            int totalCards =
                ForbiddenCards.Count +
                LimitedCards.Count +
                SemiLimitedCards.Count +
                UnLimitedCards.Count;

            try
            {
                ClearFilterCard();

                ForbiddenCardsView.Filter = item =>
                {
                    var card = (CardEditor.Models.Card)item;
                    return isDuplicate == ids.Contains(card.id);
                };
                LimitedCardsView.Filter = item =>
                {
                    var card = (CardEditor.Models.Card)item;
                    return isDuplicate == ids.Contains(card.id);
                };
                SemiLimitedCardsView.Filter = item =>
                {
                    var card = (CardEditor.Models.Card)item;
                    return isDuplicate == ids.Contains(card.id);
                };
                UnLimitedCardsView.Filter = item =>
                {
                    var card = (CardEditor.Models.Card)item;
                    return isDuplicate == ids.Contains(card.id);
                };

                int totalFilted =
                    ForbiddenCardsView.Cast<CardEditor.Models.Card>().Count() +
                    LimitedCardsView.Cast<CardEditor.Models.Card>().Count() +
                    SemiLimitedCardsView.Cast<CardEditor.Models.Card>().Count() +
                    UnLimitedCardsView.Cast<CardEditor.Models.Card>().Count();

                return new ResultItem
                {
                    Succeeded = true,
                    FilteredCount = totalFilted,
                    TotalCount = totalCards,
                    Message = string.Empty
                };
            }
            catch (Exception ex)
            {
                return new ResultItem
                {
                    Succeeded = false,
                    FilteredCount = 0,
                    TotalCount = totalCards,
                    Message = ex.Message
                };
            }
        }
        private void ClearFilterCard()
        {
            ForbiddenCardsView.Filter = null;
            LimitedCardsView.Filter = null;
            SemiLimitedCardsView.Filter = null;
            UnLimitedCardsView.Filter = null;
            ForbiddenCardsView.Refresh();
            LimitedCardsView.Refresh();
            SemiLimitedCardsView.Refresh();
            UnLimitedCardsView.Refresh();
        }

        #endregion

        #region Open
        private void ViewImage()
        {
            if (SelectedCard == null) return;
            string imagePath = CardImageCacheViewModel.Instance.GetImagePath(SelectedCard.Id);

            if (!string.IsNullOrWhiteSpace(imagePath) && System.IO.File.Exists(imagePath))
            {
                if (MainWindowService != null)
                {
                    MainWindowService.OpenViewImage(imagePath);
                }
            }
            else
            {
                ///
            }
        }
        private void OpenFileLocation()
        {
            if (SelectedCard == null) return;
            string imagePath = CardImageCacheViewModel.Instance.GetImagePath(SelectedCard.Id);
            if (!string.IsNullOrWhiteSpace(imagePath) && System.IO.File.Exists(imagePath))
            {
                if (MainWindowService != null)
                {
                    MainWindowService.OpenFileLocation(imagePath);
                }
            }
        }
        private async Task OpenDataBase()
        {
            if (SelectedCard == null || MainWindowService == null) return;
            await MainWindowService.OpenDataEditorTab(SelectedCard.Id);
        }
        private async Task OpenScript()
        {
            if (SelectedCard == null || MainWindowService == null) return;
            await MainWindowService.OpenCodeEditorTab(SelectedCard.Id);
        }
        private bool SelectedCardExist()
        {
            return SelectedCard != null;
        }
        #endregion

        #region WWeb
        private async Task OpenKonamiDB()
        {
            if (SelectedCard == null) return;
            if (MainWindowService != null)
            {
                await MainWindowService.OpenKonamiDB(SelectedCard.Id, SelectedCard.Name);
            }
        }
        private async Task OpenYugipedia()
        {
            if (SelectedCard == null) return;
            if (MainWindowService != null)
            {
                await MainWindowService.OpenYugipedia(SelectedCard.Id, SelectedCard.Name);
            }
        }
        private async Task OpenYGOResources()
        {
            if (SelectedCard == null) return;
            if (MainWindowService != null)
            {
                await MainWindowService.OpenYGOResources(SelectedCard.Id, SelectedCard.Name);
            }
        }
        #endregion

        #region Change
        private void LoadCardList()
        {
            if (SelectedBanList == null)
            {
                BanListName = string.Empty;
                WhiteList = false;
                NewFileName = string.Empty;
                NewFolderPath = string.Empty;

                ForbiddenCards?.Clear();
                LimitedCards?.Clear();
                SemiLimitedCards?.Clear();
                UnLimitedCards?.Clear();
                IsSaved = true;
                return;
            }
            if (SelectedBanList?.CardList == null)
            {
                IsSaved = true;
                return;
            }

            var (result, message, CardList) = GetListCard(SelectedBanList.CardList);
            if (result)
            {
                BanListName = SelectedBanList.Name;
                WhiteList = SelectedBanList.WhiteList;
                NewFileName = SelectedBanList.FileName;
                NewFolderPath = System.IO.Path.GetDirectoryName(SelectedBanList.FilePath);

                if (CardList.Forbidden != null) ForbiddenCards?.ReplaceAll(CardList.Forbidden);
                if (CardList.Limited != null) LimitedCards?.ReplaceAll(CardList.Limited);
                if (CardList.SemiLimited != null) SemiLimitedCards?.ReplaceAll(CardList.SemiLimited);
                if (CardList.Unlimited != null) UnLimitedCards?.ReplaceAll(CardList.Unlimited);
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
            }
            IsSaved = true;
        }
        private (bool Result, string message, CardListGroup Groups) GetListCard(Dictionary<ulong, CardBanList> cardList)
        {
            if (cardList == null) return (false, "No Card Found", null);
            try
            {
                List<CardBanList> forbiddenList = new List<CardBanList>();
                List<CardBanList> limitedList = new List<CardBanList>();
                List<CardBanList> semiLimitedList = new List<CardBanList>();
                List<CardBanList> unLimitedList = new List<CardBanList>();

                foreach (var card in cardList.Values)
                {
                    if (card.LimitedCount <= 0) forbiddenList.Add(card);
                    else if (card.LimitedCount == 1) limitedList.Add(card);
                    else if (card.LimitedCount == 2) semiLimitedList.Add(card);
                    else unLimitedList.Add(card);
                }

                return (true, string.Empty, new CardListGroup(forbiddenList, limitedList, semiLimitedList, unLimitedList));
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
        private void datagr_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            IsSaved = false;
        }
        private void datagr_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            IsSaved = false;
        }
        private void datagr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInternalUpdate) return;

            try
            {
                bool isMultiSelectMode = Keyboard.IsKeyDown(Key.LeftCtrl) ||
                             Keyboard.IsKeyDown(Key.RightCtrl) ||
                             Keyboard.IsKeyDown(Key.LeftShift) ||
                             Keyboard.IsKeyDown(Key.RightShift);

                if (isMultiSelectMode)
                {
                    SelectedCard = null;
                    SelectedCards.Clear();

                    var SelectedCardsBanned = datagrBanned.SelectedItems.Cast<CardBanList>();
                    var SelectedCardsLimited = datagrLimited.SelectedItems.Cast<CardBanList>();
                    var SelectedCardsSemiLimited = datagrSemiLimited.SelectedItems.Cast<CardBanList>();
                    var SelectedCardsUnLimited = datagrUnLimited.SelectedItems.Cast<CardBanList>();

                    SelectedCards.AddRange(SelectedCardsBanned);
                    SelectedCards.AddRange(SelectedCardsLimited);
                    SelectedCards.AddRange(SelectedCardsSemiLimited);
                    SelectedCards.AddRange(SelectedCardsUnLimited);
                }
                else
                {
                    SelectedCards.Clear();
                    var currentDataGrid = sender as DataGrid;
                    if (currentDataGrid == null)
                    {
                        _isInternalUpdate = true;
                        datagrBanned.SelectedItem = null;
                        datagrLimited.SelectedItem = null;
                        datagrSemiLimited.SelectedItem = null;
                        datagrUnLimited.SelectedItem = null;
                        _isInternalUpdate = false;

                        SelectedCard = null;
                    }
                    else
                    {
                        _isInternalUpdate = true;
                        if (currentDataGrid != datagrBanned)
                            datagrBanned.SelectedItem = null;
                        if (currentDataGrid != datagrLimited)
                            datagrLimited.SelectedItem = null;
                        if (currentDataGrid != datagrSemiLimited)
                            datagrSemiLimited.SelectedItem = null;
                        if (currentDataGrid != datagrUnLimited)
                            datagrUnLimited.SelectedItem = null;
                        _isInternalUpdate = false;

                        SelectedCard = currentDataGrid.SelectedItem as CardBanList;
                        if (SelectedCard != null)
                            SelectedCards.Add(SelectedCard);
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private void OnCardsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsSaved = false;
            SaveBanListCommand?.RaiseCanExecuteChanged();
            SortCardCommand?.RaiseCanExecuteChanged();
            RefreshCardCommand?.RaiseCanExecuteChanged();
        }
        private async void OnSelectedCardChanged()
        {
            if (SelectedCard == null)
            {
                CardIDString = string.Empty;
                CardName = string.Empty;
                LimitedCount = -1;
                imagecard.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Blank.png", UriKind.Absolute));
                ViewportImage.ToolTip = CMess.toolCardImg.ToText();
            }
            else
            {
                CardIDString = SelectedCard.Id.ToString();
                CardName = SelectedCard.Name;
                LimitedCount = SelectedCard.LimitedCount;
                await LoadCardImage();
            }
        }
        private async Task LoadCardImage()
        {
            try
            {
                string imagePath = await Task.Run(() => FindIInfoService.FindImagePath(SelectedCard.Id.ToString()));
                
                if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                {
                    byte[] imgBytes = await Task.Run(() => System.IO.File.ReadAllBytes(imagePath));
                    BitmapImage bitmap = new BitmapImage();
                    using (var ms = new MemoryStream(imgBytes))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        imagecard.Source = bitmap;
                        ViewportImage.ToolTip = imagePath;
                        ImageUrl = imagePath;
                    });
                }
                else
                {
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        imagecard.Source = ImageCacheService.Instance.Get(AppImage.Blank);
                        ViewportImage.ToolTip = CMess.toolCardImg.ToText();
                        ImageUrl = null;
                    });
                }
            }
            catch
            {
                ///
            }
        }

        private void UpdateWindowTitle()
        {
            if (MainWindowService != null)
            {
                MainWindowService.UpdateWindowTitle(CurrentBanListPath);
                MainWindowService.UpdateTabItemHeader(CurrentBanListName);
                MainWindowTitle = CurrentBanListPath;
            }
        }
        private void UpdateWindowSavedFlag()
        {
            if (MainWindowService != null)
            {
                MainWindowService.UpdateWindowSavedFlag(IsSaved);
            }
        }
        #endregion

        #region Delete
        private void menuDeleteBanned_Click(object sender, RoutedEventArgs e)
        {
            var selectedCards = datagrBanned.SelectedItems.Cast<CardBanList>().ToList();
            if (selectedCards.Count == 0) return;

            if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmDelete)
            {
                int result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    $"{string.Format(CMess.confirmDelete.ToText(), CMess.SelectCards.ToText())} ({selectedCards.Count} {CMess.Card.ToText()})",
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
            }
            ForbiddenCards.RemoveRange(selectedCards);
        }
        private void menuDeleteLimited_Click(object sender, RoutedEventArgs e)
        {
            var selectedCards = datagrLimited.SelectedItems.Cast<CardBanList>().ToList();
            if (selectedCards.Count == 0) return;

            if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmDelete)
            {
                int result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    $"{string.Format(CMess.confirmDelete.ToText(), CMess.SelectCards.ToText())} ({selectedCards.Count} {CMess.Card.ToText()})",
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
            }
            LimitedCards.RemoveRange(selectedCards);
        }
        private void menuDeleteSemi_Click(object sender, RoutedEventArgs e)
        {
            var selectedCards = datagrSemiLimited.SelectedItems.Cast<CardBanList>().ToList();
            if (selectedCards.Count == 0) return;

            if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmDelete)
            {
                int result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    $"{string.Format(CMess.confirmDelete.ToText(), CMess.SelectCards.ToText())} ({selectedCards.Count} {CMess.Card.ToText()})",
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
            }
            SemiLimitedCards.RemoveRange(selectedCards);
        }
        private void menuDeleteUnLimited_Click(object sender, RoutedEventArgs e)
        {
            var selectedCards = datagrUnLimited.SelectedItems.Cast<CardBanList>().ToList();
            if (selectedCards.Count == 0) return;

            if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmDelete)
            {
                int result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    $"{string.Format(CMess.confirmDelete.ToText(), CMess.SelectCards.ToText())} ({selectedCards.Count} {CMess.Card.ToText()})",
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
            }
            UnLimitedCards.RemoveRange(selectedCards);
        }
        #endregion

        #region Event
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }
        public void Dispose()
        {
            ForbiddenCards.Clear();
            LimitedCards.Clear();
            SemiLimitedCards.Clear();
            UnLimitedCards.Clear();

            ForbiddenCardsView.SortDescriptions.Clear();
            LimitedCardsView.SortDescriptions.Clear();
            SemiLimitedCardsView.SortDescriptions.Clear();
            UnLimitedCardsView.SortDescriptions.Clear();

            ForbiddenCards = null;
            LimitedCards = null;
            SemiLimitedCards = null;
            UnLimitedCards = null;

            SelectedBanList = null;
            SelectedCard = null;

            NewBanListCommand = null;
            BrowseCommand = null;
            SaveBanListCommand = null;
            DeleteBanListCommand = null;
            ClearBanListCommand = null;
            CreateNewBanListCommand = null;
            ReNameBanListCommand = null;
            SaveAsBanListCommand = null;
            AddCardCommand = null;
            ModifyCardCommand = null;
            UpLoadCardCommand = null;
            SortCardCommand = null;
            FilterCardCommand = null;
            RefreshCardCommand = null;
            ViewImageCommand = null;
            OpenFileImageCommand = null;
            OpenDatabaseCommand = null;
            OpenScriptCommand = null;

            _regexCache.Clear();
            _regexCache = null;

            MessageNotifi?.Dispose();


        }
        #endregion

    }
}
