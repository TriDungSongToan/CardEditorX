using System.Collections.Generic;
using CardEditor.Enums;

namespace CardEditor.Models
{
    public class CheckDatabaseResult
    {
        public bool Result { get; set; } = false;
        public DatabaseType DBType { get; set; } = DatabaseType.YGO;
        public bool HasFlag { get; set; } = false;
    }
    public class LoadAllCdbFilesResult
    {
        public bool Result { get; set; }
        public List<Card> CardList { get; set; } = new();
        public Dictionary<ulong, List<string>> CardPaths { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
    public class LoadCardDataResult
    {
        public bool Result { get; set; }
        public DatabaseType DBType { get; set; }
        public bool HasFlag { get; set; }

        public List<Card> CardList { get; set; } = new();
        public List<CardOmega> OmegaCardList { get; set; } = new();

        public string Message { get; set; } = string.Empty;
    }
    public class LoadCardBanDataResult
    {
        public bool Result { get; set; }
        public List<CardBanList> CardList { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
    public class LoadCardRareDataResult
    {
        public bool Result { get; set; }
        public List<RareCard> CardList { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
    public class LoadBanListResult
    {
        public bool Result { get; set; }
        public BanList BanList { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
    public class LoadScriptPathsResult
    {
        public bool Result { get; set; }
        public Dictionary<ulong, List<string>> Paths { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class ResultItem
    {
        public bool Succeeded { get; set; }
        public int FilteredCount { get; set; }
        public int TotalCount { get; set; }
        public string Message { get; set; }
    }

}
