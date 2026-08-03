using System.Diagnostics;
using ICSharpCode.AvalonEdit.Document;

namespace CardEditor.Editor.Hover
{
    public static class ExpressionExtractor
    {
        public static string Extract(TextDocument document, int offset)
        {
            Debug.WriteLine($"=== Extract, offset={offset} ===");

            if (document == null || offset < 0 || offset >= document.TextLength) return string.Empty;

            // ===== Tìm vị trí bắt đầu identifier =====
            int start = offset;

            // Nếu đang ở ký tự không phải identifier, lùi về phía trước
            while (start > 0 && !IsIdentifierChar(document.GetCharAt(start)))
            {
                start--;
            }

            // Kiểm tra xem có phải identifier không
            if (start < 0 || !IsIdentifierChar(document.GetCharAt(start)))
            {
                Debug.WriteLine("No identifier found");
                return string.Empty;
            }

            Debug.WriteLine($"Found identifier char at: {start} ('{document.GetCharAt(start)}')");

            // Quét ngược để tìm đầu (bao gồm cả dấu chấm)
            while (start > 0)
            {
                char c = document.GetCharAt(start - 1);
                if (IsIdentifierChar(c) || c == '.') start--;
                else break;
            }

            // Quét tiến để tìm cuối (bao gồm cả dấu chấm)
            int end = offset + 1;
            while (end < document.TextLength)
            {
                char c = document.GetCharAt(end);
                if (IsIdentifierChar(c) || c == '.') end++;
                else break;
            }

            string result = document.GetText(start, end - start).Trim('.');
            Debug.WriteLine($"Extracted: '{result}'");
            return result;
        }

        private static bool IsIdentifierChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }
    }
}
