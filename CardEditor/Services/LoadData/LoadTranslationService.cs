using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Interfaces;

namespace CardEditor.Services.LoadData
{
    public class LoadTranslationService : TranslationInterface
    {
        private const string Separator = "->";

        public TranslationConf LoadTranslationConfig(string filePath)
        {
            TranslationConf conf = new TranslationConf { FilePath = filePath };

            var parsed = new List<(TranslationSection Section, List<TranslationSectionContent> Lines)>();
            var seenKeys = new HashSet<string>(StringComparer.Ordinal);

            TranslationSection? curSection = null;
            List<TranslationSectionContent>? curLines = null;

            void OpenSection(string name)
            {
                curSection = new TranslationSection { SectionName = name };
                curLines = new List<TranslationSectionContent>();
                parsed.Add((curSection, curLines));
            }

            int lineNo = 0;
            foreach (var line in System.IO.File.ReadLines(filePath, Encoding.UTF8))
            {
                lineNo++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Dòng '#' = bắt đầu một Section mới
                if (line[0] == '#')
                {
                    OpenSection(line.TrimStart('#').Trim());
                    continue;
                }

                // Tách tại "->" đầu tiên. Không Trim(): game so khớp từng ký tự.
                int idx = line.IndexOf(Separator, StringComparison.Ordinal);
                if (idx < 0)
                {
                    conf.Warnings.Add($"Dòng {lineNo}: thiếu '->': {line}");
                    continue;
                }

                // Dòng nằm trước Section đầu tiên (nếu có) vào Section không tên
                if (curLines == null)
                    OpenSection(string.Empty);

                string source = line.Substring(0, idx);
                string translation = line.Substring(idx + Separator.Length);

                if (!seenKeys.Add(source))
                    conf.Warnings.Add($"Dòng {lineNo}: khóa trùng: {source}");

                curLines!.Add(new TranslationSectionContent
                {
                    SourceContent = source,
                    TranslationContent = translation
                });
            }

            // Nạp vào collection một lần (AddRange) thay vì Add từng dòng
            foreach (var (section, lines) in parsed)
                section.Content.AddRange(lines);

            conf.Sections.AddRange(parsed.Select(p => p.Section));
            return conf;
        }
        public Task<TranslationConf> LoadTranslationConfigAsync(string filePath)
            => Task.Run(() => LoadTranslationConfig(filePath));

        public Dictionary<string, string> LoadDictionaryTranslation(string filePath)
        {
            var conf = LoadTranslationConfig(filePath);
            return BuildDictionary(conf);
        }
        public Dictionary<string, string> BuildDictionary(TranslationConf conf)
        {
            var dict = new Dictionary<string, string>(StringComparer.Ordinal);

            foreach (var section in conf.Sections)
                foreach (var line in section.Content)
                    if (!dict.ContainsKey(line.SourceContent))
                        dict[line.SourceContent] = line.TranslationContent;

            return dict;
        }
        public Task<Dictionary<string, string>> LoadDictionaryTranslationAsync(string filePath)
            => Task.Run(() => LoadDictionaryTranslation(filePath));

        public TranslationCombine Merge(TranslationConf en, Dictionary<string, string> vi)
        {
            var result = new TranslationCombine();
            var used = new HashSet<string>(StringComparer.Ordinal);

            foreach (var enSection in en.Sections)
            {
                var rows = new List<TranslationSectionContentCombine>(enSection.Content.Count);

                foreach (var line in enSection.Content)
                {
                    var row = new TranslationSectionContentCombine
                    {
                        Source = line.SourceContent,
                        SourceContent = line.TranslationContent
                    };

                    string tr;
                    if (vi.TryGetValue(row.Source, out tr))
                    {
                        used.Add(row.Source);
                        row.TranslationContent = tr;
                        row.Status = tr == row.SourceContent ? StatusConf.Untranslated : StatusConf.Translated;
                    }
                    else
                    {
                        row.TranslationContent = row.SourceContent;
                        row.Status = StatusConf.New;
                    }

                    row.IsInitialized = true;   // đặt SAU CÙNG, để việc gán ở trên không kích hoạt tự đổi Status
                    rows.Add(row);
                }

                var sc = new TranslationSectionCombine { SectionName = enSection.SectionName };
                sc.Content.AddRange(rows);
                result.Sections.Add(sc);
            }

            foreach (var key in vi.Keys)
                if (!used.Contains(key))
                    result.RemovedKeys.Add(key);

            return result;
        }
    }
}
