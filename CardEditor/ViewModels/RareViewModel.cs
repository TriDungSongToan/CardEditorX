using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CardEditor.Models;
using CardEditor.Models.Settings;
using CardEditor.Services;
using CardEditor.Helpers;
using CardEditor.ImageGene;
using CardEditor.Localization;
using CardEditor.Collections;
using CMess = CardEditor.Localization.Language;
using RelayCommand = CardEditor.Commands.RelayCommand;
using CardAppContext = CardEditor.Models.AppContext;

namespace CardEditor.ViewModels
{
    public class RareViewModel : INotifyPropertyChanged, IDisposable
    {
        public static RareViewModel CreateInstance() => new RareViewModel();

        #region Fields
        private bool IsSynSelectedRareItems = false;
        private bool IsSynSelectedRareCards = false;
        private bool IsSynSelectedComboRareItems = false;
        // private bool IsSynSelectedRareItem = false;
        // private bool IsSynSelectedRareCard = false;
        private bool IsSynRareCardRare = false;

        private bool IsUserChangeCardID = false;
        private bool IsUserChangeImagePath = false;
        private bool IsUserChangeFolderpath = false;

        private CancellationTokenSource _findImgCardCTS;
        private CancellationTokenSource _createImgCardCTS;
        private CancellationTokenSource _loadRareCardCTS;
        #endregion

        #region ObservableCollection
        // Bộ sưu tập các RareItem
        private BulkObservableCollection<RareItem> _rareItems;
        public BulkObservableCollection<RareItem> RareItems
        {
            get => _rareItems;
            set
            {
                if (_rareItems != value)
                {
                    _rareItems = value;
                    OnPropertyChanged();
                }
            }
        }

        // Bộ sưu tập các RareCard
        private BulkObservableCollection<RareCard> _rareCards;
        public BulkObservableCollection<RareCard> RareCards
        {
            get => _rareCards;
            set
            {
                if (_rareCards != value)
                {
                    _rareCards = value;
                    OnPropertyChanged();
                    DeleteAllCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        // Các RareItem được chọn trong DataGrid RareList
        private BulkObservableCollection<RareItem> _selectedRareItems;
        public BulkObservableCollection<RareItem> SelectedRareItems
        {
            get => _selectedRareItems;
            set
            {
                if (_selectedRareItems != value)
                {
                    if (_selectedRareItems != null)
                    {
                        _selectedRareItems.CollectionChanged -= SelectedRareItems_CollectionChanged;
                    }
                    _selectedRareItems = value;
                    if (_selectedRareItems != null)
                    {
                        _selectedRareItems.CollectionChanged += SelectedRareItems_CollectionChanged;
                    }
                    OnPropertyChanged();
                    OnSelectedRareItemsChanged();
                }
            }
        }

        // Các RareCard được chọn trong DataGrid RareCard
        private BulkObservableCollection<RareCard> _selectedRareCards;
        public BulkObservableCollection<RareCard> SelectedRareCards
        {
            get => _selectedRareCards;
            set
            {
                if (_selectedRareCards != value)
                {
                    if (_selectedRareCards != null)
                    {
                        _selectedRareCards.CollectionChanged -= SelectedRareCards_CollectionChanged;
                    }
                    _selectedRareCards = value;
                    if (_selectedRareCards != null)
                    {
                        _selectedRareCards.CollectionChanged += SelectedRareCards_CollectionChanged;
                    }
                    OnPropertyChanged();
                    OnSelectedRareCardsChanged();
                    DeleteSelectedCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        // Các RareItem được chọn trong ComboBox cmbrare
        private BulkObservableCollection<RareItem> _selectedComboRareItems;
        public BulkObservableCollection<RareItem> SelectedComboRareItems
        {
            get => _selectedComboRareItems;
            set
            {
                if (_selectedComboRareItems != value)
                {
                    if (_selectedComboRareItems != null)
                    {
                        _selectedComboRareItems.CollectionChanged -= SelectedComboRareItems_CollectionChanged;
                    }
                    _selectedComboRareItems = value;
                    if (_selectedComboRareItems != null)
                    {
                        _selectedComboRareItems.CollectionChanged += SelectedComboRareItems_CollectionChanged;
                    }
                    OnPropertyChanged();
                    OnSelectedComboRareItemsChanged();
                }
            }
        }

        public int SelectedRareItemsCount => SelectedRareItems?.Count ?? 0;
        public int SelectedRareCardsCount => SelectedRareCards?.Count ?? 0;
        public int SelectedComboRareItemsCount => SelectedComboRareItems?.Count ?? 0;

        public ObservableCollection<StampPositionItem> ListStampPosition { get; set; }
        public ObservableCollection<SortItem> ListArrange { get; set; }
        #endregion

        #region CollectionView
        private ListCollectionView _rareItemsView;
        public ICollectionView RareItemsView => _rareItemsView;

        private ListCollectionView _rareCardsView;
        public ICollectionView RareCardsView => _rareCardsView;
        #endregion

        #region Property
        // ok
        #region SelectedItem
        private RareItem _selectedRareItem;
        public RareItem SelectedRareItem
        {
            get => _selectedRareItem;
            set
            {
                if (_selectedRareItem != value)
                {
                    _selectedRareItem = value;
                    OnPropertyChanged();
                    OnSelectedRareItemChanged();
                    ResetRareCommand?.RaiseCanExecuteChanged();
                    ClearRareCommand?.RaiseCanExecuteChanged();
                    DeleteRareCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private RareCard _selectedRareCard;
        public RareCard SelectedRareCard
        {
            get => _selectedRareCard;
            set
            {
                if (_selectedRareCard != value)
                {
                    _selectedRareCard = value;
                    OnPropertyChanged();
                    OnSelectedRareCardChanged();
                    ResetCardCommand?.RaiseCanExecuteChanged();
                    ClearCardCommand?.RaiseCanExecuteChanged();
                    DeleteSingleCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion

        #region Rare List
        private string _rareManaHeader;
        public string RareManaHeader
        {
            get => _rareManaHeader;
            set
            {
                if (_rareManaHeader != value)
                {
                    _rareManaHeader = value;
                    OnPropertyChanged();
                }
            }
        }

        private int? _idRare;
        public int? IdRare
        {
            get => _idRare;
            set
            {
                if (_idRare != value)
                {
                    _idRare = value;
                    OnPropertyChanged();
                    AddRareCommand?.RaiseCanExecuteChanged();
                    ModifyRareCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _rareName;
        public string RareName
        {
            get => _rareName;
            set
            {
                if (_rareName != value)
                {
                    _rareName = value;
                    OnPropertyChanged();
                    AddRareCommand?.RaiseCanExecuteChanged();
                    ModifyRareCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private int? _rareCode;
        public int? RareCode
        {
            get => _rareCode;
            set
            {
                if (_rareCode != value)
                {
                    _rareCode = value;
                    OnPropertyChanged();
                    AddRareCommand?.RaiseCanExecuteChanged();
                    ModifyRareCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private string _rareImagePath;
        public string RareImagePath
        {
            get => _rareImagePath;
            set
            {
                if (_rareImagePath != value)
                {
                    _rareImagePath = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Rare Cards
        private ImageSource _imagecardSource;
        public ImageSource ImageCardSource
        {
            get => _imagecardSource;
            set
            {
                if(_imagecardSource != value)
                    _imagecardSource = value;
                OnPropertyChanged();
            }
        }

        private string _imagecardTooltip;
        public string ImagecardTooltip
        {
            get => _imagecardTooltip;
            set
            {
                if (_imagecardTooltip != value)
                    _imagecardTooltip = value;
                OnPropertyChanged();
            }
        }

        private string _rareCardName;
        public string RareCardName
        {
            get => _rareCardName;
            set
            {
                if (_rareCardName != value)
                {
                    _rareCardName = value;
                    OnPropertyChanged();
                    AddCardCommand?.RaiseCanExecuteChanged();
                    ModifyCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private ulong? _rareCardId;
        public ulong? RareCardId
        {
            get => _rareCardId;
            set
            {
                if (_rareCardId != value)
                {
                    _rareCardId = value;
                    OnPropertyChanged();
                    AddCardCommand?.RaiseCanExecuteChanged();
                    ModifyCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private long _rareCardRare;
        public long RareCardRare
        {
            get => _rareCardRare;
            set
            {
                if (_rareCardRare != value)
                {
                    _rareCardRare = value;
                    OnPropertyChanged();

                    if (!IsSynSelectedComboRareItems)
                    {
                        try
                        {
                            IsSynRareCardRare = true;
                            UpdateSelectedComboRareItemsFromRareCardRare();
                        }
                        catch { }
                        finally
                        {
                            IsSynRareCardRare = false;
                        }
                    }
                }
            }
        }

        private Visibility _rareLabel;
        public Visibility RareLabel
        {
            get => _rareLabel;
            set
            {
                if (_rareLabel != value)
                {
                    _rareLabel = value;
                    OnPropertyChanged();
                }
            }
        }
        private Visibility _cancelLoadRareCard;
        public Visibility CancelLoadRareCard
        {
            get => _cancelLoadRareCard;
            set
            {
                if (_cancelLoadRareCard != value)
                {
                    _cancelLoadRareCard = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _isCardListLoading;
        public bool IsCardListLoading
        {
            get => _isCardListLoading;
            set { _isCardListLoading = value; OnPropertyChanged(); }
        }
        private int _selectedScope;
        public int SelectedScope
        {
            get => _selectedScope;
            set
            {
                if (_selectedScope != value)
                {
                    _selectedScope = value;
                    OnPropertyChanged();
                    CreateImageCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isLoadedCardList;
        public bool IsLoadedCardList
        {
            get => _isLoadedCardList;
            set
            {
                if (_isLoadedCardList != value)
                {
                    _isLoadedCardList = value;
                    OnPropertyChanged();
                    BrowseCardListDBCommand?.RaiseCanExecuteChanged();
                    BrowseRarityJSONFileCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private bool _isSavedCardList;
        public bool IsSavedCardList
        {
            get => _isSavedCardList;
            set
            {
                if (_isSavedCardList != value)
                {
                    _isSavedCardList = value;
                    OnPropertyChanged();
                    SaveAllCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _isCreateImageRunning;
        public bool IsCreateImageRunning
        {
            get => _isCreateImageRunning;
            set
            {
                if (_isCreateImageRunning != value)
                {
                    _isCreateImageRunning = value;
                    OnPropertyChanged();
                    CreateImageCommand?.RaiseCanExecuteChanged();
                    CancelCreateImageCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion

        #region Progress Bar
        private double _progressHeight;
        public double ProgressHeight
        {
            get => _progressHeight;
            set
            {
                if (_progressHeight != value)
                {
                    _progressHeight = value;
                    OnPropertyChanged();
                }
            }
        }
        private int _progressValue = 0;
        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                if (_progressValue != value)
                {
                    _progressValue = value;
                    OnPropertyChanged();
                    UpdateRunnerOffset();
                }
            }
        }
        private int _progressMaximum;
        public int ProgressMaximum
        {
            get => _progressMaximum;
            set
            {
                if (_progressMaximum != value)
                {
                    _progressMaximum = value;
                    OnPropertyChanged();
                    UpdateRunnerOffset();
                }
            }
        }
        private double _runnerOffsetX;
        public double RunnerOffsetX
        {
            get => _runnerOffsetX;
            set
            {
                if (_runnerOffsetX != value)
                {
                    _runnerOffsetX = value;
                    OnPropertyChanged();
                }
            }
        }
        private double _progressBarWidth;
        public double ProgressBarWidth
        {
            get => _progressBarWidth;
            set
            {
                if (_progressBarWidth != value)
                {
                    _progressBarWidth = value;
                    OnPropertyChanged();
                    UpdateRunnerOffset();
                }
            }
        }
        private int _progressCounter = 0;
        #endregion

        #region Folder
        public CardEditor.Models.Settings.ImageSetting imageSetting { get; set; }
        public CardEditor.Models.Settings.ImageSettingSource imageSettingSource { get; set; }
        public CardEditor.Models.Settings.DataHandlingSetting dataHandlingSetting { get; set; }
        #endregion

        #endregion

        #region Commands

        #region Rare Card
        public RelayCommand FilterCardCommand { get; private set; }
        public RelayCommand UnFilterCardCommand { get; private set; }
        public RelayCommand CreateImageCommand { get; private set; }
        public RelayCommand CancelCreateImageCommand { get; private set; }
        public RelayCommand LoadCardCommand { get; private set; }
        public RelayCommand CancelLoadCardCommand { get; private set; }
        public RelayCommand BrowseCardListDBCommand { get; private set; }
        public RelayCommand BrowseRarityJSONFileCommand { get; private set; }
        public RelayCommand AddCardCommand { get; private set; }
        public RelayCommand ModifyCardCommand { get; private set; }
        public RelayCommand SaveAllCommand { get; private set; }
        public RelayCommand SortCardCommand { get; private set; }
        public RelayCommand UnSortCardCommand { get; private set; }
        public RelayCommand ResetCardCommand { get; private set; }
        public RelayCommand ClearCardCommand { get; private set; }
        public RelayCommand DeleteSingleCardCommand { get; private set; }
        public RelayCommand DeleteSelectedCardCommand { get; private set; }
        public RelayCommand DeleteFoundCardCommand { get; private set; }
        public RelayCommand DeleteAllCardCommand { get; private set; }
        public RelayCommand ViewImageCommand { get; private set; }
        public RelayCommand OpenFileCommand { get; private set; }
        public RelayCommand OpenKonamiDBCommand { get; private set; }
        public RelayCommand OpenYugipediaCommand { get; private set; }
        public RelayCommand OpenYGOResourcesCommand { get; private set; }
        #endregion

        #region Rare List
        public RelayCommand ClearImgPathCommand { get; private set; }
        public RelayCommand BrowseCommand { get; private set; }
        public RelayCommand AddRareCommand { get; private set; }
        public RelayCommand ModifyRareCommand { get; private set; }
        public RelayCommand ResetRareCommand { get; private set; }
        public RelayCommand ClearRareCommand { get; private set; }
        public RelayCommand DeleteRareCommand { get; private set; }
        #endregion

        #region Folder
        public RelayCommand ClearOriginalPathCommand { get; private set; }
        public RelayCommand ClearOutputPathCommand { get; private set; }
        public RelayCommand BrowseOriginalPathCommand { get; private set; }
        public RelayCommand BrowseOutputPathCommand { get; private set; }
        public RelayCommand ReloadFolderCommand { get; private set; }
        public RelayCommand SaveFolderCommand { get; private set; }
        #endregion

        #endregion

        #region Constructor
        public RareViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
            else
            {
                ImageCardSource = ImageCacheService.Instance.Get(AppImage.Blank);
                ImagecardTooltip = CMess.toolCardImg.ToText();
            }

            InitializeCommands();
            InitializeSettingSource();
            if (LoadSettingSource())
            {
                LoadSetting();
            }

            InitializeCollections();
            SubscribeToDataChanges();

            CancelLoadRareCard = Visibility.Collapsed;
            SelectedScope = -1;
            ProgressHeight = 0;
            IsSavedCardList = RareRawDataViewModel.Instance.IsSaveRareCardListToDB;

            InitializeEvent();
        }
        private void InitializeSettingSource()
        {
            dataHandlingSetting = new Models.Settings.DataHandlingSetting();
            imageSetting = new Models.Settings.ImageSetting();
            imageSettingSource = new Models.Settings.ImageSettingSource();
        }
        private void InitializeEvent()
        {
            imageSetting.PropertyChanged += ImageSetting_PropertyChanged;
            dataHandlingSetting.PropertyChanged += DataHandlingSetting_PropertyChanged;
        }

        private bool LoadSettingSource()
        {
            try
            {
                List<ImageFormat> imageFormats = Enum.GetValues(typeof(ImageFormat)).Cast<ImageFormat>().ToList();
                imageSettingSource.ImageFormats.AddRange(imageFormats);

                var (resultSeries, messageSeries) = GeneraImageViewModel.Instance.LoadSeriesList();

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
            dataHandlingSetting = ConfigViewModel.Instance.dataHandlingSetting.Clone();
            imageSetting = ConfigViewModel.Instance.imageSetting.Clone();

            var selectedCardMaker = imageSettingSource.CardMakers
                .FirstOrDefault(cm => cm.DisplayName == imageSetting.SelectedCardMaker?.DisplayName);
            if (selectedCardMaker != null) imageSetting.SelectedCardMaker = selectedCardMaker;
        }

        private void InitializeCollections()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                RareItems = new BulkObservableCollection<RareItem>();
                RareCards = new BulkObservableCollection<RareCard>();

                SelectedRareItems = new BulkObservableCollection<RareItem>();
                SelectedRareCards = new BulkObservableCollection<RareCard>();
                SelectedComboRareItems = new BulkObservableCollection<RareItem>();

                _rareItemsView = (ListCollectionView)CollectionViewSource.GetDefaultView(RareItems);
                _rareItemsView.IsLiveSorting = true;
                _rareItemsView.IsLiveFiltering = true;
                _rareItemsView.IsLiveGrouping = false;

                _rareCardsView = (ListCollectionView)CollectionViewSource.GetDefaultView(RareCards);
                _rareCardsView.IsLiveSorting = false;
                _rareCardsView.IsLiveFiltering = false;
                _rareCardsView.IsLiveGrouping = false;

                //int arrangeValue = ConfigViewModel.Instance.Arrange;
                //switch (arrangeValue)
                //{
                //    case 1: _rareCardsView.SortDescriptions.Add(new SortDescription("id", ListSortDirection.Ascending)); break;
                //    case 2: _rareCardsView.SortDescriptions.Add(new SortDescription("id", ListSortDirection.Descending)); break;
                //    case 3: _rareCardsView.SortDescriptions.Add(new SortDescription("name", ListSortDirection.Ascending)); break;
                //    case 4: _rareCardsView.SortDescriptions.Add(new SortDescription("name", ListSortDirection.Descending)); break;
                //    default: _rareCardsView.SortDescriptions.Add(new SortDescription("id", ListSortDirection.Ascending)); break;
                //}
            });
        }
        private void EnableLiveFeatures()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(() =>
                {
                    if (_rareCardsView != null)
                    {
                        _rareCardsView.IsLiveSorting = true;
                        _rareCardsView.IsLiveFiltering = true;
                    }
                }));
        }
        public void LoadDataCallFromView()
        {
            var rawData = RareRawDataViewModel.Instance;
            RareItems.Clear();
            RareItems.AddRange(rawData.RareItemsData.Values);
        }

        private void InitializeCommands()
        {
            FilterCardCommand = new CardEditor.Commands.RelayCommand(_ => FilterData(),_ => CanFilterSortData());
            UnFilterCardCommand = new CardEditor.Commands.RelayCommand(_ => UnFilterData());
            CreateImageCommand = new CardEditor.Commands.RelayCommand(async _ => await CreateImageFunction(), _ => CanCreateImageFunction());
            CancelCreateImageCommand = new CardEditor.Commands.RelayCommand(_ => CancelImageGeneration(), _ => CanCancelImageGeneration());
            LoadCardCommand = new CardEditor.Commands.RelayCommand(async _ => await LoadRareCardDataCommand());
            CancelLoadCardCommand = new CardEditor.Commands.RelayCommand(_ => CancelLoadRareCardCommand());
            BrowseCardListDBCommand = new CardEditor.Commands.RelayCommand(async _ => await BrowseCardListDB(), _ => IsLoadedCardList);
            BrowseRarityJSONFileCommand = new CardEditor.Commands.RelayCommand(async _ => await BrowseRarityJSONFile(), _ => IsLoadedCardList);
            AddCardCommand = new CardEditor.Commands.RelayCommand(async _ => await ModifyRareCard(RareCardId.Value, RareCardName, RareCardRare), _ => CanModifyRareCard());
            ModifyCardCommand = new CardEditor.Commands.RelayCommand(async _ => await ModifyRareCard(RareCardId.Value, RareCardName, RareCardRare), _ => CanModifyRareCard());
            SaveAllCommand = new CardEditor.Commands.RelayCommand(async _ => await SaveAllCard(), _ => !IsSavedCardList);
            SortCardCommand = new CardEditor.Commands.RelayCommand(_ => SortCard(), _ => CanFilterSortData());
            UnSortCardCommand = new CardEditor.Commands.RelayCommand(_ => UnSortCard());
            ResetCardCommand = new CardEditor.Commands.RelayCommand(_ => ResetRareCard(), _ => CanResetClearRareCard());
            ClearCardCommand = new CardEditor.Commands.RelayCommand(_ => ClearRareCardCommand(), _ => CanResetClearRareCard());
            DeleteSingleCardCommand = new CardEditor.Commands.RelayCommand(async _ => await DeleteSingleRareCardCommand(), _ => CanDeleteSingleRareCard());
            DeleteSelectedCardCommand = new CardEditor.Commands.RelayCommand(async _ => await DeleteMultipleRareCardCommand(0), _ => CanDeleteSelectedRareCard());
            DeleteFoundCardCommand = new CardEditor.Commands.RelayCommand(async _ => await DeleteMultipleRareCardCommand(1), _ => CanDeleteFoundRareCard());
            DeleteAllCardCommand = new CardEditor.Commands.RelayCommand(async _ => await DeleteMultipleRareCardCommand(2), _ => CanDeleteAllRareCard());
            ViewImageCommand = new CardEditor.Commands.RelayCommand(_ => ViewImage(), _ => SelectedCardExist());
            OpenFileCommand = new CardEditor.Commands.RelayCommand(_ => OpenFile(), _ => SelectedCardExist());
            OpenKonamiDBCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenKonamiDB(), _ => SelectedCardExist());
            OpenYugipediaCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenYugipedia(), _ => SelectedCardExist());
            OpenYGOResourcesCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenYGOResources(), _ => SelectedCardExist());

            ClearImgPathCommand = new CardEditor.Commands.RelayCommand(_ => ClearImgPath());
            BrowseCommand = new CardEditor.Commands.RelayCommand(_ => BrowseImgPath());
            AddRareCommand = new CardEditor.Commands.RelayCommand(async _ => await ModifyRareItem(), _ => CanModifyRareItem());
            ModifyRareCommand = new CardEditor.Commands.RelayCommand(async _ => await ModifyRareItem(), _ => CanModifyRareItem());
            ResetRareCommand = new CardEditor.Commands.RelayCommand(_ => ResetRareItem(), _ => CanResetClearDeleteRareItem());
            ClearRareCommand = new CardEditor.Commands.RelayCommand(_ => ClearRareItem(), _ => CanResetClearDeleteRareItem());
            DeleteRareCommand = new CardEditor.Commands.RelayCommand(async _ => await DeleteRareItemCommand(), _ => CanResetClearDeleteRareItem());

            ClearOriginalPathCommand = new CardEditor.Commands.RelayCommand(_ => ClearOriginalPath(),_=> CanClearOriginalPath());
            BrowseOriginalPathCommand = new CardEditor.Commands.RelayCommand(_ => BrowseOriginalPath());
            ClearOutputPathCommand = new CardEditor.Commands.RelayCommand(_ => ClearOutputPath(), _ => CanClearOutputPath());
            BrowseOutputPathCommand = new CardEditor.Commands.RelayCommand(_ => BrowseOutputPath());
            ReloadFolderCommand = new CardEditor.Commands.RelayCommand(_ => LoadSetting());
            SaveFolderCommand = new CardEditor.Commands.RelayCommand(_ => SaveFolderPath(), _ => CanSaveFolderPath());
        }
        private void SubscribeToDataChanges()
        {
            RareRawDataViewModel.Instance.OnDataChanged += HandleDataChanged;
            RareRawDataViewModel.Instance.OnErrorOccurred += HandleError;
        }
        private void HandleDataChanged()
        {
            // _ = HandleDataChangedAsync();
            // _ = LoadRareCardsLazy();
            _ = LoadRareCardsOptimized();
        }
        private void HandleDataChangedAsync()
        {
            try
            {
                _loadRareCardCTS?.Cancel();
                _loadRareCardCTS = new CancellationTokenSource();
                var cancellationToken = _loadRareCardCTS.Token;

                IsCardListLoading = true;
                var rawData = RareRawDataViewModel.Instance.RareCardsData;

                if (RareCards == null) RareCards = new BulkObservableCollection<RareCard>();
                else RareCards.Clear();

                RareCards.AddRange(rawData.Values);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Load RareCards bị hủy.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("HandleDataChangedAsync error: " + ex.Message);
            }
        }

        private void HandleError(string errorMessage)
        {
            var request = new MessageBoxRequest
            {
                Title = CMess.error.ToText(),
                IconType = CMSG.MessageBoxIconType.Error,
                Message = $"{CMess.errorOcc.ToText()} {errorMessage}",
                Buttons = new[] { CMess.ok.ToText() },
                ResponseSource = null
            };
            OnMessageBoxRequested(request);
        }
        #endregion

        #region Change
        private void OnSelectedRareItemChanged()
        {
            if (SelectedRareItem != null)
            {
                IdRare = SelectedRareItem.IdRare;
                RareName = SelectedRareItem.Name;
                if (SelectedRareItem.Code > 0 && (SelectedRareItem.Code & (SelectedRareItem.Code - 1)) == 0)
                {
                    int bit = (int)(Math.Log(SelectedRareItem.Code) / Math.Log(2));
                    RareCode = bit;
                }
                RareImagePath = SelectedRareItem.ImagePath;
                IsUserChangeImagePath = false;
            }
            else
            {
                IdRare = null;
                RareName = string.Empty;
                RareCode = null;
                RareImagePath = string.Empty;
                IsUserChangeImagePath = false;
            }
        } //ok
        private async void OnSelectedRareCardChanged()
        {
            _findImgCardCTS?.Cancel();
            _findImgCardCTS = new CancellationTokenSource();
            var token = _findImgCardCTS.Token;

            try
            {
                if (SelectedRareCard != null)
                {
                    RareCardName = SelectedRareCard.name;
                    RareCardId = SelectedRareCard.id;
                    RareCardRare = SelectedRareCard.rare;
                    await LoadCardImage(token);
                }
                else
                {
                    RareCardName = string.Empty;
                    RareCardId = null;
                    RareCardRare = 0;
                    ImageCardSource = new BitmapImage(new Uri("pack://application:,,,/Images/Blank.png", UriKind.Absolute));
                    ImagecardTooltip = CMess.toolCardImg.ToText();
                }
            }
            catch (OperationCanceledException)
            {
                /// Operation was canceled, do nothing
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnSelectedRareCardChanged: {ex.Message}");
            }
        } //ok

        private void SelectedRareItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsSynSelectedRareItems) return;
            // UpdateRareCardRareFromSelectedRareItems();

            try
            {
                IsSynSelectedRareItems = true;

                RecalculateSelectedRareItems();
            }
            catch
            {

            }
            finally
            {
                IsSynSelectedRareItems = false;
            }
        }
        private void SelectedRareCards_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsSynSelectedRareCards) return;
            try
            {
                IsSynSelectedRareCards = true;

                RecalculateSelectedRareCards();
            }
            catch
            {

            }
            finally
            {
                IsSynSelectedRareCards = false;
            }
        }
        private void SelectedComboRareItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsSynSelectedComboRareItems || IsSynRareCardRare) return;
            try
            {
                IsSynSelectedComboRareItems = true;

                RecalculateSelectedComboRareItems();
            }
            catch
            {

            }
            finally
            {
                IsSynSelectedComboRareItems = false;
            }
        }

        private void OnSelectedRareItemsChanged()
        {
            Debug.WriteLine($"SelectedRareItems Count: {SelectedRareItemsCount}");
        }
        private void OnSelectedRareCardsChanged()
        {
            Debug.WriteLine($"SelectedRareCards Count: {SelectedRareCardsCount}");
        }
        private void OnSelectedComboRareItemsChanged()
        {
            Debug.WriteLine($"SelectedComboRareItems Count: {SelectedComboRareItemsCount}");
        }
        private void RecalculateSelectedRareItems()
        {

        }
        private void RecalculateSelectedRareCards()
        {
            Debug.WriteLine($"SelectedRareCards changed: {SelectedRareCards.Count} items selected.");

            foreach (var item in SelectedRareCards)
            {
                Debug.WriteLine($"Selected: {item.name} (id: {item.id})");
            }
        }
        private void RecalculateSelectedComboRareItems()
        {
            long combinedBits = 0;
            foreach (var item in SelectedComboRareItems)
            {
                combinedBits |= item.Code;
            }
            if (RareCardRare != combinedBits)
                RareCardRare = combinedBits;
        }
        private void UpdateSelectedComboRareItemsFromRareCardRare()
        {
            if (RareItems == null || RareItems.Count == 0)
            {
                if (SelectedComboRareItems?.Count > 0)
                    SelectedComboRareItems.Clear();
                return;
            }
            if (SelectedComboRareItems == null) return;

            var matchingItems = RareItems.Where(item => (item.Code & RareCardRare) != 0).ToList();

            SelectedComboRareItems.Clear();
            SelectedComboRareItems.AddRange(matchingItems);
        }
        #endregion

        #region Rare Card
        public async Task LoadRareCardDataCommand()
        {
            if (!RareRawDataViewModel.Instance.IsLoadRareCardListFromDB)
            {
                await RareRawDataViewModel.Instance.LoadRareCardDataFromDatabase();
                // await LoadRareCardsLazy();
                await LoadRareCardsOptimized();
            }
            else
            {
                if (!IsLoadedCardList)
                {
                    await LoadRareCardsOptimized();
                }
                else
                {
                    if (!RareRawDataViewModel.Instance.IsSaveRareCardListToDB)
                    {
                        var request = new MessageBoxRequest
                        {
                            Title = CMess.questi.ToText(),
                            IconType = CMSG.MessageBoxIconType.Question,
                            Message = $"{CMess.HasUnSaveData.ToText()} {string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.tlReload.ToText(), CMess.ListRareDB.ToText())}",
                            Buttons = new[] { CMess.originaData.ToText(), CMess.unSavedData.ToText(), CMess.cancel.ToText() },
                            ResponseSource = new TaskCompletionSource<int>()
                        };
                        OnMessageBoxRequested(request);
                        int result = await request.ResponseTask;

                        if (result == 0)
                        {
                            await RareRawDataViewModel.Instance.LoadRareCardDataFromDatabase();
                            await LoadRareCardsOptimized();
                        }
                        else if (result == 1)
                        {
                            await LoadRareCardsOptimized();
                        }
                        else
                        {
                            Debug.WriteLine("User huy tai lai RareCards tu database.");
                        }
                    }
                }
            }
        }

        private async Task LoadRareCardsOptimized()
        {
            try
            {
                IsCardListLoading = true;
                CancelLoadRareCard = Visibility.Visible;

                // Cancel previous loading operation
                _loadRareCardCTS?.Cancel();
                _loadRareCardCTS = new CancellationTokenSource();
                var cancellationToken = _loadRareCardCTS.Token;

                // Bước 1: Load data ở background thread
                Debug.WriteLine("Starting load operation...");
                var startLoadRawW = Stopwatch.StartNew();

                List<RareCard> rawData = null;

                await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    rawData = RareRawDataViewModel.Instance.RareCardsData.Values.ToList();
                }, cancellationToken);
                startLoadRawW.Stop();
                Debug.WriteLine($"Data loaded in {startLoadRawW.ElapsedMilliseconds} ms. Total items: {rawData.Count}");

                cancellationToken.ThrowIfCancellationRequested();

                // Bước 2: Khởi tạo collection trên UI thread
                Debug.WriteLine("Initializing collection on UI thread...");
                var startInitial = Stopwatch.StartNew();
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Reset collection và view
                    RareCards?.Clear();
                    // Tạo view mới và TẮT live operations trong khi load
                    _rareCardsView = (ListCollectionView)CollectionViewSource.GetDefaultView(RareCards);
                    _rareCardsView.IsLiveFiltering = false;
                    _rareCardsView.IsLiveSorting = false;
                    _rareCardsView.IsLiveGrouping = false;

                    OnPropertyChanged(nameof(RareCardsView));
                }, DispatcherPriority.Normal);
                startInitial.Stop();
                cancellationToken.ThrowIfCancellationRequested();

                // Bước 3: Load data theo batch
                Debug.WriteLine("Loading data in batches...");
                var startLoadBatch = Stopwatch.StartNew();
                await LoadDataInBatches(rawData, cancellationToken);
                startLoadBatch.Stop();
                Debug.WriteLine($"Data loaded in batches in {startLoadBatch.ElapsedMilliseconds} ms.");

                // Bước 4: Bật lại live operations
                Debug.WriteLine("⏳ Enabling live features...");
                var startEnableLive = Stopwatch.StartNew();
                //await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                //{
                //    _rareCardsView.IsLiveFiltering = true;
                //    _rareCardsView.IsLiveSorting = true;
                //    // Setup các live properties nếu cần
                //    _rareCardsView.LiveFilteringProperties.Add("id");
                //    _rareCardsView.LiveFilteringProperties.Add("name");
                //    _rareCardsView.LiveFilteringProperties.Add("rare");
                //}, DispatcherPriority.Background);
                startEnableLive.Stop();
                Debug.WriteLine($"Live features enabled in {startEnableLive.ElapsedMilliseconds} ms.");
                IsLoadedCardList = true;
                Debug.WriteLine($"Load completed: {rawData.Count()} items");
            }
            catch (OperationCanceledException)
            {
                IsLoadedCardList = false;
                Debug.WriteLine("Load operation cancelled");
            }
            catch (Exception ex)
            {
                IsLoadedCardList = false;
                Debug.WriteLine($"Error loading data: {ex.Message}");
            }
            finally
            {
                IsCardListLoading = false;
                IsSavedCardList = RareRawDataViewModel.Instance.IsSaveRareCardListToDB;
                CancelLoadRareCard = Visibility.Collapsed;
            }
        }
        private async Task LoadDataInBatches(List<RareCard> allData, CancellationToken cancellationToken)
        {
            const int BATCH_SIZE = 500; // Điều chỉnh theo performance

            int total = allData.Count;
            int totalBatches = (int)Math.Ceiling((double)total / BATCH_SIZE);

            for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int start = batchIndex * BATCH_SIZE;
                int count = Math.Min(BATCH_SIZE, total - start);

                var batch = new List<RareCard>(count);
                for (int i = 0; i < count; i++)
                {
                    batch.Add(allData[start + i]);
                }

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    RareCards.AddRange(batch);
                }, DispatcherPriority.Background);

                if (batchIndex % 2 == 0)
                {
                    await Dispatcher.Yield(DispatcherPriority.Background);
                }

                var progress = (double)(batchIndex + 1) / totalBatches * 100;
                Debug.WriteLine($"Loading progress: {progress:F1}%");
            }
        }

        private async Task LoadRareCardsLazy()
        {
            try
            {
                IsCardListLoading = true;
                CancelLoadRareCard = Visibility.Visible;
                _loadRareCardCTS?.Cancel();
                _loadRareCardCTS = new CancellationTokenSource();
                var cancellationToken = _loadRareCardCTS.Token;
                
                var rawData = RareRawDataViewModel.Instance.RareCardsData;

                if (RareCards == null) RareCards = new BulkObservableCollection<RareCard>();
                else RareCards.Clear();

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _rareCardsView = null;
                    OnPropertyChanged(nameof(RareCardsView)); // Notify UI rằng View đã null
                });

                int batchSize = rawData.Count > 10000 ? 50 : 500;
                RareCards.AddRange(rawData.Values);

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _rareCardsView = (ListCollectionView)CollectionViewSource.GetDefaultView(RareCards);
                    _rareCardsView.IsLiveSorting = true;
                    _rareCardsView.IsLiveFiltering = true;
                    _rareCardsView.IsLiveGrouping = false;

                    OnPropertyChanged(nameof(RareCardsView));
                });
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Load RareCards bị hủy.");
            }
            catch (Exception ex)
            {
                // Log hoặc raise error cho UI
                Debug.WriteLine($"Lỗi khi load RareCards: {ex.Message}");
            }
            finally
            {
                IsCardListLoading = false;
                CancelLoadRareCard = Visibility.Collapsed;
            }
        }
        private void CancelLoadRareCardCommand()
        {
            _loadRareCardCTS?.Cancel();
        }

        public long FindRareCode(ulong id)
        {
            return RareRawDataViewModel.Instance.GetRareByCardId(id);
        }

        private void FilterData()
        {
            if (RareCards == null || RareCardsView == null) return;
            int advancedSettings = ConfigViewModel.Instance.dataHandlingSetting.Advanced;
            bool isAdvancedFind = (advancedSettings & 0x01) == 0x01; // bit 1: Advanced Find
            bool matchCase = (advancedSettings & 0x02) == 0x02;     // bit 2: Match Case
            bool useWildcards = (advancedSettings & 0x04) == 0x04;  // bit 3: Use Wildcards
            bool matchPrefix = (advancedSettings & 0x08) == 0x08;   // bit 4: Match Prefix
            bool matchSuffix = (advancedSettings & 0x10) == 0x10;   // bit 5: Match Suffix
            bool wholeWords = (advancedSettings & 0x20) == 0x20;    // bit 6: Find Whole Words Only
            bool ignorePunctuation = (advancedSettings & 0x40) == 0x40; // bit 7: Ignore Punctuation
            bool ignoreWhitespace = (advancedSettings & 0x80) == 0x80;  // bit 8: Ignore White-Space

            var filterId = RareCardId;
            var fillerName = RareCardName;
            var fillerRare = RareCardRare;

            RareCardsView.Filter = item =>
            {
                var card = (CardEditor.Models.RareCard)item;

                bool matchesId = !filterId.HasValue || card.id == filterId;
                bool matchesRare = fillerRare == 0 || (card.rare & fillerRare) == fillerRare;
                bool MatchString(string source, string pattern)
                {
                    if (string.IsNullOrWhiteSpace(pattern)) return true;

                    // Chuẩn hóa chuỗi trước khi so sánh
                    string normalizedSource = source ?? "";
                    string normalizedPattern = pattern ?? "";
                    if (isAdvancedFind)
                    {
                        if (ignorePunctuation)
                        {
                            normalizedSource = new string(normalizedSource.Where(c => !char.IsPunctuation(c)).ToArray());
                            normalizedPattern = new string(normalizedPattern.Where(c => !char.IsPunctuation(c)).ToArray());
                        }
                        if (ignoreWhitespace)
                        {
                            normalizedSource = normalizedSource.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                            normalizedPattern = normalizedPattern.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
                        }
                    }

                    // Xác định cách so sánh (phân biệt hoa thường hay không)
                    StringComparison comparison = (isAdvancedFind && matchCase)
                        ? StringComparison.Ordinal
                        : StringComparison.OrdinalIgnoreCase;

                    if (!isAdvancedFind)
                    {
                        // Logic mặc định khi Advanced Find tắt
                        return normalizedSource.IndexOf(normalizedPattern, comparison) >= 0;
                    }

                    // Xử lý khi Advanced Find bật
                    if (useWildcards)
                    {
                        // Chuyển wildcard sang regex pattern
                        string regexPattern = Regex.Escape(normalizedPattern)
                            .Replace("\\*", ".*")
                            .Replace("\\?", ".");

                        // Áp dụng các điều kiện bổ sung nếu có
                        if (matchPrefix) regexPattern = "^" + regexPattern;
                        if (matchSuffix) regexPattern += "$";
                        if (wholeWords) regexPattern = $"\\b{regexPattern}\\b";

                        return Regex.IsMatch(normalizedSource, regexPattern, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        // Không dùng wildcard, xử lý các điều kiện khác
                        bool matches = normalizedSource.IndexOf(normalizedPattern, comparison) >= 0;

                        if (matchPrefix) matches = matches && normalizedSource.StartsWith(normalizedPattern, comparison);
                        if (matchSuffix) matches = matches && normalizedSource.EndsWith(normalizedPattern, comparison);
                        if (wholeWords)
                        {
                            string regexPattern = $"\\b{Regex.Escape(normalizedPattern)}\\b";
                            matches = Regex.IsMatch(normalizedSource, regexPattern, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
                        }

                        return matches;
                    }
                }
                bool matchesName = MatchString(card.name, fillerName);

                return matchesId && matchesRare && matchesName;
            };

            var request = new MessageBoxRequest
            {
                Title = CMess.notifi.ToText(),
                IconType = CMSG.MessageBoxIconType.Notification,
                Message = string.Format(CMess.filterSuc.ToText(), RareCardsView.Cast<CardEditor.Models.RareCard>().Count(), RareCards.Count),
                Buttons = new[] { CMess.ok.ToText() },
                ResponseSource = null
            };
            OnMessageBoxRequested(request);
        }
        private void UnFilterData()
        {
            RareCardsView.Filter = null;
        }
        private void UpdateRunnerOffset()
        {
            if (ProgressMaximum > 1)
            {
                double progress = (double)ProgressValue / ProgressMaximum;
                double newOffset = ProgressBarWidth * progress;

                if (System.Windows.Application.Current.Dispatcher.CheckAccess())
                {
                    RunnerOffsetX = newOffset;
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        RunnerOffsetX = newOffset;
                    });
                }
            }
        }
        private async Task CreateImageFunction()
        {
            IsCreateImageRunning = true;
            _progressCounter = 0;
            ProgressValue = 0;
            ProgressHeight = 65;
            _createImgCardCTS?.Cancel();
            _createImgCardCTS = new CancellationTokenSource();
            var token = _createImgCardCTS.Token;
            try
            {
                if (IsUserChangeFolderpath)
                {
                    if (CanSaveFolderPath()) await SaveFolderPath();
                    else return;
                }

                if (ImageValidate.isValid == false ||
                    GeneraImageViewModel.Instance.IsChangedSeries == false ||
                    GeneraImageViewModel.Instance.IsLoadedImageCache == false ||
                    RareRawDataViewModel.Instance.IsLoadedImageRareCache == false ||
                    RareRawDataViewModel.Instance.IsLoadedImageRareRect == false)
                {
                    var (resultReload, resultMessage) = await ImageValidate.ReLoadImageData();
                    if (!resultReload)
                    {
                        var request = new MessageBoxRequest
                        {
                            Title = CMess.error.ToText(),
                            IconType = CMSG.MessageBoxIconType.Error,
                            Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Create.ToText(), CMess.Image.ToText())} {resultMessage}",
                            Buttons = new[] { CMess.ok.ToText() },
                            ResponseSource = null
                        };
                        OnMessageBoxRequested(request);
                        ProgressHeight = 0;
                        IsCreateImageRunning = false;
                        _createImgCardCTS = null;
                        return;
                    }

                    ImageValidate.MarkDirty();

                    var (checkResult, messResult) = await ImageValidate.CheckValidate();
                    if (!checkResult)
                    {
                        var request = new MessageBoxRequest
                        {
                            Title = CMess.error.ToText(),
                            IconType = CMSG.MessageBoxIconType.Error,
                            Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Create.ToText(), CMess.Image.ToText())} {messResult}",
                            Buttons = new[] { CMess.ok.ToText() },
                            ResponseSource = null
                        };
                        OnMessageBoxRequested(request);
                        ProgressHeight = 0;
                        IsCreateImageRunning = false;
                        _createImgCardCTS = null;
                        return;
                    }
                }

                if (!RareRawDataViewModel.Instance.IsLoadedImageRareCache)
                {
                    if (!await RareRawDataViewModel.Instance.LoadImageCache())
                    {
                        var request = new MessageBoxRequest
                        {
                            Title = CMess.error.ToText(),
                            IconType = CMSG.MessageBoxIconType.Error,
                            Message = CMess.errorLoadImageCache.ToText(),
                            Buttons = new[] { CMess.ok.ToText() },
                            ResponseSource = null
                        };
                        OnMessageBoxRequested(request);
                        ProgressHeight = 0;
                        IsCreateImageRunning = false;
                        _createImgCardCTS = null;
                        return;
                    }
                }

                if (!RareRawDataViewModel.Instance.IsLoadedImageRareRect)
                    RareRawDataViewModel.Instance.SetRarityLabelRect();

                if (ConfigViewModel.Instance.imageSetting.Secret != 0 && !CardEXDataViewModel.Instance.IsLoadedCard)
                {
                    try
                    {
                        await CardEXDataViewModel.Instance.LoadCardsEXAsync();
                    }
                    catch
                    {
                        int chooseFailed = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                            "Failed to load all cards from the data source. It will not be possible to generate images with the Secret layer and Rarity label for the Alternate Artwork version. Continue?",
                            new[] { CMess.yes.ToText(), CMess.no.ToText() });

                        if (chooseFailed != 0) return;
                    }
                }

                ImageGenerator.outputFolderPath = ConfigViewModel.Instance.imageSetting.OutPutFolder;
                ImageGenerator.originalFolderPath = ConfigViewModel.Instance.imageSetting.OriginalCardFolder;

                IEnumerable<RareCard> cardsToProcess = null;

                if (SelectedScope == 0)
                {
                    if (!SelectedRareCards.Any())
                    {
                        OnSnackbarRequested(CMess.noCardSelec.ToText());
                        return;
                    }
                    cardsToProcess = SelectedRareCards;
                }
                else if (SelectedScope == 1)
                {
                    cardsToProcess = RareCardsView.Cast<RareCard>();
                }
                else if (SelectedScope == 2)
                {
                    cardsToProcess = RareCards;
                }
                else
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.notifi.ToText(),
                        IconType = CMSG.MessageBoxIconType.Notification,
                        Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText()),
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(request);
                    return;
                }
                int successCount = 0;
                int failCount = 0;
                var failedIds = new List<ulong>();
                int total = cardsToProcess.Count();
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ProgressMaximum = total;
                    ProgressValue = 0;
                });

                int maxParallelism = Math.Max(Environment.ProcessorCount / 2, 1);
                var semaphore = new SemaphoreSlim(maxParallelism);
                var tasks = new List<Task>();

                foreach (var cardItem in cardsToProcess)
                {
                    if (token.IsCancellationRequested) break;

                    try
                    {
                        await semaphore.WaitAsync(token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    var task = Task.Run(async () =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            semaphore.Release();
                            return;
                        }
                        try
                        {
                            var (result, message) = await ImageGenerator.GenerateImageRare(cardItem);
                            if (result)
                            {
                                Interlocked.Increment(ref successCount);
                            }
                            else
                            {
                                Interlocked.Increment(ref failCount);
                                lock (failedIds)
                                {
                                    failedIds.Add(cardItem.id);
                                }
                            }
                        }
                        catch
                        {
                            Interlocked.Increment(ref failCount);
                            lock (failedIds)
                            {
                                failedIds.Add(cardItem.id);
                            }
                        }
                        finally
                        {
                            int newValue = Interlocked.Increment(ref _progressCounter);
                            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                ProgressValue = newValue;
                            });
                            semaphore.Release();
                        }
                    }, token);
                    tasks.Add(task);
                }
                try
                {
                    await Task.WhenAll(tasks);
                    semaphore.Dispose();
                }
                catch (OperationCanceledException)
                {
                    /// Operation was canceled, do nothing
                }
                finally
                {
                    semaphore.Dispose();
                }

                var messageBuilder = new StringBuilder();
                if (token.IsCancellationRequested) messageBuilder.AppendLine($"{CMess.cancelled.ToText()}");
                messageBuilder.AppendLine(string.Format(CMess.ThreePlaceholderSuccess.ToText(), successCount.ToString(), CMess.Image.ToText(), CMess.Create.ToText()));
                if (failCount > 0)
                {
                    messageBuilder.AppendLine(string.Format(CMess.geneImgFail.ToText(), failCount.ToString()));
                    if (failedIds.Count > 0)
                    {
                        messageBuilder.AppendLine(CMess.failListID.ToText());
                        lock (failedIds)
                        {
                            foreach (var id in failedIds)
                            {
                                messageBuilder.AppendLine(id.ToString());
                            }
                        }
                    }
                }

                var result = new MessageBoxRequest
                {
                    Title = CMess.notifi.ToText(),
                    IconType = CMSG.MessageBoxIconType.Notification,
                    Message = messageBuilder.ToString(),
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(result);
                int? resultMess = await result.ResponseTask;
                if (resultMess != null)
                {
                    _createImgCardCTS = null;
                    ProgressHeight = 0;
                    ProgressMaximum = 0;
                    ProgressValue = 0;
                }
            }
            catch (Exception ex)
            {
                ProgressHeight = 0;
                ProgressMaximum = 0;
                ProgressValue = 0;
                _createImgCardCTS = null;
                var result = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{CMess.errorOcc.ToText()} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(result);
            }
            finally
            {
                IsCreateImageRunning = false;
                _progressCounter = 0;
            }
        }
        private bool CanCreateImageFunction()
        {
            if (IsCreateImageRunning) return false;
            if (string.IsNullOrWhiteSpace(ConfigViewModel.Instance.imageSetting.OriginalCardFolder) ||
                string.IsNullOrWhiteSpace(ConfigViewModel.Instance.imageSetting.OutPutFolder) ||
                !Directory.Exists(ConfigViewModel.Instance.imageSetting.OriginalCardFolder) ||
                !Directory.Exists(ConfigViewModel.Instance.imageSetting.OutPutFolder))
            {
                return false;
            }
            if (SelectedScope < 0 || SelectedScope > 2) return false;
            switch (SelectedScope)
            {
                case 0: return SelectedRareCards != null && SelectedRareCards.Any();
                case 1: return RareCardsView != null && RareCardsView.Cast<RareCard>().Any();
                case 2: return RareCards != null && RareCards.Any();
                default: return false;
            }
            return false;
        }
        private void CancelImageGeneration()
        {
            _createImgCardCTS?.Cancel();
        }
        private bool CanCancelImageGeneration()
        {
            return IsCreateImageRunning;
        }
        private async Task BrowseCardListDB()
        {
            string filePath = FileDiaLogHelper.OpenRare();
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                if (!File.Exists(filePath)) return;

                bool Overwrite;
                if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 0)
                {
                    var requestDelete = new MessageBoxRequest
                    {
                        Title = CMess.questi.ToText(),
                        IconType = CMSG.MessageBoxIconType.Question,
                        Message = CMess.confirmWriteData.ToText(),
                        Buttons = new[] { CMess.OverwriteDupli.ToText(), CMess.Appendwrite.ToText(), CMess.cancel.ToText() },
                        ResponseSource = new TaskCompletionSource<int>()
                    };
                    OnMessageBoxRequested(requestDelete);
                    int result = await requestDelete.ResponseTask;
                    if (result == 0) Overwrite = true;
                    else if (result == 1) Overwrite = false;
                    else return;
                }
                else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 1) Overwrite = true;
                else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 2) Overwrite = false;
                else return;

                string extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

                switch (extension)
                {
                    case ".cdb":
                    case ".db":
                        if (filePath != CardAppContext.Instance.RareCardDBPath)
                            await RareRawDataViewModel.Instance.BrowseDataCardDataBase(filePath, Overwrite);
                        break;

                    case ".ceds":
                        await RareRawDataViewModel.Instance.BrowseDataCeds(filePath, Overwrite);
                        break;

                    case ".xlsx":
                        await RareRawDataViewModel.Instance.BrowseDataExcel(filePath, Overwrite);
                        break;
                    
                    case ".ydk":
                        await RareRawDataViewModel.Instance.BrowseDataDeck(filePath, Overwrite);
                        break;

                    default:
                        var request = new MessageBoxRequest
                        {
                            Title = CMess.notifi.ToText(),
                            IconType = CMSG.MessageBoxIconType.Notification,
                            Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()),
                            Buttons = new[] { CMess.ok.ToText() },
                            ResponseSource = null
                        };
                        OnMessageBoxRequested(request);
                        break;
                }
                IsSavedCardList = RareRawDataViewModel.Instance.IsSaveRareCardListToDB;
            }
        }
        private async Task BrowseRarityJSONFile()
        {
            var (resultLoad, messageLoad) = await MasterDuelAPIViewModel.Instance.LoadMDRarityFile();
            if (!resultLoad)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.load.ToText(), CMess.File.ToText())} {messageLoad}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
                return;
            }

            var (resultSyn, messageSyn) = await RareRawDataViewModel.Instance.SynchronizeRarityMapFromMDAPI();
            if (resultSyn)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.notifi.ToText(),
                    IconType = CMSG.MessageBoxIconType.Notification,
                    Message = string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.File.ToText(), CMess.load.ToText()),
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);

                RareRawDataViewModel.Instance.IsSaveRareCardListToDB = false;
                IsSavedCardList = RareRawDataViewModel.Instance.IsSaveRareCardListToDB;
            }
            else
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.load.ToText(), CMess.File.ToText())} {messageSyn}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }

        public async Task ModifyRareCard(ulong idCard, string name, long rare)
        {
            try
            {
                var newCard = new RareCard { id = idCard, name = name, rare = rare };

                var (resultDB, messageDB) = await RareRawDataViewModel.Instance.ModifyRareCardDatabase(newCard);

                if (resultDB)
                {
                    var (resultMemory, isAdd) = RareRawDataViewModel.Instance.ModifyRareCard(newCard);
                    if (resultMemory)
                    {
                        ModifyRareCardView(newCard);
                        // RareCardsView.Refresh();
                        OnSnackbarRequested(string.Format(CMess.ThreePlaceholderSuccess.ToText(), 1.ToString(), CMess.Card.ToText(), isAdd ? CMess.tlAdd.ToText() : CMess.Update.ToText()));
                    }
                }
                else
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.error.ToText(),
                        IconType = CMSG.MessageBoxIconType.Error,
                        Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.Card.ToText())} {messageDB}",
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(request);
                }
            }
            catch (Exception ex)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.Card.ToText())} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        private void ModifyRareCardView(RareCard newCard)
        {
            if (newCard == null) return;
            var existing = RareCards.FirstOrDefault(card => card.id == newCard.id);

            if (existing != null)
            {
                // Cập nhật thuộc tính nếu khác
                if (existing.name != newCard.name)
                    existing.name = newCard.name;
                if (existing.rare != newCard.rare)
                    existing.rare = newCard.rare;

                SelectedRareCard = existing;
            }
            else
            {
                int insertIndex = RareCards.TakeWhile(card => card.id < newCard.id).Count();
                RareCards.Insert(insertIndex, newCard);
                SelectedRareCard = newCard;
                _rareCardsView.MoveCurrentTo(newCard);
            }
        }
        private bool CanModifyRareCard()
        {
            return (RareCardId.HasValue && RareCardId.Value > 0 && RareCardId.Value <= uint.MaxValue && RareCardRare >= 0);
        }
        private async Task SaveAllCard()
        {
            try
            {
                var (result, message) = await RareRawDataViewModel.Instance.SaveAllRareCardDatabase();
                if (result)
                {
                    OnSnackbarRequested(string.Format(CMess.ThreePlaceholderSuccess.ToText(), message, CMess.Card.ToText(), CMess.Save.ToText()));
                }
                else
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.error.ToText(),
                        IconType = CMSG.MessageBoxIconType.Error,
                        Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.Card.ToText())} {message}",
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(request);
                }
                IsSavedCardList = RareRawDataViewModel.Instance.IsSaveRareCardListToDB;
            }
            catch (Exception ex)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.Card.ToText())} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
                IsSavedCardList = RareRawDataViewModel.Instance.IsSaveRareCardListToDB;
            }
        }
        private void SortCard()
        {
            var selectedSorts = SortsViewModel.Instance.SelectedSortItems;
            if (selectedSorts == null || selectedSorts.Count <= 0) return;

            try
            {
                using (RareCardsView.DeferRefresh())
                {
                    RareCardsView.SortDescriptions.Clear();
                    foreach (var sort in selectedSorts)
                    {
                        if (sort?.SelectedItem == null) continue;
                        if (string.IsNullOrEmpty(sort.SelectedItem.Name)) continue;

                        string cardPropertyName = sort.SelectedItem.Name switch
                        {
                            "id" => "id",
                            "name" => "name",
                            "Rare" => "rare",
                            _ => null
                        };
                        if (string.IsNullOrEmpty(cardPropertyName)) continue;
                        var direction = sort.OrderByAsc ? ListSortDirection.Ascending : ListSortDirection.Descending;

                        RareCardsView.SortDescriptions.Add(new SortDescription(cardPropertyName, direction));
                    }
                }
            }
            catch (Exception ex)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Sort.ToText(), CMess.cardrare.ToText())} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        private void UnSortCard()
        {
            if (RareCardsView == null || RareCards == null) return;
            RareCardsView.SortDescriptions.Clear();
            RareCardsView.Refresh();
        }
        private bool CanFilterSortData()
        {
            return (RareCards != null && RareCards.Count >= 2);
        }
        private async void ResetRareCard()
        {
            if (SelectedRareCard != null)
            {
                if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmReSet)
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.questi.ToText(),
                        IconType = CMSG.MessageBoxIconType.Question,
                        Message = string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.tlReset.ToText(), CMess.SelectCards.ToText()),
                        Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                        ResponseSource = new TaskCompletionSource<int>()
                    };
                    OnMessageBoxRequested(request);
                    int result = await request.ResponseTask;
                    if (result != 0) return;
                }
                OnSelectedRareCardChanged();
            }
            else
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = CMSG.MessageBoxIconType.Warning,
                    Message = CMess.noCardSelec.ToText(),
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        private async void ClearRareCardCommand()
        {
            if (SelectedRareCard != null)
            {
                if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmClear)
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.conClear.ToText(),
                        IconType = CMSG.MessageBoxIconType.Question,
                        Message = string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.tlClear.ToText(), CMess.SelectCards.ToText()),
                        Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                        ResponseSource = new TaskCompletionSource<int>()
                    };
                    OnMessageBoxRequested(request);
                    int result = await request.ResponseTask;
                    if (result != 0) return;
                }
                ClearAllRareCard();
            }
            else
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = CMSG.MessageBoxIconType.Warning,
                    Message = CMess.noCardSelec.ToText(),
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        private bool CanResetClearRareCard()
        {
            return SelectedRareCard != null; 
        }
        private void ClearAllRareCard()
        {
            RareCardName = string.Empty;
            RareCardId = null;
            RareCardRare = 0;
        }

        #region Delete Single RareCard
        private async Task DeleteSingleRareCardCommand()
        {
            if (SelectedRareCard == null && !RareCardId.HasValue)
            {
                var requestNoSelect = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = CMSG.MessageBoxIconType.Warning,
                    Message = CMess.noCardSelec.ToText(),
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(requestNoSelect);
                return;
            }
            if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmDelete)
            {
                var requestDelete = new MessageBoxRequest
                {
                    Title = CMess.conDelete.ToText(),
                    IconType = CMSG.MessageBoxIconType.Question,
                    Message = String.Format(CMess.confirmDelete.ToText(), CMess.SelectCards.ToText()),
                    Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                    ResponseSource = new TaskCompletionSource<int>()
                };
                OnMessageBoxRequested(requestDelete);
                int result = await requestDelete.ResponseTask;
                if (result != 0) return;
            }

            ulong? RareCardIdWillDelete = null;
            if (SelectedRareCard != null)
            {
                if (IsUserChangeCardID)
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.conDelete.ToText(),
                        IconType = CMSG.MessageBoxIconType.Question,
                        Message = CMess.quesSelectDelete.ToText(),
                        Buttons = new[] { CMess.SelectCards.ToText(), CMess.newIDCard.ToText(), CMess.cancel.ToText() },
                        ResponseSource = new TaskCompletionSource<int>()
                    };
                    OnMessageBoxRequested(request);
                    int resultChoose = await request.ResponseTask;

                    if (resultChoose == 0)
                    {
                        RareCardIdWillDelete = SelectedRareCard.id;
                    }
                    else if (resultChoose == 1)
                    {
                        RareCardIdWillDelete = RareCardId.Value;
                    }
                    else RareCardIdWillDelete = null;
                }
                else RareCardIdWillDelete = SelectedRareCard.id;
            }
            else RareCardIdWillDelete = RareCardId.Value;

            if (RareCardIdWillDelete == null) return;

            await DeleteRareCard(RareCardIdWillDelete.Value);
        }
        private async Task DeleteRareCard(ulong idCard)
        {
            try
            {
                var (resultDB, messageDB) = await RareRawDataViewModel.Instance.DeleteRareCardDatabase(idCard);
                if (resultDB)
                {
                    bool resultMemory = RareRawDataViewModel.Instance.RemoveRareCardById(idCard);
                    if (resultMemory)
                    {
                        DeleteRareCardView(idCard);
                        // RareCardsView.Refresh();
                        OnSnackbarRequested(string.Format(CMess.ThreePlaceholderSuccess.ToText(), 1.ToString(), CMess.Card.ToText(), CMess.tlDelete.ToText()));
                    }
                }
                else
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.error.ToText(),
                        IconType = CMSG.MessageBoxIconType.Error,
                        Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Card.ToText())} {messageDB}",
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(request);
                }
            }
            catch (Exception ex)
            {
                // CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"Error deleting RareCards: {ex.Message}", new[] { CMess.ok.ToText() });
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Card.ToText())} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        private void DeleteRareCardView(ulong idCard)
        {
            var existing = RareCards.FirstOrDefault(c => c.id == idCard);
            if (existing != null)
            {
                RareCards.Remove(existing);
            }
        }
        #endregion

        #region Delete Multiple RareCard
        private async Task DeleteMultipleRareCardCommand(int scope)
        {
            if (scope < 0 || scope > 2) return;

            string scopeText = scope == 0 ? CMess.SelectCards.ToText() :
                               scope == 1 ? CMess.FoundCards.ToText() :
                               CMess.AllCards.ToText();

            if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmDelete)
            {
                var requestDelete = new MessageBoxRequest
                {
                    Title = CMess.conDelete.ToText(),
                    IconType = CMSG.MessageBoxIconType.Question,
                    Message = String.Format(CMess.confirmDelete.ToText(), scopeText),
                    Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                    ResponseSource = new TaskCompletionSource<int>()
                };
                OnMessageBoxRequested(requestDelete);
                int result = await requestDelete.ResponseTask;
                if (result != 0) return;
            }

            if (scope == 0)
            {
                if (SelectedRareCards == null || !SelectedRareCards.Any())
                {
                    var requestNoSelect = new MessageBoxRequest
                    {
                        Title = CMess.warning.ToText(),
                        IconType = CMSG.MessageBoxIconType.Warning,
                        Message = CMess.noCardSelec.ToText(),
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(requestNoSelect);
                    return;
                }

                var SelectedRareCardsToDelete = SelectedRareCards.ToList();
                await DeleteMultipleRareCard(SelectedRareCardsToDelete);
            }
            else if (scope == 1)
            {
                var foundCards = RareCardsView?.Cast<RareCard>();
                if (foundCards == null || !foundCards.Any())
                {
                    var requestNoSelect = new MessageBoxRequest
                    {
                        Title = CMess.warning.ToText(),
                        IconType = CMSG.MessageBoxIconType.Warning,
                        Message = CMess.noCardFound.ToText(),
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(requestNoSelect);
                    return;
                }
                var foundCardsToDelete = foundCards.ToList();
                await DeleteMultipleRareCard(foundCardsToDelete);
            }
            else if (scope == 2)
            {
                if (RareCards == null || !RareCards.Any())
                {
                    var requestNoSelect = new MessageBoxRequest
                    {
                        Title = CMess.warning.ToText(),
                        IconType = CMSG.MessageBoxIconType.Warning,
                        Message = CMess.noCardFound.ToText(),
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(requestNoSelect);
                    return;
                }
                await DeleteAllRareCard();
            }
            else
            {
                var requestNoSelect = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = CMSG.MessageBoxIconType.Warning,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText()),
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(requestNoSelect);
                return;
            }
        }

        private async Task DeleteMultipleRareCard(IEnumerable<RareCard> cardsToDelete)
        {
            try
            {
                var (resultDB, messageDB) = await RareRawDataViewModel.Instance.DeleteMultipleRareCardDatabase(cardsToDelete);
                if (resultDB)
                {
                    int resultMemory = RareRawDataViewModel.Instance.RemoveMultipleRareCards(cardsToDelete);
                    if (resultMemory > 0)
                    {
                        DeleteMultipleRareCardView(cardsToDelete);
                        RareCardsView.Refresh();
                        OnSnackbarRequested(string.Format(CMess.ThreePlaceholderSuccess.ToText(), resultMemory.ToString(), CMess.Card.ToText(),CMess.tlDelete.ToText()));
                    }
                }
                else
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.error.ToText(),
                        IconType = CMSG.MessageBoxIconType.Error,
                        Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Card.ToText())} {messageDB}",
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(request);
                }
            }
            catch (Exception ex)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Card.ToText())} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        private void DeleteMultipleRareCardView(IEnumerable<RareCard> cardsToDelete)
        {
            if (cardsToDelete == null || !cardsToDelete.Any()) return;
            RareCards.RemoveRange(cardsToDelete);
        }

        private async Task DeleteAllRareCard()
        {
            try
            {
                var (resultDB, messageDB) = await RareRawDataViewModel.Instance.DeleteAllRareCardsDatabase();
                if (resultDB)
                {
                    int resultMemory = RareRawDataViewModel.Instance.RemoveAllRareCards();
                    if (resultMemory > 0)
                    {
                        RareCards.Clear();
                        RareCardsView.Refresh();
                        OnSnackbarRequested(string.Format(CMess.ThreePlaceholderSuccess.ToText(), resultMemory.ToString(), CMess.Card.ToText(), CMess.tlDelete.ToText()));
                    }
                }
                else
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.error.ToText(),
                        IconType = CMSG.MessageBoxIconType.Error,
                        Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Card.ToText())} {messageDB}",
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(request);
                }
            }
            catch (Exception ex)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Card.ToText())} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        #endregion

        #region Can Delete Card
        private bool CanDeleteSingleRareCard()
        {
            return (SelectedRareCard != null || (RareCardId.HasValue && RareCardId.Value > 0 && RareCardId.Value <= uint.MaxValue));
        }
        private bool CanDeleteSelectedRareCard()
        {
            return SelectedRareCards != null && SelectedRareCards.Any();
        }
        private bool CanDeleteFoundRareCard()
        {
            return (RareCardsView != null && RareCardsView.Cast<RareCard>().Any());
        }
        private bool CanDeleteAllRareCard()
        {
            return (RareCards != null && RareCards.Count > 0);
        }
        #endregion

        private void ViewImage()
        {
            if (SelectedRareCard == null) return;

            if (!string.IsNullOrEmpty(ImagecardTooltip) && System.IO.File.Exists(ImagecardTooltip))
                ImageViewerRequested?.Invoke(this, ImagecardTooltip);
        }
        private void OpenFile()
        {
            if (SelectedRareCard == null) return;
            if (!string.IsNullOrEmpty(ImagecardTooltip) && System.IO.File.Exists(ImagecardTooltip))
                OpenFileRequested?.Invoke(this, ImagecardTooltip);
        }

        #region Web
        private async Task OpenKonamiDB()
        {
            if (SelectedRareCard == null) return;
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                await mainWindow.OpenKonamiDB(SelectedRareCard.id, SelectedRareCard.name);
            }
        }
        private async Task OpenYugipedia()
        {
            if (SelectedRareCard == null) return;
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                await mainWindow.OpenYugipedia(SelectedRareCard.id, SelectedRareCard.name);
            }
        }
        private async Task OpenYGOResources()
        {
            if (SelectedRareCard == null) return;
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                await mainWindow.OpenYGOResources(SelectedRareCard.id, SelectedRareCard.name);
            }
        }
        #endregion

        private bool SelectedCardExist()
        {
            return SelectedRareCard != null;
        }
        private async Task LoadCardImage(CancellationToken token)
        {
            try
            {
                if (SelectedRareCard?.id == null) return;

                string imagePath = await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    return FindIInfoService.FindImagePath(SelectedRareCard.id.ToString());
                }, token);
                token.ThrowIfCancellationRequested();

                bool isValidImage = !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath);

                if (isValidImage)
                {
                    byte[] imgBytes = await Task.Run(() =>
                    {
                        token.ThrowIfCancellationRequested();
                        return File.ReadAllBytes(imagePath);
                    }, token);

                    BitmapImage bitmap;
                    using (var ms = new MemoryStream(imgBytes))
                    {
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ImageCardSource = bitmap;
                        ImagecardTooltip = imagePath;
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
                else
                {
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ImageCardSource = ImageCacheService.Instance.Get(AppImage.Blank);
                        ImagecardTooltip = CMess.toolCardImg.ToText();
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("LoadCardImage was canceled.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LoadCardImage error: " + ex.Message);
            }
        }
        #endregion

        #region Rare List
        private void LoadRareListsLazy()
        {
            try
            {
                var rawData = RareRawDataViewModel.Instance.RareItemsData;
                if (RareItems == null) RareItems = new BulkObservableCollection<RareItem>();
                else RareItems.Clear();

                RareItems.AddRange(rawData.Values);
            }
            catch (Exception ex)
            {
                // Log hoặc raise error cho UI
                Debug.WriteLine($"Lỗi khi load RareLists: {ex.Message}");
            }
        }

        private void ClearImgPath()
        {
            RareImagePath = string.Empty;
        }
        private void BrowseImgPath()
        {
            string filePath = FileDiaLogHelper.OpenImage();

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                RareImagePath = filePath;
                IsUserChangeImagePath = true;
            }
        }
        public static string MakeValidFileName(string name)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            return string.Concat(name.Where(c => !invalidChars.Contains(c)));
        }

        public async Task ModifyRareItem()
        {
            try
            {
                if (IsUserChangeImagePath)
                {
                    var (resultCopy, messageCopy) = await CopyImage();

                    if(!resultCopy)
                    {
                        var request = new MessageBoxRequest
                        {
                            Title = CMess.error.ToText(),
                            IconType = CMSG.MessageBoxIconType.Error,
                            Message = $"{CMess.errorOcc.ToText()} {messageCopy}",
                            Buttons = new[] { CMess.ok.ToText() },
                            ResponseSource = null
                        };
                        OnMessageBoxRequested(request);
                        return;
                    }
                    RareImagePath = messageCopy;
                    IsUserChangeImagePath = false;
                }
                long rareCode = 1L << RareCode.Value;

                var newItem = new RareItem { IdRare = IdRare.Value, Name = MakeValidFileName(RareName), Code = rareCode, ImagePath = RareImagePath };

                var (resultDB, messageDB) = await RareRawDataViewModel.Instance.ModifyRareListDatabase(newItem);
                if (resultDB)
                {
                    var (resultMemory, isAdd) = RareRawDataViewModel.Instance.ModifyRareItem(newItem);
                    if (resultMemory)
                    {
                        ModifyRareItemView(newItem);
                        RareRawDataViewModel.Instance.RareListSyncToUI();
                        OnSnackbarRequested(string.Format(CMess.ThreePlaceholderSuccess.ToText(), newItem.Name, CMess.Rarity.ToText(), isAdd ? CMess.tlAdd.ToText() : CMess.Update.ToText()));
                    }
                }
                else
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.error.ToText(),
                        IconType = CMSG.MessageBoxIconType.Error,
                        Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.Rarity.ToText())} {messageDB}",
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(request);
                }
            }
            catch (Exception ex)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.Rarity.ToText())} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        private void ModifyRareItemView(RareItem newItem)
        {
            if (newItem == null) return;
            var existing = RareItems.FirstOrDefault(item => item.IdRare == newItem.IdRare);

            if (existing != null)
            {
                // Cập nhật thuộc tính nếu khác
                if (existing.Name != newItem.Name)
                    existing.Name = newItem.Name;
                if (existing.Code != newItem.Code)
                    existing.Code = newItem.Code;
                if (existing.ImagePath != newItem.ImagePath)
                    existing.ImagePath = newItem.ImagePath;
            }
            else
            {
                int insertIndex = RareItems.TakeWhile(item => item.IdRare < newItem.IdRare).Count();
                RareItems.Insert(insertIndex, newItem);
                SelectedRareItem = newItem;
                _rareItemsView.MoveCurrentTo(newItem);
                // RareItems.Add(newItem);
            }
        }
        private async Task<(bool,string)> CopyImage()
        {
            try
            {
                if (!Directory.Exists(CardAppContext.Instance.StampFolderPath))
                    Directory.CreateDirectory(CardAppContext.Instance.StampFolderPath);

                string safeRareName = MakeValidFileName(RareName);
                if (!System.IO.File.Exists(RareImagePath)) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText()));
                if (string.IsNullOrWhiteSpace(safeRareName)) return (false, string.Format(CMess.PlaceholderInva.ToText(), CMess.RarityName.ToText()));

                string destinationPath = System.IO.Path.Combine(CardAppContext.Instance.StampFolderPath, $"{safeRareName}.png");

                const int bufferSize = 81920;
                using (var sourceStream = new FileStream(RareImagePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true))
                using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true))
                    await sourceStream.CopyToAsync(destinationStream);

                // RareImagePath = destinationPath;
                return (true, destinationPath);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        private bool CanModifyRareItem()
        {
            return (IdRare.HasValue && IdRare.Value > 0 && !string.IsNullOrWhiteSpace(RareName) && RareCode.HasValue && RareCode.Value >= 0 && RareCode.Value <= 62);
        }
        private void ResetRareItem()
        {
            if (SelectedRareItem != null)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.questi.ToText(),
                    IconType = CMSG.MessageBoxIconType.Question,
                    Message = string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.tlReset.ToText(), CMess.SelectedRare.ToText()),
                    Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                    ResponseSource = new TaskCompletionSource<int>()
                };
                OnMessageBoxRequested(request);
                int result = request.ResponseTask.Result;
                if (result == 0) OnSelectedRareItemChanged();
            }
            else
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = CMSG.MessageBoxIconType.Warning,
                    Message = CMess.noRareSelec.ToText(),
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        private bool CanResetClearDeleteRareItem()
        {
            return SelectedRareItem != null;
        }
        private void ClearRareItem()
        {
            if (SelectedRareItem != null)
            {
                if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmClear)
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.conClear.ToText(),
                        IconType = CMSG.MessageBoxIconType.Question,
                        Message = string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.tlClear.ToText(), CMess.SelectedRare.ToText()),
                        Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                        ResponseSource = new TaskCompletionSource<int>()
                    };
                    OnMessageBoxRequested(request);
                    int result = request.ResponseTask.Result;
                    if (result != 0) return;
                }
                ClearAllRareItem();
            }
            else
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = CMSG.MessageBoxIconType.Warning,
                    Message = CMess.noRareSelec.ToText(),
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        private void ClearAllRareItem()
        {
            IdRare = null;
            RareName = string.Empty;
            RareCode = null;
            RareImagePath = string.Empty;
        }

        private async Task DeleteRareItemCommand()
        {
            if (SelectedRareItem != null)
            {
                if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmDelete)
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.conDelete.ToText(),
                        IconType = CMSG.MessageBoxIconType.Question,
                        Message = string.Format(CMess.confirmDelete.ToText(), CMess.SelectedRare.ToText()),
                        Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                        ResponseSource = new TaskCompletionSource<int>()
                    };
                    OnMessageBoxRequested(request);

                    int result = await request.ResponseTask;
                    if (result != 0) return;
                }
                await DeleteRareItem(SelectedRareItem.IdRare);
                RareItemsView.Refresh();
            }
            else
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = CMSG.MessageBoxIconType.Warning,
                    Message = CMess.noRareSelec.ToText(),
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        public async Task DeleteRareItem(int idRare)
        {
            try
            {
                var (resultDB, messageDB) = await RareRawDataViewModel.Instance.DeleteRareListDatabase(idRare);
                if (resultDB)
                {
                    bool resultMemory = RareRawDataViewModel.Instance.RemoveRareItemById(idRare);
                    if (resultMemory)
                    {
                        RareItemsView.Refresh();
                        RareRawDataViewModel.Instance.RareListSyncToUI();
                        OnSnackbarRequested(string.Format(CMess.ThreePlaceholderSuccess.ToText(), 1.ToString(), CMess.Rarity.ToText(), CMess.tlDelete.ToText()));
                    }
                }
                else
                {
                    var request = new MessageBoxRequest
                    {
                        Title = CMess.error.ToText(),
                        IconType = CMSG.MessageBoxIconType.Error,
                        Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Rarity.ToText())} {messageDB}",
                        Buttons = new[] { CMess.ok.ToText() },
                        ResponseSource = null
                    };
                    OnMessageBoxRequested(request);
                }
                IsUserChangeImagePath = false;
            }
            catch (Exception ex)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Rarity.ToText())} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        #endregion

        #region Rolder
        private void ClearOriginalPath()
        {
            imageSetting.OriginalCardFolder = string.Empty;
        }
        private bool CanClearOriginalPath()
        {
            return !string.IsNullOrWhiteSpace(imageSetting.OriginalCardFolder);
        }
        private void BrowseOriginalPath()
        {
            string folderPath = FileDiaLogHelper.OpenFolder(CMess.OriginalFolder.ToText());
            if (!string.IsNullOrWhiteSpace(folderPath) && Directory.Exists(folderPath))
            {
                imageSetting.OriginalCardFolder = folderPath;
            }
        }
        private void ClearOutputPath()
        {
            imageSetting.OutPutFolder = string.Empty;
        }
        private bool CanClearOutputPath()
        {
            return !string.IsNullOrWhiteSpace(imageSetting.OutPutFolder);
        }
        private void BrowseOutputPath()
        {
            string folderPath = FileDiaLogHelper.OpenFolder(CMess.OutPutFolder.ToText());
            if (!string.IsNullOrWhiteSpace(folderPath) && Directory.Exists(folderPath))
            {
                imageSetting.OutPutFolder = folderPath;
            }
        }

        private async Task SaveFolderPath()
        {
            try
            {
                ConfigViewModel.Instance.dataHandlingSetting = dataHandlingSetting.Clone();
                ConfigViewModel.Instance.imageSetting = imageSetting.Clone();

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

        private bool CanSaveFolderPath()
        {
            Debug.WriteLine("CanSaveSettingCommand called");


            if (string.IsNullOrWhiteSpace(imageSetting.OutPutFolder) ||
                imageSetting.OutPutFolder.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0 ||
                !System.IO.Path.IsPathRooted(imageSetting.OutPutFolder) ||
                !Directory.Exists(imageSetting.OutPutFolder))
                return false;

            if (imageSetting.StampPosition < 0 ||
                string.IsNullOrEmpty(imageSetting.BackgroundArt) ||
                string.IsNullOrEmpty(imageSetting.Foild) ||
                imageSetting.Secret < 0 || imageSetting.Secret > 3)
                return false;

            return true;
        }
        private static readonly Regex IntPairRegex = new Regex(@"^\d+,\d+$", RegexOptions.Compiled);
        private bool IsValidIntPair(string input) => !string.IsNullOrWhiteSpace(input) && IntPairRegex.IsMatch(input);
        #endregion

        #region Event
        private void DataHandlingSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ClearOriginalPathCommand?.RaiseCanExecuteChanged();
            ClearOutputPathCommand?.RaiseCanExecuteChanged();
            BrowseOriginalPathCommand?.RaiseCanExecuteChanged();
            BrowseOutputPathCommand?.RaiseCanExecuteChanged();
            ReloadFolderCommand?.RaiseCanExecuteChanged();
            SaveFolderCommand?.RaiseCanExecuteChanged();
        }
        private void ImageSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ClearOriginalPathCommand?.RaiseCanExecuteChanged();
            ClearOutputPathCommand?.RaiseCanExecuteChanged();
            BrowseOriginalPathCommand?.RaiseCanExecuteChanged();
            BrowseOutputPathCommand?.RaiseCanExecuteChanged();
            ReloadFolderCommand?.RaiseCanExecuteChanged();
            SaveFolderCommand?.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<MessageBoxRequest> MessageBoxRequested;
        public event EventHandler<string> SnackbarRequested;
        public event EventHandler<string> ImageViewerRequested;
        public event EventHandler<string> OpenFileRequested;
        public event Action OnSettingSaved;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnMessageBoxRequested(MessageBoxRequest request)
        {
            MessageBoxRequested?.Invoke(this, request);
        }
        protected void OnSnackbarRequested(string message)
        {
            SnackbarRequested?.Invoke(this, message);
        }
        protected void OnImageViewerRequested(string imageUrl)
        {
            ImageViewerRequested?.Invoke(this, imageUrl);
        }
        protected void OnOpenFileRequested(string filePath)
        {
            OpenFileRequested?.Invoke(this, filePath);
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            _findImgCardCTS?.Cancel();
            _findImgCardCTS?.Dispose();
            _findImgCardCTS = null;

            _createImgCardCTS?.Cancel();
            _createImgCardCTS?.Dispose();
            _createImgCardCTS = null;

            _loadRareCardCTS?.Cancel();
            _loadRareCardCTS?.Dispose();
            _loadRareCardCTS = null;

            IsSynSelectedRareItems = false;
            IsSynSelectedRareCards = false;
            IsSynSelectedComboRareItems = false;
            // IsSynSelectedRareItem = false;
            // IsSynSelectedRareCard = false;
            IsSynRareCardRare = false;
            IsUserChangeCardID = false;
            IsUserChangeImagePath = false;
            IsUserChangeFolderpath = false;

            RareItems?.Clear();
            RareItems = null;
            RareCards?.Clear();
            RareCards = null;
            SelectedRareItems?.Clear();
            SelectedRareItems = null;
            SelectedRareCards?.Clear();
            SelectedRareCards = null;
            SelectedComboRareItems?.Clear();
            SelectedComboRareItems = null;

            if (RareCardsView != null)
            {
                RareCardsView.Filter = null;
                RareCardsView.SortDescriptions.Clear();
                RareCardsView.GroupDescriptions.Clear();
            }
            if (RareItemsView != null)
            {
                RareItemsView.Filter = null;
                RareItemsView.SortDescriptions.Clear();
                RareItemsView.GroupDescriptions.Clear();
            }

            if (_selectedRareItems != null)
                _selectedRareItems.CollectionChanged -= SelectedRareItems_CollectionChanged;
            if (_selectedRareCards != null)
                _selectedRareCards.CollectionChanged -= SelectedRareCards_CollectionChanged;
            if (_selectedComboRareItems != null)
                _selectedComboRareItems.CollectionChanged -= SelectedComboRareItems_CollectionChanged;

            SelectedRareItem = null;
            SelectedRareCard = null;

            FilterCardCommand = null;
            UnFilterCardCommand = null;
            CreateImageCommand = null;
            CancelCreateImageCommand = null;
            LoadCardCommand = null;
            BrowseCardListDBCommand = null;
            BrowseRarityJSONFileCommand = null;
            AddCardCommand = null;
            ModifyCardCommand = null;
            SaveAllCommand = null;
            SortCardCommand = null;
            ResetCardCommand = null;
            ClearCardCommand = null;
            DeleteSingleCardCommand = null;
            DeleteSelectedCardCommand = null;
            DeleteFoundCardCommand = null;
            DeleteAllCardCommand = null;
            ViewImageCommand = null;
            OpenFileCommand = null;
            ClearImgPathCommand = null;
            BrowseCommand = null;
            AddRareCommand = null;
            ModifyRareCommand = null;
            ResetRareCommand = null;
            ClearRareCommand = null;
            DeleteRareCommand = null;
            ClearOriginalPathCommand = null;
            BrowseOriginalPathCommand = null;
            ClearOutputPathCommand = null;
            BrowseOutputPathCommand = null;
            ReloadFolderCommand = null;
            SaveFolderCommand = null;

            RareRawDataViewModel.Instance.OnDataChanged -= HandleDataChanged;
            RareRawDataViewModel.Instance.OnErrorOccurred -= HandleError;
        }
        #endregion
    }
}
