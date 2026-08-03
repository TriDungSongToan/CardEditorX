using System.Windows;
using CardEditor.ViewModels;

namespace CardEditor
{
    /// <summary>
    /// Interaction logic for Furigana.xaml
    /// </summary>
    public partial class Furigana : Window
    {
        public string FuriganaText { get; private set; }

        public Furigana(string selectedText)
        {
            InitializeComponent();
            this.DataContext = UIConfigViewModel.Instance;
            FuriganaTextBox.Text = selectedText;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            FuriganaText = FuriganaTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
