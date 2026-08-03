using System.Net;
using System.Text.RegularExpressions;

namespace CardEditor.Helpers
{
    public class NameReplaceHelper
    {
        private static string[] substringRemove = { " (VG)", " (DM)", " (Pre-Errata)", " (GOAT)" };
        private static string[] patterns = { "(Anime)", "(Manga)", "(Rush)", "(TF1)", "(TF2)", "(TF3)", "(TF4)", "(TF5)", "(TF6)", "(Rule)" };
        private static string[,] substringReplace =
        {
            { "(Anime)", "(Manga)", "(Rush)" },
            { "(anime)", "(manga)", "(Rush_Duel)" }
        };

        public static string ProcessName(string name, ulong password)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            if (name.Contains("(Deck Master)"))
            {
                return "Deck_Master";
            }
            // Loại bỏ các chuỗi cần xóa
            foreach (var pattern in substringRemove)
            {
                name = name.Replace(pattern, string.Empty);
            }

            //// Thay thế các cặp chuỗi
            //for (int i = 0; i < substringReplace.GetLength(1); i++)
            //{
            //    name = name.Replace(substringReplace[0, i], substringReplace[1, i]);
            //}
            // var regex = new Regex(@"(\(Anime\))|(\(Manga\))|(\(Rush\))|(\(TF1\))|(\(TF2\))|(\(TF3\))|(\(TF4\))|(\(TF5\))|(\(TF6\))|(\(Rule\))|(\(Skill\sCard\))");

            var regex = new Regex(@"\((Anime|Manga|Rush|TF[1-6]|Rule|Skill\sCard)\)", RegexOptions.None);

            name = regex.Replace(name, match =>
            {
                switch (match.Value)
                {
                    case "(Anime)": return "(anime)";
                    case "(Manga)": return "(manga)";
                    case "(Rush)": return "(Rush_Duel)";
                    case "(TF1)": return "(Tag_Force_1)";
                    case "(TF2)": return "(Tag_Force_2)";
                    case "(TF3)": return "(Tag_Force_3)";
                    case "(TF4)": return "(Tag_Force_4)";
                    case "(TF5)": return "(Tag_Force_5)";
                    case "(TF6)": return "(Tag_Force_6)";
                    case "(Rule)": return "(rule)";
                    default: return match.Value;
                }
            });

            string urlEncoded = WebUtility.UrlEncode(name).Replace("+", "_");

            if (password > 300000000 && password < 301000000 && !regex.IsMatch(name))
            {
                return urlEncoded + "_(Skill_Card)";
            }
            else
            {
                return urlEncoded;
            }
        }
    }
}
