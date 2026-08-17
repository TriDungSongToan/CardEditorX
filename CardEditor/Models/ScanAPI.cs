using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CardEditor.Models
{
    public sealed record ScanProgress(int Page, int Count, int TotalItems, int UniqueItems, string Message);
    public sealed class ScanResult
    {
        public HashSet<string> Values { get; init; }  = new(StringComparer.Ordinal);
        public int TotalPages { get; init; }
        public int TotalItems { get; init; }
    }

    public sealed class CardAtkDefResult
    {
        public string Name { get; init; } = "";
        public string Atk { get; init; } = "";
        public string Def { get; init; } = "";
        public JsonElement CardJson { get; init; }
    }

    public sealed class ApiCountResult
    {
        public int TotalPages { get; init; }
        public int TotalItems { get; init; }
    }
}
