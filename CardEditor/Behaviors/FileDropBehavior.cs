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
    public static class FileDropBehavior
    {
        public static readonly DependencyProperty EnableProperty =
            DependencyProperty.RegisterAttached(
                "Enable",
                typeof(bool),
                typeof(FileDropBehavior),
                new PropertyMetadata(false, OnEnableChanged));

        public static void SetEnable(DependencyObject element, bool value)
            => element.SetValue(EnableProperty, value);

        public static bool GetEnable(DependencyObject element)
            => (bool)element.GetValue(EnableProperty);

        public static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.RegisterAttached(
                "DropCommand",
                typeof(ICommand),
                typeof(FileDropBehavior));

        public static void SetDropCommand(DependencyObject element, ICommand value)
            => element.SetValue(DropCommandProperty, value);

        public static ICommand GetDropCommand(DependencyObject element)
            => (ICommand)element.GetValue(DropCommandProperty);

        private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.AllowDrop = true;
                    textBox.PreviewDragOver += OnDragOver;
                    textBox.Drop += OnDrop;
                }
                else
                {
                    textBox.PreviewDragOver -= OnDragOver;
                    textBox.Drop -= OnDrop;
                }
            }
        }

        private static void OnDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Có ít nhất 1 file hoặc folder tồn tại
            e.Effects = paths.Length > 0
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private static void OnDrop(object sender, DragEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (paths.Length == 0)
                return;

            string path = paths[0]; // file hoặc folder đầu tiên

            // 1️⃣ Ưu tiên Command (MVVM)
            var command = GetDropCommand(textBox);
            if (command != null && command.CanExecute(path))
            {
                command.Execute(path);
                return;
            }

            // 2️⃣ Fallback: set trực tiếp (code-behind / simple MVVM)
            textBox.SetCurrentValue(TextBox.TextProperty, path);
        }
    }
}
