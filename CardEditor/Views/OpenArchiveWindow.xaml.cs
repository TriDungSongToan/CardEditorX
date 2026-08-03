using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.ComponentModel;
using MaterialDesignThemes.Wpf;
using CardEditor.Helpers;
using CardEditor.Commands;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Views
{
    /// <summary>
    /// Interaction logic for OpenArchiveWindow.xaml
    /// </summary>
    public partial class OpenArchiveWindow : Window, INotifyPropertyChanged, IDisposable
    {
        #region Propertys

        #region Header
        private int _selectedIndex = -1;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged(nameof(SelectedIndex));

                    if (SelectedIndex == 0) SelectedTabName = CMess.CardDB.ToText();
                    else if (SelectedIndex == 1) SelectedTabName = CMess.CardScript.ToText();
                    else if (SelectedIndex == 2) SelectedTabName = CMess.Deck.ToText();
                    else if (SelectedIndex == 3) SelectedTabName = CMess.BanList.ToText();
                    else SelectedTabName = string.Empty;
                }
            }
        }
        private string _selectedTabName = string.Empty;
        public string SelectedTabName
        {
            get => _selectedTabName;
            set
            {
                if (_selectedTabName != value)
                {
                    _selectedTabName = value;
                    OnPropertyChanged(nameof(SelectedTabName));
                }
            }
        }
        public RelayCommand CloseCommand { get; set; }
        #endregion

        #region Archive FilePath
        private string _archiveFilePath = string.Empty;
        public string ArchiveFilePath
        {
            get => _archiveFilePath;
            set
            {
                if (_archiveFilePath != value)
                {
                    _archiveFilePath = value;
                    OnPropertyChanged(nameof(ArchiveFilePath));
                    LoadArchiveFileCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        public RelayCommand BrowserArchiveFilePathCommand { get; set; }
        public RelayCommand LoadArchiveFileCommand { get; set; }
        #endregion

        #region Card Database
        public BulkObservableCollection<string> ArchiveDatabaseEntrys { get; set; } = new();
        public BulkObservableCollection<string> SelectedDatabaseEntrys { get; set; } = new();
        public ICollectionView DatabaseEntrysView { get; set; }
        private string _searchDatabase = string.Empty;
        public string SearchDatabase
        {
            get => _searchDatabase;
            set
            {
                if (_searchDatabase != value)
                {
                    _searchDatabase = value;
                    OnPropertyChanged(nameof(SearchDatabase));
                    OnSearchDatabaseChanged();
                }
            }
        }
        private int _searchDatabaseResult = 0;
        public int SearchDatabaseResult
        {
            get => _searchDatabaseResult;
            set
            {
                if (_searchDatabaseResult != value)
                {
                    _searchDatabaseResult = value;
                    OnPropertyChanged(nameof(SearchDatabaseResult));
                }
            }
        }
        public RelayCommand OpenDatabaseCommand { get; set; }
        #endregion

        #region Card Script
        public BulkObservableCollection<string> ArchiveScriptEntrys { get; set; } = new();
        public BulkObservableCollection<string> SelectedScriptEntrys { get; set; } = new();
        public ICollectionView ScriptEntrysView { get; set; }
        private string _searchScript = string.Empty;
        public string SearchScript
        {
            get => _searchScript;
            set
            {
                if (_searchScript != value)
                {
                    _searchScript = value;
                    OnPropertyChanged(nameof(SearchScript));
                    OnSearchScriptChanged();
                }
            }
        }
        private int _searchScriptResult = 0;
        public int SearchScriptResult
        {
            get => _searchScriptResult;
            set
            {
                if (_searchScriptResult != value)
                {
                    _searchScriptResult = value;
                    OnPropertyChanged(nameof(SearchScriptResult));
                }
            }
        }
        public RelayCommand OpenScriptCommand { get; set; }
        #endregion

        #region Deck
        public BulkObservableCollection<string> ArchiveDeckEntrys { get; set; } = new();
        public BulkObservableCollection<string> SelectedDeckEntrys { get; set; } = new();
        public ICollectionView DeckEntrysView { get; set; }
        private string _searchDeck = string.Empty;
        public string SearchDeck
        {
            get => _searchDeck;
            set
            {
                if (_searchDeck != value)
                {
                    _searchDeck = value;
                    OnPropertyChanged(nameof(SearchDeck));
                    OnSearchDeckChanged();
                }
            }
        }
        private int _searchDeckResult = 0;
        public int SearchDeckResult
        {
            get => _searchDeckResult;
            set
            {
                if (_searchDeckResult != value)
                {
                    _searchDeckResult = value;
                    OnPropertyChanged(nameof(SearchDeckResult));
                }
            }
        }
        public RelayCommand OpenDeckCommand { get; set; }
        #endregion

        #region BanList
        public BulkObservableCollection<string> ArchiveBanListEntrys { get; set; } = new();
        public BulkObservableCollection<string> SelectedBanListEntrys { get; set; } = new();
        public ICollectionView BanListEntrysView { get; set; }

        private string _searchBanlist = string.Empty;
        public string SearchBanlist
        {
            get => _searchBanlist;
            set
            {
                if (_searchBanlist != value)
                {
                    _searchBanlist = value;
                    OnPropertyChanged(nameof(SearchBanlist));
                    OnSearchBanListChanged();
                }
            }
        }
        private int _searchBanlistResult = 0;
        public int SearchBanlistResult
        {
            get => _searchBanlistResult;
            set
            {
                if (_searchBanlistResult != value)
                {
                    _searchBanlistResult = value;
                    OnPropertyChanged(nameof(SearchBanlistResult));
                }
            }
        }
        public RelayCommand OpenBanListCommand { get; set; }
        #endregion

        public MainWindow MainWindowReference { get; set; }
        public SnackbarMessageQueue MessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
        #endregion

        #region Constructor
        public OpenArchiveWindow(string archiveFilePath = "")
        {
            InitializeComponent();
            InitializeView();
            InitializeCommand();
            InitializeEvent();
            this.DataContext = this;

            ArchiveFilePath = archiveFilePath;
        }
        private void InitializeView()
        {
            DatabaseEntrysView = CollectionViewSource.GetDefaultView(ArchiveDatabaseEntrys);
            ScriptEntrysView = CollectionViewSource.GetDefaultView(ArchiveScriptEntrys);
            DeckEntrysView = CollectionViewSource.GetDefaultView(ArchiveDeckEntrys);
            BanListEntrysView = CollectionViewSource.GetDefaultView(ArchiveBanListEntrys);
        }
        private void InitializeCommand()
        {
            BrowserArchiveFilePathCommand = new CardEditor.Commands.RelayCommand(_ => BrowserArchiveFilePath());
            LoadArchiveFileCommand = new CardEditor.Commands.RelayCommand(_ => LoadArchiveFile(), _ => CanLoadArchiveFile());

            OpenDatabaseCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenDatabase(), _ => CanOpenDatabase());
            OpenScriptCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenScript(), _ => CanOpenScript());
            OpenDeckCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenDeck(), _ => CanOpenDeck());
            OpenBanListCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenBanList(), _ => CanOpenBanList());

            CloseCommand = new CardEditor.Commands.RelayCommand(_ => this.Close());
        }
        private void InitializeEvent()
        {
            SelectedDatabaseEntrys.CollectionChanged += SelectedDatabaseEntrys_CollectionChanged;
            SelectedScriptEntrys.CollectionChanged += SelectedScriptEntrys_CollectionChanged;
            SelectedDeckEntrys.CollectionChanged += SelectedDeckEntrys_CollectionChanged;
            SelectedBanListEntrys.CollectionChanged += SelectedBanListEntrys_CollectionChanged;
        }
        #endregion

        #region Load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeContentMenu();
        }
        private void InitializeContentMenu()
        {
            ControlContextMenuService.Attach(txtArchiveFilePath);
            ControlContextMenuService.Attach(txtSearchDatabase);
            ControlContextMenuService.Attach(txtSearchScript);
            ControlContextMenuService.Attach(txtSearchDeck);
            ControlContextMenuService.Attach(txtSearchBanlist);
        }
        #endregion

        #region Commands

        #region Archive FilePath
        private void BrowserArchiveFilePath()
        {
            string filePath = FileDiaLogHelper.OpenCardPack();
            if (string.IsNullOrEmpty(filePath)) return;
            ArchiveFilePath = filePath;
        }
        public async Task LoadArchiveFile()
        {
            if (string.IsNullOrWhiteSpace(ArchiveFilePath) || !System.IO.File.Exists(ArchiveFilePath)) return;

            var resultLoad = await LoadDataServices.ListsEntries(ArchiveFilePath);
            if (resultLoad.success)
            {
                ArchiveDatabaseEntrys.ReplaceAll(resultLoad.fileLists.DatabaseEntries);
                ArchiveScriptEntrys.ReplaceAll(resultLoad.fileLists.ScriptEntries);
                ArchiveDeckEntrys.ReplaceAll(resultLoad.fileLists.DeckEntries);
                ArchiveBanListEntrys.ReplaceAll(resultLoad.fileLists.BanlistEntries);

                MessageQueue.Enqueue(string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.load.ToText(), CMess.CardArchive.ToText()));
                MainWindowReference.UpdateArchiveRecentItem(ArchiveFilePath);
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    string.Format(CMess.TwoPlaceholderError.ToText(), CMess.load.ToText(), CMess.CardArchive.ToText()), new[] { CMess.ok.ToText() });
            }
        }
        private bool CanLoadArchiveFile()
        {
            return (!string.IsNullOrWhiteSpace(ArchiveFilePath) && System.IO.File.Exists(ArchiveFilePath));
        }
        #endregion

        #region Database
        private CancellationTokenSource _searchDatabaseCts;
        private async void OnSearchDatabaseChanged()
        {
            _searchDatabaseCts?.Cancel();
            _searchDatabaseCts?.Dispose();

            _searchDatabaseCts = new CancellationTokenSource();
            CancellationToken token = _searchDatabaseCts.Token;

            try
            {
                await Task.Delay(200, token);
                if (token.IsCancellationRequested) return;

                SearchDatabaseFunction();
            }
            catch (TaskCanceledException)
            {
                ///
            }
            catch (Exception)
            {
                ///
            }
        }
        private void SearchDatabaseFunction()
        {
            if (DatabaseEntrysView == null)  return;

            string keyword = SearchDatabase?.Trim();

            DatabaseEntrysView.Filter = item =>
            {
                if (item is not string text) return false;

                // Không nhập gì thì hiển thị tất cả
                if (string.IsNullOrWhiteSpace(keyword)) return true;

                return text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
            };

            DatabaseEntrysView?.Refresh();
        }
        private async Task OpenDatabase()
        {
            if (string.IsNullOrWhiteSpace(ArchiveFilePath) || !System.IO.File.Exists(ArchiveFilePath))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.fileNotExit.ToText(), new[] { CMess.ok.ToText() });
                return;
            }
            if (SelectedDatabaseEntrys.Count == 0)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.noFileFound.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            var (result, message) = await MainWindowReference.OpenArchiveDatabase(ArchiveFilePath, SelectedDatabaseEntrys);
            if (result)
            {
                MessageQueue.Enqueue(string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Open.ToText(), CMess.CardDB.ToText()));
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.CardDB.ToText())} {message}",
                    new[] { CMess.ok.ToText() });
            }
        }
        private bool CanOpenDatabase()
        {
            return SelectedDatabaseEntrys == null ? false : SelectedDatabaseEntrys.Any();
        }
        #endregion

        #region Script
        private CancellationTokenSource _searchScriptCts;
        private async void OnSearchScriptChanged()
        {
            _searchScriptCts?.Cancel();
            _searchScriptCts?.Dispose();

            _searchScriptCts = new CancellationTokenSource();
            CancellationToken token = _searchScriptCts.Token;

            try
            {
                await Task.Delay(200, token);
                if (token.IsCancellationRequested) return;

                SearchScriptFunction();
            }
            catch (TaskCanceledException)
            {
                ///
            }
            catch (Exception)
            {
                ///
            }
        }
        private void SearchScriptFunction()
        {
            if (ScriptEntrysView == null) return;

            string keyWord = SearchScript?.Trim();

            ScriptEntrysView.Filter = item =>
            {
                if (item is not string text) return false;

                if (string.IsNullOrWhiteSpace(keyWord)) return true;

                return text.IndexOf(keyWord, StringComparison.OrdinalIgnoreCase) >= 0;
            };

            ScriptEntrysView?.Refresh();
        }
        private async Task OpenScript()
        {
            if (string.IsNullOrWhiteSpace(ArchiveFilePath) || !System.IO.File.Exists(ArchiveFilePath))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.fileNotExit.ToText(), new[] { CMess.ok.ToText() });
                return;
            }
            if (SelectedScriptEntrys.Count == 0)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.noFileFound.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            var (result, message) = await MainWindowReference.OpenArchiveScript(ArchiveFilePath, SelectedScriptEntrys);
            if (result)
            {
                MessageQueue.Enqueue(string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Open.ToText(), CMess.CardScript.ToText()));
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.CardScript.ToText())} {message}",
                    new[] { CMess.ok.ToText() });
            }
        }
        private bool CanOpenScript()
        {
            return SelectedScriptEntrys == null ? false : SelectedScriptEntrys.Any();
        }
        #endregion

        #region Deck
        private CancellationTokenSource _searchDeckCts;
        private async void OnSearchDeckChanged()
        {
            _searchDeckCts?.Cancel();
            _searchDeckCts?.Dispose();

            _searchDeckCts = new CancellationTokenSource();
            CancellationToken token = _searchDeckCts.Token;

            try
            {
                await Task.Delay(200, token);
                if (token.IsCancellationRequested) return;

                SearchDeckFzunction();
            }
            catch (TaskCanceledException)
            {
                ///
            }
            catch (Exception)
            {
                ///
            }
        }
        private void SearchDeckFzunction()
        {
            if (DeckEntrysView == null) return;

            string keyWord = SearchDeck?.Trim();

            DeckEntrysView.Filter = item =>
            {
                if (item is not string text) return false;

                if (string.IsNullOrWhiteSpace(keyWord)) return true;

                return text.IndexOf(keyWord, StringComparison.OrdinalIgnoreCase) >= 0;
            };

            DeckEntrysView?.Refresh();
        }
        private async Task OpenDeck()
        {
            if (string.IsNullOrWhiteSpace(ArchiveFilePath) || !System.IO.File.Exists(ArchiveFilePath))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.fileNotExit.ToText(), new[] { CMess.ok.ToText() });
                return;
            }
            if (SelectedDeckEntrys.Count == 0)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.noFileFound.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            #region Load
            try
            {
                if (!BanListRawDataViewModel.Instance.IsLoaded)
                {
                    BanListRawDataViewModel.Instance.LoadLimitImages();
                    var (resultban, messageban) = await BanListRawDataViewModel.Instance.LoadBanLists();
                    if (!resultban)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {messageban}", new[] { CMess.ok.ToText() });
                    }
                }
                await CardEXDataViewModel.Instance.LoadCardsEXAsync(); // Deck
            }
            catch //(Exception ex)
            {
                //CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            #endregion

            var (result, message) = await MainWindowReference.OpenArchiveDeck(ArchiveFilePath, SelectedDeckEntrys);
            if (result)
            {
                MessageQueue.Enqueue(string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Open.ToText(), CMess.Deck.ToText()));
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.Deck.ToText())} {message}",
                    new[] { CMess.ok.ToText() });
            }
        }
        private bool CanOpenDeck()
        {
            return SelectedDeckEntrys == null ? false : SelectedDeckEntrys.Any();
        }
        #endregion

        #region BanList
        private CancellationTokenSource _searchBanListCts;
        private async void OnSearchBanListChanged()
        {
            _searchBanListCts?.Cancel();
            _searchBanListCts?.Dispose();

            _searchBanListCts = new CancellationTokenSource();
            CancellationToken token = _searchBanListCts.Token;

            try
            {
                await Task.Delay(200, token);
                if (token.IsCancellationRequested) return;

                SearchBanListFunction();
            }
            catch (TaskCanceledException)
            {
                ///
            }
            catch (Exception)
            {
                ///
            }
        }
        private void SearchBanListFunction()
        {
            if (BanListEntrysView == null) return;

            string keyWord = SearchBanlist?.Trim();

            BanListEntrysView.Filter = item =>
            {
                if (item is not string text) return false;

                if (string.IsNullOrWhiteSpace(keyWord)) return true;

                return text.IndexOf(keyWord, StringComparison.OrdinalIgnoreCase) >= 0;
            };

            BanListEntrysView?.Refresh();
        }
        private async Task OpenBanList()
        {
            if (string.IsNullOrWhiteSpace(ArchiveFilePath) || !System.IO.File.Exists(ArchiveFilePath))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.fileNotExit.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            if (SelectedBanListEntrys.Count == 0)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.noFileFound.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            #region Load
            if (!BanListRawDataViewModel.Instance.IsLoaded)
            {
                var (resultban, messageban) = await BanListRawDataViewModel.Instance.LoadBanLists();
                if (!resultban)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {resultban}", new[] { CMess.ok.ToText() });
                }
            }
            #endregion

            var (result, message) = await MainWindowReference.OpenArchiveBanList(ArchiveFilePath, SelectedBanListEntrys);
            if (result)
            {
                MessageQueue.Enqueue(string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Open.ToText(), CMess.BanList.ToText()));
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.BanList.ToText())} {message}",
                    new[] { CMess.ok.ToText() });
            }
        }
        private bool CanOpenBanList()
        {
            return SelectedBanListEntrys == null ? false : SelectedBanListEntrys.Any();
        }
        #endregion

        #endregion

        #region Event
        private void SelectedDatabaseEntrys_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OpenDatabaseCommand?.RaiseCanExecuteChanged();
        }
        private void SelectedScriptEntrys_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OpenScriptCommand?.RaiseCanExecuteChanged();
        }
        private void SelectedDeckEntrys_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OpenDeckCommand?.RaiseCanExecuteChanged();
        }
        private void SelectedBanListEntrys_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OpenBanListCommand?.RaiseCanExecuteChanged();
        }

        private void blHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        #endregion

        #region IDisposable
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Dispose();
        }
        public void Dispose()
        {
            SelectedDatabaseEntrys.CollectionChanged -= SelectedDatabaseEntrys_CollectionChanged;
            SelectedScriptEntrys.CollectionChanged -= SelectedScriptEntrys_CollectionChanged;
            SelectedDeckEntrys.CollectionChanged -= SelectedDeckEntrys_CollectionChanged;
            SelectedBanListEntrys.CollectionChanged -= SelectedBanListEntrys_CollectionChanged;

            if (DatabaseEntrysView != null)
            {
                DatabaseEntrysView.Filter = null;
                DatabaseEntrysView.SortDescriptions.Clear();
                DatabaseEntrysView.GroupDescriptions.Clear();
                DatabaseEntrysView = null;
            }
            if (ScriptEntrysView != null)
            {
                ScriptEntrysView.Filter = null;
                ScriptEntrysView.SortDescriptions.Clear();
                ScriptEntrysView.GroupDescriptions.Clear();
                ScriptEntrysView = null;
            }
            if (DeckEntrysView != null)
            {
                DeckEntrysView.Filter = null;
                DeckEntrysView.SortDescriptions.Clear();
                DeckEntrysView.GroupDescriptions.Clear();
                DeckEntrysView = null;
            }
            if (BanListEntrysView != null)
            {
                BanListEntrysView.Filter = null;
                BanListEntrysView.SortDescriptions.Clear();
                BanListEntrysView.GroupDescriptions.Clear();
                BanListEntrysView = null;
            }

            SelectedDatabaseEntrys?.Clear();
            SelectedScriptEntrys?.Clear();
            SelectedDeckEntrys?.Clear();
            SelectedBanListEntrys?.Clear();
            SelectedDatabaseEntrys = null;
            SelectedScriptEntrys = null;
            SelectedDeckEntrys = null;
            SelectedBanListEntrys = null;

            ArchiveDatabaseEntrys?.Clear();
            ArchiveScriptEntrys?.Clear();
            ArchiveDeckEntrys?.Clear();
            ArchiveBanListEntrys?.Clear();
            ArchiveDatabaseEntrys = null;
            ArchiveScriptEntrys = null;
            ArchiveDeckEntrys = null;
            ArchiveBanListEntrys = null;

            BrowserArchiveFilePathCommand = null;
            LoadArchiveFileCommand = null;
            OpenDatabaseCommand = null;
            OpenScriptCommand = null;
            OpenDeckCommand = null;
            OpenBanListCommand = null;
        }
        #endregion

    }
}