using System.Collections.Generic;
using CardEditor.Enums;

namespace CardEditor.Models
{
    public class CreateFileResult
    {
        public bool Result { get; set; } = false;
        public string FilePath { get; set; } = string.Empty;
        public string Messenger { get; set; } = string.Empty;
    }
    public class CreateCardListResult
    {
        public bool Result { get; set; } = false;
        public string Messenger { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public CardListFormat Format { get; set; } = CardListFormat.YGONoFlag;
        public bool HasFlag { get; set; } = false;
    }
    public class CheckCardListResult
    {
        public bool Result { get; set; } = false;
        public CardListFormat Format { get; set; } = CardListFormat.YGONoFlag;
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
        public CardListFormat Format { get; set; }

        public List<Card> CardList { get; set; } = new();
        public List<CardOmega> OmegaCardList { get; set; } = new();
        public List<CardBanList> CardBanlistList { get; set; } = new();

        public bool HasFlag { get; set; }

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
    public class LoadJSONCedsResult
    {
        public bool Result { get; set; }
        public JSONCeds Cards { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }
    public class JSONCeds
    {
        public List<Card> CardList { get; set; } = new();
        public List<CardOmega> CardOmegaList { get; set; } = new();
        public List<CardBanList> CardBanlistList { get; set; } = new();
        public CardListFormat Format { get; set; }
    }
    public class SetClipboardResult
    {
        public bool Result { get; set; }
        public string Messenger { get; set; }
    }
    public class LoadClipboardResult
    {
        public bool Result { get; set; }
        public JSONCeds Cards { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class WriteResult
    {
        public bool Result { get; set; }
        public string Messenger { get; set; }
    }
    public class ResultItem
    {
        public bool Succeeded { get; set; }
        public int FilteredCount { get; set; }
        public int TotalCount { get; set; }
        public string Message { get; set; }
    }

}
