using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using CardEditor.Generator;
using CardEditor.ViewModels;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for ScriptDescription.xaml
    /// </summary>
    public partial class ScriptDescription : UserControl
    {
        private HyperlinkElement _hyperlinkGenerator;
        public event EventHandler CloseRequested;

        public ScriptDescription()
        {
            InitializeComponent();
            iconCopy.Kind = MaterialDesignThemes.Wpf.PackIconKind.ContentCopy;

            ScriptDescriptionPane.IsReadOnly = true;
            ScriptDescriptionPane.Options.EnableHyperlinks = true;
            ScriptDescriptionPane.Options.EnableEmailHyperlinks = false;
            ScriptDescriptionPane.IsHitTestVisible = true;

            DataContextChanged += ScriptDescription_DataContextChanged;
            ScriptDescriptionPane.PreviewKeyDown += ScriptDescriptionPane_PreviewKeyDown;
            ConfigureHyperlinks(DataContext as ScriptDescriptionViewModel);
        }

        public ScriptDescriptionViewModel ViewModel
            => DataContext as ScriptDescriptionViewModel;

        private void ScriptDescription_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ConfigureHyperlinks(e.NewValue as ScriptDescriptionViewModel);
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            iconCopy.Kind = PackIconKind.ContentCopy;
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        private async void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            string text = ScriptDescriptionPane.Text;
            if (string.IsNullOrEmpty(text)) return;
            bool success = CopyText(text);
            if (!success) return;

            iconCopy.Kind = PackIconKind.Check;
            await Task.Delay(3000);
            iconCopy.Kind = PackIconKind.ContentCopy;
        }
        private bool CopyText(string text)
        {
            try
            {
                Clipboard.SetText(text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ScriptDescriptionPane_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
            {
                return;
            }

            CloseRequested?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }

        private void ConfigureHyperlinks(ScriptDescriptionViewModel viewModel)
        {
            if (_hyperlinkGenerator != null)
            {
                ScriptDescriptionPane.TextArea.TextView.ElementGenerators.Remove(_hyperlinkGenerator);
                _hyperlinkGenerator = null;
            }

            if (viewModel == null) return;

            _hyperlinkGenerator = new HyperlinkElement
            {
                OnLinkClicked = url => viewModel.LinkClickedCommand.Execute(url)
            };

            ScriptDescriptionPane.TextArea.TextView.ElementGenerators.Add(_hyperlinkGenerator);
        }



        //public void SetSymbol(CompletionSymbol symbol)
        //{
        //    if (symbol == null)
        //    {
        //        ScriptDescriptionPane.Text = string.Empty;
        //        return;
        //    }

        //    var content = DescriptionFormatter.Format(symbol);
        //    ScriptDescriptionPane.Text = content;

        //    // Resize theo nội dung
        //    AdjustSize();
        //}
        //public void SetText(string text)
        //{
        //    ScriptDescriptionPane.Text = text ?? string.Empty;
        //    AdjustSize();
        //}
        //private void AdjustSize()
        //{
        //    // Tự động điều chỉnh kích thước
        //    ScriptDescriptionPane.Measure(new Size(MaxWidth, MaxHeight));
        //    var desiredSize = ScriptDescriptionPane.DesiredSize;

        //    Width = Math.Min(desiredSize.Width + 20, MaxWidth);
        //    Height = Math.Min(desiredSize.Height + 20, MaxHeight);
        //}
    }
}
