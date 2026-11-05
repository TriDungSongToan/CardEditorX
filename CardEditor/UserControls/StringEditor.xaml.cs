using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Services;
using CardEditor.Commands;
using CardEditor.Behaviors;
using CardEditor.Interfaces;
using CardEditor.Localization;
using CardEditor.Services.LoadData;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for StringEditor.xaml
    /// </summary>
    public partial class StringEditor : UserControl, INotifyPropertyChanged, IDisposable, ISaveable
    {
        #region Properties
        private IMainWindowService MainWindowService;
        private AppTitle _mainWindowTitle = new();
        public AppTitle MainWindowTitle
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
                    MainWindowService.UpdateTabItemHeader(this, MainWindowTitle.DisplayTabItemHeader);
                }
            }
        }
        private void RebuildWindowTitle()
        {
            MainWindowTitle = FileLocationService.BuildTitleTranslation(TranslationFilePath, SourceName, TranslationName);
        }

        #region Data
        public StringConf SourceFileData { get; set; }
        public StringConf TranslationFileData { get; set; }
        private StringCombine _combineData;
        public StringCombine CombineData
        {
            get => _combineData;
            set
            {
                if (_combineData != value)
                {
                    _combineData = value;
                    OnPropertyChanged(nameof(CombineData));
                }
            }
        }
        private StringSectionCombine _selectedSection;
        public StringSectionCombine SelectedSection
        {
            get => _selectedSection;
            set
            {
                if (_selectedSection != value)
                {
                    _selectedSection = value;
                    OnPropertyChanged(nameof(SelectedSection));

                }
            }
        }
        #endregion

        #region File Paths
        private string _sourceFilePath = string.Empty;
        public string SourceFilePath
        {
            get => _sourceFilePath;
            set
            {
                if (_sourceFilePath != value)
                {
                    _sourceFilePath = value;
                    OnPropertyChanged(nameof(SourceFilePath));
                    MergeFileCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private string _translationFilePath = string.Empty;
        public string TranslationFilePath
        {
            get => _translationFilePath;
            set
            {
                if (_translationFilePath != value)
                {
                    _translationFilePath = value;
                    OnPropertyChanged(nameof(TranslationFilePath));
                    MergeFileCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        #endregion

        #region Display Names
        private string _sourceName = CMess.SourceContent.ToText();
        public string SourceName
        {
            get => _sourceName;
            set
            {
                if (_sourceName != value)
                {
                    _sourceName = value;
                    OnPropertyChanged(nameof(SourceName));
                }
            }
        }
        private string _translationName = CMess.TranslationContent.ToText();
        public string TranslationName
        {
            get => _translationName;
            set
            {
                if (_translationName != value)
                {
                    _translationName = value;
                    OnPropertyChanged(nameof(TranslationName));
                }
            }
        }
        #endregion

        #region Status
        private int _totalCount, _translatedCount, _untranslatedCount, _newCount, _removedCount, _disabledCount;
        public int TotalCount { get => _totalCount; set => SetCount(ref _totalCount, value); }
        public int TranslatedCount { get => _translatedCount; set => SetCount(ref _translatedCount, value); }
        public int UntranslatedCount { get => _untranslatedCount; set => SetCount(ref _untranslatedCount, value); }
        public int NewCount { get => _newCount; set => SetCount(ref _newCount, value); }
        public int RemovedCount { get => _removedCount; set => SetCount(ref _removedCount, value); }
        public int DisabledCount { get => _disabledCount; set => SetCount(ref _disabledCount, value); }

        private void SetCount(ref int field, int value, [CallerMemberName] string name = null)
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(name);
        }

        private CancellationTokenSource _notificationCts;
        private string _notification = string.Empty;
        public string Notification
        {
            get => _notification;
            set
            {
                if (_notification != value)
                {
                    _notification = value;
                    OnPropertyChanged(nameof(Notification));
                    if (!string.IsNullOrEmpty(value))
                        _ = StartNotificationTimerAsync();
                }
            }
        }
        private async Task StartNotificationTimerAsync()
        {
            _notificationCts?.Cancel();
            _notificationCts?.Dispose();

            var cts = new CancellationTokenSource();
            _notificationCts = cts;

            try
            {
                await Task.Delay(5000, cts.Token);
                if (_notificationCts == cts)
                    Notification = string.Empty;
            }
            catch (OperationCanceledException) { }
            finally
            {
                if (_notificationCts == cts)
                {
                    _notificationCts.Dispose();
                    _notificationCts = null;
                }
            }
        }

        private string _summaryText = string.Empty;
        public string SummaryText
        {
            get => _summaryText;
            set
            {
                if (_summaryText != value)
                {
                    _summaryText = value;
                    OnPropertyChanged(nameof(SummaryText));
                }
            }
        }
        #endregion

        private bool _isSaved = false;
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
        #endregion

        #region Commands
        public RelayCommand BrowseSourceFileCommand { get; set; }
        public RelayCommand BrowseTranslationFileCommand { get; set; }
        public RelayCommand MergeFileCommand { get; set; }
        public RelayCommand SaveFileCommand { get; set; }
        public RelayCommand SaveAsFileCommand { get; set; }
        #endregion

        #region Constructor
        public StringEditor()
        {
            InitializeComponent();
            InitializeCommands();

            this.DataContext = this;
        }
        public StringEditor(IMainWindowService mainWindowService) : this()
        {
            MainWindowService = mainWindowService;
        }
        private void InitializeCommands()
        {
            BrowseSourceFileCommand = new RelayCommand(_ => BrowseSourceFile());
            BrowseTranslationFileCommand = new RelayCommand(_ => BrowseTranslationFile());
            MergeFileCommand = new RelayCommand(async _ => await MergeFile(), _ => CanMergeFile());

            SaveFileCommand = new RelayCommand(async _ => await Save());
            SaveAsFileCommand = new RelayCommand(async _ => await SaveAs());
        }
        #endregion

        #region Load
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            tbtnFilepath.IsChecked = false;

            IsSaved = true;
        }
        public void LoadConfig()
        {
            if (CombineData == null)
            {
                MainWindowTitle = new AppTitle
                {
                    DisplayTitle = CMess.StringsEdit.ToText(),
                    DisplayTabItemHeader = CMess.StringsEdit.ToText(),
                    PhysicalFullPath = string.Empty
                };
            }
            else
            {
                RebuildWindowTitle();
            }
        }
        #endregion

        #region Commands
        private void BrowseSourceFile()
        {
            string filePath = FileDiaLogHelper.OpenConf();
            if (!string.IsNullOrEmpty(filePath)) SourceFilePath = filePath;
        }
        private void BrowseTranslationFile()
        {
            string filePath = FileDiaLogHelper.OpenConf();
            if (!string.IsNullOrEmpty(filePath)) TranslationFilePath = filePath;
        }
        private async Task MergeFile()
        {
            StringInterface _service = new LoadStringService();

            if (string.IsNullOrEmpty(SourceFilePath) || !File.Exists(SourceFilePath))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    string.Format(CMess.PlaceholderInva.ToText(), CMess.SourceFile.ToText()), new[] { CMess.ok.ToText() });
                return;
            }
            if (string.IsNullOrEmpty(TranslationFilePath) || !File.Exists(TranslationFilePath))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    string.Format(CMess.PlaceholderInva.ToText(), CMess.TranslationFile.ToText()), new[] { CMess.ok.ToText() });
                return;
            }

            if (CombineData != null && CombineData.Sections.Count > 0 && !IsSaved)
            {
                int result = CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    $"{CMess.HasUnSaveData.ToText()} {CMess.QuestContinue.ToText()}",
                    new[] { CMess.yes.ToText(), CMess.no.ToText() });
                if (result != 0) return;
            }

            try
            {
                var result = await Task.Run(() =>
                {
                    var source = _service.LoadStringConf(SourceFilePath);
                    var translation = _service.LoadStringConf(TranslationFilePath);
                    return (Source: source, Translation: translation, Combine: _service.Merge(source, translation));
                });

                SourceFileData = result.Source;
                TranslationFileData = result.Translation;
                SelectedSection = null;
                CombineData = result.Combine;
                SelectedSection = CombineData.Sections.FirstOrDefault();

                foreach (var s in CombineData.Sections)
                    foreach (var r in s.Content)
                        r.PropertyChanged += Row_PropertyChanged;
                IsSaved = true;
                RefreshCounters();

                ShowSummary(result.Source, result.Combine);

                SourceName = new DirectoryInfo(Path.GetDirectoryName(SourceFilePath)).Name;
                TranslationName = new DirectoryInfo(Path.GetDirectoryName(TranslationFilePath)).Name;

                RebuildWindowTitle();

                Notification = string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Merge.ToText(), CMess.File.ToText());
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.load.ToText(), CMess.File.ToText())} {ex.Message}",
                    new[] { CMess.ok.ToText() });
            }
        }
        private void ShowSummary(StringConf en, StringCombine combine)
        {
            int total = 0, news = 0, untranslated = 0, disabled = 0;
            foreach (var s in combine.Sections)
                foreach (var r in s.Content)
                {
                    if (r.IsDisabled) { disabled++; continue; }
                    total++;
                    if (r.Status == StatusConf.New) news++;
                    else if (r.Status == StatusConf.Untranslated) untranslated++;
                }

            SummaryText = $"{total} lines | New: {news} | Untranslated: {untranslated} | " +
                          $"Disable: {disabled} | Removed: {combine.RemovedCount} | Warnings: {en.Warnings.Count}";
        }
        private bool CanMergeFile()
        {
            return !string.IsNullOrEmpty(SourceFilePath) && System.IO.File.Exists(SourceFilePath)
                && !string.IsNullOrEmpty(TranslationFilePath) && System.IO.File.Exists(TranslationFilePath);
        }

        private void RefreshCounters()
        {
            int total = 0, translated = 0, untranslated = 0, news = 0, disabled = 0;

            foreach (var s in CombineData.Sections)
                foreach (var r in s.Content)
                {
                    total++;
                    if (r.IsDisabled) { disabled++; continue; }
                    switch (r.Status)
                    {
                        case StatusConf.Translated: translated++; break;
                        case StatusConf.Untranslated: untranslated++; break;
                        case StatusConf.New: news++; break;
                    }
                }

            TotalCount = total;
            TranslatedCount = translated;
            UntranslatedCount = untranslated;
            NewCount = news;
            RemovedCount = CombineData.RemovedCount;
            DisabledCount = disabled;
        }
        #endregion

        #region Save
        private bool _isSaving;
        public async Task<bool> Save()
        {
            if (string.IsNullOrEmpty(TranslationFilePath))
                return await SaveAs();
            return await WriteFileAsync(TranslationFilePath);
        }
        public async Task<bool> SaveAs()
        {
            string filePath = FileDiaLogHelper.SaveConfig("strings");
            if (string.IsNullOrEmpty(filePath)) return false;

            if (!await WriteFileAsync(filePath)) return false;
            TranslationFilePath = filePath;
            TranslationName = new DirectoryInfo(Path.GetDirectoryName(filePath)).Name;
            return true;
        }
        private async Task<bool> WriteFileAsync(string path)
        {
            if (_isSaving) return false;          // chặn lưu chồng khi bấm Ctrl+S liên tục
            _isSaving = true;

            try
            {
                // Luồng UI: chốt ô đang sửa rồi chụp dữ liệu
                RowsGrid.CommitEdit(DataGridEditingUnit.Row, true);
                var lines = BuildLines();

                // Luồng nền: chỉ ghi file (ghi file tạm rồi thay thế để lỗi giữa chừng không hỏng file cũ)
                await Task.Run(() =>
                {
                    string tmp = path + ".tmp";
                    File.WriteAllLines(tmp, lines, new UTF8Encoding(false));
                    if (File.Exists(path)) File.Replace(tmp, path, null);
                    else File.Move(tmp, path);
                });

                IsSaved = true;

                Notification = string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Save.ToText(), CMess.File.ToText());
                return true;
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Save.ToText(), CMess.File.ToText())} {ex.Message}",
                    new[] { CMess.ok.ToText() });
                return false;
            }
            finally
            {
                _isSaving = false;
            }
        }
        private List<string> BuildLines()
        {
            var lines = new List<string>();
            foreach (var s in CombineData.Sections)
            {
                if (s.SectionName.Length > 0 || s.Content.Count == 0)
                    lines.Add("#" + s.SectionName);

                foreach (var r in s.Content)
                {
                    string prefix = r.IsDisabled ? "#!" : "!";
                    lines.Add(prefix + r.Token + " " + r.ID + " " + r.TranslationContent);
                }
            }
            return lines;
        }

        private void UpdateWindowSavedFlag()
        {
            if (MainWindowService != null)
            {
                MainWindowService.UpdateWindowSavedFlag(IsSaved);
            }
        }
        #endregion

        #region Section drag & drop
        private Point _dragStartPoint;
        private const string SectionDragFormat = "CardEditor.SectionCombine";
        private const string RowsDragFormat = "CardEditor.Rows";

        private InsertionAdorner _adorner;
        private ListBoxItem _adornerItem;
        private bool _adornerBelow;

        private void SectionItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
        }
        private void SectionItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Vector delta = e.GetPosition(null) - _dragStartPoint;
            if (Math.Abs(delta.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(delta.Y) < SystemParameters.MinimumVerticalDragDistance) return;

            var item = sender as ListBoxItem;
            var section = item != null ? item.DataContext as StringSectionCombine : null;
            if (section == null) return;

            try
            {
                DragDrop.DoDragDrop(item, new DataObject(SectionDragFormat, section), DragDropEffects.Move);
            }
            finally
            {
                ClearInsertionMark();   // dọn dẹp dù thả thành công hay bị hủy (Esc)
            }
        }
        private void SectionItem_DragEnter(object sender, DragEventArgs e)
        {

        }
        private void SectionItem_DragOver(object sender, DragEventArgs e)
        {
            var item = (ListBoxItem)sender;

            if (e.Data.GetDataPresent(RowsDragFormat))
            {
                var data = e.Data.GetData(RowsDragFormat) as StringRowsDragData;
                var target = item.DataContext as StringSectionCombine;

                if (data != null && target != null && target != data.From)
                {
                    ClearInsertionMark();
                    ShowRowTarget(item);
                    e.Effects = DragDropEffects.Move;
                }
                else
                {
                    ClearRowTarget();
                    e.Effects = DragDropEffects.None;   // thả vào chính nhóm cũ: không làm gì
                }
                AutoScroll(e);
                e.Handled = true;
                return;
            }

            if (!e.Data.GetDataPresent(SectionDragFormat))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            ClearRowTarget();
            bool below = e.GetPosition(item).Y > item.ActualHeight / 2;
            ShowInsertionMark(item, below);
            AutoScroll(e);
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }
        private void SectionItem_DragLeave(object sender, DragEventArgs e)
        {
            // Không xóa ở đây: chuyển giữa hai item sẽ gọi DragLeave rồi DragOver liên tiếp,
            // gây nhấp nháy. Đường chèn được dọn khi Drop hoặc khi DoDragDrop kết thúc.
        }
        private void SectionItem_Drop(object sender, DragEventArgs e)
        {
            var item = (ListBoxItem)sender;
            e.Handled = true;

            if (e.Data.GetDataPresent(RowsDragFormat))
            {
                MoveRows(e.Data.GetData(RowsDragFormat) as StringRowsDragData, item.DataContext as StringSectionCombine);
                return;
            }

            if (!e.Data.GetDataPresent(SectionDragFormat)) return;

            var section = e.Data.GetData(SectionDragFormat) as StringSectionCombine;
            if (section == null) return;

            int targetIndex = SectionListBox.ItemContainerGenerator.IndexFromContainer(item);
            bool below = e.GetPosition(item).Y > item.ActualHeight / 2;
            MoveSection(section, below ? targetIndex + 1 : targetIndex);
        }

        // Thả vào vùng trống của ListBox (dưới item cuối) = chuyển xuống cuối danh sách
        private void SectionListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(SectionDragFormat))
            {
                AutoScroll(e);
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void SectionListBox_Drop(object sender, DragEventArgs e)
        {
            var section = e.Data.GetData(SectionDragFormat) as StringSectionCombine;
            if (section == null) return;
            MoveSection(section, CombineData.Sections.Count);
        }

        // insertIndex = vị trí chèn tính theo danh sách TRƯỚC khi bỏ item ra
        private void MoveSection(StringSectionCombine section, int insertIndex)
        {
            var sections = CombineData.Sections;
            int oldIndex = sections.IndexOf(section);
            if (oldIndex < 0) return;

            int newIndex = insertIndex;
            if (oldIndex < newIndex) newIndex--;        // bỏ item ra thì các vị trí phía sau lùi 1
            if (newIndex == oldIndex) return;           // thả về chỗ cũ

            sections.Move(oldIndex, newIndex);
            SelectedSection = section;                  // giữ nhóm vừa kéo ở trạng thái đang chọn
            IsSaved = false;
            ClearInsertionMark();
        }
        private void ShowInsertionMark(ListBoxItem item, bool below)
        {
            if (_adornerItem == item && _adornerBelow == below) return;   // không đổi thì khỏi vẽ lại
            ClearInsertionMark();

            var layer = AdornerLayer.GetAdornerLayer(item);
            if (layer == null) return;

            _adorner = new InsertionAdorner(item, below);
            layer.Add(_adorner);
            _adornerItem = item;
            _adornerBelow = below;
        }

        private void ClearInsertionMark()
        {
            if (_adorner != null && _adornerItem != null)
            {
                var layer = AdornerLayer.GetAdornerLayer(_adornerItem);
                if (layer != null) layer.Remove(_adorner);
            }
            _adorner = null;
            _adornerItem = null;
        }

        // Tự cuộn khi kéo gần mép trên/dưới của ListBox
        private void AutoScroll(DragEventArgs e)
        {
            var sv = FindChild<ScrollViewer>(SectionListBox);
            if (sv == null) return;

            double y = e.GetPosition(SectionListBox).Y;
            const double margin = 24;
            if (y < margin) sv.ScrollToVerticalOffset(sv.VerticalOffset - 1);
            else if (y > SectionListBox.ActualHeight - margin) sv.ScrollToVerticalOffset(sv.VerticalOffset + 1);
        }
        private static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int n = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < n; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var found = child as T ?? FindChild<T>(child);
                if (found != null) return found;
            }
            return null;
        }

        private Point _rowDragStart;
        private DropTargetAdorner _rowTargetAdorner;
        private ListBoxItem _rowTargetItem;

        private void RowHandle_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _rowDragStart = e.GetPosition(null);

            // Bấm vào tay cầm của dòng đã nằm trong nhóm được chọn: giữ nguyên selection,
            // nếu không DataGrid sẽ thu về chỉ còn một dòng.
            var row = ((FrameworkElement)sender).DataContext as StringSectionContentCombine;
            if (row != null && RowsGrid.SelectedItems.Count > 1 && RowsGrid.SelectedItems.Contains(row))
                e.Handled = true;
        }
        private void RowHandle_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Vector delta = e.GetPosition(null) - _rowDragStart;
            if (Math.Abs(delta.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(delta.Y) < SystemParameters.MinimumVerticalDragDistance) return;

            var fe = (FrameworkElement)sender;
            var row = fe.DataContext as StringSectionContentCombine;
            if (row == null || SelectedSection == null) return;

            var items = RowsGrid.SelectedItems.Cast<StringSectionContentCombine>().ToList();
            if (!items.Contains(row)) items = new List<StringSectionContentCombine> { row };

            var data = new StringRowsDragData { From = SelectedSection, Items = items };

            try
            {
                DragDrop.DoDragDrop(fe, new DataObject(RowsDragFormat, data), DragDropEffects.Move);
            }
            finally
            {
                ClearRowTarget();
            }
        }
        private void ShowRowTarget(ListBoxItem item)
        {
            if (_rowTargetItem == item) return;
            ClearRowTarget();

            var layer = AdornerLayer.GetAdornerLayer(item);
            if (layer == null) return;

            _rowTargetAdorner = new DropTargetAdorner(item);
            layer.Add(_rowTargetAdorner);
            _rowTargetItem = item;
        }
        private void ClearRowTarget()
        {
            if (_rowTargetAdorner != null && _rowTargetItem != null)
            {
                var layer = AdornerLayer.GetAdornerLayer(_rowTargetItem);
                if (layer != null) layer.Remove(_rowTargetAdorner);
            }
            _rowTargetAdorner = null;
            _rowTargetItem = null;
        }
        private void MoveRows(StringRowsDragData data, StringSectionCombine target)
        {
            if (data == null || target == null || data.From == target) return;

            // Kết thúc việc sửa ô dở dang để không mất giá trị đang gõ
            RowsGrid.CommitEdit(DataGridEditingUnit.Row, true);

            // Giữ đúng thứ tự như trong nhóm cũ, không theo thứ tự chọn
            var set = new HashSet<StringSectionContentCombine>(data.Items);
            var moved = data.From.Content.Where(set.Contains).ToList();
            if (moved.Count == 0) return;

            foreach (var item in moved)
                data.From.Content.Remove(item);
            target.Content.AddRange(moved);          // thêm vào cuối nhóm đích

            IsSaved = false;
            ClearRowTarget();
        }
        #endregion

        #region Event 
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }
        private void tbtnFilepath_Checked(object sender, RoutedEventArgs e)
        {
            GrFilePath.Visibility = Visibility.Collapsed;
        }
        private void tbtnFilepath_Unchecked(object sender, RoutedEventArgs e)
        {
            GrFilePath.Visibility = Visibility.Visible;
        }
        private void Row_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StringSectionContentCombine.TranslationContent) ||
                e.PropertyName == nameof(StringSectionContentCombine.Status) ||
                e.PropertyName == nameof(StringSectionContentCombine.IsDisabled))
            {
                IsSaved = false;
                RefreshCounters();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            foreach (var s in CombineData.Sections)
                foreach (var r in s.Content)
                    r.PropertyChanged -= Row_PropertyChanged;

            SourceFileData = null;
            TranslationFileData = null;
            CombineData = new StringCombine();
            SelectedSection = null;
        }
        #endregion

    }
}
