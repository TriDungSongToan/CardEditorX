using System;
using System.Linq;
using System.Text;

namespace CardEditor.Helpers
{
    public class JoinStringHelper
    {
        public static string JoinWithSeparator(string separator, params string[] parts)
        {
            return string.Join(separator, parts.Where(p => !string.IsNullOrEmpty(p)));
        }
        public static string SanitizationString(string input)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var sanitized = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                if (!invalidChars.Contains(c))
                {
                    sanitized.Append(c);
                }
            }
            return sanitized.ToString();
        }
    }
    public class TrimStringHelper
    {
        public static string ShortenTitle(string text, int maxLength = 30)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }
    }
    public class SanitizeStringHelper
    {
        public static string SanitizeWindowsFileName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Xóa các ký tự Windows không cho phép trong file name
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

            string result = new string(
                input.Where(c => !invalidChars.Contains(c)).ToArray()
            );

            // Windows không cho phép tên file kết thúc bằng '.' hoặc ' '
            result = result.TrimEnd('.', ' ');

            return result;
        }
    }
}
