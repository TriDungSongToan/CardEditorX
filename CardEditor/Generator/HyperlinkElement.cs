using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using ICSharpCode.AvalonEdit.Rendering;
using CardEditor.ViewModels;

namespace CardEditor.Generator
{
    public sealed class HyperlinkElement : VisualLineElementGenerator
    {
        private static readonly Regex LinkRegex =
            new Regex(@"\[([^\]]+)\]\(([^)]+)\)", RegexOptions.Compiled);

        public Action<string> OnLinkClicked { get; set; }
        public override int GetFirstInterestedOffset(int startOffset)
        {
            var match = FindMatch(startOffset);
            return match.Success
                ? CurrentContext.VisualLine.LastDocumentLine.Offset + match.Index
                : -1;
        }

        public override VisualLineElement ConstructElement(int offset)
        {
            var match = FindMatch(offset);
            if (!match.Success) return null;

            int lineStart = CurrentContext.VisualLine.LastDocumentLine.Offset;
            if (lineStart + match.Index != offset) return null;

            var hyperlink = new Hyperlink(new Run(match.Groups[1].Value))
            {
                NavigateUri = new Uri(match.Groups[2].Value, UriKind.RelativeOrAbsolute),
                Foreground = Brushes.DodgerBlue,
                TextDecorations = TextDecorations.Underline
            };

            hyperlink.MouseEnter += (_, __) => hyperlink.Foreground = UIConfigViewModel.Instance.ThemeColor;
            hyperlink.MouseLeave += (_, __) => hyperlink.Foreground = Brushes.DodgerBlue;

            hyperlink.RequestNavigate += OnRequestNavigate;

            return new InlineObjectElement(match.Length, new TextBlock(hyperlink)
            {
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        private Match FindMatch(int startOffset)
        {
            var line = CurrentContext.VisualLine.LastDocumentLine;
            string text = CurrentContext.Document.GetText(line.Offset, line.Length);

            return LinkRegex.Match(text, Math.Max(0, startOffset - line.Offset));
        }

        private void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            OnLinkClicked?.Invoke(e.Uri.OriginalString);
            e.Handled = true;
        }
    }
}
