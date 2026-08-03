namespace CardEditor.Models
{
    public class SearchOptions
    {
        public bool MatchCase { get; set; }
        public bool UseWildcards { get; set; }
        public bool WholeWord { get; set; }
        public bool MatchPrefix { get; set; }
        public bool MatchSuffix { get; set; }

        public bool IgnoreWhitespace { get; set; }
        public bool IgnorePunctuation { get; set; }
    }
}
