using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using ICSharpCode.AvalonEdit.Document;
using OfficeOpenXml;
using MaterialDesignThemes.Wpf;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Manager;
using CardEditor.Commands;
using CardEditor.Services;
using CardEditor.ImageGene;
using CardEditor.ViewModels;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for DataEditor.xaml
    /// </summary>
    public partial class DataEditor : UserControl, INotifyPropertyChanged, IDisposable, ISaveable
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
                    if (MainWindowService == null) return;
                    MainWindowService.UpdateWindowTitle(MainWindowTitle);
                    string header = string.IsNullOrWhiteSpace(archiveFilePath)
                        ? System.IO.Path.GetFileName(MainWindowTitle)
                        : archiveEntryName;
                    MainWindowService.UpdateTabItemHeader(header);
                }
            }
        }
        private void RebuildWindowTitle()
        {
            string display = string.Empty;
            if (string.IsNullOrWhiteSpace(archiveFilePath))
            {
                if (!string.IsNullOrEmpty(cdbFilePath)) display = cdbFilePath;
                else if (!string.IsNullOrEmpty(xlsxFilePath)) display = xlsxFilePath;
                else if (!string.IsNullOrEmpty(cedsFilePath)) display = cedsFilePath;
            }
            else display = Path.Combine(archiveFilePath, archiveEntryName.Replace('/', Path.DirectorySeparatorChar));

            MainWindowTitle = display;
        }

        private bool _isSaved { get; set; } = true;
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

        private bool isChangedID = false;
        private bool isChangedGrid = false;
        private bool isDeleting = false;

        private string ImageUrl = string.Empty;

        public string cdbFilePath = string.Empty;
        public string cedsFilePath = string.Empty;
        public string xlsxFilePath = string.Empty;
        public string archiveFilePath;
        public string archiveEntryName;

        private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();
        private SnackbarMessageQueue MessageNotifi = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
        private SnackbarMessageQueue MessageDelete = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));
        private DateTime lastClickTime = DateTime.MinValue;
        private const double DOUBLE_CLICK_TIME = 500;
        private CancellationTokenSource _ctsCreateImg;

        public BulkObservableCollection<CardEditor.Models.Card> Cards { get; set; }
        public ICollectionView CollectionViewCollection { get; set; }
        private CardEditor.Models.Card _currentCard;
        public CardEditor.Models.Card CurrentCard
        {
            get => _currentCard;
            set
            {
                if (_currentCard != value)
                {
                    _currentCard = value;
                    OnPropertyChanged(nameof(CurrentCard));
                    CardCommandCanExecute();
                }
            }
        }
        private bool _isPendulumIntf = true;
        public bool IsPendulumIntf
        {
            get => _isPendulumIntf;
            set
            {
                if (_isPendulumIntf != value)
                {
                    _isPendulumIntf = value;
                    OnPropertyChanged(nameof(IsPendulumIntf));
                }
            }
        }
        private bool _isLinkIntf = true;
        public bool IsLinkIntf
        {
            get => _isLinkIntf;
            set
            {
                if (_isLinkIntf != value)
                {
                    _isLinkIntf = value;
                    OnPropertyChanged(nameof(IsLinkIntf));
                }
            }
        }
        private bool _isSkillIntf = false;
        public bool IsSkillIntf
        {
            get => _isSkillIntf;
            set
            {
                if (_isSkillIntf != value)
                {
                    _isSkillIntf = value;
                    OnPropertyChanged(nameof(IsSkillIntf));
                }
            }
        }

        private PendulumLanguageRule _selectedPenLangRule;
        public PendulumLanguageRule SelectedPenLangRule
        {
            get => _selectedPenLangRule;
            set
            {
                if (_selectedPenLangRule != value)
                {
                    _selectedPenLangRule = value;
                    OnPropertyChanged(nameof(SelectedPenLangRule));
                }
            }
        }

        private MaterialDesignThemes.Wpf.PackIconKind _copyCardDescIcon = PackIconKind.ContentCopy;
        public MaterialDesignThemes.Wpf.PackIconKind CopyCardDescIcon
        {
            get => _copyCardDescIcon;
            set
            {
                if (_copyCardDescIcon != value)
                {
                    _copyCardDescIcon = value;
                    OnPropertyChanged(nameof(CopyCardDescIcon));
                }
            }
        }
        #endregion

        #region Command
        public RelayCommand AddCardCommand { get; set; }
        public RelayCommand ModifyCardCommand { get; set; }
        public RelayCommand ScriptCommand { get; set; }
        public RelayCommand SortCardCommand { get; set; }
        public RelayCommand UndoCommand { get; set; }
        public RelayCommand ResetCardCommand { get; set; }
        public RelayCommand ClearCardCommand { get; set; }
        public RelayCommand DeleteCardCommand { get; set; }
        public RelayCommand FilterCardCommand { get; set; }
        public RelayCommand AdvancedFilterCommand { get; set; }
        public RelayCommand ClearFilterCommand { get; set; }
        public RelayCommand CreateImageCommand { get; set; }
        public RelayCommand SelectImageCommand { get; set; }
        public RelayCommand SelectedArtWorkCommand { get; set; }
        public RelayCommand ViewImageCommand { get; set; }
        public RelayCommand OpenFileImageCommand { get; set; }
        public RelayCommand OpenKonamiDBCommand { get; set; }
        public RelayCommand OpenYugipediaCommand { get; set; }
        public RelayCommand OpenYGOResourcesCommand { get; set; }

        public RelayCommand PreViewPendulumDescCommand { get; set; }
        public RelayCommand ApplyPendulumDescCommand { get; set; }
        public RelayCommand CopyCardDescCommand { get; set; }
        #endregion

        #region Constructor
        public DataEditor()
        {
            InitializeCommands();
            InitializeComponent();
            
            Cards = new BulkObservableCollection<CardEditor.Models.Card>();
            CollectionViewCollection = CollectionViewSource.GetDefaultView(Cards);

            datagrMain.ItemsSource = CollectionViewCollection;
            
            NotifiSnackbar.MessageQueue = MessageNotifi;
            DeleteSnackbar.MessageQueue = MessageDelete;

            Cards.CollectionChanged += Cards_CollectionChanged;

            this.DataContext = this;
        }
        public DataEditor(IMainWindowService service) : this()
        {
            MainWindowService = service;
        }
        private void Cards_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ListCommandCanExecute();
        }

        private void InitializeCommands()
        {
            AddCardCommand = new CardEditor.Commands.RelayCommand(async _ => await AddCard());
            ModifyCardCommand = new CardEditor.Commands.RelayCommand(async _ => await ModifyCardFunction(), _ => CurrentCardExists());
            ScriptCommand = new CardEditor.Commands.RelayCommand(async _ => await ScriptCard(), _ => CurrentCardExists());
            SortCardCommand = new CardEditor.Commands.RelayCommand(_ => SortCard(), _ => ListCardExists());
            UndoCommand = new CardEditor.Commands.RelayCommand(_ => UndoAction());
            ResetCardCommand = new CardEditor.Commands.RelayCommand(_ => ResetCurrentCard(), _ => CurrentCardExists());
            ClearCardCommand = new CardEditor.Commands.RelayCommand(_ => ClearCurrentCard(), _ => CurrentCardExists());
            DeleteCardCommand = new CardEditor.Commands.RelayCommand(async _ => await DeleteCurrentCard(), _ => CurrentCardExists());

            FilterCardCommand = new CardEditor.Commands.RelayCommand(_ => FilterCard(), _ => ListCardExists());
            AdvancedFilterCommand = new CardEditor.Commands.RelayCommand(_ => AdvanccedFilterCard());
            ClearFilterCommand = new CardEditor.Commands.RelayCommand(_ => ClearFilterCard());

            CreateImageCommand = new CardEditor.Commands.RelayCommand(async _ => await CreateImage(), _ => CurrentCardExists());
            SelectImageCommand = new CardEditor.Commands.RelayCommand(async _ => await SelectImage(), _ => CurrentCardExists());
            SelectedArtWorkCommand = new CardEditor.Commands.RelayCommand(async _ => await SelectArtwork(), _ => CurrentCardExists());
            ViewImageCommand = new CardEditor.Commands.RelayCommand(_ => ViewImage(), _ => CurrentCardExists());
            OpenFileImageCommand = new CardEditor.Commands.RelayCommand(_ => OpenFileImage(), _ => CurrentCardExists());

            OpenKonamiDBCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenKonamiDB(), _ => CurrentCardExists());
            OpenYugipediaCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenYugipedia(), _ => CurrentCardExists());
            OpenYGOResourcesCommand = new CardEditor.Commands.RelayCommand(async _ => await OpenYGOResources(), _ => CurrentCardExists());

            PreViewPendulumDescCommand = new CardEditor.Commands.RelayCommand(_ => OpenPreViewDescWindow(), _ => CurrentCardExists());
            ApplyPendulumDescCommand = new CardEditor.Commands.RelayCommand(_ => ApplyPendulumLanguageDesc(), _ => CurrentCardExists());
            CopyCardDescCommand = new CardEditor.Commands.RelayCommand(async _ => await CopyCardDescExecute());
        }
        private void CardCommandCanExecute()
        {
            AddCardCommand?.RaiseCanExecuteChanged();
            ModifyCardCommand?.RaiseCanExecuteChanged();
            ScriptCommand?.RaiseCanExecuteChanged();

            ResetCardCommand?.RaiseCanExecuteChanged();
            ClearCardCommand?.RaiseCanExecuteChanged();
            DeleteCardCommand?.RaiseCanExecuteChanged();

            CreateImageCommand?.RaiseCanExecuteChanged();
            SelectImageCommand?.RaiseCanExecuteChanged();
            SelectedArtWorkCommand?.RaiseCanExecuteChanged();
            ViewImageCommand?.RaiseCanExecuteChanged();
            OpenFileImageCommand?.RaiseCanExecuteChanged();
            OpenKonamiDBCommand?.RaiseCanExecuteChanged();
            OpenYugipediaCommand?.RaiseCanExecuteChanged();
            OpenYGOResourcesCommand?.RaiseCanExecuteChanged();
        }
        private void ListCommandCanExecute()
        {
            SortCardCommand?.RaiseCanExecuteChanged();
            FilterCardCommand?.RaiseCanExecuteChanged();
        }
        #endregion

        #region Load

        #region Load Card Element
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //IsPendulumIntf = true;
                //IsPendulumIntf = false;
                //IsLinkIntf = true;
                //IsLinkIntf = false;
                IsSkillIntf = true;
                IsSkillIntf = false;

                blArtworkFull.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                cvArtWorkNor.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                blArtworkNor.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                blArtworkPen.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                grChar.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                grRace.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                cmbcardchar?.ApplyTemplate();
                cmbcardrace?.ApplyTemplate();

                this.UpdateLayout();
            }), System.Windows.Threading.DispatcherPriority.Loaded);

            LoadConfig();

            InitializeContentMenu();

            if (MainWindowService == null) Debug.WriteLine("DataEditor.MainWindowService Null");
            else Debug.WriteLine("DataEditor.MainWindowService Not Null");
        }
        public void LoadDataViewMode()
        {
            #region Color
            string backgroundHex = ConfigViewModel.Instance.displaySetting.Background;
            string foregroundHex = ConfigViewModel.Instance.displaySetting.Foreground;
            Color backgroundColor = (Color)ColorConverter.ConvertFromString(backgroundHex);
            Color foregroundColor = (Color)ColorConverter.ConvertFromString(foregroundHex);
            Brush backgroundBrush = new SolidColorBrush(backgroundColor);
            Brush foregroundBrush = new SolidColorBrush(foregroundColor);
            #endregion

            #region Font
            string fontFamilyConfig = ConfigViewModel.Instance.displaySetting.FontFamily;
            FontFamily fontFamily = new FontFamily(fontFamilyConfig);

            int fontSizeSetting = ConfigViewModel.Instance.displaySetting.FontSize.Value;

            #endregion

            #region Theme
            string[] itemstheme = new string[] { "Amber", "Blue", "BlueGrey", "Brown", "Cyan", "DeepOrange", "DeepPurple", "Green", "Grey", "Indigo", "LightBlue", "LightGreen", "Lime", "Orange", "Pink", "Purple", "Red", "Teal", "Yellow" };
            string themeSet = ConfigViewModel.Instance.displaySetting.Theme;
            if (!Array.Exists(itemstheme, theme => theme.Equals(themeSet, StringComparison.OrdinalIgnoreCase)))
            {
                themeSet = "DeepPurple";
            }
            Uri themeUri = new Uri($"pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor.{themeSet}.xaml", UriKind.Absolute);
            ResourceDictionary resourceDict = new ResourceDictionary();
            resourceDict.Source = themeUri;
            this.Resources.MergedDictionaries.Add(resourceDict);
            #endregion
        }
        public void LoadConfig()
        {
            var selectedSorts = SortsViewModel.Instance.SelectedSortItems;
            if (selectedSorts != null && selectedSorts.Count > 0)
            {
                string toolTip = CMess.SortBy.ToText();

                foreach (var sort in selectedSorts)
                {
                    if (sort?.SelectedItem == null) continue;

                    string cardPropertyName = sort.SelectedItem.Name;
                    string direction = sort.OrderByAsc ? CMess.Ascending.ToText() : CMess.Descending.ToText();

                    toolTip += $"\n{cardPropertyName}: {direction}";
                }
                btnSort.ToolTip = toolTip;
            }
        }
        private void InitializeContentMenu()
        {
            ControlContextMenuService.Attach(txtcardname);
            ControlContextMenuService.Attach(txtcarddesc);
            ControlContextMenuService.Attach(txtstr1);
            ControlContextMenuService.Attach(txtstr2);
            ControlContextMenuService.Attach(txtstr3);
            ControlContextMenuService.Attach(txtstr4);
            ControlContextMenuService.Attach(txtstr5);
            ControlContextMenuService.Attach(txtstr6);
            ControlContextMenuService.Attach(txtstr7);
            ControlContextMenuService.Attach(txtstr8);
            ControlContextMenuService.Attach(txtstr9);
            ControlContextMenuService.Attach(txtstr10);
            ControlContextMenuService.Attach(txtstr11);
            ControlContextMenuService.Attach(txtstr12);
            ControlContextMenuService.Attach(txtstr13);
            ControlContextMenuService.Attach(txtstr14);
            ControlContextMenuService.Attach(txtstr15);
            ControlContextMenuService.Attach(txtstr16);

            ControlContextMenuService.Attach(txtLvRk);
            ControlContextMenuService.Attach(txtlinkrating);
            ControlContextMenuService.Attach(txtleftscale);
            ControlContextMenuService.Attach(txtrightscale);
            ControlContextMenuService.Attach(txtGenesysPoint);
            ControlContextMenuService.Attach(txtatk);
            ControlContextMenuService.Attach(txtdef);
            ControlContextMenuService.Attach(txtSupport);
            ControlContextMenuService.Attach(txtid);
            ControlContextMenuService.Attach(txtalias);

            ControlContextMenuService.Attach(txtimgWidth);
            ControlContextMenuService.Attach(txtimgHeight);
            ControlContextMenuService.Attach(txtAWFullOpaqueY);
            ControlContextMenuService.Attach(txtAWFullOpaqueHeight);
            ControlContextMenuService.Attach(txtAWFullTransparentY);
            ControlContextMenuService.Attach(txtAWFullTransparentHeight);
        }
        #endregion

        #region Load Database
        private void datagrMain_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.None;
            else e.Effects = DragDropEffects.Copy;
        }
        private async void datagrMain_Drop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0)
            {
                string filePath = files[0];
                if (CheckDatabase.IsDatabaseFile(filePath))
                {
                    if(System.IO.Path.GetExtension(filePath) == ".ceds")
                    {
                        var (CardList, message) = await LoadDataServices.LoadCedsCard(filePath);
                        if (CardList != null) ImportCardList(CardList);
                        else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                    }
                    else if(System.IO.Path.GetExtension(filePath) == ".xlsx")
                    {
                        var (CardList, message) = await LoadDataServices.LoadExcelCard(filePath);
                        if (CardList != null) ImportCardList(CardList);
                        else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                    }
                    else
                    {
                        await LoadDatabase(filePath);
                    }
                }
                else CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, CMess.notDatabase.ToText(), new[] { CMess.ok.ToText() });
            }
        }
        
        private void ImportCardList(IEnumerable<CardEditor.Models.Card> CardList)
        {
            try
            {
                if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 0) // Ask
                {
                    int resultImport = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question, CMess.confirmWriteData.ToText(),
                        new[] { CMess.OverwriteDupli.ToText(), CMess.Appendwrite.ToText(), CMess.CreateNew.ToText(), CMess.cancel.ToText() });

                    if (resultImport == 0) // Overwrite
                    {
                        ImportOverwrite(CardList);
                    }
                    else if (resultImport == 1) // Append
                    {
                        ImportAppendwrite(CardList);
                    }
                    else if (resultImport == 2) // Create New
                    {
                        if (MainWindowService != null)
                        {
                            MainWindowService.ImportDataCreateNewDataEdit(CardList);
                        }
                    }
                    else return;
                }
                else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 1) // Overwrite
                {
                    ImportOverwrite(CardList);
                }
                else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 2) // Append
                {
                    ImportAppendwrite(CardList);
                }
                else if (ConfigViewModel.Instance.dataHandlingSetting.WriteMode == 3) // Create New
                {
                    if (MainWindowService != null)
                    {
                        MainWindowService.ImportDataCreateNewDataEdit(CardList);
                    }
                }
                else return;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        public async Task LoadDatabase(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var (ListResult, messageResult) = await LoadDataServices.LoadDatabaseCard(filePath);
                Mouse.OverrideCursor = null;
                if (ListResult == null)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, messageResult, new[] { CMess.ok.ToText() });
                    return;
                }
                cdbFilePath = filePath;
                Cards.Clear();
                Cards.AddRange(ListResult);
                CollectionViewCollection.Refresh();
                RebuildWindowTitle();
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            finally
            {
                IsSaved = true;
            }
        }
        public async Task LoadDatabaseArchive(string cdbFilePath, string sourceArchivePath, string sourceEntryName)
        {
            if (string.IsNullOrWhiteSpace(cdbFilePath)) return;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var (ListResult, messageResult) = await LoadDataServices.LoadDatabaseCard(cdbFilePath);
                Mouse.OverrideCursor = null;
                if (ListResult == null)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, messageResult, new[] { CMess.ok.ToText() });
                    return;
                }
                Cards.Clear();
                Cards.AddRange(ListResult);
                CollectionViewCollection.Refresh();

                archiveFilePath = sourceArchivePath;
                archiveEntryName = sourceEntryName;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            finally
            {
                IsSaved = true;
            }
        }
        #endregion

        #region Load Card Into Form
        private void LoadCardData()
        {
            ClearAll();

            bool isMonster = FindIInfoService.CheckCardInfo(CurrentCard.type, CardType.Monster);
            bool isXyz = FindIInfoService.CheckCardInfo(CurrentCard.type, CardType.eXceed);
            bool isnonXyz = FindIInfoService.CheckCardInfo(CurrentCard.type, CardType.Fusion, CardType.Ritual, CardType.Synchro);
            bool isPendulum = FindIInfoService.CheckCardInfo(CurrentCard.type, CardType.Pendulum);
            bool isLink = FindIInfoService.CheckCardInfo(CurrentCard.type, CardType.Link);
            bool isSkill = FindIInfoService.CheckCardInfo(CurrentCard.type, CardType.Skill);

            #region Rule
            ulong rule = CurrentCard.ot;
            var selectedRules = CardDataViewModel.Instance.RuleItems.Where(item => (rule & item.RuleCode) != 0).ToList();
            cmbrule?.SelectedItems?.Clear();
            foreach (var ruleItem in selectedRules)
            {
                cmbrule?.SelectedItems?.Add(ruleItem);
            }
            #endregion

            #region Type
            ulong type = CurrentCard.type;
            var selectedTypes = CardDataViewModel.Instance.TypeItems.Where(item => (type & item.TypeCode) != 0).ToList();
            cmbcardtype?.SelectedItems?.Clear();
            foreach (var typeItem in selectedTypes)
            {
                cmbcardtype?.SelectedItems?.Add(typeItem);
            }
            #endregion

            #region Race
            ulong race = CurrentCard.race;
            var selectedRaces = CardDataViewModel.Instance.RaceItems.Where(item => (race & item.RaceCode) != 0).ToList();
            cmbcardrace?.SelectedItems?.Clear();
            foreach (var raceItem in selectedRaces)
            {
                cmbcardrace?.SelectedItems?.Add(raceItem);
            }
            #endregion

            #region Character
            var selectedChars = CardDataViewModel.Instance.CharItems.Where(item => (race & item.CharCode) != 0).ToList();
            cmbcardchar?.SelectedItems?.Clear();
            foreach (var charItem in selectedChars)
            {
                cmbcardchar?.SelectedItems?.Add(charItem);
            }
            #endregion

            #region Attribute
            ulong attribute = CurrentCard.attribute;
            var selectedAttributes = CardDataViewModel.Instance.AttributeItems.Where(item => (attribute & item.AttributeCode) != 0).ToList();
            cmbcardattribute?.SelectedItems?.Clear();
            foreach (var attributeItem in selectedAttributes)
            {
                cmbcardattribute?.SelectedItems?.Add(attributeItem);
            }
            #endregion

            #region SetCode
            ulong setcode = CurrentCard.setcode;
            HashSet<SetCodeItem> selectedItems = new HashSet<SetCodeItem>();

            for (int i = 0; i < 4; i++) // Vì tối đa có 4 setcode con
            {
                ushort subCode = (ushort)((setcode >> (i * 16)) & 0xFFFF);
                if (subCode == 0) continue;

                var setItem = CardDataViewModel.Instance.SetCodeItems.FirstOrDefault(set => set.SetCode == subCode);
                if (setItem != null)
                {
                    selectedItems.Add(setItem);
                }
            }
            cmbsetcode?.SelectedItems?.Clear();
            foreach (var setcodeitem in selectedItems)
            {
                cmbsetcode?.SelectedItems?.Add(setcodeitem);
            }
            #endregion

            #region Category
            ulong category = CurrentCard.category;
            var selectedCategory = CardDataViewModel.Instance.CategoryItems.Where(item => (category & item.CategoryCode) != 0).ToList();
            cmbcategory?.SelectedItems?.Clear();
            foreach (var categoryItem in selectedCategory)
            {
                cmbcategory?.SelectedItems?.Add(categoryItem);
            }
            #endregion

            #region Flag
            ulong flag = CurrentCard.flag;
            var selectedFlag = CardDataViewModel.Instance.FlagItems.Where(item => (flag & item.FlagCode) != 0).ToList();
            cmbflag?.SelectedItems?.Clear();
            foreach (var flagItem in selectedFlag)
            {
                cmbflag?.SelectedItems?.Add(flagItem);
            }
            #endregion

            #region Texts
            txtcardname.Text = CurrentCard.name;

            txtid.Text = CurrentCard.id.ToString();
            txtalias.Text = CurrentCard.alias.ToString();
            
            txtcarddesc.AppendText(CurrentCard.desc);
            txtcarddesc.ScrollToHome();
            #endregion

            #region Str
            txtstr1.Text = CurrentCard.str1;
            txtstr2.Text = CurrentCard.str2;
            txtstr3.Text = CurrentCard.str3;
            txtstr4.Text = CurrentCard.str4;
            txtstr5.Text = CurrentCard.str5;
            txtstr6.Text = CurrentCard.str6;
            txtstr7.Text = CurrentCard.str7;
            txtstr8.Text = CurrentCard.str8;
            txtstr9.Text = CurrentCard.str9;
            txtstr10.Text = CurrentCard.str10;
            txtstr11.Text = CurrentCard.str11;
            txtstr12.Text = CurrentCard.str12;
            txtstr13.Text = CurrentCard.str13;
            txtstr14.Text = CurrentCard.str14;
            txtstr15.Text = CurrentCard.str15;
            txtstr16.Text = CurrentCard.str16;
            #endregion

            #region Datas
            if (isLink)
            {
                var (DecoLinkarrow, DecoDef, notHasATK) = GetInfoService.DecodeDef(CurrentCard.def);
                txtatk.Text = notHasATK ? string.Empty : (CurrentCard.atk >= 0 ? CurrentCard.atk.ToString() : "?");
                txtdef.Text = DecoDef.HasValue ? (DecoDef >= 0 ? DecoDef.ToString() : "?") : string.Empty;

                txtLvRk.Text = ((CurrentCard.level >> 8) & 0xff).ToString();
                txtlinkrating.Text = (CurrentCard.level & 0xff).ToString();
                SetLinkArrow(true, DecoLinkarrow);
            }
            else
            {
                txtatk.Text = (CurrentCard.atk >= 0) ? CurrentCard.atk.ToString() : "?";
                txtdef.Text = (CurrentCard.def >= 0) ? CurrentCard.def.ToString() : "?";

                txtLvRk.Text = (CurrentCard.level & 0xff).ToString();
                SetLinkArrow(false);
            }
            txtrightscale.Text = ((CurrentCard.level >> 16) & 0xff).ToString();
            txtleftscale.Text = ((CurrentCard.level >> 24) & 0xff).ToString();

            //if (isXyz && !isnonXyz)
            //{
            //    imgLvRk.Source = new BitmapImage(new Uri("pack://application:,,,/Images/rankstart.ico"));
            //    txtLvRk.ToolTip = CMess.Rank.ToText();
            //}
            //else if (isXyz && isnonXyz)
            //{
            //    imgLvRk.Source = new BitmapImage(new Uri("pack://application:,,,/Images/levelrank.ico"));
            //    txtLvRk.ToolTip = $"{CMess.Level.ToText()}/{CMess.Rank.ToText()}";
            //}
            //else
            //{
            //    imgLvRk.Source = new BitmapImage(new Uri("pack://application:,,,/Images/levelstar.ico"));
            //    txtLvRk.ToolTip = CMess.Level.ToText();
            //}
            #endregion

        }
        private void LoadCardRare()
        {
            if (CurrentCard == null) return;
            long rare = RareRawDataViewModel.Instance.GetRareByCardId(CurrentCard.id);
            var selectedRare = RareRawDataViewModel.Instance.RareItemsForUI.Where(item => (rare & item.Code) != 0).ToList();
            cmbrarity.SelectedItems.Clear();
            foreach (var rareItem in selectedRare)
            {
                cmbrarity.SelectedItems.Add(rareItem);
            }
        }
        private void LoadGenesysPoint()
        {
            if (CurrentCard == null)  return;

            int genesysPoint = GenesysRawDataViewModel.Instance.GetGenesysPointByID(CurrentCard.id);
            txtGenesysPoint.Text = genesysPoint.ToString();
        }
        private async Task LoadCardImage()
        {
            try
            {
                string folderPath = string.Empty;
                if (!string.IsNullOrWhiteSpace(cdbFilePath) && System.IO.File.Exists(cdbFilePath))
                {
                    folderPath = System.IO.Path.GetDirectoryName(cdbFilePath);
                }
                else if (!string.IsNullOrWhiteSpace(cedsFilePath) && System.IO.File.Exists(cedsFilePath))
                {
                    folderPath = System.IO.Path.GetDirectoryName(cedsFilePath);
                }
                else if (!string.IsNullOrWhiteSpace(xlsxFilePath) && System.IO.File.Exists(xlsxFilePath))
                {
                    folderPath = System.IO.Path.GetDirectoryName(xlsxFilePath);
                }
                else
                {
                    ///
                }

                string imagePath = await Task.Run(() => FindIInfoService.FindImagePath(CurrentCard.id.ToString(), folderPath));
                BitmapImage bitmap;
                if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                {
                    byte[] imgBytes = await Task.Run(() => System.IO.File.ReadAllBytes(imagePath));
                    bitmap = new BitmapImage();
                    using (var ms = new MemoryStream(imgBytes))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                    ImageUrl = imagePath;

                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        imagecard.Source = bitmap;
                        ViewportImage.ToolTip = ImageUrl ?? CMess.toolCardImg.ToText();
                    });
                }
                else SetImageCardDefault();
            }
            catch
            {
                ///
            }
        }
        private void SetLinkArrow(bool islink, long DecoLinkarrow = 0)
        {
            if (islink)
            {
                // var (topleft, top, topright, left, right, botleft, bot, botright) = FindIInfoService.GetLinkMarker(_currentcard.def & 0x1FF);
                var (topleft, top, topright, left, right, botleft, bot, botright) = FindIInfoService.GetLinkMarker(DecoLinkarrow);
                tbtntopleft.IsChecked = topleft;
                tbtntop.IsChecked = top;
                tbtntopright.IsChecked = topright;
                tbtnleft.IsChecked = left;
                tbtnright.IsChecked = right;
                tbtnbotleft.IsChecked = botleft;
                tbtnbot.IsChecked = bot;
                tbtnbotright.IsChecked = botright;
            }
            else
            {
                tbtntopleft.IsChecked = false;
                tbtntop.IsChecked = false;
                tbtntopright.IsChecked = false;
                tbtnleft.IsChecked = false;
                tbtnright.IsChecked = false;
                tbtnbotleft.IsChecked = false;
                tbtnbot.IsChecked = false;
                tbtnbotright.IsChecked = false;
            }
        }
        #endregion

        #endregion

        #region Get Data On Form
        private ulong GetCardID()
        {
            if (string.IsNullOrWhiteSpace(txtid.Text))
                throw new FormatException(string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardID.ToText()));
            if (!ulong.TryParse(txtid.Text, out ulong id))
                throw new FormatException(string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardID.ToText()));
            if (id > uint.MaxValue || id <= 0)
                throw new ArgumentOutOfRangeException($"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardID.ToText())} {CMess.cardID.ToText()} {CMess.outofrange.ToText()}");
            return id;
        }
        private ulong? GetCardIDNull()
        {
            if (string.IsNullOrWhiteSpace(txtid.Text))
                return null;
            if (!ulong.TryParse(txtid.Text, out ulong id))
                return null;
            return id;
        }
        private string GetCardname()
        {
            return string.IsNullOrWhiteSpace(txtcardname.Text) ? "" : txtcardname.Text.Trim();
        }
        private string GetCardDesc()
        {
            // TextRange description = new TextRange(txtcarddesc.Document.ContentStart, txtcarddesc.Document.ContentEnd);
            // string text = description.Text?.Trim();
            string text = txtcarddesc.Text?.Trim();
            return string.IsNullOrEmpty(text) ? "" : text;
        }
        private string GetSTR1()
        {
            return string.IsNullOrWhiteSpace(txtstr1.Text) ? "" : txtstr1.Text.Trim();
        }
        private string GetSTR2()
        {
            return string.IsNullOrWhiteSpace(txtstr2.Text) ? "" : txtstr2.Text.Trim();
        }
        private string GetSTR3()
        {
            return string.IsNullOrWhiteSpace(txtstr3.Text) ? "" : txtstr3.Text.Trim();
        }
        private string GetSTR4()
        {
            return string.IsNullOrWhiteSpace(txtstr4.Text) ? "" : txtstr4.Text.Trim();
        }
        private string GetSTR5()
        {
            return string.IsNullOrWhiteSpace(txtstr5.Text) ? "" : txtstr5.Text.Trim();
        }
        private string GetSTR6()
        {
            return string.IsNullOrWhiteSpace(txtstr6.Text) ? "" : txtstr6.Text.Trim();
        }
        private string GetSTR7()
        {
            return string.IsNullOrWhiteSpace(txtstr7.Text) ? "" : txtstr7.Text.Trim();
        }
        private string GetSTR8()
        {
            return string.IsNullOrWhiteSpace(txtstr8.Text) ? "" : txtstr8.Text.Trim();
        }
        private string GetSTR9()
        {
            return string.IsNullOrWhiteSpace(txtstr9.Text) ? "" : txtstr9.Text.Trim();
        }
        private string GetSTR10()
        {
            return string.IsNullOrWhiteSpace(txtstr10.Text) ? "" : txtstr10.Text.Trim();
        }
        private string GetSTR11()
        {
            return string.IsNullOrWhiteSpace(txtstr11.Text) ? "" : txtstr11.Text.Trim();
        }
        private string GetSTR12()
        {
            return string.IsNullOrWhiteSpace(txtstr12.Text) ? "" : txtstr12.Text.Trim();
        }
        private string GetSTR13()
        {
            return string.IsNullOrWhiteSpace(txtstr13.Text) ? "" : txtstr13.Text.Trim();
        }
        private string GetSTR14()
        {
            return string.IsNullOrWhiteSpace(txtstr14.Text) ? "" : txtstr14.Text.Trim();
        }
        private string GetSTR15()
        {
            return string.IsNullOrWhiteSpace(txtstr15.Text) ? "" : txtstr15.Text.Trim();
        }
        private string GetSTR16()
        {
            return string.IsNullOrWhiteSpace(txtstr16.Text) ? "" : txtstr16.Text.Trim();
        }

        private ulong GetCardRule()
        {
            ulong rule = 0;
            var selectedRules = cmbrule.SelectedItems?.Cast<RuleItem>().ToList();
            if (selectedRules != null && selectedRules.Any())
                foreach (var ruleItem in selectedRules)
                    rule |= ruleItem.RuleCode;
            return rule;
        }
        private ulong GetCardAlias()
        {
            var input = txtalias.Text?.Trim();
            if (string.IsNullOrWhiteSpace(input))
                return 0;
            if (!ulong.TryParse(input, out ulong alias))
                throw new FormatException(string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardAlias.ToText()));
            if (alias > uint.MaxValue || alias < 0)
                throw new ArgumentOutOfRangeException($"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardAlias.ToText())} {CMess.cardAlias.ToText()} {CMess.outofrange.ToText()}");
            return alias;
        }
        private ulong? GetCardAliasNull()
        {
            if (string.IsNullOrWhiteSpace(txtalias.Text))
                return null;
            if (!ulong.TryParse(txtalias.Text, out ulong alias))
                return null;
            return alias;
        }
        private ulong GetSetCode()
        {
            ulong setcode = 0;
            var selectedSetCodeItems = cmbsetcode.SelectedItems?.Cast<SetCodeItem>().ToList();
            if (selectedSetCodeItems != null && selectedSetCodeItems.Any())
                for (int i = 0; i < selectedSetCodeItems.Count; i++)
                {
                    var setItem = selectedSetCodeItems[i];
                    setcode |= (ulong)setItem.SetCode << (i * 16);
                }
            return setcode;
        }
        private ulong GetCardType()
        {
            ulong type = 0;
            var selectedTypes = cmbcardtype.SelectedItems?.Cast<TypeItem>().ToList();
            if (selectedTypes != null && selectedTypes.Any())
                foreach (var typeItem in selectedTypes)
                {
                    type |= typeItem.TypeCode;
                }
            return type;
        }
        private long GetATK()
        {
            long atk;
            if (string.IsNullOrWhiteSpace(txtatk.Text)) atk = 0;
            else
            {
                if (long.TryParse(txtatk.Text, out atk))
                {
                    if (atk < 0) atk = -2;
                }
                else atk = -2;
            }
            return atk;
        }
        private long? GetATKNull()
        {
            long atk;
            if (string.IsNullOrWhiteSpace(txtatk.Text)) return null;
            else
            {
                if (long.TryParse(txtatk.Text, out atk))
                {
                    if (atk < 0) atk = -2;
                }
                else atk = -2;
            }
            return atk;
        }
        private long GetDEF()
        {
            if (IsLinkIntf)
            {
                bool notHasATK = string.IsNullOrWhiteSpace(txtatk.Text);
                long linkarrow = GetInfoService.GetLinkArrowValue(
                    tbtntopleft.IsChecked ?? false,
                    tbtntop.IsChecked ?? false,
                    tbtntopright.IsChecked ?? false,
                    tbtnleft.IsChecked ?? false,
                    tbtnright.IsChecked ?? false,
                    tbtnbotleft.IsChecked ?? false,
                    tbtnbot.IsChecked ?? false,
                    tbtnbotright.IsChecked ?? false);

                long? defLinkFromtext = null;
                if (string.IsNullOrWhiteSpace(txtdef.Text))
                    defLinkFromtext = null;
                else
                {
                    if (long.TryParse(txtdef.Text, out long deftext))
                    {
                        if (deftext < 0) defLinkFromtext = -2;
                        else defLinkFromtext = deftext;
                    }
                    else defLinkFromtext = -2;
                }
                
                return GetInfoService.GetDefValue(linkarrow, defLinkFromtext, notHasATK);
            }
            else
            {
                long defNLinkFromtext = 0;
                if (string.IsNullOrWhiteSpace(txtdef.Text)) defNLinkFromtext = 0;
                else
                {
                    if(long.TryParse(txtdef.Text, out defNLinkFromtext))
                    {
                        if (defNLinkFromtext < 0) defNLinkFromtext = -2;
                    }
                    else defNLinkFromtext = -2;
                }

                return defNLinkFromtext;
            }

            #region Note

            /* 
             * Đối với quái non-link:
             * Nếu txtdef bỏ trống hoặc chỉ chứa space: mặc định là 0
             * Nếu không thể chuyển đổi thành số, đặt là -2 (hiển thị ?)
             * Nếu có thể chuyển thành số nhỏ hơn 0, đặt là -2 (hiển thị ?)
             * Nếu có thể chuyển thành số lớn hơn hoặc bằng 0, đặt là def
             * 
             * Nếu là quái link:
             * Đặt link arrows là 9 bit từ 0->8
             * Đặt cờ notHasATK là bit số 4 (0: Có ATK, 1: Không có ATK)
             * Nếu txtdef bỏ trống hoặc chỉ chứa space: mặc định không có DEF
             * Nếu txtATK bỏ trống hoặc chỉ chứa space: mặc định không có ATK
             * Nếu không thể chuyển thành số, def = -2 (hiển thị ?)
             * Nếu có thể chuyển thành số nhỏ hơn 0, def = -2 (hiển thị ?)
             * Nếu lớn hơn 0, đặt là def
             * Ghép Linkarrow, DEF và notHasATK thành 32 bit
             * Bit 0->8: LinkArrow
             * Bit 4: Flag không có ATK 
             * Bit 9->30: def
             * Bit 30: Dấu của def
             * Bit 31: Flag tồn tại def
             */
            #endregion
        }
        private long? GetDEFNull()
        {
            long? defFromtext = null;
            bool notHasATK = string.IsNullOrWhiteSpace(txtatk.Text);

            if (string.IsNullOrWhiteSpace(txtdef.Text)) defFromtext = null;
            else
            {
                if (long.TryParse(txtdef.Text, out long deftext))
                {
                    if (deftext >= 0) defFromtext = deftext;
                    else defFromtext = -2; // Hiển thị ?
                }
                else defFromtext = -2;
            }

            if (IsLinkIntf)
            {
                long linkarrow = GetInfoService.GetLinkArrowValue(
                    tbtntopleft.IsChecked ?? false,
                    tbtntop.IsChecked ?? false,
                    tbtntopright.IsChecked ?? false,
                    tbtnleft.IsChecked ?? false,
                    tbtnright.IsChecked ?? false,
                    tbtnbotleft.IsChecked ?? false,
                    tbtnbot.IsChecked ?? false,
                    tbtnbotright.IsChecked ?? false);

                if (linkarrow == 0 && defFromtext == null) return null;
                else return GetInfoService.GetDefValue(linkarrow, defFromtext, notHasATK);
            }
            else return defFromtext;

            /* Nếu là quái non-link:
             * Nếu txtdef bỏ trống hoặc chỉ chứa space: mặc định là null
             * Nếu không thể chuyển đổi thành số, đặt là -2 (hiển thị ?)
             * Nếu có thể chuyển thành số nhỏ hơn 0, đặt là -2 (hiển thị ?)
             * Nếu có thể chuyển thành số lớn hơn hoặc bằng 0, đặt là def
             * 
             * Nếu là quái link:
             * Đặt link arrows là 9 bit từ 0->8
             * Đặt cờ notHasATK là bit số 4 (0: Có ATK, 1: Không có ATK)
             * Không có LinkArrow nào được chọn + deffromtext null -> return null
             * Có Link LinkArrow được chọn + deffromtext null - > return value
             * Không có LinkArrow nào được chọn + deffromtext value -> return value
             * Ghép linkarrow và def thành 32 bit value
             * Bit 0->8: LinkArrow
             * Bit 4: Flag không có ATK 
             * Bit 9->30: def
             * Bit 30: Flag dấu của def
             * Bit 31: Flag tồn tại def
             */
        }
        private ulong GetLevel()
        {
            ulong level;
            int cardlevel, linklevel, rightScale, leftScale;

            if (IsLinkIntf)
            {
                if (string.IsNullOrWhiteSpace(txtlinkrating.Text) || !int.TryParse(txtlinkrating.Text, out cardlevel)) cardlevel = 0;
                if (string.IsNullOrWhiteSpace(txtLvRk.Text) || !int.TryParse(txtLvRk.Text, out linklevel)) linklevel = 0;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtLvRk.Text) || !int.TryParse(txtLvRk.Text, out cardlevel)) cardlevel = 0;
                linklevel = 0;
            }
            if (string.IsNullOrWhiteSpace(txtrightscale.Text) || !int.TryParse(txtrightscale.Text, out rightScale)) rightScale = 0;
            if (string.IsNullOrWhiteSpace(txtleftscale.Text) || !int.TryParse(txtleftscale.Text, out leftScale)) leftScale = 0;

            level = (ulong)(
                ((cardlevel & 0xff) << 0) |
                ((linklevel & 0xff) << 8) |
                ((rightScale & 0xff) << 16) |
                ((leftScale & 0xff) << 24));
            return level;

            /* Quái link
             * 0->7 Link Rating
             * 8->15 Level
             * 16->23 Right
             * 24->31 Left
             * 
             * Quái non-link
             * 0->7 Level
             * 8->15 Trống
             * 16->23 Right
             * 24->31 Left
             */
        }
        private ulong? GetLevelNull()
        {
            if (IsLinkIntf)
            {
                if (string.IsNullOrWhiteSpace(txtLvRk.Text) && string.IsNullOrWhiteSpace(txtlinkrating.Text)) return null;
                else
                {
                    if(string.IsNullOrWhiteSpace(txtLvRk.Text) || !int.TryParse(txtLvRk.Text, out int level)) level = 0;
                    if (string.IsNullOrWhiteSpace(txtlinkrating.Text) || !int.TryParse(txtlinkrating.Text, out int link)) link = 0;

                    return (ulong)((link & 0xFF) | ((level & 0xFF) << 8));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(txtLvRk.Text) && ulong.TryParse(txtLvRk.Text, out ulong level)) return level;
                else return null;
            }
        }
        private ulong? GetRightScaleNull()
        {
            ulong right;
            if (!string.IsNullOrWhiteSpace(txtrightscale.Text)) return null;
            else
            {
                if (!ulong.TryParse(txtrightscale.Text, out right))
                {
                    return null;
                }
            }
            return right;
        }
        private ulong? GetLeftScaleNull()
        {
            ulong left;
            if (!string.IsNullOrWhiteSpace(txtleftscale.Text)) return null;
            else
            {
                if (!ulong.TryParse(txtleftscale.Text, out left))
                {
                    return null;
                }
            }
            return left;
        }
        private ulong GetCardRace()
        {
            ulong race = 0;
            bool isskill = cmbcardtype.SelectedItems.Cast<TypeItem>().Any(item => item.TypeCode == (ulong)CardType.Skill);
            if (isskill)
            {
                var selectedChars = cmbcardchar.SelectedItems?.Cast<CharItem>().ToList();
                if(selectedChars != null && selectedChars.Any())
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
            
            return race;
        }
        private ulong GetCardAttribue()
        {
            ulong attribute = 0;
            var selectedAttributes = cmbcardattribute.SelectedItems?.Cast<AttributeItem>().ToList();
            if (selectedAttributes != null && selectedAttributes.Any())
                foreach (var attributeItem in selectedAttributes)
                {
                    attribute |= attributeItem.AttributeCode;
                }
            return attribute;
        }
        private ulong GetCardCategory()
        {
            ulong category = 0;
            var selectedCategorys = cmbcategory.SelectedItems?.Cast<CategoryItem>().ToList();
            if (selectedCategorys != null && selectedCategorys.Any())
                foreach (var categoryItem in selectedCategorys)
                {
                    category |= categoryItem.CategoryCode;
                }
            return category;
        }
        private ulong GetCardFlag()
        {
            ulong flag = 0;
            var selectedFlags = cmbflag.SelectedItems?.Cast<FlagItem>().ToList();
            if (selectedFlags != null && selectedFlags.Any())
                foreach (var flagItem in selectedFlags)
                {
                    flag |= flagItem.FlagCode;
                }
            return flag;
        }
        private long GetCardRare()
        {
            long rare = 0;
            var selectedRares = cmbrarity.SelectedItems?.Cast<RareItem>().ToList();
            if (selectedRares != null && selectedRares.Any())
                foreach (var rareItem in selectedRares)
                {
                    rare |= rareItem.Code;
                }
            return rare;
        }
        private int GetCardGenesysPoint()
        {
            var input = txtGenesysPoint.Text?.Trim();
            if (string.IsNullOrWhiteSpace(input)) return 0;

            if (!int.TryParse(input, out int genesysPoint) || genesysPoint < 0) return 0;
            else return genesysPoint;
        }

        private CardEditor.Models.Card ExtractCardFromForm()
        {
            try
            {
                return new CardEditor.Models.Card
                {
                    id = GetCardID(),
                    name = GetCardname(),
                    desc = GetCardDesc(),
                    str1 = GetSTR1(),
                    str2 = GetSTR2(),
                    str3 = GetSTR3(),
                    str4 = GetSTR4(),
                    str5 = GetSTR5(),
                    str6 = GetSTR6(),
                    str7 = GetSTR7(),
                    str8 = GetSTR8(),
                    str9 = GetSTR9(),
                    str10 = GetSTR10(),
                    str11 = GetSTR11(),
                    str12 = GetSTR12(),
                    str13 = GetSTR13(),
                    str14 = GetSTR14(),
                    str15 = GetSTR15(),
                    str16 = GetSTR16(),

                    ot = GetCardRule(),
                    alias = GetCardAlias(),
                    setcode = GetSetCode(),
                    type = GetCardType(),
                    atk = GetATK(),
                    def = GetDEF(),
                    level = GetLevel(),
                    race = GetCardRace(),
                    attribute = GetCardAttribue(),
                    category = GetCardCategory(),
                    flag = GetCardFlag()
                };
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Save Card
        public async Task<bool> Save()
        {
            string createdDbPath = string.Empty;
            if (string.IsNullOrWhiteSpace(cdbFilePath) || !System.IO.File.Exists(cdbFilePath))
            {
                try
                {
                    string filePath = FileDiaLogHelper.SaveDataBase();

                    if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                    {
                        string dbFilePath = filePath;

                        string[] validExtensions = { ".cdb", ".db", ".sqlite" };
                        string extension = System.IO.Path.GetExtension(dbFilePath).ToLower();

                        if (string.IsNullOrEmpty(extension) || !validExtensions.Contains(extension))
                        {
                            dbFilePath += ".cdb";
                        }
                        string folderPath = System.IO.Path.GetDirectoryName(dbFilePath);
                        string cdbFileName = System.IO.Path.GetFileName(dbFilePath);

                        var (resultCreate, messageCreate) = await Task.Run(() => CreateFileServices.CreateDatabase(folderPath, cdbFileName));
                        if (resultCreate) createdDbPath = messageCreate;
                        else
                        {
                            CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                                $"{CMess.errorOcc.ToText()} {messageCreate}", new[] { CMess.ok.ToText() });
                            return false;
                        }
                    }
                    else return false;
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                    return false;
                }
            }
            else createdDbPath = cdbFilePath;

            try
            {
                if (await SaveAllCardsInCard(createdDbPath)) return true;
                else return false;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return false;
            }
        }

        #region Archive
        public async Task<(bool, string)> SaveToArchive()
        {
            if (string.IsNullOrEmpty(archiveFilePath) || !System.IO.File.Exists(archiveFilePath) || string.IsNullOrEmpty(archiveEntryName))
                return (true, string.Empty);

            string targetSourcePath =
                !string.IsNullOrEmpty(cdbFilePath) ? cdbFilePath :
                !string.IsNullOrEmpty(xlsxFilePath) ? xlsxFilePath :
                !string.IsNullOrEmpty(cedsFilePath) ? cedsFilePath :
                null;
            if (string.IsNullOrEmpty(targetSourcePath) || !System.IO.File.Exists(targetSourcePath))
                return (false, CMess.fileNotExit.ToText());

            return await LoadDataServices.SaveEntryToZip(archiveFilePath, archiveEntryName, targetSourcePath);
        }
        #endregion

        #region Current Card

        #region Add New Card
        private async Task AddCard()
        {
            if (string.IsNullOrWhiteSpace(txtid.Text) || !ulong.TryParse(txtid.Text, out ulong cardid) || cardid <= 0)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardID.ToText()), new[] { CMess.ok.ToText() });
                return;
            }

            CardEditor.Models.Card newCard = ExtractCardFromForm();
            if (newCard == null) return;
            if (newCard.id < 10000)
            {
                var addQuest = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question, CMess.confirmAdd.ToText(),
                    new[] { CMess.add.ToText(), CMess.cancel.ToText() });
                if (addQuest != 0) return;
            }

            var (resultAdd, messageAdd) = await AddNewCardCommand(newCard);
            if (resultAdd)
            {
                long rare = GetCardRare();
                int genesysPoint = GetCardGenesysPoint();
                var (resultRare, messageRare) = await SaveRareCardCommand(newCard.id, newCard.name, rare);
                var (resultGenesys, messageGenesys) = await SaveGenesysCardCommand(newCard.id, newCard.name, genesysPoint);

                if (resultRare && resultGenesys)
                    MessageNotifi.Enqueue(string.Format(CMess.ThreePlaceholderSuccess.ToText(), 1.ToString(), CMess.Card.ToText(), CMess.tlAdd.ToText()));
                else
                {
                    StringBuilder errorMessage = new StringBuilder();
                    if (!resultRare) errorMessage.AppendLine($"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.Card.ToText())} {messageRare}");
                    if (!resultGenesys) errorMessage.AppendLine($"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.Card.ToText())} {messageGenesys}");
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, errorMessage.ToString(), new[] { CMess.ok.ToText() });

                }
            }
            else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.Card.ToText())} {messageAdd}", new[] { CMess.ok.ToText() });
        }
        public async Task<(bool, string)> AddNewCardCommand(CardEditor.Models.Card newCard = null)
        {
            if (newCard == null) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));

            if (!string.IsNullOrWhiteSpace(cdbFilePath) && System.IO.File.Exists(cdbFilePath))
            {
                var (resultDB, messageDB) = await AddNewCardDB(newCard);
                if (!resultDB) return (false, messageDB);
                var (resultArchive, messageArchive) = await SaveToArchive();
                if (!resultArchive) return (false, messageArchive);
                var (resultGrid, messageGrid) = AddNewCardGrid(newCard);
                if (!resultGrid) return (false, messageGrid);

                IsSaved = true;
                return (true, string.Empty);
            }
            else
            {
                var (resultGrid, messageGrid) = AddNewCardGrid(newCard);
                if (!resultGrid) return (false, messageGrid);
                IsSaved = true;
                return (true, string.Empty);
            }
        }
        private async Task<(bool, string)> AddNewCardDB(CardEditor.Models.Card newCard)
        {
            string connectionString = $"Data Source={cdbFilePath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                if (!CheckDatabase.CheckDatabaseValidity(connection))
                {
                    return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()));
                }
                
                if (await CheckIfIdExists(connection, newCard.id))
                {
                    return (false, CMess.cardIDExist.ToText());
                }
                
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string insertTextsQuery = @"
                            INSERT INTO texts (id, name, desc, str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12, str13, str14, str15, str16)
                            VALUES (@id, @name, @desc, @str1, @str2, @str3, @str4, @str5, @str6, @str7, @str8, @str9, @str10, @str11, @str12, @str13, @str14, @str15, @str16)";

                        using (SQLiteCommand cmd = new SQLiteCommand(insertTextsQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", newCard.id);
                            cmd.Parameters.AddWithValue("@name", newCard.name);
                            cmd.Parameters.AddWithValue("@desc", newCard.desc);
                            cmd.Parameters.AddWithValue("@str1", newCard.str1);
                            cmd.Parameters.AddWithValue("@str2", newCard.str2);
                            cmd.Parameters.AddWithValue("@str3", newCard.str3);
                            cmd.Parameters.AddWithValue("@str4", newCard.str4);
                            cmd.Parameters.AddWithValue("@str5", newCard.str5);
                            cmd.Parameters.AddWithValue("@str6", newCard.str6);
                            cmd.Parameters.AddWithValue("@str7", newCard.str7);
                            cmd.Parameters.AddWithValue("@str8", newCard.str8);
                            cmd.Parameters.AddWithValue("@str9", newCard.str9);
                            cmd.Parameters.AddWithValue("@str10", newCard.str10);
                            cmd.Parameters.AddWithValue("@str11", newCard.str11);
                            cmd.Parameters.AddWithValue("@str12", newCard.str12);
                            cmd.Parameters.AddWithValue("@str13", newCard.str13);
                            cmd.Parameters.AddWithValue("@str14", newCard.str14);
                            cmd.Parameters.AddWithValue("@str15", newCard.str15);
                            cmd.Parameters.AddWithValue("@str16", newCard.str16);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        string insertDatasQuery = @"
                            INSERT INTO datas (id, ot, alias, setcode, type, atk, def, level, race, attribute, category)
                            VALUES (@id, @ot, @alias, @setcode, @type, @atk, @def, @level, @race, @attribute, @category)";

                        using (SQLiteCommand cmd = new SQLiteCommand(insertDatasQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", newCard.id);
                            cmd.Parameters.AddWithValue("@ot", newCard.ot);
                            cmd.Parameters.AddWithValue("@alias", newCard.alias);
                            cmd.Parameters.AddWithValue("@setcode", newCard.setcode);
                            cmd.Parameters.AddWithValue("@type", newCard.type);
                            cmd.Parameters.AddWithValue("@atk", newCard.atk);
                            cmd.Parameters.AddWithValue("@def", newCard.def);
                            cmd.Parameters.AddWithValue("@level", newCard.level);
                            cmd.Parameters.AddWithValue("@race", newCard.race);
                            cmd.Parameters.AddWithValue("@attribute", newCard.attribute);
                            cmd.Parameters.AddWithValue("@category", newCard.category);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        // Nếu không có lỗi, commit giao dịch
                        transaction.Commit();
                        return (true, string.Empty);
                    }
                    catch (SQLiteException ex)
                    {
                        transaction.Rollback();
                        return (false, $"{CMess.errorConDB.ToText()} {ex.Message}");
                    }
                    catch (FormatException ex)
                    {
                        transaction.Rollback();
                        return (false, $"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText())} {ex.Message}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        transaction.Rollback();
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Operation.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                        return (false, $"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Operation.ToText())} {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                        return (false, $"{CMess.errorOcc.ToText()} {ex.Message}");
                    }
                    finally
                    {
                        if (connection != null && connection.State != System.Data.ConnectionState.Closed)
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }
        private (bool, string) AddNewCardGrid(CardEditor.Models.Card newCard)
        {
            try
            {
                if (Cards.Any(card => card.id == newCard.id))
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, CMess.cardIDExist.ToText(), new[] { CMess.ok.ToText() });
                    return (false, CMess.cardIDExist.ToText());
                }
                int insertIndex = Cards.ToList().BinarySearch(newCard, Comparer<CardEditor.Models.Card>.Create((x, y) => x.id.CompareTo(y.id)));
                if (insertIndex < 0) insertIndex = ~insertIndex;

                Cards.Insert(insertIndex, newCard);
                CollectionViewCollection.Refresh();
                datagrMain.SelectedIndex = insertIndex;
                return (true, string.Empty);
            }
            catch (FormatException ex)
            {
                return (false, $"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText())} {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"{CMess.errorOcc.ToText()} {ex.Message}");
            }
        }
        #endregion

        #region Modify Current Card
        private async Task ModifyCardFunction()
        {
            if (string.IsNullOrWhiteSpace(txtid.Text) ||
                !ulong.TryParse(txtid.Text, out ulong cardid) || cardid <= 0)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardID.ToText()), new[] { CMess.ok.ToText() });
                return;
            }

            CardEditor.Models.Card newCard = ExtractCardFromForm();

            var (resultModify, messageModify) = await ModifyCard(newCard);
            if (resultModify)
            {
                long rare = GetCardRare();
                int genesysPoint = GetCardGenesysPoint();

                var (resultRare, messageRare) = await SaveRareCardCommand(newCard.id, newCard.name, rare);
                var (resultGenesys, messageGenesys) = await SaveGenesysCardCommand(newCard.id, newCard.name, genesysPoint);

                if (resultRare && resultGenesys)
                    MessageNotifi.Enqueue(string.Format(CMess.ThreePlaceholderSuccess.ToText(), 1.ToString(), CMess.Card.ToText(), CMess.Update.ToText()));
                else
                {
                    StringBuilder errorMessage = new StringBuilder();
                    if (!resultRare) errorMessage.AppendLine($"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlModify.ToText(), CMess.Card.ToText())} {messageRare}");
                    if (!resultGenesys) errorMessage.AppendLine($"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlModify.ToText(), CMess.Card.ToText())} {messageGenesys}");
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, errorMessage.ToString(), new[] { CMess.ok.ToText() });
                }
            }
            else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlModify.ToText(), CMess.Card.ToText())} {messageModify}", new[] { CMess.ok.ToText() });
        }
        public async Task<(bool, string)> ModifyCard(CardEditor.Models.Card newCard = null)
        {
            if (newCard == null) newCard = ExtractCardFromForm();
            if (newCard == null) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText()));
            if (!string.IsNullOrWhiteSpace(cdbFilePath) && System.IO.File.Exists(cdbFilePath))
            {
                var (resultDB, messageDB) = await ModifyCardDB(newCard);
                if (!resultDB) return (false, messageDB);
                var (resultArchive, messageArchive) = await SaveToArchive();
                if (!resultArchive) return (false, messageArchive);

                var (resultGrid, messageGrid) = ModifyCardGrid(newCard);
                if (!resultGrid) return (false, messageGrid);

                IsSaved = true;
                return (true, string.Empty);
            }
            else
            {
                var (resultGrid, messageGrid) = ModifyCardGrid(newCard);
                if (!resultGrid) return (false, messageGrid);
                IsSaved = true;
                return (true, string.Empty);
            }
        }
        private async Task<(bool, string)> ModifyCardDB(CardEditor.Models.Card newCard)
        {
            string connectionString = $"Data Source={cdbFilePath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                if (!CheckDatabase.CheckDatabaseValidity(connection))
                {
                    return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()));
                }

                if (!await CheckIfIdExists(connection, newCard.id))
                {
                    return (false, CMess.cardIDNotExist.ToText());
                }
                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string updateTextsQuery = @"
                                UPDATE texts SET name = @name, desc = @desc, str1 = @str1, str2 = @str2, str3 = @str3, str4 = @str4, str5 = @str5, str6 = @str6, str7 = @str7, str8 = @str8, str9 = @str9, str10 = @str10, str11 = @str11, str12 = @str12, str13 = @str13, str14 = @str14, str15 = @str15, str16 = @str16 WHERE id = @id";
                        using (SQLiteCommand cmd = new SQLiteCommand(updateTextsQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", newCard.id);
                            cmd.Parameters.AddWithValue("@name", newCard.name);
                            cmd.Parameters.AddWithValue("@desc", newCard.desc);
                            cmd.Parameters.AddWithValue("@str1", newCard.str1);
                            cmd.Parameters.AddWithValue("@str2", newCard.str2);
                            cmd.Parameters.AddWithValue("@str3", newCard.str3);
                            cmd.Parameters.AddWithValue("@str4", newCard.str4);
                            cmd.Parameters.AddWithValue("@str5", newCard.str5);
                            cmd.Parameters.AddWithValue("@str6", newCard.str6);
                            cmd.Parameters.AddWithValue("@str7", newCard.str7);
                            cmd.Parameters.AddWithValue("@str8", newCard.str8);
                            cmd.Parameters.AddWithValue("@str9", newCard.str9);
                            cmd.Parameters.AddWithValue("@str10", newCard.str10);
                            cmd.Parameters.AddWithValue("@str11", newCard.str11);
                            cmd.Parameters.AddWithValue("@str12", newCard.str12);
                            cmd.Parameters.AddWithValue("@str13", newCard.str13);
                            cmd.Parameters.AddWithValue("@str14", newCard.str14);
                            cmd.Parameters.AddWithValue("@str15", newCard.str15);
                            cmd.Parameters.AddWithValue("@str16", newCard.str16);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        string updateDatasQuery = @"
                                UPDATE datas SET ot = @ot, alias = @alias, setcode = @setcode, type = @type, atk = @atk, def = @def, level = @level, race = @race, attribute = @attribute, category = @category WHERE id = @id";
                        using (SQLiteCommand cmd = new SQLiteCommand(updateDatasQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", newCard.id);  // Dùng ID cũ
                            cmd.Parameters.AddWithValue("@ot", newCard.ot);
                            cmd.Parameters.AddWithValue("@alias", newCard.alias);
                            cmd.Parameters.AddWithValue("@setcode", newCard.setcode);
                            cmd.Parameters.AddWithValue("@type", newCard.type);
                            cmd.Parameters.AddWithValue("@atk", newCard.atk);
                            cmd.Parameters.AddWithValue("@def", newCard.def);
                            cmd.Parameters.AddWithValue("@level", newCard.level);
                            cmd.Parameters.AddWithValue("@race", newCard.race);
                            cmd.Parameters.AddWithValue("@attribute", newCard.attribute);
                            cmd.Parameters.AddWithValue("@category", newCard.category);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                        return (true, string.Empty);
                    }
                    catch (SQLiteException ex)
                    {
                        transaction.Rollback();
                        return (false, $"{CMess.errorConDB.ToText()} {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return (false, $"{CMess.errorOcc.ToText()} {ex.Message}");
                    }
                    finally
                    {
                        if (connection != null && connection.State != System.Data.ConnectionState.Closed)
                        {
                            connection.Close();
                        }
                    }
                }
            }
        }
        private (bool, string) ModifyCardGrid(CardEditor.Models.Card newCard)
        {
            try
            {
                var existingCard = Cards.FirstOrDefault(card => card.id == newCard.id);

                if (existingCard == null)
                {
                    return (false, CMess.cardIDNotExist.ToText());
                }

                existingCard.name = newCard.name;
                existingCard.desc = newCard.desc;
                existingCard.str1 = newCard.str1;
                existingCard.str2 = newCard.str2;
                existingCard.str3 = newCard.str3;
                existingCard.str4 = newCard.str4;
                existingCard.str5 = newCard.str5;
                existingCard.str6 = newCard.str6;
                existingCard.str7 = newCard.str7;
                existingCard.str8 = newCard.str8;
                existingCard.str9 = newCard.str9;
                existingCard.str10 = newCard.str10;
                existingCard.str11 = newCard.str11;
                existingCard.str12 = newCard.str12;
                existingCard.str13 = newCard.str13;
                existingCard.str14 = newCard.str14;
                existingCard.str15 = newCard.str15;
                existingCard.str16 = newCard.str16;

                existingCard.ot = newCard.ot;
                existingCard.alias = newCard.alias;
                existingCard.setcode = newCard.setcode;
                existingCard.type = newCard.type;
                existingCard.atk = newCard.atk;
                existingCard.def = newCard.def;
                existingCard.level = newCard.level;
                existingCard.race = newCard.race;
                existingCard.attribute = newCard.attribute;
                existingCard.category = newCard.category;

                CollectionViewCollection.Refresh();
                return (true, string.Empty);
            }
            catch (FormatException ex)
            {
                return (false, $"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText())} {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        public async Task<(bool, string)> SaveRareCardCommand(ulong id, string name, long rare)
        {
            var newCard = new RareCard { id = id, name = name, rare = rare };
            var (resultDB, messageDB) = await RareRawDataViewModel.Instance.ModifyRareCardDatabase(newCard);
            if (resultDB)
            {
                var (resultMemory, isAdd) = RareRawDataViewModel.Instance.ModifyRareCard(newCard);
                if (resultMemory) return (true, string.Empty);
                else return (false, string.Format(CMess.ThreePlaceholderError.ToText(), isAdd ? CMess.tlAdd.ToText() : CMess.tlModify.ToText(), CMess.Rarity.ToText(), CMess.Card.ToText()));
            }
            else return (false, messageDB);
            // RareRawDataViewModel.Instance.UpdateRareMap(id, rare);
        }
        public async Task<(bool, string)> SaveGenesysCardCommand(ulong id, string name, int gPoint)
        {
            var newCard = new GenesysCard { Id = id, Name = name, GPoints = gPoint };
            var (resultDB, messageDB) = await GenesysRawDataViewModel.Instance.ModifyGenesysCardDatabase(newCard);
            if (resultDB)
            {
                var (resultMemory, isAdd) = GenesysRawDataViewModel.Instance.ModifyGenesysCard(newCard);
                if (resultMemory) return (true, string.Empty);
                else return (false, string.Format(CMess.ThreePlaceholderError.ToText(), isAdd ? CMess.tlAdd.ToText() : CMess.tlModify.ToText(), CMess.Genesys.ToText(), CMess.Card.ToText()));
            }
            else return (false, messageDB);
        }
        private async Task<bool> CheckIfIdExists(SQLiteConnection connection, ulong id)
        {
            string query = "SELECT COUNT(1) FROM texts WHERE id = @id";
            using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
        }
        #endregion

        #region Cards Grid
        public void SaveAllCard()
        {
            string connectionString = $"Data Source={cdbFilePath};Version=3;";

            const string checkExistenceQuery = "SELECT COUNT(*) FROM datas WHERE id = @Id";

            const string updateDatasQuery = @"UPDATE datas 
                SET ot = @Ot, alias = @Alias, setcode = @Setcode, type = @Type, 
                atk = @Atk, def = @Def, level = @Level, race = @Race, 
                attribute = @Attribute, category = @Category
                WHERE id = @Id";
            const string updateTextsQuery = @"UPDATE texts 
                SET name = @Name, desc = @Desc,
                str1 = @Str1, str2 = @Str2, str3 = @Str3, str4 = @Str4,
                str5 = @Str5, str6 = @Str6, str7 = @Str7, str8 = @Str8,
                str9 = @Str9, str10 = @Str10, str11 = @Str11, str12 = @Str12,
                str13 = @Str13, str14 = @Str14, str15 = @Str15, str16 = @Str16
                WHERE id = @Id";

            const string insertDatasQuery = @"INSERT INTO datas (id, ot, alias, setcode, type, atk, def, level, race, attribute, category)
                VALUES (@Id, @Ot, @Alias, @Setcode, @Type, @Atk, @Def, @Level, @Race, @Attribute, @Category)";

            const string insertTextsQuery = @"INSERT INTO texts (id, name, desc, str1, str2, str3, str4, str5, str6, str7, str8,
                str9, str10, str11, str12, str13, str14, str15, str16)
                VALUES (@Id, @Name, @Desc, @Str1, @Str2, @Str3, @Str4, @Str5, @Str6, @Str7, @Str8,
                @Str9, @Str10, @Str11, @Str12, @Str13, @Str14, @Str15, @Str16)";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var card in Cards)
                        {
                            bool cardExists;
                            using (var checkCommand = new SQLiteCommand(checkExistenceQuery, connection, transaction))
                            {
                                checkCommand.Parameters.AddWithValue("@Id", card.id);
                                cardExists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                            }

                            if (cardExists)
                            {
                                using (var updateDatasCommand = new SQLiteCommand(updateDatasQuery, connection, transaction))
                                {
                                    updateDatasCommand.Parameters.AddWithValue("@Id", card.id);
                                    updateDatasCommand.Parameters.AddWithValue("@Ot", card.ot);
                                    updateDatasCommand.Parameters.AddWithValue("@Alias", card.alias);
                                    updateDatasCommand.Parameters.AddWithValue("@Setcode", card.setcode);
                                    updateDatasCommand.Parameters.AddWithValue("@Type", card.type);
                                    updateDatasCommand.Parameters.AddWithValue("@Atk", card.atk);
                                    updateDatasCommand.Parameters.AddWithValue("@Def", card.def);
                                    updateDatasCommand.Parameters.AddWithValue("@Level", card.level);
                                    updateDatasCommand.Parameters.AddWithValue("@Race", card.race);
                                    updateDatasCommand.Parameters.AddWithValue("@Attribute", card.attribute);
                                    updateDatasCommand.Parameters.AddWithValue("@Category", card.category);
                                    updateDatasCommand.ExecuteNonQuery();
                                }
                                using (var updateTextsCommand = new SQLiteCommand(updateTextsQuery, connection, transaction))
                                {
                                    updateTextsCommand.Parameters.AddWithValue("@Id", card.id);
                                    updateTextsCommand.Parameters.AddWithValue("@Name", card.name);
                                    updateTextsCommand.Parameters.AddWithValue("@Desc", card.desc);
                                    updateTextsCommand.Parameters.AddWithValue("@Str1", card.str1);
                                    updateTextsCommand.Parameters.AddWithValue("@Str2", card.str2);
                                    updateTextsCommand.Parameters.AddWithValue("@Str3", card.str3);
                                    updateTextsCommand.Parameters.AddWithValue("@Str4", card.str4);
                                    updateTextsCommand.Parameters.AddWithValue("@Str5", card.str5);
                                    updateTextsCommand.Parameters.AddWithValue("@Str6", card.str6);
                                    updateTextsCommand.Parameters.AddWithValue("@Str7", card.str7);
                                    updateTextsCommand.Parameters.AddWithValue("@Str8", card.str8);
                                    updateTextsCommand.Parameters.AddWithValue("@Str9", card.str9);
                                    updateTextsCommand.Parameters.AddWithValue("@Str10", card.str10);
                                    updateTextsCommand.Parameters.AddWithValue("@Str11", card.str11);
                                    updateTextsCommand.Parameters.AddWithValue("@Str12", card.str12);
                                    updateTextsCommand.Parameters.AddWithValue("@Str13", card.str13);
                                    updateTextsCommand.Parameters.AddWithValue("@Str14", card.str14);
                                    updateTextsCommand.Parameters.AddWithValue("@Str15", card.str15);
                                    updateTextsCommand.Parameters.AddWithValue("@Str16", card.str16);
                                    updateTextsCommand.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                using (var insertDatasCommand = new SQLiteCommand(insertDatasQuery, connection, transaction))
                                {
                                    insertDatasCommand.Parameters.AddWithValue("@Id", card.id);
                                    insertDatasCommand.Parameters.AddWithValue("@Ot", card.ot);
                                    insertDatasCommand.Parameters.AddWithValue("@Alias", card.alias);
                                    insertDatasCommand.Parameters.AddWithValue("@Setcode", card.setcode);
                                    insertDatasCommand.Parameters.AddWithValue("@Type", card.type);
                                    insertDatasCommand.Parameters.AddWithValue("@Atk", card.atk);
                                    insertDatasCommand.Parameters.AddWithValue("@Def", card.def);
                                    insertDatasCommand.Parameters.AddWithValue("@Level", card.level);
                                    insertDatasCommand.Parameters.AddWithValue("@Race", card.race);
                                    insertDatasCommand.Parameters.AddWithValue("@Attribute", card.attribute);
                                    insertDatasCommand.Parameters.AddWithValue("@Category", card.category);
                                    insertDatasCommand.ExecuteNonQuery();
                                }
                                using (var insertTextsCommand = new SQLiteCommand(insertTextsQuery, connection, transaction))
                                {
                                    insertTextsCommand.Parameters.AddWithValue("@Id", card.id);
                                    insertTextsCommand.Parameters.AddWithValue("@Name", card.name);
                                    insertTextsCommand.Parameters.AddWithValue("@Desc", card.desc);
                                    insertTextsCommand.Parameters.AddWithValue("@Str1", card.str1);
                                    insertTextsCommand.Parameters.AddWithValue("@Str2", card.str2);
                                    insertTextsCommand.Parameters.AddWithValue("@Str3", card.str3);
                                    insertTextsCommand.Parameters.AddWithValue("@Str4", card.str4);
                                    insertTextsCommand.Parameters.AddWithValue("@Str5", card.str5);
                                    insertTextsCommand.Parameters.AddWithValue("@Str6", card.str6);
                                    insertTextsCommand.Parameters.AddWithValue("@Str7", card.str7);
                                    insertTextsCommand.Parameters.AddWithValue("@Str8", card.str8);
                                    insertTextsCommand.Parameters.AddWithValue("@Str9", card.str9);
                                    insertTextsCommand.Parameters.AddWithValue("@Str10", card.str10);
                                    insertTextsCommand.Parameters.AddWithValue("@Str11", card.str11);
                                    insertTextsCommand.Parameters.AddWithValue("@Str12", card.str12);
                                    insertTextsCommand.Parameters.AddWithValue("@Str13", card.str13);
                                    insertTextsCommand.Parameters.AddWithValue("@Str14", card.str14);
                                    insertTextsCommand.Parameters.AddWithValue("@Str15", card.str15);
                                    insertTextsCommand.Parameters.AddWithValue("@Str16", card.str16);
                                    insertTextsCommand.ExecuteNonQuery();
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        } // Bỏ

        #region Get Selected Card
        public async Task<bool> SaveSelectedCardsOnDataGrid(string targetedPath = null)
        {
            string filePath = targetedPath ?? cdbFilePath;

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, CMess.noSelecDB.ToText(), new[] { CMess.ok.ToText() });
                return false;
            }

            if (datagrMain.SelectedItems == null)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, CMess.noCardSelec.ToText(), new[] { CMess.ok.ToText() });
                return false;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var selectedItems = datagrMain.SelectedItems.Cast<CardEditor.Models.Card>().ToList();
                if (selectedItems.Any())
                {
                    if (await SaveAllCards(selectedItems, filePath)) return true;
                    else return false;
                }
                else
                {
                    CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, CMess.noCardSelec.ToText(), new[] { CMess.ok.ToText() });
                    return true;
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return false;
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        public async Task<bool> SaveFiltedCardOnDataGrid(string targetedPath = null)
        {
            string filePath = targetedPath ?? cdbFilePath;

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, CMess.noSelecDB.ToText(), new[] { CMess.ok.ToText() });
                return false;
            }
            if (CollectionViewCollection != null && CollectionViewCollection.Cast<CardEditor.Models.Card>().Any())
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    var filteredItems = CollectionViewCollection.Cast<CardEditor.Models.Card>().ToList();
                    if (await SaveAllCards(filteredItems, filePath)) return true;
                    else return false;
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.Card.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                    return false;
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
            else
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, CMess.noCardExport.ToText(), new[] { CMess.ok.ToText() });
                return true;
            }
        }
        public async Task<bool> SaveAllCardsInCard(string targetedPath = null)
        {
            string filePath = targetedPath ?? cdbFilePath;

            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, CMess.noSelecDB.ToText(), new[] { CMess.ok.ToText() });
                return false;
            }
            if (Cards != null && Cards.Any())
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    if (await SaveAllCards(Cards.ToList(), filePath)) return true;
                    else return false;
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.Card.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                    return false;
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
            else
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, CMess.noCardExport.ToText(), new[] { CMess.ok.ToText() });
                return true;
            }
        }
        #endregion

        #region Save Cards Selected
        public async Task<bool> SaveAllCards(List<CardEditor.Models.Card> selectedItems, string targetPath)
        {
            if (selectedItems == null || !selectedItems.Any())
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, CMess.noCardSave.ToText(), new[] { CMess.ok.ToText() });
                return false;
            }
            if(string.IsNullOrWhiteSpace(targetPath) || !System.IO.File.Exists(targetPath))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, CMess.noSelecDB.ToText(), new[] { CMess.ok.ToText() });
                return false;
            }    
            string connectionString = $"Data Source={targetPath};Version=3;";

            const string checkExistenceQuery = "SELECT id FROM datas";

            const string updateDatasQuery = @"UPDATE datas 
                SET ot = @Ot, alias = @Alias, setcode = @Setcode, type = @Type, 
                atk = @Atk, def = @Def, level = @Level, race = @Race, 
                attribute = @Attribute, category = @Category
                WHERE id = @Id";
            const string updateTextsQuery = @"UPDATE texts 
                SET name = @Name, desc = @Desc,
                str1 = @Str1, str2 = @Str2, str3 = @Str3, str4 = @Str4,
                str5 = @Str5, str6 = @Str6, str7 = @Str7, str8 = @Str8,
                str9 = @Str9, str10 = @Str10, str11 = @Str11, str12 = @Str12,
                str13 = @Str13, str14 = @Str14, str15 = @Str15, str16 = @Str16
                WHERE id = @Id";

            const string insertDatasQuery = @"INSERT INTO datas (id, ot, alias, setcode, type, atk, def, level, race, attribute, category)
                VALUES (@Id, @Ot, @Alias, @Setcode, @Type, @Atk, @Def, @Level, @Race, @Attribute, @Category)";

            const string insertTextsQuery = @"INSERT INTO texts (id, name, desc, str1, str2, str3, str4, str5, str6, str7, str8,
                str9, str10, str11, str12, str13, str14, str15, str16)
                VALUES (@Id, @Name, @Desc, @Str1, @Str2, @Str3, @Str4, @Str5, @Str6, @Str7, @Str8,
                @Str9, @Str10, @Str11, @Str12, @Str13, @Str14, @Str15, @Str16)";

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // **Lấy danh sách ID hiện có để tránh truy vấn nhiều lần**
                        List<ulong> existingIds = new List<ulong>();
                        using (var checkCommand = new SQLiteCommand(checkExistenceQuery, connection, transaction))
                        using (var reader = await checkCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                existingIds.Add((ulong)reader.GetInt64(0));
                            }
                        }

                        foreach (var card in selectedItems)
                        {
                            if (card.id < 0 || card.id > (1UL << 32) - 1) continue;
                            bool cardExists = existingIds.Contains(card.id);

                            string queryToUse = cardExists ? updateDatasQuery : insertDatasQuery;
                            using (var command = new SQLiteCommand(queryToUse, connection, transaction))
                            {
                                AddParameters(command, card);
                                await command.ExecuteNonQueryAsync();
                            }

                            queryToUse = cardExists ? updateTextsQuery : insertTextsQuery;
                            using (var command = new SQLiteCommand(queryToUse, connection, transaction))
                            {
                                AddTextParameters(command, card);
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                        transaction.Commit();

                        CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                            string.Format(CMess.ThreePlaceholderSuccess.ToText(), selectedItems.Count.ToString(), CMess.Card.ToText(), CMess.Save.ToText()),
                            new[] { CMess.ok.ToText() });
                        IsSaved = true;
                        return true;
                    }
                    catch(Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        private void AddParameters(SQLiteCommand command, CardEditor.Models.Card card)
        {
            command.Parameters.AddWithValue("@Id", card.id);
            command.Parameters.AddWithValue("@Ot", card.ot);
            command.Parameters.AddWithValue("@Alias", card.alias);
            command.Parameters.AddWithValue("@Setcode", card.setcode);
            command.Parameters.AddWithValue("@Type", card.type);
            command.Parameters.AddWithValue("@Atk", card.atk);
            command.Parameters.AddWithValue("@Def", card.def);
            command.Parameters.AddWithValue("@Level", card.level);
            command.Parameters.AddWithValue("@Race", card.race);
            command.Parameters.AddWithValue("@Attribute", card.attribute);
            command.Parameters.AddWithValue("@Category", card.category);
        }
        private void AddTextParameters(SQLiteCommand command, CardEditor.Models.Card card)
        {
            command.Parameters.AddWithValue("@Id", card.id);
            command.Parameters.AddWithValue("@Name", card.name ?? string.Empty);
            command.Parameters.AddWithValue("@Desc", card.desc ?? string.Empty);
            command.Parameters.AddWithValue("@Str1", card.str1 ?? string.Empty);
            command.Parameters.AddWithValue("@Str2", card.str2 ?? string.Empty);
            command.Parameters.AddWithValue("@Str3", card.str3 ?? string.Empty);
            command.Parameters.AddWithValue("@Str4", card.str4 ?? string.Empty);
            command.Parameters.AddWithValue("@Str5", card.str5 ?? string.Empty);
            command.Parameters.AddWithValue("@Str6", card.str6 ?? string.Empty);
            command.Parameters.AddWithValue("@Str7", card.str7 ?? string.Empty);
            command.Parameters.AddWithValue("@Str8", card.str8 ?? string.Empty);
            command.Parameters.AddWithValue("@Str9", card.str9 ?? string.Empty);
            command.Parameters.AddWithValue("@Str10", card.str10 ?? string.Empty);
            command.Parameters.AddWithValue("@Str11", card.str11 ?? string.Empty);
            command.Parameters.AddWithValue("@Str12", card.str12 ?? string.Empty);
            command.Parameters.AddWithValue("@Str13", card.str13 ?? string.Empty);
            command.Parameters.AddWithValue("@Str14", card.str14 ?? string.Empty);
            command.Parameters.AddWithValue("@Str15", card.str15 ?? string.Empty);
            command.Parameters.AddWithValue("@Str16", card.str16 ?? string.Empty);
        }
        #endregion

        #endregion

        #endregion

        #region Scriot Card
        private (ulong, ulong) GetCardPassword()
        {
            if (string.IsNullOrWhiteSpace(txtid.Text)) return (0, 0);
            if (!ulong.TryParse(txtid.Text, out ulong passWord) || !ulong.TryParse(txtalias.Text, out ulong alias)) return (0, 0);
            if (CurrentCard != null)
            {
                if (isChangedID)
                {
                    var chooseID = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                        $"{CMess.idChanged} {CMess.quesSelectCreaScript.ToText()}",
                        new[] { CMess.SelectCards.ToText(), CMess.newIDCard.ToText(), CMess.cancel.ToText() });

                    if (chooseID == 0) return (CurrentCard.id, CurrentCard.alias);
                    else if (chooseID == 1) return (passWord, alias);
                    else return (0, 0);
                }
                else return (CurrentCard.id, CurrentCard.alias);
            }
            else return (passWord, alias);
        }
        private (string, LanguageArea) GetCardScriptContent()
        {
            string luaContent = CurrentCard != null
                ? CurrentCard.name
                : (string.IsNullOrEmpty(txtcardname.Text) ? string.Empty : txtcardname.Text);

            LanguageArea Area = LanguageDetectorHelper.CheckLanguageArea(luaContent);
            return (luaContent, Area);
        }
        private string GetDirectoryPath()
        {
            var firstValidFile = new[] { cdbFilePath, cedsFilePath, xlsxFilePath }
            .FirstOrDefault(path => !string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path));

            if (!string.IsNullOrWhiteSpace(firstValidFile))
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(firstValidFile), "script");

            string folderPath = FileDiaLogHelper.OpenFolder();
            return !string.IsNullOrWhiteSpace(folderPath)
                ? System.IO.Path.Combine(folderPath, "script")
                : string.Empty;
        }

        private (bool, string) CreateScriptFile(string DirectoryPath, ulong password, string luaContent, LanguageArea Area)
        {
            var (resultCreate, messageCreate) = CreateFileServices.CreateScript(DirectoryPath, $"c{password}.lua", newScript: true, luaContent, Area);
            if (resultCreate)
            {
                CardEXDataViewModel.Instance.AddScriptPath(password, messageCreate);
                return (true, messageCreate);
            }
            else return (false, messageCreate);
        }
        private async Task ScriptCardHandler(ulong password, string luaContent, LanguageArea Area, string DirectoryPath)
        {
            var candidates = new List<(string fullPath, string archiveFilePath, string archiveEntryName)>();

            #region Archive
            if (!string.IsNullOrEmpty(archiveFilePath) && System.IO.File.Exists(archiveFilePath))
            {
                var resultFilterArchive = await LoadDataServices.FindEntriesByName(archiveFilePath, $"c{password}", "lua");
                if (resultFilterArchive.success)
                {
                    if (resultFilterArchive.fileLists.Count() > 0)
                    {
                        var resultExtract = await LoadDataServices.ExtractTempEntry(archiveFilePath, resultFilterArchive.fileLists);
                        if (resultExtract.result && resultExtract.extractedEntries.Count() > 0)
                        {
                            foreach (var entry in resultExtract.extractedEntries)
                            {
                                candidates.Add((entry.TempPath, archiveFilePath, entry.EntryFullName));
                            }
                        }
                    }
                    //else
                    //{
                    //    string tempDir = Path.Combine(Path.GetTempPath(), "CardEditorTemp", "script");
                    //    var (resultCreate, messageCreate) = CreateScriptFile(tempDir, password, luaContent, Area);
                    //    if (resultCreate) await MainWindowService.OpenCodeEditorTab(messageCreate, archiveFilePath, $"script/{System.IO.Path.GetFileName(messageCreate)}");
                    //    else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    //        $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.Script.ToText(), CMess.File.ToText())} {messageCreate}",
                    //        new[] { CMess.ok.ToText() });
                    //}
                }
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.load.ToText(), CMess.CardScript.ToText())} {resultFilterArchive.message}",
                    new[] { CMess.ok.ToText() });
            }
            #endregion

            #region Physical files
            var ScriptList = CardEXDataViewModel.Instance.TryGetScriptpath(password)?.ToList() ?? new List<string>();
            string foundScriptPath = FindIInfoService.FindScriptFile(password.ToString(), Path.GetDirectoryName(DirectoryPath), false);
            if (!string.IsNullOrWhiteSpace(foundScriptPath) && System.IO.File.Exists(foundScriptPath) && !ScriptList.Contains(foundScriptPath))
                ScriptList.Add(foundScriptPath);

            foreach (var path in ScriptList)
                candidates.Add((path, null, null));
            #endregion

            if (candidates.Count == 0) // Không tìm thấy ở đâu cả -> tạo mới
            {
                // Nếu có archiveFilePath, script mới nên nằm trong temp (giữ hành vi cũ);
                // ngược lại tạo trực tiếp trong DirectoryPath.
                string createDir = !string.IsNullOrEmpty(archiveFilePath) && System.IO.File.Exists(archiveFilePath)
                    ? Path.Combine(Path.GetTempPath(), "CardEditorTemp", "script")
                    : DirectoryPath;

                var (resultCreate, messageCreate) = CreateScriptFile(createDir, password, luaContent, Area);
                if (resultCreate)
                {
                    string entryName = !string.IsNullOrEmpty(archiveFilePath) && System.IO.File.Exists(archiveFilePath)
                        ? $"script/{System.IO.Path.GetFileName(messageCreate)}"
                        : null;
                    await MainWindowService.OpenCodeEditorTab(messageCreate,
                        entryName != null ? archiveFilePath : null, entryName);
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.Script.ToText(), CMess.File.ToText())} {messageCreate}",
                        new[] { CMess.ok.ToText() });
                }
            }
            else if (candidates.Count == 1) // Chỉ có 1 -> mở thẳng
            {
                var c = candidates[0];
                await MainWindowService.OpenCodeEditorTab(c.fullPath, c.archiveFilePath, c.archiveEntryName);
            }
            else // Nhiều -> cho user chọn
            {
                var (selectResult, selectedItems) = MainWindowService.SelectFileToOpen(candidates);
                if (selectResult)
                {
                    foreach (var item in selectedItems)
                    {
                        if (!File.Exists(item.FullPath))
                        {
                            CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                                CMess.fileNotExit.ToText(), new[] { CMess.ok.ToText() });
                            continue;
                        }
                        await MainWindowService.OpenCodeEditorTab(item.FullPath, item.ArchiveFilePath, item.ArchiveEntryName);
                    }
                }
            }
        }
        private async Task ScriptCard()
        {
            if (MainWindowService == null) return;
            var (password, aliasPassword) = GetCardPassword();
            if (password == 0)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardID.ToText()),
                    new[] { CMess.ok.ToText() });
                return;
            }
            var (luaContent, Area) = GetCardScriptContent();
            string DirectoryPath = GetDirectoryPath();
            if (string.IsNullOrWhiteSpace(DirectoryPath)) return;
            if (!Directory.Exists(DirectoryPath)) Directory.CreateDirectory(DirectoryPath);

            try
            {
                if (!CardEXDataViewModel.Instance.IsLoadedScript) await CardEXDataViewModel.Instance.LoadScriptAsync();
                foreach (var id in new[] { aliasPassword, password }.Where(x => x > 0).Distinct())
                {
                    await ScriptCardHandler(id, luaContent, Area, DirectoryPath);
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.Script.ToText(), CMess.File.ToText())} {ex.Message}",
                    new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #region Sort
        private void SortCard()
        {
            ApplySorting();
        }
        private void ApplySorting()
        {
            var selectedSorts = SortsViewModel.Instance.SelectedSortItems;
            if (selectedSorts == null || selectedSorts.Count <= 0) return;

            try
            {
                using (CollectionViewCollection.DeferRefresh())
                {
                    CollectionViewCollection.SortDescriptions.Clear();

                    foreach (var sort in selectedSorts)
                    {
                        if (sort?.SelectedItem == null) continue;

                        string cardPropertyName = sort.SelectedItem.Name;
                        if (cardPropertyName == null || string.IsNullOrEmpty(cardPropertyName)) continue;
                        if (cardPropertyName == "Rare" || cardPropertyName == "GPoint") continue;

                        var direction = sort.OrderByAsc ? ListSortDirection.Ascending : ListSortDirection.Descending;

                        CollectionViewCollection.SortDescriptions.Add(new SortDescription(cardPropertyName, direction));
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }

            

            //int arrangeValue = ConfigViewModel.Instance.Arrange;
            //switch (arrangeValue)
            //{
            //    case 1: // Tăng dần theo id
            //        CollectionViewCollection.SortDescriptions.Add(new SortDescription("id", ListSortDirection.Ascending));
            //        break;
            //    case 2: // Giảm dần theo id
            //        CollectionViewCollection.SortDescriptions.Add(new SortDescription("id", ListSortDirection.Descending));
            //        break;
            //    case 3: // Tăng dần theo name (theo bảng chữ cái)
            //        CollectionViewCollection.SortDescriptions.Add(new SortDescription("name", ListSortDirection.Ascending));
            //        break;
            //    case 4: // Giảm dần theo name (ngược bảng chữ cái)
            //        CollectionViewCollection.SortDescriptions.Add(new SortDescription("name", ListSortDirection.Descending));
            //        break;
            //    default: // Mặc định không sắp xếp hoặc theo id tăng dần
            //        CollectionViewCollection.SortDescriptions.Add(new SortDescription("id", ListSortDirection.Ascending));
            //        break;
            //}
            //CollectionViewCollection.Refresh();
        }
        private void btnSort_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            CollectionViewCollection.SortDescriptions.Clear();
            CollectionViewCollection.Refresh();
        }
        #endregion

        #region undo
        private void UndoAction()
        {
            Undo();
        }
        private void btnundo_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            RedoCommand();
        }
        #endregion

        #region Reset Card
        private void ResetCurrentCard()
        {
            if (CurrentCard != null)
            {
                if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmReSet)
                {
                    var result = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                        string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.tlReset.ToText(), CMess.SelectCards.ToText()),
                        new[] { CMess.yes.ToText(), CMess.no.ToText() });
                    if (result != 0) return;
                }
                LoadCardData();
                LoadCardRare();
                LoadGenesysPoint();
            }
            else CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                CMess.noCardSelec.ToText(), new[] { CMess.ok.ToText() });
        }
        #endregion

        #region Clear Card
        private void ClearCurrentCard()
        {
            if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmClear)
            {
                var result = CMSG.Show(CMess.conClear.ToText(), CMSG.MessageBoxIconType.Question,
                    string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.tlClear.ToText(), CMess.SelectCards.ToText()),
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
            }
            CurrentCard = null;
            datagrMain.SelectedItem = null;
        }

        private void ClearAllAttributes()
        {
            CurrentCard.id = 0;

            CurrentCard.name = string.Empty;
            CurrentCard.desc = string.Empty;
            CurrentCard.str1 = string.Empty;
            CurrentCard.str2 = string.Empty;
            CurrentCard.str3 = string.Empty;
            CurrentCard.str4 = string.Empty;
            CurrentCard.str5 = string.Empty;
            CurrentCard.str6 = string.Empty;
            CurrentCard.str7 = string.Empty;
            CurrentCard.str8 = string.Empty;
            CurrentCard.str9 = string.Empty;
            CurrentCard.str10 = string.Empty;
            CurrentCard.str11 = string.Empty;
            CurrentCard.str12 = string.Empty;
            CurrentCard.str13 = string.Empty;
            CurrentCard.str14 = string.Empty;
            CurrentCard.str15 = string.Empty;
            CurrentCard.str16 = string.Empty;

            CurrentCard.ot = 0;
            CurrentCard.alias = 0;
            CurrentCard.setcode = 0;
            CurrentCard.type = 0;
            CurrentCard.atk = 0;
            CurrentCard.def = 0;
            CurrentCard.level = 0;
            CurrentCard.race = 0;
            CurrentCard.attribute = 0;
            CurrentCard.category = 0;

        }
        private void ClearAll()
        {
            try
            {
                txtcardname.Text = string.Empty;
                cmbrule?.SelectedItems?.Clear();
                cmbcardtype?.SelectedItems?.Clear();
                cmbcardrace?.SelectedItems?.Clear();
                cmbcardattribute?.SelectedItems?.Clear();
                cmbsetcode?.SelectedItems?.Clear();
                cmbcategory?.SelectedItems?.Clear();
                cmbflag?.SelectedItems?.Clear();
                cmbrarity?.SelectedItems?.Clear();
                txtLvRk.Text = string.Empty;
                txtlinkrating.Text = string.Empty;
                txtleftscale.Text = string.Empty;
                txtrightscale.Text = string.Empty;
                txtatk.Text = string.Empty;
                txtdef.Text = string.Empty;
                txtid.Text = string.Empty;
                txtalias.Text = string.Empty;
                SetLinkArrow(false);
                
                txtcarddesc.Clear();
                txtstr1.Text = string.Empty;
                txtstr2.Text = string.Empty;
                txtstr3.Text = string.Empty;
                txtstr4.Text = string.Empty;
                txtstr5.Text = string.Empty;
                txtstr6.Text = string.Empty;
                txtstr7.Text = string.Empty;
                txtstr8.Text = string.Empty;
                txtstr9.Text = string.Empty;
                txtstr10.Text = string.Empty;
                txtstr11.Text = string.Empty;
                txtstr12.Text = string.Empty;
                txtstr13.Text = string.Empty;
                txtstr14.Text = string.Empty;
                txtstr15.Text = string.Empty;
                txtstr16.Text = string.Empty;

                SetImageCardToNull();
                SetImageCardDefault();

                SetArtWorkToNull();
                //SetArtWorkDefault();
                //ShowArtWork();
            }
            catch { }
            finally { }
        }
        #endregion

        #region Delete
        private async Task DeleteCurrentCard()
        {
            if (ConfigViewModel.Instance.dataHandlingSetting.ConfirmDelete)
            {
                var result = CMSG.Show(CMess.conDelete.ToText(), CMSG.MessageBoxIconType.Question,
                    String.Format(CMess.confirmDelete.ToText(), CMess.SelectCards.ToText()),
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
            }
            await DeleteCard();
        }
        private async Task DeleteCard()
        {
            ulong cardIdToDelete;
            if (isChangedID)
            {
                var resultID = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    $"{CMess.idChanged.ToText()} {CMess.quesSelectDelete.ToText()}",
                    new[] { CMess.SelectCards.ToText(), CMess.newIDCard.ToText() });
                cardIdToDelete = (resultID == 0) ? CurrentCard.id : GetCardID();
                }
            else cardIdToDelete = GetCardID();

            await ShowUndoPanel(cardIdToDelete);
        }
        private async Task ShowUndoPanel(ulong cardIdToDelete)
        {
            btndelete.Visibility = Visibility.Collapsed;
            blUndoDelete.Visibility = Visibility.Visible;
            progressBar.Value = 0;
            bool userCancelled = false;
            RoutedEventHandler undoHandler = null;
            RoutedEventHandler deleteNowHandler = null;

            undoHandler = (s, e) =>
            {
                userCancelled = true;
                blUndoDelete.Visibility = Visibility.Collapsed;
                btndelete.Visibility = Visibility.Visible;
                btnUndoDelete.Click -= undoHandler;
                btnDeleteNow.Click -= deleteNowHandler;
            };
            deleteNowHandler = async (s, e) =>
            {
                userCancelled = true;
                blUndoDelete.Visibility = Visibility.Collapsed;
                btndelete.Visibility = Visibility.Visible;
                btnUndoDelete.Click -= undoHandler;
                btnDeleteNow.Click -= deleteNowHandler;
                await DeleteCard(cardIdToDelete);
            };
            btnUndoDelete.Click += undoHandler;
            btnDeleteNow.Click += deleteNowHandler;

            await RunUndoProgress();

            if (!userCancelled)
            {
                blUndoDelete.Visibility = Visibility.Collapsed;
                btndelete.Visibility = Visibility.Visible;
                btnUndoDelete.Click -= undoHandler;
                btnDeleteNow.Click -= deleteNowHandler;
                await DeleteCard(cardIdToDelete);
            }
        }
        private async Task RunUndoProgress()
        {
            int durationMs = 5000;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < durationMs)
            {
                double progress = (double)sw.ElapsedMilliseconds / durationMs * 100;
                progressBar.Value = progress;
                await Task.Delay(16); // khoảng 60 FPS
            }
            progressBar.Value = 100;
        }

        private async Task DeleteCard(ulong cardIdToDelete)
        {
            try
            {
                bool databaseExists = !string.IsNullOrWhiteSpace(cdbFilePath) && System.IO.File.Exists(cdbFilePath);
                if (databaseExists)
                {
                    if (await DeleteCardDB(cardIdToDelete))
                    {
                        var (resultArchive, messArchive) = await SaveToArchive();
                        if (!resultArchive)
                        {
                            CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                                messArchive, new[] { CMess.ok.ToText() });
                            return;
                        }

                        DeleteCardGrid(cardIdToDelete);
                        IsSaved = true;
                    }
                    else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.Card.ToText()),
                        new[] { CMess.ok.ToText() });
                }
                else
                {
                    DeleteCardGrid(cardIdToDelete);
                    IsSaved = true;
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task<bool> DeleteCardDB(ulong cardId)
        {
            if (cardId <= 0)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardID.ToText()),
                    new[] { CMess.ok.ToText() });
                return false;
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection($"Data Source={cdbFilePath};Version=3;"))
                {
                    await connection.OpenAsync();
                    // Kiểm tra xem id có tồn tại trong database không
                    string checkQuery = "SELECT COUNT(*) FROM texts WHERE id = @id";
                    using (SQLiteCommand checkCommand = new SQLiteCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@id", cardId);
                        long count = (long)await checkCommand.ExecuteScalarAsync();
                        if (count == 0)
                        {
                            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                                CMess.cardNotExit.ToText(), new[] { CMess.ok.ToText() });
                            return false;
                        }
                    }

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string deleteQuery = @"DELETE FROM texts WHERE id = @id; DELETE FROM datas WHERE id = @id;";

                            using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, connection, transaction))
                            {
                                deleteCommand.Parameters.AddWithValue("@id", cardId);
                                await deleteCommand.ExecuteNonQueryAsync();
                            }
                            transaction.Commit();
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }

                    //// Xóa dòng có id đó trong database
                    //string deleteQuery = "DELETE FROM texts WHERE id = @id; DELETE FROM datas WHERE id = @id;";
                    //using (SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, connection))
                    //{
                    //    deleteCommand.Parameters.AddWithValue("@id", cardId);
                    //    await deleteCommand.ExecuteNonQueryAsync();
                    //}
                }
                // return true;
            }
            catch(Exception ex)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return false;
            }
        }
        private void DeleteCardGrid(ulong cardId)
        {
            try
            {
                isDeleting = true;
                var cardToRemove = Cards.FirstOrDefault(c => c.id == cardId);
                if (cardToRemove != null)
                {
                    Cards.Remove(cardToRemove);
                    CurrentCard = null;
                    CollectionViewCollection.Refresh();

                    MessageDelete.Enqueue(string.Format(CMess.ThreePlaceholderSuccess.ToText(), CMess.tlDelete.ToText(), 1.ToString(), CMess.Card.ToText()));

                    var result = CMSG.Show(CMess.conClear.ToText(), CMSG.MessageBoxIconType.Question,
                        string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.tlClear.ToText(), CMess.SelectCards.ToText()),
                        new[] { CMess.yes.ToText(), CMess.no.ToText() });
                    if (result == 0)
                    {
                        ClearAll();
                        isChangedID = false;
                    }
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        CMess.cardIDNotExistList.ToText(), new[] { CMess.ok.ToText() });
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            finally
            {
                isDeleting = false;
            }
        }
        #endregion

        #region Filter
        private Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();
        private void FilterCard()
        {
            CollectionViewCollection.Filter = null;
            FilterCardData();
            CollectionViewCollection.Refresh();
        }
        private void AdvanccedFilterCard()
        {
            if (MainWindowService != null)
            {
                MainWindowService.OpenFinterSetting();
            }
        }
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
        private void FilterCardData()
        {
            if (CollectionViewCollection == null || Cards == null) return;
            int advancedSettings = ConfigViewModel.Instance.dataHandlingSetting.Advanced;
            bool isAdvancedFind = (advancedSettings & 0x01) == 0x01;
            bool matchCase = (advancedSettings & 0x02) == 0x02;
            bool useWildcards = (advancedSettings & 0x04) == 0x04;
            bool matchPrefix = (advancedSettings & 0x08) == 0x08;
            bool matchSuffix = (advancedSettings & 0x10) == 0x10;
            bool wholeWords = (advancedSettings & 0x20) == 0x20;
            bool ignorePunctuation = (advancedSettings & 0x40) == 0x40;
            bool ignoreWhitespace = (advancedSettings & 0x80) == 0x80;

            var filterId = GetCardIDNull();
            var fillerName = GetCardname();
            var fillerDesc = GetCardDesc();
            var fillerSTR1 = GetSTR1();
            var fillerSTR2 = GetSTR2();
            var fillerSTR3 = GetSTR3();
            var fillerSTR4 = GetSTR4();
            var fillerSTR5 = GetSTR5();
            var fillerSTR6 = GetSTR6();
            var fillerSTR7 = GetSTR7();
            var fillerSTR8 = GetSTR8();
            var fillerSTR9 = GetSTR9();
            var fillerSTR10 = GetSTR10();
            var fillerSTR11 = GetSTR11();
            var fillerSTR12 = GetSTR12();
            var fillerSTR13 = GetSTR13();
            var fillerSTR14 = GetSTR14();
            var fillerSTR15 = GetSTR15();
            var fillerSTR16 = GetSTR16();
            var fillerRule = GetCardRule();
            var fillerAlias = GetCardAliasNull();
            var fillerSetCode = GetSetCode();
            var fillerType = GetCardType();
            var fillerATK = GetATKNull();
            var fillerDEF = GetDEFNull();
            var fillerLevel = GetLevelNull();
            var fillerright = GetRightScaleNull();
            var fillerleft = GetLeftScaleNull();
            var fillerRace = GetCardRace();
            var fillerAttribute = GetCardAttribue();
            var fillerCategory = GetCardCategory();
            var fillerFlag = GetCardFlag();

            var normalizedFilters = new Dictionary<string, string>();
            var strFilters = new[] {
                fillerSTR1, fillerSTR2, fillerSTR3, fillerSTR4,
                fillerSTR5, fillerSTR6, fillerSTR7, fillerSTR8,
                fillerSTR9, fillerSTR10, fillerSTR11, fillerSTR12,
                fillerSTR13, fillerSTR14, fillerSTR15, fillerSTR16
            };
            bool hasAnyStrFilter = strFilters.Any(f => !string.IsNullOrWhiteSpace(f));

            if (isAdvancedFind && (ignorePunctuation || ignoreWhitespace))
            {
                foreach (var filter in new[] { fillerName, fillerDesc }.Concat(strFilters))
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        string normalized = filter;
                        if (ignorePunctuation)
                            normalized = new string(normalized.Where(c => !char.IsPunctuation(c)).ToArray());
                        if (ignoreWhitespace)
                            normalized = Regex.Replace(normalized, @"\s+", "");

                        normalizedFilters[filter] = normalized;
                    }
                }
            }

            Dictionary<string, Regex> preCompiledRegex = null;
            if (isAdvancedFind && (useWildcards || wholeWords))
            {
                preCompiledRegex = new Dictionary<string, Regex>();
                foreach (var kvp in normalizedFilters)
                {
                    string pattern = useWildcards
                        ? BuildRegexPattern(kvp.Value, matchPrefix, matchSuffix, wholeWords)
                        : wholeWords ? $@"\b{Regex.Escape(kvp.Value)}\b" : null;

                    if (pattern != null)
                    {
                        preCompiledRegex[kvp.Key] = GetOrCreateRegex(pattern, matchCase);
                    }
                }
            }

            StringComparison comparison = (isAdvancedFind && matchCase)
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            bool MatchString(string source, string pattern)
            {
                if (string.IsNullOrWhiteSpace(pattern)) return true;
                if (string.IsNullOrWhiteSpace(source)) return false;

                string normalizedPattern = normalizedFilters.TryGetValue(pattern, out var cached)
                    ? cached
                    : pattern;

                string normalizedSource = source;
                if (isAdvancedFind)
                {
                    if (ignorePunctuation)
                        normalizedSource = new string(normalizedSource.Where(c => !char.IsPunctuation(c)).ToArray());
                    if (ignoreWhitespace)
                        normalizedSource = Regex.Replace(normalizedSource, @"\s+", "");
                }

                if (!isAdvancedFind)
                {
                    return normalizedSource.IndexOf(normalizedPattern, comparison) >= 0;
                }

                if (useWildcards || wholeWords)
                {
                    if (preCompiledRegex != null && preCompiledRegex.TryGetValue(pattern, out var regex))
                    {
                        return regex.IsMatch(normalizedSource);
                    }

                    string regexPattern = useWildcards
                        ? BuildRegexPattern(normalizedPattern, matchPrefix, matchSuffix, wholeWords)
                        : $@"\b{Regex.Escape(normalizedPattern)}\b";

                    return GetOrCreateRegex(regexPattern, matchCase).IsMatch(normalizedSource);
                }

                int index = normalizedSource.IndexOf(normalizedPattern, comparison);
                if (index < 0) return false;

                if (matchPrefix && index != 0) return false;
                if (matchSuffix && (index + normalizedPattern.Length) != normalizedSource.Length) return false;

                return true;
            }

            if (ConfigViewModel.Instance.dataHandlingSetting.FilterMode == 0) // Pure AND
            {
                CollectionViewCollection.Filter = item =>
                {
                    var card = (CardEditor.Models.Card)item;

                    if (filterId.HasValue && card.id != filterId.Value) return false;
                    if (fillerAlias.HasValue && card.alias != fillerAlias.Value) return false;
                    if (fillerRule != 0 && (card.ot & fillerRule) != fillerRule) return false;
                    if (fillerSetCode != 0 && (card.setcode & fillerSetCode) != fillerSetCode) return false;
                    if (fillerType != 0 && (card.type & fillerType) != fillerType) return false;

                    if (fillerATK.HasValue)
                    {
                        if (fillerATK.Value >= 0 && card.atk != fillerATK.Value) return false;
                        if (fillerATK.Value < 0 && card.atk >= 0) return false;
                    }

                    if (fillerDEF.HasValue)
                    {
                        if (fillerDEF.Value >= 0 && card.def != fillerDEF.Value) return false;
                        if (fillerDEF.Value < 0 && card.def >= 0) return false;
                    }

                    if (fillerLevel.HasValue)
                    {
                        ulong levelValue = IsLinkIntf ? (card.level & 0xFFFF) : (card.level & 0xFF);
                        if (fillerLevel.Value != levelValue) return false;
                    }

                    if (fillerright.HasValue && fillerright.Value != ((card.level >> 16) & 0xFF)) return false;
                    if (fillerleft.HasValue && fillerleft.Value != ((card.level >> 24) & 0xFF)) return false;
                    if (fillerRace != 0 && (card.race & fillerRace) != fillerRace) return false;
                    if (fillerAttribute != 0 && (card.attribute & fillerAttribute) != fillerAttribute) return false;
                    if (fillerCategory != 0 && (card.category & fillerCategory) != fillerCategory) return false;
                    if (fillerFlag != 0 && (card.flag & fillerFlag) != fillerFlag) return false;


                    if (!MatchString(card.name, fillerName)) return false;
                    if (!MatchString(card.desc, fillerDesc)) return false;

                    if (hasAnyStrFilter)
                    {
                        bool anyStrMatches =
                            MatchString(card.str1, fillerSTR1) ||
                            MatchString(card.str2, fillerSTR2) ||
                            MatchString(card.str3, fillerSTR3) ||
                            MatchString(card.str4, fillerSTR4) ||
                            MatchString(card.str5, fillerSTR5) ||
                            MatchString(card.str6, fillerSTR6) ||
                            MatchString(card.str7, fillerSTR7) ||
                            MatchString(card.str8, fillerSTR8) ||
                            MatchString(card.str9, fillerSTR9) ||
                            MatchString(card.str10, fillerSTR10) ||
                            MatchString(card.str11, fillerSTR11) ||
                            MatchString(card.str12, fillerSTR12) ||
                            MatchString(card.str13, fillerSTR13) ||
                            MatchString(card.str14, fillerSTR14) ||
                            MatchString(card.str15, fillerSTR15) ||
                            MatchString(card.str16, fillerSTR16);

                        if (!anyStrMatches) return false;
                    }
                    return true;
                };
            }
            else if (ConfigViewModel.Instance.dataHandlingSetting.FilterMode == 1) // Pure OR
            {
                CollectionViewCollection.Filter = item =>
                {
                    var card = (CardEditor.Models.Card)item;

                    if (filterId.HasValue && card.id == filterId.Value) return true;
                    if (fillerAlias.HasValue && card.alias == fillerAlias.Value) return true;
                    if (fillerRule != 0 && (card.ot & fillerRule) != 0) return true;
                    if (fillerSetCode != 0 && (card.setcode & fillerSetCode) != 0) return true;
                    if (fillerType != 0 && (card.type & fillerType) != 0) return true;

                    if (fillerATK.HasValue)
                    {
                        if (fillerATK.Value >= 0 && card.atk == fillerATK.Value) return true;
                        if (fillerATK.Value < 0 && card.atk < 0) return true;
                    }

                    if (fillerDEF.HasValue)
                    {
                        if (fillerDEF.Value >= 0 && card.def == fillerDEF.Value) return true;
                        if (fillerDEF.Value < 0 && card.def < 0) return true;
                    }

                    if (fillerLevel.HasValue)
                    {
                        ulong levelValue = IsLinkIntf ? (card.level & 0xFFFF) : (card.level & 0xFF);
                        if (fillerLevel.Value == levelValue) return true;
                    }

                    if (fillerright.HasValue && fillerright.Value == ((card.level >> 16) & 0xFF)) return true;
                    if (fillerleft.HasValue && fillerleft.Value == ((card.level >> 24) & 0xFF)) return true;
                    if (fillerRace != 0 && (card.race & fillerRace) != 0) return true;
                    if (fillerAttribute != 0 && (card.attribute & fillerAttribute) != 0) return true;
                    if (fillerCategory != 0 && (card.category & fillerCategory) != 0) return true;
                    if (fillerFlag != 0 && (card.flag & fillerFlag) != 0) return true;

                    if (MatchString(card.name, fillerName)) return true;
                    if (MatchString(card.desc, fillerDesc)) return true;
                    if (MatchString(card.str1, fillerSTR1)) return true;
                    if (MatchString(card.str2, fillerSTR2)) return true;
                    if (MatchString(card.str3, fillerSTR3)) return true;
                    if (MatchString(card.str4, fillerSTR4)) return true;
                    if (MatchString(card.str5, fillerSTR5)) return true;
                    if (MatchString(card.str6, fillerSTR6)) return true;
                    if (MatchString(card.str7, fillerSTR7)) return true;
                    if (MatchString(card.str8, fillerSTR8)) return true;
                    if (MatchString(card.str9, fillerSTR9)) return true;
                    if (MatchString(card.str10, fillerSTR10)) return true;
                    if (MatchString(card.str11, fillerSTR11)) return true;
                    if (MatchString(card.str12, fillerSTR12)) return true;
                    if (MatchString(card.str13, fillerSTR13)) return true;
                    if (MatchString(card.str14, fillerSTR14)) return true;
                    if (MatchString(card.str15, fillerSTR15)) return true;
                    if (MatchString(card.str16, fillerSTR16)) return true;

                    return false;
                };
            }
            else // Mixed
            {
                CollectionViewCollection.Filter = item =>
                {
                    var card = (CardEditor.Models.Card)item;

                    if (filterId.HasValue && card.id != filterId.Value) return false;
                    if (fillerAlias.HasValue && card.alias != fillerAlias.Value) return false;

                    // Bit-flag filters: match ANY bit (OR within group)
                    if (fillerRule != 0 && (card.ot & fillerRule) == 0) return false;
                    if (fillerSetCode != 0 && (card.setcode & fillerSetCode) == 0) return false;
                    if (fillerType != 0 && (card.type & fillerType) == 0) return false;
                    if (fillerRace != 0 && (card.race & fillerRace) == 0) return false;
                    if (fillerAttribute != 0 && (card.attribute & fillerAttribute) == 0) return false;
                    if (fillerCategory != 0 && (card.category & fillerCategory) == 0) return false;
                    if (fillerFlag != 0 && (card.flag & fillerFlag) == 0) return false;

                    // Numeric filters: exact match
                    if (fillerATK.HasValue)
                    {
                        if (fillerATK.Value >= 0 && card.atk != fillerATK.Value) return false;
                        if (fillerATK.Value < 0 && card.atk >= 0) return false;
                    }

                    if (fillerDEF.HasValue)
                    {
                        if (fillerDEF.Value >= 0 && card.def != fillerDEF.Value) return false;
                        if (fillerDEF.Value < 0 && card.def >= 0) return false;
                    }

                    if (fillerLevel.HasValue)
                    {
                        ulong levelValue = IsLinkIntf ? (card.level & 0xFFFF) : (card.level & 0xFF);
                        if (fillerLevel.Value != levelValue) return false;
                    }

                    if (fillerright.HasValue && fillerright.Value != ((card.level >> 16) & 0xFF)) return false;
                    if (fillerleft.HasValue && fillerleft.Value != ((card.level >> 24) & 0xFF)) return false;

                    // String filters
                    if (!MatchString(card.name, fillerName)) return false;
                    if (!MatchString(card.desc, fillerDesc)) return false;

                    // STR fields: OR logic
                    if (hasAnyStrFilter)
                    {
                        bool anyStrMatches =
                            MatchString(card.str1, fillerSTR1) ||
                            MatchString(card.str2, fillerSTR2) ||
                            MatchString(card.str3, fillerSTR3) ||
                            MatchString(card.str4, fillerSTR4) ||
                            MatchString(card.str5, fillerSTR5) ||
                            MatchString(card.str6, fillerSTR6) ||
                            MatchString(card.str7, fillerSTR7) ||
                            MatchString(card.str8, fillerSTR8) ||
                            MatchString(card.str9, fillerSTR9) ||
                            MatchString(card.str10, fillerSTR10) ||
                            MatchString(card.str11, fillerSTR11) ||
                            MatchString(card.str12, fillerSTR12) ||
                            MatchString(card.str13, fillerSTR13) ||
                            MatchString(card.str14, fillerSTR14) ||
                            MatchString(card.str15, fillerSTR15) ||
                            MatchString(card.str16, fillerSTR16);

                        if (!anyStrMatches) return false;
                    }

                    return true;
                };
            }
            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                string.Format(CMess.filterSuc.ToText(), CollectionViewCollection.Cast<CardEditor.Models.Card>().Count().ToString(), Cards.Count.ToString()),
                new[] { CMess.ok.ToText() });
        }

        private void FilterData1()
        {
            int advancedSettings = ConfigViewModel.Instance.dataHandlingSetting.Advanced;
            bool isAdvancedFind = (advancedSettings & 0x01) == 0x01; // bit 1: Advanced Find
            bool matchCase = (advancedSettings & 0x02) == 0x02;     // bit 2: Match Case
            bool useWildcards = (advancedSettings & 0x04) == 0x04;  // bit 3: Use Wildcards
            bool matchPrefix = (advancedSettings & 0x08) == 0x08;   // bit 4: Match Prefix
            bool matchSuffix = (advancedSettings & 0x10) == 0x10;   // bit 5: Match Suffix
            bool wholeWords = (advancedSettings & 0x20) == 0x20;    // bit 6: Find Whole Words Only
            bool ignorePunctuation = (advancedSettings & 0x40) == 0x40; // bit 7: Ignore Punctuation
            bool ignoreWhitespace = (advancedSettings & 0x80) == 0x80;  // bit 8: Ignore White-Space

            var filterId = GetCardIDNull();

            var fillerName = GetCardname();
            var fillerDesc = GetCardDesc();
            var fillerSTR1 = GetSTR1();
            var fillerSTR2 = GetSTR2();
            var fillerSTR3 = GetSTR3();
            var fillerSTR4 = GetSTR4();
            var fillerSTR5 = GetSTR5();
            var fillerSTR6 = GetSTR6();
            var fillerSTR7 = GetSTR7();
            var fillerSTR8 = GetSTR8();
            var fillerSTR9 = GetSTR9();
            var fillerSTR10 = GetSTR10();
            var fillerSTR11 = GetSTR11();
            var fillerSTR12 = GetSTR12();
            var fillerSTR13 = GetSTR13();
            var fillerSTR14 = GetSTR14();
            var fillerSTR15 = GetSTR15();
            var fillerSTR16 = GetSTR16();

            var fillerRule = GetCardRule();
            var fillerAlias = GetCardAliasNull();
            var fillerSetCode = GetSetCode();
            var fillerType = GetCardType();
            var fillerATK = GetATKNull();
            var fillerDEF = GetDEFNull();
            var fillerLevel = GetLevelNull();
            var fillerright = GetRightScaleNull();
            var fillerleft = GetLeftScaleNull();
            var fillerRace = GetCardRace();
            var fillerAttribute = GetCardAttribue();
            var fillerCategory = GetCardCategory();

            var normalizedFilters = new Dictionary<string, string>();
            var strFilters = new[] {
                fillerSTR1, fillerSTR2, fillerSTR3, fillerSTR4,
                fillerSTR5, fillerSTR6, fillerSTR7, fillerSTR8,
                fillerSTR9, fillerSTR10, fillerSTR11, fillerSTR12,
                fillerSTR13, fillerSTR14, fillerSTR15, fillerSTR16
            };
            bool hasAnyStrFilter = strFilters.Any(f => !string.IsNullOrWhiteSpace(f));

            // Pre-normalize tất cả filter strings
            if (isAdvancedFind && (ignorePunctuation || ignoreWhitespace))
            {
                foreach (var filter in new[] { fillerName, fillerDesc }.Concat(strFilters))
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        string normalized = filter;
                        if (ignorePunctuation)
                            normalized = new string(normalized.Where(c => !char.IsPunctuation(c)).ToArray());
                        if (ignoreWhitespace)
                            normalized = Regex.Replace(normalized, @"\s+", "");

                        normalizedFilters[filter] = normalized;
                    }
                }
            }

            // Pre-compile regex patterns nếu dùng wildcards hoặc whole words
            Dictionary<string, Regex> preCompiledRegex = null;
            if (isAdvancedFind && (useWildcards || wholeWords))
            {
                preCompiledRegex = new Dictionary<string, Regex>();
                foreach (var kvp in normalizedFilters)
                {
                    string pattern = useWildcards
                        ? BuildRegexPattern(kvp.Value, matchPrefix, matchSuffix, wholeWords)
                        : wholeWords ? $@"\b{Regex.Escape(kvp.Value)}\b" : null;

                    if (pattern != null)
                    {
                        preCompiledRegex[kvp.Key] = GetOrCreateRegex(pattern, matchCase);
                    }
                }
            }

            // String comparison type
            StringComparison comparison = (isAdvancedFind && matchCase)
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            // Optimized MatchString - closure captures all pre-processed data
            bool MatchString(string source, string pattern)
            {
                if (string.IsNullOrWhiteSpace(pattern)) return true;
                if (string.IsNullOrWhiteSpace(source)) return false;

                // Get normalized pattern
                string normalizedPattern = normalizedFilters.TryGetValue(pattern, out var cached)
                    ? cached
                    : pattern;

                // Normalize source
                string normalizedSource = source;
                if (isAdvancedFind)
                {
                    if (ignorePunctuation)
                        normalizedSource = new string(normalizedSource.Where(c => !char.IsPunctuation(c)).ToArray());
                    if (ignoreWhitespace)
                        normalizedSource = Regex.Replace(normalizedSource, @"\s+", "");
                }

                // Fast path: no advanced options
                if (!isAdvancedFind)
                {
                    return normalizedSource.IndexOf(normalizedPattern, comparison) >= 0;
                }

                // Advanced find với wildcards hoặc whole words
                if (useWildcards || wholeWords)
                {
                    if (preCompiledRegex != null && preCompiledRegex.TryGetValue(pattern, out var regex))
                    {
                        return regex.IsMatch(normalizedSource);
                    }

                    // Fallback nếu không có pre-compiled regex
                    string regexPattern = useWildcards
                        ? BuildRegexPattern(normalizedPattern, matchPrefix, matchSuffix, wholeWords)
                        : $@"\b{Regex.Escape(normalizedPattern)}\b";

                    return GetOrCreateRegex(regexPattern, matchCase).IsMatch(normalizedSource);
                }

                // Simple string matching với prefix/suffix
                int index = normalizedSource.IndexOf(normalizedPattern, comparison);
                if (index < 0) return false;

                if (matchPrefix && index != 0) return false;
                if (matchSuffix && (index + normalizedPattern.Length) != normalizedSource.Length) return false;

                return true;
            }

            CollectionViewCollection.Filter = item =>
            {
                var card = (CardEditor.Models.Card)item;

                // Fast checks first - numeric và bitwise operations (cheapest)
                if (filterId.HasValue && card.id != filterId.Value) return false;
                if (fillerAlias.HasValue && card.alias != fillerAlias.Value) return false;
                if (fillerRule != 0 && (card.ot & fillerRule) != fillerRule) return false;
                if (fillerSetCode != 0 && (card.setcode & fillerSetCode) != fillerSetCode) return false;
                if (fillerType != 0 && (card.type & fillerType) != fillerType) return false;

                // Numeric comparisons
                if (fillerATK.HasValue)
                {
                    if (fillerATK.Value >= 0)
                    {
                        if (card.atk != fillerATK.Value) return false;
                    }
                    else
                    {
                        if (card.atk >= 0) return false;
                    }
                }

                if (fillerDEF.HasValue)
                {
                    if (fillerDEF.Value >= 0)
                    {
                        if (card.def != fillerDEF.Value) return false;
                    }
                    else
                    {
                        if (card.def >= 0) return false;
                    }
                }

                if (fillerLevel.HasValue)
                {
                    ulong levelValue = IsLinkIntf ? (card.level & 0xFFFF) : (card.level & 0xFF);
                    if (fillerLevel.Value != levelValue) return false;
                }

                if (fillerright.HasValue && fillerright.Value != ((card.level >> 16) & 0xFF)) return false;
                if (fillerleft.HasValue && fillerleft.Value != ((card.level >> 24) & 0xFF)) return false;
                if (fillerRace != 0 && (card.race & fillerRace) != fillerRace) return false;
                if (fillerAttribute != 0 && (card.attribute & fillerAttribute) != fillerAttribute) return false;
                if (fillerCategory != 0 && (card.category & fillerCategory) != fillerCategory) return false;

                // String matching (more expensive) - check simpler ones first
                if (!MatchString(card.name, fillerName)) return false;
                if (!MatchString(card.desc, fillerDesc)) return false;

                // STR fields với OR logic - short circuit khi tìm thấy match đầu tiên
                if (hasAnyStrFilter)
                {
                    bool anyStrMatches =
                        MatchString(card.str1, fillerSTR1) ||
                        MatchString(card.str2, fillerSTR2) ||
                        MatchString(card.str3, fillerSTR3) ||
                        MatchString(card.str4, fillerSTR4) ||
                        MatchString(card.str5, fillerSTR5) ||
                        MatchString(card.str6, fillerSTR6) ||
                        MatchString(card.str7, fillerSTR7) ||
                        MatchString(card.str8, fillerSTR8) ||
                        MatchString(card.str9, fillerSTR9) ||
                        MatchString(card.str10, fillerSTR10) ||
                        MatchString(card.str11, fillerSTR11) ||
                        MatchString(card.str12, fillerSTR12) ||
                        MatchString(card.str13, fillerSTR13) ||
                        MatchString(card.str14, fillerSTR14) ||
                        MatchString(card.str15, fillerSTR15) ||
                        MatchString(card.str16, fillerSTR16);

                    if (!anyStrMatches) return false;
                }

                return true;
            };

            CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
        string.Format(CMess.filterSuc.ToText(),
            CollectionViewCollection.Cast<CardEditor.Models.Card>().Count().ToString(),
            Cards.Count.ToString()),
        new[] { CMess.ok.ToText() });



            //CollectionViewCollection.Filter = item =>
            //{
            //    var card = (CardEditor.Models.Card)item;

            //    // Kiểm tra các điều kiện lọc
            //    bool matchesId = !filterId.HasValue || card.id == filterId;
            //    bool matchesAlias = !fillerAlias.HasValue || card.alias == fillerAlias;

            //    bool matchesRule = fillerRule == 0 || (card.ot & fillerRule) == fillerRule; 
            //    bool matchesSetCode = fillerSetCode == 0 || (card.setcode & fillerSetCode) == fillerSetCode;
            //    bool matchesType = fillerType == 0 || (card.type & fillerType) == fillerType;

            //    bool matchesATK = !fillerATK.HasValue || (fillerATK.Value >= 0 ? card.atk == fillerATK.Value : card.atk < 0);
            //    bool matchesDEF = !fillerDEF.HasValue || (fillerDEF.Value >= 0 ? card.def == fillerDEF.Value : card.def < 0);
                
            //    // bool matchesLevel = !fillerLevel.HasValue || fillerLevel == (card.level & 0xFF);

            //    bool matchesLevel = !fillerLevel.HasValue || (IsLink ? fillerLevel == (card.level & 0xFFFF) : fillerLevel == (card.level & 0xFF));


            //    bool matchesRight = !fillerright.HasValue || fillerright == ((card.level >> 16) & 0xFF);
            //    bool matchesLeft = !fillerleft.HasValue || fillerleft == ((card.level >> 24) & 0xFF);

            //    bool matchesRace = fillerRace == 0 || (card.race & fillerRace) == fillerRace;
            //    bool matchesAttri = fillerAttribute == 0 || (card.attribute & fillerAttribute) == fillerAttribute;
            //    bool matchesCategory = fillerCategory == 0 || (card.category & fillerCategory) == fillerCategory;

            //    // Hàm phụ để xử lý so sánh chuỗi với các tùy chọn nâng cao
            //    bool MatchString(string source, string pattern)
            //    {
            //        if (string.IsNullOrWhiteSpace(pattern)) return true;

            //        // Chuẩn hóa chuỗi trước khi so sánh
            //        string normalizedSource = source ?? "";
            //        string normalizedPattern = pattern ?? "";
            //        if (isAdvancedFind)
            //        {
            //            if (ignorePunctuation)
            //            {
            //                normalizedSource = new string(normalizedSource.Where(c => !char.IsPunctuation(c)).ToArray());
            //                normalizedPattern = new string(normalizedPattern.Where(c => !char.IsPunctuation(c)).ToArray());
            //            }
            //            if (ignoreWhitespace)
            //            {
            //                normalizedSource = normalizedSource.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
            //                normalizedPattern = normalizedPattern.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
            //            }
            //        }

            //        // Xác định cách so sánh (phân biệt hoa thường hay không)
            //        StringComparison comparison = (isAdvancedFind && matchCase)
            //            ? StringComparison.Ordinal
            //            : StringComparison.OrdinalIgnoreCase;

            //        if (!isAdvancedFind)
            //        {
            //            // Logic mặc định khi Advanced Find tắt
            //            return normalizedSource.IndexOf(normalizedPattern, comparison) >= 0;
            //        }

            //        // Xử lý khi Advanced Find bật
            //        if (useWildcards)
            //        {
            //            // Chuyển wildcard sang regex pattern
            //            string regexPattern = Regex.Escape(normalizedPattern)
            //                .Replace("\\*", ".*")
            //                .Replace("\\?", ".");

            //            // Áp dụng các điều kiện bổ sung nếu có
            //            if (matchPrefix) regexPattern = "^" + regexPattern;
            //            if (matchSuffix) regexPattern += "$";
            //            if (wholeWords) regexPattern = $"\\b{regexPattern}\\b";

            //            return Regex.IsMatch(normalizedSource, regexPattern, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
            //        }
            //        else
            //        {
            //            // Không dùng wildcard, xử lý các điều kiện khác
            //            bool matches = normalizedSource.IndexOf(normalizedPattern, comparison) >= 0;

            //            if (matchPrefix) matches = matches && normalizedSource.StartsWith(normalizedPattern, comparison);
            //            if (matchSuffix) matches = matches && normalizedSource.EndsWith(normalizedPattern, comparison);
            //            if (wholeWords)
            //            {
            //                string regexPattern = $"\\b{Regex.Escape(normalizedPattern)}\\b";
            //                matches = Regex.IsMatch(normalizedSource, regexPattern, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
            //            }

            //            return matches;
            //        }
            //    }

            //    bool matchesName = MatchString(card.name, fillerName);
            //    bool matchesDesc = MatchString(card.desc, fillerDesc);
            //    bool matchesStr1 = MatchString(card.str1, fillerSTR1);
            //    bool matchesStr2 = MatchString(card.str2, fillerSTR2);
            //    bool matchesStr3 = MatchString(card.str3, fillerSTR3);
            //    bool matchesStr4 = MatchString(card.str4, fillerSTR4);
            //    bool matchesStr5 = MatchString(card.str5, fillerSTR5);
            //    bool matchesStr6 = MatchString(card.str6, fillerSTR6);
            //    bool matchesStr7 = MatchString(card.str7, fillerSTR7);
            //    bool matchesStr8 = MatchString(card.str8, fillerSTR8);
            //    bool matchesStr9 = MatchString(card.str9, fillerSTR9);
            //    bool matchesStr10 = MatchString(card.str10, fillerSTR10);
            //    bool matchesStr11 = MatchString(card.str11, fillerSTR11);
            //    bool matchesStr12 = MatchString(card.str12, fillerSTR12);
            //    bool matchesStr13 = MatchString(card.str13, fillerSTR13);
            //    bool matchesStr14 = MatchString(card.str14, fillerSTR14);
            //    bool matchesStr15 = MatchString(card.str15, fillerSTR15);
            //    bool matchesStr16 = MatchString(card.str16, fillerSTR16);

            //    return matchesId && matchesAlias && matchesName && matchesDesc &&
            //        (matchesStr1 || matchesStr2 || matchesStr3 || matchesStr4 || matchesStr5 || matchesStr6 ||
            //         matchesStr7 || matchesStr8 || matchesStr9 || matchesStr10 || matchesStr11 || matchesStr12 ||
            //         matchesStr13 || matchesStr14 || matchesStr15 || matchesStr16) &&
            //         matchesRule && matchesSetCode && matchesType && matchesATK && matchesDEF &&
            //         matchesLevel && matchesRight && matchesLeft && matchesRace && matchesAttri && matchesCategory;
            //};

            //CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
            //    string.Format(CMess.filterSuc.ToText(), CollectionViewCollection.Cast<CardEditor.Models.Card>().Count().ToString(), Cards.Count.ToString()),
            //    new[] { CMess.ok.ToText() });
        }
        private void ClearRegexCache()
        {
            _regexCache.Clear();
        }

        public async Task FilterDuplicateDataFromYdkFile(bool isDuplicate)
        {
            if(CollectionViewCollection == null || Cards == null || !Cards.Any())
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
                        CollectionViewCollection.Filter = item =>
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
                        CollectionViewCollection.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return !cardIds.Contains(card.id);
                        };
                    });
                }

                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.filterSuc.ToText(), CollectionViewCollection.Cast<CardEditor.Models.Card>().Count().ToString(), Cards.Count.ToString()),
                    new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        public async Task FilterDuplicateDataFromCdbFile(bool isDuplicate)
        {
            if (CollectionViewCollection == null || Cards == null || !Cards.Any())
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, CMess.noCardFilter.ToText(), new[] { CMess.ok.ToText() });
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
                        CollectionViewCollection.Filter = item =>
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
                        CollectionViewCollection.Filter = item =>
                        {
                            var card = (CardEditor.Models.Card)item;
                            return !cardIds.Contains(card.id);
                        };
                    });
                }

                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.filterSuc.ToText(), CollectionViewCollection.Cast<CardEditor.Models.Card>().Count().ToString(), Cards.Count.ToString()),
                    new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }


        public async Task<ResultItem> FilterCardByLanguage(int languageCode, bool isInclude)
        {
            if (CollectionViewCollection == null || Cards == null || !Cards.Any())
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
                ClearFilterCard();

                // Chọn property selector theo languageCode — chỉ 1 switch, không nhân đôi
                Func<DetectionResult, bool> languageCheck;

                switch (languageCode)
                {
                    case 0: languageCheck = r => r.ContainsEnglishOrLatin; break;
                    case 1: languageCheck = r => r.ContainsVietnamese; break;
                    case 2: languageCheck = r => r.ContainsChinese; break;
                    case 3: languageCheck = r => r.ContainsJapanese; break;
                    case 4: languageCheck = r => r.ContainsKorean; break;
                    default:
                        return new ResultItem
                        {
                            Succeeded = false,
                            FilteredCount = 0,
                            TotalCount = Cards.Count,
                            Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                        };
                }

                // ✅ Pre-compute: Detect() chạy MỘT LẦN cho mỗi card, không lặp lại mỗi lần filter re-evaluate
                var cache = Cards.ToDictionary(
                    card => card,
                    card => languageCheck(MixedLanguageDetector.Detect(card.desc))
                );

                // Predicate chỉ tra cứu dictionary → O(1), không tính toán lại
                CollectionViewCollection.Filter = item =>
                {
                    var card = (CardEditor.Models.Card)item;
                    return cache.TryGetValue(card, out var match)
                        ? (isInclude ? match : !match)
                        : false;
                };

                return new ResultItem
                {
                    Succeeded = true,
                    FilteredCount = CollectionViewCollection.Cast<CardEditor.Models.Card>().Count(),
                    TotalCount = Cards.Count,
                    Message = string.Empty
                };
            }
            catch (Exception ex)
            {
                return new ResultItem
                {
                    Succeeded = false,
                    FilteredCount = 0,
                    TotalCount = Cards.Count,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResultItem> FilterCardByPenLang(PendulumLanguageRule rule, bool isInclude)
        {
            if (CollectionViewCollection == null || Cards == null || !Cards.Any())
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
                ClearFilterCard();

                var analyzer = PenLanguageViewModel.Instance;
                // Analyze chỉ chạy đúng một lần cho mỗi card
                var cache = Cards.ToDictionary(card => card, card =>
                {
                    // Không phải Pendulum -> luôn false
                    if ((card.type & (ulong)CardType.Pendulum) == 0)
                        return false;

                    var analysis = analyzer.Analyze(card.desc);

                    if (analysis.RuleResult == null)
                        return !isInclude;

                    bool isMatch = ReferenceEquals(analysis.RuleResult, rule);

                    return isInclude ? isMatch : !isMatch;
                });

                CollectionViewCollection.Filter = item =>
                {
                    var card = (CardEditor.Models.Card)item;
                    return cache.TryGetValue(card, out var match) && match;
                };

                return new ResultItem
                {
                    Succeeded = true,
                    FilteredCount = CollectionViewCollection.Cast<CardEditor.Models.Card>().Count(),
                    TotalCount = Cards.Count,
                    Message = string.Empty
                };
            }
            catch (Exception ex)
            {
                return new ResultItem
                {
                    Succeeded = false,
                    FilteredCount = 0,
                    TotalCount = Cards.Count,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResultItem> FilterCardByYDKFile(string filePath, bool isDuplicate)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return new ResultItem
                {
                    Succeeded = false,
                    FilteredCount = 0,
                    TotalCount = Cards?.Count ?? 0,
                    Message = CMess.noFileFound.ToText()
                };
            }

            if (CollectionViewCollection == null || Cards == null || !Cards.Any())
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
                    TotalCount = Cards.Count,
                    Message = ex.Message
                };
            }
        }
        public async Task<ResultItem> FilterCardByCDBFile(string filePath, bool isDuplicate)
        {

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return new ResultItem
                {
                    Succeeded = false,
                    FilteredCount = 0,
                    TotalCount = Cards?.Count ?? 0,
                    Message = CMess.noFileFound.ToText()
                };
            }

            if (CollectionViewCollection == null || Cards == null || !Cards.Any())
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
                    TotalCount = Cards.Count,
                    Message = ex.Message
                };
            }
        }
        private ResultItem FilterCardByListID(HashSet<ulong> ids, bool isDuplicate)
        {
            try
            {
                ClearFilterCard();

                CollectionViewCollection.Filter = item =>
                {
                    var card = (CardEditor.Models.Card)item;
                    return isDuplicate == ids.Contains(card.id);
                };

                return new ResultItem
                {
                    Succeeded = true,
                    FilteredCount = CollectionViewCollection.Cast<CardEditor.Models.Card>().Count(),
                    TotalCount = Cards.Count,
                    Message = string.Empty
                };
            }
            catch (Exception ex)
            {
                return new ResultItem
                {
                    Succeeded = false,
                    FilteredCount = 0,
                    TotalCount = Cards.Count,
                    Message = ex.Message
                };
            }
        }
        private void ClearFilterCard()
        {
            CollectionViewCollection.Filter = null;
            CollectionViewCollection.Refresh();
        }
        #endregion

        #region Image
        private async Task CreateImage()
        {
            if(CurrentCard == null)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.noCardSelec.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            try
            {
                #region Validate Image Data
                if (ImageValidate.isValid == false ||
                    GeneraImageViewModel.Instance.IsChangedSeries == false ||
                    GeneraImageViewModel.Instance.IsLoadedImageCache == false ||
                    RareRawDataViewModel.Instance.IsLoadedImageRareCache == false ||
                    RareRawDataViewModel.Instance.IsLoadedImageRareRect == false)
                {
                    var (checkResult, messResult) = await ImageValidate.ReLoadImageData();

                    if(!checkResult)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{CMess.errorLoadImageCache.ToText()} {messResult}", new[] { CMess.ok.ToText() });
                        return;
                    }
                }
                var (frameResult, frameMessage) = await ImageValidate.ReloadFrameImageCache();
                if (!frameResult)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorLoadImageCache.ToText()} {frameMessage}", new[] { CMess.ok.ToText() });
                    return;
                }
                #endregion

                #region OutPutPath
                string outPutPath = string.Empty;
                if (!string.IsNullOrWhiteSpace(cdbFilePath) && System.IO.File.Exists(cdbFilePath))
                    outPutPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(cdbFilePath), "picsGene");
                else outPutPath = ConfigViewModel.Instance.imageSetting.OutPutFolder;
                if(!Directory.Exists(outPutPath)) Directory.CreateDirectory(outPutPath);
                #endregion

                #region Generate Image
                var (result, ImgUrl) = await CreateSingleImageDataEditor(outPutPath);
                BitmapImage bitmap;

                if (result && !string.IsNullOrWhiteSpace(ImgUrl) && System.IO.File.Exists(ImgUrl))
                {
                    HiddenArtWork();
                    byte[] imgBytes = await Task.Run(() => System.IO.File.ReadAllBytes(ImgUrl));
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
                        imagecard.Source = bitmap;
                        ViewportImage.ToolTip = ImgUrl;
                    });
                    ImageUrl = ImgUrl;
                    MessageNotifi.Enqueue(string.Format(CMess.FourPlaceholderSuccess.ToText(), 1.ToString(), CMess.Card.ToText(), CMess.Image.ToText(), CMess.Create.ToText()));
                }
                else
                {
                    SetImageCardDefault();
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Create.ToText(), CMess.Image.ToText())} {ImgUrl}",
                        new[] { CMess.ok.ToText() });
                }
                #endregion
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Create.ToText(), CMess.Image.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                SetImageCardDefault();
            }
        }

        #region Image Card
        private void SetImageCardToNull()
        {
            imagecard.Source = null;
            ImageUrl = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        private void SetImageCardDefault()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                imagecard.Source = ImageCacheService.Instance.Get(AppImage.Blank);
                ViewportImage.ToolTip = CMess.toolCardImg.ToText();
                ImageUrl = null;
            });

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        private async void imagecard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickTime = DateTime.Now;
            var timeSinceLastClick = (clickTime - lastClickTime).TotalMilliseconds;
            if (timeSinceLastClick <= DOUBLE_CLICK_TIME)
            {
                await SelectImage();
            }
            lastClickTime = clickTime;
        }
        private async Task SelectImage()
        {
            if (!string.IsNullOrWhiteSpace(txtid.Text))
            {
                string filePath = FileDiaLogHelper.OpenImage();

                if (!string.IsNullOrEmpty(filePath))
                {
                    string selectedFilePath = filePath;
                    await CreatImage(selectedFilePath);
                }
            }
            else
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardID.ToText()),
                    new[] { CMess.ok.ToText() });
            }
        }
        private async Task CreatImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(txtid.Text))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardID.ToText()),
                    new[] { CMess.ok.ToText() });
                return;
            }

            try
            {
                SetImageCardToNull();

                string passWord = string.Empty, targetPath = string.Empty;

                if (string.IsNullOrWhiteSpace(cdbFilePath) || !System.IO.File.Exists(cdbFilePath))
                {
                    string folderPath = FileDiaLogHelper.OpenFolder();

                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        targetPath = folderPath;
                    }
                    else return;
                }
                else targetPath = System.IO.Path.GetDirectoryName(cdbFilePath);

                if (CurrentCard != null)
                {
                    if (isChangedID)
                    {
                        var chooseID = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                            $"{CMess.idChanged} {CMess.quesSelectCreaImg.ToText()}",
                            new[] { CMess.SelectCards.ToText(), CMess.newIDCard.ToText() });

                        passWord = chooseID == 0 ? CurrentCard.id.ToString() : txtid.Text;
                    }
                    else passWord = CurrentCard.id.ToString();
                }
                else passWord = txtid.Text;

                if (!ImageValidate.isValid)
                {
                    var (checkResult, checkMessage) = await ImageValidate.CheckValidate();
                    if (!checkResult)
                    {
                        CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                            $"{string.Format(CMess.PlaceholderInva.ToText(), CMess.Setting.ToText())} {checkMessage}", new[] { CMess.ok.ToText() });
                        SetImageCardDefault();
                        return;
                    }
                }

                string imageSource = await ImageProcessor.CreateImageCard(passWord, imagePath, targetPath);

                if (!string.IsNullOrWhiteSpace(imageSource) && System.IO.File.Exists(imageSource))
                {
                    byte[] imgBytes = await Task.Run(() => System.IO.File.ReadAllBytes(imageSource));
                    var bitmap = new BitmapImage();
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
                        ViewportImage.ToolTip = imageSource;
                    });
                    ImageUrl = imageSource;
                }
                else
                {
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        SetImageCardDefault();
                    });
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SetImageCardDefault();
                });
            }
        }
        private void imagecard_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Copy;
            }
        }
        private async void imagecard_Drop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0)
            {
                string filePath = files[0];
                if (IsImageFile(filePath))
                {
                    await CreatImage(filePath);
                }
                else
                {
                    CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                        CMess.notImage.ToText(), new[] { CMess.ok.ToText() });
                }
            }
        }
        #endregion

        #region Artwork Card 
        private void SetArtWorkToNull()
        {
            ImgArtWorkFull.Source = null;
            ImgArtWorkNor.Source = null;
            ImgArtWorkPen.Source = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        private void SetArtWorkDefault()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ImgArtWorkFull.Source = ImageCacheService.Instance.Get(AppImage.Logo);
                ImgArtWorkNor.Source = ImageCacheService.Instance.Get(AppImage.Logo);
                ImgArtWorkPen.Source = ImageCacheService.Instance.Get(AppImage.Logo);
            });
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void HiddenArtWork()
        {
            grArtWork.Visibility = Visibility.Collapsed;
        }
        private void ShowArtWork()
        {
            grArtWork.Visibility = Visibility.Visible;
        }
        private async void blArtwork_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickTime = DateTime.Now;
            var timeSinceLastClick = (clickTime - lastClickTime).TotalMilliseconds;
            if (timeSinceLastClick <= DOUBLE_CLICK_TIME)
            {
                await SelectArtwork();
            }
            lastClickTime = clickTime;
        }
        private async Task SelectArtwork()
        {
            if (!string.IsNullOrWhiteSpace(txtid.Text))
            {
                string artWorkFolderPath = ConfigViewModel.Instance.imageSetting.ArtworkFolder;
                if (!string.IsNullOrEmpty(artWorkFolderPath) && System.IO.Directory.Exists(artWorkFolderPath))
                {
                    string filePath = FileDiaLogHelper.OpenImage();

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        string selectedFilePath = filePath;
                        await CreateArtWork(selectedFilePath, artWorkFolderPath);
                    }
                }
                else
                {
                    if (MainWindowService != null)
                    {
                        MainWindowService.OpenCreateImage();
                    }
                }
            }
            else CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardID.ToText()),
                new[] { CMess.ok.ToText() });
        }
        private async Task CreateArtWork(string imagePath, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(txtid.Text))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Card.ToText(), CMess.cardID.ToText()),
                    new[] { CMess.ok.ToText() });
                return;
            }

            try
            {
                SetArtWorkToNull();

                string passWord = string.Empty;
                if (CurrentCard != null)
                {
                    if (isChangedID)
                    {
                        var chooseID = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question, $"{CMess.idChanged} {CMess.quesSelectCreaImg.ToText()}",
                                new[] { CMess.SelectCards.ToText(), CMess.newIDCard.ToText() });
                        passWord = chooseID == 0 ? CurrentCard.id.ToString() : txtid.Text;
                    }
                    else passWord = CurrentCard.id.ToString();
                }
                else passWord = txtid.Text;

                BitmapImage bitmap;
                var (resultArtWork, messageArtWork) = await ImageGenerator.CopyArtWork(passWord, imagePath, targetPath);
                if (resultArtWork)
                {
                    if (!string.IsNullOrEmpty(messageArtWork) && System.IO.File.Exists(messageArtWork))
                    {
                        byte[] imgBytes = await Task.Run(() => System.IO.File.ReadAllBytes(messageArtWork));
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
                            ImgArtWorkFull.Source = bitmap;
                            ImgArtWorkNor.Source = bitmap;
                            ImgArtWorkPen.Source = bitmap;
                            ShowArtWork();
                        });
                    }
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {messageArtWork}", new[] { CMess.ok.ToText() });
                    SetArtWorkDefault();
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                SetArtWorkDefault();
            }
        }
        private void blArtwork_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Copy;
            }
        }
        private async void blArtwork_Drop(object sender, DragEventArgs e)
        {
            string artWorkFolderPath = ConfigViewModel.Instance.imageSetting.ArtworkFolder;
            if (!string.IsNullOrEmpty(artWorkFolderPath) && System.IO.Directory.Exists(artWorkFolderPath))
            {
                if (MainWindowService != null)
                {
                    MainWindowService.OpenCreateImage();
                    return;
                }
            }
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0)
            {
                string filePath = files[0];
                if (IsImageFile(filePath))
                {
                    await CreateArtWork(filePath, artWorkFolderPath);
                }
                else
                {
                    CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                        CMess.notImage.ToText(), new[] { CMess.ok.ToText() });
                }
            }
        }
        #endregion

        private bool IsImageFile(string filePath)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".tiff" };
            string fileExtension = System.IO.Path.GetExtension(filePath).ToLower();
            return Array.Exists(imageExtensions, ext => ext == fileExtension);
        }
        private void ViewImage()
        {
            if (!string.IsNullOrWhiteSpace(ImageUrl) && System.IO.File.Exists(ImageUrl))
            {
                if (MainWindowService != null)
                {
                    MainWindowService.OpenViewImage(ImageUrl);
                }
            }
            else
            {
                ///
            }
        }
        private void OpenFileImage()
        {
            if (!string.IsNullOrWhiteSpace(ImageUrl) && System.IO.File.Exists(ImageUrl))
            {
                if (MainWindowService != null)
                {
                    MainWindowService.OpenFileLocation(ImageUrl);
                }
            }
        }
        #endregion

        #region Create Image
        public async Task<(bool,string)> CreateSingleImageDataEditor(string outPutPath)
        {
            try
            {
                ImageGenerator.outputFolderPath = outPutPath;
                if (ConfigViewModel.Instance.imageSetting.Series == 4)
                {
                    var (resultImage, Imagepath) = await ImageGenerator.GenerateImage10(CurrentCard);
                    return (resultImage, Imagepath);
                }
                else return (false, "This series is not yet supported.");
                
                /// Khi nào rảnh thì làm thêm ảnh

            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool, string)> CreateMultiImageDataEditor(int Scope, int Series, Action<int, int> onProgress = null)
        {
            #region OutPutPath
            string outPutPath = string.Empty;
            if (!string.IsNullOrWhiteSpace(cdbFilePath) && System.IO.File.Exists(cdbFilePath))
                outPutPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(cdbFilePath), "picsGene");
            else outPutPath = ConfigViewModel.Instance.imageSetting.OutPutFolder;
            if (!Directory.Exists(outPutPath)) Directory.CreateDirectory(outPutPath);
            ImageGenerator.outputFolderPath = outPutPath;
            #endregion

            int successCount = 0;
            int failCount = 0;
            var failedIds = new List<ulong>();
            _ctsCreateImg?.Cancel();
            _ctsCreateImg = new CancellationTokenSource();
            var token = _ctsCreateImg.Token;

            try
            {
                IEnumerable<CardEditor.Models.Card> cardsToProcess = null;

                if (Series == 0)
                {
                    if (Scope == 0)
                    {
                        cardsToProcess = datagrMain.SelectedItems.OfType<CardEditor.Models.Card>();
                    }
                    else if (Scope == 1)
                    {
                        cardsToProcess = CollectionViewCollection.Cast<CardEditor.Models.Card>();
                    }
                    else if (Scope == 2)
                    {
                        cardsToProcess = Cards;
                    }
                    else
                    {
                        return (false, string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText()));
                    }

                    if(cardsToProcess != null && cardsToProcess.Any())
                    {
                        var total = cardsToProcess.Count();
                        int processed = 0;
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
                                    var (result, imageURL) = await ImageGenerator.GenerateImage10(cardItem);
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
                                    Interlocked.Increment(ref processed);
                                    onProgress?.Invoke(processed, total);
                                    semaphore.Release();
                                }
                            }, token);
                            tasks.Add(task);
                        }
                        try
                        {
                            await Task.WhenAll(tasks);
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
                        messageBuilder.AppendLine(
                            string.Format(CMess.FourPlaceholderSuccess.ToText(), successCount.ToString(), CMess.Card.ToText(), CMess.Image.ToText(), CMess.Create.ToText()));
                        if (failCount > 0)
                        {
                            messageBuilder.AppendLine(String.Format(CMess.geneImgFail.ToText(), failCount.ToString()));
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
                        return (failCount == 0, messageBuilder.ToString());
                    }
                    else
                    {
                        return (false, CMess.noCardFound.ToText());
                    }
                }
                else
                {
                    return (false, string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText()));
                }
            }
            catch (Exception ex)
            {
                return (false, $"{CMess.errorOcc.ToText()} {ex.Message}");
            }
        }
        public void CancelCreateImageDataEditor()
        {
            _ctsCreateImg?.Cancel();
        }

        private void blCreateImage_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            popCreateImage.IsOpen = true;
            e.Handled = true;
        }
        private void btnCloseImageSetting_Click(object sender, RoutedEventArgs e)
        {
            popCreateImage.IsOpen = false;
        }
        #endregion

        #region Web
        private async Task OpenKonamiDB()
        {
            if (CurrentCard == null) return;
            if (MainWindowService != null)
            {
                await MainWindowService.OpenKonamiDB(CurrentCard.id, CurrentCard.name);
            }
        }
        private async Task OpenYugipedia()
        {
            if (CurrentCard == null) return;
            if (MainWindowService != null)
            {
                await MainWindowService.OpenYugipedia(CurrentCard.id, CurrentCard.name);
            }
        }
        private async Task OpenYGOResources()
        {
            if (CurrentCard == null) return;
            if (MainWindowService != null)
            {
                await MainWindowService.OpenYGOResources(CurrentCard.id, CurrentCard.name);
            }
        }
        #endregion

        #region Change
        private async void datagrMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isDeleting) return;
            isChangedGrid = true;
            try
            {
                if (datagrMain.SelectedItem is CardEditor.Models.Card selectedCard)
                {
                    CurrentCard = selectedCard;
                    LoadCardData();
                    LoadCardRare();
                    LoadGenesysPoint();
                    await LoadCardImage();
                }
                else
                {
                    CurrentCard = null;
                    ClearAll();
                }
                LoadPendulumLanguage();
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            HiddenArtWork();
            isChangedID = false;
            isChangedGrid = false;
            ResetUndoRedoCommand();
        }
        private void DataGridCell_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
        private void txtid_TextChanged(object sender, TextChangedEventArgs e)
        {
            isChangedID = true;
        }
        public void FocusCardById(ulong cardID)
        {
            var foundCard = Cards.FirstOrDefault(c => c.id == cardID);
            if (foundCard == null) return;

            CurrentCard = foundCard;
            datagrMain.SelectedItem = foundCard;
            datagrMain.ScrollIntoView(foundCard);

            datagrMain.UpdateLayout();
            datagrMain.Focus();
        }
        private void linkCenter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetLinkArrow(false);
        }

        private void UpdateWindowSavedFlag()
        {
            if (MainWindowService != null)
            {
                MainWindowService.UpdateWindowSavedFlag(IsSaved);
            }
        }

        #region Visibility
        private void cmbrule_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if(cmbrule.SelectedItems != null && cmbrule.SelectedItems.Count > 0)
                tbrule.Visibility = Visibility.Collapsed;
            else tbrule.Visibility = Visibility.Visible;
        }
        private void cmbcardtype_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbcardtype.SelectedItems != null && cmbcardtype.SelectedItems.Count >0) tbtype.Visibility = Visibility.Collapsed;
            else tbtype.Visibility = Visibility.Visible;

            //if (!isChangedGrid)
            {
                bool isxyz = cmbcardtype.SelectedItems.Cast<TypeItem>().Any(item => item.TypeCode == (ulong)CardType.eXceed);
                bool isnonxyz = cmbcardtype.SelectedItems.Cast<TypeItem>().Any(item =>
                    item.TypeCode == (ulong)CardType.Fusion ||
                    item.TypeCode == (ulong)CardType.Ritual ||
                    item.TypeCode == (ulong)CardType.Synchro);

                IsLinkIntf = cmbcardtype.SelectedItems.Cast<TypeItem>().Any(item => item.TypeCode == (ulong)CardType.Link);

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

                IsPendulumIntf = cmbcardtype.SelectedItems.Cast<TypeItem>().Any(item => item.TypeCode == (ulong)CardType.Pendulum);
                IsSkillIntf = cmbcardtype.SelectedItems.Cast<TypeItem>().Any(item => item.TypeCode == (ulong)CardType.Skill);
            }
        }

        private void cmbcardrace_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbcardrace.SelectedItems != null && cmbcardrace.SelectedItems.Count > 0)
                tbrace.Visibility = Visibility.Collapsed;
            else tbrace.Visibility = Visibility.Visible;
        }
        private void cmbcardchar_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if(cmbcardchar.SelectedItems != null && cmbcardchar.SelectedItems.Count > 0)
                tbchar.Visibility = Visibility.Collapsed;
            else tbchar.Visibility = Visibility.Visible;
        }
        private void cmbcardattribute_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if(cmbcardattribute.SelectedItems != null && cmbcardattribute.SelectedItems.Count > 0)
                tbattribute.Visibility = Visibility.Collapsed;
            else tbattribute.Visibility = Visibility.Visible;
        }
        private void cmbsetcode_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if(cmbsetcode.SelectedItems != null && cmbsetcode.SelectedItems.Count > 0)
                tbsetcode.Visibility = Visibility.Collapsed;
            else tbsetcode.Visibility = Visibility.Visible;
        }
        private void cmbcategory_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbcategory.SelectedItems != null && cmbcategory.SelectedItems.Count > 0)
                tbcategory.Visibility = Visibility.Collapsed;
            else tbcategory.Visibility = Visibility.Visible;
        }
        private void cmbflag_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbflag.SelectedItems != null && cmbflag.SelectedItems.Count > 0)
                tbflag.Visibility = Visibility.Collapsed;
            else tbflag.Visibility = Visibility.Visible;
        }
        private void cmbrarity_SelectedItemsChanged(object sender, Sdl.MultiSelectComboBox.EventArgs.SelectedItemsChangedEventArgs e)
        {
            if (cmbrarity.SelectedItems != null && cmbrarity.SelectedItems.Count > 0)
                tbrarity.Visibility = Visibility.Collapsed;
            else tbrarity.Visibility = Visibility.Visible;
        }
        #endregion

        #region Can Execute
        private bool CurrentCardExists()
        {
            return CurrentCard != null;
        }
        private bool ListCardExists()
        {
            return Cards != null && Cards.Count >= 1;
        }
        private bool ValidCardInfor()
        {
            if (CurrentCard.id <= 0) return false;
            return true;
        }
        #endregion

        #endregion

        #region Export

        #region Ceds
        private static JsonSerializerOptions CreateSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }
        public async Task ExportCedsSelectedData()
        {
            if (datagrMain.SelectedItems == null) return;

            try
            {
                string filePath = FileDiaLogHelper.SaveCeds();
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return;

                var selectedItems = datagrMain.SelectedItems.Cast<CardEditor.Models.Card>().ToList();
                var (resultExport, messExport) = await ExportCedsData(selectedItems, filePath);
                if (resultExport) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Export.ToText(), CMess.Data.ToText()), new[] { CMess.ok.ToText() });
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {messExport}", new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        public async Task ExportCedsFiltedData()
        {
            if (CollectionViewCollection == null) return;

            try
            {
                string filePath = FileDiaLogHelper.SaveCeds();
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return;

                var filteredItems = CollectionViewCollection.Cast<CardEditor.Models.Card>().ToList();
                var (resultExport, messExport) = await ExportCedsData(filteredItems, filePath);
                if (resultExport) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Export.ToText(), CMess.Data.ToText()), new[] { CMess.ok.ToText() });
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {messExport}", new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }

        private string GetExportCedsFilePath(bool isSave)
        {
            if (isSave && !string.IsNullOrWhiteSpace(cedsFilePath) && System.IO.File.Exists(cedsFilePath)) return cedsFilePath;

            string filePath = FileDiaLogHelper.SaveCeds();

            if (string.IsNullOrEmpty(filePath)) return null;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText()), new[] { CMess.ok.ToText() });
                return null;
            }
            return filePath;
        }
        public async Task ExportCedsAllData(bool isSave)
        {
            if (Cards == null || !Cards.Any())
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, CMess.noCardFound.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            try
            {
                string filePath = GetExportCedsFilePath(isSave);
                if (string.IsNullOrWhiteSpace(filePath)) return;
                if (isSave) cedsFilePath = filePath;

                var (resultExport, messExport) = await ExportCedsData(Cards.ToList(), filePath);
                if (!resultExport)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Export.ToText(), CMess.Data.ToText())} {messExport}", new[] { CMess.ok.ToText() });
                    return;
                }

                var (resultArchi, messArchi) = await SaveToArchive();
                if (!resultArchi)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, messArchi, new[] { CMess.ok.ToText() });
                    return;
                }
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Export.ToText(), CMess.Data.ToText()), new[] { CMess.ok.ToText() });
            }
            catch (InvalidCastException ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task<(bool, string)> ExportCedsData(List<CardEditor.Models.Card> selectedItems, string targetedFilepath)
        {
            if (selectedItems == null || !selectedItems.Any())
                return (false, CMess.noCardExport.ToText());

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
            });

            try
            {
                string json = JsonSerializer.Serialize(selectedItems, SerializerOptions);

                await Task.Run(() =>
                {
                    using (StreamWriter writer = new StreamWriter(targetedFilepath, false, Encoding.UTF8))
                    {
                        writer.Write(json);
                        writer.Flush();
                    }
                });

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        #endregion

        #region Excel
        public async Task ExportExcelSelectedData()
        {
            if (datagrMain.SelectedItems == null) return;

            try
            {
                string filePath = FileDiaLogHelper.SaveExcel();
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return;

                var selectedItems = datagrMain.SelectedItems.Cast<CardEditor.Models.Card>().ToList();
                var (resultExport, messExport) = await ExportExcelData(selectedItems, filePath);
                if (resultExport) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Export.ToText(), CMess.Data.ToText()), new[] { CMess.ok.ToText() });
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {messExport}", new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        public async Task ExportExcelFiltedData()
        {
            if (CollectionViewCollection == null) return;

            try
            {
                string filePath = FileDiaLogHelper.SaveExcel();
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return;

                var filteredItems = CollectionViewCollection.Cast<CardEditor.Models.Card>().ToList();
                var (resultExport, messExport) = await ExportExcelData(filteredItems, filePath);
                if (resultExport) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Export.ToText(), CMess.Data.ToText()), new[] { CMess.ok.ToText() });
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {messExport}", new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }

        private string GetExportExcelFilePath(bool isSave)
        {
            if(isSave && !string.IsNullOrWhiteSpace(xlsxFilePath) && System.IO.File.Exists(xlsxFilePath))
            {
                return xlsxFilePath;
            }

            string filePath = FileDiaLogHelper.SaveExcel();

            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(filePath))
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText()), new[] { CMess.ok.ToText() });
                return null;
            }
            return filePath;
        }
        public async Task ExportExcelAllData(bool isSave)
        {
            if (Cards == null || !Cards.Any())
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, CMess.noCardFound.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            try
            {
                string filePath = GetExportExcelFilePath(isSave);
                if (string.IsNullOrWhiteSpace(filePath)) return;
                if (isSave) xlsxFilePath = filePath;

                var (resultExport, messExport) = await ExportExcelData(Cards.ToList(), filePath);
                if (!resultExport)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Export.ToText(), CMess.Data.ToText())} {messExport}", new[] { CMess.ok.ToText() });
                    return;
                }

                var (resultArchi, messArchi) = await SaveToArchive();
                if (!resultArchi)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, messArchi, new[] { CMess.ok.ToText() });
                    return;
                }
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Export.ToText(), CMess.Data.ToText()), new[] { CMess.ok.ToText() });
            }
            catch (InvalidCastException ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task<(bool, string)> ExportExcelData(List<CardEditor.Models.Card> selectedItems, string targetedFilepath)
        {
            if (selectedItems == null || !selectedItems.Any())
                return (false, CMess.noCardExport.ToText());

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
            });

            try
            {
                // ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                ExcelPackage package;
                ExcelWorksheet worksheet;
                if (!System.IO.File.Exists(targetedFilepath))
                {
                    // File mới: Tạo file và ghi toàn bộ dữ liệu
                    package = new ExcelPackage();
                    worksheet = package.Workbook.Worksheets.Add("Cards");
                    // Thiết lập tiêu đề cột
                    worksheet.Cells[1, 1].Value = "CardEditorX";
                    worksheet.Cells[1, 2].Value = "name";
                    worksheet.Cells[1, 3].Value = "desc";

                    worksheet.Cells[1, 4].Value = "ot";
                    worksheet.Cells[1, 5].Value = "alias";
                    worksheet.Cells[1, 6].Value = "setcode";
                    worksheet.Cells[1, 7].Value = "type";
                    worksheet.Cells[1, 8].Value = "atk";
                    worksheet.Cells[1, 9].Value = "def";
                    worksheet.Cells[1, 10].Value = "level";
                    worksheet.Cells[1, 11].Value = "race";
                    worksheet.Cells[1, 12].Value = "attribute";
                    worksheet.Cells[1, 13].Value = "category";

                    worksheet.Cells[1, 14].Value = "str1";
                    worksheet.Cells[1, 15].Value = "str2";
                    worksheet.Cells[1, 16].Value = "str3";
                    worksheet.Cells[1, 17].Value = "str4";
                    worksheet.Cells[1, 18].Value = "str5";
                    worksheet.Cells[1, 19].Value = "str6";
                    worksheet.Cells[1, 20].Value = "str7";
                    worksheet.Cells[1, 21].Value = "str8";
                    worksheet.Cells[1, 22].Value = "str9";
                    worksheet.Cells[1, 23].Value = "str10";
                    worksheet.Cells[1, 24].Value = "str11";
                    worksheet.Cells[1, 25].Value = "str12";
                    worksheet.Cells[1, 26].Value = "str13";
                    worksheet.Cells[1, 27].Value = "str14";
                    worksheet.Cells[1, 28].Value = "str15";
                    worksheet.Cells[1, 29].Value = "str16";

                    // Ghi toàn bộ dữ liệu
                    for (int i = 0; i < selectedItems.Count; i++)
                    {
                        try
                        {
                            var card = selectedItems[i];
                            int row = i + 2;
                            worksheet.Cells[row, 1].Value = card.id;
                            worksheet.Cells[row, 2].Value = card.name;
                            worksheet.Cells[row, 3].Value = card.desc;

                            worksheet.Cells[row, 4].Value = card.ot;
                            worksheet.Cells[row, 5].Value = card.alias;
                            worksheet.Cells[row, 6].Value = card.setcode;
                            worksheet.Cells[row, 7].Value = card.type;
                            worksheet.Cells[row, 8].Value = card.atk;
                            worksheet.Cells[row, 9].Value = card.def;
                            worksheet.Cells[row, 10].Value = card.level;
                            worksheet.Cells[row, 11].Value = card.race;
                            worksheet.Cells[row, 12].Value = card.attribute;
                            worksheet.Cells[row, 13].Value = card.category;

                            worksheet.Cells[row, 14].Value = card.str1;
                            worksheet.Cells[row, 15].Value = card.str2;
                            worksheet.Cells[row, 16].Value = card.str3;
                            worksheet.Cells[row, 17].Value = card.str4;
                            worksheet.Cells[row, 18].Value = card.str5;
                            worksheet.Cells[row, 19].Value = card.str6;
                            worksheet.Cells[row, 20].Value = card.str7;
                            worksheet.Cells[row, 21].Value = card.str8;
                            worksheet.Cells[row, 22].Value = card.str9;
                            worksheet.Cells[row, 23].Value = card.str10;
                            worksheet.Cells[row, 24].Value = card.str11;
                            worksheet.Cells[row, 25].Value = card.str12;
                            worksheet.Cells[row, 26].Value = card.str13;
                            worksheet.Cells[row, 27].Value = card.str14;
                            worksheet.Cells[row, 28].Value = card.str15;
                            worksheet.Cells[row, 29].Value = card.str16;
                        }
                        catch
                        {
                            ///
                        }
                    }
                }
                else
                {
                    // File cũ: Kiểm tra file
                    using (var fileStream = new FileStream(targetedFilepath, FileMode.Open, FileAccess.ReadWrite))
                    {
                        package = new ExcelPackage(fileStream);
                    }
                    worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (!CheckDatabase.CheckExcelValidity(worksheet))
                        return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()));

                    // Tạo Dictionary để lưu id và số dòng tương ứng
                    var idToRowMap = new Dictionary<ulong, int>();
                    int lastRow = worksheet.Dimension?.End.Row ?? 1;

                    // Duyệt qua cột A để xây dựng map
                    for (int row = 2; row <= lastRow; row++)
                    {
                        var cellValue = worksheet.Cells[row, 1].Value;
                        if (cellValue != null)
                        {
                            try
                            {
                                ulong id = Convert.ToUInt64(cellValue);
                                idToRowMap[id] = row;
                            }
                            catch
                            {
                                ///
                            }
                        }
                    }

                    // Xử lý dữ liệu với file cũ
                    foreach (var card in selectedItems)
                    {
                        try
                        {
                            if (idToRowMap.TryGetValue(card.id, out int existingRow))
                            {
                                worksheet.Cells[existingRow, 3].Value = card.desc;
                            }
                            else
                            {
                                // Thêm mới vào dòng cuối
                                lastRow++;
                                worksheet.Cells[lastRow, 1].Value = card.id;
                                worksheet.Cells[lastRow, 2].Value = card.name;
                                worksheet.Cells[lastRow, 3].Value = card.desc;
                                worksheet.Cells[lastRow, 4].Value = card.ot;
                                worksheet.Cells[lastRow, 5].Value = card.alias;
                                worksheet.Cells[lastRow, 6].Value = card.setcode;
                                worksheet.Cells[lastRow, 7].Value = card.type;
                                worksheet.Cells[lastRow, 8].Value = card.atk;
                                worksheet.Cells[lastRow, 9].Value = card.def;
                                worksheet.Cells[lastRow, 10].Value = card.level;
                                worksheet.Cells[lastRow, 11].Value = card.race;
                                worksheet.Cells[lastRow, 12].Value = card.attribute;
                                worksheet.Cells[lastRow, 13].Value = card.category;
                                worksheet.Cells[lastRow, 14].Value = card.str1;
                                worksheet.Cells[lastRow, 15].Value = card.str2;
                                worksheet.Cells[lastRow, 16].Value = card.str3;
                                worksheet.Cells[lastRow, 17].Value = card.str4;
                                worksheet.Cells[lastRow, 18].Value = card.str5;
                                worksheet.Cells[lastRow, 19].Value = card.str6;
                                worksheet.Cells[lastRow, 20].Value = card.str7;
                                worksheet.Cells[lastRow, 21].Value = card.str8;
                                worksheet.Cells[lastRow, 22].Value = card.str9;
                                worksheet.Cells[lastRow, 23].Value = card.str10;
                                worksheet.Cells[lastRow, 24].Value = card.str11;
                                worksheet.Cells[lastRow, 25].Value = card.str12;
                                worksheet.Cells[lastRow, 26].Value = card.str13;
                                worksheet.Cells[lastRow, 27].Value = card.str14;
                                worksheet.Cells[lastRow, 28].Value = card.str15;
                                worksheet.Cells[lastRow, 29].Value = card.str16;

                                // Cập nhật map với id mới
                                idToRowMap[card.id] = lastRow;
                            }
                        }
                        catch
                        {
                            lastRow++;
                            idToRowMap[card.id] = lastRow;
                        }
                    }
                }

                // Tự động điều chỉnh kích thước cột
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                await Task.Run(() =>
                {
                    System.IO.File.WriteAllBytes(targetedFilepath, package.GetAsByteArray());
                });

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        #endregion

        #endregion

        #region Items Editor

        #region Replace Description
        public async Task<ResultItem> ReplaceText1(string findWhat, string replaceWith, int scope)
        {
            // 1. Validate input
            if (string.IsNullOrWhiteSpace(findWhat))
                return new ResultItem
                {
                    Succeeded = false,
                    Message = $"{CMess.FindWhat.ToText()} {CMess.cannotEmpty.ToText()}"
                };

            if (string.IsNullOrWhiteSpace(replaceWith))
                return new ResultItem
                {
                    Succeeded = false,
                    Message = $"{CMess.Replacewith.ToText()} {CMess.cannotEmpty.ToText()}"
                };

            if (scope < 1 || scope > 3)
                return new ResultItem
                {
                    Succeeded = false,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };

            // 2. Settings
            int advancedSettings = ConfigViewModel.Instance.dataHandlingSetting.Advanced;

            bool advancedFind = (advancedSettings & 0x01) != 0;
            bool matchCase = (advancedSettings & 0x02) != 0;
            bool useWildcards = (advancedSettings & 0x04) != 0;
            bool matchPrefix = (advancedSettings & 0x08) != 0;
            bool matchSuffix = (advancedSettings & 0x10) != 0;
            bool wholeWord = (advancedSettings & 0x20) != 0;
            bool ignorePunctuation = (advancedSettings & 0x40) != 0;
            bool ignoreWhitespace = (advancedSettings & 0x80) != 0;

            // 3. Get target cards
            var targetCards = scope switch
            {
                1 => datagrMain.SelectedItems.Cast<CardEditor.Models.Card>().ToList(),
                2 => CollectionViewCollection.Cast<CardEditor.Models.Card>().ToList(),
                3 => Cards.ToList(),
                _ => new List<CardEditor.Models.Card>()
            };

            int totalCount = targetCards.Count;

            if (totalCount == 0)
            {
                return new ResultItem
                {
                    Succeeded = false,
                    TotalCount = 0,
                    FilteredCount = 0,
                    Message = CMess.noCardReplace.ToText()
                };
            }

            // 4. Normalize helper
            string Normalize(string input)
            {
                if (string.IsNullOrEmpty(input))
                    return string.Empty;

                if (advancedFind)
                {
                    if (ignoreWhitespace)
                        input = Regex.Replace(input, @"\s+", "");

                    if (ignorePunctuation)
                        input = Regex.Replace(input, @"[^\w\s]", "");
                }

                return input;
            }

            string normalizedFind = Normalize(findWhat);

            // 5. Build regex pattern
            string BuildPattern(string input)
            {
                string pattern = advancedFind
                    ? Regex.Escape(input)
                    : Regex.Escape(input);

                if (advancedFind)
                {
                    if (useWildcards)
                    {
                        pattern = pattern
                            .Replace(@"\*", ".*")
                            .Replace(@"\?", ".");
                    }

                    if (wholeWord)
                        pattern = $@"\b{pattern}\b";

                    if (matchPrefix)
                        pattern = "^" + pattern;

                    if (matchSuffix)
                        pattern += "$";
                }

                return pattern;
            }

            string pattern = BuildPattern(normalizedFind);

            RegexOptions options = matchCase
                ? RegexOptions.None
                : RegexOptions.IgnoreCase;

            var regex = new Regex(pattern, options);

            // 6. Process
            int modifiedCount = 0;

            await Task.Run(() =>
            {
                foreach (var card in targetCards)
                {
                    if (string.IsNullOrEmpty(card.desc))
                        continue;

                    string original = card.desc;
                    string result = regex.Replace(original, replaceWith);

                    if (!string.Equals(original, result, StringComparison.Ordinal))
                    {
                        card.desc = result;
                        modifiedCount++;
                    }
                }
            });

            // 7. UI refresh
            if (modifiedCount > 0)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CollectionViewCollection.Refresh();
                });
            }

            IsSaved = modifiedCount == 0;

            // 8. Return structured result
            return new ResultItem
            {
                Succeeded = true,
                TotalCount = totalCount,
                FilteredCount = modifiedCount,
                Message = modifiedCount > 0
                    ? string.Format(CMess.TwoPlaceholderSuccess.ToText(), modifiedCount, CMess.Card.ToText())
                    : CMess.noCardReplace.ToText()
            };
        }
        public async Task<ResultItem> ReplaceText(string findWhat, string replaceWith, int scope)
        {
            if (string.IsNullOrWhiteSpace(findWhat))
                return new ResultItem
                {
                    Succeeded = false,
                    Message = $"{CMess.FindWhat.ToText()} {CMess.cannotEmpty.ToText()}"
                };
            if (string.IsNullOrWhiteSpace(replaceWith))
                return new ResultItem
                {
                    Succeeded = false,
                    Message = $"{CMess.Replacewith.ToText()} {CMess.cannotEmpty.ToText()}"
                };
            if (scope < 1 || scope > 3)
                return new ResultItem
                {
                    Succeeded = false,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };

            var optRaw = ConfigViewModel.Instance.dataHandlingSetting.Advanced;

            var opt = new CardEditor.Models.SearchOptions
            {
                MatchCase = (optRaw & 0x02) != 0,
                UseWildcards = (optRaw & 0x04) != 0,
                MatchPrefix = (optRaw & 0x08) != 0,
                MatchSuffix = (optRaw & 0x10) != 0,
                WholeWord = (optRaw & 0x20) != 0,
                IgnorePunctuation = (optRaw & 0x40) != 0,
                IgnoreWhitespace = (optRaw & 0x80) != 0
            };

            var targetCards = scope switch
            {
                1 => datagrMain.SelectedItems.Cast<CardEditor.Models.Card>().ToList(),
                2 => CollectionViewCollection.Cast<CardEditor.Models.Card>().ToList(),
                3 => Cards.ToList(),
                _ => new List<CardEditor.Models.Card>()
            };

            if (!targetCards.Any())
                return new ResultItem
                {
                    Succeeded = false,
                    Message = string.Format(CMess.noCardFound.ToText())
                };

            RegexOptions regexOpt = opt.MatchCase
                ? RegexOptions.None
                : RegexOptions.IgnoreCase;

            int modifiedCount = await Task.Run(() => SearchPattern.Replace(targetCards, findWhat, replaceWith, opt, regexOpt));

            if (modifiedCount > 0)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CollectionViewCollection.Refresh();
                });
            }

            IsSaved = modifiedCount == 0;

            return new ResultItem
            {
                Succeeded = true,
                TotalCount = targetCards.Count,
                FilteredCount = modifiedCount,
                Message = modifiedCount > 0
                    ? string.Format(CMess.TwoPlaceholderSuccess.ToText(), modifiedCount, CMess.Card.ToText())
                    : CMess.noCardReplace.ToText()
            };
        }
        #endregion

        #region Replace Field
        public (bool Success, int ReplacedCard, int TotalCard, string Message) ReplaceDataCommand(IEnumerable<CardEditor.Models.Card> cardList, ulong flags, bool isAddNew)
        {
            if (cardList == null || flags == 0) return (false, 0, 0, CMess.noCardReplace.ToText());

            IEnumerable<Action<CardEditor.Models.Card, CardEditor.Models.Card>> updaters =
                Enumerable.Empty<Action<CardEditor.Models.Card, CardEditor.Models.Card>>();

            if (flags != 0)
            {
                var selected = (CardField)flags;
                updaters = CardFieldUpdate.FieldUpdaters.Where(kv => selected.HasFlag(kv.Key)).Select(kv => kv.Value);
            }

            return ReplaceDataField(cardList, isAddNew, updaters);
        }
        public (bool Success, int ReplacedCard, int TotalCard, string Message) ReplaceDataField(IEnumerable<CardEditor.Models.Card> cardList, bool isAddNew,
            params Action<CardEditor.Models.Card, CardEditor.Models.Card>[] updaters)
        {
            return ReplaceDataField(cardList, isAddNew, updaters.AsEnumerable());
        }
        public (bool Success, int ReplacedCard, int TotalCard, string Message) ReplaceDataField(IEnumerable<CardEditor.Models.Card> cardList, bool isAddNew,
            IEnumerable<Action<CardEditor.Models.Card, CardEditor.Models.Card>> updaters)
        {
            if (cardList == null || Cards == null)
                return (false, 0, 0, CMess.noCardReplace.ToText());

            try
            {
                var existingMap = Cards.GroupBy(c => c.id).ToDictionary(g => g.Key, g => g.First());
                var updaterList = updaters?.ToList();
                bool allowUpdate = updaterList?.Count > 0;
                int replaced = 0;

                Cards.BeginUpdate();
                foreach (var newCard in cardList)
                {
                    if (existingMap.TryGetValue(newCard.id, out var existing))
                    {
                        if (allowUpdate) existing.UpdateFrom(newCard, updaterList);
                        replaced++;
                    }
                    else if (isAddNew)
                    {
                        Cards.Add(newCard);
                        replaced++;
                    }
                }
                
                IsSaved = false;

                return (true, replaced, Cards.Count(), string.Empty);
            }
            catch (Exception ex)
            {
                return (false, 0, 0, ex.Message);
            }
            finally
            {
                Cards.EndUpdate();
            }
        }
        #endregion

        #region Import Data
        public (bool, List<ulong>) CheckDuplicateIds()
        {
            if (Cards == null) return (false, null);
            var duplicates = Cards.GroupBy(c => c.id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            return (duplicates.Count > 0, duplicates);
        }
        public (bool, string) ImportDataCommand(IEnumerable<CardEditor.Models.Card> cardList, ulong flags)
        {
            if (cardList == null) return (false, CMess.noCardReplace.ToText());

            IEnumerable<Action<CardEditor.Models.Card, CardEditor.Models.Card>> updaters =
                Enumerable.Empty<Action<CardEditor.Models.Card, CardEditor.Models.Card>>();

            if (flags != 0)
            {
                var selected = (CardField)flags;
                updaters = CardFieldUpdate.FieldUpdaters.Where(kv => selected.HasFlag(kv.Key)).Select(kv => kv.Value);
            }

            return ImportDataField(cardList, updaters);
        }
        public (bool, string) ImportDataField(IEnumerable<CardEditor.Models.Card> cardList,
            IEnumerable<Action<CardEditor.Models.Card, CardEditor.Models.Card>> updaters)
        {
            if (cardList == null || Cards == null)
                return (false, CMess.noCardReplace.ToText());

            try
            {
                var existingMap = Cards.GroupBy(c => c.id).ToDictionary(g => g.Key, g => g.First());
                var updaterList = updaters?.ToList();
                bool allowUpdate = updaterList?.Count > 0;
                int count = 0;

                Cards.BeginUpdate();
                foreach (var newCard in cardList)
                {
                    if (existingMap.TryGetValue(newCard.id, out var existing))
                    {
                        if (allowUpdate) existing.UpdateFrom(newCard, updaterList);
                    }
                    else Cards.Add(newCard);
                    count++;
                }
                Cards.EndUpdate();
                IsSaved = false;

                return (true, count.ToString());
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (bool, string) ImportDataField(IEnumerable<CardEditor.Models.Card> cardList,
            params Action<CardEditor.Models.Card, CardEditor.Models.Card>[] updaters)
        {
            return ImportDataField(cardList, updaters.AsEnumerable());
        }

        public void ImportCreateNew(IEnumerable<CardEditor.Models.Card> importedCards)
        {
            if (importedCards == null) return;
            Cards.ReplaceAll(importedCards);
            Mouse.OverrideCursor = null;
            RebuildWindowTitle();
        }
        public void ImportOverwrite(IEnumerable<CardEditor.Models.Card> importedCards)
        {
            if (importedCards == null || !importedCards.Any()) return;

            List<CardEditor.Models.Card> CardList = new List<CardEditor.Models.Card>();
            var CardsDict = Cards.GroupBy(c => c.id).ToDictionary(g => g.Key, g => g.First());

            foreach (var item in importedCards)
            {
                if (CardsDict.TryGetValue(item.id, out var existingCard))
                {
                    existingCard.desc = item.desc;
                }
                else
                {
                    CardList.Add(item);
                }
            }
            Cards.AddRange(CardList);
            Mouse.OverrideCursor = null;
        }
        public void ImportAppendwrite(IEnumerable<CardEditor.Models.Card> importedCards)
        {
            if (importedCards == null || !importedCards.Any()) return;
            List<CardEditor.Models.Card> CardList = new List<CardEditor.Models.Card>();
            var CardsDict = Cards.GroupBy(c => c.id).ToDictionary(g => g.Key, g => g.First());

            foreach (var item in importedCards)
            {
                if (!CardsDict.ContainsKey(item.id))
                {
                    CardList.Add(item);
                }
            }
            Cards.AddRange(CardList);
            Mouse.OverrideCursor = null;
        }
        #endregion

        #region Pendulum Language
        private CancellationTokenSource _applyPenLangCts;
        private void LoadPendulumLanguage() // Selected Card Changed
        {
            if (CurrentCard == null || !IsPendulumIntf) return;

            PenAnalysisResult result = PenLanguageViewModel.Instance.Analyze(CurrentCard.desc);
            if (result == null)
            {
                SelectedPenLangRule = null;
                return;
            }
            Debug.WriteLine($"Rule: {result.RuleResult.Locale.ToString()}");
            SelectedPenLangRule = result.RuleResult;
        }
        private void OpenPreViewDescWindow()
        {
            if (CurrentCard == null || !IsPendulumIntf) return;
            if (SelectedPenLangRule?.Locale is PendulumLocales.EDOProNonPen or PendulumLocales.Unknown) return;

            if (MainWindowService != null)
            {
                MainWindowService.OpenPreViewDescWindow(CurrentCard, SelectedPenLangRule);
            }
        }
        private void ApplyPendulumLanguageDesc()
        {
            if (CurrentCard == null || !IsPendulumIntf || SelectedPenLangRule == null) return;
            if (SelectedPenLangRule?.Locale is PendulumLocales.EDOProNonPen or PendulumLocales.Unknown) return;

            PenAnalysisResult result = PenLanguageViewModel.Instance.Analyze(CurrentCard.desc);
            PenDescResult effect = result.DescResult;

            PenScale scale = GetPenScaleHelp.GetPenScale(CurrentCard.level);
            bool IsNormalCard = (CurrentCard.type & (ulong)CardType.Normal) != 0;

            string desc = PenLanguageViewModel.Instance.BuildDesc(SelectedPenLangRule, effect, scale, IsNormalCard);
            txtcarddesc.Text = desc;
        }
        public async Task<PenDescProcessSummary> PendulumLanguage(PendulumLanguageRule rule, int scope, bool overwrite)
        {
            _applyPenLangCts?.Dispose();
            _applyPenLangCts = new CancellationTokenSource();

            if (scope < 0 || scope > 2)
            {
                return new PenDescProcessSummary
                {
                    Result = false,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };
            }

            var cardList = scope switch
            {
                0 => datagrMain.SelectedItems?.OfType<CardEditor.Models.Card>().ToList() ?? new List<CardEditor.Models.Card>(),
                1 => CollectionViewCollection?.OfType<CardEditor.Models.Card>().ToList() ?? new List<CardEditor.Models.Card>(),
                2 => Cards?.ToList() ?? new List<CardEditor.Models.Card>(),
                _ => new List<CardEditor.Models.Card>()
            };

            if (cardList == null || cardList.Count == 0)
            {
                return new PenDescProcessSummary
                {
                    Result = false,
                    Message = CMess.noCardSelec.ToText()
                };
            }

            try
            {
                PenDescProcessSummary result = await PenLanguageViewModel.Instance.ProcessPenDesc(cardList, rule, _applyPenLangCts.Token, overwrite);
                // Total: Tổng số card có cờ CardType.Pendulum được đưa vào xử lý (đếm ngay từ đầu vòng lặp, trước khi biết kết quả thành hay bại)
                // Success: Số card đã build lại desc thành công bằng newRule, toàn bộ pipeline Analyze → BuildDesc chạy trót lọt không throw exception, và card.desc đã được gán giá trị mới.
                // Error: Số card mà Analyze hoặc BuildDesc throw exception trong lúc xử lý. Card này bị bỏ qua, card.desc giữ nguyên giá trị gốc (không được gán lại). ID của các card này nằm trong ErrorCardIds.
                // EmptyDesc: Số card có desc rỗng hoặc chỉ toàn khoảng trắng → bị skip ngay, không gọi Analyze/BuildDesc, không tính là lỗi. Comment gốc "-> Unknown" ý là: không xác định được ngôn ngữ vì không có gì để phân tích.
                // EdoProFallback: Số card Analyze chạy không lỗi, nhưng kết quả phân tích rơi vào rule mặc định EdoProNoPenRule — tức là desc gốc không khớp với bất kỳ rule ngôn ngữ chính thức nào, phải fallback về format chung của EDOPro
                /// Card này vẫn được build lại bằng newRule bình thường (không bị loại khỏi Success), số này chỉ để theo dõi tỷ lệ desc "không rõ nguồn gốc ngôn ngữ".
                /// 1 card có thể vừa nằm trong EdoProFallback vừa nằm trong Success (2 số này không loại trừ nhau, vì EdoProFallback đếm ở giữa quá trình, Success đếm ở cuối nếu không exception).
                // Cancelled: true nếu quá trình xử lý bị dừng giữa chừng do CancellationToken được yêu cầu hủy (user bấm Stop). Khi đó tất cả thay đổi đã tự động rollback.
                // RolledBack: true nếu đã tự động rollback toàn bộ card.desc về giá trị gốc bằng _lastSnapshot, do gặp Cancelled hoặc lỗi nghiêm trọng ngoài dự kiến (không phải lỗi per-card)
                /// Không liên quan đến rollback thủ công qua processor.Rollback()
                // ErrorCardIds: Danh sách id của các card rơi vào nhóm Error, để tầng gọi (UI) có thể hiển thị chi tiết "những card nào bị lỗi, cần xem lại thủ công".

                IsSaved = false;
                return result;
            }
            finally
            {
                _applyPenLangCts.Dispose();
                _applyPenLangCts = null;
            }
        }
        public (bool, string) RollBackPenlang()
        {
            var result = PenLanguageViewModel.Instance.PerformManualRollback();
            IsSaved = false;
            return result;
        }
        public void StopApplyPenLang()
        {
            _applyPenLangCts?.Cancel();
        }

        public void ApplyPendulumLanguage(string desc)
        {
            txtcarddesc.Text = desc;
        }
        #endregion

        #region Credits
        private CancellationTokenSource _applyCreditCts;
        public async Task<ResultItem> CreditTeam(CreditItem credit, int scope, int writeMode)
        {
            _applyCreditCts?.Dispose();
            _applyCreditCts = new CancellationTokenSource();

            if (scope < 0 || scope > 2)
            {
                return new ResultItem
                {
                    Succeeded = false,
                    TotalCount = 0,
                    FilteredCount = 0,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };
            }

            var cardList = scope switch
            {
                0 => datagrMain.SelectedItems?.OfType<CardEditor.Models.Card>().ToList() ?? new List<CardEditor.Models.Card>(),
                1 => CollectionViewCollection?.OfType<CardEditor.Models.Card>().ToList() ?? new List<CardEditor.Models.Card>(),
                2 => Cards?.ToList() ?? new List<CardEditor.Models.Card>(),
                _ => new List<CardEditor.Models.Card>()
            };

            if (cardList == null || cardList.Count == 0)
            {
                return new ResultItem
                {
                    Succeeded = false,
                    TotalCount = 0,
                    FilteredCount = 0,
                    Message = CMess.noCardSelec.ToText()
                };
            }

            try
            {
                var result = await CreditsViewModel.Instance.CreditTeam(cardList, credit, writeMode, _applyCreditCts.Token);
                IsSaved = false;
                return result;
            }
            finally
            {
                _applyCreditCts.Dispose();
                _applyCreditCts = null;
            }
        }
        public async Task<ResultItem> RemoveCredit(int scope)
        {
            if (scope < 0 || scope > 2)
            {
                return new ResultItem
                {
                    Succeeded = false,
                    TotalCount = 0,
                    FilteredCount = 0,
                    Message = string.Format(CMess.PlaceholderInva.ToText(), CMess.cardLabelScope.ToText())
                };
            }

            var cardList = scope switch
            {
                0 => datagrMain.SelectedItems?.OfType<CardEditor.Models.Card>().ToList() ?? new List<CardEditor.Models.Card>(),
                1 => CollectionViewCollection?.OfType<CardEditor.Models.Card>().ToList() ?? new List<CardEditor.Models.Card>(),
                2 => Cards?.ToList() ?? new List<CardEditor.Models.Card>(),
                _ => new List<CardEditor.Models.Card>()
            };

            if (cardList == null || cardList.Count == 0)
            {
                return new ResultItem
                {
                    Succeeded = false,
                    TotalCount = 0,
                    FilteredCount = 0,
                    Message = CMess.noCardSelec.ToText()
                };
            }

            var result = await CreditsViewModel.Instance.RemoveAllCredits(cardList);
            IsSaved = false;
            return result;
        }
        public (bool, string) RollbackCredits()
        {
            var result = CreditsViewModel.Instance.PerformManualRollback();
            IsSaved = false;
            return result;
        }
        public void StopApplyCredits()
        {
            _applyCreditCts?.Cancel();
        }
        #endregion

        #endregion

        #region Undo/Redo
        private void InitializeControls()
        {
            //_undoStack = new Stack<ControlState>();
            //_redoStack = new Stack<ControlState>();

            //var textBoxes = FindVisualChildren<System.Windows.Controls.TextBox>(this);
            //foreach (var textBox in textBoxes)
            //{
            //    textBox.TextChanged += (s, e) =>
            //    {
            //        if (e.Changes.Any())
            //        {
            //            SaveState(textBox, textBox.Text, "TextBox");
            //        }
            //    };
            //}

            //var richTextBoxes = FindVisualChildren<System.Windows.Controls.RichTextBox>(this);
            //foreach (var richTextBox in richTextBoxes)
            //{
            //    richTextBox.TextChanged += (s, e) =>
            //    {
            //        TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            //        SaveState(richTextBox, range.Text, "RichTextBox");
            //    };
            //}

            //var multiSelectComboBoxes = FindVisualChildren<Sdl.MultiSelectComboBox.Themes.Generic.MultiSelectComboBox>(this);
            //foreach (var multiSelectComboBox in multiSelectComboBoxes)
            //{
            //    multiSelectComboBox.SelectedItemsChanged += (s, e) =>
            //    {
            //        SaveState(multiSelectComboBox, multiSelectComboBox.SelectedItems?.Cast<object>().ToList(), "MultiSelectComboBox");
            //    };
            //}
        }
        public void SaveState(object control, object state, string controlType)
        {
            //_undoStack.Push(new ControlState { Control = control, State = state, ControlType = controlType });
            //_redoStack.Clear(); // Xóa Redo khi có thay đổi mới
        }
        public void Undo()
        {
            //if (_undoStack.Count == 0) return;

            //var state = _undoStack.Pop();
            //ApplyState(state);
            //_redoStack.Push(CaptureCurrentState(state.Control, state.ControlType));
        }
        public void Redo()
        {
            //if (_redoStack.Count == 0) return;

            //var state = _redoStack.Pop();
            //ApplyState(state);
            //_undoStack.Push(CaptureCurrentState(state.Control, state.ControlType));
        }
        public void ResetUndo()
        {
            //_undoStack.Clear();
            //_redoStack.Clear();
        }
        private ControlState CaptureCurrentState(object control, string controlType)
        {
            object state = null;
            if (controlType == "TextBox" && control is System.Windows.Controls.TextBox textBox)
            {
                state = textBox.Text;
            }
            else if (controlType == "RichTextBox" && control is RichTextBox richTextBox)
            {
                TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                state = range.Text;
            }
            //else if (controlType == "MultiSelectComboBox" && control is Sdl.MultiSelectComboBox.Themes.Generic.MultiSelectComboBox multiSelectComboBox)
            //{
            //    // Sao chép danh sách SelectedItems
            //    state = multiSelectComboBox.SelectedItems?.Cast<object>().ToList();
            //}

            return new ControlState { Control = control, State = state, ControlType = controlType };
        }
        private void ApplyState(ControlState state)
        {
            if (state.ControlType == "TextBox" && state.Control is System.Windows.Controls.TextBox textBox)
            {
                textBox.Text = state.State?.ToString();
            }
            else if (state.ControlType == "RichTextBox" && state.Control is RichTextBox richTextBox)
            {
                richTextBox.Document.Blocks.Clear();
                richTextBox.Document.Blocks.Add(new Paragraph(new Run(state.State?.ToString())));
            }
            //else if (state.ControlType == "MultiSelectComboBox" && state.Control is Sdl.MultiSelectComboBox.Themes.Generic.MultiSelectComboBox multiSelectComboBox)
            //{
            //    multiSelectComboBox.SelectedItems.Clear();
            //    if (state.State is List<object> selectedItems)
            //    {
            //        foreach (var item in selectedItems)
            //        {
            //            multiSelectComboBox.SelectedItems.Add(item);
            //        }
            //    }
            //}
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        private void RedoCommand()
        {
            Redo();
        }
        private void ResetUndoRedoCommand()
        {
            ResetUndo();
        }

        #endregion

        #region Copy
        public async Task CopySelectedCard()
        {
            var selectedCards = datagrMain.SelectedItems.Cast<CardEditor.Models.Card>().ToList();
            if (selectedCards.Any())
            {
                try
                {
                    await CopyCards(selectedCards);
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Copy.ToText(), CMess.Card.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
            else
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, CMess.noCardCopy.ToText(), new[] { CMess.ok.ToText() });
            }
        }
        public async Task CopyAllFilterCard()
        {
            var filteredCards = CollectionViewCollection.Cast<CardEditor.Models.Card>().ToList();
            if (filteredCards.Any())
            {
                try
                {
                    await CopyCards(filteredCards);
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Copy.ToText(), CMess.Card.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
            else
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, CMess.noCardCopy.ToText(), new[] { CMess.ok.ToText() });
            }
        }
        public async Task CopyAllCard()
        {
            if(Cards != null && Cards.Any())
            {
                try
                {
                    await CopyCards(Cards.ToList());
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Copy.ToText(), CMess.Card.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
            else
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, CMess.noCardCopy.ToText(), new[] { CMess.ok.ToText() });
            }
        }
        private async Task CopyCards(List<CardEditor.Models.Card> SelectedItems)
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
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Copy.ToText(), CMess.Card.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async Task CopyCardDescExecute()
        {
            bool success = CopyCardDesc();
            if (!success) return;

            CopyCardDescIcon = PackIconKind.Check;

            await Task.Delay(3000);
            CopyCardDescIcon = PackIconKind.ContentCopy;
        }
        private bool CopyCardDesc()
        {
            try
            {
                Clipboard.SetText(txtcarddesc.Text);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Compressed
        public async Task ExportCompressedSelectedData()
        {
            if (datagrMain.SelectedItems == null)
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, CMess.noCardSelec.ToText(), new[] { CMess.ok.ToText() });
                return;
            }
            try
            {
                var selectedItems = datagrMain.SelectedItems.Cast<CardEditor.Models.Card>().ToList();
                if (selectedItems.Any())
                {
                    await CreateCompressedFile(selectedItems);
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        public async Task ExportCompressedFiltedData()
        {
            if (CollectionViewCollection != null && CollectionViewCollection.Cast<CardEditor.Models.Card>().Any())
            {
                try
                {
                    var filteredItems = CollectionViewCollection.Cast<CardEditor.Models.Card>().ToList();
                    await CreateCompressedFile(filteredItems);
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
        }
        public async Task ExportCompressedAllData()
        {
            if (Cards != null && Cards.Any())
            {
                try
                {
                    await CreateCompressedFile(Cards.ToList());
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Export.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }
        }

        private async Task CreateCompressedFile(List<CardEditor.Models.Card> selectedItems)
        {
            if (selectedItems == null || !selectedItems.Any())
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    CMess.noCardExport.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            string filePath = FileDiaLogHelper.SaveZip();

            if (string.IsNullOrEmpty(filePath))
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText()),
                    new[] { CMess.ok.ToText() });
                return;
            }
            string outPutCdbPath = string.Empty;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                string directoryZIPPath = System.IO.Path.GetDirectoryName(filePath);
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);
                string fileWithCdbExtension = System.IO.Path.Combine(directoryZIPPath, fileNameWithoutExtension + ".cdb");

                if (System.IO.File.Exists(fileWithCdbExtension))
                {
                    CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                        $"{string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText())}" +
                        $"\n{CMess.File.ToText()} {fileWithCdbExtension} {CMess.filealreadyExit.ToText()}",
                        new[] { CMess.ok.ToText() });
                    return;
                }

                var (resultCreate, messageCreate) = CreateFileServices.CreateDatabase(directoryZIPPath, $"{fileNameWithoutExtension}.cdb");
                if (resultCreate) outPutCdbPath = messageCreate;
                else
                {
                    throw new InvalidOperationException(messageCreate);
                }

                if (string.IsNullOrEmpty(outPutCdbPath))
                {
                    throw new InvalidOperationException(string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText()));
                }
                await SaveAllCards(selectedItems, outPutCdbPath).ConfigureAwait(false);

                var fileGroups = selectedItems.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount / 2) // Giới hạn mức song song
                    .Select(card => new
                    {
                        ImagePath = FindIInfoService.FindImagePath(card.id.ToString(), System.IO.Path.GetDirectoryName(cdbFilePath)),
                        ScriptPath = FindIInfoService.FindScriptPath(System.IO.Path.GetDirectoryName(cdbFilePath), card.id.ToString())
                    })
                    .Where(paths => !string.IsNullOrEmpty(paths.ImagePath) || !string.IsNullOrEmpty(paths.ScriptPath))
                    .ToList();
                // Tạo ZIP trong một task riêng biệt
                await Task.Run(() =>
                {
                    using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
                    using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Update))
                    {
                        // Thêm file cdb
                        zip.CreateEntryFromFile(outPutCdbPath, System.IO.Path.GetFileName(outPutCdbPath), System.IO.Compression.CompressionLevel.Fastest);

                        // Nén ảnh & script theo batch
                        const int batchSize = 100;
                        for (int i = 0; i < fileGroups.Count; i += batchSize)
                        {
                            foreach (var files in fileGroups.Skip(i).Take(batchSize))
                            {
                                if (!string.IsNullOrEmpty(files.ImagePath))
                                {
                                    zip.CreateEntryFromFile(files.ImagePath,
                                        $"pics/{System.IO.Path.GetFileName(files.ImagePath)}",
                                        System.IO.Compression.CompressionLevel.Fastest);
                                }
                                if (!string.IsNullOrEmpty(files.ScriptPath))
                                {
                                    zip.CreateEntryFromFile(files.ScriptPath,
                                        $"script/{System.IO.Path.GetFileName(files.ScriptPath)}",
                                        System.IO.Compression.CompressionLevel.Fastest);
                                }
                            }
                        }
                    }
                });
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, CMess.expoZIPSuc.ToText(), new[] { CMess.ok.ToText() });
                return;
            }
            catch (InvalidOperationException ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return;
            }
            finally
            {
                if (!string.IsNullOrEmpty(outPutCdbPath) && System.IO.File.Exists(outPutCdbPath))
                {
                    try
                    {
                        System.IO.File.Delete(outPutCdbPath);
                    }
                    catch (Exception ex)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.File.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                    }
                }
                Mouse.OverrideCursor = null;
            }
        }
        #endregion

        #region RichtextBox
        /*
        private void AddFurigana_Click(object sender, RoutedEventArgs e)
        {
            var selection = txtcarddesc.Selection;
            if (!selection.IsEmpty)
            {
                string baseText = selection.Text;
                var dialog = new Furigana(baseText);
                if (dialog.ShowDialog() == true)
                {
                    string furigana = dialog.FuriganaText;
                    var snapshot = txtcarddesc.Document.Blocks.ToList();
                    AddFurigana(txtcarddesc, baseText, furigana, selection);

                }
            }
        }
        private void AddFurigana(RichTextBox richTextBox, string baseText, string furigana, TextSelection selection)
        {
            var paragraph = richTextBox.Document.Blocks.FirstBlock as Paragraph;
            if (paragraph == null)
            {
                paragraph = new Paragraph();
                richTextBox.Document.Blocks.Add(paragraph);
            }
            var stackPanel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Vertical };
            var furiganaText = new TextBlock
            {
                Text = furigana,
                FontSize = 8,
                TextAlignment = TextAlignment.Center
            };

            var baseTextBlock = new TextBlock
            {
                Text = baseText,
                FontSize = 14,
                TextAlignment = TextAlignment.Center
            };

            stackPanel.Children.Add(furiganaText);
            stackPanel.Children.Add(baseTextBlock);

            var range = new TextRange(selection.Start, selection.End);
            range.Text = string.Empty;

            var inlineContainer = new InlineUIContainer(stackPanel);
            var pointer = selection.Start; // Vị trí bắt đầu của vùng chọn
            var run = pointer.Parent as Run; // Kiểm tra xem pointer có nằm trong Run không
            if (run != null)
            {
                // Nếu pointer nằm trong Run, chèn inlineContainer sau Run
                paragraph.Inlines.InsertAfter(run, inlineContainer);
            }
            else
            {
                // Nếu không, chèn tại vị trí con trỏ
                pointer.Paragraph.Inlines.Add(inlineContainer);
            }
        }
        */
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
            this.DataContext = null;

            MessageNotifi?.Dispose();
            MessageDelete?.Dispose();
            _ctsCreateImg?.Cancel();
            _ctsCreateImg?.Dispose();
            _ctsCreateImg = null;

            datagrMain.ItemsSource = null;
            if (CollectionViewCollection != null)
            {
                CollectionViewCollection.Filter = null;
                CollectionViewCollection.SortDescriptions.Clear();
                CollectionViewCollection.GroupDescriptions.Clear();
            }

            CollectionViewCollection = null;
            Cards.CollectionChanged -= Cards_CollectionChanged;
            Cards?.Clear();
            Cards = null;

            CurrentCard = null;
            _regexCache.Clear();

            imagecard.Source = null;
            ImgArtWorkFull.Source = null;
            ImgArtWorkNor.Source = null;
            ImgArtWorkPen.Source = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        #endregion

    }
}
