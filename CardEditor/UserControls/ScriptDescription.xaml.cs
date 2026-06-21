using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CardEditor.Editor.Completion;
using CardEditor.Models;
using CardEditor.ViewModels;

namespace CardEditor.UserControls
{
    /// <summary>
    /// Interaction logic for ScriptDescription.xaml
    /// </summary>
    public partial class ScriptDescription : UserControl
    {
        public ScriptDescription()
        {
            InitializeComponent();

            ScriptDescriptionPane.IsReadOnly = true;
            ScriptDescriptionPane.Options.EnableHyperlinks = true;
            ScriptDescriptionPane.Options.EnableEmailHyperlinks = false;
            ScriptDescriptionPane.IsHitTestVisible = true;
        }


        public ScriptDescriptionViewModel ViewModel
            => DataContext as ScriptDescriptionViewModel;

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
