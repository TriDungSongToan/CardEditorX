using System;
using System.Collections.Generic;

namespace CardEditor.Models
{
    public class YamiYugiRegulation
    {
        public DateTime Date { get; set; }
        public List<CardLimit> Regulation { get; set; } = new();
    }
    public class CardLimit
    {
        public int KonamiID { get; set; }
        public ulong CardID { get; set; } = 0;
        public string CardName { get; set; } = string.Empty;
        public int LimitedCount { get; set; }
    }
    public class RegulationRawDto
    {
        public DateTime Date { get; set; }
        public Dictionary<string, int> Regulation { get; set; }
    }
}
