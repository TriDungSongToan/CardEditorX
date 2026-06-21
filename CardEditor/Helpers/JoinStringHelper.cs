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
}
