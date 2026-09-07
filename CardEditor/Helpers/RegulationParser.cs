using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using CardEditor.Models;

namespace CardEditor.Helpers
{
    public static class RegulationParser
    {
        public static YamiYugiRegulation Parse(string json)
        {
            var raw = JsonConvert.DeserializeObject<RegulationRawDto>(json)
                       ?? throw new InvalidOperationException("Invalid JSON format.");

            var result = new YamiYugiRegulation
            {
                Date = raw.Date,
                Regulation = new List<CardLimit>()
            };

            if (raw.Regulation != null)
            {
                foreach (var kv in raw.Regulation)
                {
                    if (int.TryParse(kv.Key, out int konamiId))
                    {
                        result.Regulation.Add(new CardLimit
                        {
                            KonamiID = konamiId,
                            LimitedCount = kv.Value,
                            CardID = 0
                        });
                    }
                }
            }

            return result;
        }
    }
}
