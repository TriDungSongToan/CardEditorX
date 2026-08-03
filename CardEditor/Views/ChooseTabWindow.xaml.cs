using System.Windows;
using CardEditor.Models;
using CardEditor.Commands;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for ChooseTabWindow.xaml
    /// </summary>
    public partial class ChooseTabWindow : Window
    {
        public EditorType SelectedOption { get; set; }
        public RelayCommand ImageEditorCommand { get; set; }
        public RelayCommand DataEditorCommand { get; set; }
        public RelayCommand DeckEditorCommand { get; set; }
        public RelayCommand CodeEditorCommand { get; set; }
        public RelayCommand BanListEditorCommand { get; set; }

        public ChooseTabWindow()
        {
            InitializeComponent();
            InitializeCommand();
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.ShowInTaskbar = false;

            var mousePosition = System.Windows.Forms.Cursor.Position;

            this.Left = mousePosition.X;
            this.Top = mousePosition.Y;

            this.DataContext = this;
        }
        private void InitializeCommand()
        {
            ImageEditorCommand = new RelayCommand(_ => ImageEditor());
            DataEditorCommand = new RelayCommand(_ => DataEditor());
            DeckEditorCommand = new RelayCommand(_ => DeckEditor());
            CodeEditorCommand = new RelayCommand(_ => CodeEditor());
            BanListEditorCommand = new RelayCommand(_ => BanListEditor());
        }

        private void ImageEditor()
        {
            SelectedOption = EditorType.Image;
            this.DialogResult = true;
            this.Close();
        }
        private void DataEditor()
        {
            SelectedOption = EditorType.Data;
            this.DialogResult = true;
            this.Close();
        }
        private void DeckEditor()
        {
            SelectedOption = EditorType.Deck;
            this.DialogResult = true;
            this.Close();
        }
        private void CodeEditor()
        {
            SelectedOption = EditorType.Code;
            this.DialogResult = true;
            this.Close();
        }
        private void BanListEditor()
        {
            SelectedOption = EditorType.BanList;
            this.DialogResult = true;
            this.Close();
        }
    }
}
