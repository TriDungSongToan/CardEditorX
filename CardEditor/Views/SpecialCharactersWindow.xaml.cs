using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using CardEditor.Models;
using CardEditor.Commands;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.Collections;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for SpecialCharactersWindow.xaml
    /// </summary>
    public partial class SpecialCharactersWindow : Window, INotifyPropertyChanged, IDisposable
    {
        private readonly FrameworkElement _targetControl;

        #region Properties
        public BulkObservableCollection<CharacterItem> CharacterItems { get; set; } = new();
        private CharacterItem _selectedCharItem;
        public CharacterItem SelectedCharItem
        {
            get => _selectedCharItem;
            set
            {
                if (_selectedCharItem != value)
                {
                    _selectedCharItem = value;
                    OnPropertyChanged();
                    OnSelectedCharItemChanged();
                }
            }
        }
        private void OnSelectedCharItemChanged()
        {
            Character = SelectedCharItem?.Character ?? string.Empty;
        }

        private string _character = string.Empty;
        public string Character
        {
            get => _character;
            set
            {
                if (_character != value)
                {
                    _character = value;
                    OnPropertyChanged();
                    InsertCommand?.RaiseCanExecuteChanged();
                    CopyCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        private string _desc = string.Empty;
        public string Desc
        {
            get => _desc;
            set
            {
                if (_desc != value)
                {
                    _desc = value;
                    OnPropertyChanged();
                    SearchCommand();
                }
            }
        }
        public IEnumerable<CharacterGroup> CharacterGroups { get; }
        private CharacterGroup? _selectedGroup;
        public CharacterGroup? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (_selectedGroup != value)
                {
                    _selectedGroup = value;
                    OnPropertyChanged();
                    SearchCommand();
                }
            }
        }
        private string _subCategory = string.Empty;
        public string SubCategory
        {
            get => _subCategory;
            set
            {
                if (_subCategory != value)
                {
                    _subCategory = value;
                    OnPropertyChanged();
                    SearchCommand();
                }
            }
        }
        public BulkObservableCollection<TagItem> AvailableTags { get; set; } = new();
        public BulkObservableCollection<TagItem> SelectedAvailableTags { get; set; } = new();
        private Visibility _tagLabelVisibility = Visibility.Visible;
        public Visibility TagLabelVisibility
        {
            get => _tagLabelVisibility;
            set
            {
                if(_tagLabelVisibility != value)
                {
                    _tagLabelVisibility = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Commands
        public RelayCommand InsertCommand { get; set; }
        public RelayCommand CopyCommand { get; set; }
        public RelayCommand ClearFilterCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }
        #endregion

        #region Constructor
        public SpecialCharactersWindow(FrameworkElement targetControl)
        {
            InitializeComponent();
            InitializeCommand();
            InitializeEvent();
            _targetControl = targetControl;
            if (_targetControl == null)
            {
                this.Close();
                return;
            }
            CharacterItems.ReplaceAll(SpecialCharViewModel.Instance.CharItems);
            CharacterGroups = Enum.GetValues(typeof(CharacterGroup)).Cast<CharacterGroup>().ToList();
            SelectedGroup = CharacterGroup.All;

            AvailableTags.AddRange(SpecialCharViewModel.Instance.TagItems);

            this.DataContext = this;
            Debug.WriteLine($"SpecialCharactersWindow opened for control: {_targetControl.GetType().Name} (Name: {(_targetControl as Control)?.Name})");
        }
        private void InitializeCommand()
        {
            InsertCommand = new RelayCommand(_ => InsertCharacter(), _ => SelectedCharacterNotNull());
            CopyCommand = new RelayCommand(_ => CopyCharacter(), _ => SelectedCharacterNotNull());
            ClearFilterCommand = new RelayCommand(_ => ClearFilter());
            CloseCommand = new RelayCommand(_ => this.Close());
        }
        private void InitializeEvent()
        {
            SelectedAvailableTags.CollectionChanged += SelectedAvailableTags_CollectionChanged;
        }
        #endregion

        #region Load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeContentMenu();
        }
        private void InitializeContentMenu()
        {
            ControlContextMenuService.Attach(txtSubCategory);
            ControlContextMenuService.Attach(txtDesc);
            ControlContextMenuService.Attach(txtCharacter);
        }
        #endregion

        #region Command Methods
        private void InsertCharacter()
        {
            if (!string.IsNullOrEmpty(Character))
            {
                InsertCharacter(Character);
                this.Close();
            }
        }
        private void InsertCharacter(string character)
        {
            Debug.WriteLine($"Inserting '{character}' into {_targetControl.GetType().Name} (Name: {(_targetControl as Control)?.Name})");

            switch (_targetControl)
            {
                case RichTextBox richTextBox:
                    richTextBox.Document.InsertText(character, richTextBox.CaretPosition);
                    break;
                case TextBox textBox:
                    int caretIndex = textBox.CaretIndex;
                    textBox.Text = textBox.Text.Insert(caretIndex, character);
                    textBox.CaretIndex = caretIndex + character.Length;
                    break;
                case ICSharpCode.AvalonEdit.TextEditor textEditor:
                    textEditor.Document.Insert(textEditor.CaretOffset, character);
                    break;
                default:
                    Debug.WriteLine($"Unsupported control type: {_targetControl?.GetType().Name}");
                    break;
            }
        }

        private void CopyCharacter()
        {
            Clipboard.SetText(Character);
        }
        private bool SelectedCharacterNotNull()
        {
            return !string.IsNullOrEmpty(Character);
        }
        private void ClearFilter()
        {
            SelectedCharItem = null;
            Desc = string.Empty;
            SelectedGroup = CharacterGroup.All;
            SubCategory = string.Empty;
            SelectedAvailableTags.Clear();
        }

        #endregion

        #region Filter
        private CancellationTokenSource _searchCts;
        private async void SearchCommand()
        {
            // Hủy lần tìm kiếm trước
            _searchCts?.Cancel();
            _searchCts?.Dispose();

            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(200, token);
                await Search(token);
            }
            catch (OperationCanceledException) { }
        }
        public async Task Search(CancellationToken token)
        {
            var filter = new CharacterFilter
            {
                SearchText = Desc,
                Group = SelectedGroup == null ? CharacterGroup.All : SelectedGroup,
                SubCategory = SubCategory,
                Tags = SelectedAvailableTags?.Select(t => t.Name).ToList() ?? new List<string>()
            };

            var result = await Task.Run(() =>
            {
                return SpecialCharViewModel.Instance.Filter(filter);
            }, token);

            App.Current.Dispatcher.Invoke(() =>
            {
                CharacterItems.Clear();
                CharacterItems.AddRange(result);
            });
        }
        #endregion

        #region Event
        private void blHeader_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void SelectedAvailableTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TagLabelVisibility = SelectedAvailableTags.Count() == 0 ? Visibility.Visible : Visibility.Collapsed;
            SearchCommand();
        }
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CardEditor.Models.CharacterItem item && item != null)
            {
                InsertCharacter(item.Character);
                this.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        #endregion

        #region Dispose
        public void Dispose()
        {
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            _searchCts = null;
            CharacterItems.Clear();
            SelectedAvailableTags.CollectionChanged -= SelectedAvailableTags_CollectionChanged;
        }
        #endregion

    }

    public static class RichTextBoxExtensions
    {
        public static void InsertText(this FlowDocument document, string text, TextPointer position)
        {
            position.InsertTextInRun(text);
        }
    }

}
