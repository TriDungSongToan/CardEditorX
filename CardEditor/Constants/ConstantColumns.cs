using System;
using System.Collections.Generic;

namespace CardEditor.Constants
{
    public static class ConstantExtension
    {
        public static readonly HashSet<string> CardDBExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".db",
            ".cdb",
            ".bytes",
            ".sqlite",
        };
        public static readonly HashSet<string> ExcelExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".xlsx",
            ".xlsm",
            ".xltx",
            ".xltm"
        };
        public static readonly HashSet<string> CedsExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".ceds",
            ".txt"
        };
        public static readonly HashSet<string> ScriptExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".lua",
            ".txt",
            ".md",
            ".log",
            ".yml",
            ".conf"
        };
        public static readonly HashSet<string> DeckExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".ydk",
        };
    }

    public static class ConstantColumnDatabase
    {
        public static readonly HashSet<string> DatabaseTable = new(StringComparer.OrdinalIgnoreCase)
        {
            "texts", "datas"
        };
        public static readonly HashSet<string> TextsTableColumn = new(StringComparer.OrdinalIgnoreCase)
        {
            "id", "name", "desc", "str1", "str2", "str3", "str4", "str5", "str6", "str7",
            "str8", "str9", "str10", "str11", "str12", "str13", "str14", "str15", "str16"
        };
        public static readonly HashSet<string> DataTableYGOColumn = new(StringComparer.OrdinalIgnoreCase)
        {
            "id", "ot", "alias", "setcode", "type", "atk", "def", "level", "race", "attribute", "category"
        };
        public static readonly HashSet<string> DataTableOMegaColumn = new(StringComparer.OrdinalIgnoreCase)
        {
            "id", "ot", "alias", "setcode", "type", "atk", "def", "level", "race", "attribute", "category", "genre", "script", "support"
        };
        public static readonly HashSet<string> ListNameColumn = new(StringComparer.OrdinalIgnoreCase)
        {
            "id", "name"
        };
        public static readonly HashSet<string> DataTableFlagColumn = new(StringComparer.OrdinalIgnoreCase)
        {
            "flag"
        };
    }

    public static class ConstantColumnExcel
    {
        public static readonly string[] HeaderNoFlag =
        {
            "CardEditorX", "name", "desc",
            "ot", "alias", "setcode", "type", "atk", "def", "level", "race", "attribute", "category",
            "str1", "str2", "str3", "str4", "str5", "str6", "str7", "str8", "str9", "str10", "str11", "str12", "str13", "str14", "str15", "str16"
        };
        public static readonly string[] HeaderWithFlag =
        {
            "CardEditorX", "name", "desc",
            "ot", "alias", "setcode", "type", "atk", "def", "level", "race", "attribute", "category", "flag",
            "str1", "str2", "str3", "str4", "str5", "str6", "str7", "str8", "str9", "str10", "str11", "str12", "str13", "str14", "str15", "str16"
        };

        public static readonly HashSet<string> HeaderNoFlagSet = new(HeaderNoFlag, StringComparer.OrdinalIgnoreCase);
        public static readonly HashSet<string> HeaderWithFlagSet = new(HeaderWithFlag, StringComparer.OrdinalIgnoreCase);

    }
}
