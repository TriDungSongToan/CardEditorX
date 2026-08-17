using System.Text.Json;
using System.Collections.Generic;

namespace CardEditor.Helpers
{
    public static class Extractors
    {
        public static IEnumerable<string>? TypeExtractor(JsonElement el)
        {
            if (el.TryGetProperty("type", out var t) && t.ValueKind == JsonValueKind.String)
                yield return t.GetString()!;
        }

        public static IEnumerable<string>? MonsterTypeExtractor(JsonElement el)
        {
            if (!el.TryGetProperty("monsterType", out var arr) || arr.ValueKind != JsonValueKind.Array) yield break;
            foreach (var x in arr.EnumerateArray())
                if (x.ValueKind == JsonValueKind.String)
                    yield return x.GetString()!;
        }

        public static IEnumerable<string>? RaceExtractor(JsonElement el)
        {
            if (el.TryGetProperty("race", out var r) && r.ValueKind == JsonValueKind.String)
                yield return r.GetString()!;
        }

        public static IEnumerable<string>? LinkArrowsExtractor(JsonElement el)
        {
            if (!el.TryGetProperty("linkArrows", out var arr) || arr.ValueKind != JsonValueKind.Array) yield break;
            foreach (var x in arr.EnumerateArray())
                if (x.ValueKind == JsonValueKind.String)
                    yield return x.GetString()!;
        }
    }
}
