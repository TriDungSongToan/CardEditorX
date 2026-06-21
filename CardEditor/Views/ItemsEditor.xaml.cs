using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Models;
using CardEditor.Models.Settings;
using CardEditor.Helpers;
using CardEditor.Commands;
using CardEditor.ViewModels;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using System.Diagnostics;

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
        private ImageSource _headerIcon;
        public ImageSource HeaderIcon
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
        private int _tabHeaderIndex = 0;
        public int TabHeaderIndex
        {
            get => _tabHeaderIndex;
            set
            {
                if (_tabHeaderIndex != value)
                {
                    _tabHeaderIndex = value;
                    OnPropertyChanged(nameof(TabHeaderIndex));
                    if (value == 0) iconHeader.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Setting/Setting.ico"));
                    if (value == 1) iconHeader.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Menu/Data/Replace.ico"));
                    if (value == 2) iconHeader.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Menu/Data/Replace.ico"));
                    if (value == 3) iconHeader.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Menu/Data/Import/Import.ico"));
                }
            }
        }
        #endregion

        #region Advanced Setting
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
        #endregion

        private const int LeftCount = 18;

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
        #endregion

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
        #endregion

        #region Commands
        public RelayCommand CancelCommand => new CardEditor.Commands.RelayCommand(_ => this.Close());
        public RelayCommand SaveSettingCommand { get; set; }
        public RelayCommand ReplaceDescCommand { get; set; }
        public RelayCommand ReplaceFieldCommand { get; set; }
        public RelayCommand BrowseReplaceFieldFilePathCommand { get; set; }
        public RelayCommand ImportDataCommand { get; set; }
        public RelayCommand BrowseImportDataFilePathCommand { get; set; }
        #endregion

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
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            iconHeader.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Setting/Setting.ico"));
            filterSetting.SetValue(ConfigViewModel.Instance.dataHandlingSetting.Advanced);
        }

        #region Commands
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

        private async Task ReplaceDesc()
        {
            await SaveSetting();
            await Task.Delay(100);

            var (resultReplace, messageReplace) = await MainWindowReference.ReplaceDesc(FindWhat, ReplaceWith, Scope);

            if (resultReplace)
            {
                var result = CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    CMess.replaceSuc.ToText(), new[] { CMess.ok.ToText(), CMess.cancel.ToText() });
                if (result == 0) this.Close();
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageReplace}", new[] { CMess.ok.ToText() });
            }
        }
        private bool CanReplaceDesc()
        {
            if (string.IsNullOrWhiteSpace(FindWhat)) return false;
            if (string.IsNullOrWhiteSpace(ReplaceWith)) return false;
            if (Scope < 0 || Scope > 2) return false;
            return true;
        }

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

                var (resultReplaceField, messageReplaceField) = await MainWindowReference.ReplaceField(ReplaceFieldFilePath, ReplaceFieldFlags, IsAddNew);
                if (resultReplaceField) CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.replaceSuc.ToText(), messageReplaceField, CMess.Card.ToText()), new[] { CMess.ok.ToText() });
                else CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {messageReplaceField}", new[] { CMess.error.ToText() });
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
