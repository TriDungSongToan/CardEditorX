using System;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services
{
    public static class ScriptData
    {
        private static SymbolOverload BuildRootFunctionOverload(FunctionYaml yaml)
        {
            return new SymbolOverload
            {
                Description = yaml.description,
                Parameters = yaml.parameters?.Select(ConvertParameter).ToList() ?? new List<SymbolParameter>(),

                Returns = ConvertReturns(yaml.returns)
            };
        }
        private static SymbolReturn BuildRootFunctionReturn(YamlReturn yaml)
        {
            return new SymbolReturn
            {
                Name = yaml.name,
                Types = yaml.type,
                Description = yaml.description,
            };
        }
        private static IReadOnlyList<SymbolReturn> ConvertReturns(List<YamlReturn> yamlReturns)
        {
            if (yamlReturns == null || yamlReturns.Count == 0) return Array.Empty<SymbolReturn>();

            return yamlReturns.Select(r => new SymbolReturn
            {
                Types = r.type,
                Description = r.description
            }).ToList();
        }

        private static SymbolOverload ConvertOverload(YamlOverload ov)
        {
            return new SymbolOverload
            {
                Description = ov.description,
                Parameters = ov.parameters?
                    .Select(ConvertParameter)
                    .ToList()
                    ?? new List<SymbolParameter>(),
                Returns = new List<SymbolReturn>()
            };
        }
        private static SymbolParameter ConvertParameter(YamlParameter p)
        {
            bool isVariadic = p.name == "...";

            return new SymbolParameter
            {
                Name = p.name,
                Types = p.type ?? new List<string>(),
                IsOptional = p.required == false || p.defaultValue != null,
                IsVariadic = isVariadic,
                Description = p.description
            };
        }

        public static (bool, string) ExportConstantsToJson(string folderPath = null)
        {
            folderPath ??= FileDiaLogHelper.OpenFolder("Constants Folder (scrapiyard Folder)");
            if (string.IsNullOrWhiteSpace(folderPath) || !System.IO.Directory.Exists(folderPath))
                return (false, CMess.folderNotExit.ToText());
            int CountSuccesses = 0;
            int CountSkip = 0;
            int CountErrors = 0;
            try
            {
                var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

                var symbols = new List<CompletionSymbol>();
                List<string> failedFiles = new();
                foreach (string file in Directory.GetFiles(folderPath, "*.yml", SearchOption.AllDirectories))
                {
                    string text = File.ReadAllText(file);
                    if (!text.StartsWith("---!constant"))
                    {
                        CountSkip++;
                        continue;
                    }

                    int idx = text.IndexOf('\n');
                    if (idx > 0) text = text.Substring(idx + 1);

                    ConstantYaml yaml;
                    try
                    {
                        yaml = deserializer.Deserialize<ConstantYaml>(text);
                    }
                    catch (Exception ex)
                    {
                        CountErrors++;
                        failedFiles.Add($"{file} | Error: {ex.Message}");
                        continue;
                    }

                    if (yaml == null || string.IsNullOrEmpty(yaml.name)) continue;

                    symbols.Add(new CompletionSymbol
                    {
                        Kind = SymbolKind.Constant,
                        Name = yaml.name,
                        Namespace = string.Empty,
                        OwnerEnum = yaml.@enum,
                        Value = yaml.value,
                        Summary = yaml.summary,
                        Description = yaml.description,
                        Status = yaml.status
                    });
                    CountSuccesses++;
                }

                string json = JsonSerializer.Serialize(symbols, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, "constants.json"), json);
                File.WriteAllLines("constants_deserialize_failed.txt", failedFiles);
                return (true, $"Thanh cong: {CountSuccesses}\nBo qua: {CountSkip}\nThat bai: {CountErrors}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public static (bool, string) ExportEnumsToJson(string folderPath = null)
        {
            folderPath ??= FileDiaLogHelper.OpenFolder("Enums Folder (scrapiyard Folder)");
            if (string.IsNullOrWhiteSpace(folderPath) || !System.IO.Directory.Exists(folderPath))
                return (false, CMess.folderNotExit.ToText());
            int CountSuccesses = 0;
            int CountSkip = 0;
            int CountErrors = 0;
            try
            {
                var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

                var symbols = new List<CompletionSymbol>();
                List<string> failedFiles = new();
                foreach (string file in Directory.GetFiles(folderPath, "*.yml", SearchOption.AllDirectories))
                {
                    string text = File.ReadAllText(file);
                    if (!text.StartsWith("---!enum"))
                    {
                        CountSkip++;
                        continue;
                    }
                    int idx = text.IndexOf('\n');
                    if (idx > 0) text = text.Substring(idx + 1);

                    EnumYaml yaml;
                    try
                    {
                        yaml = deserializer.Deserialize<EnumYaml>(text);
                    }
                    catch (Exception ex)
                    {
                        CountErrors++;
                        failedFiles.Add($"{file} | Error: {ex.Message}");
                        continue;
                    }

                    if (yaml == null || string.IsNullOrEmpty(yaml.name)) continue;

                    symbols.Add(new CompletionSymbol
                    {
                        Kind = SymbolKind.Enum,
                        Name = yaml.name,
                        Namespace = string.Empty,
                        Summary = yaml.summary,
                        Description = yaml.description,
                        IsBitmask = yaml.bitmaskInt,
                        Tags = yaml.tags ?? new List<string>()
                    });
                    CountSuccesses++;
                }

                string json = JsonSerializer.Serialize(symbols, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, "enums.json"), json);
                File.WriteAllLines("enums_deserialize_failed.txt", failedFiles);
                return (true, $"Thanh cong: {CountSuccesses}\nBo qua: {CountSkip}\nThat bai: {CountErrors}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public static (bool, string) ExportFunctionsToJson(string folderPath = null)
        {
            folderPath ??= FileDiaLogHelper.OpenFolder("Functions Folder (scrapiyard Folder)");
            if (string.IsNullOrWhiteSpace(folderPath) || !System.IO.Directory.Exists(folderPath))
                return (false, CMess.folderNotExit.ToText());
            int CountSuccesses = 0;
            int CountSkip = 0;
            int CountErrors = 0;
            try
            {
                var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

                var symbols = new List<CompletionSymbol>();
                List<string> failedFiles = new();

                foreach (string file in Directory.GetFiles(folderPath, "*.yml", SearchOption.AllDirectories))
                {
                    string text = File.ReadAllText(file);
                    if (!text.StartsWith("---!function"))
                    {
                        CountSkip++;
                        continue;
                    }
                    int idx = text.IndexOf('\n');
                    if (idx > 0) text = text.Substring(idx + 1);

                    FunctionYaml yaml;

                    try
                    {
                        yaml = deserializer.Deserialize<FunctionYaml>(text);
                    }
                    catch (Exception ex)
                    {
                        CountErrors++;
                        failedFiles.Add($"{file} | Error: {ex.Message}");
                        continue;
                    }

                    if (yaml == null || string.IsNullOrEmpty(yaml.name)) continue;

                    var overloads = new List<SymbolOverload>{ BuildRootFunctionOverload(yaml)};

                    if (yaml.overloads != null)
                    {
                        overloads.AddRange(yaml.overloads.Select(ConvertOverload));
                    }

                    symbols.Add(new CompletionSymbol
                    {
                        Kind = SymbolKind.Function,
                        Name = yaml.name,
                        Namespace = yaml.@namespace ?? string.Empty,
                        Summary = yaml.summary,
                        Description = yaml.description,
                        Overloads = overloads
                    });
                    CountSuccesses++;
                }

                string json = JsonSerializer.Serialize(symbols, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, "functions.json"), json);
                File.WriteAllLines("function_deserialize_failed.txt", failedFiles);
                return (true, $"Thanh cong: {CountSuccesses}\nBo qua: {CountSkip}\nThat bai: {CountErrors}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public static (bool, string) ExportNameSpacesToJson(string folderPath = null)
        {
            folderPath ??= FileDiaLogHelper.OpenFolder("NameSpaces Folder (scrapiyard Folder)");
            if (string.IsNullOrWhiteSpace(folderPath) || !System.IO.Directory.Exists(folderPath))
                return (false, CMess.folderNotExit.ToText());
            int CountSuccesses = 0;
            int CountSkip = 0;
            int CountErrors = 0;
            try
            {
                var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

                var symbols = new List<CompletionSymbol>();
                List<string> failedFiles = new();
                foreach (string file in Directory.GetFiles(folderPath, "*.yml", SearchOption.AllDirectories))
                {
                    string text = File.ReadAllText(file);
                    if (!text.StartsWith("---!namespace"))
                    {
                        CountSkip++;
                        continue;
                    }

                    int idx = text.IndexOf('\n');
                    if (idx > 0) text = text.Substring(idx + 1);

                    NameSpaceYaml yaml;
                    try
                    {
                        yaml = deserializer.Deserialize<NameSpaceYaml>(text);
                    }
                    catch (Exception ex)
                    {
                        CountErrors++;
                        failedFiles.Add($"{file} | Error: {ex.Message}");
                        continue;
                    }

                    if (yaml == null || string.IsNullOrEmpty(yaml.name)) continue;

                    symbols.Add(new CompletionSymbol
                    {
                        Kind = SymbolKind.NameSpace,
                        Name = yaml.name,
                        Namespace = yaml.name,
                        Summary = yaml.summary,
                        Description = yaml.description,
                        Status = yaml.status
                    });
                    CountSuccesses++;
                }

                string json = JsonSerializer.Serialize(symbols, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, "namespaces.json"), json);
                File.WriteAllLines("namespaces_deserialize_failed.txt", failedFiles);
                return (true, $"Thanh cong: {CountSuccesses}\nBo qua: {CountSkip}\nThat bai: {CountErrors}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public static (bool, string) ExportTagToJson(string folderPath = null)
        {
            folderPath ??= FileDiaLogHelper.OpenFolder("Tags Folder (scrapiyard Folder)");
            if (string.IsNullOrWhiteSpace(folderPath) || !System.IO.Directory.Exists(folderPath))
                return (false, CMess.folderNotExit.ToText());
            int CountSuccesses = 0;
            int CountSkip = 0;
            int CountErrors = 0;
            try
            {
                var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

                var symbols = new List<CompletionSymbol>();
                List<string> failedFiles = new();
                foreach (string file in Directory.GetFiles(folderPath, "*.yml", SearchOption.AllDirectories))
                {
                    string text = File.ReadAllText(file);
                    if (!text.StartsWith("---!tag"))
                    {
                        CountSkip++;
                        continue;
                    }
                    int idx = text.IndexOf('\n');
                    if (idx > 0) text = text.Substring(idx + 1);

                    TagYaml yaml;
                    try
                    {
                        yaml = deserializer.Deserialize<TagYaml>(text);
                    }
                    catch (Exception ex)
                    {
                        CountErrors++;
                        failedFiles.Add($"{file} | Error: {ex.Message}");
                        continue;
                    }

                    if (yaml == null || string.IsNullOrEmpty(yaml.name)) continue;

                    symbols.Add(new CompletionSymbol
                    {
                        Kind = SymbolKind.Tag,
                        Name = yaml.name,
                        Namespace = string.Empty,
                        Summary = yaml.summary,
                        Description = yaml.description,
                        Links = yaml.suggestedLinks
                    });
                    CountSuccesses++;
                }

                string json = JsonSerializer.Serialize(symbols, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, "tags.json"), json);
                File.WriteAllLines("tag_deserialize_failed.txt", failedFiles);
                return (true, $"Thanh cong: {CountSuccesses}\nBo qua: {CountSkip}\nThat bai: {CountErrors}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public static (bool, string) ExportTypesToJson(string folderPath = null)
        {
            folderPath ??= FileDiaLogHelper.OpenFolder("Types Folder (scrapiyard Folder)");
            if (string.IsNullOrWhiteSpace(folderPath) || !System.IO.Directory.Exists(folderPath))
                return (false, CMess.folderNotExit.ToText());
            int CountSuccesses = 0;
            int CountSkip = 0;
            int CountErrors = 0;
            try
            {
                var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

                var symbols = new List<CompletionSymbol>();
                List<string> failedFiles = new();
                foreach (string file in Directory.GetFiles(folderPath, "*.yml", SearchOption.AllDirectories))
                {
                    string text = File.ReadAllText(file);
                    if (!text.StartsWith("---!type"))
                    {
                        CountSkip++;
                        continue;
                    }
                    int idx = text.IndexOf('\n');
                    if (idx > 0) text = text.Substring(idx + 1);

                    TypeYaml yaml;

                    try
                    {
                        yaml = deserializer.Deserialize<TypeYaml>(text);
                    }
                    catch (Exception ex)
                    {
                        CountErrors++;
                        failedFiles.Add($"{file} | Error: {ex.Message}");
                        continue;
                    }

                    if (yaml == null || string.IsNullOrWhiteSpace(yaml.name)) continue;

                    symbols.Add(new CompletionSymbol
                    {
                        Kind = SymbolKind.Type,
                        Name = yaml.name,
                        Namespace = string.Empty,
                        Summary = yaml.summary,
                        Description = yaml.description,
                        Tags = yaml.tags ?? new List<string>(),
                        Status = yaml.status,
                        supertype = yaml.supertype,
                        Links = yaml.suggestedLinks,
                        DeclaredType = yaml
                    });
                    CountSuccesses++;
                }

                string json = JsonSerializer.Serialize(symbols, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, "types.json"), json);
                File.WriteAllLines("type_deserialize_failed.txt", failedFiles);
                return (true, $"Thanh cong: {CountSuccesses}\nBo qua: {CountSkip}\nThat bai: {CountErrors}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (IReadOnlyList<CompletionSymbol>, string) LoadALlData()
        {
            try
            {
                var result = new List<CompletionSymbol>();
                result.AddRange(LoadFile(System.IO.Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, $@"CardData\Language\{ConfigViewModel.Instance.userSetting.Language}\scriptinfo\constants.json")));
                result.AddRange(LoadFile(System.IO.Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, $@"CardData\Language\{ConfigViewModel.Instance.userSetting.Language}\scriptinfo\enums.json")));
                result.AddRange(LoadFile(System.IO.Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, $@"CardData\Language\{ConfigViewModel.Instance.userSetting.Language}\scriptinfo\functions.json")));
                result.AddRange(LoadFile(System.IO.Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, $@"CardData\Language\{ConfigViewModel.Instance.userSetting.Language}\scriptinfo\namespaces.json")));
                result.AddRange(LoadFile(System.IO.Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, $@"CardData\Language\{ConfigViewModel.Instance.userSetting.Language}\scriptinfo\tags.json")));
                result.AddRange(LoadFile(System.IO.Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, $@"CardData\Language\{ConfigViewModel.Instance.userSetting.Language}\scriptinfo\types.json")));

                return (result, string.Empty);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
        private static IReadOnlyList<CompletionSymbol> LoadFile(string path)
        {
            if (!File.Exists(path)) return Array.Empty<CompletionSymbol>();

            var json = File.ReadAllText(path);

            var symbols = JsonSerializer.Deserialize<List<CompletionSymbol>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                }
            );

            if (symbols != null)
            {
                Debug.WriteLine($"File {System.IO.Path.GetFileName(path)} co: {symbols.Count()} item hop le.");
                return symbols;
            }
            else return Array.Empty<CompletionSymbol>();
        }
    }
}
