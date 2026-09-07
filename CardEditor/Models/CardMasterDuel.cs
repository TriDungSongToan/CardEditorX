using System;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using CardEditor.Enums.MasterDuel;
using CardEditor.Converter;

namespace CardEditor.Models.MasterDuel
{
    public class CardMasterDuel
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("konamiID")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string KonamiId { get; set; } = "";

        [JsonPropertyName("gameId")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? GameId { get; set; }

        public string Name { get; set; } = "";

        [JsonPropertyName("name_ja")]
        public string? NameJa { get; set; }

        public string Description { get; set; } = "";

        public CardType Type { get; set; }

        [JsonPropertyName("monsterType")]
        public List<MonsterType> MonsterTypes { get; set; } = new();

        // Dùng chung cho Monster/Spell/Trap subtype
        public Race? Race { get; set; }

        public CardEditor.Enums.MasterDuel.Attribute? Attribute { get; set; }

        public int? Level { get; set; }
        public int? Scale { get; set; }
        public int? Atk { get; set; }
        public int? Def { get; set; }

        [JsonPropertyName("linkRating")]
        public int? LinkRating { get; set; }

        [JsonPropertyName("linkArrows")]
        public List<LinkArrow> LinkArrows { get; set; } = new();

        public Rarity? Rarity { get; set; }

        public bool Generic { get; set; }

        [JsonPropertyName("alternateArt")]
        public bool? AlternateArt { get; set; }

        [JsonPropertyName("isUpdated")]
        public bool? IsUpdated { get; set; }

        // Konami dùng double.MaxValue để biểu thị "không có popRank"
        [JsonPropertyName("popRank")]
        public double PopRankRaw { get; set; }

        [JsonIgnore]
        public double? PopRank => PopRankRaw >= double.MaxValue ? null : PopRankRaw;

        public DateTime? Release { get; set; }

        [JsonPropertyName("nameRelease")]
        public DateTime? NameRelease { get; set; }

        // Ban status không có enum cố định (giá trị như "Limited 1/2", "Forbidden"...) -> để string cho an toàn
        [JsonPropertyName("banStatus")]
        public string? BanStatus { get; set; }

        [JsonPropertyName("ocgBanStatus")]
        public string? OcgBanStatus { get; set; }

        [JsonPropertyName("tcgBanStatus")]
        public string? TcgBanStatus { get; set; }

        [JsonPropertyName("imageHash")]
        public string? ImageHash { get; set; }
    }

    public class CardCache
    {
        public DateTime LastFetched { get; set; }
        public int CardCount { get; set; }
        public List<CardMasterDuel> Cards { get; set; } = new();
    }

    public class RarityMasterDuel
    {
        public ulong KonamiId { get; set; }
        public Rarity? Rarity { get; set; }
    }
    public class RarityCache
    {
        public DateTime LastFetched { get; set; }
        public int CardCount { get; set; }
        public int RarityCount { get; set; }
        public int CardRarityCount { get; set; }
        public List<RarityMasterDuel> CardsRarity { get; set; } = new();

    }
}
