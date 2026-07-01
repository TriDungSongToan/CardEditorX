using System;
using System.Windows;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Commands;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Views
{
    /// <summary>
    /// Interaction logic for FilterCard.xaml
    /// </summary>
    public partial class FilterCard : Window, INotifyPropertyChanged
    {
        public MainWindow MainWindowReference { get; set; }

        #region Header title
        private int _tabHeaderIndex = -1;
        public int TabHeaderIndex
        {
            get => _tabHeaderIndex;
            set
            {
                if (_tabHeaderIndex != value)
                {
                    _tabHeaderIndex = value;
                    OnPropertyChanged();
                    OnTabIndexChanged();
                }
            }
        }
        private string _headerTitle = string.Empty;
        public string HeaderTitle
        {
            get => _headerTitle;
            set
            {
                if (_headerTitle != value)
                {
                    _headerTitle = value;
                    OnPropertyChanged();
                }
            }
        }
        private void OnTabIndexChanged()
        {
            switch (TabHeaderIndex)
            {
                case 0:
                    HeaderTitle = CMess.ByCDBFile.ToText();
                    break;
                case 1:
                    HeaderTitle = CMess.ByYDKFile.ToText();
                    break;
                case 2:
                    HeaderTitle = CMess.ByLanguage.ToText();
                    break;
                default:
                    HeaderTitle = string.Empty;
                    break;
            }
        }
        #endregion

        #region Filter By CDB File
        private string _filterCDBFilePath = string.Empty;
        public string FilterCDBFilePath
        {
            get => _filterCDBFilePath;
            set
            {
                if (_filterCDBFilePath != value)
                {
                    _filterCDBFilePath = value;
                    OnPropertyChanged();
                    Console.WriteLine($"Filter CDB File Path Changed: {FilterCDBFilePath}");
                }
            }
        }
        private bool _isDupliCDBCards = false;
        public bool IsDupliCDBCards
        {
            get => _isDupliCDBCards;
            set
            {
                if (_isDupliCDBCards != value)
                {
                    _isDupliCDBCards = value;
                    OnPropertyChanged();
                    Console.WriteLine($"IsDupliCDBCards Changed: {IsDupliCDBCards.ToString()}");
                }
            }
        }
        public RelayCommand BrowseCDBFilePathCommand { get; set; }
        public RelayCommand FilterCDBCommand { get; set; }
        #endregion

        #region Filter By YDK File
        private string _filterYDKFilePath = string.Empty;
        public string FilterYDKFilePath
        {
            get => _filterYDKFilePath;
            set
            {
                if (_filterYDKFilePath != value)
                {
                    _filterYDKFilePath = value;
                    OnPropertyChanged();
                    Console.WriteLine("Filter YDK File Path Changed: " + FilterYDKFilePath);
                }
            }
        }
        private bool _isDupliYDKCards = false;
        public bool IsDupliYDKCards
        {
            get => _isDupliYDKCards;
            set
            {
                if (_isDupliYDKCards != value)
                {
                    _isDupliYDKCards = value;
                    OnPropertyChanged();
                    Console.WriteLine($"IsDupliYDKCards Changed: {IsDupliYDKCards.ToString()}");
                }
            }
        }
        public RelayCommand BrowseYDKFilePathCommand { get; set; }
        public RelayCommand FilterYDKCommand { get; set; }
        #endregion

        #region Filter By Language
        public BulkObservableCollection<string> AvailableLanguages { get; set; } = new ();
        private int _selectedLanguageIndex = -1;
        public int SelectedLanguageIndex
        {
            get => _selectedLanguageIndex;
            set
            {
                if (_selectedLanguageIndex != value)
                {
                    _selectedLanguageIndex = value;
                    OnPropertyChanged();
                    Console.WriteLine($"SelectedLanguageIndex Changed: {SelectedLanguageIndex.ToString()}");
                }
            }
        }
        private bool _isInclude = true;
        public bool IsInclude
        {
            get => _isInclude;
            set
            {
                if (_isInclude != value)
                {
                    _isInclude = value;
                    OnPropertyChanged();
                    Console.WriteLine($"IsInclude Changed: {IsInclude.ToString()}");
                }
            }
        }
        public RelayCommand FilterLanguageCommand { get; set; }
        #endregion

        public RelayCommand CancelCommand { get; set; }

        #region Constructor
        public FilterCard()
        {
            InitializeCommands();

            List<string> languages = new List<string>
            {
                "en-US",
                "vi-VN",  
                "zh-CN",
                "ja-JP",
                "ko-KR",
            };
            AvailableLanguages.AddRange(languages);

            InitializeComponent();

            DataContext = this;
        }
        private void InitializeCommands()
        {
            BrowseCDBFilePathCommand = new RelayCommand(_ => BrowseCDBFilePathFunction());
            BrowseYDKFilePathCommand = new RelayCommand(_ => BrowseYDKFilePathFunction());

            FilterCDBCommand = new RelayCommand(async _ => await FilterByCDBFunction());
            FilterYDKCommand = new RelayCommand(async _ => await FilterByYDKFunction());
            FilterLanguageCommand = new RelayCommand(async _ => await FilterByLanguageFunction());

            CancelCommand = new RelayCommand(_ => this.Close());
        }
        #endregion

        #region Functions
        private void BrowseCDBFilePathFunction()
        {
            string filePath = FileDiaLogHelper.OpenCardList();
            if (string.IsNullOrWhiteSpace(filePath)) return;
            FilterCDBFilePath = filePath;
        }
        private void BrowseYDKFilePathFunction()
        {
            string filePath = FileDiaLogHelper.OpenDeck();
            if (string.IsNullOrWhiteSpace(filePath)) return;
            FilterYDKFilePath = filePath;
        }

        private async Task FilterByCDBFunction()
        {
            ResultItem result = await MainWindowReference.FilterCardByCDBFile(FilterCDBFilePath, IsDupliCDBCards);

            if (result.Succeeded)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.filterSuc.ToText(), result.FilteredCount, result.TotalCount),
                    new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlFilter.ToText(), CMess.Card.ToText())} {result.Message}",
                    new[] { CMess.ok.ToText() });
            }
        }
        private async Task FilterByYDKFunction()
        {
            ResultItem result = await MainWindowReference.FilterCardByYDKFile(FilterYDKFilePath, IsDupliYDKCards);

            if (result.Succeeded)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.filterSuc.ToText(), result.FilteredCount, result.TotalCount),
                    new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlFilter.ToText(), CMess.Card.ToText())} {result.Message}",
                    new[] { CMess.ok.ToText() });
            }
        }
        private async Task FilterByLanguageFunction()
        {
            ResultItem result = await MainWindowReference.FilterCardByLanguage(SelectedLanguageIndex, IsInclude);
            if (result.Succeeded)
            {
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    string.Format(CMess.filterSuc.ToText(), result.FilteredCount, result.TotalCount),
                    new[] { CMess.ok.ToText() });
            }
            else
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                   $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.tlFilter.ToText(), CMess.Card.ToText())} {result.Message}",
                   new[] { CMess.ok.ToText() });
            }
        }
        #endregion

        #region Events
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
