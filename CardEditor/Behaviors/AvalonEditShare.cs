using System.Windows;
using ICSharpCode.AvalonEdit;

namespace CardEditor.Behaviors
{
    public static class AvalonEditBehavior
    {
        public static readonly DependencyProperty BindableTextProperty =
            DependencyProperty.RegisterAttached(
                "BindableText",
                typeof(string),
                typeof(AvalonEditBehavior),
                new FrameworkPropertyMetadata(
                    defaultValue: string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnBindableTextChanged)
            );

        public static string GetBindableText(DependencyObject obj)
        {
            return (string)obj.GetValue(BindableTextProperty);
        }

        public static void SetBindableText(DependencyObject obj, string value)
        {
            obj.SetValue(BindableTextProperty, value);
        }

        private static bool _isUpdating = false;

        private static void OnBindableTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_isUpdating) return;

            var textEditor = d as TextEditor;
            if (textEditor == null) return;

            var newText = e.NewValue as string ?? string.Empty;

            // Chỉ update khi text thực sự khác nhau
            if (textEditor.Text != newText)
            {
                _isUpdating = true;

                // Lưu vị trí con trỏ
                var caretOffset = textEditor.CaretOffset;
                var scrollOffset = textEditor.VerticalOffset;

                textEditor.Text = newText;

                // Khôi phục vị trí con trỏ (nếu còn hợp lệ)
                if (caretOffset <= textEditor.Text.Length)
                {
                    textEditor.CaretOffset = caretOffset;
                }
                textEditor.ScrollToVerticalOffset(scrollOffset);

                _isUpdating = false;
            }

            // Subscribe TextChanged event để đồng bộ ngược lại
            textEditor.TextChanged -= TextEditor_TextChanged;
            textEditor.TextChanged += TextEditor_TextChanged;
        }

        private static void TextEditor_TextChanged(object sender, System.EventArgs e)
        {
            if (_isUpdating) return;

            var textEditor = sender as TextEditor;
            if (textEditor == null) return;

            _isUpdating = true;
            SetBindableText(textEditor, textEditor.Text);
            _isUpdating = false;
        }
    }
}
