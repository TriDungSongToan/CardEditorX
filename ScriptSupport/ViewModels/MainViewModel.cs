using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Scrapiyard.Core.Models;
using ScriptSupport.Models;
using ScriptSupport.States;
using ScriptSupport.Stores;
using ScriptSupport.Helpers;
using ScriptSupport.Manager;
using ScriptSupport.Commands;
using ScriptSupport.Services;
using ScriptSupport.Interfaces;
using ScriptSupport.Collections;
using ScriptSupport.Environment;
using ScriptSupport.Localization;
using CMess = ScriptSupport.Localization.Language;

namespace ScriptSupport.ViewModels
{
    public class MainViewModel : BaseViewModel, IDisposable
    {
        #region Fields
        public UIConfigState UIConfig { get; }
        public EditorCommandsService EditorCommands { get; }
        private readonly AppEnvironment _appEnvironment;
        private readonly ConfigStore _configStore;
        private readonly IApplicationInterface _appService;
        private readonly IConfigInterface _configInterface;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHelpInterface _helpService;
        private readonly IDialogInterface _dialogService;
        private readonly ICardInterface _cardService;
        private readonly IScriptInterface _scriptService;
        private readonly IScrapiInterface _scrapiService;
        private readonly IScrapiyardInterface _scrapiyardService;
        private readonly IResultInterface _resultService;
        private readonly IDataFolderInterface _dataFolderInterface;
        private readonly IFloatingPanelInterface _panelService;
        private readonly ILauncherInterface _launcherService;
        private readonly FilterCardState _filterCardState;
        private readonly ResultState _resultState;


        private readonly DocumentManager _documentManager;
        public ObservableCollection<DocumentViewModel> Documents => _documentManager.Documents;
        public DocumentViewModel? ActiveDocument
        {
            get => _documentManager.ActiveDocument;
            set
            {
                _documentManager.ActiveDocument = value;
                OnPropertyChanged();
                RaiseSaveCanExecuteChanged();
            }
        }

        private CancellationTokenSource? _searchCardCts;
        private CancellationTokenSource? _selectFileCts;
        #endregion

        #region Properties

        #region Cards
        public BulkObservableCollection<CardText> Cards { get; } = new();
        private string _searchCard = string.Empty;
        public string SearchCard
        {
            get => _searchCard;
            set
            {
                if (SetProperty(ref _searchCard, value))
                {
                    OnSearchCardChanged();
                }
            }
        }
        private CardText? _selectedCard;
        public CardText? SelectedCard
        {
            get => _selectedCard;
            set
            {
                if (SetProperty(ref _selectedCard, value))
                {
                    if (value != null) _= HandleCardSingleClick(_selectedCard);
                }
            }
        }
        public int ResultCards => Cards?.Count ?? 0;
        #endregion

        #region Script
        public BulkObservableCollection<ScriptItem> ScriptItems { get; } = new();
        private string _searchScript = string.Empty;
        public string SearchScript
        {
            get => _searchScript;
            set
            {
                if (SetProperty(ref _searchScript, value))
                {
                    _ = OnSearchTextChangedAsync(value);
                }
            }
        }
        public ScriptItem? _selectedScriptItem;
        public ScriptItem? SelectedScriptItem
        {
            get => _selectedScriptItem;
            set
            {
                if (SetProperty(ref _selectedScriptItem, value))
                {
                    if (value != null) _= HandleScriptSingleClick(_selectedScriptItem);
                }
            }
        }
        public int ResultScripts => ScriptItems?.Count ?? 0;
        #endregion

        #region Open/New
        public BulkObservableCollection<FileItem> FileItemsNew { get; } = new();
        public BulkObservableCollection<FileItem> FileItemsOpen { get; } = new();

        private FileItem? _selectedFileItemsNew;
        public FileItem? SelectedFileItemsNew
        {
            get => _selectedFileItemsNew;
            set
            {
                if (SetProperty(ref _selectedFileItemsNew, value))
                {
                    if (value != null) _= HandleFileSingleClick(_selectedFileItemsNew);
                }
            }
        }
        private FileItem? _selectedFileItemsOpen;
        public FileItem? SelectedFileItemsOpen
        {
            get => _selectedFileItemsOpen;
            set
            {
                if (SetProperty(ref _selectedFileItemsOpen, value))
                {
                    if (value != null) _= HandleFileSingleClick(_selectedFileItemsOpen);
                }
            }
        }
        #endregion

        #region Scrapiyard

        #region Name
        public BulkObservableCollection<CompletionSymbol> ScrapiyardNameItems { get; } = new();
        private CompletionSymbol? _selectedScrapiyardNameItem;
        public CompletionSymbol? SelectedScrapiyardNameItem
        {
            get => _selectedScrapiyardNameItem;
            set
            {
                if (SetProperty(ref _selectedScrapiyardNameItem, value))
                {
                    if (value != null) _ = OnSelectedScrapiyardNameChanged(value);
                }
            }
        }
        private string _searchScrapiyardName = string.Empty;
        public string SearchScrapiyardName
        {
            get => _searchScrapiyardName;
            set
            {
                if (SetProperty(ref _searchScrapiyardName, value))
                {
                    _ = OnSearchScrapiyardNameChanged(value);
                }
            }
        }
        public int ResultScrapiyardName => ScrapiyardNameItems?.Count ?? 0;
        #endregion

        #region Desc
        public BulkObservableCollection<CompletionSymbol> ScrapiyardDescItems { get; } = new();
        private CompletionSymbol? _selectedScrapiyardDescItem;
        public CompletionSymbol? SelectedScrapiyardDescItem
        {
            get => _selectedScrapiyardDescItem;
            set
            {
                if (SetProperty(ref _selectedScrapiyardDescItem, value))
                {
                    if (value != null) _ = OnSelectedScrapiyardDescChanged(value);
                }
            }
        }
        private string _searchScrapiyardDesc = string.Empty;
        public string SearchScrapiyardDesc
        {
            get => _searchScrapiyardDesc;
            set
            {
                if (SetProperty(ref _searchScrapiyardDesc, value))
                {
                    _ = OnSearchScrapiyardDescChanged(value);
                }
            }
        }
        public int ResultScrapiyardDesc => ScrapiyardDescItems?.Count ?? 0;
        #endregion

        #endregion

        private bool _isOpenCardFilter = false;
        public bool IsOpenCardFilter
        {
            get => _isOpenCardFilter;
            set => SetProperty(ref _isOpenCardFilter, value);
        }
        #endregion

        #region Commands

        #region App Commands
        public RelayCommand NewFileCommand { get; set; } = null!;
        public RelayCommand OpenFileCommand { get; set; } = null!;
        public RelayCommand SaveFileCommand { get; set; } = null!;
        public RelayCommand SaveAsFileCommand { get; set; } = null!;
        public RelayCommand ExitCommand { get; set; } = null!;
        public RelayCommand SettingCommand { get; set; } = null!;
        public RelayCommand HelpCommand { get; set; } = null!;
        public RelayCommand CheckUpDateCommand { get; set; } = null!;
        public RelayCommand AboutCommand { get; set; } = null!;
        public RelayCommand SpecialCharCommand { get; set; } = null!;
        #endregion

        #region Commands
        public ICommand? DoubleClickCardCommand { get; set; }
        public ICommand? DoubleClickScriptCommand { get; set; }
        public ICommand? DoubleClickNewCommand { get; set; }
        public ICommand? DoubleClickOpenCommand { get; set; }
        public ICommand? DoubleClickScrapiyardNameCommand { get; set; }
        public ICommand? DoubleClickScrapiyardDescCommand { get; set; }
        #endregion

        public RelayCommand CloseCommand { get; set; } = null!;
        #endregion

        #region Constructor
        public MainViewModel(UIConfigState uIConfig, EditorCommandsService editorCommands, AppEnvironment appEnvironment, ConfigStore configStore,
            IApplicationInterface appService, IConfigInterface configInterface, IServiceProvider serviceProvider,
            IDialogInterface dialogInterface, IHelpInterface helpService,
            ICardInterface cardInterface, IScriptInterface scriptInterface,
            IScrapiInterface scrapiInterface, IScrapiyardInterface scrapiyardInterface,
            IResultInterface resultService, IDataFolderInterface dataFolderInterface,
            IFloatingPanelInterface panelService, ILauncherInterface launcherService,
            DocumentManager documents,
            FilterCardState filterCardState, ResultState resultState)
        {
            _appService = appService;
            UIConfig = uIConfig;
            EditorCommands = editorCommands;
            _appEnvironment = appEnvironment;
            _configStore = configStore;
            _configInterface = configInterface;
            _serviceProvider = serviceProvider;
            _dialogService = dialogInterface;
            _cardService = cardInterface;
            _helpService = helpService;
            _scriptService = scriptInterface;
            _scrapiService = scrapiInterface;
            _scrapiyardService = scrapiyardInterface;
            _resultService = resultService;
            _dataFolderInterface = dataFolderInterface;
            _panelService = panelService;
            _launcherService = launcherService;
            _documentManager = documents;
            _filterCardState = filterCardState;
            _resultState = resultState;

            InitializeCommand();

            _documentManager.PropertyChanged += _documentManager_PropertyChanged;
            _filterCardState.PropertyChanged += FilterCardState_PropertyChanged;
            Cards.CollectionChanged += Cards_CollectionChanged;
            ScriptItems.CollectionChanged += ScriptItems_CollectionChanged;

            _configStore.DataHandlingSetting.PropertyChanged += DataHandlingSetting_PropertyChanged;
            ScrapiyardNameItems.CollectionChanged += ScrapiyardNameItems_CollectionChanged;
            ScrapiyardDescItems.CollectionChanged += ScrapiyardDescItems_CollectionChanged;
        }

        private void DataHandlingSetting_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaiseCanOpenExecuteChanged();
            RaiseSaveCanExecuteChanged();
        }

        private void InitializeCommand()
        {
            #region App Commands
            NewFileCommand = new ScriptSupport.Commands.RelayCommand(_ => NewFileFunction(), _ => CanNewFileFunction());
            OpenFileCommand = new ScriptSupport.Commands.RelayCommand(_ => OpenFileFunction(), _ => CanOpenFileFunction());
            SaveFileCommand = new ScriptSupport.Commands.RelayCommand(_ => SaveFileFunction(), _ => CanSaveFileFunction());
            SaveAsFileCommand = new ScriptSupport.Commands.RelayCommand(_ => SaveAsFileFunction(), _ => CanSaveAsFileFunction());
            ExitCommand = new ScriptSupport.Commands.RelayCommand(_ => ExitFunction(), _ => CanExitFunction());
            SettingCommand = new ScriptSupport.Commands.RelayCommand(async _ => await SettingFunction(), _ => CanSettingFunction());
            HelpCommand = new ScriptSupport.Commands.RelayCommand(async _ => await BrowseHelp());
            CheckUpDateCommand = new ScriptSupport.Commands.RelayCommand(async _ => await CheckUpdateFunction());
            AboutCommand = new ScriptSupport.Commands.RelayCommand(_ => _dialogService.ShowDialog<AboutViewModel>());
            SpecialCharCommand = new RelayCommand(_ => SpecialCharFunction());
            #endregion

            #region Commands
            DoubleClickCardCommand = new RelayCommand<CardText>(HandleCardDoubleClick);
            DoubleClickScriptCommand = new RelayCommand<ScriptItem>(HandleScriptDoubleClick);
            DoubleClickNewCommand = new RelayCommand<FileItem>(HandleFileDoubleClick);
            DoubleClickOpenCommand = new RelayCommand<FileItem>(HandleFileDoubleClick);
            DoubleClickScrapiyardNameCommand = new RelayCommand<CompletionSymbol>(OnSelectedScrapiyardNameChanged);
            DoubleClickScrapiyardDescCommand = new RelayCommand<CompletionSymbol>(OnSelectedScrapiyardDescChanged);
            #endregion

            CloseCommand = new RelayCommand(_ => { });
        }
        #endregion

        #region Command Function
        private void SpecialCharFunction()
        {
            _panelService.Show<SpecialCharViewModel>();
        }
        private void NewFileFunction()
        {
            string filePath = _dialogService.SaveScript();
            if (string.IsNullOrEmpty(filePath)) return;
            if (!System.IO.File.Exists(filePath))
                using (var fs = System.IO.File.Create(filePath)) { }

            FileItem newFile = new() { FullPath = filePath };
            FileItemsNew.Add(newFile);
        }
        private bool CanNewFileFunction()
        {
            return _configStore.DataHandlingSetting.AllowNew;
        }
        private void OpenFileFunction()
        {
            var fileItems = _dialogService.OpenScripts()
                .Where(System.IO.File.Exists)
                .Where(f => !FileItemsOpen.Any(x => x.FullPath == f))
                .Select(f => new FileItem { FullPath = f });

            if (!fileItems.Any()) return;

            FileItemsOpen.AddRange(fileItems);
        }
        private bool CanOpenFileFunction()
        {
            return true;
        }

        private void SaveFileFunction()
        {
            var doc = ActiveDocument;
            if (doc == null) return;

            // chưa có file path → chuyển sang SaveAs
            if (string.IsNullOrWhiteSpace(doc.FilePath))
            {
                SaveAsFileFunction();
                return;
            }

            _documentManager.SaveDocument(doc);
        }
        private bool CanSaveFileFunction()
        {
            return _configStore.DataHandlingSetting.AllowSave && ActiveDocument != null && ActiveDocument.IsDirty;
        }

        private void SaveAsFileFunction()
        {
            var doc = ActiveDocument;
            if (doc == null) return;

            var path = _dialogService.SaveScript();

            if (string.IsNullOrWhiteSpace(path)) return;

            _documentManager.SaveAsDocument(doc, path);
        }
        private bool CanSaveAsFileFunction()
        {
            return _configStore.DataHandlingSetting.AllowSave && ActiveDocument != null;
        }

        private void ExitFunction()
        {
            _appService.Shutdown();
        }
        private bool CanExitFunction()
        {
            return true;
        }

        private async Task SettingFunction()
        {
            try
            {
                var result = _dialogService.ShowDialog<SettingViewModel>();
            }
            catch (Exception ex)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = MessageBoxIconType.Error,
                    Message = $"{CMess.errorOcc.ToText()} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                await _dialogService.ShowMessage(request);
            }

            //var testMess = new MessageBoxRequest
            //{
            //    Title = "Test",
            //    Message = "This is a test message.",
            //    IconType = MessageBoxIconType.Information,
            //    Buttons = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K" },
            //    DefaultButtonIndex = 2,
            //    ResponseSource = new TaskCompletionSource<int>()
            //};
            //int test = await _dialogService.ShowMessage(testMess);
            //MessageBox.Show("You clicked: " + $"{test}");
            //MessageBox.Show("You clicked: " + testMess.Buttons[test]);
        }
        private bool CanSettingFunction()
        {
            return true;
        }

        private void RaiseCanOpenExecuteChanged()
        {
            NewFileCommand.RaiseCanExecuteChanged();
            OpenFileCommand.RaiseCanExecuteChanged();
        }
        private void RaiseSaveCanExecuteChanged()
        {
            SaveFileCommand.RaiseCanExecuteChanged();
            SaveAsFileCommand.RaiseCanExecuteChanged();
        }
        private async Task BrowseHelp()
        {
            string url = _helpService.BuildHelpUrl();
            if (string.IsNullOrEmpty(url)) return;

            var (Success, ErrorMessage) = _launcherService.OpenWeb(url);
            if (!Success)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = MessageBoxIconType.Error,
                    Message = $"{CMess.errorOcc.ToText()} {ErrorMessage}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                await _dialogService.ShowMessage(request);
            }
        }
        private async Task CheckUpdateFunction()
        {
            var (successCardData, MessageCardData) = await _dataFolderInterface.CheckUpdateCardData();

            if (!successCardData)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = MessageBoxIconType.Warning,
                    Message = MessageCardData,
                    Buttons = new[] { CMess.ok.ToText() },
                    DefaultButtonIndex = 0,
                    ResponseSource = new TaskCompletionSource<int>()
                };
                await _dialogService.ShowMessage(request);
            }
        }
        #endregion

        #region Cards
        private async void FilterCardState_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _searchCardCts?.Cancel();

            _searchCardCts = new CancellationTokenSource();
            var token = _searchCardCts.Token;
            try
            {
                await Task.Delay(300, token);
                var result = await _cardService.ApplyFilterAsync();
                if (result != null)
                {
                    Cards.ReplaceAll(result);
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception)
            {

            }
        }
        private void Cards_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ResultCards));
        }
        private void OnSearchCardChanged()
        {
            _filterCardState.FilterCardText.Desc = SearchCard;
        }
        private async Task HandleCardSingleClick(CardText? item)
        {
            if (item == null) return;
            _selectFileCts?.Cancel();
            _selectFileCts = new CancellationTokenSource();
            try
            {
                await Task.Delay(200, _selectFileCts.Token);
                ulong cardId = item?.id ?? 0;
                await OpenResultCard(item);
                await OpenResultScriptFromCard(cardId, doubleClick: false);
            }
            catch (TaskCanceledException)
            {
                ///
            }
        }
        private async Task HandleCardDoubleClick(CardText? item)
        {
            if (item == null) return;
            _selectFileCts?.Cancel();
            ulong cardId = item?.id ?? 0;
            await OpenResultCard(item);
            await OpenResultScriptFromCard(cardId, doubleClick: true);
        }

        private async Task OpenResultCard(CardText? item)
        {
            _resultService.BuildResultID(item);
            _resultService.BuildResultCardTexts(item);
            _resultService.BuildResultImageCards(item);
            _resultService.BuildResultCardData(item);
            await Task.Yield();
            //var tasks = new Task[]
            //{
            //Task.Run(() => _resultService.BuildResultID(item)),
            //Task.Run(() => _resultService.BuildResultCardTexts(item)),
            //Task.Run(() => _resultService.BuildResultImageCards(item)),
            //Task.Run(() => _resultService.BuildResultCardData(item))
            //};
            //await Task.WhenAll(tasks);
        }

        private async Task OpenResultScriptFromCard(ulong cardId, bool doubleClick)
        {
            if (cardId == 0)
            {
                if (doubleClick) await OpenFilePermanentEmpty(cardId.ToString());
                else await OpenFilePreviewEmpty(cardId.ToString());
                return; 
            }
            List<string> cardScripts = new List<string>();

            List<ulong> listID = _cardService.GetListIDByID(cardId);
            if (listID == null || !listID.Any()) return;
            foreach (var id in listID)
            {
                var listScriptID = _scriptService.GetListScript(id);
                if (listScriptID != null) cardScripts.AddRange(listScriptID);
            }

            if (cardScripts == null || !cardScripts.Any()) //Open Empty TextEditor
            {
                if (doubleClick) await OpenFilePermanentEmpty(cardId.ToString());
                else await OpenFilePreviewEmpty(cardId.ToString());
                return;
            }
            foreach (var script in cardScripts)
            {
                if (string.IsNullOrWhiteSpace(script) || !System.IO.File.Exists(script))
                {
                    if (doubleClick) await OpenFilePermanentEmpty(cardId.ToString());
                    else await OpenFilePreviewEmpty(cardId.ToString());
                    continue;
                }
                if (doubleClick) await OpenFilePermanent(script);
                else await OpenFilePreview(script);
            }
        }
        #endregion

        #region Scripts
        private CancellationTokenSource? _scriptCts;
        private bool _isSearching;
        public bool IsSearching
        {
            get => _isSearching;
            set { _isSearching = value; OnPropertyChanged(); }
        }
        private string _statusText = string.Empty;
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private async Task OnSearchTextChangedAsync(string query)
        {
            _scriptCts?.Cancel();
            _scriptCts?.Dispose();
            _scriptCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(400, _scriptCts.Token);
                ScriptItems.Clear();
                if (string.IsNullOrWhiteSpace(query))
                {
                    StatusText = string.Empty;
                    return;
                }

                IsSearching = true;
                StatusText = "Đang tìm kiếm...";

                var totalFound = 0;
                var progress = new Progress<List<ScriptItem>>(batch =>
                {
                    ScriptItems.AddRange(batch);
                    totalFound += batch.Count;
                });

                await _scriptService.SearchFileContent(query, progress, _scriptCts.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
            finally
            {
                IsSearching = false;
            }
        }

        private async Task HandleScriptSingleClick(ScriptItem? item)
        {
            if (item == null) return;
            _selectFileCts?.Cancel();
            _selectFileCts = new CancellationTokenSource();
            try
            {
                await Task.Delay(200, _selectFileCts.Token);
                await OpenFilePreview(item.FullPath, lineHighLights: item.LineNumbers);
                await OpenResultCardFromScript(item);
            }
            catch (TaskCanceledException)
            {
                ///
            }
        }
        private async Task HandleScriptDoubleClick(ScriptItem? item)
        {
            if (item == null) return;
            _selectFileCts?.Cancel();
            await OpenFilePermanent(item.FullPath, lineHighLights: item.LineNumbers);
            await OpenResultCardFromScript(item);
        }
        private async Task OpenResultCardFromScript(ScriptItem? item)
        {
            if (item == null) return;
            ulong? cardID = StringHelper.GetCardIDFromScript(item.FullPath);
            if (cardID == null || cardID == 0) return;

            var listCardText = _cardService.GetListCardTextByID(cardID.Value);
            if (listCardText == null || !listCardText.Any()) return;

            await OpenResultCard(listCardText.First());
        }

        private void ScriptItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ResultScripts));
        }
        #endregion

        #region Scrapiyard
        private CancellationTokenSource? _scrapiyardNameCts;
        private CancellationTokenSource? _scrapiyardDescCts;

        private async Task OnSearchScrapiyardNameChanged(string query)
        {
            _scrapiyardNameCts?.Cancel();
            _scrapiyardNameCts?.Dispose();
            _scrapiyardNameCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(400, _scrapiyardNameCts.Token);
                ScrapiyardNameItems.Clear();

                if (string.IsNullOrWhiteSpace(query)) return;

                var result = await Task.Run(() => _scrapiyardService.SearchName(query), _scrapiyardNameCts.Token);
                ScrapiyardNameItems.AddRange(result);
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
            finally { }
        }
        private async Task OnSearchScrapiyardDescChanged(string query)
        {
            _scrapiyardDescCts?.Cancel();
            _scrapiyardDescCts?.Dispose();
            _scrapiyardDescCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(400, _scrapiyardDescCts.Token);
                ScrapiyardDescItems.Clear();

                if (string.IsNullOrWhiteSpace(query)) return;

                var result = await Task.Run(() => _scrapiyardService.SearchDesc(query), _scrapiyardDescCts.Token);
                ScrapiyardDescItems.AddRange(result);
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
            finally { }
        }

        private async Task OnSelectedScrapiyardNameChanged(CompletionSymbol? item)
        {
            _resultService.BuildResultScrapiyard(item);
            await Task.Yield();
            if (item != null && _configStore.DataHandlingSetting.AutoSearch) SearchScript = item.Name;
        }
        private async Task OnSelectedScrapiyardDescChanged(CompletionSymbol? item)
        {
            _resultService.BuildResultScrapiyard(item);
            await Task.Yield();
            if (item != null && _configStore.DataHandlingSetting.AutoSearch) SearchScript = item.Name;
        }

        private void ScrapiyardNameItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ResultScrapiyardName));
        }
        private void ScrapiyardDescItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ResultScrapiyardDesc));
        }
        #endregion

        #region File
        private void _documentManager_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DocumentManager.ActiveDocument))
            {
                OnPropertyChanged(nameof(ActiveDocument));
                if (_documentManager.ActiveDocument != null)
                {
                    _documentManager.ActiveDocument.PropertyChanged -= ActiveDocument_PropertyChanged;
                    _documentManager.ActiveDocument.PropertyChanged += ActiveDocument_PropertyChanged;
                }
                RaiseSaveCanExecuteChanged();
            }
        }
        private void ActiveDocument_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DocumentViewModel.IsDirty))
            {
                RaiseSaveCanExecuteChanged();
            }
        }
        private async Task HandleFileSingleClick(FileItem? item)
        {
            if (item == null) return;
            _selectFileCts?.Cancel();
            _selectFileCts = new CancellationTokenSource();
            try
            {
                await Task.Delay(200, _selectFileCts.Token);
                await OpenFilePreview(item.FullPath);
            }
            catch (TaskCanceledException)
            {
                ///
            }
        }
        private async Task HandleFileDoubleClick(FileItem? item)
        {
            if (item == null) return;
            _selectFileCts?.Cancel();
            await OpenFilePermanent(item.FullPath);
        }

        public async Task OpenFilePreview(string path, IReadOnlyList<int>? lineHighLights = null)
        {
            try
            {
                await _documentManager.OpenPreview(path, lineHighLights);
            }
            catch (Exception)
            {

            }
        }
        public async Task OpenFilePermanent(string path, IReadOnlyList<int>? lineHighLights = null)
        {
            try
            {
                await _documentManager.OpenDocument(path, lineHighLights);
            }
            catch (Exception)
            {

            }
        }
        public async Task OpenFilePreviewEmpty(string title = "")
        {
            await _documentManager.OpenPreviewEmpty(title);
        }
        public async Task OpenFilePermanentEmpty(string title = "")
        {
            await _documentManager.OpenDocumentEmpty(title);
        }

        public void SwitchDocument()
        {
            _documentManager.SwitchNext();
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            _configStore.DataHandlingSetting.PropertyChanged -= DataHandlingSetting_PropertyChanged;
            _documentManager.PropertyChanged -= _documentManager_PropertyChanged;
            if (_documentManager.ActiveDocument != null)
                _documentManager.ActiveDocument.PropertyChanged -= ActiveDocument_PropertyChanged;
            _filterCardState.PropertyChanged -= FilterCardState_PropertyChanged;
            Cards.CollectionChanged -= Cards_CollectionChanged;
            ScriptItems.CollectionChanged -= ScriptItems_CollectionChanged;

            ScrapiyardNameItems.CollectionChanged -= ScrapiyardNameItems_CollectionChanged;
            ScrapiyardDescItems.CollectionChanged -= ScrapiyardDescItems_CollectionChanged;
        }
        #endregion

    }
}
