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
using System.Threading.Tasks;
using System.Collections.Generic;
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
