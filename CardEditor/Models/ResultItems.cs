namespace CardEditor.Models
{
    public class ResultItem
    {
        public bool Succeeded { get; set; }
        public int FilteredCount { get; set; }
        public int TotalCount { get; set; }
        public string Message { get; set; }

    }
}
