using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Models.Settings;
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
    /// Interaction logic for ItemsEditor.xaml
    /// </summary>
    public class ToggleMeta
    {
        public int Index { get; set; }
        public string Label { get; set; }
    }
    public partial class ItemsEditor : Window, INotifyPropertyChanged
    {
        public MainWindow MainWindowReference { get; set; }

        #region Header
        private int _tabHeaderIndex = -1;
        public int TabHeaderIndex
        {
            get => _tabHeaderIndex;
            set
            {
                if (_tabHeaderIndex != value)
                {
                    _tabHeaderIndex = value;
                    OnPropertyChanged(nameof(TabHeaderIndex));
                    if (value == 0) HeaderIcon = MaterialDesignThemes.Wpf.PackIconKind.CogOutline;
                    if (value == 1) HeaderIcon = MaterialDesignThemes.Wpf.PackIconKind.ReceiptTextEditOutline;
                    if (value == 2) HeaderIcon = MaterialDesignThemes.Wpf.PackIconKind.FileReplaceOutline;
                    if (value == 3) HeaderIcon = MaterialDesignThemes.Wpf.PackIconKind.DatabaseImportOutline;
                    if (value == 4) HeaderIcon = MaterialDesignThemes.Wpf.PackIconKind.Translate;
                    if (value == 5) HeaderIcon = MaterialDesignThemes.Wpf.PackIconKind.CreditCardEditOutline;
                }
            }
        }
        private MaterialDesignThemes.Wpf.PackIconKind _headerIcon;
        public MaterialDesignThemes.Wpf.PackIconKind HeaderIcon
        {
            get => _headerIcon;
            set
            {
                if (_headerIcon != value)
                {
                    _headerIcon = value;
                    OnPropertyChanged(nameof(HeaderIcon));
                }
            }
        }
        public RelayCommand CancelCommand => new CardEditor.Commands.RelayCommand(_ => this.Close());
        #endregion

        #region Filter Setting
        private FilterSetting _filterSetting = new FilterSetting();
        public FilterSetting filterSetting
        {
            get => _filterSetting;
            set
            {
                if (_filterSetting != value)
                {
                    _filterSetting = value;
                    OnPropertyChanged(nameof(filterSetting));
                    SaveSettingCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        public RelayCommand SaveSettingCommand { get; set; }
        #endregion

        #region Replace Description
        private string _findWhat = string.Empty;
        public string FindWhat
        {
            get => _findWhat;
            set
            {
                if (_findWhat != value)
                {
                    _findWhat = value;
                    OnPropertyChanged(nameof(FindWhat));
                    ReplaceDescCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private string _replaceWith = string.Empty;
        public string ReplaceWith
        {
            get => _replaceWith;
            set
            {
                if (_replaceWith != value)
                {
                    _replaceWith = value;
                    OnPropertyChanged(nameof(ReplaceWith));
                    ReplaceDescCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private int _scope = -1;
        public int Scope
        {
            get => _scope;
            set
            {
                if (_scope != value)
                {
                    _scope = value;
                    OnPropertyChanged(nameof(Scope));
                    ReplaceDescCommand?.RaiseCanExecuteChanged();
                }    
            }
        }

        public RelayCommand ReplaceDescCommand { get; set; }
        #endregion

        #region Replace Field
        private string _replaceFieldFilePath = string.Empty;
        public string ReplaceFieldFilePath
        {
            get => _replaceFieldFilePath;
            set
            {
                _replaceFieldFilePath = value;
                OnPropertyChanged(nameof(ReplaceFieldFilePath));
                ReplaceFieldCommand?.RaiseCanExecuteChanged();
            }
        }
        private ulong _replaceFieldFlags;
        public ulong ReplaceFieldFlags
        {
            get => _replaceFieldFlags;
            set
            {
                _replaceFieldFlags = value;
                OnPropertyChanged(nameof(ReplaceFieldFlags));
                ReplaceFieldCommand?.RaiseCanExecuteChanged();
            }
        }
        private bool _isAddNew = false;
        public bool IsAddNew
        {
            get => _isAddNew;
            set
            {
                _isAddNew = value;
                OnPropertyChanged(nameof(IsAddNew));
            }
        }
        public BulkObservableCollection<ToggleMeta> ReplaceFieldToggleItems { get; }
        public IEnumerable<ToggleMeta> ReplaceFieldLeftToggles => ReplaceFieldToggleItems.Take(LeftCount);
        public IEnumerable<ToggleMeta> ReplaceFieldRightToggles => ReplaceFieldToggleItems.Skip(LeftCount);

        public RelayCommand ReplaceFieldCommand { get; set; }
        public RelayCommand BrowseReplaceFieldFilePathCommand { get; set; }
        #endregion

        private const int LeftCount = 18;

        #region Import Data
        private string _importDataFilePath = string.Empty;
        public string ImportDataFilePath
        {
            get => _importDataFilePath;
            set
            {
                _importDataFilePath = value;
                OnPropertyChanged(nameof(ImportDataFilePath));
                ImportDataCommand?.RaiseCanExecuteChanged();
            }
        }
        private ulong _importDataFlags;
        public ulong ImportDataFlags
        {
            get => _importDataFlags;
            set
            {
                _importDataFlags = value;
                OnPropertyChanged(nameof(ImportDataFlags));
                ImportDataCommand?.RaiseCanExecuteChanged();
            }
        }
        public BulkObservableCollection<ToggleMeta> ImportDataToggleItems { get; }
        public IEnumerable<ToggleMeta> ImportDataLeftToggles => ImportDataToggleItems.Take(LeftCount);
        public IEnumerable<ToggleMeta> ImportDataRightToggles => ImportDataToggleItems.Skip(LeftCount);

        public RelayCommand ImportDataCommand { get; set; }
        public RelayCommand BrowseImportDataFilePathCommand { get; set; }
        #endregion

        #region Pendulum Language
        private PendulumLanguageRule _selectedRule;
        public PendulumLanguageRule SelectedRule
        {
            get => _selectedRule;
            set
            {
                if (_selectedRule != value)
                {
                    _selectedRule = value;
                    OnPropertyChanged(nameof(SelectedRule));
                    ApplyPenLanguageCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private int _selectedPenLangScope = -1;
        public int SelectedPenLangScope
        {
            get => _selectedPenLangScope;
            set
            {
                if (_selectedPenLangScope != value)
                {
                    _selectedPenLangScope = value;
                    OnPropertyChanged(nameof(SelectedPenLangScope));
                }
            }
        }
        private bool _isApplyingPenLanguage;
        public bool IsApplyingPenLanguage
        {
            get => _isApplyingPenLanguage;
            set
            {
                if (_isApplyingPenLanguage != value)
                {
                    _isApplyingPenLanguage = value;
                    OnPropertyChanged(nameof(IsApplyingPenLanguage));
                }
            }
        }

        public RelayCommand ReloadPenLanguageCommand { get; set; }
        public RelayCommand OpenEditPenLanguageCommand { get; set; }
        public RelayCommand ApplyPenLanguageCommand { get; set; }
        public RelayCommand CancelPenLanguageCommand { get; set; }
        public RelayCommand RollbackPenLanguageCommand { get; set; }

        public RelayCommand OpenApplyPenLanguageCommand { get; set; }
        #endregion

        #region Credits
        private CreditItem _selectedCreditItem;
        public CreditItem SelectedCreditItem
        {
            get => _selectedCreditItem;
            set
            {
                if (_selectedCreditItem != value)
                {
                    _selectedCreditItem = value;
                    OnPropertyChanged(nameof(SelectedCreditItem));
                    OnSelectedCreditItemChanged();
                    ApplyCreditCommand?.RaiseCanExecuteChanged();
                    ResetCreditCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private void OnSelectedCreditItemChanged()
        {
            if (SelectedCreditItem != null)
            {
                Id = SelectedCreditItem.Id;
                CreditName = SelectedCreditItem.Name;
                CreditHeader = SelectedCreditItem.Header;
                CreditFooter = SelectedCreditItem.Footer;
                CreditDesc = SelectedCreditItem.Description;
            }
            else
            {
                Id = null;
                CreditName = string.Empty;
                CreditHeader = string.Empty;
                CreditFooter = string.Empty;
                CreditDesc = string.Empty;
            }
        }

        private bool _syncing;
        private int? _id;
        public int? Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged(nameof(Id));
                if (_syncing) return;
                try
                {
                    _syncing = true;
                    _idText = value?.ToString() ?? string.Empty;
                    OnPropertyChanged(nameof(IdText));
                }
                finally
                {
                    _syncing = false;
                }
                AddCreditCommand?.RaiseCanExecuteChanged();
                ModifyCreditCommand?.RaiseCanExecuteChanged();
            }
        }
        private string _idText = string.Empty;
        public string IdText
        {
            get => _idText;
            set
            {
                if (_idText == value) return;
                _idText = value;
                OnPropertyChanged(nameof(IdText));
                if (_syncing) return;
                try
                {
                    _syncing = true;
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        _id = null;
                        OnPropertyChanged(nameof(Id));
                    }
                    else if (int.TryParse(value, out int parsed))
                    {
                        _id = parsed;
                        OnPropertyChanged(nameof(Id));
                    }
                    else
                    {
                        _id = null;
                        OnPropertyChanged(nameof(Id));
                    }
                }
                finally
                {
                    _syncing = false;
                }
            }
        }
        private string _creditName = string.Empty;
        public string CreditName
        {
            get => _creditName;
            set
            {
                if (_creditName != value)
                {
                    _creditName = value;
                    OnPropertyChanged(nameof(CreditName));
                }
            }
        }
        private string _creditHeader = string.Empty;
        public string CreditHeader
        {
            get => _creditHeader;
            set
            {
                if (_creditHeader != value)
                {
                    _creditHeader = value;
                    OnPropertyChanged(nameof(CreditHeader));
                    AddCreditCommand?.RaiseCanExecuteChanged();
                    ModifyCreditCommand?.RaiseCanExecuteChanged();
                }    
            }
        }
        private string _creditFooter;
        public string CreditFooter
        {
            get => _creditFooter;
            set
            {
                if (_creditFooter != value)
                {
                    _creditFooter = value;
                    OnPropertyChanged(nameof(CreditFooter));
                }    
            }
        }
        private string _creditDesc = string.Empty;
        public string CreditDesc
        {
            get => _creditDesc;
            set
            {
                if (_creditDesc != value)
                {
                    _creditDesc = value;
                    OnPropertyChanged(nameof(CreditDesc));
                }
            }
        }

        private int _selectedScopeCredit = -1;
        public int SelectedScopeCredit
        {
            get => _selectedScopeCredit;
            set
            {
                if (_selectedScopeCredit != value)
                {
                    _selectedScopeCredit = value;
                    OnPropertyChanged(nameof(SelectedScopeCredit));
                }
            }
        }
        private int _selectedWriteModeCredit = -1;
        public int SelectedWriteModeCredit
        {
            get => _selectedWriteModeCredit;
            set
            {
                if (_selectedWriteModeCredit != value)
                {
                    _selectedWriteModeCredit = value;
                    OnPropertyChanged(nameof(SelectedWriteModeCredit));
                }
            }
        }

        private bool _isApplyingCredit;
        public bool IsApplyingCredit
        {
            get => _isApplyingCredit;
            set
            {
                if (_isApplyingCredit != value)
                {
                    _isApplyingCredit = value;
                    OnPropertyChanged(nameof(IsApplyingCredit));
                }
            }
        }

        public RelayCommand RollbackCreditCommand { get; set; }
        public RelayCommand RemoveCreditCommand { get; set; }
        public RelayCommand ApplyCreditCommand { get; set; }
        public RelayCommand CancelCreditCommand { get; set; }

        public RelayCommand AddCreditCommand { get; set; }
        public RelayCommand ModifyCreditCommand { get; set; }
        public RelayCommand ResetCreditCommand { get; set; }
        public RelayCommand ClearCreditCommand { get; set; }
        public RelayCommand SortCreditCommand { get; set; }
        public RelayCommand DeleteCreditCommand { get; set; }
        #endregion

        #region Constructor
        public ItemsEditor(ItemsEdit tabIndex = ItemsEdit.Setting)
        {

            ReplaceFieldToggleItems = new BulkObservableCollection<ToggleMeta>();
            ImportDataToggleItems = new BulkObservableCollection<ToggleMeta>();

            int ReplaceFieldIndex = 0;
            int ImportDataIndex = 0;
            /////////////////////
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.cardName.ToText() });
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.cardDesc.ToText() });
            for (int i = 0; i < 16; i++)
            {
                ReplaceFieldToggleItems.Add(new ToggleMeta
                {
                    Index = ReplaceFieldIndex++,
                    Label = $"{CMess.cardStr.ToText()} {i + 1}"
                });
            }
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.cardLabelScope.ToText() });
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.cardAlias.ToText() });
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.cardlabelSetCode.ToText() });
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.cardLabelType.ToText() });
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.cardatk.ToText() });
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.carddef.ToText() });
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.Level.ToText() });
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.cardLabelRace.ToText() });
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.cardLabelAttri.ToText() });
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.cardLabelCategory.ToText() });
            ReplaceFieldToggleItems.Add(new ToggleMeta { Index = ReplaceFieldIndex++, Label = CMess.cardLabelFlag.ToText() });
            /////////////////////
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.cardName.ToText() });
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.cardDesc.ToText() });
            for (int i = 0; i < 16; i++)
            {
                ImportDataToggleItems.Add(new ToggleMeta
                {
                    Index = ImportDataIndex++,
                    Label = $"{CMess.cardStr.ToText()} {i + 1}"
                });
            }
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.cardLabelScope.ToText() });
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.cardAlias.ToText() });
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.cardlabelSetCode.ToText() });
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.cardLabelType.ToText() });
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.cardatk.ToText() });
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.carddef.ToText() });
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.Level.ToText() });
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.cardLabelRace.ToText() });
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.cardLabelAttri.ToText() });
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.cardLabelCategory.ToText() });
            ImportDataToggleItems.Add(new ToggleMeta { Index = ImportDataIndex++, Label = CMess.cardLabelFlag.ToText() });

            InitializeCommand();
            InitializeComponent();

            DataContext = this;
            TabHeaderIndex = (int)tabIndex;
        }
        private void InitializeCommand()
        {
            SaveSettingCommand = new CardEditor.Commands.RelayCommand(async _ => await SaveSetting(), _ => CanSaveSetting());
            ReplaceDescCommand = new CardEditor.Commands.RelayCommand(async _ => await ReplaceDesc(), _ => CanSaveSetting() && CanReplaceDesc());
            
            ReplaceFieldCommand = new CardEditor.Commands.RelayCommand(async _ => await ReplaceField(), _ => CanReplaceField());
            BrowseReplaceFieldFilePathCommand = new CardEditor.Commands.RelayCommand(_ => BrowseReplaceFieldFilePath());
            
            ImportDataCommand = new CardEditor.Commands.RelayCommand(async _ => await ImportData(), _ => CanImportData());
            BrowseImportDataFilePathCommand = new CardEditor.Commands.RelayCommand(_ => BrowseImportDataFilePath());

            ReloadPenLanguageCommand = new CardEditor.Commands.RelayCommand(async _ => await ReloadPenLanguage());
            OpenEditPenLanguageCommand = new CardEditor.Commands.RelayCommand(_ => OpenEditPenLanguage());
            ApplyPenLanguageCommand = new CardEditor.Commands.RelayCommand(async _ => await ApplyPenLanguage(), _ => CanApplyPenLanguage());
            CancelPenLanguageCommand = new CardEditor.Commands.RelayCommand(_ => CancelPenLanguage());
            RollbackPenLanguageCommand = new CardEditor.Commands.RelayCommand(_ => RollBackPenLanguage());
            OpenApplyPenLanguageCommand = new CardEditor.Commands.RelayCommand(_ => OpenApplyPenLanguage());

            RollbackCreditCommand = new CardEditor.Commands.RelayCommand(_ => RollBackCredit());
            RemoveCreditCommand = new CardEditor.Commands.RelayCommand(async _ => await RemoveCredit());
            ApplyCreditCommand = new CardEditor.Commands.RelayCommand(async _ => await ApplyCredit(), _ => SelectedCreditNotNull());
            CancelCreditCommand = new CardEditor.Commands.RelayCommand( _ => CancelCredit());

            AddCreditCommand = new CardEditor.Commands.RelayCommand(async _ => await AddCredit(), _ => CanModifyCredit());
            ModifyCreditCommand = new CardEditor.Commands.RelayCommand(async _ => await ModifyCredit(), _ => CanModifyCredit());
            ResetCreditCommand = new CardEditor.Commands.RelayCommand(async _ => await ResetCredit(), _ => SelectedCreditNotNull());
            ClearCreditCommand = new CardEditor.Commands.RelayCommand(async _ => await ClearCredit());
            SortCreditCommand = new CardEditor.Commands.RelayCommand(_ => SortCredit());
            DeleteCreditCommand = new CardEditor.Commands.RelayCommand(async _ => await DeleteCredit(), _ => SelectedCreditNotNull());
            
        }
        #endregion

        #region Load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeContentMenu();
            // TabHeaderIndex = 0;
            filterSetting.SetValue(ConfigViewModel.Instance.dataHandlingSetting.Advanced);
        }
        private void InitializeContentMenu()
        {
            ControlContextMenuService.Attach(txtfindwhat);
            ControlContextMenuService.Attach(txtreplacewith);

            ControlContextMenuService.Attach(txtReplaceFieldFilePath);
            ControlContextMenuService.Attach(txtImportDataFilePath);

            ControlContextMenuService.Attach(txtIdText);
            ControlContextMenuService.Attach(txtCreditName);
            ControlContextMenuService.Attach(txtCreditHeader);
            ControlContextMenuService.Attach(txtCreditFooter);
            ControlContextMenuService.Attach(txtCreditDesc);
        }
        #endregion

        #region Commands

        #region Setting
        private async Task SaveSetting()
        {
            int advanced = filterSetting.GetValue();
            ConfigViewModel.Instance.dataHandlingSetting.Advanced = advanced;
            var (result, message) = ConfigViewModel.Instance.SaveSingleSettingFile("DataHandlingSetting", ConfigViewModel.Instance.dataHandlingSetting);
            await Task.CompletedTask;
        }
        private bool CanSaveSetting()
        {
            return true;
        }
        #endregion

        #region Replace Description
        private async Task ReplaceDesc()
        {
            await SaveSetting();
            await Task.Delay(100);

            var result = await MainWindowReference.ReplaceDesc(FindWhat, ReplaceWith, Scope);

            if (result == null)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    CMess.errorOcc.ToText(), new[] { CMess.ok.ToText() });
                return;
            }

            if (result.Succeeded)
            {
                var msg = string.Format(CMess.replaceSuc.ToText(), result.FilteredCount, result.TotalCount);

                int choice = CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, msg,
                    new[] { CMess.ok.ToText(), CMess.cancel.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Replace.ToText(), CMess.Card.ToText())} {result.Message}",
                    new[] { CMess.ok.ToText() });
            }
        }
        private bool CanReplaceDesc()
        {
            if (string.IsNullOrWhiteSpace(FindWhat)) return false;
            if (string.IsNullOrWhiteSpace(ReplaceWith)) return false;
            if (Scope < 0 || Scope > 2) return false;
            return true;
        }
        #endregion

        #region Replace Field
        private bool CheckDuplicateIds()
        {
            var (dupResule, dupListID) = MainWindowReference.CheckDuplicateIds();
            if (dupResule && dupListID.Count > 0)
            {
                var preview = dupListID.Take(10);
                var more = dupListID.Count > 10 ? "\n..." : "";

                var result = CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    $"{string.Format(CMess.HasDuplicateIDs.ToText(), dupListID.Count())} {CMess.QuestContinue.ToText()}\n{CMess.dupliListID.ToText()}\n{string.Join("\n", preview) + more}",
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result == 1) return true;
                else return false;
            }
            return true;
        }
        private async Task ReplaceField()
        {
            if (MainWindowReference != null)
            {
                if (!CheckDuplicateIds()) return;

                var resultReplaceField = await MainWindowReference.ReplaceField(ReplaceFieldFilePath, ReplaceFieldFlags, IsAddNew);

                if (resultReplaceField.Success) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.replaceSuc.ToText(), resultReplaceField.ReplacedCard.ToString(), resultReplaceField.TotalCard.ToString()),
                    new[] { CMess.ok.ToText() });
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {resultReplaceField.Message}", new[] { CMess.error.ToText() });
            } 
        }
        private bool CanReplaceField()
        {
            return (!string.IsNullOrWhiteSpace(ReplaceFieldFilePath) && System.IO.File.Exists(ReplaceFieldFilePath) && ReplaceFieldFlags != 0);
        }
        private void BrowseReplaceFieldFilePath()
        {
            string filePath = FileDiaLogHelper.OpenCardList();
            if (string.IsNullOrWhiteSpace(filePath)) return;
            ReplaceFieldFilePath = filePath;
        }
        #endregion

        #region Import Data
        private async Task ImportData()
        {
            if (MainWindowReference != null)
            {
                if (!CheckDuplicateIds()) return;

                var (resultReplace, messageReplace) = await MainWindowReference.ImportData(ImportDataFilePath, ImportDataFlags);
                if (resultReplace) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.replaceSuc.ToText(), messageReplace, CMess.Card.ToText()), new[] { CMess.ok.ToText() });
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageReplace}", new[] { CMess.error.ToText() });
            }
        }
        private bool CanImportData()
        {
            return (!string.IsNullOrWhiteSpace(ImportDataFilePath) && System.IO.File.Exists(ImportDataFilePath));
        }
        private void BrowseImportDataFilePath()
        {
            string filePath = FileDiaLogHelper.OpenCardList();
            if (string.IsNullOrWhiteSpace(filePath)) return;
            ImportDataFilePath = filePath;
        }
        #endregion

        #region Pendulum Language
        private async Task ReloadPenLanguage()
        {
            var (resultPenLang, messagePenLang) = await PenLanguageViewModel.Instance.LoadAsync();
            if (resultPenLang)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.tlReload.ToText(), CMess.PendulumLanguage.ToText()),
                    new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messagePenLang}", new[] { CMess.ok.ToText() });
            }
        }
        private void OpenEditPenLanguage()
        {
            PenLangHost.IsRightDrawerOpen = true;
        }
        private async Task ApplyPenLanguage()
        {
            if (SelectedRule == null) return;

            try
            {
                IsApplyingPenLanguage = true;

                if (MainWindowReference != null)
                {
                    if (PenLanguageViewModel.Instance.HasLastSnapshot())
                    {
                        int confirm = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                            $"{CMess.HasSnapshot.ToText()} {CMess.QuestContinue.ToText()}",
                            new[] { CMess.yes.ToText(), CMess.no.ToText() });

                        if (confirm != 0) return;
                    }

                    var resultPenLang = await MainWindowReference.PendulumLanguage(SelectedRule, SelectedPenLangScope, true);
                    if (resultPenLang == null || !resultPenLang.Result)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Apply.ToText(), CMess.PendulumLanguage.ToText())} {resultPenLang.Message}",
                            new[] { CMess.ok.ToText() });
                    }
                    else
                    {
                        CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                            $"{string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Apply.ToText(), CMess.PendulumLanguage.ToText())}\n{resultPenLang.Message}",
                            new[] { CMess.ok.ToText() });
                    }
                }
            }
            catch { }
            finally
            {
                IsApplyingPenLanguage = false;
            }
        }
        private void CancelPenLanguage()
        {
            if (MainWindowReference != null)
            {
                MainWindowReference.StopApplyPenLang();
            }
        }
        private void RollBackPenLanguage()
        {
            if (MainWindowReference != null)
            {
                int resultChoose = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question,
                    string.Format(CMess.TwoPlaceholderConfirm.ToText(), CMess.RollBack.ToText(), CMess.PendulumLanguage.ToText()),
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (resultChoose != 0) return;

                var (resultRollback, messageRollback) = MainWindowReference.RollBackPenlang();
                if (resultRollback)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.RollBack.ToText(), CMess.PendulumLanguage.ToText()),
                        new[] { CMess.ok.ToText() });
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {messageRollback}", new[] { CMess.ok.ToText() });
                }
            }
        }
        private void OpenApplyPenLanguage()
        {
            PenLangHost.IsRightDrawerOpen = false;
        }
        private bool CanApplyPenLanguage()
        {
            return SelectedRule != null;
        }
        #endregion

        #region Credits
        private void RollBackCredit()
        {
            if (MainWindowReference != null)
            {
                var (result, message) = MainWindowReference.RollbackCredit();
                if (result)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.RollBack.ToText(), CMess.CreditTeam.ToText()),
                        new[] { CMess.ok.ToText() });
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                }
            }
        }
        private async Task RemoveCredit()
        {
            if (MainWindowReference != null)
            {
                var resultCredit = await MainWindowReference.RemoveCredit(SelectedScopeCredit);
                if (resultCredit == null)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        CMess.errorOcc.ToText(), new[] { CMess.ok.ToText() });
                    return;
                }
                if (resultCredit.Succeeded)
                {
                    string msg = string.Format(CMess.changeSuc.ToText(), CMess.CreditTeam.ToText(), resultCredit.FilteredCount, resultCredit.TotalCount);
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, msg, new[] { CMess.ok.ToText(), CMess.cancel.ToText() });
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Replace.ToText(), CMess.Card.ToText())} {resultCredit.Message}",
                        new[] { CMess.ok.ToText() });
                }
            }
        }
        private async Task ApplyCredit()
        {
            if (SelectedCreditItem == null) return;

            try
            {
                IsApplyingCredit = true;

                if (MainWindowReference != null)
                {
                    int WriteModeCredit = SelectedWriteModeCredit;

                    if (SelectedWriteModeCredit == 0)
                    {
                        int quesstWriteMode = CMSG.Show(CMess.questi.ToText(), CMSG.MessageBoxIconType.Question, CMess.confirmWriteData.ToText(),
                            new[] { CMess.OverwriteDupli.ToText(), CMess.OverwriteAll.ToText(), CMess.Appendwrite.ToText(), CMess.Skip.ToText(), CMess.cancel.ToText() });
                        if (quesstWriteMode == 0) WriteModeCredit = 1; // OverwriteDupli
                        else if (quesstWriteMode == 1) WriteModeCredit = 2; // OverwriteAll
                        else if (quesstWriteMode == 2) WriteModeCredit = 3; // Appendwrite
                        else if (quesstWriteMode == 3) WriteModeCredit = 4; // Skip
                        else return; // Cancel
                    }

                    var resultCredit = await MainWindowReference.CreditTeam(SelectedCreditItem, SelectedScopeCredit, WriteModeCredit);
                    if (resultCredit == null)
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            CMess.errorOcc.ToText(), new[] { CMess.ok.ToText() });
                        return;
                    }
                    if (resultCredit.Succeeded)
                    {
                        string msg = string.Format(CMess.changeSuc.ToText(), CMess.CreditTeam.ToText(), resultCredit.FilteredCount, resultCredit.TotalCount);
                        CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, msg, new[] { CMess.ok.ToText(), CMess.cancel.ToText() });
                    }
                    else
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Replace.ToText(), CMess.Card.ToText())} {resultCredit.Message}",
                            new[] { CMess.ok.ToText() });
                    }
                }
            }
            catch { }
            finally
            {
                IsApplyingCredit = false;
            }
        }
        private bool SelectedCreditNotNull()
        {
            return SelectedCreditItem != null;
        }
        private void CancelCredit()
        {
            if (MainWindowReference != null)
            {
                MainWindowReference.StopApplyCredits();
            }
        }

        private async Task AddCredit()
        {
            if (Id == null || Id <= 0 || string.IsNullOrWhiteSpace(CreditName)) return;

            CreditItem newitem = new CreditItem
            {
                Id = Id.Value,
                Name = CreditName,
                Header = CreditHeader,
                Footer = CreditFooter,
                Description = CreditDesc
            };

            var (resultDB, messageDB) = await CreditsViewModel.Instance.ModifyCreditsDatabase(newitem);
            if (resultDB)
            {
                var (resultItem, isAdd) = CreditsViewModel.Instance.ModifyCreditItem(newitem);
                if (resultItem)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.ThreePlaceholderSuccess.ToText(), 1.ToString(), CMess.CreditTeam.ToText(), CMess.tlAdd.ToText()),
                        new[] { CMess.ok.ToText() });
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.CreditTeam.ToText())}",
                        new[] { CMess.ok.ToText() });
                }
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.CreditTeam.ToText())} {messageDB}", new[] { CMess.ok.ToText() });
            }
        }
        private async Task ModifyCredit()
        {
            if (Id == null || Id <= 0 || string.IsNullOrWhiteSpace(CreditName)) return;

            CreditItem newitem = new CreditItem
            {
                Id = Id.Value,
                Name = CreditName,
                Header = CreditHeader,
                Footer = CreditFooter,
                Description = CreditDesc
            };

            var (resultDB, messageDB) = await CreditsViewModel.Instance.ModifyCreditsDatabase(newitem);
            if (resultDB)
            {
                var (resultItem, isAdd) = CreditsViewModel.Instance.ModifyCreditItem(newitem);
                if (resultItem)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.ThreePlaceholderSuccess.ToText(), 1.ToString(), CMess.CreditTeam.ToText(), CMess.Update.ToText()),
                        new[] { CMess.ok.ToText() });
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.CreditTeam.ToText())}", new[] { CMess.ok.ToText() });
                }
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlAdd.ToText(), CMess.CreditTeam.ToText())} {messageDB}", new[] { CMess.ok.ToText() });
            }
        }
        private bool CanModifyCredit()
        {
            return (Id != null && Id > 0 && !string.IsNullOrWhiteSpace(CreditHeader));
        }
        private async Task ResetCredit()
        {
            OnSelectedCreditItemChanged();
        }
        private async Task ClearCredit()
        {
            Id = null;
            CreditName = string.Empty;
            CreditHeader = string.Empty;
            CreditFooter = string.Empty;
            CreditDesc = string.Empty;
        }
        private void SortCredit()
        {

        }
        private async Task DeleteCredit()
        {
            if (SelectedCreditItem == null) return;

            var (resultDB, messageDB) = await CreditsViewModel.Instance.RemoveCreditDatabase(SelectedCreditItem);
            if(resultDB)
            {
                bool resultItem = CreditsViewModel.Instance.RemoveCreditItem(SelectedCreditItem);
                if (resultItem)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        string.Format(CMess.ThreePlaceholderSuccess.ToText(), 1.ToString(), CMess.CreditTeam.ToText(), CMess.tlDelete.ToText()),
                        new[] { CMess.ok.ToText() });
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.CreditTeam.ToText())}", new[] { CMess.ok.ToText() });
                }
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlDelete.ToText(), CMess.CreditTeam.ToText())} {messageDB}", new[] { CMess.ok.ToText() });
            }
        }

        #endregion

        #endregion

        #region Event Handlers
        private void ReplaceFieldToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton tb && tb.DataContext is ToggleMeta meta)
                ReplaceFieldFlags |= (1UL << meta.Index);
        }
        private void ReplaceFieldToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton tb && tb.DataContext is ToggleMeta meta)
                ReplaceFieldFlags &= ~(1UL << meta.Index);
        }

        private void ImportDataToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton tb && tb.DataContext is ToggleMeta meta)
                ImportDataFlags |= (1UL << meta.Index);
        }
        private void ImportDataToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton tb && tb.DataContext is ToggleMeta meta)
                ImportDataFlags &= ~(1UL << meta.Index);
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

    }
}
