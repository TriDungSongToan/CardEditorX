namespace CardEditor.Editor.Completion
{
    internal readonly struct CompletionContext
    {
        public string Qualifier { get; }
        public string Prefix { get; }
        public bool IsDotCompletion => Qualifier != null;

        public CompletionContext(string qualifier, string prefix)
        {
            Qualifier = qualifier;
            Prefix = prefix;
        }
    }
}
