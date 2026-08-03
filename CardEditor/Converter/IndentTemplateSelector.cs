using System.Windows;
using System.Windows.Controls;

namespace CardEditor.Converter
{
    public class IndentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate SelectedTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element)
            {
                // Nếu đang hiển thị trong ComboBoxItem (dropdown)
                if (container is ComboBoxItem)
                    return ItemTemplate;

                // Nếu đang hiển thị trong phần SelectedItem
                return SelectedTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
