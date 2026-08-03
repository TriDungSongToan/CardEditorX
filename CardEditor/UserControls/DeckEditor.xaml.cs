using System;
using System.IO;
using System.Web.UI.WebControls;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Commands;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for DeckEditor.xaml
    /// </summary>
    public partial class DeckEditor : UserControl, INotifyPropertyChanged, IDisposable, ISaveable
    {
        #region Variable
        private IMainWindowService MainWindowService;
        private string _mainWindowTitle = string.Empty;
        public string MainWindowTitle
        {
            get => _mainWindowTitle;
            set
            {
                if (_mainWindowTitle != value)
                {
                    _mainWindowTitle = value;
                    OnPropertyChanged(nameof(MainWindowTitle));
                    UpdateWindowTitle();
                }
            }
        }

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
                    SaveDeckCommand.RaiseCanExecuteChanged();
                    UpdateWindowSavedFlag();
                }
            }
        }
        public string CurrentDeckPath = string.Empty;
        public string CurrentDeckName = string.Empty;
        private ScrollViewer NameScrollViewer;
        private ScrollViewer ListCardScrollViewer;
        private DispatcherTimer scrollTimer;
        private double scrollSpeed = 0.5; // tốc độ (pixel mỗi tick)
        private bool isPaused = false;
        
        private bool _isDragging = false;
        private bool _isUpdatingSize = false;
        private bool _isInitialized = false;

        private DispatcherTimer ResizeTimer;
        private Point _dragStartPoint;

        #region Setting
        private bool _alternateFormats = false;
        public bool AlternateFormats
        {
            get => _alternateFormats;
            set
            {
                if (_alternateFormats != value)
                {
                    _alternateFormats = value;
                    OnPropertyChanged(nameof(AlternateFormats));
                }
            }
        }
        private bool _cardListViewMode = true;
        public bool CardListViewMode
        {
            get => _cardListViewMode;
            set
            {
                if (_cardListViewMode != value)
                {
                    _cardListViewMode = value;
                    OnPropertyChanged(nameof(CardListViewMode));
                }
            }
        }

        private Visibility _displayID = Visibility.Visible;
        public Visibility DisplayID
        {
            get => _displayID;
            set
            {
                if (_displayID != value)
                {
                    _displayID = value;
                    OnPropertyChanged(nameof(DisplayID));
                }
            }
        }
        private Visibility _displayArchetype = Visibility.Visible;
        public Visibility DisplayArchetype
        {
            get => _displayArchetype;
            set
            {
                if (_displayArchetype != value)
                {
                    _displayArchetype = value;
                    OnPropertyChanged(nameof(DisplayArchetype));
                }
            }
        }
        private Visibility _displayGPoint = Visibility.Visible;
        public Visibility DisplayGPoint
        {
            get => _displayGPoint;
            set
            {
                if (_displayGPoint != value)
                {
                    _displayGPoint = value;
                    OnPropertyChanged(nameof(DisplayGPoint));
                }
            }
        }
        private Visibility _displayScope = Visibility.Visible;
        public Visibility DisplayScope
        {
            get => _displayScope;
            set
            {
                if (_displayScope != value)
                {
                    _displayScope = value;
                    OnPropertyChanged(nameof(DisplayScope));
                }
            }
        }

        private bool RitualPlaceExtra = false;
        private bool SaveName = false;
        private bool IgnoreSize = false;
        private bool _ignoreContent = false;
        public bool IgnoreContent
        {
            get => _ignoreContent;
            set
            {
                if (_ignoreContent != value)
                {
                    _ignoreContent = value;
                    OnPropertyChanged(nameof(IgnoreContent));
                    OnBurhVisibilityChange();
                }
            }
        }
        #endregion

        #endregion

        #region Property

        #region Deck & Card List
        private double _listViewItemWidth = 71;
        public double ListViewItemWidth
        {
            get => _listViewItemWidth;
            set
            {
                if (_listViewItemWidth != value)
                {
                    if (_isUpdatingSize) return;
                    _isUpdatingSize = true;

                    _listViewItemWidth = value;
                    _listViewItemHeight = _listViewItemWidth * 2026.0 / 1388.0;

                    OnPropertyChanged(nameof(ListViewItemWidth));
                    OnPropertyChanged(nameof(ListViewItemHeight));
                    OnPropertyChanged(nameof(GridItemSize));

                    _isUpdatingSize = false;
                }
            }
        }
        private double _listViewItemHeight = 80;
        public double ListViewItemHeight
        {
            get => _listViewItemHeight;
            set
            {
                if (_listViewItemHeight != value)
                {
                    if (_isUpdatingSize) return;
                    _isUpdatingSize = true;

                    _listViewItemHeight = value;
                    _listViewItemWidth = _listViewItemHeight * 1388.0 / 2026.0;

                    OnPropertyChanged(nameof(ListViewItemHeight));
                    OnPropertyChanged(nameof(ListViewItemWidth));
                    OnPropertyChanged(nameof(GridItemSize));

                    _isUpdatingSize = false;
                }
            }
        }
        public Size GridItemSize => new Size(ListViewItemWidth, ListViewItemHeight);

        public CardEditor.Models.Deck _currentDeck;
        public CardEditor.Models.Deck CurrentDeck
        {
            get => _currentDeck;
            set
            {
                if (_currentDeck != value)
                {
                    _currentDeck = value;
                    OnPropertyChanged(nameof(CurrentDeck));
                    RefreshDeckView();
                    CurrentDeckPath = _currentDeck?.Path ?? string.Empty;
                    CurrentDeckName = _currentDeck?.Name ?? string.Empty;
                    NewFolderPath = (string.IsNullOrEmpty(CurrentDeckPath) || !System.IO.File.Exists(CurrentDeckPath))
                        ? string.Empty : System.IO.Path.GetDirectoryName(CurrentDeckPath);
                    NewNameDeck = CurrentDeckName;
                    MainWindowTitle = CurrentDeckPath;
                    IsSaved = true;
                    DeleteDeckCommand.RaiseCanExecuteChanged();
                    ReNameDeckCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private void cmbDeck_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _isChangedDeck = true;
            var newDeck = cmbDeck.SelectedItem as CardEditor.Models.Deck;
            if (newDeck == CurrentDeck) return;

            if (!IsSaved)
            {
                var result = CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    $"{CMess.HasUnSaveData.ToText()} {CMess.QuestContinue.ToText()}",
                    new[] { CMess.yes.ToText(), CMess.no.ToText(), });

                if (result != 0)
                {
                    OnPropertyChanged(nameof(CurrentDeck));
                    //cmbDeck.SelectedItem = CurrentDeck;
                    return;
                }
            }
            CurrentDeck = newDeck;
            _isChangedDeck = false;
        }
        private bool _hasAllowedCard = false;
        public bool HasAllowedCard
        {
            get => _hasAllowedCard;
            set
            {
                if (_hasAllowedCard != value)
                {
                    _hasAllowedCard = value;
                    OnPropertyChanged(nameof(HasAllowedCard));
                }
            }
        }
        private bool _isChangedDeck = false;
        private bool _isChangedAllowedCard = false;
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
                    RefreshLimitItem();
                    RefreshCardListView();
                }
            }
        }
        //public BulkObservableCollection<CardInstance> MainDeck { get; set; }
        //public BulkObservableCollection<CardInstance> ExtraDeck { get; set; }
        //public BulkObservableCollection<CardInstance> SideDeck { get; set; }
        public CardDeck MainDeck { get; set; }
        public CardDeck ExtraDeck { get; set; }
        public CardDeck SideDeck { get; set; }
        #endregion

        #region Selected Card
        private CardEX _draggedCardEX;
        private CardInstance _draggedCardInstance;

        private CardEX _hoveredCardEX = null;
        private CardEX _selectedCardEX = null;

        private CardInstance _hoveredCardInstance = null;
        private CardInstance _selectedCardInstance = null;
        #endregion

        #region Card List
        private BulkObservableCollection<CardEX> _filteredCards;
        public BulkObservableCollection<CardEX> FilteredCards
        {
            get => _filteredCards;
            set
            {
                if (_filteredCards != value)
                {
                    if (_filteredCards != null) _filteredCards.CollectionChanged -= FilteredCards_CollectionChanged;
                    _filteredCards = value;
                    if (_filteredCards != null) _filteredCards.CollectionChanged += FilteredCards_CollectionChanged;

                    OnPropertyChanged(nameof(FilteredCards));
                    OnPropertyChanged(nameof(ResultCards));
                }
            }
        }
        private CancellationTokenSource _filterCts;
        private readonly SemaphoreSlim _filterLock = new SemaphoreSlim(1, 1);
        #endregion

        public CardEX CurrentCard =>
            _selectedCardInstance?.Card ??
            _selectedCardEX ??
            _hoveredCardInstance?.Card ??
            _hoveredCardEX;
        #endregion

        #region Commands
        public RelayCommand SaveDeckCommand { get; set; }
        public RelayCommand SortDeckCommand { get; set; }
        public RelayCommand ShuffleDeckCommand { get; set; }
        public RelayCommand ClearDeckCommand { get; set; }
        public RelayCommand DeleteDeckCommand { get; set; }
        public RelayCommand ViewImageCommand { get; set; }
        public RelayCommand OpenFileLocalCommand { get; set; }
        public RelayCommand OpenDatabaseCommand { get; set; }
        public RelayCommand OpenScriptCommand { get; set; }
        public RelayCommand OpenKonamiDBCommand { get; set; }
        public RelayCommand OpenYugipediaCommand { get; set; }
        public RelayCommand OpenYGOResourcesCommand { get; set; }
        public RelayCommand CreateNewDeckCommand { get; set; }
        public RelayCommand ReNameDeckCommand { get; set; }
        public RelayCommand SaveAsDeckCommand { get; set; }
        public RelayCommand YDKEImportCommand { get; set; }
        public RelayCommand ExportLinkYDKECommand { get; set; }
        public RelayCommand ExportTextYDKECommand { get; set; }
        public RelayCommand ExportDeckYDKECommand { get; set; }
        public RelayCommand CopyYDKEURLCommand { get; set; }
        #endregion

        #region Constructor
        public DeckEditor()
        {
            InitializeComponent();
            InitializeCommands();
            ViewData = new DeckEditData();
            FilteredCards = new BulkObservableCollection<CardEX>();

            //MainDeck = new BulkObservableCollection<CardInstance>();
            //ExtraDeck = new BulkObservableCollection<CardInstance>();
            //SideDeck = new BulkObservableCollection<CardInstance>();
            MainDeck = new CardDeck(DeckType.Main);
            ExtraDeck = new CardDeck(DeckType.Extra);
            SideDeck = new CardDeck(DeckType.Side);

            imageCard.Source = CardImageCacheViewModel.Instance.BlankImage;

            ResizeTimer = new DispatcherTimer();
            ResizeTimer.Interval = TimeSpan.FromMilliseconds(500);
            ResizeTimer.Tick += ResizeTimer_Tick;

            MainDeck.CollectionChanged += MainDeck_CollectionChanged;
            ExtraDeck.CollectionChanged += ExtraDeck_CollectionChanged;
            SideDeck.CollectionChanged += SideDeck_CollectionChanged;

            this.DataContext = this;
            RootGrid.AddHandler(DragOverEvent, new DragEventHandler(Universal_DragOver), true);
        }
        public DeckEditor(IMainWindowService service) : this()
        {
            MainWindowService = service;
        }
        private void InitializeCommands()
        {
            SaveDeckCommand = new CardEditor.Commands.RelayCommand(async _ => await SaveCommand(), _ => CanSaveCommand());
            SortDeckCommand = new CardEditor.Commands.RelayCommand(_ => SortCommand(), _ => DeckHasCards());
            ShuffleDeckCommand = new CardEditor.Commands.RelayCommand(_ => ShuffleCommand(), _ => DeckHasCards());
            ClearDeckCommand = new CardEditor.Commands.RelayCommand(_ => ClearCommand(), _ => DeckHasCard());
            DeleteDeckCommand = new CardEditor.Commands.RelayCommand(_ => DeleteCommand(), _ => HasSelectedDeck());
            ViewImageCommand = new CardEditor.Commands.RelayCommand(_ => ViewImageCard(), _ => CurrentCard != null);
            OpenFileLocalCommand = new CardEditor.Commands.RelayCommand(_ => OpenFileLocal(), _ => CurrentCard != null);
            OpenDatabaseCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenDataBase(), _ => CurrentCard != null);
            OpenScriptCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenScript(), _ => CurrentCard != null);
            CreateNewDeckCommand = new CardEditor.Commands.RelayCommand(_ => CreateNewDeck(), _ => ReNameInValid());
            ReNameDeckCommand = new CardEditor.Commands.RelayCommand(_ => ReNameDeck(), _ => HasSelectedDeck() && ReNameInValid());
            SaveAsDeckCommand = new CardEditor.Commands.RelayCommand(async _ => await SaveAsDeck(), _ => ReNameInValid());

            YDKEImportCommand = new CardEditor.Commands.RelayCommand(_ => YDKEImport(), _ => CanYDKEImport());
            ExportLinkYDKECommand = new CardEditor.Commands.RelayCommand(_ => ExportLinkYDKE(), _ => DeckHasCard());
            ExportTextYDKECommand = new CardEditor.Commands.RelayCommand(_ => ExportTextYDKE(), _ => DeckHasCard());
            ExportDeckYDKECommand = new CardEditor.Commands.RelayCommand(_ => ExportDeckYDKE(), _ => DeckHasCard());
            CopyYDKEURLCommand = new CardEditor.Commands.RelayCommand(_ => CopyYDKEURL(), _ => !string.IsNullOrWhiteSpace(YDKEString));

            OpenKonamiDBCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenKonamiDB(), _ => CurrentCard != null);
            OpenYugipediaCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenYugipedia(), _ => CurrentCard != null);
            OpenYGOResourcesCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenYGOResources(), _ => CurrentCard != null);
        }
        private void Universal_DragOver(object sender, DragEventArgs e)
        {
            UpdateGhostCardPosition(e);
            // Nếu sender không phải là control đặc biệt cần xử lý drag logic riêng
            // thì cho phép event bubble up
            if (sender is System.Windows.Controls.TextBox || sender is RichTextBox || sender is Border)
            {
                e.Effects = DragDropEffects.Copy;
            }
        }
        #endregion

        #region Load
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            InitializeContentMenu();
            UpdateCardListItemSize();
            AlternateRule = (ulong)(CardRule.Anime | CardRule.Illegal | CardRule.VideoGame |
                CardRule.Custom | CardRule.SpeedDuel | CardRule.NA1 | CardRule.Rush |
                CardRule.Legend | CardRule.NA2 | CardRule.Hidden);
            NameScrollViewer = GetDescendantByType<ScrollViewer>(txtCardName);
            scrollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            scrollTimer.Tick += ScrollTimer_Tick;
            await LoadBanList();
            await LoadCardList();
            await LoadDeck();
            IconCopy.Kind = MaterialDesignThemes.Wpf.PackIconKind.ContentCopy;
        }
        private void InitializeContentMenu()
        {
            ControlContextMenuService.Attach(txtid);
            ControlContextMenuService.Attach(txtLvRk);
            ControlContextMenuService.Attach(txtLinkRating);
            ControlContextMenuService.Attach(txtGPoint);

            ControlContextMenuService.Attach(txtATK);
            ControlContextMenuService.Attach(txtDEF);
            ControlContextMenuService.Attach(txtLeftScale);
            ControlContextMenuService.Attach(txtRightScale);
            ControlContextMenuService.Attach(txtdesc);

            ControlContextMenuService.Attach(txtReName);
            ControlContextMenuService.Attach(txtNewPath);
            ControlContextMenuService.Attach(txtYDKEString);
        }
        private async Task LoadBanList()
        {
            if (!BanListRawDataViewModel.Instance.IsLoaded)
            {
                BanListRawDataViewModel.Instance.LoadLimitImages();
                var (result, message) = await BanListRawDataViewModel.Instance.LoadBanLists();
                if (!result)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                }
            }
        }
        private async Task LoadCardList()
        {
            if (_isInitialized) return;

            try
            {
                await CardEXDataViewModel.Instance.LoadCardsEXAsync();
                
                _isInitialized = true;
                _isRegexDirty = true;
                await ApplyFilterAsync();
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task LoadDeck()
        {
            cmbDeck.ItemsSource = DeckViewModel.Instance.Decks;
            if (!DeckViewModel.Instance.IsLoadedDecksList)
            {
                bool resultLoadDeck = await DeckViewModel.Instance.LoadDeckAsync();
                if (resultLoadDeck)
                {
                    // cmbDeck.SelectedIndex = 0;
                }
                else
                {
                    cmbDeck.SelectedIndex = -1;
                    CMSG.Show(CMess.infoma.ToText(), CMSG.MessageBoxIconType.Information,
                        CMess.noDeckFound.ToText(), new[] { CMess.ok.ToText() });
                }
            }
            if (!string.IsNullOrEmpty(DeckViewModel.Instance.DeckFolderPath) && Directory.Exists(DeckViewModel.Instance.DeckFolderPath))
            {
                NewFolderPath = DeckViewModel.Instance.DeckFolderPath;
            }
        }
        public void LoadConfig()
        {
            //int deckEditBool = ConfigViewModel.Instance.DeckEditor;

            AlternateFormats = ConfigViewModel.Instance.deckEditSetting.Alternate;
            CardListViewMode = ConfigViewModel.Instance.deckEditSetting.ListMode;
            DisplayID = ConfigViewModel.Instance.deckEditSetting.DisplayID ? Visibility.Visible : Visibility.Collapsed;
            DisplayArchetype = ConfigViewModel.Instance.deckEditSetting.DisplayArtchetype ? Visibility.Visible : Visibility.Collapsed;
            DisplayGPoint = ConfigViewModel.Instance.deckEditSetting.DisplayGPoint ? Visibility.Visible : Visibility.Collapsed;
            DisplayScope = ConfigViewModel.Instance.deckEditSetting.DisplayScopeImg ? Visibility.Visible : Visibility.Collapsed;
            RitualPlaceExtra = ConfigViewModel.Instance.deckEditSetting.RitualPlaceExtra;
            SaveName = ConfigViewModel.Instance.deckEditSetting.SaveCardName;
            IgnoreSize = ConfigViewModel.Instance.deckEditSetting.IgnoreDeckSize;
            IgnoreContent = ConfigViewModel.Instance.deckEditSetting.IgnoreDeckContent;
            //AlternateFormats = (deckEditBool & (1 << 0)) != 0;
            //CardListViewMode = (deckEditBool & (1 << 1)) != 0;
            //DisplayID = (deckEditBool & (1 << 2)) != 0 ? Visibility.Visible : Visibility.Collapsed;
            //DisplayArchetype = (deckEditBool & (1 << 3)) != 0 ? Visibility.Visible: Visibility.Collapsed;
            //DisplayGPoint = (deckEditBool & (1 << 4)) != 0 ? Visibility.Visible : Visibility.Collapsed;
            //DisplayScope = (deckEditBool & (1 << 5)) != 0 ? Visibility.Visible : Visibility.Collapsed;
            //RitualPlaceExtra = (deckEditBool & (1 << 6)) != 0;
            //SaveName = (deckEditBool & (1 << 7)) != 0;
            //IgnoreSize = (deckEditBool & (1 << 8)) != 0;
            //IgnoreContent = (deckEditBool & (1 << 9)) != 0;

            int advancedSettings = ConfigViewModel.Instance.dataHandlingSetting.Advanced;
            isAdvancedFind = (advancedSettings & 0x01) == 0x01; // bit 1: Advanced Find
            matchCase = (advancedSettings & 0x02) == 0x02;     // bit 2: Match Case
            useWildcards = (advancedSettings & 0x04) == 0x04;  // bit 3: Use Wildcards
            matchPrefix = (advancedSettings & 0x08) == 0x08;   // bit 4: Match Prefix
            matchSuffix = (advancedSettings & 0x10) == 0x10;   // bit 5: Match Suffix
            wholeWords = (advancedSettings & 0x20) == 0x20;    // bit 6: Find Whole Words Only
            ignorePunctuation = (advancedSettings & 0x40) == 0x40; // bit 7: Ignore Punctuation
            ignoreWhitespace = (advancedSettings & 0x80) == 0x80;  // bit 8: Ignore White-Space
            _comparison = (isAdvancedFind && matchCase)
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;
            LoadDeckSize();
        }
        public async Task LoadFilter()
        {
            _isRegexDirty = true;
            await RefreshFilter();
        }
        public void LoadSort()
        {
            //var sourceCardList = CardEXDataViewModel.Instance.AllCards.Values;
            //ListCardCollectionView = CollectionViewSource.GetDefaultView(sourceCardList);

            //var selectedSorts = SortsViewModel.Instance.SelectedSortItems;
            //if (selectedSorts != null && selectedSorts.Count > 0)
            //{
            //    using (ListCardCollectionView.DeferRefresh())
            //    {
            //        ListCardCollectionView.SortDescriptions.Clear();

            //        foreach (var sort in selectedSorts)
            //        {
            //            if (sort?.SelectedItem == null) continue;

            //            string cardEXPropertyName = string.Empty;

            //            string cardPropertyName = sort.SelectedItem.Name;
            //            if (cardPropertyName == null || string.IsNullOrEmpty(cardPropertyName)) continue;

            //            if (cardPropertyName == "Rare" || cardPropertyName == "GPoint")
            //            {
            //                cardEXPropertyName = sort.SelectedItem.Name;
            //            }
            //            else
            //            {
            //                cardEXPropertyName = $"BaseCard.{sort.SelectedItem.Name}";
            //            }

            //            var direction = sort.OrderByAsc ? ListSortDirection.Ascending : ListSortDirection.Descending;

            //            ListCardCollectionView.SortDescriptions.Add(new SortDescription(cardEXPropertyName, direction));
            //        }
            //    }
            //}
        }
        private void LoadDeckSize()
        {
            NumCardMainLimit = (ConfigViewModel.Instance.deckEditSetting.MainDeckLimit.HasValue && ConfigViewModel.Instance.deckEditSetting.MainDeckLimit > 0)
                ? $"/{ConfigViewModel.Instance.deckEditSetting.MainDeckLimit.Value}" : "/Ulimited";
            NumCardExtraLimit = (ConfigViewModel.Instance.deckEditSetting.ExtraDeckLimit.HasValue && ConfigViewModel.Instance.deckEditSetting.ExtraDeckLimit > 0)
                ? $"/{ConfigViewModel.Instance.deckEditSetting.ExtraDeckLimit.Value}" : "/Ulimited";
            NumCardSideLimit = (ConfigViewModel.Instance.deckEditSetting.SideDeckLimit.HasValue && ConfigViewModel.Instance.deckEditSetting.SideDeckLimit > 0)
                ? $"/{ConfigViewModel.Instance.deckEditSetting.SideDeckLimit.Value}" : "/Ulimited";
        }
        private void OnBurhVisibilityChange()
        {
            if (IgnoreContent) BurhVisibility = Visibility.Visible;
            else BurhVisibility = Visibility.Collapsed;
        }
        #endregion

        #region Change

        #region CollectionChanged
        private void MainDeck_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsSaved = false;
            UpdateMainDeckInformation();
            RecalculateTotalGPoint();
            SortDeckCommand.RaiseCanExecuteChanged();
            ShuffleDeckCommand.RaiseCanExecuteChanged();
            ClearDeckCommand.RaiseCanExecuteChanged();
            ExportLinkYDKECommand.RaiseCanExecuteChanged();
            ExportTextYDKECommand.RaiseCanExecuteChanged();
            ExportDeckYDKECommand.RaiseCanExecuteChanged();
        }
        private void ExtraDeck_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsSaved = false;
            UpdateExtraDeckInformation();
            RecalculateTotalGPoint();
            SortDeckCommand.RaiseCanExecuteChanged();
            ShuffleDeckCommand.RaiseCanExecuteChanged();
            ClearDeckCommand.RaiseCanExecuteChanged();
            ExportLinkYDKECommand.RaiseCanExecuteChanged();
            ExportTextYDKECommand.RaiseCanExecuteChanged();
            ExportDeckYDKECommand.RaiseCanExecuteChanged();
        }
        private void SideDeck_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsSaved = false;
            UpdateSideDeckInformation();
            RecalculateTotalGPoint();
            SortDeckCommand.RaiseCanExecuteChanged();
            ShuffleDeckCommand.RaiseCanExecuteChanged();
            ClearDeckCommand.RaiseCanExecuteChanged();
            ExportLinkYDKECommand.RaiseCanExecuteChanged();
            ExportTextYDKECommand.RaiseCanExecuteChanged();
            ExportDeckYDKECommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region SelectedItemsChanged
        private void cmbcardtype_LostFocus(object sender, RoutedEventArgs e)
        {
            ulong type = 0;
            var selectedTypes = cmbcardtype.SelectedItems?.Cast<TypeItem>().ToList();
            if (selectedTypes != null && selectedTypes.Any())
                foreach (var typeItem in selectedTypes)
                {
                    type |= typeItem.TypeCode;
                }
            ViewData.SelectedType = type;
        }
        private void cmbcardattribute_LostFocus(object sender, RoutedEventArgs e)
        {
            ulong attribute = 0;
            var selectedAttributes = cmbcardattribute.SelectedItems?.Cast<AttributeItem>().ToList();
            if (selectedAttributes != null && selectedAttributes.Any())
                foreach (var attributeItem in selectedAttributes)
                {
                    attribute |= attributeItem.AttributeCode;
                }
            ViewData.SelectedAttribute = attribute;
        }
        private void cmbcardrace_LostFocus(object sender, RoutedEventArgs e)
        {
            ulong race = 0;
            bool isskill = cmbcardtype.SelectedItems.Cast<TypeItem>().Any(item => item.TypeCode == (ulong)CardType.Skill);
            if (isskill)
            {
                var selectedChars = cmbcardchar.SelectedItems?.Cast<CharItem>().ToList();
                if (selectedChars != null && selectedChars.Any())
                    foreach (var charItem in selectedChars)
                    {
                        race |= charItem.CharCode;
                    }
            }
            else
            {
                var selectedRaces = cmbcardrace.SelectedItems?.Cast<RaceItem>().ToList();
                if (selectedRaces != null && selectedRaces.Any())
                    foreach (var raceItem in selectedRaces)
                    {
                        race |= raceItem.RaceCode;
                    }
            }

            ViewData.SelectedRace = race;
        }
        private void cmbcardchar_LostFocus(object sender, RoutedEventArgs e)
        {
            ulong race = 0;
            bool isskill = cmbcardtype.SelectedItems.Cast<TypeItem>().Any(item => item.TypeCode == (ulong)CardType.Skill);
            if (isskill)
            {
                var selectedChars = cmbcardchar.SelectedItems?.Cast<CharItem>().ToList();
                if (selectedChars != null && selectedChars.Any())
                    foreach (var charItem in selectedChars)
                    {
                        race |= charItem.CharCode;
                    }
            }
            else
            {
                var selectedRaces = cmbcardrace.SelectedItems?.Cast<RaceItem>().ToList();
                if (selectedRaces != null && selectedRaces.Any())
                    foreach (var raceItem in selectedRaces)
                    {
                        race |= raceItem.RaceCode;
                    }
            }

            ViewData.SelectedRace = race;
        }
        private void cmbrule_LostFocus(object sender, RoutedEventArgs e)
        {
            ulong rule = 0;
            var selectedRules = cmbrule.SelectedItems?.Cast<RuleItem>().ToList();
            if (selectedRules != null && selectedRules.Any())
                foreach (var ruleItem in selectedRules)
                    rule |= ruleItem.RuleCode;
            ViewData.SelectedRule = rule;
        }
        private void cmbsetcode_LostFocus(object sender, RoutedEventArgs e)
        {
            ulong setcode = 0;
            var selectedSetCodeItems = cmbsetcode.SelectedItems?.Cast<SetCodeItem>().ToList();
            if (selectedSetCodeItems != null && selectedSetCodeItems.Any())
                for (int i = 0; i < selectedSetCodeItems.Count; i++)
                {
                    var setItem = selectedSetCodeItems[i];
                    setcode |= (ulong)setItem.SetCode << (i * 16);
                }
            ViewData.SelectedSetcode = setcode;
        }
        private void cmbflag_LostFocus(object sender, RoutedEventArgs e)
        {
            ulong flag = 0;
            var selectedFlag = cmbflag.SelectedItems?.Cast<FlagItem>().ToList();
            if (selectedFlag != null && selectedFlag.Any())
                foreach (var flagItem in selectedFlag)
                    flag |= flagItem.FlagCode;
            ViewData.SelectedFlag = flag;
        }
        private void cmbrarity_LostFocus(object sender, RoutedEventArgs e)
        {
            long rare = 0;
            var selectedRares = cmbrarity.SelectedItems?.Cast<RareItem>().ToList();
            if (selectedRares != null && selectedRares.Any())
                foreach (var rareItem in selectedRares)
                {
                    rare |= rareItem.Code;
                }
            ViewData.SelectedRarity = rare;
        }
        private void cmbcategory_LostFocus(object sender, RoutedEventArgs e)
        {
            ulong category = 0;
            var selectedCategorys = cmbcategory.SelectedItems?.Cast<CategoryItem>().ToList();
            if (selectedCategorys != null && selectedCategorys.Any())
                foreach (var categoryItem in selectedCategorys)
                {
                    category |= categoryItem.CategoryCode;
                }
            ViewData.SelectedCategory = category;
        }

        private void cmbcardtype_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbcardtype.SelectedItems != null && cmbcardtype.SelectedItems.Count > 0) lbeCardType.Visibility = Visibility.Collapsed;
            else lbeCardType.Visibility = Visibility.Visible;

            IsLink = cmbcardtype.SelectedItems.Cast<TypeItem>().Any(item => item.TypeCode == (ulong)CardType.Link);
            bool isskill = cmbcardtype.SelectedItems.Cast<TypeItem>().Any(item => item.TypeCode == (ulong)CardType.Skill);
            bool isxyz = cmbcardtype.SelectedItems.Cast<TypeItem>().Any(item => item.TypeCode == (ulong)CardType.eXceed);
            bool isnonxyz = cmbcardtype.SelectedItems.Cast<TypeItem>().Any(item =>
                item.TypeCode == (ulong)CardType.Fusion ||
                item.TypeCode == (ulong)CardType.Ritual ||
                item.TypeCode == (ulong)CardType.Synchro);

            if (isxyz && !isnonxyz)
            {
                imgLvRk.Source = ImageCacheService.Instance.Get(AppImage.RankStar);
                txtLvRk.ToolTip = CMess.Rank.ToText();
            }
            else if (isxyz && isnonxyz)
            {
                imgLvRk.Source = ImageCacheService.Instance.Get(AppImage.LevelRankStar);
                txtLvRk.ToolTip = $"{CMess.Level.ToText()}/{CMess.Rank.ToText()}";
            }
            else
            {
                imgLvRk.Source = ImageCacheService.Instance.Get(AppImage.LevelStar);
                txtLvRk.ToolTip = CMess.Level.ToText();
            }

            if (isskill)
            {
                grcardchar.Visibility = Visibility.Visible;
                grcardrace.Visibility = Visibility.Collapsed;
            }
            else
            {
                grcardrace.Visibility = Visibility.Visible;
                grcardchar.Visibility = Visibility.Collapsed;
            }
        }
        private void cmbcardattribute_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbcardattribute.SelectedItems != null && cmbcardattribute.SelectedItems.Count > 0) lbeAttribute.Visibility = Visibility.Collapsed;
            else lbeAttribute.Visibility = Visibility.Visible;
        }
        private void cmbcardrace_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbcardrace.SelectedItems != null && cmbcardrace.SelectedItems.Count > 0) lbeRace.Visibility = Visibility.Collapsed;
            else lbeRace.Visibility = Visibility.Visible;
        }
        private void cmbcardchar_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbcardchar.SelectedItems != null && cmbcardchar.SelectedItems.Count > 0) lbeChar.Visibility = Visibility.Collapsed;
            else lbeChar.Visibility = Visibility.Visible;
        }
        private void cmbrule_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbrule.SelectedItems != null && cmbrule.SelectedItems.Count > 0) lbeCardRule.Visibility = Visibility.Collapsed;
            else lbeCardRule.Visibility = Visibility.Visible;
        }
        private void cmbsetcode_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbsetcode.SelectedItems != null && cmbsetcode.SelectedItems.Count > 0) lbrSetcode.Visibility = Visibility.Collapsed;
            else lbrSetcode.Visibility = Visibility.Visible;
        }
        private void cmbflag_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbflag.SelectedItems != null && cmbflag.SelectedItems.Count > 0) lbeFlag.Visibility = Visibility.Collapsed;
            else lbeFlag.Visibility = Visibility.Visible;
        }
        private void cmbrarity_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbrarity.SelectedItems != null && cmbrarity.SelectedItems.Count > 0) lbeRarity.Visibility = Visibility.Collapsed;
            else lbeRarity.Visibility = Visibility.Visible;
        }
        private void cmbcategory_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbcategory.SelectedItems != null && cmbcategory.SelectedItems.Count > 0) lbeCategory.Visibility = Visibility.Collapsed;
            else lbeCategory.Visibility = Visibility.Visible;
        }
        #endregion

        #region Deck Infomation

        #region Main Deck
        private int _numCardMain;
        public int NumCardMain
        {
            get => _numCardMain;
            set
            {
                if (_numCardMain != value)
                {
                    _numCardMain = value;
                    OnPropertyChanged(nameof(NumCardMain));
                }
            }
        }
        private string _numCardMainLimit;
        public string NumCardMainLimit
        {
            get => _numCardMainLimit;
            set
            {
                if (_numCardMainLimit != value)
                {
                    _numCardMainLimit = value;
                    OnPropertyChanged(nameof(NumCardMainLimit));
                }
            }
        }

        private int _numMonsterCard = 0;
        public int NumMonsterCard
        {
            get => _numMonsterCard;
            set
            {
                if ( _numMonsterCard != value)
                {
                    _numMonsterCard = value;
                    OnPropertyChanged(nameof(NumMonsterCard));
                }
            }
        }
        private int _numSpellCard = 0;
        public int NumSpellCard
        {
            get => _numSpellCard;
            set
            {
                if ( _numSpellCard != value)
                {
                    _numSpellCard = value;
                    OnPropertyChanged(nameof(NumSpellCard));
                }
            }
        }
        private int _numTrapCard = 0;
        public int NumTrapCard
        {
            get => _numTrapCard;
            set
            {
                if (_numTrapCard != value)
                {
                    _numTrapCard = value;
                    OnPropertyChanged(nameof(NumTrapCard));
                }
            }
        }
        private int _numSkillCard = 0;
        public int NumSkillCard
        {
            get => _numSkillCard;
            set
            {
                if (_numSkillCard != value)
                {
                    _numSkillCard = value;
                    OnPropertyChanged(nameof(NumSkillCard));
                }
            }
        }
        private int _numBurhMain = 0;
        public int NumBurhMain
        {
            get => _numBurhMain;
            set
            {
                if ( _numBurhMain != value)
                {
                    _numBurhMain = value;
                    OnPropertyChanged(nameof(NumBurhMain));
                }
            }
        }
        #endregion

        #region Extra Deck
        private int _numCardExtra;
        public int NumCardExtra
        {
            get => _numCardExtra;
            set
            {
                if (_numCardExtra != value)
                {
                    _numCardExtra = value;
                    OnPropertyChanged(nameof(NumCardExtra));
                }
            }
        }
        private string _numCardExtraLimit;
        public string NumCardExtraLimit
        {
            get => _numCardExtraLimit;
            set
            {
                if (_numCardExtraLimit != value)
                {
                    _numCardExtraLimit = value;
                    OnPropertyChanged(nameof(NumCardExtraLimit));
                }
            }
        }

        private int _numRitualCard = 0;
        public int NumRitualCard
        {
            get => _numRitualCard;
            set
            {
                if (_numRitualCard != value)
                {
                    _numRitualCard = value;
                    OnPropertyChanged(nameof(NumRitualCard));
                }
            }
        }
        private int _numFusionCard = 0;
        public int NumFusionCard
        {
            get => _numFusionCard;
            set
            {
                if (_numFusionCard != value)
                {
                    _numFusionCard = value;
                    OnPropertyChanged(nameof(NumFusionCard));
                }
            }
        }
        private int _numSynchroCard = 0;
        public int NumSynchroCard
        {
            get => _numSynchroCard;
            set
            {
                if (_numSynchroCard != value)
                {
                    _numSynchroCard = value;
                    OnPropertyChanged(nameof(NumSynchroCard));
                }
            }
        }
        private int _numeXceedCard = 0;
        public int NumeXceedCard
        {
            get => _numeXceedCard;
            set
            {
                if (_numeXceedCard != value)
                {
                    _numeXceedCard = value;
                    OnPropertyChanged(nameof(NumeXceedCard));
                }
            }
        }
        private int _numeLinkCard = 0;
        public int NumeLinkCard
        {
            get => _numeLinkCard;
            set
            {
                if (_numeLinkCard != value)
                {
                    _numeLinkCard = value;
                    OnPropertyChanged(nameof(NumeLinkCard));
                }
            }
        }
        private int _numBurhExtra = 0;
        public int NumBurhExtra
        {
            get => _numBurhExtra;
            set
            {
                if (_numBurhExtra != value)
                {
                    _numBurhExtra = value;
                    OnPropertyChanged(nameof(NumBurhExtra));
                }
            }
        }
        #endregion

        #region Side Deck
        private int _numCardSide;
        public int NumCardSide
        {
            get => _numCardSide;
            set
            {
                if (_numCardSide != value)
                {
                    _numCardSide = value;
                    OnPropertyChanged(nameof(NumCardSide));
                }
            }
        }
        private string _numCardSideLimit;
        public string NumCardSideLimit
        {
            get => _numCardSideLimit;
            set
            {
                if (_numCardSideLimit != value)
                {
                    _numCardSideLimit = value;
                    OnPropertyChanged(nameof(NumCardSideLimit));
                }
            }
        }

        private int _numMonsterCardSide = 0;
        public int NumMonsterCardSide
        {
            get => _numMonsterCardSide;
            set
            {
                if (_numMonsterCardSide != value)
                {
                    _numMonsterCardSide = value;
                    OnPropertyChanged(nameof(NumMonsterCardSide));
                }
            }
        }
        private int _numSpellCardSide = 0;
        public int NumSpellCardSide
        {
            get => _numSpellCardSide;
            set
            {
                if (_numSpellCardSide != value)
                {
                    _numSpellCardSide = value;
                    OnPropertyChanged(nameof(NumSpellCardSide));
                }
            }
        }
        private int _numTrapCardSide = 0;
        public int NumTrapCardSide
        {
            get => _numTrapCardSide;
            set
            {
                if (_numTrapCardSide != value)
                {
                    _numTrapCardSide = value;
                    OnPropertyChanged(nameof(NumTrapCardSide));
                }
            }
        }
        private int _numSkillCardSide = 0;
        public int NumSkillCardSide
        {
            get => _numSkillCardSide;
            set
            {
                if (_numSkillCardSide != value)
                {
                    _numSkillCardSide = value;
                    OnPropertyChanged(nameof(NumSkillCardSide));
                }
            }
        }
        #endregion

        #region Infomation
        private int _totalGPoint;
        public int TotalGPoint
        {
            get => _totalGPoint;
            private set
            {
                if (_totalGPoint != value)
                {
                    _totalGPoint = value;
                    OnPropertyChanged();
                }
            }
        }

        private Visibility _burhVisibility = Visibility.Collapsed;
        public Visibility BurhVisibility
        {
            get => _burhVisibility;
            set
            {
                if (_burhVisibility != value)
                {
                    _burhVisibility = value;
                    OnPropertyChanged(nameof(BurhVisibility));
                }
            }
        }

        private void RecalculateTotalGPoint()
        {
            TotalGPoint = MainDeck.Sum(c => c.Card.GPoint)
                        + ExtraDeck.Sum(c => c.Card.GPoint)
                        + SideDeck.Sum(c => c.Card.GPoint);
        }
        private void UpdateMainDeckInformation()
        {
            NumCardMain = MainDeck.Count;

            int[] countsType = CountTypesMainDeck();
            NumMonsterCard = countsType[0];
            NumSpellCard = countsType[1];
            NumTrapCard = countsType[2];
            NumSkillCard = countsType[3];
            NumBurhMain = countsType[4];

            NumRitualCard = CountTypesRitual();
        }
        private void UpdateExtraDeckInformation()
        {
            NumCardExtra = ExtraDeck.Count;

            int[] countsType = CountTypesExtraDeck();
            NumFusionCard = countsType[0];
            NumSynchroCard = countsType[1];
            NumeXceedCard = countsType[2];
            NumeLinkCard = countsType[3];
            NumBurhExtra = countsType[4];

            NumRitualCard = CountTypesRitual();
        }
        private void UpdateSideDeckInformation()
        {
            NumCardSide = SideDeck.Count;

            int[] countsType = CountTypesSideDeck();
            NumMonsterCardSide = countsType[0];
            NumSpellCardSide = countsType[1];
            NumTrapCardSide = countsType[2];
            NumSkillCardSide = countsType[3];
        }

        private int[] CountTypesMainDeck()
        {
            int[] counts = new int[5];

            foreach (var card in MainDeck)
            {
                if ((card.Card.BaseCard.type & (ulong)CardType.Monster) == (ulong)CardType.Monster)
                    counts[0]++;
                if ((card.Card.BaseCard.type & (ulong)CardType.Spell) == (ulong)CardType.Spell)
                    counts[1]++;
                if ((card.Card.BaseCard.type & (ulong)CardType.Trap) == (ulong)CardType.Trap)
                    counts[2]++;
                if ((card.Card.BaseCard.type & (ulong)CardType.Skill) == (ulong)CardType.Skill)
                    counts[3]++;
                if ((card.Card.BaseCard.type & (ulong)CardType.Fusion) == (ulong)CardType.Fusion ||
                    (card.Card.BaseCard.type & (ulong)CardType.Synchro) == (ulong)CardType.Synchro ||
                    (card.Card.BaseCard.type & (ulong)CardType.eXceed) == (ulong)CardType.eXceed ||
                    (card.Card.BaseCard.type & (ulong)CardType.Link) == (ulong)CardType.Link)
                    counts[4]++;
            }
            return counts;
        }
        private int[] CountTypesExtraDeck()
        {
            int[] counts = new int[5];

            foreach (var card in ExtraDeck)
            {
                if ((card.Card.BaseCard.type & (ulong)CardType.Fusion) == (ulong)CardType.Fusion)
                    counts[0]++;
                if ((card.Card.BaseCard.type & (ulong)CardType.Synchro) == (ulong)CardType.Synchro)
                    counts[1]++;
                if ((card.Card.BaseCard.type & (ulong)CardType.eXceed) == (ulong)CardType.eXceed)
                    counts[2]++;
                if ((card.Card.BaseCard.type & (ulong)CardType.Link) == (ulong)CardType.Link)
                    counts[3]++;

                if ((card.Card.BaseCard.type & (ulong)CardType.Spell) == (ulong)CardType.Spell ||
                    (card.Card.BaseCard.type & (ulong)CardType.Trap) == (ulong)CardType.Trap)
                    counts[4]++;
            }
            return counts;
        }
        private int[] CountTypesSideDeck()
        {
            int[] counts = new int[4];

            foreach (var card in SideDeck)
            {
                if ((card.Card.BaseCard.type & (ulong)CardType.Monster) == (ulong)CardType.Monster)
                    counts[0]++;
                if ((card.Card.BaseCard.type & (ulong)CardType.Spell) == (ulong)CardType.Spell)
                    counts[1]++;
                if ((card.Card.BaseCard.type & (ulong)CardType.Trap) == (ulong)CardType.Trap)
                    counts[2]++;
                if ((card.Card.BaseCard.type & (ulong)CardType.Skill) == (ulong)CardType.Skill)
                    counts[3]++;
            }
            return counts;
        }
        private int CountTypesRitual()
        {
            int counts = 0;
            foreach(var card in MainDeck)
            {
                if ((card.Card.BaseCard.type & (ulong)CardType.Ritual) == (ulong)CardType.Ritual)
                    counts++;
            }
            foreach (var card in ExtraDeck)
            {
                if ((card.Card.BaseCard.type & (ulong)CardType.Ritual) == (ulong)CardType.Ritual)
                    counts++;
            }
            return counts;
        }
        #endregion

        public void UpdateWindowTitle()
        {
            if (MainWindowService != null)
            {
                MainWindowService.UpdateWindowTitle(CurrentDeckPath);
                MainWindowService.UpdateTabItemHeader(CurrentDeckName);
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

        #endregion

        #region Deck
        private string _newNameDeck = string.Empty;
        public string NewNameDeck
        {
            get => _newNameDeck;
            set
            {
                if (_newNameDeck != value)
                {
                    _newNameDeck = value;
                    OnPropertyChanged(nameof(NewNameDeck));
                    CreateNewDeckCommand.RaiseCanExecuteChanged();
                    ReNameDeckCommand.RaiseCanExecuteChanged();
                    SaveAsDeckCommand.RaiseCanExecuteChanged();
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
                    CreateNewDeckCommand.RaiseCanExecuteChanged();
                    ReNameDeckCommand.RaiseCanExecuteChanged();
                    SaveAsDeckCommand.RaiseCanExecuteChanged();
                }
            }
        }

        #region Deck View
        private void RefreshDeckView()
        {
            MainDeck.Clear();
            ExtraDeck.Clear();
            SideDeck.Clear();
            if (CurrentDeck != null)
            {
                MainDeck.AddRange(CurrentDeck.MainDeck);
                ExtraDeck.AddRange(CurrentDeck.ExtraDeck);
                SideDeck.AddRange(CurrentDeck.SideDeck);
            }
            else
            {

            }
            IsSaved = true;
        }
        private void RefreshLimitItem()
        {
            if (SelectedBanList == null)
            {
                HasAllowedCard = false;
                return;
            }
            _isChangedAllowedCard = true;
            HasAllowedCard = true;
            var itemAllowed = cmbLimit.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(i => (string)i.Tag == "4");
            if (itemAllowed == null) return;

            if (SelectedBanList.WhiteList)
            {
                itemAllowed.Visibility = Visibility.Visible;
            }
            else
            {
                itemAllowed.Visibility = Visibility.Collapsed;
                if (ViewData.Limit == 4)
                    ViewData.Limit = 5;
            }
            _isChangedAllowedCard = false;
        }
        private void RefreshCardListView()
        {
            if (CardListView?.Items != null)
            {
                CardListView.Items.Refresh();
            }
        }
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
        #endregion

        #region Deck Command
        private async Task SaveCommand()
        {
            await Save();
        }
        private bool CanSaveCommand()
        {
            return !IsSaved;
        }

        public async Task<bool> Save()
        {
            if (DeckViewModel.Instance.Decks == null) return false;

            bool isNull = false;
            if (CurrentDeck == null)
            {
                isNull = true;
                if (MainDeck == null || ExtraDeck == null || SideDeck == null) return true;
                var newDeck = new CardEditor.Models.Deck
                {
                    MainDeck = MainDeck.ToList(),
                    ExtraDeck = ExtraDeck.ToList(),
                    SideDeck = SideDeck.ToList(),
                };
                DeckViewModel.Instance.Decks.Add(newDeck);
                CurrentDeck = newDeck;
            }

            string filePath = null;

            if (string.IsNullOrWhiteSpace(CurrentDeck.Path) || !System.IO.File.Exists(CurrentDeck.Path))
            {
                if (string.IsNullOrEmpty(DeckViewModel.Instance.DeckFolderPath) ||
                    !System.IO.Directory.Exists(DeckViewModel.Instance.DeckFolderPath))
                {
                    string filePathChoose = FileDiaLogHelper.SaveDeck();

                    if (!string.IsNullOrEmpty(filePathChoose)) filePath = filePathChoose;
                    else return false;
                }
            }
            else filePath = CurrentDeck.Path;
            if (!isNull)
            {
                CurrentDeck.MainDeck = new List<CardInstance>(MainDeck);
                CurrentDeck.ExtraDeck = new List<CardInstance>(ExtraDeck);
                CurrentDeck.SideDeck = new List<CardInstance>(SideDeck);
            }

            var (resultFile, messageFile) = await DeckViewModel.Instance.SaveDeckFile(CurrentDeck, SaveName, filePath);
            if (!resultFile)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {messageFile}", new[] { CMess.ok.ToText() });
                return false;
            }
            var (resultArchi, messArchi) = await CurrentDeck.SaveToArchive(filePath);
            if (!resultArchi)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, messArchi, new[] { CMess.ok.ToText() });
                return false;
            }

            CurrentDeck.Path = filePath;
            var (resultMemory, messageMemory) = DeckViewModel.Instance.SaveDeckMemory(CurrentDeck);
            if (!resultMemory)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {messageMemory}", new[] { CMess.ok.ToText() });
                return false;
            }
            CurrentDeck.Path = filePath;
            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                string.Format(CMess.ThreePlaceholderSuccess.ToText(), CurrentDeck.Name, CMess.Deck.ToText(), CMess.Save.ToText()),
                new[] { CMess.ok.ToText() });
            return true;
        }
        public async Task SaveAsDeck()
        {
            string deckPath = GetNewFolderPath();
            if (string.IsNullOrEmpty(deckPath)) return;

            string newName = JoinStringHelper.SanitizationString(NewNameDeck.Trim());
            var ext = System.IO.Path.GetExtension(newName);
            if (string.IsNullOrEmpty(ext) || ext.Length <= 1)
            {
                if (newName.EndsWith("."))
                    newName = newName.TrimEnd('.');
                newName += ".ydk";
            }
            string newPath = System.IO.Path.Combine(deckPath, newName);
            if (System.IO.File.Exists(newPath))
            {
                int result = CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    $"{CMess.filealreadyExit.ToText()} {CMess.QuestOverwrite.ToText()}",
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
            }

            CardEditor.Models.Deck newDeck = new CardEditor.Models.Deck
            {
                Path = newPath,
                Name = newName,
                MainDeck = new List<CardInstance>(MainDeck),
                ExtraDeck = new List<CardInstance>(ExtraDeck),
                SideDeck = new List<CardInstance>(SideDeck),
            };

            var (resultCreate, messageCreate) = await DeckViewModel.Instance.SaveDeckFile(newDeck, SaveName,
                System.IO.Path.Combine(deckPath, newName));
            if (!resultCreate)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageCreate}", new[] { CMess.ok.ToText() });
                return;
            }
            DeckViewModel.Instance.Decks.Add(newDeck);
            CurrentDeck = newDeck;
            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                string.Format(CMess.ThreePlaceholderSuccess.ToText(), newName, CMess.Deck.ToText(), CMess.Save.ToText()), new[] { CMess.ok.ToText() });
        }
        private void SortCommand()
        {
            SortMain();
            SortExtra();
            SortSide();
        }
        private void ShuffleCommand()
        {
            MainDeck.Shuffle();
            ExtraDeck.Shuffle();
            SideDeck.Shuffle();
        }
        private void ClearCommand()
        {
            if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmClear)
            {
                var result = CMSG.Show(CMess.conClear.ToText(), CMSG.MessageBoxIconType.Question,
                    string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.tlClear.ToText(), CMess.SelectedDeck.ToText()),
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
            }
            MainDeck.Clear();
            ExtraDeck.Clear();
            SideDeck.Clear();
        }
        private bool DeckHasCard()
        {
            if (MainDeck == null || ExtraDeck == null || SideDeck == null) return false;
            return MainDeck.Count > 1 || ExtraDeck.Count > 1 || SideDeck.Count > 1;
        }
        private bool DeckHasCards()
        {
            if (MainDeck == null || ExtraDeck == null || SideDeck == null) return false;
            return MainDeck.Count >= 1 || ExtraDeck.Count >= 1 || SideDeck.Count >= 1;
        }
        private void DeleteCommand()
        {
            if (CurrentDeck == null) return;
            if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmDelete)
            {
                var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    string.Format(CMess.confirmDelete.ToText(), CMess.SelectedDeck.ToText()),
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
            }
            string deleteName = CurrentDeck.Name;
            int deckIndex = DeckViewModel.Instance.Decks.IndexOf(CurrentDeck);
            var (resultFile, messageFile) = DeckViewModel.Instance.DeleteDeckFile(CurrentDeck);
            if (!resultFile)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {messageFile}", new[] { CMess.ok.ToText() });
                return;
            }
            var (resultMemory, messageMemory) = DeckViewModel.Instance.DeleteDeckMemory(CurrentDeck.Path);
            if (!resultMemory)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {messageMemory}", new[] { CMess.ok.ToText() });
                return;
            }
            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                string.Format(CMess.ThreePlaceholderSuccess.ToText(), deleteName, CMess.Deck.ToText(), CMess.tlDelete.ToText()),
                new[] { CMess.ok.ToText() });

            if (DeckViewModel.Instance.Decks.Count == 0)
                CurrentDeck = null;
            else
            {
                int newIndex = deckIndex - 1;

                if (newIndex < 0) newIndex = 0;
                if (newIndex >= DeckViewModel.Instance.Decks.Count) newIndex = DeckViewModel.Instance.Decks.Count - 1;
                CurrentDeck = DeckViewModel.Instance.Decks[newIndex];
            }
        }
        private bool HasSelectedDeck()
        {
            return CurrentDeck != null;
        }
        #endregion

        #region Image
        private void ViewImageCard()
        {
            if (CurrentCard == null) return;
            string imagePath = CardImageCacheViewModel.Instance.GetImagePath(CurrentCard.ID);

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
        private void OpenFileLocal()
        {
            if (CurrentCard == null) return;
            string imagePath = CardImageCacheViewModel.Instance.GetImagePath(CurrentCard.ID);
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
            if (CurrentCard == null || MainWindowService == null) return;
            if (!CardEXDataViewModel.Instance.IsLoadedCard) await CardEXDataViewModel.Instance.LoadCardsEXAsync();

            await MainWindowService.OpenDataEditorTab(CurrentCard.ID);
        }
        private async Task OpenScript()
        {
            if (CurrentCard == null || MainWindowService == null) return;
            if (!CardEXDataViewModel.Instance.IsLoadedScript) await CardEXDataViewModel.Instance.LoadScriptAsync();

            foreach (var id in new[] { CurrentCard.BaseCard.alias, CurrentCard.BaseCard.id }.Where(x => x > 0).Distinct())
            {
                await MainWindowService.OpenCodeEditorTab(id);
            }
        }
        #endregion

        #region ReName
        public void OpenReNameDeck()
        {
            popReNameDeck.IsOpen = true;
        }
        private void CreateNewDeck()
        {
            string deckPath = GetNewFolderPath();
            if (string.IsNullOrEmpty(deckPath)) return;

            string newName = JoinStringHelper.SanitizationString(NewNameDeck.Trim());
            var ext = System.IO.Path.GetExtension(newName);
            if (string.IsNullOrEmpty(ext) || ext.Length <= 1)
            {
                if (newName.EndsWith("."))
                    newName = newName.TrimEnd('.');
                newName += ".ydk";
            }

            var (resultFile, messageFile) = DeckViewModel.Instance.CreateNewDeckFile(newName, null, deckPath);
            if (!resultFile)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageFile}", new[] { CMess.ok.ToText() });
                return;
            }
            var (resultMemory, messageMemory) = DeckViewModel.Instance.CreateNewDeckMemory(
                    System.IO.Path.GetFileNameWithoutExtension(messageFile), messageFile);
            if (!resultMemory)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageMemory}", new[] { CMess.ok.ToText() });
                return;
            }
            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                string.Format(CMess.ThreePlaceholderSuccess.ToText(), newName, CMess.Deck.ToText(), CMess.Create.ToText()),
                new[] { CMess.ok.ToText() });

            var newdeck = DeckViewModel.Instance.Decks.FirstOrDefault(d => d.Path == messageFile);
            if (newdeck != null) CurrentDeck = newdeck;
        }
        private void ReNameDeck()
        {
            string deckPath = GetNewFolderPath();
            if (string.IsNullOrEmpty(deckPath)) return;

            string newName = JoinStringHelper.SanitizationString(NewNameDeck.Trim());
            var ext = System.IO.Path.GetExtension(newName);
            if (string.IsNullOrEmpty(ext) || ext.Length <= 1)
            {
                if (newName.EndsWith("."))
                    newName = newName.TrimEnd('.');
                newName += ".ydk";
            }

            string oldPath = CurrentDeck.Path;
            string oldName = CurrentDeck.Name;

            var (resultFile, newPathOrError) = DeckViewModel.Instance.ReNameDeckFile(oldPath, newName);
            if (!resultFile)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {newPathOrError}", new[] { CMess.ok.ToText() });
                return;
            }
            var (resultMemory, messageMemory) = DeckViewModel.Instance.RenameDeckMemory(oldPath, newPathOrError, newName);
            if (!resultMemory)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageMemory}", new[] { CMess.ok.ToText() });
                return;
            }
            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                string.Format(CMess.renameSuc.ToText(), CMess.Deck.ToText(), oldName, newName),
                new[] { CMess.ok.ToText() });

        }
        private bool ReNameInValid()
        {
            if (string.IsNullOrWhiteSpace(NewNameDeck)) return false;
            if (string.IsNullOrWhiteSpace(NewFolderPath) || !Directory.Exists(NewFolderPath)) return false;
            return true;
        }
        #endregion

        #region YDKE

        #region Property
        private bool? _overwriteYDKE = false;
        public bool? OverwriteYDKE
        {
            get => _overwriteYDKE;
            set
            {
                if (_overwriteYDKE != value)
                {
                    _overwriteYDKE = value;
                    OnPropertyChanged(nameof(OverwriteYDKE));
                }
            }
        }
        public bool _systemType = false;
        public bool SystemType
        {
            get => _systemType;
            set
            {
                if (_systemType != value)
                {
                    _systemType = value;
                    OnPropertyChanged(nameof(SystemType));
                }
            }
        }
        private string _YDKEString = string.Empty;
        public string YDKEString
        {
            get => _YDKEString;
            set
            {
                if (_YDKEString != value)
                {
                    _YDKEString = value;
                    OnPropertyChanged(nameof(YDKEString));
                    OnYDKEStringChanged();
                    YDKEImportCommand.RaiseCanExecuteChanged();
                    CopyYDKEURLCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private void OnYDKEStringChanged()
        {
            IconCopy.Kind = MaterialDesignThemes.Wpf.PackIconKind.ContentCopy;
        }
        #endregion

        private void YDKEImport()
        {
            if (YDKEString.StartsWith("ydke://"))
            {
                ImportYDKEURL();
            }
            else if (YDKEString.StartsWith("#created by") ||
                YDKEString.StartsWith("#main") ||
                YDKEString.StartsWith("#extra") ||
                YDKEString.StartsWith("!side"))
            {
                ImportYDKEText();
            }
            else
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()),
                    new[] { CMess.ok.ToText() });
            }
        }
        private bool CanYDKEImport()
        {
            if (string.IsNullOrWhiteSpace(YDKEString)) return false;
            if (YDKEString.StartsWith("ydke://") ||
                YDKEString.StartsWith("#created by") ||
                YDKEString.StartsWith("#main") ||
                YDKEString.StartsWith("#extra") ||
                YDKEString.StartsWith("!side")) return true;
            else return false;
        }

        private void ExportLinkYDKE()
        {
            Debug.WriteLine("ExportLinkYDKE");

            List<ulong> MainCardID = new List<ulong>();
            List<ulong> ExtraCardID = new List<ulong>();
            List<ulong> SideCardID = new List<ulong>();

            foreach(var card in MainDeck)
            {
                MainCardID.Add(card.Card.ID);
            }
            foreach(var card in ExtraDeck)
            {
                ExtraCardID.Add(card.Card.ID);
            }
            foreach (var card in SideDeck)
            {
                SideCardID.Add(card.Card.ID);
            }

            string ydkeString = string.Empty;
            if (SystemType) //64 bit
            {
                var ydke = YDKEURLHelper64.PasscodesToBase64(MainCardID, ExtraCardID, SideCardID);
                if (!ydke.result)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ydke.message}", new[] { CMess.ok.ToText() });
                    return;
                }
                ydkeString = ydke.message;
            }
            else //32 bit
            {
                var ydke = YDKEURLHelper32.PasscodesToBase64(MainCardID, ExtraCardID, SideCardID);
                if (!ydke.result)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ydke.message}", new[] { CMess.ok.ToText() });
                    return;
                }
                ydkeString = ydke.message;
            }
            YDKEString = ydkeString;
        }

        private class DeckExportInfo
        {
            public int Count { get; set; }
            public ulong CardID { get; set; }
            public string Name { get; set; }
        }
        private void ExportTextYDKE()
        {
            StringBuilder PlainText = new StringBuilder();
            List<DeckExportInfo> MainDeckInfo = new List<DeckExportInfo>();
            List<DeckExportInfo> ExtraDeckInfo = new List<DeckExportInfo>();
            List<DeckExportInfo> SideDeckInfo = new List<DeckExportInfo>();

            foreach (var card in MainDeck)
            {
                var existingInfo = MainDeckInfo.FirstOrDefault(c => c.CardID == card.Card.ID);
                if (existingInfo != null)
                {
                    existingInfo.Count++;
                }
                else
                {
                    MainDeckInfo.Add(new DeckExportInfo
                    {
                        Count = 1,
                        CardID = card.Card.ID,
                        Name = card.Card.BaseCard.name
                    });
                }
            }
            foreach (var card in ExtraDeck)
            {
                var existingInfo = ExtraDeckInfo.FirstOrDefault(c => c.CardID == card.Card.ID);
                if (existingInfo != null)
                {
                    existingInfo.Count++;
                }
                else
                {
                    ExtraDeckInfo.Add(new DeckExportInfo
                    {
                        Count = 1,
                        CardID = card.Card.ID,
                        Name = card.Card.BaseCard.name
                    });
                }
            }
            foreach (var card in SideDeck)
            {
                var existingInfo = SideDeckInfo.FirstOrDefault(c => c.CardID == card.Card.ID);
                if (existingInfo != null)
                {
                    existingInfo.Count++;
                }
                else
                {
                    SideDeckInfo.Add(new DeckExportInfo
                    {
                        Count = 1,
                        CardID = card.Card.ID,
                        Name = card.Card.BaseCard.name
                    });
                }
            }

            PlainText.AppendLine($"{CMess.MainDeck.ToText()}:");
            foreach (var cardInfo in MainDeckInfo)
            {
                PlainText.AppendLine($"{cardInfo.CardID}\t{cardInfo.Count} x {cardInfo.Name}");
            }
            PlainText.AppendLine($"{CMess.ExtraDeck.ToText()}:");
            foreach (var cardInfo in ExtraDeckInfo)
            {
                PlainText.AppendLine($"{cardInfo.CardID}\t{cardInfo.Count} x {cardInfo.Name}");
            }
            PlainText.AppendLine($"{CMess.SideDeck.ToText()}:");
            foreach (var cardInfo in SideDeckInfo)
            {
                PlainText.AppendLine($"{cardInfo.CardID}\t{cardInfo.Count} x {cardInfo.Name}");
            }
            YDKEString = PlainText.ToString();
        }

        private void ExportDeckYDKE()
        {
            StringBuilder DeckText = new StringBuilder();
            DeckText.AppendLine($"#created by {ConfigViewModel.Instance.userSetting.UserName}");

            if (SaveName)
            {
                DeckText.AppendLine("#main");
                foreach(var card in MainDeck)
                {
                    DeckText.AppendLine($"# {card.Card.BaseCard.name}");
                    DeckText.AppendLine(card.Card.ID.ToString());
                }
                DeckText.AppendLine("#extra");
                foreach (var card in ExtraDeck)
                {
                    DeckText.AppendLine($"# {card.Card.BaseCard.name}");
                    DeckText.AppendLine(card.Card.ID.ToString());
                }
                DeckText.AppendLine("!side");
                foreach (var card in SideDeck)
                {
                    DeckText.AppendLine($"# {card.Card.BaseCard.name}");
                    DeckText.AppendLine(card.Card.ID.ToString());
                }
            }
            else
            {
                DeckText.AppendLine("#main");
                foreach (var card in MainDeck)
                {
                    DeckText.AppendLine(card.Card.ID.ToString());
                }
                DeckText.AppendLine("#extra");
                foreach (var card in ExtraDeck)
                {
                    DeckText.AppendLine(card.Card.ID.ToString());
                }
                DeckText.AppendLine("!side");
                foreach (var card in SideDeck)
                {
                    DeckText.AppendLine(card.Card.ID.ToString());
                }
            }
            YDKEString = DeckText.ToString();
        }
        private void ImportYDKEURL()
        {
            if (string.IsNullOrEmpty(YDKEString)) return;

            List<ulong> MainCardID = new();
            List<ulong> ExtraCardID = new();
            List<ulong> SideCardID = new();

            if (YDKEString.EndsWith("!64"))
            {
                var result64 = YDKEURLHelper64.Base64ToPasscodes(YDKEString);
                if (result64.result)
                {
                    MainCardID = result64.MainDeck;
                    ExtraCardID = result64.ExtraDeck;
                    SideCardID = result64.SideDeck;
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {result64.message}", new[] { CMess.ok.ToText() });
                    return;
                }
            }
            else
            {
                var result32 = YDKEURLHelper32.Base64ToPasscodes(YDKEString);
                if (result32.result)
                {
                    MainCardID = ConvertUintListToUlongList(result32.MainDeck);
                    ExtraCardID = ConvertUintListToUlongList(result32.ExtraDeck);
                    SideCardID = ConvertUintListToUlongList(result32.SideDeck);
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {result32.message}", new[] { CMess.ok.ToText() });
                    return;
                }
            }

            if (OverwriteYDKE == true)
            {
                var newMainDeck = CardEXDataViewModel.Instance.GetListCardInstance(MainCardID);
                var newExtraDeck = CardEXDataViewModel.Instance.GetListCardInstance(ExtraCardID);
                var newSideDeck = CardEXDataViewModel.Instance.GetListCardInstance(SideCardID);
                if (newMainDeck != null && newMainDeck.Count > 0)
                {
                    MainDeck.Clear();
                    MainDeck.AddRange(newMainDeck);
                }
                if (newExtraDeck != null && newExtraDeck.Count > 0)
                {
                    ExtraDeck.Clear();
                    ExtraDeck.AddRange(newExtraDeck);
                }
                if (newSideDeck != null && newSideDeck.Count > 0)
                {
                    SideDeck.Clear();
                    SideDeck.AddRange(newSideDeck);
                }
            }
            else if (OverwriteYDKE == false)
            {
                var newMainDeck = CardEXDataViewModel.Instance.GetListCardInstance(MainCardID);
                var newExtraDeck = CardEXDataViewModel.Instance.GetListCardInstance(ExtraCardID);
                var newSideDeck = CardEXDataViewModel.Instance.GetListCardInstance(SideCardID);
                if (newMainDeck != null && newMainDeck.Count > 0)
                {
                    MainDeck.AddRange(newMainDeck);
                }
                if (newExtraDeck != null && newExtraDeck.Count > 0)
                {
                    ExtraDeck.AddRange(newExtraDeck);
                }
                if (newSideDeck != null && newSideDeck.Count > 0)
                {
                    SideDeck.AddRange(newSideDeck);
                }
            }
            else
            {
                var (newDeck, message) = DeckViewModel.Instance.AddNewDeckMemoryFromListIDs(MainCardID, ExtraCardID, SideCardID);
                if (newDeck == null)
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                else
                {
                    CurrentDeck = newDeck;
                }
            }
        }
        private void ImportYDKEText()
        {
            if (string.IsNullOrEmpty(YDKEString)) return;

            var (MainCardID, ExtraCardID, SideCardID, messageYDK) = DeckViewModel.Instance.GetListIDFromYDK(YDKEString);
            if (MainCardID == null || ExtraCardID == null || SideCardID == null)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageYDK}", new[] { CMess.ok.ToText() });
                return;
            }

            if (OverwriteYDKE == true)
            {
                var newMainDeck = CardEXDataViewModel.Instance.GetListCardInstance(MainCardID);
                var newExtraDeck = CardEXDataViewModel.Instance.GetListCardInstance(ExtraCardID);
                var newSideDeck = CardEXDataViewModel.Instance.GetListCardInstance(SideCardID);
                if (newMainDeck != null && newMainDeck.Count > 0)
                {
                    MainDeck.Clear();
                    MainDeck.AddRange(newMainDeck);
                }
                if (newExtraDeck != null && newExtraDeck.Count > 0)
                {
                    ExtraDeck.Clear();
                    ExtraDeck.AddRange(newExtraDeck);
                }
                if (newSideDeck != null && newSideDeck.Count > 0)
                {
                    SideDeck.Clear();
                    SideDeck.AddRange(newSideDeck);
                }
            }
            else if (OverwriteYDKE == false)
            {
                var newMainDeck = CardEXDataViewModel.Instance.GetListCardInstance(MainCardID);
                var newExtraDeck = CardEXDataViewModel.Instance.GetListCardInstance(ExtraCardID);
                var newSideDeck = CardEXDataViewModel.Instance.GetListCardInstance(SideCardID);
                if (newMainDeck != null && newMainDeck.Count > 0)
                {
                    MainDeck.AddRange(newMainDeck);
                }
                if (newExtraDeck != null && newExtraDeck.Count > 0)
                {
                    ExtraDeck.AddRange(newExtraDeck);
                }
                if (newSideDeck != null && newSideDeck.Count > 0)
                {
                    SideDeck.AddRange(newSideDeck);
                }
            }
            else
            {
                var (newDeck, message) = DeckViewModel.Instance.AddNewDeckMemoryFromListIDs(MainCardID, ExtraCardID, SideCardID);
                if (newDeck == null)
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                else
                {
                    CurrentDeck = newDeck;
                }
            }
        }
        private List<ulong> ConvertUintListToUlongList(List<uint> input)
        {
            if (input == null || input.Count == 0) return new List<ulong>();

            List<ulong> result = new List<ulong>(input.Count);
            foreach (var item in input)
            {
                result.Add((ulong)item);
            }
            return result;
        }
        private void CopyYDKEURL()
        {
            try
            {
                if (!string.IsNullOrEmpty(YDKEString))
                {
                    Clipboard.SetText(YDKEString);
                    IconCopy.Kind = MaterialDesignThemes.Wpf.PackIconKind.Check;
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #region WWeb
        private async Task OpenKonamiDB()
        {
            if (CurrentCard == null) return;
            if (MainWindowService != null)
            {
                await MainWindowService.OpenKonamiDB(CurrentCard.ID, CurrentCard.BaseCard.name);
            }
        }
        private async Task OpenYugipedia()
        {
            if (CurrentCard == null) return;
            if (MainWindowService != null)
            {
                await MainWindowService.OpenYugipedia(CurrentCard.ID, CurrentCard.BaseCard.name);
            }
        }
        private async Task OpenYGOResources()
        {
            if (CurrentCard == null) return;
            if (MainWindowService != null)
            {
                await MainWindowService.OpenYGOResources(CurrentCard.ID, CurrentCard.BaseCard.name);
            }
        }
        #endregion

        #endregion

        #region Card List
        private void CardListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ListCardScrollViewer == null && sender is System.Windows.Controls.ListView listView)
            {
                ListCardScrollViewer = FindVisualChild<ScrollViewer>(listView);
            }
            if (ListCardScrollViewer != null)
            {
                if (e.Delta > 0) ListCardScrollViewer.LineUp();
                else ListCardScrollViewer.LineDown();
                e.Handled = true;
            }
        }
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                    return typedChild;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
        private void CardListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeTimer.Stop();
            ResizeTimer.Start();
        }
        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            ResizeTimer.Stop();
            UpdateCardListItemSize();
        }
        private void UpdateCardListItemSize()
        {
            if (CardListView.ActualWidth <= 0 ||  CardListView.ActualHeight <= 0) return;
            if (CardListViewMode)
            {
                ListViewItemWidth = (CardListView.ActualWidth - 28) / 3;
            }
            else
            {
                ListViewItemHeight = CardListView.ActualHeight / 7;
            }
        }

        #endregion

        #region Card Information

        #region Card Name
        private void grCardName_MouseEnter(object sender, MouseEventArgs e)
        {
            isPaused = true;
            NameScrollViewer?.ScrollToHorizontalOffset(0);
        }
        private void grCardName_MouseLeave(object sender, MouseEventArgs e)
        {
            isPaused = false;
        }
        
        private void txtCardName_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // OpenScript();
        }

        public void UpdateMarqueeCardName()
        {
            if (NameScrollViewer == null)
                NameScrollViewer = GetDescendantByType<ScrollViewer>(txtCardName);
            txtCardName.UpdateLayout();
            StopMarquee();
            double scrollableWidth = NameScrollViewer.ScrollableWidth;
            if (scrollableWidth > 0) StartMarquee();
            else NameScrollViewer.ScrollToHorizontalOffset(0);
        }
        private void StartMarquee()
        {
            if (scrollTimer?.IsEnabled == true) return;
            scrollTimer?.Start();
        }
        private void StopMarquee()
        {
            scrollTimer?.Stop();
            NameScrollViewer?.ScrollToHorizontalOffset(0);
        }
        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            if (isPaused || NameScrollViewer == null) return;

            double maxOffset = NameScrollViewer.ScrollableWidth;
            if (maxOffset <= 0) return;

            double newOffset = NameScrollViewer.HorizontalOffset + scrollSpeed;
            if (newOffset >= maxOffset) NameScrollViewer.ScrollToHorizontalOffset(0);
            else NameScrollViewer.ScrollToHorizontalOffset(newOffset);
        }
        private static T GetDescendantByType<T>(DependencyObject element) where T : class
        {
            if (element == null) return null;
            if (element is T correctlyTyped) return correctlyTyped;

            int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(element, i);
                var result = GetDescendantByType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
        #endregion

        #region Card Properties
        private void UpdateCurrentCardBinding()
        {
            ViewImageCommand.RaiseCanExecuteChanged();
            OpenFileLocalCommand.RaiseCanExecuteChanged();
            OpenDatabaseCommand.RaiseCanExecuteChanged();
            OpenScriptCommand.RaiseCanExecuteChanged();
            if (CurrentCard != null) UpdateUICardProperties();
            else ClearUICardProperties();

            UpdateMarqueeCardName();
        }
        private void UpdateUICardProperties()
        {
            //string path = CardEXDataViewModel.Instance.TryGetPath(CurrentCard.ID);
            var CardInfo = new CardItemInfo(CurrentCard.BaseCard.type);

            imageCard.Source = CardImageCacheViewModel.Instance.GetFullCardImage(CurrentCard.ID) ?? CardImageCacheViewModel.Instance.BlankImage;
            txtCardName.Text = CurrentCard.BaseCard.name;

            string typeFind = string.Empty, raceFind = string.Empty, charFind = string.Empty, attriFind = string.Empty;

            if (CardDataViewModel.Instance.listtype.Count > 0)
                typeFind = FindIInfoService.FindCardInfo(CardDataViewModel.Instance.listtypedeck, CurrentCard.BaseCard.type, true).Trim();

            if (CardInfo.IsSkill && CardDataViewModel.Instance.listchar.Count > 0)
                charFind = FindIInfoService.FindCardInfo(CardDataViewModel.Instance.listchar, CurrentCard.BaseCard.race, true).Trim();

            if (CardDataViewModel.Instance.listrace.Count > 0)
                raceFind = FindIInfoService.FindCardInfo(CardDataViewModel.Instance.listrace, CurrentCard.BaseCard.race, true).Trim();

            if (CardDataViewModel.Instance.listattr.Count > 0)
                attriFind = FindIInfoService.FindCardInfo(CardDataViewModel.Instance.listattr, CurrentCard.BaseCard.attribute, true).Trim();

            tblCardType.Text = $"[{typeFind}] {JoinStringHelper.JoinWithSeparator("|", attriFind, (CardInfo.IsSkill ? charFind : raceFind))}";

            string star, level, atk, def;
            if (CardInfo.IsXyz && !CardInfo.IsNonXyz) star = "☆";
            else if (CardInfo.IsXyz && CardInfo.IsNonXyz) star = "⯪";
            else star = "★";

            if (CardInfo.IsLink)
            {
                string linkrating, linkarrow;
                var (DecoLinkarrow, DecoDef, notHasATK) = GetInfoService.DecodeDef(CurrentCard.BaseCard.def);

                if (CardInfo.IsMonster)
                {
                    level = $"[{star}{((CurrentCard.BaseCard.level >> 8) & 0xff)}]";
                    atk = notHasATK ? string.Empty : (CurrentCard.BaseCard.atk >= 0 ? CurrentCard.BaseCard.atk.ToString() : "?");
                    def = DecoDef.HasValue ? (DecoDef >= 0 ? DecoDef.ToString() : "?") : string.Empty;
                }
                else
                {
                    level = string.Empty;
                    atk = string.Empty;
                    def = string.Empty;
                }

                linkrating = $"LINK {(CurrentCard.BaseCard.level & 0xff)}";
                if (CardDataViewModel.Instance.listlinkarrow.Count > 0)
                    linkarrow = FindIInfoService.FindCardInfo(CardDataViewModel.Instance.listlinkarrow, (ulong)DecoLinkarrow, true).Trim();
                else linkarrow = string.Empty;

                tblCardLvRk.Text = $"{level} {JoinStringHelper.JoinWithSeparator("/", atk, def, linkrating)} [{linkarrow}] ";
            }
            else
            {
                if (CardInfo.IsMonster)
                {
                    level = $"[{star}{(CurrentCard.BaseCard.level & 0xff)}]";
                    atk = (CurrentCard.BaseCard.atk >= 0) ? CurrentCard.BaseCard.atk.ToString() : "?";
                    def = (CurrentCard.BaseCard.def >= 0) ? CurrentCard.BaseCard.def.ToString() : "?";

                    tblCardLvRk.Text = $"{level} {JoinStringHelper.JoinWithSeparator("/", atk, def)}";
                }
                else
                {
                    tblCardLvRk.Text = string.Empty;
                }
            }

            string rightScale = ((CurrentCard.BaseCard.level >> 16) & 0xff).ToString();
            string leftScale = ((CurrentCard.BaseCard.level >> 24) & 0xff).ToString();
            tblCardCardPenScale.Text = CardInfo.IsPendulum ? $"[{leftScale}|Scale|{rightScale}]" : string.Empty;

            grCardLvRK.Visibility = (string.IsNullOrWhiteSpace(tblCardLvRk.Text) && string.IsNullOrWhiteSpace(tblCardCardPenScale.Text))
                ? Visibility.Collapsed : Visibility.Visible;

            string SetCodeFind = FindIInfoService.FindSetcode(CardDataViewModel.Instance.listsetcode, CurrentCard.BaseCard.setcode).Trim();
            StackCardSetCode.Visibility = string.IsNullOrWhiteSpace(SetCodeFind) ? Visibility.Collapsed : Visibility.Visible;
            tblCardSetCode.Text = SetCodeFind;

            string rarity = RareRawDataViewModel.Instance.GetRareItemNamesFromCode(CurrentCard.Rare, "|");
            if (string.IsNullOrWhiteSpace(rarity)) tblCardRare.Text = string.Empty;
            else tblCardRare.Text = $"{CMess.Rarity.ToText()}: {rarity}  ";

            tblCardGPoint.Text = $"{CMess.genesysPoint.ToText()}: {CurrentCard.GPoint}";

            string ruleFind = string.Empty;
            if (CardDataViewModel.Instance.listrule.Count > 0)
                ruleFind = FindIInfoService.FindCardInfo(CardDataViewModel.Instance.listrule, CurrentCard.BaseCard.ot, true).Trim();
            tblCardID.Text = $"[{CurrentCard.ID}] {ruleFind}";

            txtcarddesc.Clear();
            txtcarddesc.AppendText(CurrentCard.BaseCard.desc);
            txtcarddesc.ScrollToHome();
        }
        private void ClearUICardProperties()
        {
            imageCard.Source = CardImageCacheViewModel.Instance.BlankImage;
            txtCardName.Text = "";

            tblCardType.Text = string.Empty;
            tblCardLvRk.Text = string.Empty;
            tblCardCardPenScale.Text = string.Empty;
            grCardLvRK.Visibility = Visibility.Collapsed;
            StackCardSetCode.Visibility = Visibility.Collapsed;
            tblCardSetCode.Text = string.Empty;

            tblCardRare.Text = string.Empty;
            tblCardGPoint.Text = string.Empty;

            tblCardID.Text = string.Empty;
            txtcarddesc.Clear();
        }
        #endregion

        #region Card Information Behaviors
        private void DeckListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_selectedCardInstance != null) return; // Không update hover nếu đã selected

            if (sender is System.Windows.Controls.ListViewItem item && item.DataContext is CardInstance card)
            {
                _hoveredCardInstance = card;

                _selectedCardInstance = null;
                _hoveredCardEX = null;
                _selectedCardEX = null;

                UpdateCurrentCardBinding();
            }
        }
        private void DeckListViewItem_MouseLeave(object sender, MouseEventArgs e)
        {
            /// Không update
        }
        private void DeckListViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging) return;

            if (sender is System.Windows.Controls.ListViewItem item && item.DataContext is CardInstance instance)
            {
                System.Windows.Controls.ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as System.Windows.Controls.ListView;
                if (_selectedCardInstance?.Equals(instance) ?? false)
                {
                    _selectedCardInstance = null;
                    _hoveredCardInstance = null;

                    _hoveredCardEX = null;
                    _selectedCardEX = null;

                    listView.SelectedItem = null;
                }
                else
                {
                    _selectedCardInstance = instance;
                    _hoveredCardInstance = null;
                    _selectedCardEX = null;
                    _hoveredCardEX = null;
                }
            }
            UpdateCurrentCardBinding();
        }

        private void CardListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_selectedCardEX != null) return; // Không update hover nếu đã selected

            if (sender is System.Windows.Controls.ListViewItem item && item.DataContext is CardEX card)
            {
                _hoveredCardEX = card;

                _selectedCardEX = null;
                _hoveredCardInstance = null;
                _selectedCardInstance = null;
                UpdateCurrentCardBinding();
            }
        }
        private void CardListViewItem_MouseLeave(object sender, MouseEventArgs e)
        {
            /// Không update
        }
        private void CardListViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging) return;

            if (sender is System.Windows.Controls.ListViewItem item && item.DataContext is CardEX card)
            {
                System.Windows.Controls.ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as System.Windows.Controls.ListView;
                if (_selectedCardEX == card)
                {
                    _selectedCardEX = null;
                    _hoveredCardEX = null;

                    _selectedCardInstance = null;
                    _hoveredCardInstance = null;

                    listView.SelectedItem = null;
                }
                else
                {
                    _selectedCardEX = card;
                    _hoveredCardEX = null;

                    _selectedCardInstance = null;
                    _hoveredCardInstance = null;
                }
            }
            UpdateCurrentCardBinding();
        }
        #endregion

        #endregion

        #region Card Filter

        #region Fields
        private CancellationTokenSource _textFilterCts;
        private CancellationTokenSource _immediateFilterCts;
        private Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();
        private Dictionary<string, Regex> _compiledRegexes;
        private bool _isRegexDirty = true;
        private bool _isFilterDirty = false;
        #endregion

        #region Advanced Filter
        private const long maskHasDEF = 1L << 31;
        private const long maskLinkArrow = 1L << 4;
        private bool IsLink = false;
        private bool isAdvancedFind = false;
        private bool matchCase = false;
        private bool useWildcards = false;
        private bool matchPrefix = false;
        private bool matchSuffix = false;
        private bool wholeWords = false;
        private bool ignorePunctuation = false;
        private bool ignoreWhitespace = false;
        private StringComparison _comparison;
        #endregion

        #region Card Element
        private ulong AlternateRule;
        private DeckEditData CurrentData;
        private DeckEditData _viewData;
        public DeckEditData ViewData
        {
            get => _viewData;
            set
            {
                if (_viewData != value)
                {
                    if (_viewData != null)
                        _viewData.PropertyChanged -= OnViewDataPropertyChanged;

                    _viewData = value;
                    OnPropertyChanged(nameof(ViewData));
                    
                    if (_viewData != null)
                        _viewData.PropertyChanged += OnViewDataPropertyChanged;
                }
            }
        }
        private void OnViewDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DeckEditData.CardDesc):
                    MarkFilterDirty();
                    OnTextFilterChanged();
                    break;

                default:
                    MarkFilterDirty();
                    OnNonTextFilterChanged();
                    break;
            }
        }
        #endregion

        public int ResultCards => FilteredCards?.Count() ?? 0;
        private void FilteredCards_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ResultCards));
        }

        #region Match String
        private string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            string normalized = input;
            if (ignorePunctuation)
                normalized = new string(normalized.Where(c => !char.IsPunctuation(c)).ToArray());
            if (ignoreWhitespace)
                normalized = Regex.Replace(normalized, @"\s+", "");

            return normalized;
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
        private Regex GetOrCreateRegex(string pattern, bool matchCase)
        {
            string key = $"{pattern}|{matchCase}";
            if (!_regexCache.TryGetValue(key, out Regex regex))
            {
                var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                regex = new Regex(pattern, options | RegexOptions.Compiled);
                _regexCache[key] = regex;
            }
            return regex;
        }
        private bool MatchString(string source, string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return true;
            if (string.IsNullOrWhiteSpace(source)) return false;

            string normalizedSource = NormalizeString(source);
            string normalizedPattern = NormalizeString(pattern);

            if (!isAdvancedFind)
            {
                return normalizedSource.IndexOf(normalizedPattern, _comparison) >= 0;
            }

            if (useWildcards || wholeWords)
            {
                if (_compiledRegexes != null && _compiledRegexes.TryGetValue(pattern, out var regex))
                {
                    return regex.IsMatch(normalizedSource);
                }

                string regexPattern = useWildcards
                    ? BuildRegexPattern(normalizedPattern, matchPrefix, matchSuffix, wholeWords)
                    : $@"\b{Regex.Escape(normalizedPattern)}\b";

                return GetOrCreateRegex(regexPattern, matchCase).IsMatch(normalizedSource);
            }

            int index = normalizedSource.IndexOf(normalizedPattern, _comparison);
            if (index < 0) return false;

            if (matchPrefix && index != 0) return false;
            if (matchSuffix && (index + normalizedPattern.Length) != normalizedSource.Length) return false;

            return true;
        }
        private void EnsureRegexCompiled()
        {
            if (!_isRegexDirty) return;

            _compiledRegexes = new Dictionary<string, Regex>();
            if (isAdvancedFind && (useWildcards || wholeWords))
            {
                if (!string.IsNullOrWhiteSpace(ViewData.CardDesc))
                {
                    string normalized = NormalizeString(ViewData.CardDesc);
                    string pattern = useWildcards
                        ? BuildRegexPattern(normalized, matchPrefix, matchSuffix, wholeWords)
                        : wholeWords ? $@"\b{Regex.Escape(normalized)}\b" : null;
                    if (pattern != null)
                    {
                        _compiledRegexes[ViewData.CardDesc] = GetOrCreateRegex(pattern, matchCase);
                    }
                }
            }
            _isRegexDirty = false;
        }
        private bool MatchStringWithCriteria(string source, string pattern, Dictionary<string, Regex> compiledRegexes)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return true;
            if (string.IsNullOrWhiteSpace(source)) return false;

            string normalizedSource = NormalizeString(source);
            string normalizedPattern = NormalizeString(pattern);

            if (!isAdvancedFind)
                return normalizedSource.IndexOf(normalizedPattern, _comparison) >= 0;

            if (compiledRegexes != null && compiledRegexes.TryGetValue(pattern, out var regex))
            {
                return regex.IsMatch(normalizedSource);
            }

            // Fallback nếu không có compiled regex
            return normalizedSource.IndexOf(normalizedPattern, _comparison) >= 0;
        }
        #endregion

        private void MarkFilterDirty()
        {
            _isFilterDirty = true;
        }
        private async void OnTextFilterChanged()
        {
            _textFilterCts?.Cancel();
            _textFilterCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(400, _textFilterCts.Token);
                await RefreshFilter();
            }
            catch (TaskCanceledException) { }
        }
        private async void OnNonTextFilterChanged()
        {
            _immediateFilterCts?.Cancel();
            _immediateFilterCts = new CancellationTokenSource();

            try
            {
                await Task.Delay(200, _immediateFilterCts.Token);
                await RefreshFilter();
            }
            catch (TaskCanceledException) { }
        }
        private async Task RefreshFilter()
        {
            if (!_isFilterDirty) return;
            _isFilterDirty = false;

            EnsureRegexCompiled();
            await ApplyFilterAsync();
        }
        private async Task ApplyFilterAsync()
        {
            _filterCts?.Cancel();
            _filterCts = new CancellationTokenSource();
            var token = _filterCts.Token;

            if (!await _filterLock.WaitAsync(0)) return;

            try
            {
                var allCards = CardEXDataViewModel.Instance.AllCards.Values;
                CurrentData = ViewData;

                if (ConfigViewModel.Instance.dataHandlingSetting.FilterMode == 0)
                {
                    var filtered = await Task.Run(() =>
                    {
                        var result = allCards
                        .AsParallel()
                        .WithCancellation(token)
                        .Where(card => FilterAND(card))
                        .AsEnumerable();

                        result = ApplySorting(result);
                        return result.ToList();
                    }, token);
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        FilteredCards.ReplaceAll(filtered);
                    }, DispatcherPriority.Background);
                }
                else if (ConfigViewModel.Instance.dataHandlingSetting.FilterMode == 1)
                {
                    var filtered = await Task.Run(() =>
                    {
                        var result = allCards
                        .AsParallel()
                        .WithCancellation(token)
                        .Where(card => FilterOR(card))
                        .AsEnumerable();

                        //var result = allCards.Where(card =>
                        //{
                        //    token.ThrowIfCancellationRequested();
                        //    return FilterOR(card);
                        //});
                        result = ApplySorting(result);
                        return result.ToList();
                    }, token);
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        FilteredCards.ReplaceAll(filtered);
                    }, DispatcherPriority.Background);
                }
                else if (ConfigViewModel.Instance.dataHandlingSetting.FilterMode == 2)
                {
                    var filtered = await Task.Run(() =>
                    {
                        var result = allCards
                        .AsParallel()
                        .WithCancellation(token)
                        .Where(card => FilterMixedAND_OR(card))
                        .AsEnumerable();

                        //var result = allCards.Where(card =>
                        //{
                        //    token.ThrowIfCancellationRequested();
                        //    return FilterMixedAND_OR(card);
                        //});
                        result = ApplySorting(result);
                        return result.ToList();
                    }, token);
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        FilteredCards.ReplaceAll(filtered);
                    }, DispatcherPriority.Background);
                }
                else if (ConfigViewModel.Instance.dataHandlingSetting.FilterMode == 3)
                {
                    var filtered = await Task.Run(() =>
                    {
                        var result = allCards
                        .AsParallel()
                        .WithCancellation(token)
                        .Where(card => FilterMixedOR_AND(card))
                        .AsEnumerable();

                        //var result = allCards.Where(card =>
                        //{
                        //    token.ThrowIfCancellationRequested();
                        //    return FilterMixedOR_AND(card);
                        //});
                        result = ApplySorting(result);
                        return result.ToList();
                    }, token);
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        FilteredCards.ReplaceAll(filtered);
                    }, DispatcherPriority.Background);
                }
                else
                {
                    var filtered = await Task.Run(() =>
                    {
                        var result = allCards
                        .AsParallel()
                        .WithCancellation(token)
                        .Where(card => FilterAND(card))
                        .AsEnumerable();

                        //var result = allCards.Where(card =>
                        //{
                        //    token.ThrowIfCancellationRequested();
                        //    return FilterAND(card);
                        //});
                        result = ApplySorting(result);
                        return result.ToList();
                    }, token);
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        FilteredCards.ReplaceAll(filtered);
                    }, DispatcherPriority.Background);
                }
                
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlFilter.ToText(), CMess.Card.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                });
            }
            finally
            {
                _filterLock.Release();
            }
        }
        private IEnumerable<CardEX> ApplySorting(IEnumerable<CardEX> source)
        {
            var sortItems = SortsViewModel.Instance.SelectedSortItems;
            if (sortItems == null || !sortItems.Any())
                return source;

            var orderByString = string.Join(", ", sortItems.Select(item =>
            {
                var propertyName = item.SelectedItem.Name;
                var path = (propertyName == "Rare" || propertyName == "GPoint")
                    ? propertyName
                    : $"BaseCard.{propertyName}";
                var direction = item.OrderByAsc ? "" : " descending";
                return $"{path}{direction}";
            }));

            return source.AsQueryable().OrderBy(orderByString).AsEnumerable();
        }

        private bool FilterAND(CardEX card)
        {
            if (SelectedBanList != null && CurrentData.Limit != 5)
            {
                bool existInList = SelectedBanList.CardList.TryGetValue(card.ID, out CardBanList banInfo);

                if (SelectedBanList.WhiteList) // Danh sách trắng. Không có trong CardList => Không tồn tại.
                {
                    if (!existInList || banInfo == null) return false; // Không tồn tại.
                    if (CurrentData.Limit != 4) // Khác Allowed Card
                    {
                        if (banInfo.LimitedCount != CurrentData.Limit) return false;
                    }
                }
                else // Danh sách đen. Không có trong CardList => UnLimited.
                {
                    if (!existInList || banInfo == null) // Không tồn tại.
                    {
                        if (CurrentData.Limit != 3) return false; // Không phải UnLimited
                    }
                    else
                    {
                        if (banInfo.LimitedCount != CurrentData.Limit) return false; // Có tồn tại nhưng khác giới hạn.
                    }
                }
            }

            if (CurrentData.CardID.HasValue && card.ID != CurrentData.CardID.Value && card.BaseCard.alias != CurrentData.CardID.Value) return false;
            if (AlternateFormats)
            {
                if (CurrentData.SelectedRule != 0 && (card.BaseCard.ot & CurrentData.SelectedRule) != CurrentData.SelectedRule) return false;
            }
            else
            {
                if ((card.BaseCard.ot & AlternateRule) != 0) return false;
                if (CurrentData.SelectedRule != 0 && (card.BaseCard.ot & CurrentData.SelectedRule) != CurrentData.SelectedRule) return false;
            }

            if (CurrentData.SelectedType != 0 && (card.BaseCard.type & CurrentData.SelectedType) != CurrentData.SelectedType) return false;
            if (CurrentData.SelectedAttribute != 0 && (card.BaseCard.attribute & CurrentData.SelectedAttribute) != CurrentData.SelectedAttribute) return false;
            if (CurrentData.SelectedRace != 0 && (card.BaseCard.race & CurrentData.SelectedRace) != CurrentData.SelectedRace) return false;
            if (MatchesSetcode.MatchesSetCode_AND(card.BaseCard.setcode, CurrentData.SelectedSetcode) == false) return false;
            if (CurrentData.SelectedCategory != 0 && (card.BaseCard.category & CurrentData.SelectedCategory) != CurrentData.SelectedCategory) return false;
            if (CurrentData.SelectedFlag != 0 && (card.BaseCard.flag & CurrentData.SelectedFlag) != CurrentData.SelectedFlag) return false;
            if (CurrentData.SelectedRarity != 0 && (card.Rare & CurrentData.SelectedRarity) != CurrentData.SelectedRarity) return false;
            ///////////////////////
            int low8 = (int)(card.BaseCard.level & 0xFF);
            int high8 = (int)((card.BaseCard.level >> 8) & 0xFF);
            if (IsLink)
            {
                if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value != low8) return false;
                if (CurrentData.CardLevel.HasValue && CurrentData.CardLevel.Value != high8) return false;
                if (CurrentData.CardDEF.HasValue || CurrentData.LinkArrows != 0)
                {
                    var (matchDEF, matchLink) = MatchesDEFLink_AND(card.BaseCard.def);
                    if (!matchDEF || !matchLink) return false;
                }
            }
            else
            {
                if (CurrentData.CardLevel.HasValue && CurrentData.CardLevel.Value != low8) return false;
                // if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value != high8) return false;
                if (CurrentData.CardDEF.HasValue)
                {
                    if (CurrentData.CardDEF.Value >= 0 && card.BaseCard.def != CurrentData.CardDEF.Value) return false;
                    if (CurrentData.CardDEF.Value < 0 && card.BaseCard.def >= 0) return false;
                }
            }
            if (CurrentData.ScaleLeft.HasValue && CurrentData.ScaleLeft.Value != (int)((card.BaseCard.level >> 24) & 0xFF)) return false;
            if (CurrentData.ScaleRight.HasValue && CurrentData.ScaleRight.Value != (int)((card.BaseCard.level >> 16) & 0xFF)) return false;

            if (CurrentData.CardATK.HasValue)
            {
                if (CurrentData.CardATK.Value >= 0 && card.BaseCard.atk != CurrentData.CardATK.Value) return false;
                if (CurrentData.CardATK.Value < 0 && card.BaseCard.atk >= 0) return false;
            }

            if (CurrentData.GPoint.HasValue && card.GPoint != CurrentData.GPoint.Value) return false;

            if (!MatchString(card.BaseCard.name, CurrentData.CardDesc) && !MatchString(card.BaseCard.desc, CurrentData.CardDesc)) return false;

            return true;
        }
        private bool FilterOR(CardEX card)
        {
            if (SelectedBanList != null && CurrentData.Limit != 5)
            {
                bool existInList = SelectedBanList.CardList.TryGetValue(card.ID, out CardBanList banInfo);

                if (SelectedBanList.WhiteList) // Danh sách trắng. Không có trong CardList => Không tồn tại.
                {
                    if (!existInList || banInfo == null) return false; // Không tồn tại.
                    if (CurrentData.Limit != 4) // Khác Allowed Card
                    {
                        if (banInfo.LimitedCount != CurrentData.Limit) return false;
                    }
                }
                else // Danh sách đen. Không có trong CardList => UnLimited.
                {
                    if (!existInList || banInfo == null) // Không tồn tại.
                    {
                        if (CurrentData.Limit != 3) return false; // Không phải UnLimited
                    }
                    else
                    {
                        if (banInfo.LimitedCount != CurrentData.Limit) return false; // Có tồn tại nhưng khác giới hạn.
                    }
                }
            }

            if (CurrentData.CardID.HasValue && (card.ID == CurrentData.CardID.Value || card.BaseCard.alias == CurrentData.CardID.Value)) return true;
            if (AlternateFormats)
            {
                if (CurrentData.SelectedRule != 0 && (card.BaseCard.ot & CurrentData.SelectedRule) != 0) return true;
            }
            else
            {
                if ((card.BaseCard.ot & AlternateRule) != 0) return false;
                if (CurrentData.SelectedRule != 0 && (card.BaseCard.ot & CurrentData.SelectedRule) != 0) return true;
            }

            if (CurrentData.SelectedType != 0 && (card.BaseCard.type & CurrentData.SelectedType) != 0) return true;
            if (CurrentData.SelectedAttribute != 0 && (card.BaseCard.attribute & CurrentData.SelectedAttribute) != 0) return true;
            if (CurrentData.SelectedRace != 0 && (card.BaseCard.race & CurrentData.SelectedRace) != 0) return true;
            if (MatchesSetcode.MatchesSetCode_OR(card.BaseCard.setcode, CurrentData.SelectedSetcode) == true) return true;
            if (CurrentData.SelectedCategory != 0 && (card.BaseCard.category & CurrentData.SelectedCategory) != 0) return true;
            if (CurrentData.SelectedFlag != 0 && (card.BaseCard.flag & CurrentData.SelectedFlag) != 0) return true;
            if (CurrentData.SelectedRarity != 0 && (card.Rare & CurrentData.SelectedRarity) != 0) return true;
            ///////////////////////

            int low8 = (int)(card.BaseCard.level & 0xFF);
            int high8 = (int)((card.BaseCard.level >> 8) & 0xFF);
            if (IsLink)
            {
                if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value == low8) return true;
                if (CurrentData.CardLevel.HasValue && CurrentData.CardLevel.Value == high8) return true;
                if (CurrentData.CardDEF.HasValue || CurrentData.LinkArrows != 0)
                {
                    var (matchDEF, matchLink) = MatchesDEFLink_OR(card.BaseCard.def);
                    if (matchDEF || matchLink) return true;
                }
            }
            else
            {
                if (CurrentData.CardLevel.HasValue && CurrentData.CardLevel.Value == low8) return true;
                if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value == high8) return true;
                if (CurrentData.CardDEF.HasValue)
                {
                    if (CurrentData.CardDEF.Value >= 0 && card.BaseCard.def == CurrentData.CardDEF.Value) return true;
                    if (CurrentData.CardDEF.Value < 0 && card.BaseCard.def < 0) return true;
                }
            }
            if (CurrentData.ScaleLeft.HasValue && CurrentData.ScaleLeft.Value == (int)((card.BaseCard.level >> 24) & 0xFF)) return true;
            if (CurrentData.ScaleRight.HasValue && CurrentData.ScaleRight.Value == (int)((card.BaseCard.level >> 16) & 0xFF)) return true;
            if (CurrentData.CardATK.HasValue)
            {
                if (CurrentData.CardATK.Value >= 0 && card.BaseCard.atk == CurrentData.CardATK.Value) return true;
                if (CurrentData.CardATK.Value < 0 && card.BaseCard.atk < 0) return true;
            }
            
            if (CurrentData.GPoint.HasValue && card.GPoint == CurrentData.GPoint.Value) return true;
            if (MatchString(card.BaseCard.name, CurrentData.CardDesc)) return true;
            if (MatchString(card.BaseCard.desc, CurrentData.CardDesc)) return true;

            return false;
        }
        ///////
        class FilterGroup
        {
            public bool IsActive { get; init; }
            public Func<CardEX, bool> Matches { get; init; }
        }
        bool FilterMixed_ANDBetweenGroups(List<FilterGroup> groups, CardEX card)
        {
            // AND between groups: every active group must match
            foreach (var g in groups)
            {
                if (g.IsActive && !g.Matches(card)) return false; // short-circuit
            }
            return true; // all active groups passed (or no active groups => true)
        }
        bool FilterMixed_ORBetweenGroups(List<FilterGroup> groups, CardEX card)
        {
            // OR between groups: any active group matching is enough
            bool anyActive = false;
            foreach (var g in groups)
            {
                if (!g.IsActive) continue;
                anyActive = true;
                if (g.Matches(card)) return true; // short-circuit: accepted by one scenario
            }
            return !anyActive; // if no active groups => true (no filter); otherwise none matched => false
        }
        FilterGroup CardIdGroup()
        {
            bool isActive = CurrentData.CardID.HasValue;
            return new FilterGroup
            {
                IsActive = isActive,
                Matches = card =>
                {
                    return card.ID == CurrentData.CardID.Value || card.BaseCard.alias == CurrentData.CardID.Value;
                }
            };
        }
        FilterGroup CardRuleGroup()
        {
            bool isActive = CurrentData.SelectedRule != 0;
            return new FilterGroup
            {
                IsActive = isActive,
                Matches = card =>
                {
                    if (AlternateFormats)
                    {
                        if ((card.BaseCard.ot & CurrentData.SelectedRule) != 0) return true;
                    }
                    else
                    {
                        if ((card.BaseCard.ot & AlternateRule) != 0) return false;
                        if ((card.BaseCard.ot & CurrentData.SelectedRule) != 0) return true;
                    }
                    return false;
                }
            };
        }
        FilterGroup CardTypeGroup()
        {
            bool isActive = CurrentData.SelectedType != 0;
            return new FilterGroup
            {
                IsActive = isActive,
                Matches = card =>
                {
                    return (card.BaseCard.type & CurrentData.SelectedType) != 0;
                }
            };
        }
        FilterGroup CardAttributeGroup()
        {
            bool isActive = CurrentData.SelectedAttribute != 0;
            return new FilterGroup
            {
                IsActive = isActive,
                Matches = card =>
                {
                    return (card.BaseCard.attribute & CurrentData.SelectedAttribute) != 0;
                }
            };
        }
        FilterGroup CardRaceGroup()
        {
            bool isActive = CurrentData.SelectedRace != 0;
            return new FilterGroup
            {
                IsActive = isActive,
                Matches = card =>
                {
                    return (card.BaseCard.race & CurrentData.SelectedRace) != 0;
                }
            };
        }
        FilterGroup CardSetCodeGroup()
        {
            bool isActive = CurrentData.SelectedSetcode != 0;
            return new FilterGroup
            {
                IsActive = isActive,
                Matches = card =>
                {
                    return MatchesSetcode.MatchesSetCode_OR(card.BaseCard.setcode, CurrentData.SelectedSetcode);
                }
            };
        }
        FilterGroup CardCategoryGroup()
        {
            bool isActive = CurrentData.SelectedCategory != 0;
            return new FilterGroup
            {
                IsActive = isActive,
                Matches = card =>
                {
                    return (card.BaseCard.category & CurrentData.SelectedCategory) != 0;
                }
            };
        }
        FilterGroup CardFlagGroup()
        {
            bool isActive = CurrentData.SelectedFlag != 0;
            return new FilterGroup
            {
                IsActive = isActive,
                Matches = card =>
                {
                    return (card.BaseCard.flag & CurrentData.SelectedFlag) != 0;
                }
            };
        }
        FilterGroup CardRarityGroup()
        {
            bool isActive = CurrentData.SelectedRarity != 0;
            return new FilterGroup
            {
                IsActive = isActive,
                Matches = card =>
                {
                    return (card.Rare & CurrentData.SelectedRarity) != 0;
                }
            };
        }
        FilterGroup CardLevelGroup()
        {
            bool isActive = CurrentData.CardLevel.HasValue || CurrentData.LinkRating.HasValue ||
                CurrentData.ScaleLeft.HasValue || CurrentData.ScaleRight.HasValue;
            return new FilterGroup
            {
                IsActive = isActive,
                Matches = card =>
                {
                    int low8 = (int)(card.BaseCard.level & 0xFF);
                    int high8 = (int)((card.BaseCard.level >> 8) & 0xFF);
                    if (IsLink)
                    {
                        if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value == low8) return true;
                        if(CurrentData.CardLevel.HasValue && CurrentData.CardLevel.Value == high8) return true;
                    }
                    else
                    {
                        if (CurrentData.CardLevel.HasValue && CurrentData.CardLevel.Value == low8) return true;
                        if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value == high8) return true;
                    }
                    if (CurrentData.ScaleLeft.HasValue && CurrentData.ScaleLeft.Value == (int)((card.BaseCard.level >> 24) & 0xFF)) return true;
                    if (CurrentData.ScaleRight.HasValue && CurrentData.ScaleRight.Value == (int)((card.BaseCard.level >> 16) & 0xFF)) return true;
                    return false;
                }
            };
        }
        FilterGroup CardPowerGroup()
        {
            bool isActive = CurrentData.CardATK.HasValue || CurrentData.CardDEF.HasValue || CurrentData.LinkArrows != 0;
            return new FilterGroup
            {
                IsActive = isActive,
                Matches = card =>
                {
                    if (CurrentData.CardATK.HasValue)
                    {
                        if (CurrentData.CardATK.Value >= 0 && card.BaseCard.atk == CurrentData.CardATK.Value) return true;
                        if (CurrentData.CardATK.Value < 0 && card.BaseCard.atk < 0) return true;
                    }

                    if (IsLink)
                    {
                        if (CurrentData.CardDEF.HasValue || CurrentData.LinkArrows != 0)
                        {
                            var (matchDEF, matchLink) = MatchesDEFLink_OR(card.BaseCard.def);
                            if (matchDEF || matchLink) return true;
                        }
                    }
                    else
                    {
                        if (CurrentData.CardDEF.HasValue)
                        {
                            if (CurrentData.CardDEF.Value >= 0 && card.BaseCard.def == CurrentData.CardDEF.Value) return true;
                            if (CurrentData.CardDEF.Value < 0 && card.BaseCard.def < 0) return true;
                        }
                    }
                    if (CurrentData.GPoint.HasValue && card.GPoint == CurrentData.GPoint.Value) return true;

                    return false;
                }
            };
        }
        FilterGroup CardString()
        {
            bool isActive = !string.IsNullOrWhiteSpace(CurrentData.CardDesc);
            return new FilterGroup
            {
                IsActive = isActive,
                Matches = card =>
                {
                    return MatchString(card.BaseCard.name, CurrentData.CardDesc) || MatchString(card.BaseCard.desc, CurrentData.CardDesc);
                }
            };
        }

        private bool FilterMixedAND_OR(CardEX card)
        {
            if (SelectedBanList != null && CurrentData.Limit != 5)
            {
                bool existInList = SelectedBanList.CardList.TryGetValue(card.ID, out CardBanList banInfo);

                if (SelectedBanList.WhiteList) // Danh sách trắng. Không có trong CardList => Không tồn tại.
                {
                    if (!existInList || banInfo == null) return false; // Không tồn tại.
                    if (CurrentData.Limit != 4) // Khác Allowed Card
                    {
                        if (banInfo.LimitedCount != CurrentData.Limit) return false;
                    }
                }
                else // Danh sách đen. Không có trong CardList => UnLimited.
                {
                    if (!existInList || banInfo == null) // Không tồn tại.
                    {
                        if (CurrentData.Limit != 3) return false; // Không phải UnLimited
                    }
                    else
                    {
                        if (banInfo.LimitedCount != CurrentData.Limit) return false; // Có tồn tại nhưng khác giới hạn.
                    }
                }
            }
            // AND giữa các nhóm, OR trong cùng nhóm
            var groups = new List<FilterGroup>
            {
                CardIdGroup(),
                CardRuleGroup(),
                CardTypeGroup(),
                CardAttributeGroup(),
                CardRaceGroup(),
                CardSetCodeGroup(),
                CardCategoryGroup(),
                CardFlagGroup(),
                CardRarityGroup(),
                CardLevelGroup(),
                CardPowerGroup(),
                CardString()
            };
            return FilterMixed_ANDBetweenGroups(groups, card);
        }
        private bool FilterMixedOR_AND(CardEX card)
        {
            if (SelectedBanList != null && CurrentData.Limit != 5)
            {
                bool existInList = SelectedBanList.CardList.TryGetValue(card.ID, out CardBanList banInfo);

                if (SelectedBanList.WhiteList) // Danh sách trắng. Không có trong CardList => Không tồn tại.
                {
                    if (!existInList || banInfo == null) return false; // Không tồn tại.
                    if (CurrentData.Limit != 4) // Khác Allowed Card
                    {
                        if (banInfo.LimitedCount != CurrentData.Limit) return false;
                    }
                }
                else // Danh sách đen. Không có trong CardList => UnLimited.
                {
                    if (!existInList || banInfo == null) // Không tồn tại.
                    {
                        if (CurrentData.Limit != 3) return false; // Không phải UnLimited
                    }
                    else
                    {
                        if (banInfo.LimitedCount != CurrentData.Limit) return false; // Có tồn tại nhưng khác giới hạn.
                    }
                }
            }
            // OR giữa các nhóm, AND trong cùng nhóm

            var FilterIDGroup = new FilterGroup
            {
                IsActive = CurrentData.CardID.HasValue,
                Matches = c =>
                {
                    return c.ID == CurrentData.CardID.Value || c.BaseCard.alias == CurrentData.CardID.Value;
                }
            };
            var FilterRuleGroup = new FilterGroup
            {
                IsActive = CurrentData.SelectedRule != 0,
                Matches = c =>
                {
                    if (AlternateFormats)
                    {
                        if ((c.BaseCard.ot & CurrentData.SelectedRule) != CurrentData.SelectedRule) return false;
                    }
                    else
                    {
                        if ((c.BaseCard.ot & AlternateRule) != 0) return false;
                        if ((c.BaseCard.ot & CurrentData.SelectedRule) != CurrentData.SelectedRule) return false;
                    }
                    return true;
                }
            };
            var FilterTypeGroup = new FilterGroup
            {
                IsActive = CurrentData.SelectedType != 0,
                Matches = c =>
                {
                    return (c.BaseCard.type & CurrentData.SelectedType) == CurrentData.SelectedType;
                }
            };
            var FilterAttributeGroup = new FilterGroup
            {
                IsActive = CurrentData.SelectedAttribute != 0,
                Matches = c =>
                {
                    return (c.BaseCard.attribute & CurrentData.SelectedAttribute) == CurrentData.SelectedAttribute;
                }
            };
            var FilterRaceGroup = new FilterGroup
            {
                IsActive = CurrentData.SelectedRace != 0,
                Matches = c =>
                {
                    return (c.BaseCard.race & CurrentData.SelectedRace) == CurrentData.SelectedRace;
                }
            };
            var FilterSetCodeGroup = new FilterGroup
            {
                IsActive = CurrentData.SelectedSetcode != 0,
                Matches = c =>
                {
                    return MatchesSetcode.MatchesSetCode_AND(c.BaseCard.setcode, CurrentData.SelectedSetcode);
                }
            };
            var FilterCategoryGroup = new FilterGroup
            {
                IsActive = CurrentData.SelectedCategory != 0,
                Matches = c =>
                {
                    return (c.BaseCard.category & CurrentData.SelectedCategory) == CurrentData.SelectedCategory;
                }
            };
            var FilterFlagGroup = new FilterGroup
            {
                IsActive = CurrentData.SelectedFlag != 0,
                Matches = c =>
                {
                    return (c.BaseCard.flag & CurrentData.SelectedFlag) == CurrentData.SelectedFlag;
                }
            };
            var FilterRarityGroup = new FilterGroup
            {
                IsActive = CurrentData.SelectedRarity != 0,
                Matches = c =>
                {
                    return (c.Rare & CurrentData.SelectedRarity) == CurrentData.SelectedRarity;
                }
            };
            var FilterLevelGroup = new FilterGroup
            {
                IsActive = CurrentData.CardLevel.HasValue || CurrentData.LinkRating.HasValue ||
                CurrentData.ScaleLeft.HasValue || CurrentData.ScaleRight.HasValue,
                Matches = c =>
                {
                    int low8 = (int)(c.BaseCard.level & 0xFF);
                    int high8 = (int)((c.BaseCard.level >> 8) & 0xFF);
                    if (IsLink)
                    {
                        if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value != low8) return false;
                        if (CurrentData.CardLevel.HasValue && CurrentData.CardLevel.Value != high8) return false;
                    }
                    else
                    {
                        if (CurrentData.CardLevel.HasValue && CurrentData.CardLevel.Value != low8) return false;
                        if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value != high8) return false;
                    }
                    if (CurrentData.ScaleLeft.HasValue && CurrentData.ScaleLeft.Value != (int)((c.BaseCard.level >> 24) & 0xFF)) return false;
                    if (CurrentData.ScaleRight.HasValue && CurrentData.ScaleRight.Value != (int)((c.BaseCard.level >> 16) & 0xFF)) return false;
                    return true;
                }
            };
            var FilterPowerGroup = new FilterGroup
            {
                IsActive = CurrentData.CardATK.HasValue || CurrentData.CardDEF.HasValue ||
                CurrentData.LinkArrows != 0 || CurrentData.GPoint.HasValue,
                Matches = c =>
                {
                    if (CurrentData.CardATK.HasValue)
                    {
                        if (CurrentData.CardATK.Value >= 0 && c.BaseCard.atk != CurrentData.CardATK.Value) return false;
                        if (CurrentData.CardATK.Value < 0 && c.BaseCard.atk >= 0) return false;
                    }
                    if (IsLink)
                    {
                        if (CurrentData.CardDEF.HasValue || CurrentData.LinkArrows != 0)
                        {
                            var (matchDEF, matchLink) = MatchesDEFLink_AND(c.BaseCard.def);
                            if (!matchDEF || !matchLink) return false;
                        }
                    }
                    else
                    {
                        if (CurrentData.CardDEF.HasValue)
                        {
                            if (CurrentData.CardDEF.Value >= 0 && c.BaseCard.def != CurrentData.CardDEF.Value) return false;
                            if (CurrentData.CardDEF.Value < 0 && c.BaseCard.def >= 0) return false;
                        }
                    }
                    if (CurrentData.GPoint.HasValue && c.GPoint != CurrentData.GPoint.Value) return false;
                    return true;
                }
            };
            var FilterStringGroup = new FilterGroup
            {
                IsActive = !string.IsNullOrWhiteSpace(CurrentData.CardDesc),
                Matches = c =>
                {
                    return MatchString(c.BaseCard.name, CurrentData.CardDesc) || MatchString(c.BaseCard.desc, CurrentData.CardDesc);
                }
            };

            var groups = new List<FilterGroup>
            {
                FilterIDGroup,
                FilterRuleGroup,
                FilterTypeGroup,
                FilterAttributeGroup,
                FilterRaceGroup,
                FilterSetCodeGroup,
                FilterCategoryGroup,
                FilterFlagGroup,
                FilterRarityGroup,
                FilterLevelGroup,
                FilterPowerGroup,
                FilterStringGroup
            };
            return FilterMixed_ORBetweenGroups(groups, card);
        }

        public (bool MatchDEF, bool MatchLink) MatchesDEFLink_AND(long baseDEF)
        {
            bool matchesDEF = true;
            bool matchesLinkArrows = true;

            if (CurrentData.CardDEF.HasValue)
            {
                if ((baseDEF & maskHasDEF) != 0) // hasDEF
                {
                    var decoded = GetInfoService.DecodeDef(baseDEF);
                    long defFromCard = decoded.deffromtext.Value;
                    if (CurrentData.CardDEF.Value >= 0) matchesDEF = defFromCard == CurrentData.CardDEF.Value;
                    else matchesDEF = defFromCard < 0;
                }
            }

            if (CurrentData.LinkArrows != 0)
            {
                long linkArrowText = baseDEF & 0x1FFL;

                if (linkArrowText == CurrentData.LinkArrows) matchesLinkArrows = true;
                else
                {
                    long merged = linkArrowText | maskLinkArrow;
                    matchesLinkArrows = merged == CurrentData.LinkArrows;
                }
            }
            return (matchesDEF, matchesLinkArrows);
        }
        public (bool MatchDEF, bool MatchLink) MatchesDEFLink_OR(long baseDEF)
        {
            bool matchesDEF = true;
            bool matchesLinkArrows = true;

            if (CurrentData.CardDEF.HasValue)
            {
                if ((baseDEF & maskHasDEF) != 0) // hasDEF
                {
                    var decoded = GetInfoService.DecodeDef(baseDEF);
                    long defFromCard = decoded.deffromtext.Value;
                    if (CurrentData.CardDEF.Value >= 0) matchesDEF = defFromCard == CurrentData.CardDEF.Value;
                    else matchesDEF = defFromCard < 0;
                }
            }

            if (CurrentData.LinkArrows != 0)
            {
                long linkArrowText = baseDEF & 0x1FFL;

                matchesLinkArrows = (linkArrowText & CurrentData.LinkArrows) == CurrentData.LinkArrows;
                if (!matchesLinkArrows)
                {
                    linkArrowText |= maskLinkArrow;
                    matchesLinkArrows = (linkArrowText & CurrentData.LinkArrows) == CurrentData.LinkArrows;
                }
            }
            return (matchesDEF, matchesLinkArrows);
        }
        #endregion

        #region Card Sort
        private Func<CardInstance, IComparable> GetSortKeySelector(string propertyName)
        {
            return propertyName switch
            {
                "Rare" => x => x.Card?.Rare ?? long.MinValue,
                "GPoint" => x => x.Card?.GPoint ?? int.MinValue,

                "id" => x => x.Card?.BaseCard?.id ?? ulong.MinValue,
                "name" => x => x.Card?.BaseCard?.name ?? string.Empty,
                "ot" => x => x.Card?.BaseCard?.ot ?? ulong.MinValue,
                "alias" => x => x.Card?.BaseCard?.alias ?? ulong.MinValue,
                "setcode" => x => x.Card?.BaseCard?.setcode ?? ulong.MinValue,
                "type" => x => x.Card?.BaseCard?.type ?? ulong.MinValue,
                "atk" => x => x.Card?.BaseCard?.atk ?? long.MinValue,
                "def" => x => x.Card?.BaseCard?.def ?? long.MinValue,
                "level" => x => x.Card?.BaseCard?.level ?? ulong.MinValue,
                "race" => x => x.Card?.BaseCard?.race ?? ulong.MinValue,
                "attribute" => x => x.Card?.BaseCard?.attribute ?? ulong.MinValue,
                "category" => x => x.Card?.BaseCard?.category ?? ulong.MinValue,

                _ => x => x.UniqueID
            };
        }

        public void SortMain()
        {
            var selectedSorts = SortsViewModel.Instance.SelectedSortItems;
            if (selectedSorts == null || selectedSorts.Count == 0) return;

            IEnumerable<CardInstance> sortedItems = MainDeck;
            bool isFirstSort = true;

            foreach (var sort in selectedSorts)
            {
                if (sort?.SelectedItem?.Name == null) continue;

                var keySelector = GetSortKeySelector(sort.SelectedItem.Name);
                if (isFirstSort)
                {
                    sortedItems = sort.OrderByAsc
                        ? sortedItems.OrderBy(keySelector)
                        : sortedItems.OrderByDescending(keySelector);
                    isFirstSort = false;
                }
                else
                {
                    sortedItems = sort.OrderByAsc
                        ? ((IOrderedEnumerable<CardInstance>)sortedItems).ThenBy(keySelector)
                        : ((IOrderedEnumerable<CardInstance>)sortedItems).ThenByDescending(keySelector);
                }
            }
            sortedItems = ((IOrderedEnumerable<CardInstance>)sortedItems).ThenBy(x => x.UniqueID);

            MainDeck.ReplaceAll(sortedItems.ToList());
        }
        public void SortExtra()
        {
            var selectedSorts = SortsViewModel.Instance.SelectedSortItems;
            if (selectedSorts == null || selectedSorts.Count == 0) return;

            IEnumerable<CardInstance> sortedItems = ExtraDeck;
            bool isFirstSort = true;

            foreach (var sort in selectedSorts)
            {
                if (sort?.SelectedItem?.Name == null) continue;

                var keySelector = GetSortKeySelector(sort.SelectedItem.Name);
                if (isFirstSort)
                {
                    sortedItems = sort.OrderByAsc
                        ? sortedItems.OrderBy(keySelector)
                        : sortedItems.OrderByDescending(keySelector);
                    isFirstSort = false;
                }
                else
                {
                    sortedItems = sort.OrderByAsc
                        ? ((IOrderedEnumerable<CardInstance>)sortedItems).ThenBy(keySelector)
                        : ((IOrderedEnumerable<CardInstance>)sortedItems).ThenByDescending(keySelector);
                }
            }
            sortedItems = ((IOrderedEnumerable<CardInstance>)sortedItems).ThenBy(x => x.UniqueID);

            ExtraDeck.ReplaceAll(sortedItems.ToList());
        }
        public void SortSide()
        {
            var selectedSorts = SortsViewModel.Instance.SelectedSortItems;
            if (selectedSorts == null || selectedSorts.Count == 0) return;

            IEnumerable<CardInstance> sortedItems = SideDeck;
            bool isFirstSort = true;

            foreach (var sort in selectedSorts)
            {
                if (sort?.SelectedItem?.Name == null) continue;

                var keySelector = GetSortKeySelector(sort.SelectedItem.Name);
                if (isFirstSort)
                {
                    sortedItems = sort.OrderByAsc
                        ? sortedItems.OrderBy(keySelector)
                        : sortedItems.OrderByDescending(keySelector);
                    isFirstSort = false;
                }
                else
                {
                    sortedItems = sort.OrderByAsc
                        ? ((IOrderedEnumerable<CardInstance>)sortedItems).ThenBy(keySelector)
                        : ((IOrderedEnumerable<CardInstance>)sortedItems).ThenByDescending(keySelector);
                }
            }
            sortedItems = ((IOrderedEnumerable<CardInstance>)sortedItems).ThenBy(x => x.UniqueID);

            SideDeck.ReplaceAll(sortedItems.ToList());
        }
        #endregion

        #region Drag-Drop Behaviors

        #region Event Handlers
        private void DeckListView_DragOver(object sender, DragEventArgs e)
        {
            UpdateGhostCardPosition(e);

            if (e.Data.GetDataPresent(typeof(CardEX)))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else if (e.Data.GetDataPresent(typeof(CardInstance)))
            {
                if (e.AllowedEffects.HasFlag(DragDropEffects.Move))
                    e.Effects = DragDropEffects.Move;
                else e.Effects = DragDropEffects.Copy;
            }
            else e.Effects = DragDropEffects.None;

            e.Handled = true;
        }
        private void DeckListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);

            if (sender is System.Windows.Controls.ListView listView)
            {
                var item = ItemsControl.ContainerFromElement(listView, e.OriginalSource as DependencyObject) as System.Windows.Controls.ListViewItem;
                if (item != null && item.DataContext is CardInstance instance)
                {
                    _draggedCardInstance = instance;
                }
            }
        }
        private void DeckListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.ListViewItem item && item.DataContext is CardInstance card)
            {
                _dragStartPoint = e.GetPosition(null);
                _draggedCardInstance = card;
            }
        }
        private void DeckListViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedCardInstance != null)
            {
                Point currentPos = e.GetPosition(null);
                Vector diff = _dragStartPoint - currentPos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (_draggedCardInstance?.Card == null)
                    {
                        _draggedCardInstance = null;
                        return;
                    }
                    if (GhostCardImage == null || GhostCardTransform == null)
                    {
                        _draggedCardInstance = null;
                        return;
                    }
                    
                    Point mousePos = Mouse.GetPosition(RootGrid);
                    GhostCardTransform.X = mousePos.X - (GhostCardImage.Width / 2);
                    GhostCardTransform.Y = mousePos.Y - (GhostCardImage.Height / 2);

                    if (_draggedCardInstance.Card.CardImage != null)
                    {
                        GhostCardImage.Source = _draggedCardInstance.Card.CardImage;
                    }
                    GhostCardImage.Visibility = Visibility.Visible;
                    _draggedCardInstance.IsBeingDragged = true;

                    var draggedInstance = _draggedCardInstance;

                    DataObject dragData = new DataObject(typeof(CardInstance), draggedInstance);
                    DragDropEffects result = DragDrop.DoDragDrop(sender as DependencyObject, dragData, DragDropEffects.Move);

                    if (draggedInstance != null)
                    {
                        draggedInstance.IsBeingDragged = false;
                    }

                    GhostCardImage.Visibility = Visibility.Collapsed;
                    GhostCardImage.Source = null;

                    // Xử lý khi thả ngoài vùng grDeckZone
                    if (result == DragDropEffects.None && draggedInstance != null)
                    {
                        Point mousePosition = Mouse.GetPosition(grDeckZone);
                        Rect deckZoneBounds = new Rect(0, 0, grDeckZone.ActualWidth, grDeckZone.ActualHeight);

                        if (!deckZoneBounds.Contains(mousePosition))
                        {
                            // Xóa card khỏi deck nguồn
                            if (MainDeck.Contains(_draggedCardInstance))
                                MainDeck.Remove(_draggedCardInstance);
                            else if (ExtraDeck.Contains(_draggedCardInstance))
                                ExtraDeck.Remove(_draggedCardInstance);
                            else if (SideDeck.Contains(_draggedCardInstance))
                                SideDeck.Remove(_draggedCardInstance);
                        }
                    }

                    _draggedCardInstance = null;
                }
            }
        }
        //private void HandleDrop(DragEventArgs e, System.Windows.Controls.ListView targetListView, BulkObservableCollection<CardInstance> targetDeck)
        private void HandleDrop(DragEventArgs e, System.Windows.Controls.ListView targetListView, CardDeck targetDeck)
        {
            if (targetDeck == null) return;

            Point dropPosition = e.GetPosition(targetListView);
            int insertIndex = GetItemIndexAtPosition(targetListView, dropPosition);

            CardInstance instance = null;
            ObservableCollection<CardInstance> sourceDeck = null;
            bool isReorder = false;

            // Lấy card từ DragData
            if (e.Data.GetDataPresent(typeof(CardInstance)))
            {
                var originalInstance = e.Data.GetData(typeof(CardInstance)) as CardInstance;
                if (originalInstance == null) return;
                if (!CheckValidCard(originalInstance.Card.BaseCard, targetDeck)) return;

                // Xác định ListView nguồn
                if (MainDeck.Contains(originalInstance))
                    sourceDeck = MainDeck;
                else if (ExtraDeck.Contains(originalInstance))
                    sourceDeck = ExtraDeck;
                else if (SideDeck.Contains(originalInstance))
                    sourceDeck = SideDeck;

                // Kiểm tra xem có phải reorder trong cùng ListView không
                if (sourceDeck == targetDeck)
                {
                    instance = originalInstance;
                    isReorder = true;
                }
                else
                {
                    // Di chuyển giữa các ListView khác nhau
                    instance = originalInstance; // Dùng chính instance đó
                }
            }
            else if (e.Data.GetDataPresent(typeof(CardEX)))
            {
                var card = e.Data.GetData(typeof(CardEX)) as CardEX;
                if (card == null) return;
                // if (!CheckValidCard(card.BaseCard, targetDeck)) return;
                if (!CheckValidCard(card.BaseCard, targetDeck)) return;

                instance = new CardInstance
                {
                    Card = card
                };
            }
            else return;

            if (insertIndex < 0 || insertIndex > targetDeck.Count)
            {
                insertIndex = targetDeck.Count;
            }

            if (isReorder)
            {
                // Reorder trong cùng ListView
                int oldIndex = targetDeck.IndexOf(instance);
                if (oldIndex == insertIndex || oldIndex == insertIndex - 1)
                {
                    return;
                }

                targetDeck.RemoveAt(oldIndex);

                if (insertIndex > oldIndex)
                {
                    insertIndex--;
                }
            }
            else if (sourceDeck != null)
            {
                // Di chuyển từ ListView khác
                sourceDeck.Remove(instance);
            }

            targetDeck.Insert(insertIndex, instance);
            e.Handled = true;
        }
        #endregion

        #region Ghost Image
        private void Card_DragOver(object sender, DragEventArgs e)
        {
            UpdateGhostCardPosition(e);
        }
        private void RootGrid_DragOver(object sender, DragEventArgs e)
        {
            UpdateGhostCardPosition(e);
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
        private void UpdateGhostCardPosition(DragEventArgs e)
        {
            var mousePos = e.GetPosition(RootGrid);
            GhostCardTransform.X = mousePos.X - (GhostCardImage.Width / 2);
            GhostCardTransform.Y = mousePos.Y - (GhostCardImage.Height / 2);
        }
        #endregion

        #region Helpers
        private int GetCardLimitCount(ulong cardID)
        {
            if (SelectedBanList == null) return 3; // Mặc định không giới hạn
            else
            {
                if (SelectedBanList.CardList.TryGetValue(cardID, out CardBanList banInfo))
                {
                    return banInfo.LimitedCount;
                }
                else
                {
                    return SelectedBanList.WhiteList ? 0 : 3; // Danh sách trắng: không tồn tại => 0; Danh sách đen: không tồn tại => 3
                }
            }
        }
        private ulong GetRealCardID(CardEditor.Models.Card card)
        {
            return card.alias != 0 ? card.alias : card.id;
        }
        private int CountRealCopies(CardDeck targetDeck, ulong realId)
        {
            return targetDeck.Count(c =>
            {
                var cRealId = c.Card.BaseCard.alias != 0
                                ? c.Card.BaseCard.alias
                                : c.Card.BaseCard.id;

                return cRealId == realId;
            });
        }
        private bool CheckValidDeckSize(CardEditor.Models.Card card, CardDeck targetDeck) // Deck Limits + Each Card Limits
        {
            if (IgnoreSize) return true;

            if (targetDeck.Type == DeckType.Main)
            {
                if (ConfigViewModel.Instance.deckEditSetting.MainDeckLimit.HasValue &&
                    targetDeck.Count >= ConfigViewModel.Instance.deckEditSetting.MainDeckLimit.Value)
                    return false;
            }
            else if (targetDeck.Type == DeckType.Extra)
            {
                if (ConfigViewModel.Instance.deckEditSetting.ExtraDeckLimit.HasValue &&
                    targetDeck.Count >= ConfigViewModel.Instance.deckEditSetting.ExtraDeckLimit.Value)
                    return false;
            }
            else if (targetDeck.Type == DeckType.Side)
            {
                if (ConfigViewModel.Instance.deckEditSetting.SideDeckLimit.HasValue &&
                    targetDeck.Count >= ConfigViewModel.Instance.deckEditSetting.SideDeckLimit.Value)
                    return false;
            }
            else return false;

            ulong realId = GetRealCardID(card);
            int count = CountRealCopies(targetDeck, realId);
            if (count >= GetCardLimitCount(realId))
                return false;
            return true;
        }

        private bool CheckValidDeckContent(CardEditor.Models.Card card, CardDeck targetDeck) // Each Card Type Limits
        {
            if (IgnoreContent) return true;

            bool isMain = targetDeck.Type == DeckType.Main;
            bool isExtra = targetDeck.Type == DeckType.Extra;
            bool isSide = targetDeck.Type == DeckType.Side;

            if (FindIInfoService.CheckCardInfo(card.type, CardType.Spell, CardType.Trap, CardType.Skill))
                return isMain || isSide;
            else if (FindIInfoService.CheckCardInfo(card.type, CardType.Monster))
            {
                if (FindIInfoService.CheckCardInfo(card.type, CardType.Fusion, CardType.Synchro, CardType.eXceed, CardType.Link))
                    return isExtra || isSide;
                if (FindIInfoService.CheckCardInfo(card.type, CardType.Ritual))
                    return RitualPlaceExtra ? (isExtra || isSide) : (isMain || isSide);
                return isMain || isSide;
            }
            else return isMain || isSide;
        }
        private bool CheckValidCard(CardEditor.Models.Card card, CardDeck targetDeck)
        {
            return CheckValidDeckSize(card, targetDeck) && CheckValidDeckContent(card, targetDeck);
        }

        private int GetItemIndexAtPosition(System.Windows.Controls.ListView listView, Point position)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {
                var itemContainer = listView.ItemContainerGenerator.ContainerFromIndex(i) as System.Windows.Controls.ListViewItem;
                if (itemContainer != null)
                {
                    Rect bounds = VisualTreeHelper.GetDescendantBounds(itemContainer);
                    Point topLeft = itemContainer.TranslatePoint(new Point(0, 0), listView);
                    Rect itemRect = new Rect(topLeft, bounds.Size);

                    if (itemRect.Contains(position))
                        return i;
                }
            }

            return -1; // Không có item tại vị trí chuột
        }
        private System.Windows.Controls.ListView GetTargetListView(Point screenPosition)
        {
            // Kiểm tra xem chuột có trong MainDeckListView không
            if (IsPointInsideElement(Mouse.GetPosition(MainDeckListView), MainDeckListView))
                return MainDeckListView;

            // Kiểm tra ExtraDeckListView
            if (IsPointInsideElement(Mouse.GetPosition(ExtraDeckListView), ExtraDeckListView))
                return ExtraDeckListView;

            // Kiểm tra SideDeckListView
            if (IsPointInsideElement(Mouse.GetPosition(SideDeckListView), SideDeckListView))
                return SideDeckListView;

            return null;
        }
        private bool IsPointInsideElement(Point point, FrameworkElement element)
        {
            if (element == null) return false;

            Point relativePoint = element.PointFromScreen(
                element.PointToScreen(new Point(0, 0))
            );

            Rect bounds = new Rect(0, 0, element.ActualWidth, element.ActualHeight);
            Point testPoint = Mouse.GetPosition(element);

            return bounds.Contains(testPoint);
        }
        #endregion

        #region Card List
        private void CardListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.ListViewItem item && item.DataContext is CardEX card)
            {
                _dragStartPoint = e.GetPosition(null);
                _draggedCardEX = card;
            }
        }
        private void CardListViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedCardEX != null)
            {
                Point currentPos = e.GetPosition(null);
                Vector diff = _dragStartPoint - currentPos;

                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    Point mousePos = Mouse.GetPosition(RootGrid);
                    // Trừ đi nửa kích thước để con trỏ chuột ở chính giữa
                    GhostCardTransform.X = mousePos.X - (GhostCardImage.Width / 2);
                    GhostCardTransform.Y = mousePos.Y - (GhostCardImage.Height / 2);

                    // Gán ảnh
                    GhostCardImage.Source = _draggedCardEX.CardImage;
                    GhostCardImage.Visibility = Visibility.Visible;
                    //GhostCardImage.UpdateLayout();

                    DataObject dragData = new DataObject(typeof(CardEX), _draggedCardEX);
                    DragDrop.DoDragDrop(sender as DependencyObject, dragData, DragDropEffects.Copy);

                    GhostCardImage.Visibility = Visibility.Collapsed;
                    GhostCardImage.Source = null;
                    _draggedCardEX = null;
                }
            }
        }
        #endregion

        #region Main Deck
        private void MainDeckListView_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e, MainDeckListView, MainDeck);

            //if (MainDeck == null) return;

            //Point dropPosition = e.GetPosition(MainDeckListView);

            ////Rect listViewBounds = new Rect(0, 0, MainDeckListView.ActualWidth, MainDeckListView.ActualHeight);

            ////if (!listViewBounds.Contains(dropPosition) && _draggedCardInstance != null)
            ////{
            ////    RemoveCardInstance(_draggedCardInstance);
            ////    _draggedCardInstance = null;
            ////    e.Handled = true;
            ////    return;
            ////}

            //int insertIndex = GetItemIndexAtPosition(MainDeckListView, dropPosition);
            //CardInstance instance = null;
            //bool isReorder = false;

            //if (e.Data.GetDataPresent(typeof(CardInstance)))
            //{
            //    var originalInstance = e.Data.GetData(typeof(CardInstance)) as CardInstance;
            //    if (originalInstance == null)  return;

            //    if (!CheckValidCard(originalInstance.Card.ID, originalInstance.Card.BaseCard.type)) return;

            //    if (MainDeck.Contains(originalInstance))
            //    {
            //        instance = originalInstance;
            //        isReorder = true;
            //    }
            //    else
            //    {
            //        instance = new CardInstance
            //        {
            //            Card = originalInstance.Card
            //        };
            //    }
            //}
            //else if (e.Data.GetDataPresent(typeof(CardEX)))
            //{
            //    var card = e.Data.GetData(typeof(CardEX)) as CardEX;
            //    if (card == null) return;

            //    if (!CheckValidCard(card.ID, card.BaseCard.type)) return;

            //    instance = new CardInstance
            //    {
            //        Card = card
            //    };
            //}
            //else return;

            //if (insertIndex < 0 || insertIndex > MainDeck.Count)
            //{
            //    insertIndex = MainDeck.Count;
            //}
            //if (isReorder)
            //{
            //    int oldIndex = MainDeck.IndexOf(instance);

            //    if (oldIndex == insertIndex || oldIndex == insertIndex - 1)
            //    {
            //        // Không thay đổi vị trí → bỏ qua
            //        return;
            //    }

            //    // Gỡ item cũ
            //    MainDeck.RemoveAt(oldIndex);

            //    // Tính lại insertIndex nếu cần
            //    if (insertIndex > oldIndex)
            //    {
            //        insertIndex--; // Vì đã Remove trước đó
            //    }
            //}

            //MainDeck.Insert(insertIndex, instance);

            //e.Handled = true;
        }
        private void MainDeckListView_DragLeave(object sender, DragEventArgs e)
        {

        }
        private void MainDeckListViewItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.ListViewItem item && item.DataContext is CardInstance card)
            {
                MainDeck.Remove(card);
                e.Handled = true;
            }
        }
        #endregion

        #region Extra Deck
        private void ExtraDeckListView_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e, ExtraDeckListView, ExtraDeck);
        }
        private void ExtraDeckListView_DragLeave(object sender, DragEventArgs e)
        {

        }
        private void ExtraDeckListViewItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.ListViewItem item && item.DataContext is CardInstance card)
            {
                ExtraDeck.Remove(card);
                e.Handled = true;
            }
        }
        #endregion

        #region Side Deck
        private void SideDeckListView_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e, SideDeckListView, SideDeck);
        }
        private void SideDeckListView_DragLeave(object sender, DragEventArgs e)
        {

        }
        private void SideDeckListViewItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.ListViewItem item && item.DataContext is CardInstance card)
            {
                SideDeck.Remove(card);
                e.Handled = true;
            }
        }
        #endregion

        #endregion

        #region PopUp
        private double _vOffset;
        private double _hOffset;
        private Point _startPoint;

        public double VOffset
        {
            get => _vOffset;
            set
            {
                _vOffset = value;
                OnPropertyChanged(nameof(VOffset));
            }
        }
        public double HOffset
        {
            get => _hOffset;
            set
            {
                _hOffset = value;
                OnPropertyChanged(nameof(HOffset));
            }
        }
        private void PopupBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _startPoint = e.GetPosition(this);
            ((UIElement)sender).CaptureMouse();
        }
        private void PopupBorder_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPoint = e.GetPosition(this);

                HOffset += currentPoint.X - _startPoint.X;
                VOffset += currentPoint.Y - _startPoint.Y;

                _startPoint = currentPoint;
            }
        }
        private void PopupBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ((UIElement)sender).ReleaseMouseCapture();
        }
        #endregion

        #region Buttons
        private void btnYDKEDeck_Click(object sender, RoutedEventArgs e)
        {
            popYDKE.IsOpen = true;
        }
        private void btnOpenPopUpReName_Click(object sender, RoutedEventArgs e)
        {
            OpenReNameDeck();
        }
        private void btnBrowseNewPath_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = FileDiaLogHelper.OpenFolder();
            if (!string.IsNullOrEmpty(folderPath))
            {
                NewFolderPath = folderPath;
            }
            OpenReNameDeck();
        }
        private void btnOpenLinkArrows_Click(object sender, RoutedEventArgs e)
        {
            popLinkArrows.IsOpen = true;
        }
        private void btnApplyArrows_Click(object sender, RoutedEventArgs e)
        {
            long newMask = 0;

            ToggleButton[] toggles = { tbtnBottomLeft, tbtnBottom, tbtnBottomRight,
                tbtnLeft, tbtnRight, tbtnTopLeft, tbtnTop, tbtnTopRight };

            foreach (var togg in toggles)
            {
                if (togg.IsChecked == true && int.TryParse(togg.Tag.ToString(), out int bit))
                    newMask |= 1L << bit;
            }

            CurrentData.LinkArrows = newMask;

            popLinkArrows.IsOpen = false;
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            cmbcardtype?.SelectedItems.Clear();
            cmbcardattribute?.SelectedItems.Clear();
            cmbcardrace?.SelectedItems.Clear();
            cmbrule?.SelectedItems.Clear();
            cmbsetcode?.SelectedItems.Clear();
            cmbflag?.SelectedItems.Clear();
            cmbrarity?.SelectedItems.Clear();
            cmbcategory?.SelectedItems.Clear();

            ViewData.Clear();

            ToggleButton[] toggles = { tbtnBottomLeft, tbtnBottom, tbtnBottomRight,
                tbtnLeft, tbtnRight, tbtnTopLeft, tbtnTop, tbtnTopRight };

            foreach (var togg in toggles)
            {
                togg.IsChecked = false;
            }

        }
        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            await RefreshFilter();
        }
        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            MarkFilterDirty();
            await RefreshFilter();
        }
        private async void btnReload_Click(object sender, RoutedEventArgs e)
        {
            var (result, message) = await BanListRawDataViewModel.Instance.LoadBanLists();
            await CardEXDataViewModel.Instance.ReloadCardsAsync();
            MarkFilterDirty();
            _isInitialized = true;
            await RefreshFilter();
            if (!result)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
            }
        }

        private void btnCloseYDKEURL_Click(object sender, RoutedEventArgs e)
        {
            popYDKE.IsOpen = false;
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
            CardListView.ItemsSource = null;
            _filteredCards.CollectionChanged -= FilteredCards_CollectionChanged;
            FilteredCards.Clear();
            FilteredCards = null;

            CurrentDeck = null;

            ResizeTimer.Tick -= ResizeTimer_Tick;

            MainDeck.CollectionChanged -= MainDeck_CollectionChanged;
            ExtraDeck.CollectionChanged -= ExtraDeck_CollectionChanged;
            SideDeck.CollectionChanged -= SideDeck_CollectionChanged;

            scrollTimer.Tick -= ScrollTimer_Tick;
            _viewData.PropertyChanged -= OnViewDataPropertyChanged;

            MainDeck.Clear();
            MainDeck = null;
            ExtraDeck.Clear();
            ExtraDeck = null;
            SideDeck.Clear();
            SideDeck = null;

            _hoveredCardEX = null;
            _selectedCardEX = null;
            _hoveredCardInstance = null;
            _selectedCardInstance = null;
        }
        #endregion
    }
}
