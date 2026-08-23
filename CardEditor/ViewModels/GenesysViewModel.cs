using System;
using System.IO;
using System.Web.UI.WebControls;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Commands;
using CardEditor.Services;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.ViewModels
{
    public class GenesysViewModel : INotifyPropertyChanged, IDisposable
    {
        public static GenesysViewModel CreateInstance() => new GenesysViewModel();

        #region Fields
        // private bool IsSynSelectedGenesysCard = false;
        private bool IsSynSelectedGenesysCards = false;
        private bool IsUserChangeCardID = false;

        private CancellationTokenSource _findImgCardCTS;
        private CancellationTokenSource _loadGenesysCardCTS;
        #endregion

        #region ObservableCollection
        private BulkObservableCollection<GenesysCard> _genesysCards;
        public BulkObservableCollection<GenesysCard> GenesysCards
        {
            get => _genesysCards;
            set
            {
                if (_genesysCards != value)
                {
                    _genesysCards = value;
                    OnPropertyChanged();
                    DeleteAllCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private BulkObservableCollection<GenesysCard> _selectedGenesysCards;
        public BulkObservableCollection<GenesysCard> SelectedGenesysCards
        {
            get => _selectedGenesysCards;
            set
            {
                if (_selectedGenesysCards != value)
                {
                    if (_selectedGenesysCards != null)
                    {
                        _selectedGenesysCards.CollectionChanged -= SelectedGenesysCards_CollectionChanged;
                    }
                    _selectedGenesysCards = value;
                    if (_selectedGenesysCards != null)
                    {
                        _selectedGenesysCards.CollectionChanged += SelectedGenesysCards_CollectionChanged;
                    }
                    OnPropertyChanged();
                    OnSelectedGenesysCardsChanged();
                    DeleteSelectedCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion

        #region CollectionView
        private ListCollectionView _genesysCardsView;
        public ICollectionView GenesysCardsView => _genesysCardsView;
        #endregion

        #region Property
        private GenesysCard _selectedGenesysCard;
        public GenesysCard SelectedGenesysCard
        {
            get => _selectedGenesysCard;
            set
            {
                if (_selectedGenesysCard != value)
                {
                    _selectedGenesysCard = value;
                    OnPropertyChanged();
                    OnSelectedGenesysCardChanged();
                    ResetCardCommand?.RaiseCanExecuteChanged();
                    ClearCardCommand?.RaiseCanExecuteChanged();
                    DeleteSingleCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private ImageSource _imagecardSource;
        public ImageSource ImageCardSource
        {
            get => _imagecardSource;
            set
            {
                if (_imagecardSource != value)
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

        private string _genesysCardName;
        public string GenesysCardName
        {
            get => _genesysCardName;
            set
            {
                if (_genesysCardName != value)
                {
                    _genesysCardName = value;
                    OnPropertyChanged();
                    AddCardCommand?.RaiseCanExecuteChanged();
                    ModifyCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private ulong? _genesysCardID;
        public ulong? GenesysCardID
        {
            get => _genesysCardID;
            set
            {
                if (_genesysCardID != value)
                {
                    _genesysCardID = value;
                    OnPropertyChanged();
                    AddCardCommand?.RaiseCanExecuteChanged();
                    ModifyCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private int? _genesysCardPoint;
        public int? GenesysCardPoint
        {
            get => _genesysCardPoint;
            set
            {
                if (_genesysCardPoint != value)
                {
                    _genesysCardPoint = value;
                    OnPropertyChanged();
                    AddCardCommand?.RaiseCanExecuteChanged();
                    ModifyCardCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        private Visibility _cancelLoadGenesysCard;
        public Visibility CancelLoadGenesysCard
        {
            get => _cancelLoadGenesysCard;
            set
            {
                if (_cancelLoadGenesysCard != value)
                {
                    _cancelLoadGenesysCard = value;
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
                    BrowseDBCommand?.RaiseCanExecuteChanged();
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

        #endregion

        #region Commands
        public RelayCommand FilterCardCommand { get; private set; }
        public RelayCommand UnFilterCardCommand { get; private set; }
        public RelayCommand LoadCardCommand { get; private set; }
        public RelayCommand CancelLoadCardCommand { get; private set; }
        public RelayCommand BrowseDBCommand { get; private set; }
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

        #region Constructor
        public GenesysViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject())) return;
            else
            {
                ImageCardSource = new BitmapImage(new Uri("pack://application:,,,/Images/Blank.png", UriKind.Absolute));
                ImagecardTooltip = CMess.toolCardImg.ToText();
            }

            InitializeCollections();
            InitializeCommands();
            SubscribeToDataChanges();

            CancelLoadGenesysCard = Visibility.Collapsed;
            IsSavedCardList = GenesysRawDataViewModel.Instance.IsSaveGenesysCardListToDB;
        }
        private void InitializeCollections()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                GenesysCards = new BulkObservableCollection<GenesysCard>();
                SelectedGenesysCards = new BulkObservableCollection<GenesysCard>();

                _genesysCardsView = (ListCollectionView)CollectionViewSource.GetDefaultView(GenesysCards);
                _genesysCardsView.IsLiveSorting = false;
                _genesysCardsView.IsLiveFiltering = false;
                _genesysCardsView.IsLiveGrouping = false;

                //int arrangeValue = ConfigViewModel.Instance.dataHandlingSetting.Arrange;
                //switch (arrangeValue)
                //{
                //    case 1: _genesysCardsView.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending)); break;
                //    case 2: _genesysCardsView.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending)); break;
                //    case 3: _genesysCardsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending)); break;
                //    case 4: _genesysCardsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Descending)); break;
                //    default: _genesysCardsView.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Ascending)); break;
                //}
            });
        }

        private void InitializeCommands()
        {
            FilterCardCommand = new CardEditor.Commands.RelayCommand(_ => FilterData(), _ => CanFilterSortData());
            UnFilterCardCommand = new CardEditor.Commands.RelayCommand(_ => UnFilterData());
            LoadCardCommand = new CardEditor.Commands.RelayCommand(async _ => await LoadGenesysCardDataCommand());
            CancelLoadCardCommand = new CardEditor.Commands.RelayCommand(_ => CancelLoadGenesysCardCommand());
            BrowseDBCommand = new CardEditor.Commands.RelayCommand(async _ => await BrowseData(), _ => IsLoadedCardList);
            AddCardCommand = new CardEditor.Commands.RelayCommand(async _ => await ModifyGenesysCard(), _ => CanModifyGenesysCard());
            ModifyCardCommand = new CardEditor.Commands.RelayCommand(async _ => await ModifyGenesysCard(), _ => CanModifyGenesysCard());
            SaveAllCommand = new CardEditor.Commands.RelayCommand(async _ => await SaveAllCard(), _ => !IsSavedCardList);
            SortCardCommand = new CardEditor.Commands.RelayCommand(_ => SortCard(), _ => CanFilterSortData());
            UnSortCardCommand = new CardEditor.Commands.RelayCommand(_ => UnSortCard());
            ResetCardCommand = new CardEditor.Commands.RelayCommand(_ => ResetGenesysCard(), _ => CanResetClearGenesysCard());
            ClearCardCommand = new CardEditor.Commands.RelayCommand(_ => ClearGenesysCardCommand(), _ => CanResetClearGenesysCard());
            DeleteSingleCardCommand = new CardEditor.Commands.RelayCommand(async _ => await DeleteSingleGenesysCardCommand(), _ => CanDeleteSingleGenesysCard());
            DeleteSelectedCardCommand = new CardEditor.Commands.RelayCommand(async _ => await DeleteMultipleGenesysCardCommand(0), _ => CanDeleteSelectedGenesysCard());
            DeleteFoundCardCommand = new CardEditor.Commands.RelayCommand(async _ => await DeleteMultipleGenesysCardCommand(1), _ => CanDeleteFoundGenesysCard());
            DeleteAllCardCommand = new CardEditor.Commands.RelayCommand(async _ => await DeleteMultipleGenesysCardCommand(2), _ => CanDeleteAllGenesysCard());
            ViewImageCommand = new CardEditor.Commands.RelayCommand(_ => ViewImage(), _ => SelectedCardExist());
            OpenFileCommand = new CardEditor.Commands.RelayCommand(_ => OpenFile(), _ => SelectedCardExist());
            OpenKonamiDBCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenKonamiDB(), _ => SelectedCardExist());
            OpenYugipediaCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenYugipedia(), _ => SelectedCardExist());
            OpenYGOResourcesCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenYGOResources(), _ => SelectedCardExist());
        }
        private void SubscribeToDataChanges()
        {
            GenesysRawDataViewModel.Instance.OnDataChanged += HandleDataChanged;
            GenesysRawDataViewModel.Instance.OnErrorOccurred += HandleError;
        }
        private void HandleDataChanged()
        {
            _ = LoadGenesysCardsOptimized();
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
        private async void OnSelectedGenesysCardChanged()
        {
            _findImgCardCTS?.Cancel();
            _findImgCardCTS = new CancellationTokenSource();
            var token = _findImgCardCTS.Token;

            try
            {
                if(SelectedGenesysCard != null)
                {
                    GenesysCardID = SelectedGenesysCard.Id;
                    GenesysCardName = SelectedGenesysCard.Name;
                    GenesysCardPoint = SelectedGenesysCard.GPoints;
                    await LoadCardImage(token);
                }
                else
                {
                    GenesysCardID = null;
                    GenesysCardName = string.Empty;
                    GenesysCardPoint = null;
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
        }
        private void SelectedGenesysCards_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (IsSynSelectedGenesysCards) return;
            try
            {
                IsSynSelectedGenesysCards = true;
                RecalculateSelectedGenesysCards();
            }
            catch
            {

            }
            finally
            {
                IsSynSelectedGenesysCards = false;
            }
        }
        private void RecalculateSelectedGenesysCards()
        {
            Debug.WriteLine($"SelectedRareCards changed: {SelectedGenesysCards.Count} items selected.");

            foreach (var item in SelectedGenesysCards)
            {
                Debug.WriteLine($"Selected: {item.Name} (id: {item.Id})");
            }
        }

        private void OnSelectedGenesysCardsChanged()
        {

        }

        #endregion

        #region Genesys Card
        public async Task LoadGenesysCardDataCommand()
        {
            if (!GenesysRawDataViewModel.Instance.IsLoadGenesysCardListFromDB)
            {
                await GenesysRawDataViewModel.Instance.LoadGenesysCardDataFromDatabase();
                await LoadGenesysCardsOptimized();
            }
            else
            {
                if (!IsLoadedCardList)
                {
                    await LoadGenesysCardsOptimized();
                }
                else
                {
                    if (!GenesysRawDataViewModel.Instance.IsSaveGenesysCardListToDB)
                    {
                        var request = new MessageBoxRequest
                        {
                            Title = CMess.questi.ToText(),
                            IconType = CMSG.MessageBoxIconType.Question,
                            Message = $"{CMess.HasUnSaveData.ToText()} {string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.tlReload.ToText(), CMess.GenesysDB.ToText())}",
                            Buttons = new[] { CMess.originaData.ToText(), CMess.unSavedData.ToText(), CMess.cancel.ToText() },
                            ResponseSource = new TaskCompletionSource<int>()
                        };
                        OnMessageBoxRequested(request);
                        int result = await request.ResponseTask;

                        if (result == 0)
                        {
                            await GenesysRawDataViewModel.Instance.LoadGenesysCardDataFromDatabase();
                            await LoadGenesysCardsOptimized();
                        }
                        else if (result == 1)
                        {
                            await LoadGenesysCardsOptimized();
                        }
                        else
                        {
                            ///
                        }
                    }
                }
            }
        }

        private async Task LoadGenesysCardsOptimized()
        {
            try
            {
                IsCardListLoading = true;
                CancelLoadGenesysCard = Visibility.Visible;

                _loadGenesysCardCTS?.Cancel();
                _loadGenesysCardCTS = new CancellationTokenSource();
                var cancellationToken = _loadGenesysCardCTS.Token;

                List<GenesysCard> rawData = null;

                await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    rawData = GenesysRawDataViewModel.Instance.GenesysCardsData.Values.ToList();
                }, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    GenesysCards?.Clear();
                    // GenesysCards = new BulkObservableCollection<GenesysCard>();

                    _genesysCardsView = (ListCollectionView)CollectionViewSource.GetDefaultView(GenesysCards);
                    _genesysCardsView.IsLiveFiltering = false;
                    _genesysCardsView.IsLiveSorting = false;
                    _genesysCardsView.IsLiveGrouping = false;

                    OnPropertyChanged(nameof(GenesysCardsView));

                }, DispatcherPriority.Normal);

                cancellationToken.ThrowIfCancellationRequested();

                await LoadDataInBatches(rawData, cancellationToken);

                //await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                //{
                //    _genesysCardsView.IsLiveFiltering = true;
                //    _genesysCardsView.IsLiveSorting = true;
                //    _genesysCardsView.LiveFilteringProperties.Add("Id");
                //    _genesysCardsView.LiveFilteringProperties.Add("Name");
                //    _genesysCardsView.LiveFilteringProperties.Add("GPoints");
                //}, DispatcherPriority.Background);
                IsLoadedCardList = true;
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
                IsSavedCardList = GenesysRawDataViewModel.Instance.IsSaveGenesysCardListToDB;
                CancelLoadGenesysCard = Visibility.Collapsed;
            }
        }
        private async Task LoadDataInBatches(List<GenesysCard> allData, CancellationToken cancellationToken)
        {
            const int BATCH_SIZE = 100;

            int total = allData.Count;
            var totalBatches = (int)Math.Ceiling((double)total / BATCH_SIZE);

            for (int batchIndex = 0; batchIndex < totalBatches; batchIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int start = batchIndex * BATCH_SIZE;
                int count = Math.Min(BATCH_SIZE, total - start);

                var batch = new List<GenesysCard>(count);
                for (int i = 0; i < count; i++)
                {
                    batch.Add(allData[start + i]);
                }

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    GenesysCards.AddRange(batch);
                }, DispatcherPriority.Background);

                if (batchIndex % 2 == 0)
                {
                    await Dispatcher.Yield(DispatcherPriority.Background);
                }

                var progress = (double)(batchIndex + 1) / totalBatches * 100;
                Debug.WriteLine($"Loading progress: {progress:F1}%");
            }
        }
        private void CancelLoadGenesysCardCommand()
        {
            _loadGenesysCardCTS?.Cancel();
        }

        private void FilterData()
        {
            if (GenesysCards == null || GenesysCardsView == null) return;

            int advancedSettings = ConfigViewModel.Instance.dataHandlingSetting.Advanced;
            bool isAdvancedFind = (advancedSettings & 0x01) == 0x01; // bit 1: Advanced Find
            bool matchCase = (advancedSettings & 0x02) == 0x02;     // bit 2: Match Case
            bool useWildcards = (advancedSettings & 0x04) == 0x04;  // bit 3: Use Wildcards
            bool matchPrefix = (advancedSettings & 0x08) == 0x08;   // bit 4: Match Prefix
            bool matchSuffix = (advancedSettings & 0x10) == 0x10;   // bit 5: Match Suffix
            bool wholeWords = (advancedSettings & 0x20) == 0x20;    // bit 6: Find Whole Words Only
            bool ignorePunctuation = (advancedSettings & 0x40) == 0x40; // bit 7: Ignore Punctuation
            bool ignoreWhitespace = (advancedSettings & 0x80) == 0x80;  // bit 8: Ignore White-Space

            var filterId = GenesysCardID;
            var fillerName = GenesysCardName;
            var fillerPoint = GenesysCardPoint;

            GenesysCardsView.Filter = item =>
            {
                var card = (CardEditor.Models.GenesysCard)item;

                bool matchesId = !filterId.HasValue || card.Id == filterId;
                bool matchesPoint = !fillerPoint.HasValue || fillerPoint == 0 || card.GPoints == fillerPoint;
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
                bool matchesName = MatchString(card.Name, fillerName);

                return matchesId && matchesName && matchesPoint;
            };
            var request = new MessageBoxRequest
            {
                Title = CMess.notifi.ToText(),
                IconType = CMSG.MessageBoxIconType.Notification,
                Message = string.Format(CMess.filterSuc.ToText(), GenesysCardsView.Cast<CardEditor.Models.GenesysCard>().Count(), GenesysCards.Count),
                Buttons = new[] { CMess.ok.ToText() },
                ResponseSource = null
            };
            OnMessageBoxRequested(request);
        }
        private void UnFilterData()
        {
            GenesysCardsView.Filter = null;
        }


        private bool CanFilterSortData()
        {
            return (GenesysCards != null &&  GenesysCards.Count > 2);
        }

        private async Task BrowseData()
        {
            string filePath = FileDiaLogHelper.OpenGenesys();

            if (!string.IsNullOrEmpty(filePath))
            {
                if (!System.IO.File.Exists(filePath)) return;

                bool Overwrite;
                if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 0)
                {
                    var requestDelete = new MessageBoxRequest
                    {
                        Title = CMess.conDelete.ToText(),
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
                        await GenesysRawDataViewModel.Instance.BrowseDataCardDataBase(filePath, Overwrite);
                        break;

                    case ".txt":
                        await GenesysRawDataViewModel.Instance.BrowseDataCardText(filePath, Overwrite);
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
                IsSavedCardList = GenesysRawDataViewModel.Instance.IsSaveGenesysCardListToDB;
            }
        }

        public async Task ModifyGenesysCard()
        {
            try
            {
                var newCard = new GenesysCard { Id = GenesysCardID.Value, Name = GenesysCardName, GPoints = GenesysCardPoint.Value };

                var (resultDB, messageDB) = await GenesysRawDataViewModel.Instance.ModifyGenesysCardDatabase(newCard);

                if (resultDB)
                {
                    var (resultMemory, isAdd) = GenesysRawDataViewModel.Instance.ModifyGenesysCard(newCard);
                    if (resultMemory)
                    {
                        ModifyGenesysCardView(newCard);
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
        private void ModifyGenesysCardView(GenesysCard newCard)
        {
            if (newCard == null) return;
            var existing = GenesysCards.FirstOrDefault(card => card.Id == newCard.Id);

            if (existing != null)
            {
                if (existing.Name != newCard.Name) existing.Name = newCard.Name;
                if (existing.GPoints != newCard.GPoints) existing.GPoints = newCard.GPoints;

                SelectedGenesysCard = existing;
            }
            else
            {
                int insertIndex = GenesysCards.TakeWhile(card => card.Id < newCard.Id).Count();
                GenesysCards.Insert(insertIndex, newCard);
                SelectedGenesysCard = newCard;
                _genesysCardsView.MoveCurrentTo(newCard);
            }
        }
        private bool CanModifyGenesysCard()
        {
            return (GenesysCardID.HasValue && GenesysCardID.Value > 0 && GenesysCardID.Value <= uint.MaxValue);
        }

        private async Task SaveAllCard()
        {
            try
            {
                var (result, message) = await GenesysRawDataViewModel.Instance.SaveAllGenesysCardDatabase();
                if (result)
                {
                    OnSnackbarRequested(string.Format(CMess.ThreePlaceholderSuccess.ToText(), CMess.Save.ToText(), message, CMess.Card.ToText()));
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
                IsSavedCardList = GenesysRawDataViewModel.Instance.IsSaveGenesysCardListToDB;
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
                IsSavedCardList = GenesysRawDataViewModel.Instance.IsSaveGenesysCardListToDB;
            }
        }

        private void SortCard()
        {
            var selectedSorts = SortsViewModel.Instance.SelectedSortItems;
            if (selectedSorts == null || selectedSorts.Count <= 0) return;

            try
            {
                using (GenesysCardsView.DeferRefresh())
                {
                    GenesysCardsView.SortDescriptions.Clear();
                    foreach (var sort in selectedSorts)
                    {
                        if (sort?.SelectedItem == null) continue;
                        if (string.IsNullOrEmpty(sort.SelectedItem.Name)) continue;

                        string cardPropertyName = sort.SelectedItem.Name switch
                        {
                            "id" => "Id",
                            "name" => "Name",
                            "GPoint" => "GPoints",
                            _ => null
                        };
                        if (string.IsNullOrEmpty(cardPropertyName)) continue;
                        var direction = sort.OrderByAsc ? ListSortDirection.Ascending : ListSortDirection.Descending;

                        GenesysCardsView.SortDescriptions.Add(new SortDescription(cardPropertyName, direction));
                    }
                }
            }
            catch (Exception ex)
            {
                var request = new MessageBoxRequest
                {
                    Title = CMess.error.ToText(),
                    IconType = CMSG.MessageBoxIconType.Error,
                    Message = $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Sort.ToText(), CMess.Genesys.ToText())} {ex.Message}",
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(request);
            }
        }
        private void UnSortCard()
        {
            if (GenesysCards == null || GenesysCardsView == null) return;
            GenesysCardsView.SortDescriptions.Clear();
            GenesysCardsView.Refresh();
        }

        private async void ResetGenesysCard()
        {
            if (SelectedGenesysCard != null)
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
                OnSelectedGenesysCardChanged();
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
        private async void ClearGenesysCardCommand()
        {
            if (SelectedGenesysCard != null)
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
                ClearAllGenesysCard();
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
        private void ClearAllGenesysCard()
        {
            GenesysCardID = null;
            GenesysCardName = string.Empty;
            GenesysCardPoint = null;
            ImageCardSource = new BitmapImage(new Uri("pack://application:,,,/Images/Blank.png", UriKind.Absolute));
            ImagecardTooltip = CMess.toolCardImg.ToText();
        }

        private bool CanResetClearGenesysCard()
        {
            return SelectedGenesysCard != null;
        }

        #region Delete Single Genesys Card
        private async Task DeleteSingleGenesysCardCommand()
        {
            if (SelectedGenesysCard == null && !GenesysCardID.HasValue)
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

                ulong? GenesysCardIdWillDelete = null;
                if (SelectedGenesysCard != null)
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
                            GenesysCardIdWillDelete = SelectedGenesysCard.Id;
                        }
                        else if (resultChoose == 1)
                        {
                            GenesysCardIdWillDelete = GenesysCardID.Value;
                        }
                        else GenesysCardIdWillDelete = null;
                    }
                    else GenesysCardIdWillDelete = SelectedGenesysCard.Id;
                }
                else GenesysCardIdWillDelete = GenesysCardID.Value;

                if (GenesysCardIdWillDelete == null) return;

                await DeleteGenesysCard(GenesysCardIdWillDelete.Value);
            }
        }
        public async Task DeleteGenesysCard(ulong idCard)
        {
            try
            {
                var (resultDB, messageDB) = await GenesysRawDataViewModel.Instance.DeleteGenesysCardDatabase(idCard);
                if (resultDB)
                {
                    bool resultMemory = GenesysRawDataViewModel.Instance.RemoveGenesysCardByID(idCard);
                    if (resultMemory)
                    {
                        DeleteGenesysCardView(idCard);
                        // GenesysCardsView.Refresh();
                        OnSnackbarRequested(string.Format(CMess.ThreePlaceholderSuccess.ToText(), CMess.tlDelete.ToText(), resultMemory.ToString(), CMess.Card.ToText()));
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
        private void DeleteGenesysCardView(ulong idCard)
        {
            var existing = GenesysCards.FirstOrDefault(c => c.Id == idCard);
            if (existing != null)
            {
                GenesysCards.Remove(existing);
            }
        }
        #endregion

        #region Delete Multiple GenesysCard
        private async Task DeleteMultipleGenesysCardCommand(int scope)
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
                    Message = string.Format(CMess.confirmDelete.ToText(), scopeText),
                    Buttons = new[] { CMess.yes.ToText(), CMess.no.ToText() },
                    ResponseSource = new TaskCompletionSource<int>()
                };
                OnMessageBoxRequested(requestDelete);
                int result = await requestDelete.ResponseTask;
                if (result != 0) return;
            }

            if (scope == 0)
            {
                if (SelectedGenesysCards == null || !SelectedGenesysCards.Any())
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
                var SelectedGenesysCardsToDelete = SelectedGenesysCards.ToList();
                await DeleteMultipleGenesysCard(SelectedGenesysCardsToDelete);
            }
            else if (scope == 1)
            {
                var foundCards = GenesysCardsView?.Cast<GenesysCard>();
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
                await DeleteMultipleGenesysCard(foundCardsToDelete);
            }
            else if (scope == 2)
            {
                if (GenesysCards == null || !GenesysCards.Any())
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
                await DeleteAllGenesysCard();
            }
            else
            {
                var requestNoSelect = new MessageBoxRequest
                {
                    Title = CMess.warning.ToText(),
                    IconType = CMSG.MessageBoxIconType.Warning,
                    Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.cardLabelScope.ToText()),
                    Buttons = new[] { CMess.ok.ToText() },
                    ResponseSource = null
                };
                OnMessageBoxRequested(requestNoSelect);
                return;
            }
        }
        private async Task DeleteMultipleGenesysCard(IEnumerable<GenesysCard> cardsToDelete)
        {
            try
            {
                var (resultDB, messageDB) = await GenesysRawDataViewModel.Instance.DeleteMultipleGenesysCardDatabase(cardsToDelete);
                if (resultDB)
                {
                    int resultMemory = GenesysRawDataViewModel.Instance.RemoveMultipleGenesysCards(cardsToDelete);
                    if (resultMemory > 0)
                    {
                        DeleteMultipleGenesysCardView(cardsToDelete);
                        GenesysCardsView.Refresh();
                        OnSnackbarRequested(string.Format(CMess.ThreePlaceholderSuccess.ToText(), CMess.tlDelete.ToText(), resultMemory.ToString(), CMess.Card.ToText()));
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
        private void DeleteMultipleGenesysCardView(IEnumerable<GenesysCard> cardsToDelete)
        {
            if (cardsToDelete == null || !cardsToDelete.Any()) return;
            GenesysCards.RemoveRange(cardsToDelete);
        }
        private async Task DeleteAllGenesysCard()
        {
            try
            {
                var (resultDB, messageDB) = await GenesysRawDataViewModel.Instance.DeleteAllGenesysCardsDatabase();
                if (resultDB)
                {
                    int resultMemory = GenesysRawDataViewModel.Instance.RemoveAllGenesysCards();
                    if (resultMemory > 0)
                    {
                        GenesysCards.Clear();
                        GenesysCardsView.Refresh();
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

        private bool CanDeleteSingleGenesysCard()
        {
            return (SelectedGenesysCard != null || (GenesysCardID.HasValue && GenesysCardID.Value > 0 && GenesysCardID.Value <= uint.MaxValue));
        }
        private bool CanDeleteSelectedGenesysCard()
        {
            return SelectedGenesysCards != null && SelectedGenesysCards.Any();
        }
        private bool CanDeleteFoundGenesysCard()
        {
            return (GenesysCardsView != null && GenesysCardsView.Cast<GenesysCard>().Any());
        }
        private bool CanDeleteAllGenesysCard()
        {
            return (GenesysCards != null && GenesysCards.Count > 0);
        }

        private async Task LoadCardImage(CancellationToken token)
        {
            try
            {
                if (SelectedGenesysCard?.Id == null) return;

                string imagePath = await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    return FindIInfoService.FindImagePath(SelectedGenesysCard.Id.ToString());
                }, token);
                token.ThrowIfCancellationRequested();

                bool isValidImage = !string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath);

                if (isValidImage)
                {
                    byte[] imgBytes = await Task.Run(() =>
                    {
                        token.ThrowIfCancellationRequested();
                        return System.IO.File.ReadAllBytes(imagePath);
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
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ImageCardSource = bitmap;
                        ImagecardTooltip = imagePath;
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
                else
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
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

        private void ViewImage()
        {
            if (SelectedGenesysCard == null) return;

            if (!string.IsNullOrEmpty(ImagecardTooltip) && System.IO.File.Exists(ImagecardTooltip))
                ImageViewerRequested?.Invoke(this, ImagecardTooltip);
        }
        private void OpenFile()
        {
            if (SelectedGenesysCard == null) return;
            if (!string.IsNullOrEmpty(ImagecardTooltip) && System.IO.File.Exists(ImagecardTooltip))
                OpenFileRequested?.Invoke(this, ImagecardTooltip);
        }

        #region Web
        private async Task OpenKonamiDB()
        {
            if (SelectedGenesysCard == null) return;
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                await mainWindow.OpenKonamiDB(SelectedGenesysCard.Id, SelectedGenesysCard.Name);
            }
        }
        private async Task OpenYugipedia()
        {
            if (SelectedGenesysCard == null) return;
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                await mainWindow.OpenYugipedia(SelectedGenesysCard.Id, SelectedGenesysCard.Name);
            }
        }
        private async Task OpenYGOResources()
        {
            if (SelectedGenesysCard == null) return;
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                await mainWindow.OpenYGOResources(SelectedGenesysCard.Id, SelectedGenesysCard.Name);
            }
        }
        #endregion

        private bool SelectedCardExist()
        {
            return SelectedGenesysCard != null;
        }
        #endregion

        #region Event
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<MessageBoxRequest> MessageBoxRequested;
        public event EventHandler<string> SnackbarRequested;
        public event EventHandler<string> ImageViewerRequested;
        public event EventHandler<string> OpenFileRequested;
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

            _loadGenesysCardCTS?.Cancel();
            _loadGenesysCardCTS?.Dispose();

            GenesysCards.Clear();
            GenesysCards = null;

            SelectedGenesysCards.Clear();
            SelectedGenesysCards = null;

            if (_genesysCardsView != null)
            {
                _genesysCardsView.Filter = null;
                _genesysCardsView.SortDescriptions.Clear();
                _genesysCardsView.GroupDescriptions.Clear();
                _genesysCardsView = null;
            }

            if (_selectedGenesysCards != null)
                _selectedGenesysCards.CollectionChanged -= SelectedGenesysCards_CollectionChanged;

            SelectedGenesysCard = null;
            ImageCardSource = null;
            ImagecardTooltip = null;
            GenesysCardName = null;
            GenesysCardID = null;
            GenesysCardPoint = null;

            GenesysRawDataViewModel.Instance.OnDataChanged -= HandleDataChanged;
            GenesysRawDataViewModel.Instance.OnErrorOccurred -= HandleError;
        }

        #endregion

    }
}
