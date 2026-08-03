using System;
using System.Text.RegularExpressions;

namespace CardEditor.ImageGene
{
    public static class CardTextExtractor
    {
        static readonly Regex LabelRegex = new Regex(@"(\[[^\[\]]+\]|【[^【】]+】)", RegexOptions.Multiline);
        static readonly Regex DividerRegex = new Regex(@"^[-━]{4,}\s*$", RegexOptions.Multiline);
        public static (string Pendulum, string Monster) ExtractEffects(string desc)
        {
            if (string.IsNullOrWhiteSpace(desc)) return (null, null);
            var matches = LabelRegex.Matches(desc);

            string Clean(string input)
            {
                if (string.IsNullOrWhiteSpace(input)) return "";
                // Xóa các dòng gạch ngang
                string cleaned = DividerRegex.Replace(input, "").Trim();
                // Kiểm tra "n/a"
                return cleaned.Equals("n/a", StringComparison.OrdinalIgnoreCase) ? "" : cleaned;
            }

            if (matches.Count >= 2)
            {
                int firstLabelEnd = matches[0].Index + matches[0].Length;
                int secondLabelStart = matches[1].Index;
                int secondLabelEnd = matches[1].Index + matches[1].Length;

                string pendulum = desc.Substring(firstLabelEnd, secondLabelStart - firstLabelEnd);
                string monster = desc.Substring(secondLabelEnd);

                return (Clean(pendulum), Clean(monster));
            }
            else if (matches.Count == 1)
            {
                int labelEnd = matches[0].Index + matches[0].Length;
                string monster = desc.Substring(labelEnd);

                return ("", Clean(monster));
            }
            else
            {
                return ("", Clean(desc));
            }
        }
    }
}
