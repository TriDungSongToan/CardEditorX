using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CardEditor.Collections;
using CardEditor.Commands;
using CardEditor.Models;

namespace CardEditor.Views
{
    /// <summary>
    /// Interaction logic for SelectFileWindow.xaml
    /// </summary>
    public partial class SelectFileWindow : Window, INotifyPropertyChanged
    {
        private readonly string _rootFolder1;
        private readonly string _rootFolder2;
        private readonly IReadOnlyList<string> _absolutePaths;
        public string ResultPath { get; private set; }

        private BulkObservableCollection<CardEditor.Models.FileItem> _fileLists;
        public BulkObservableCollection<CardEditor.Models.FileItem> FileLists
        {
            get => _fileLists;
            set
            {
               if (!ReferenceEquals(_fileLists, value))
                {
                    _fileLists = value ?? new BulkObservableCollection<Models.FileItem>();
                    OnPropertyChanged(nameof(FileLists));
                }
            }
        }
        private CardEditor.Models.FileItem _selectedFile;
        public CardEditor.Models.FileItem SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (!ReferenceEquals(_selectedFile, value))
                {
                    _selectedFile = value;
                    OnPropertyChanged(nameof(SelectedFile));
                    SelectCommand?.RaiseCanExecuteChanged();
                }
            }
        }
        public RelayCommand SelectCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        public SelectFileWindow(IReadOnlyList<string> filePaths, string RootFolder1, string RootFolder2 = null)
        {
            _absolutePaths = filePaths;
            _rootFolder1 = RootFolder1;
            _rootFolder2 = RootFolder2;

            FileLists = new BulkObservableCollection<Models.FileItem>();
            SelectCommand = new CardEditor.Commands.RelayCommand(_ => SelectFileCommand(), _ => SelectedFile != null);
            CancelCommand = new CardEditor.Commands.RelayCommand(_ => CancelFileCommand());

            InitializeComponent();
            this.DataContext = this;

            LoadData();
        }

        private void LoadData()
        {
            List<CardEditor.Models.FileItem> items = new List<Models.FileItem>()
            {
                new CardEditor.Models.FileItem(_rootFolder1, string.Empty, _rootFolder2)
            };

            foreach (var path in _absolutePaths)
            {
                items.Add(new CardEditor.Models.FileItem(_rootFolder1, path, _rootFolder2));
            }
            FileLists.AddRange(items);
        }

        private void SelectFileCommand()
        {
            if (SelectedFile == null) return;
            ResultPath = SelectedFile.FullPath;
            DialogResult = true;
            this.Close();
        }
        private void CancelFileCommand()
        {
            ResultPath = string.Empty;
            DialogResult = false;
            this.Close();
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
    }
}
