using System.Text.RegularExpressions;
using System.Collections.Generic;
using CardEditor.Models;

namespace CardEditor.Helpers
{
    public static class SearchPattern
    {
        public static string Build(string input, CardEditor.Models.SearchOptions opt)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // escape base
            string pattern = Regex.Escape(input);

            // wildcard support
            if (opt.UseWildcards)
            {
                pattern = pattern
                    .Replace(@"\*", ".*")
                    .Replace(@"\?", ".");
            }

            // whitespace-insensitive (VS Code style)
            if (opt.IgnoreWhitespace)
            {
                pattern = Regex.Replace(pattern, @"\s+", @"\\s+");
            }

            // punctuation-insensitive (loose match)
            if (opt.IgnorePunctuation)
            {
                pattern = Regex.Replace(pattern, @"\\W+", ".*?");
            }

            // whole word
            if (opt.WholeWord)
                pattern = $@"\b{pattern}\b";

            // prefix
            if (opt.MatchPrefix)
                pattern = "^" + pattern;

            // suffix
            if (opt.MatchSuffix)
                pattern += "$";

            return pattern;
        }

        public static int Replace(List<Card> cards, string findWhat, string replaceWith, SearchOptions opt, RegexOptions regexOpt)
        {
            string pattern = SearchPattern.Build(findWhat, opt);

            var regex = new Regex(pattern, regexOpt);

            int modified = 0;

            foreach (var card in cards)
            {
                if (string.IsNullOrEmpty(card.desc))
                    continue;

                string newText = regex.Replace(card.desc, replaceWith);

                if (newText != card.desc)
                {
                    card.desc = newText;
                    modified++;
                }
            }

            return modified;
        }
    }
}
