using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Interfaces;

namespace CardEditor.Services.LoadData
{
    public class LoadStringService : StringInterface
    {
        private static readonly Regex EntryRx = new Regex(@"^!(?<token>\S+) +(?<id>\S+)(?: (?<text>.*))?$", RegexOptions.Compiled);
        // Dòng disable thiếu '!': "#token ID text"
        private static readonly Regex LooseDisabledRx =
            new Regex(@"^#(?<token>[A-Za-z_]\w*) +(?<id>(?:0[xX][0-9A-Fa-f]+|\d+))(?: (?<text>.*))?$",
                      RegexOptions.Compiled);
        public StringConf LoadStringConf(string filePath)
        {
            var conf = new StringConf { FilePath = filePath };
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);

            // Lượt 1: thu thập token đã biết
            var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var l in lines)
            {
                string b = l.StartsWith("#!", StringComparison.Ordinal) ? l.Substring(1) : l;
                if (b.Length > 0 && b[0] == '!')
                {
                    var m = EntryRx.Match(b);
                    if (m.Success) tokens.Add(m.Groups["token"].Value);
                }
            }

            // Lượt 2: phân loại
            StringSection cur = null;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                bool disabled = false;
                bool loose = false;
                Match m = null;

                if (line[0] == '!')
                    m = EntryRx.Match(line);
                else if (line.StartsWith("#!", StringComparison.Ordinal))
                {
                    m = EntryRx.Match(line.Substring(1));
                    disabled = true;
                }
                else if (line[0] == '#')
                {
                    var lm = LooseDisabledRx.Match(line);
                    if (lm.Success && tokens.Contains(lm.Groups["token"].Value))
                    {
                        m = lm; disabled = true; loose = true;
                    }
                }

                if (m != null && m.Success)
                {
                    if (cur == null)
                    {
                        cur = new StringSection { SectionName = string.Empty };
                        conf.Sections.Add(cur);
                    }
                    cur.Content.Add(new StringSectionContent
                    {
                        Token = m.Groups["token"].Value,
                        ID = m.Groups["id"].Value,
                        Content = m.Groups["text"].Success ? m.Groups["text"].Value : string.Empty,
                        IsDisabled = disabled,
                        LooseDisabled = loose
                    });
                    continue;
                }

                if (line[0] == '#')
                {
                    cur = new StringSection { SectionName = line.Substring(1) };
                    conf.Sections.Add(cur);
                    continue;
                }

                conf.Warnings.Add($"Dòng {i + 1}: không nhận dạng được: {line}");
            }
            return conf;
        }

        private static string Key(string token, string id)
            => token.ToLowerInvariant() + "\n" + id.ToLowerInvariant();
        public StringCombine Merge(StringConf en, StringConf vi)
        {
            // Khóa trùng: ưu tiên dòng đang bật, vì game bỏ qua dòng disable
            var viMap = new Dictionary<string, StringSectionContent>();
            foreach (var s in vi.Sections)
                foreach (var r in s.Content)
                {
                    string k = Key(r.Token, r.ID);
                    StringSectionContent old;
                    if (!viMap.TryGetValue(k, out old) || (old.IsDisabled && !r.IsDisabled))
                        viMap[k] = r;
                }

            var used = new HashSet<string>();
            var result = new StringCombine();

            foreach (var es in en.Sections)
            {
                var sc = new StringSectionCombine { SectionName = es.SectionName };
                var rows = new List<StringSectionContentCombine>(es.Content.Count);

                foreach (var e in es.Content)
                {
                    var row = new StringSectionContentCombine
                    {
                        Token = e.Token,
                        ID = e.ID,
                        IdValue = ToValue(e.ID),
                        SourceContent = e.Content,
                        IsDisabled = e.IsDisabled
                    };

                    string k = Key(e.Token, e.ID);
                    StringSectionContent v;
                    if (viMap.TryGetValue(k, out v))
                    {
                        used.Add(k);
                        row.TranslationContent = v.Content;
                        row.Status = v.Content == e.Content ? StatusConf.Untranslated : StatusConf.Translated;
                    }
                    else
                    {
                        row.TranslationContent = e.Content;
                        row.Status = StatusConf.New;
                    }
                    row.IsInitialized = true;   // đặt sau cùng
                    rows.Add(row);
                }
                sc.Content.AddRange(rows);
                result.Sections.Add(sc);
            }

            foreach (var kv in viMap)
                if (!used.Contains(kv.Key))
                    result.RemovedKeys.Add(kv.Value.Token + " " + kv.Value.ID);
            return result;
        }
        private static long ToValue(string id)
        {
            long v;
            if (id.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return long.TryParse(id.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v) ? v : long.MaxValue;
            return long.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out v) ? v : long.MaxValue;
        }

    }
}
