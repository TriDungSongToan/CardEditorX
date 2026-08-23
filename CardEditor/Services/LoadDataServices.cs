using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Data.SQLite;
using System.Text.Encodings.Web;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using CardEditor.Models;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services
{
    public static class LoadDataServices
    {
        private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();
        private static JsonSerializerOptions CreateSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }
        public static readonly HashSet<string> KnownDbExtensions
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".db", ".cdb", ".sqlite", ".xlsx", ".ceds" };
        private const string ScriptExtension = ".lua";
        private const string DeckExtension = ".ydk";
        private const string BanlistSuffix = ".lflist.conf";

        public static Task<(Dictionary<ulong, List<string>>, string)> LoadAllScriptPaths()
        {
            return Task.Run(() =>
            {
                string dataSourcePath = CardEditor.ViewModels.ConfigViewModel.Instance.userSetting.DataSource;
                if (!Directory.Exists(dataSourcePath)) return (null, $"{CMess.dataSourceErr.ToText()} {dataSourcePath}");

                try
                {
                    var concurrentDict = new System.Collections.Concurrent.ConcurrentDictionary<ulong, List<string>>(
                            Environment.ProcessorCount, 25000);

                    var files = Directory.EnumerateFiles(dataSourcePath, "*.lua", SearchOption.AllDirectories);

                    Parallel.ForEach(files, new ParallelOptions{ MaxDegreeOfParallelism = Environment.ProcessorCount }, filePath =>
                    {
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        if (fileName.Length <= 1 || fileName[0] != 'c') return;
                        // Parse số thủ công (nhanh hơn TryParse)
                        ulong cardId = 0;
                        for (int i = 1; i < fileName.Length; i++)
                        {
                            char c = fileName[i];
                            if (c < '0' || c > '9') return;
                            cardId = cardId * 10 + (ulong)(c - '0');
                        }
                        if (cardId == 0) return;
                        concurrentDict.AddOrUpdate(cardId, _ => new List<string>(1) { filePath }, (_, list) =>
                        {
                            lock (list)
                            {
                                list.Add(filePath);
                                return list;
                            }
                        });
                    });
                    return (new Dictionary<ulong, List<string>>(concurrentDict), string.Empty);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }
            });
        }

        public static async Task<(bool Success, List<CardEditor.Models.Card> Cards,
            Dictionary<ulong, List<string>> Paths, string Message)> LoadAllCdbFiles()
        {
            string dataSourcePath = CardEditor.ViewModels.ConfigViewModel.Instance.userSetting.DataSource;
            if (!Directory.Exists(dataSourcePath)) return (false, null, null, $"{CMess.dataSourceErr.ToText()} {dataSourcePath}");

            var allCards = new List<CardEditor.Models.Card>(25000);
            var cardPaths = new Dictionary<ulong, List<string>>(25000);

            try
            {
                var cdbFiles = Directory.EnumerateFiles(dataSourcePath, "*.cdb", SearchOption.AllDirectories);
                foreach (var cdbFile in cdbFiles)
                {
                    try
                    {
                        var (cards, message) = await LoadDatabaseCard(cdbFile);
                        if (cards == null || cards.Count == 0) continue;

                        foreach (var card in cards)
                        {
                            if (card == null || card.id == 0) continue;

                            if (!cardPaths.TryGetValue(card.id, out var list))
                            {
                                list = new List<string>(1);
                                cardPaths[card.id] = list;
                                allCards.Add(card);
                            }
                            list.Add(cdbFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading {cdbFile}: {ex.Message}");
                    }
                }
                return (true, allCards, cardPaths, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, null, null, ex.Message);
            }
        }
        public static async Task<(List<CardEditor.Models.Card>, string)> LoadDatabaseCard(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return (null, CMess.fileNotExit.ToText());

            List<CardEditor.Models.Card> Cards = new List<CardEditor.Models.Card>();
            string connectionString = $"Data Source={filePath};Version=3;";
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                            SELECT
                                texts.id, texts.name, texts.desc, texts.str1, texts.str2, texts.str3, texts.str4, texts.str5,
                                texts.str6, texts.str7, texts.str8, texts.str9, texts.str10, texts.str11, texts.str12,
                                texts.str13, texts.str14, texts.str15, texts.str16,
                                datas.ot, datas.alias, datas.setcode, datas.type, datas.atk, datas.def, datas.level,
                                datas.race, datas.attribute, datas.category
                            FROM texts
                            INNER JOIN datas ON texts.id = datas.id";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = await command.ExecuteReaderAsync() as SQLiteDataReader)
                        {
                            while (await reader.ReadAsync())
                            {
                                try
                                {
                                    if (reader.IsDBNull(0) || !ulong.TryParse(reader.GetValue(0)?.ToString(), out ulong idvar)) continue;
                                    var card = new CardEditor.Models.Card
                                    {
                                        id = idvar,

                                        name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                        desc = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                        str1 = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                        str2 = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                        str3 = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                        str4 = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                                        str5 = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                                        str6 = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                                        str7 = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                                        str8 = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                                        str9 = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                                        str10 = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
                                        str11 = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
                                        str12 = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
                                        str13 = reader.IsDBNull(15) ? string.Empty : reader.GetString(15),
                                        str14 = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
                                        str15 = reader.IsDBNull(17) ? string.Empty : reader.GetString(17),
                                        str16 = reader.IsDBNull(18) ? string.Empty : reader.GetString(18),

                                        ot = reader.IsDBNull(19) ? 0UL : (ulong.TryParse(reader.GetValue(19)?.ToString(), out ulong otvar) ? otvar : 0UL),
                                        alias = reader.IsDBNull(20) ? 0UL : (ulong.TryParse(reader.GetValue(20)?.ToString(), out ulong aliasvar) ? aliasvar : 0UL),
                                        setcode = reader.IsDBNull(21) ? 0UL : (ulong.TryParse(reader.GetValue(21)?.ToString(), out ulong setcodevar) ? setcodevar : 0UL),
                                        type = reader.IsDBNull(22) ? 0UL : (ulong.TryParse(reader.GetValue(22)?.ToString(), out ulong typevar) ? typevar : 0UL),
                                        atk = reader.IsDBNull(23) ? 0L : (long.TryParse(reader.GetValue(23)?.ToString(), out long atkvar) ? atkvar : 0L),
                                        def = reader.IsDBNull(24) ? 0L : (long.TryParse(reader.GetValue(24)?.ToString(), out long defvar) ? defvar : 0L),
                                        level = reader.IsDBNull(25) ? 0UL : (ulong.TryParse(reader.GetValue(25)?.ToString(), out ulong levelvar) ? levelvar : 0UL),
                                        race = reader.IsDBNull(26) ? 0UL : (ulong.TryParse(reader.GetValue(26)?.ToString(), out ulong racevar) ? racevar : 0UL),
                                        attribute = reader.IsDBNull(27) ? 0UL : (ulong.TryParse(reader.GetValue(27)?.ToString(), out ulong attributevar) ? attributevar : 0UL),
                                        category = reader.IsDBNull(28) ? 0UL : (ulong.TryParse(reader.GetValue(28)?.ToString(), out ulong categoryvar) ? categoryvar : 0UL)
                                    };
                                    Cards.Add(card);
                                }
                                catch (Exception ex)
                                {
                                    string cardId = reader.IsDBNull(0) ? "Unknown" : reader.GetValue(0).ToString();
                                    string logMessage = $"Conversion error occurred at ID = {cardId}. Exception: {ex.Message}";
                                    System.IO.File.AppendAllText(CardEditor.Models.AppContext.Instance.ErrorLogFilePath,
                                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {logMessage} {Environment.NewLine}");
                                }
                            }
                        }
                    }
                }
                return (Cards, string.Empty);
            }
            catch (SQLiteException ex)
            {
                return (null, $"{CMess.errorConDB.ToText()} {ex.Message}");
            }
            catch (Exception ex)
            {
                return (null, $"{CMess.errorOcc.ToText()} {ex.Message}");
            }
        }
        public static async Task<(BanList, string)> LoadFileBanList(string filePath)
        {
            if (!File.Exists(filePath)) return (null, CMess.fileNotExit.ToText());

            var banList = new BanList
            {
                Name = string.Empty,
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                CardList = new Dictionary<ulong, CardBanList>(),
                WhiteList = false
            };

            try
            {
                const int bufferSize = 4096;
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);

                string line;

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

                    switch (line[0])
                    {
                        case '!': banList.Name = ParseBanListName(line); break;
                        case '$': banList.WhiteList = true; break;
                        default:
                            var card = ParseCardLine(line);
                            if (card != null)
                                banList.CardList[card.Id] = card;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
            return (banList, string.Empty);
        }
        private static string ParseBanListName(string rawLine)
        {
            if (string.IsNullOrWhiteSpace(rawLine))
                return string.Empty;
            var name = rawLine.Substring(1).Trim();
            if (name.StartsWith("["))
                name = name.Substring(1).Trim();
            if (name.EndsWith("]"))
                name = name.Substring(0, name.Length - 1).Trim();

            return name;
        }
        private static CardBanList ParseCardLine(string line)
        {
            var parts = line.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return null;

            if (!ulong.TryParse(parts[0], out var id)) return null;
            if (!int.TryParse(parts[1], out var count)) return null;
            int countFinal;
            if (count <= 0) countFinal = 0;
            else if (count > 3) countFinal = 3;
            else countFinal = count;

            var name = parts[2].TrimStart('-', ' ');
            return new CardBanList
            {
                Id = id,
                Name = name,
                LimitedCount = countFinal
            };
        }
        public static async Task<(List<CardEditor.Models.CardBanList>, string message)> LoadDatabaseCardBanList(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return (null, CMess.fileNotExit.ToText());

            List<CardEditor.Models.CardBanList> Cards = new List<CardEditor.Models.CardBanList>();
            string connectionString = $"Data Source={filePath};Version=3;";
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"
                            SELECT
                                texts.id, texts.name, texts.desc, texts.str1, texts.str2, texts.str3, texts.str4, texts.str5,
                                texts.str6, texts.str7, texts.str8, texts.str9, texts.str10, texts.str11, texts.str12,
                                texts.str13, texts.str14, texts.str15, texts.str16,
                                datas.ot, datas.alias, datas.setcode, datas.type, datas.atk, datas.def, datas.level,
                                datas.race, datas.attribute, datas.category
                            FROM texts
                            INNER JOIN datas ON texts.id = datas.id";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (SQLiteDataReader reader = await command.ExecuteReaderAsync() as SQLiteDataReader)
                        {
                            while (await reader.ReadAsync())
                            {
                                try
                                {
                                    if (reader.IsDBNull(0) || !ulong.TryParse(reader.GetValue(0)?.ToString(), out ulong idvar)) continue;
                                    var card = new CardEditor.Models.CardBanList
                                    {
                                        Id = idvar,
                                        Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                        LimitedCount = 3
                                    };
                                    Cards.Add(card);
                                }
                                catch (Exception ex)
                                {
                                    string cardId = reader.IsDBNull(0) ? "Unknown" : reader.GetValue(0).ToString();
                                    string logMessage = $"Conversion error occurred at ID = {cardId}. Exception: {ex.Message}";
                                    System.IO.File.AppendAllText(CardEditor.Models.AppContext.Instance.ErrorLogFilePath,
                                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {logMessage} {Environment.NewLine}");
                                }
                            }
                        }
                    }
                }
                return (Cards, string.Empty);
            }
            catch (SQLiteException ex)
            {
                return (null, $"{CMess.errorConDB.ToText()} {ex.Message}");
            }
            catch (Exception ex)
            {
                return (null, $"{CMess.errorOcc.ToText()} {ex.Message}");
            }
        }
        public static async Task<(List<CardEditor.Models.Card>, string message)> LoadClipboardCard()
        {
            if (!Clipboard.ContainsText()) return (null, CMess.noValiDataClip.ToText());

            string json = string.Empty;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                json = Clipboard.GetText();
            });
            List<CardEditor.Models.Card> importList = null;

            try
            {
                importList = await Task.Run(() => JsonSerializer.Deserialize<List<CardEditor.Models.Card>>(json, SerializerOptions));
                if (importList == null || importList.Count == 0)
                {
                    return (null, CMess.noValiDataFound.ToText());
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
            return (importList, string.Empty);
        }
        public static async Task<(List<CardEditor.Models.CardBanList>, string message)> LoadClipboardCardBanList()
        {
            if (!Clipboard.ContainsText()) return (null, CMess.noValiDataClip.ToText());

            string json = string.Empty;
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                json = Clipboard.GetText();
            });
            List<CardEditor.Models.CardBanList> importList = null;

            try
            {
                importList = await Task.Run(() => JsonSerializer.Deserialize<List<CardEditor.Models.CardBanList>>(json, SerializerOptions));
                if (importList == null || importList.Count == 0)
                {
                    return (null, CMess.noValiDataFound.ToText());
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
            return (importList, string.Empty);
        }
        public static async Task<(List<CardEditor.Models.Card>, string message)> LoadCedsCard(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return (null, CMess.fileNotExit.ToText());

            List<CardEditor.Models.Card> importedCards = null;

            try
            {
                const int bufferSize = 8192;
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);

                string json = await reader.ReadToEndAsync().ConfigureAwait(false);
                importedCards = JsonSerializer.Deserialize<List<CardEditor.Models.Card>>(json, SerializerOptions);
                if (importedCards == null || importedCards.Count == 0)
                {
                    return (null, CMess.noValiDataFound.ToText());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (null, ex.Message);

            }
            Debug.WriteLine($"Imported {importedCards.Count} cards from CEDS file.");
            return (importedCards, string.Empty);
        }
        public static async Task<(List<CardEditor.Models.CardBanList>, string message)> LoadCedsCardBanList(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return (null, CMess.fileNotExit.ToText());

            List<CardEditor.Models.CardBanList> importedCards = null;

            try
            {
                const int bufferSize = 8192;
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);

                string json = await reader.ReadToEndAsync().ConfigureAwait(false);
                importedCards = JsonSerializer.Deserialize<List<CardEditor.Models.CardBanList>>(json, SerializerOptions);
                if (importedCards == null || importedCards.Count == 0)
                {
                    return (null, CMess.noValiDataFound.ToText());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (null, ex.Message);

            }
            Debug.WriteLine($"Imported {importedCards.Count} cards from CEDS file.");
            return (importedCards, string.Empty);
        }
        public static async Task<(List<CardEditor.Models.Card>, string message)> LoadExcelCard(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return (null, CMess.fileNotExit.ToText());

            List<CardEditor.Models.Card> importedCards = new List<CardEditor.Models.Card>();
            try
            {
                const int bufferSize = 8192;
                int errorCount = 0;
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var memoryStream = new MemoryStream();
                await fs.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var package = new OfficeOpenXml.ExcelPackage(memoryStream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null || !CheckDatabase.CheckExcelValidity(worksheet))
                {
                    return (null, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()));
                }

                int lastRow = worksheet.Dimension?.End.Row ?? 1;
                if (lastRow < 2)
                {
                    return (null, CMess.noValiCardFound.ToText());
                }

                for (int row = 2; row <= lastRow; row++)
                {
                    try
                    {
                        var idCell = worksheet.Cells[row, 1].Value;
                        if (idCell == null) continue;

                        if (!ulong.TryParse(idCell.ToString(), out ulong id)) continue;
                        var card = new CardEditor.Models.Card
                        {
                            id = id,
                            
                            name = worksheet.Cells[row, 2].Text ?? "",
                            desc = worksheet.Cells[row, 3].Text ?? "",

                            ot = ulong.TryParse(worksheet.Cells[row, 4].Value?.ToString(), out ulong otvar) ? otvar : 0UL,
                            alias = ulong.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out ulong aliasvar) ? aliasvar : 0UL,
                            setcode = ulong.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out ulong setcodevar) ? setcodevar : 0UL,
                            type = ulong.TryParse(worksheet.Cells[row, 7].Value?.ToString(), out ulong typevar) ? typevar : 0UL,
                            atk = long.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out long atkvar) ? atkvar : 0L,
                            def = long.TryParse(worksheet.Cells[row, 9].Value?.ToString(), out long defvar) ? defvar : 0L,
                            level = ulong.TryParse(worksheet.Cells[row, 10].Value?.ToString(), out ulong levelvar) ? levelvar : 0UL,
                            race = ulong.TryParse(worksheet.Cells[ row, 11].Value?.ToString(), out ulong racevar) ? racevar : 0UL,
                            attribute = ulong.TryParse(worksheet.Cells[row, 12].Value?.ToString(), out ulong attributevar) ? attributevar : 0UL,
                            category = ulong.TryParse(worksheet.Cells[row, 13].Value?.ToString(), out ulong categoryvar) ? categoryvar : 0UL,

                            str1 = worksheet.Cells[row, 14].Text ?? "",
                            str2 = worksheet.Cells[row, 15].Text ?? "",
                            str3 = worksheet.Cells[row, 16].Text ?? "",
                            str4 = worksheet.Cells[row, 17].Text ?? "",
                            str5 = worksheet.Cells[row, 18].Text ?? "",
                            str6 = worksheet.Cells[row, 19].Text ?? "",
                            str7 = worksheet.Cells[row, 20].Text ?? "",
                            str8 = worksheet.Cells[row, 21].Text ?? "",
                            str9 = worksheet.Cells[row, 22].Text ?? "",
                            str10 = worksheet.Cells[row, 23].Text ?? "",
                            str11 = worksheet.Cells[row, 24].Text ?? "",
                            str12 = worksheet.Cells[row, 25].Text ?? "",
                            str13 = worksheet.Cells[row, 26].Text ?? "",
                            str14 = worksheet.Cells[row, 27].Text ?? "",
                            str15 = worksheet.Cells[row, 28].Text ?? "",
                            str16 = worksheet.Cells[row, 29].Text ?? ""
                        };
                        importedCards.Add(card);
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        File.AppendAllText(CardEditor.Models.AppContext.Instance.ErrorLogFilePath,
                            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Row: {row} {ex.Message}{Environment.NewLine}");
                        continue;
                    }
                }
                if (importedCards.Count == 0)
                {
                    return (null, CMess.noValiDataFound.ToText());
                }
                return (importedCards, errorCount.ToString());
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
        public static async Task<(List<CardEditor.Models.CardBanList>, string message)> LoadExcelCardBanList(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return (null, CMess.fileNotExit.ToText());

            List<CardEditor.Models.CardBanList> importedCards = new List<CardEditor.Models.CardBanList>();
            try
            {
                const int bufferSize = 8192;
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var memoryStream = new MemoryStream();
                await fs.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var package = new OfficeOpenXml.ExcelPackage(memoryStream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null || !CheckDatabase.CheckExcelValidity(worksheet))
                {
                    return (null, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()));
                }

                int lastRow = worksheet.Dimension?.End.Row ?? 1;
                if (lastRow < 2)
                {
                    return (null, CMess.noValiCardFound.ToText());
                }

                for (int row = 2; row <= lastRow; row++)
                {
                    var idCell = worksheet.Cells[row, 1].Value;
                    if (idCell == null) continue;

                    if (!ulong.TryParse(idCell.ToString(), out ulong id)) continue;
                    var card = new CardEditor.Models.CardBanList
                    {
                        Id = id,
                        Name = worksheet.Cells[row, 2].Text ?? "",
                        LimitedCount = 3
                    };
                    importedCards.Add(card);
                }
                if (importedCards.Count == 0)
                {
                    return (null, CMess.noValiDataFound.ToText());
                }
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
            return (importedCards, string.Empty);
        }


        /// <summary>
        /// Liệt kê các entry trong archive theo từng loại (DB, Script, Deck, Banlist).
        /// Chỉ trả về FullName (giữ cấu trúc thư mục gốc trong zip), không extract, không tạo temp.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="getListDB"></param>
        /// <param name="getListScript"></param>
        /// <param name="getListDeck"></param>
        /// <param name="getListBanlist"></param>
        /// <returns></returns>
        public static Task<(bool success, ArchiveFileList fileLists, string message)> ListsEntries(string archivePath,
            bool getListDB = true, bool getListScript = true, bool getListDeck = true, bool getListBanlist = true)
        {
            if (string.IsNullOrEmpty(archivePath) || !System.IO.File.Exists(archivePath))
                return Task.FromResult((false, (ArchiveFileList)null, CMess.fileNotExit.ToText()));

            return Task.Run(() =>
            {
                ZipArchive archive;
                try
                {
                    archive = ZipFile.OpenRead(archivePath);
                }
                catch (Exception ex)
                {
                    // Không mở được như một archive hợp lệ
                    return (false, (ArchiveFileList)null,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.File.ToText())} {ex.Message}");
                }

                using (archive)
                {
                    var fileList = new ArchiveFileList();

                    foreach (var entry in archive.Entries)
                    {
                        // entry.Name rỗng nghĩa là entry này là thư mục (directory entry), bỏ qua
                        if (string.IsNullOrEmpty(entry.Name)) continue;

                        string ext = Path.GetExtension(entry.Name);

                        if (getListBanlist && entry.Name.EndsWith(BanlistSuffix, StringComparison.OrdinalIgnoreCase))
                        {
                            fileList.BanlistEntries.Add(entry.FullName);
                            continue;
                        }
                        if (getListDB && KnownDbExtensions.Contains(ext))
                        {
                            fileList.DatabaseEntries.Add(entry.FullName);
                            continue;
                        }
                        if (getListScript && ext.Equals(ScriptExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            fileList.ScriptEntries.Add(entry.FullName);
                            continue;
                        }
                        if (getListDeck && ext.Equals(DeckExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            fileList.DeckEntries.Add(entry.FullName);
                            continue;
                        }
                    }

                    bool anyFound = fileList.DatabaseEntries.Count > 0
                        || fileList.ScriptEntries.Count > 0
                        || fileList.DeckEntries.Count > 0
                        || fileList.BanlistEntries.Count > 0;

                    string message = anyFound ? string.Empty : CMess.noValiDataFound.ToText();
                    return (true, fileList, message);
                }
            });
        }

        /// <summary>
        /// Lấy ra danh sách Fullname của các entry trong archive theo entryName.
        /// Tùy chọn lọc theo extension.
        /// </summary>
        /// <param name="archiveFilePath"></param>
        /// <param name="entryName"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static Task<(bool success, IEnumerable<string> fileLists, string message)>
            FindEntriesByName(string archiveFilePath, string entryName, string extension = null)
        {
            if (string.IsNullOrWhiteSpace(archiveFilePath) || !File.Exists(archiveFilePath))
                return Task.FromResult((false, (IEnumerable<string>)null, CMess.fileNotExit.ToText()));

            if (string.IsNullOrWhiteSpace(entryName))
                return Task.FromResult((false, (IEnumerable<string>)null, CMess.fileNotExit.ToText()));

            if (!string.IsNullOrWhiteSpace(extension))
                if (!extension.StartsWith(".")) extension = "." + extension;

            try
            {
                using var archive = ZipFile.OpenRead(archiveFilePath);

                var fileLists = archive.Entries.Where(e =>
                {
                    // Bỏ qua folder
                    if (string.IsNullOrEmpty(e.Name)) return false;

                    // Bước 1: lọc extension nếu có
                    if (!string.IsNullOrWhiteSpace(extension))
                    {
                        var ext = Path.GetExtension(e.Name);

                        if (!ext.Equals(extension, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                    // Bước 2: tìm kiếm chứa trong toàn bộ tên file

                    return e.Name.IndexOf(entryName, StringComparison.OrdinalIgnoreCase) >= 0;
                }).Select(e => e.FullName).ToList();

                return Task.FromResult((true, (IEnumerable<string>)fileLists, string.Empty));
            }
            catch (Exception ex)
            {
                return Task.FromResult((false, (IEnumerable<string>)null, ex.Message));
            }
        }

        /// <summary>
        /// Trích xuất các entry được chỉ định (theo FullName) ra thư mục Temp, giữ nguyên cấu trúc thư mục con.
        /// Entry nào không xử lý được (không tồn tại, hoặc path không hợp lệ) sẽ được liệt kê trong FailedEntries.
        /// Các entry còn lại vẫn được xử lý bình thường. Việc dọn dẹp Temp do Caller tự quản lý.
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="entryFullNames"></param>
        /// <returns></returns>
        public static Task<(bool result, List<ExtractedEntry> extractedEntries, List<string> failedEntries, string message)> ExtractTempEntry
            (string archivePath, IEnumerable<string> entryFullNames)
        {
            if (string.IsNullOrWhiteSpace(archivePath) || !System.IO.File.Exists(archivePath))
                return Task.FromResult((false, (List<ExtractedEntry>)null, (List<string>)null, CMess.fileNotExit.ToText()));
            if (entryFullNames == null)
                return Task.FromResult((false, (List<ExtractedEntry>)null, (List<string>)null, CMess.fileNotExit.ToText()));

            return Task.Run(() =>
            {
                ZipArchive archive;
                try
                {
                    archive = ZipFile.OpenRead(archivePath);
                }
                catch (Exception ex)
                {
                    return (false, (List<ExtractedEntry>)null, (List<string>)null,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.File.ToText())} {ex.Message}");
                }

                using (archive)
                {
                    string tempDir = Path.Combine(Path.GetTempPath(), "CardEditorTemp", Path.GetFileNameWithoutExtension(archivePath));
                    Directory.CreateDirectory(tempDir);

                    // GetFullPath để chuẩn hoá path, dùng làm mốc so sánh chống Zip Slip
                    string tempDirFull = Path.GetFullPath(tempDir);

                    var extractedEntries = new List<ExtractedEntry>();
                    var failedEntries = new List<string>();

                    foreach (var entryFullName in entryFullNames)
                    {
                        var entry = archive.GetEntry(entryFullName);
                        if (entry == null)
                        {
                            failedEntries.Add(entryFullName);
                            continue;
                        }

                        // Giữ cấu trúc thư mục con theo FullName trong zip
                        string relativePath = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                        string destPath = Path.GetFullPath(Path.Combine(tempDirFull, relativePath));

                        // Chống Zip Slip: đảm bảo destPath thực sự nằm trong tempDir, không thoát ra ngoài qua "../"
                        if (!destPath.StartsWith(tempDirFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        {
                            failedEntries.Add(entryFullName);
                            continue;
                        }

                        try
                        {
                            string directory = Path.GetDirectoryName(destPath);
                            if (!string.IsNullOrEmpty(directory))
                                Directory.CreateDirectory(directory);
                            entry.ExtractToFile(destPath, overwrite: true);
                            extractedEntries.Add(new ExtractedEntry
                            {
                                EntryFullName = entry.FullName,
                                TempPath = destPath
                            });
                        }
                        catch
                        {
                            failedEntries.Add(entryFullName);
                        }
                    }

                    // Không tự ý gán message lỗi cụ thể vì không rõ CMess đã có key phù hợp chưa;
                    // Caller có thể tự kiểm tra failedEntries.Count > 0 để xử lý thông báo riêng.
                    return (true, extractedEntries, failedEntries, string.Empty);
                }
            });
        }

        /// <summary>
        /// Ghi các file đã chỉnh sửa trong Temp ngược lại vào archive gốc (ghi đè trực tiếp archivePath),
        /// theo cơ chế an toàn: làm việc trên 1 bản copy tạm, chỉ thay thế archive gốc khi MỌI THỨ đã thành công.
        /// Nếu có lỗi ở bất kỳ bước nào trước khi thay thế, archive gốc không hề bị đụng tới.
        /// Không hỗ trợ xoá Entry.
        /// </summary>
        /// <param name="entries">Danh sách (entryFullName, tempPath) cần ghi ngược vào archive.</param>
        /// <param name="createEntryIfMissing">
        /// Nếu entryFullName chưa tồn tại trong archive: true = tạo entry mới, false = coi là lỗi (thêm vào failedEntries).
        /// </param>
        public static Task<(bool result, List<string> failedEntries, string message)> SaveTempEntriesToArchive(
            string archivePath, List<(string entryFullName, string tempPath)> entries, bool createEntryIfMissing = true)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(archivePath))
                {
                    return (false, (List<string>)null, $"{string.Format(CMess.fileNotExit.ToText())} {archivePath}");
                }

                // Bản copy tạm PHẢI nằm CÙNG thư mục với archive gốc (cùng ổ đĩa),
                // vì File.Replace yêu cầu source/destination/backup cùng volume mới hoạt động đúng.
                string workingCopyPath = archivePath + $".{Guid.NewGuid():N}.tmp";
                File.Copy(archivePath, workingCopyPath, overwrite: true);

                var failedEntries = new List<string>();

                try
                {
                    using (var archive = ZipFile.Open(workingCopyPath, ZipArchiveMode.Update))
                    {
                        foreach (var (entryFullName, tempPath) in entries)
                        {
                            if (!File.Exists(tempPath))
                            {
                                failedEntries.Add(entryFullName);
                                continue;
                            }

                            var existingEntry = archive.GetEntry(entryFullName);
                            if (existingEntry == null && !createEntryIfMissing)
                            {
                                failedEntries.Add(entryFullName);
                                continue;
                            }

                            // Cách chuẩn để "ghi đè" nội dung 1 entry trong ZipArchiveMode.Update:
                            // xoá entry cũ (nếu có) rồi tạo lại entry mới từ file trong Temp.
                            existingEntry?.Delete();
                            archive.CreateEntryFromFile(tempPath, entryFullName, CompressionLevel.Optimal);
                        }
                    } // Dispose tại đây mới thực sự ghi mọi thay đổi xuống workingCopyPath
                }
                catch (Exception ex)
                {
                    // Ghi thất bại -> workingCopyPath có thể hỏng, xoá bỏ, KHÔNG đụng tới archive gốc
                    TryDeleteFile(workingCopyPath);
                    return (false, (List<string>)null,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.File.ToText())} {ex.Message}");
                }

                try
                {
                    // Thay thế archive gốc bằng bản đã cập nhật - gần như atomic, an toàn nếu crash giữa chừng
                    string backupPath = archivePath + ".bak";
                    File.Replace(workingCopyPath, archivePath, backupPath);
                    TryDeleteFile(backupPath);
                }
                catch (Exception ex)
                {
                    TryDeleteFile(workingCopyPath);
                    return (false, failedEntries,
                        $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.File.ToText())} {ex.Message}");
                }

                // Caller tự kiểm tra failedEntries.Count > 0 để biết có entry nào bị bỏ qua không.
                return (true, failedEntries, string.Empty);
            });
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // Bỏ qua lỗi khi dọn file tạm, không ảnh hưởng tới kết quả chính
            }
        }
        /// <summary>
        /// Tiện ích cho trường hợp chỉ cần lưu 1 entry — tái sử dụng cơ chế an toàn của SaveTempEntriesToArchive
        /// (copy tạm, chỉ thay thế archive gốc khi chắc chắn thành công) thay vì thao tác trực tiếp lên archivePath.
        /// </summary>
        public static async Task<(bool success, string message)> SaveEntryToZip(string archivePath, string entryFullName, string tempFilePath)
        {
            var (result, failedEntries, message) = await SaveTempEntriesToArchive(
                archivePath,
                new List<(string entryFullName, string tempPath)> { (entryFullName, tempFilePath) });

            if (!result) return (false, message);
            if (failedEntries.Count > 0) return (false, $"Không thể lưu entry: {entryFullName}");
            return (true, string.Empty);
        }



        public static (bool, List<(string tempPath, string entryName)>, string) ExtractDatabaseEntries(string archivePath)
        {
            ZipArchive archive;
            try
            {
                archive = ZipFile.OpenRead(archivePath);
            }
            catch (Exception ex)
            {
                // Không mở được như một archive hợp lệ
                return (false, null, $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.File.ToText())} {ex.Message}");
            }

            using (archive)
            {
                // Lọc ra những entry có đuôi nằm trong KnownDbExtensions
                var matchedEntries = archive.Entries
                    .Where(e => !string.IsNullOrEmpty(e.Name) && KnownDbExtensions.Contains(Path.GetExtension(e.Name)))
                    .ToList();

                // Archive mở được, nhưng bên trong không có file nào khớp -> archiveOk = true, list rỗng
                if (matchedEntries.Count == 0)
                    return (true, new List<(string, string)>(), CMess.noValiDataFound.ToText());

                string tempDir = Path.Combine(Path.GetTempPath(), "CardEditorTemp", Path.GetFileNameWithoutExtension(archivePath));
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                // Đổi kiểu list để lưu CẢ đường dẫn tạm LẪN tên entry gốc
                var result = new List<(string tempPath, string entryName)>();
                foreach (var entry in matchedEntries)
                {
                    string destPath = Path.Combine(tempDir, entry.Name);
                    entry.ExtractToFile(destPath, overwrite: true);
                    result.Add((destPath, entry.FullName)); // lưu cặp (file tạm, tên gốc trong archive)
                }

                return (true, result, string.Empty);
            }
        }
        public static (bool, List<(string virtualPath, string entryName)>, string) ListDatabaseEntries(string archivePath)
        {
            ZipArchive archive;
            try
            {
                archive = ZipFile.OpenRead(archivePath);
            }
            catch (Exception ex)
            {
                return (false, null, $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Open.ToText(), CMess.File.ToText())} {ex.Message}");
            }

            using (archive)
            {
                var matchedEntries = archive.Entries
                    .Where(e => !string.IsNullOrEmpty(e.Name) && KnownDbExtensions.Contains(Path.GetExtension(e.Name)))
                    .ToList();

                if (matchedEntries.Count == 0)
                    return (true, new List<(string, string)>(), CMess.noValiDataFound.ToText());

                // Virtual root: cùng thư mục với archive, tên = tên archive không đuôi
                string virtualRoot = Path.Combine(
                    Path.GetDirectoryName(archivePath),
                    Path.GetFileNameWithoutExtension(archivePath));

                var result = matchedEntries.Select(entry =>
                {
                    // entry.FullName trong zip dùng '/' -> đổi sang '\' cho đúng Windows path
                    string relativePath = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                    string virtualPath = Path.Combine(virtualRoot, relativePath);
                    return (virtualPath, entry.FullName);
                }).ToList();

                return (true, result, string.Empty);
            }
        }
        public static List<(string tempPath, string entryName)> ExtractSelectedEntries(string archivePath, List<string> entryNames)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "CardEditorTemp",
                Path.GetFileNameWithoutExtension(archivePath));

            Directory.CreateDirectory(tempDir); // không xóa tempDir cũ nữa, vì giờ trích theo yêu cầu, không trích hết 1 lần

            var result = new List<(string, string)>();
            using (var archive = ZipFile.OpenRead(archivePath))
            {
                foreach (var entryName in entryNames)
                {
                    var entry = archive.GetEntry(entryName);
                    if (entry == null) continue;

                    string destPath = Path.Combine(tempDir, entry.Name); // hoặc giữ cấu trúc thư mục nếu cần
                    entry.ExtractToFile(destPath, overwrite: true);
                    result.Add((destPath, entry.FullName));
                }
            }
            return result;
        }
    }
}
