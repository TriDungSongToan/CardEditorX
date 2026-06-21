using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CardEditor.Behaviors
{
    public static class PasteSanitizerBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(PasteSanitizerBehavior),
                new PropertyMetadata(false, OnEnableChanged));
        public static void SetEnable(DependencyObject element, bool value)
            => element.SetValue(EnableProperty, value);

        public static bool GetEnable(DependencyObject element)
            => (bool)element.GetValue(EnableProperty);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox)
                return;

            if ((bool)e.NewValue)
            {
                textBox.CommandBindings.Add(CreatePasteBinding(textBox));
            }
        }

        private static CommandBinding CreatePasteBinding(TextBox textBox)
        {
            return new CommandBinding(
                ApplicationCommands.Paste,
                (s, e) => OnPaste(textBox, e),
                (s, e) => e.CanExecute = Clipboard.ContainsText());
        }

        private static void OnPaste(TextBox textBox, ExecutedRoutedEventArgs e)
        {
            if (!Clipboard.ContainsText())
                return;

            string text = Clipboard.GetText();

            // ✂ Strip quotes nếu có
            if (text.Length >= 2 &&
                text.StartsWith("\"") &&
                text.EndsWith("\""))
            {
                text = text.Substring(1, text.Length - 2);
            }

            // Insert tại caret, không phá binding
            textBox.SetCurrentValue(TextBox.TextProperty, text);
            textBox.CaretIndex = text.Length;

            e.Handled = true; // chặn Paste mặc định
        }
    }
}
