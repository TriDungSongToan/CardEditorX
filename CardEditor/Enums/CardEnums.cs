using System;
using System.Collections.Generic;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Enums
{
    #region Card Constant
    [Flags]
    public enum CardType : ulong
    {
        None = 0x0,
        Monster = 0x1,
        Spell = 0x2,
        Trap = 0x4,
        NotUsed1 = 0x8,
        Normal = 0x10,
        Effect = 0x20,
        Fusion = 0x40,
        Ritual = 0x80,
        NotUsed2 = 0x100,
        Spirit = 0x200,
        Union = 0x400,
        Gemini = 0x800,
        Tuner = 0x1000,
        Synchro = 0x2000,
        Token = 0x4000,
        Maximum = 0x8000,
        QuickPlay = 0x10000,
        Continuous = 0x20000,
        Equip = 0x40000,
        Field = 0x80000,
        Counter = 0x100000,
        Flip = 0x200000,
        Toon = 0x400000,
        eXceed = 0x800000,
        Pendulum = 0x1000000,
        SPSummon = 0x2000000,
        Link = 0x4000000,
        Skill = 0x8000000,
        Action = 0x10000000,
        Plus = 0x20000000,
        Minor = 0x40000000,
        Armor = 0x80000000
    }
    [Flags]
    public enum CardRule : ulong
    {
        None = 0x0,
        OCG = 0x1,
        TCG = 0x2,
        Anime = 0x4,
        Illegal = 0x8,
        VideoGame = 0x10,
        Custom = 0x20,
        SpeedDuel = 0x40,
        NA1 = 0x80,
        PreRelease = 0x100,
        Rush = 0x200,
        Legend = 0x400,
        NA2 = 0x800,
        Hidden = 0x1000
    }
    public enum CardPaste
    {
        CardYGO = 1,
        CardOMEGA = 2,
        CardBAN = 3,
    }
    #endregion

    #region Card Field
    [Flags]
    public enum CardFieldNoFlag : ulong
    {
        None = 0,

        Name = 1UL << 0,
        Desc = 1UL << 1,
        Str1 = 1UL << 2,
        Str2 = 1UL << 3,
        Str3 = 1UL << 4,
        Str4 = 1UL << 5,
        Str5 = 1UL << 6,
        Str6 = 1UL << 7,
        Str7 = 1UL << 8,
        Str8 = 1UL << 9,
        Str9 = 1UL << 10,
        Str10 = 1UL << 11,
        Str11 = 1UL << 12,
        Str12 = 1UL << 13,
        Str13 = 1UL << 14,
        Str14 = 1UL << 15,
        Str15 = 1UL << 16,
        Str16 = 1UL << 17,

        Ot = 1UL << 18,
        Alias = 1UL << 19,
        Setcode = 1UL << 20,
        Type = 1UL << 21,
        Atk = 1UL << 22,
        Def = 1UL << 23,
        Level = 1UL << 24,
        Race = 1UL << 25,
        Attribute = 1UL << 26,
        Category = 1UL << 27,
    }
    [Flags]
    public enum CardFieldHasFlag : ulong
    {
        None = 0,

        Name = 1UL << 0,
        Desc = 1UL << 1,
        Str1 = 1UL << 2,
        Str2 = 1UL << 3,
        Str3 = 1UL << 4,
        Str4 = 1UL << 5,
        Str5 = 1UL << 6,
        Str6 = 1UL << 7,
        Str7 = 1UL << 8,
        Str8 = 1UL << 9,
        Str9 = 1UL << 10,
        Str10 = 1UL << 11,
        Str11 = 1UL << 12,
        Str12 = 1UL << 13,
        Str13 = 1UL << 14,
        Str14 = 1UL << 15,
        Str15 = 1UL << 16,
        Str16 = 1UL << 17,

        Ot = 1UL << 18,
        Alias = 1UL << 19,
        Setcode = 1UL << 20,
        Type = 1UL << 21,
        Atk = 1UL << 22,
        Def = 1UL << 23,
        Level = 1UL << 24,
        Race = 1UL << 25,
        Attribute = 1UL << 26,
        Category = 1UL << 27,
        Flag = 1UL << 28,
    }
    [Flags]
    public enum CardOmegaField : ulong
    {
        None = 0,

        Name = 1UL << 0,
        Desc = 1UL << 1,
        Str1 = 1UL << 2,
        Str2 = 1UL << 3,
        Str3 = 1UL << 4,
        Str4 = 1UL << 5,
        Str5 = 1UL << 6,
        Str6 = 1UL << 7,
        Str7 = 1UL << 8,
        Str8 = 1UL << 9,
        Str9 = 1UL << 10,
        Str10 = 1UL << 11,
        Str11 = 1UL << 12,
        Str12 = 1UL << 13,
        Str13 = 1UL << 14,
        Str14 = 1UL << 15,
        Str15 = 1UL << 16,
        Str16 = 1UL << 17,

        Ot = 1UL << 18,
        Alias = 1UL << 19,
        Setcode = 1UL << 20,
        Type = 1UL << 21,
        Atk = 1UL << 22,
        Def = 1UL << 23,
        Level = 1UL << 24,
        Race = 1UL << 25,
        Attribute = 1UL << 26,
        Category = 1UL << 27,
        Genre = 1UL << 28,
        Script = 1UL << 29,
        Support = 1UL << 30
    }
    [Flags]
    public enum CardFieldBanList : ulong
    {
        None = 0,
        Name = 1UL << 0,
    }
    
    public static class CardFieldNoFlagUpdate
    {
        public static readonly Dictionary<CardFieldNoFlag, Action<CardEditor.Models.Card, CardEditor.Models.Card>> FieldUpdaters = new()
        {
            {CardFieldNoFlag.Name, (t, s) => t.name = s.name },
            {CardFieldNoFlag.Desc, (t, s) => t.desc = s.desc },
            {CardFieldNoFlag.Str1, (t, s) => t.str1 = s.str1 },
            {CardFieldNoFlag.Str2, (t, s) => t.str2 = s.str2 },
            {CardFieldNoFlag.Str3, (t, s) => t.str3 = s.str3 },
            {CardFieldNoFlag.Str4, (t, s) => t.str4 = s.str4 },
            {CardFieldNoFlag.Str5, (t, s) => t.str5 = s.str5 },
            {CardFieldNoFlag.Str6, (t, s) => t.str6 = s.str6 },
            {CardFieldNoFlag.Str7, (t, s) => t.str7 = s.str7 },
            {CardFieldNoFlag.Str8, (t, s) => t.str8 = s.str8 },
            {CardFieldNoFlag.Str9, (t, s) => t.str9 = s.str9 },
            {CardFieldNoFlag.Str10, (t, s) => t.str10 = s.str10 },
            {CardFieldNoFlag.Str11, (t, s) => t.str11 = s.str11 },
            {CardFieldNoFlag.Str12, (t, s) => t.str12 = s.str12 },
            {CardFieldNoFlag.Str13, (t, s) => t.str13 = s.str13 },
            {CardFieldNoFlag.Str14, (t, s) => t.str14 = s.str14 },
            {CardFieldNoFlag.Str15, (t, s) => t.str15 = s.str15 },
            {CardFieldNoFlag.Str16, (t, s) => t.str16 = s.str16 },

            {CardFieldNoFlag.Ot, (t, s) => t.ot = s.ot },
            {CardFieldNoFlag.Alias, (t, s) => t.alias = s.alias },
            {CardFieldNoFlag.Setcode, (t, s) => t.setcode = s.setcode },
            {CardFieldNoFlag.Type, (t, s) => t.type = s.type },
            {CardFieldNoFlag.Atk, (t, s) => t.atk = s.atk },
            {CardFieldNoFlag.Def, (t, s) => t.def = s.def },
            {CardFieldNoFlag.Level, (t, s) => t.level = s.level },
            {CardFieldNoFlag.Race, (t, s) => t.race = s.race },
            {CardFieldNoFlag.Attribute, (t, s) => t.attribute = s.attribute },
            {CardFieldNoFlag.Category, (t, s) => t.category = s.category }
        };
    }
    public static class CardFieldHasFlagUpdate
    {
        public static readonly Dictionary<CardFieldHasFlag, Action<CardEditor.Models.Card, CardEditor.Models.Card>> FieldUpdaters = new()
        {
            {CardFieldHasFlag.Name, (t, s) => t.name = s.name },
            {CardFieldHasFlag.Desc, (t, s) => t.desc = s.desc },
            {CardFieldHasFlag.Str1, (t, s) => t.str1 = s.str1 },
            {CardFieldHasFlag.Str2, (t, s) => t.str2 = s.str2 },
            {CardFieldHasFlag.Str3, (t, s) => t.str3 = s.str3 },
            {CardFieldHasFlag.Str4, (t, s) => t.str4 = s.str4 },
            {CardFieldHasFlag.Str5, (t, s) => t.str5 = s.str5 },
            {CardFieldHasFlag.Str6, (t, s) => t.str6 = s.str6 },
            {CardFieldHasFlag.Str7, (t, s) => t.str7 = s.str7 },
            {CardFieldHasFlag.Str8, (t, s) => t.str8 = s.str8 },
            {CardFieldHasFlag.Str9, (t, s) => t.str9 = s.str9 },
            {CardFieldHasFlag.Str10, (t, s) => t.str10 = s.str10 },
            {CardFieldHasFlag.Str11, (t, s) => t.str11 = s.str11 },
            {CardFieldHasFlag.Str12, (t, s) => t.str12 = s.str12 },
            {CardFieldHasFlag.Str13, (t, s) => t.str13 = s.str13 },
            {CardFieldHasFlag.Str14, (t, s) => t.str14 = s.str14 },
            {CardFieldHasFlag.Str15, (t, s) => t.str15 = s.str15 },
            {CardFieldHasFlag.Str16, (t, s) => t.str16 = s.str16 },

            {CardFieldHasFlag.Ot, (t, s) => t.ot = s.ot },
            {CardFieldHasFlag.Alias, (t, s) => t.alias = s.alias },
            {CardFieldHasFlag.Setcode, (t, s) => t.setcode = s.setcode },
            {CardFieldHasFlag.Type, (t, s) => t.type = s.type },
            {CardFieldHasFlag.Atk, (t, s) => t.atk = s.atk },
            {CardFieldHasFlag.Def, (t, s) => t.def = s.def },
            {CardFieldHasFlag.Level, (t, s) => t.level = s.level },
            {CardFieldHasFlag.Race, (t, s) => t.race = s.race },
            {CardFieldHasFlag.Attribute, (t, s) => t.attribute = s.attribute },
            {CardFieldHasFlag.Category, (t, s) => t.category = s.category },
            {CardFieldHasFlag.Flag, (t, s) => t.flag = s.flag },
        };
    }
    public static class CardOmegaFieldUpdate
    {
        public static readonly Dictionary<CardOmegaField, Action<CardEditor.Models.CardOmega, CardEditor.Models.CardOmega>> FieldUpdaters = new()
        {
            {CardOmegaField.Name, (t, s) => t.name = s.name },
            {CardOmegaField.Desc, (t, s) => t.desc = s.desc },
            {CardOmegaField.Str1, (t, s) => t.str1 = s.str1 },
            {CardOmegaField.Str2, (t, s) => t.str2 = s.str2 },
            {CardOmegaField.Str3, (t, s) => t.str3 = s.str3 },
            {CardOmegaField.Str4, (t, s) => t.str4 = s.str4 },
            {CardOmegaField.Str5, (t, s) => t.str5 = s.str5 },
            {CardOmegaField.Str6, (t, s) => t.str6 = s.str6 },
            {CardOmegaField.Str7, (t, s) => t.str7 = s.str7 },
            {CardOmegaField.Str8, (t, s) => t.str8 = s.str8 },
            {CardOmegaField.Str9, (t, s) => t.str9 = s.str9 },
            {CardOmegaField.Str10, (t, s) => t.str10 = s.str10 },
            {CardOmegaField.Str11, (t, s) => t.str11 = s.str11 },
            {CardOmegaField.Str12, (t, s) => t.str12 = s.str12 },
            {CardOmegaField.Str13, (t, s) => t.str13 = s.str13 },
            {CardOmegaField.Str14, (t, s) => t.str14 = s.str14 },
            {CardOmegaField.Str15, (t, s) => t.str15 = s.str15 },
            {CardOmegaField.Str16, (t, s) => t.str16 = s.str16 },

            {CardOmegaField.Ot, (t, s) => t.ot = s.ot },
            {CardOmegaField.Alias, (t, s) => t.alias = s.alias },
            {CardOmegaField.Setcode, (t, s) => t.setcode = s.setcode },
            {CardOmegaField.Type, (t, s) => t.type = s.type },
            {CardOmegaField.Atk, (t, s) => t.atk = s.atk },
            {CardOmegaField.Def, (t, s) => t.def = s.def },
            {CardOmegaField.Level, (t, s) => t.level = s.level },
            {CardOmegaField.Race, (t, s) => t.race = s.race },
            {CardOmegaField.Attribute, (t, s) => t.attribute = s.attribute },
            {CardOmegaField.Category, (t, s) => t.category = s.category },
            {CardOmegaField.Genre, (t, s) => t.genre = s.genre },
            {CardOmegaField.Script, (t, s) => t.script = s.script },
            {CardOmegaField.Support, (t, s) => t.support = s.support }
        };
    }
    public static class CardFieldBanListUpdate
    {
        public static readonly Dictionary<CardFieldBanList, Action<CardEditor.Models.CardBanList, CardEditor.Models.CardBanList>> FieldUpdaters = new()
        {
            {CardFieldBanList.Name, (t, s) => t.Name = s.Name },
        };
    }
    #endregion

    #region Card Sort
    public enum SortType
    {
        ID = 0,
        NAME = 1,
        RULE = 2,
        ALIAS = 3,
        SETCODE = 4,
        TYPE = 5,
        ATK = 6,
        DEF = 7,
        LEVEL = 8,
        RACE = 9,
        ATTRIBUTE = 10,
        CATEGORY = 11,
        FLAG = 12,
        GENRE = 13,
        SUPPORT = 14,
        RARE = 15,
        GPOINT = 16
    }
    public static class SortExtensions
    {
        public static string ToFriendlyString(this SortType arrange)
        {
            return arrange switch
            {
                SortType.ID => CMess.cardID.ToText(),
                SortType.NAME => CMess.cardName.ToText(),
                SortType.RULE => CMess.cardLabelScope.ToText(),
                SortType.ALIAS => CMess.cardAlias.ToText(),
                SortType.SETCODE => CMess.cardlabelSetCode.ToText(),
                SortType.TYPE => CMess.cardLabelType.ToText(),
                SortType.ATK => CMess.cardatk.ToText(),
                SortType.DEF => CMess.carddef.ToText(),
                SortType.LEVEL => CMess.Level.ToText(),
                SortType.RACE => CMess.cardLabelRace.ToText(),
                SortType.ATTRIBUTE => CMess.cardLabelAttri.ToText(),
                SortType.CATEGORY => CMess.cardLabelCategory.ToText(),
                SortType.FLAG => CMess.cardLabelFlag.ToText(),
                SortType.GENRE => CMess.cardLabelGenre.ToText(),
                SortType.SUPPORT => CMess.Support.ToText(),

                SortType.RARE => CMess.cardrare.ToText(),
                SortType.GPOINT => CMess.genesysPoint.ToText(),
                _ => CMess.unknown.ToText(),
            };
        }
    }
    #endregion

    #region Card Filter
    public enum ItemsEdit
    {
        Setting = 0,
        ReplaceDesc = 1,
        ReplaceField = 2,
        ImportData = 3,
        PendulumLanguage = 4,
        Credit = 5,
    }
    [Flags]
    public enum FilterOption
    {
        None = 0,
        Advanced = 1 << 0,
        MatchCase = 1 << 1,
        Wildcards = 1 << 2,
        Prefix = 1 << 3,
        Suffix = 1 << 4,
        MatchWhole = 1 << 5,
        IgnPunct = 1 << 6,
        IgnSpace = 1 << 7
    }

    public enum LanguageArea
    {
        Unknown = 0,
        TCG = 1,
        OCG = 2,
        Mixed = 3
    }

    public enum ScopeCard
    {
        SelectedCards,
        FiltedCards,
        AllCards
    }
    #endregion
}
