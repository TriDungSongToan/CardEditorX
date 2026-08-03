using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using CardEditor.Models;
using CardEditor.Commands;
using CardEditor.Collections;

namespace CardEditor.Views
{
    /// <summary>
    /// Interaction logic for SelectFileWindow.xaml
    /// </summary>
    public partial class SelectFileWindow : Window, INotifyPropertyChanged
    {
        #region Property
        private readonly IReadOnlyList<(string fullPath, string archiveFilePath, string archiveEntryName)> _entries;
        public List<CardEditor.Models.FileItem> Result { get; private set; } = new();

        public BulkObservableCollection<CardEditor.Models.FileItem> FileList { get; set; } = new();
        private BulkObservableCollection<CardEditor.Models.FileItem> _selectedFiles = new();
        public BulkObservableCollection<CardEditor.Models.FileItem> SelectedFiles
        {
            get => _selectedFiles;
            set
            {
                if (!ReferenceEquals(_selectedFiles, value))
                {
                    if (_selectedFiles != null) _selectedFiles.CollectionChanged -= SelectedFiles_CollectionChanged;
                    _selectedFiles = value ?? new BulkObservableCollection<FileItem>();
                    _selectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
                    OnPropertyChanged(nameof(SelectedFiles));
                }
            }
        }
        #endregion

        #region Commands
        public RelayCommand OpenCommand { get; set; }
        public RelayCommand OpenAllCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }
        #endregion

        #region Constructor
        public SelectFileWindow(IReadOnlyList<string> filePaths)
            : this(filePaths?.Select(p => (p, (string)null, (string)null)).ToList()) { }
        public SelectFileWindow(IReadOnlyList<(string fullPath, string archiveFilePath, string archiveEntryName)> fileEntries)
        {
            InitializeComponent();
            this.DataContext = this;

            _entries = fileEntries ?? Array.Empty<(string, string, string)>();

            InitializeCommand();
            InitializeEvent();

            LoadData();
        }

        private void InitializeCommand()
        {
            OpenCommand = new CardEditor.Commands.RelayCommand(_ => OpenFileCommand(), _ => CanOpenFileCommand());
            OpenAllCommand = new CardEditor.Commands.RelayCommand(_ => OpenAllFileCommand(), _ => CanOpenAllFileCommand());
            CancelCommand = new CardEditor.Commands.RelayCommand(_ => CancelFileCommand());
        }
        private void InitializeEvent()
        {
            FileList.CollectionChanged += FileList_CollectionChanged;
            _selectedFiles.CollectionChanged += SelectedFiles_CollectionChanged;
        }
        #endregion

        #region Load
        private void LoadData()
        {
            var items = _entries
                .Select(e => new CardEditor.Models.FileItem(e.fullPath, e.archiveFilePath, e.archiveEntryName))
                .ToList();
            FileList.AddRange(items);
        }
        #endregion

        #region Command Functions
        private void OpenFileCommand()
        {
            Result.Clear();
            Result.AddRange(SelectedFiles);
            DialogResult = true;
            this.Close();
        }
        private bool CanOpenFileCommand()
        {
            return SelectedFiles != null && SelectedFiles.Count > 0;
        }
        private void OpenAllFileCommand()
        {
            Result.Clear();
            Result.AddRange(FileList);
            DialogResult = true;
            this.Close();
        }
        private bool CanOpenAllFileCommand()
        {
            return FileList!= null && FileList.Count > 0;
        }
        private void CancelFileCommand()
        {
            Result.Clear();
            DialogResult = false;
            this.Close();
        }
        #endregion

        #region Event
        private void FileList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OpenAllCommand?.RaiseCanExecuteChanged();
        }
        private void SelectedFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OpenCommand?.RaiseCanExecuteChanged();
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is CardEditor.Models.FileItem item)
            {
                // Double-click luôn ưu tiên item được click, bỏ qua SelectedFiles hiện tại
                // (kể cả khi user đang multi-select item khác)
                Result.Clear();
                Result.Add(item);
                DialogResult = true;
                this.Close();
            }
        }

        private void blHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}