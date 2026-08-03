using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Documents;
using ICSharpCode.AvalonEdit;
using CardEditor.Manager;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services
{
    public static class ControlContextMenuService
    {
        public static void Attach(FrameworkElement control)
        {
            control.ContextMenu = CreateContextMenu();
        }

        private static ContextMenu CreateContextMenu()
        {
            var menu = new ContextMenu();
            ApplyStyle(menu);


            menu.Items.Add(CreateMenuItem("Cut", CMess.Cut.ToText(), ApplicationCommands.Cut));
            menu.Items.Add(CreateMenuItem("Copy", CMess.Copy.ToText(), ApplicationCommands.Copy));
            menu.Items.Add(CreateMenuItem("Paste", CMess.Paste.ToText(), ApplicationCommands.Paste));

            menu.Items.Add(CreateConvertMenu("FullWidth", CMess.ToFull.ToText(), true));
            menu.Items.Add(CreateConvertMenu("HalfWidth", CMess.ToHalf.ToText(), false));
            menu.Items.Add(CreateSuperMenu("Super", CMess.ToSuper.ToText(), true));
            menu.Items.Add(CreateSuperMenu("FromSuper", CMess.FromSuper.ToText(), false));
            menu.Items.Add(CreateSubMenu("Sub", CMess.ToSub.ToText(), true));
            menu.Items.Add(CreateSubMenu("FromSub", CMess.FromSub.ToText(), false));

            var special = CreateMenuItem("SpecialChar", CMess.SpecialChar.ToText());

            special.Click += (s, e) =>
            {
                if (menu.PlacementTarget is FrameworkElement target)
                {
                    var window = new SpecialCharactersWindow(target);
                    window.ShowDialog();
                }
            };

            menu.Items.Add(special);
            return menu;
        }

        private static MenuItem CreateMenuItem(string tag, string header, System.Windows.Input.ICommand command = null)
        {
            return new MenuItem
            {
                Tag = tag,
                Header = header,
                Command = command,
                Height = 25
            };
        }
        private static MenuItem CreateConvertMenu(string tag, string header, bool full)
        {
            var item = CreateMenuItem(tag, header);
            item.Click += (s, e) =>
            {
                if (item.Parent is ContextMenu menu &&
                   menu.PlacementTarget is FrameworkElement target)
                {
                    ConvertWidth(target, full);
                }
            };
            return item;
        }
        private static MenuItem CreateSuperMenu(string tag, string header, bool super)
        {
            var item = CreateMenuItem(tag, header);
            item.Click += (s, e) =>
            {
                if (item.Parent is ContextMenu menu && menu.PlacementTarget is FrameworkElement target)
                {
                    ConvertSuper(target, super);
                }
            };
            return item;
        }
        private static MenuItem CreateSubMenu(string tag, string header, bool sub)
        {
            var item = CreateMenuItem(tag, header);
            item.Click += (s, e) =>
            {
                if (item.Parent is ContextMenu menu &&
                   menu.PlacementTarget is FrameworkElement target)
                {
                    ConvertSub(target, sub);
                }
            };
            return item;
        }
        public static void Refresh(ContextMenu menu)
        {
            if (menu == null)  return;

            ApplyStyle(menu);
            foreach (MenuItem item in menu.Items)
            {
                ApplyStyle(item);

                UpdateHeader(item);
            }
        }
        private static void ApplyStyle(Control control)
        {
            control.Background = UIConfigViewModel.Instance.Background;
            control.Foreground = UIConfigViewModel.Instance.Foreground;
            control.FontFamily = UIConfigViewModel.Instance.FontFamily;
            control.FontSize = UIConfigViewModel.Instance.FontSize;
        }
        private static void UpdateHeader(MenuItem item)
        {
            switch (item.Tag?.ToString())
            {
                case "Cut":
                    item.Header = CMess.Cut.ToText();
                    break;

                case "Copy":
                    item.Header = CMess.Copy.ToText();
                    break;

                case "Paste":
                    item.Header = CMess.Paste.ToText();
                    break;

                case "FullWidth":
                    item.Header = CMess.ToFull.ToText();
                    break;

                case "HalfWidth":
                    item.Header = CMess.ToHalf.ToText();
                    break;

                case "Super":
                    item.Header = CMess.ToSuper.ToText();
                    break;

                case "FromSuper":
                    item.Header = CMess.FromSuper.ToText();
                    break;

                case "Sub":
                    item.Header = CMess.ToSub.ToText();
                    break;

                case "FromSub":
                    item.Header = CMess.FromSub.ToText();
                    break;

                case "SpecialChar":
                    item.Header = CMess.SpecialChar.ToText();
                    break;
            }
        }

        private static void ConvertWidth(FrameworkElement control, bool full)
        {
            switch (control)
            {
                case RichTextBox box:

                    var range = new TextRange(box.Selection.Start, box.Selection.End);
                    if (!string.IsNullOrEmpty(range.Text))
                    {
                        range.Text = full
                            ? ConvertString.ConvertToFullWidth(range.Text)
                            : ConvertString.ConvertToHalfWidth(range.Text);
                    }
                    break;
                case TextBox box:

                    ReplaceTextBox(box, text => full
                        ? ConvertString.ConvertToFullWidth(text)
                        : ConvertString.ConvertToHalfWidth(text));
                    break;
                case TextEditor editor:

                    if (!string.IsNullOrEmpty(editor.SelectedText))
                    {
                        var text = full
                        ? ConvertString.ConvertToFullWidth(editor.SelectedText)
                        : ConvertString.ConvertToHalfWidth(editor.SelectedText);
                        editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, text);
                    }
                    break;
            }
        }
        private static void ConvertSuper(FrameworkElement control, bool super)
        {
            ReplaceSelectedText(control, text => super
                ? ConvertString.ConvertToSuperscript(text)
                : ConvertString.ConvertFromSuperscript(text));
        }
        private static void ConvertSub(FrameworkElement control, bool sub)
        {
            ReplaceSelectedText(control, text => sub
                ? ConvertString.ConvertToSubscript(text)
                : ConvertString.ConvertFromSubscript(text));
        }
        private static void ReplaceSelectedText(FrameworkElement control, Func<string, string> converter)
        {
            switch (control)
            {
                case TextBox box:
                    ReplaceTextBox(box, converter);
                    break;
                case RichTextBox box:
                    var range = new TextRange(box.Selection.Start, box.Selection.End);
                    if (!string.IsNullOrEmpty(range.Text)) range.Text = converter(range.Text);
                    break;
                case TextEditor editor:
                    if (!string.IsNullOrEmpty(editor.SelectedText))
                    {
                        editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, converter(editor.SelectedText));
                    }
                    break;
            }
        }
        private static void ReplaceTextBox(TextBox box, Func<string, string> converter)
        {
            if (string.IsNullOrEmpty(box.SelectedText)) return;
            int start = box.SelectionStart;
            string result = converter(box.SelectedText);

            box.Text = box.Text.Remove(start, box.SelectionLength).Insert(start, result);
            box.SelectionStart = start;
            box.SelectionLength = result.Length;
        }

    }
}