using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace ScriptSupport.Legacy.Behaviors
{
    public static class NumericBehavior
    {
        private static readonly Regex _regex = new Regex("[^0-9]+"); // Ký tự không phải số

        public static readonly DependencyProperty IsNumericOnlyProperty =
            DependencyProperty.RegisterAttached(
                "IsNumericOnly",
                typeof(bool),
                typeof(NumericBehavior),
                new UIPropertyMetadata(false, OnIsNumericOnlyChanged));
        public static bool GetIsNumericOnly(DependencyObject obj)
            => (bool)obj.GetValue(IsNumericOnlyProperty);
        public static void SetIsNumericOnly(DependencyObject obj, bool value)
            => obj.SetValue(IsNumericOnlyProperty, value);

        private static void OnIsNumericOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.PreviewTextInput += OnPreviewTextInput;
                    DataObject.AddPastingHandler(textBox, OnPaste);
                    textBox.TextChanged += OnTextChanged;
                }
                else
                {
                    textBox.PreviewTextInput -= OnPreviewTextInput;
                    DataObject.RemovePastingHandler(textBox, OnPaste);
                    textBox.TextChanged -= OnTextChanged;
                }
            }
        }
        private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _regex.IsMatch(e.Text);
        }
        private static void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (_regex.IsMatch(text))
                    e.CancelCommand();
            }
            else e.CancelCommand();
        }
        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            int caret = tb.CaretIndex;
            string filtered = _regex.Replace(tb.Text, "");
            if (filtered != tb.Text)
            {
                tb.Text = filtered;
                tb.CaretIndex = Math.Min(caret, tb.Text.Length);
            }
        }
    }

    public static class NumericOnlyBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(NumericOnlyBehavior),
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
                textBox.PreviewTextInput += OnPreviewTextInput;
                DataObject.AddPastingHandler(textBox, OnPaste);
            }
        }

        private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private static void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.SourceDataObject.GetDataPresent(DataFormats.Text))
            {
                e.CancelCommand();
                return;
            }

            string text = e.SourceDataObject.GetData(DataFormats.Text) as string;

            string numeric = new string(text.Where(char.IsDigit).ToArray());

            if (sender is TextBox textBox)
            {
                textBox.SetCurrentValue(TextBox.TextProperty, numeric);
                textBox.CaretIndex = numeric.Length;
            }

            e.CancelCommand();
        }
    }
}
