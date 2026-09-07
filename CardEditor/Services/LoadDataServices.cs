using System;
using System.IO;
using System.IO.Compression;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Windows;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using ClosedXML.Excel;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Converter;
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

        public static Task<LoadScriptPathsResult> LoadAllScriptPaths()
        {
            return Task.Run(() =>
            {
                var result = new LoadScriptPathsResult();

                string dataSourcePath = CardEditor.ViewModels.ConfigViewModel.Instance.userSetting.DataSource;
                if (!Directory.Exists(dataSourcePath))
                {
                    result.Result = false;
                    result.Message =
                        $"{CMess.dataSourceErr.ToText()} {dataSourcePath}";

                    return result;
                }

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
                    result.Result = true;
                    result.Paths = new Dictionary<ulong, List<string>>(concurrentDict);

                    return result;
                }
                catch (Exception ex)
                {
                    result.Result = false;
                    result.Message = ex.Message;

                    return result;
                }
            });
        }

        public static async Task<LoadAllCdbFilesResult> LoadAllDatabaseFiles()
        {
            var result = new LoadAllCdbFilesResult();

            string dataSourcePath = CardEditor.ViewModels.ConfigViewModel.Instance.userSetting.DataSource;
            if (!Directory.Exists(dataSourcePath))
            {
                result.Result = false;
                result.Message = $"{CMess.dataSourceErr.ToText()} {dataSourcePath}";
                return result;
            }

            var allCards = new List<CardEditor.Models.Card>(25000);
            var cardPaths = new Dictionary<ulong, List<string>>(25000);

            try
            {
                var databaseFiles = Directory
                    .EnumerateFiles(dataSourcePath, "*.cdb", SearchOption.AllDirectories)
                    .Concat(Directory.EnumerateFiles(dataSourcePath, "*.db", SearchOption.AllDirectories))
                    .Concat(Directory.EnumerateFiles(dataSourcePath, "*.bytes", SearchOption.AllDirectories))
                    .Concat(Directory.EnumerateFiles(dataSourcePath, "*.sqlite", SearchOption.AllDirectories));
                foreach (var databaseFile in databaseFiles)
                {
                    try
                    {
                        var loadResult = await LoadDatabaseCard(databaseFile);

                        if (!loadResult.Result)
                        {
                            Debug.WriteLine($"Error loading {databaseFile}: {loadResult.Message}");
                            continue;
                        }

                        IEnumerable<Card> cardsToAdd;

                        switch (loadResult.DBType)
                        {
                            case DatabaseType.YGO:
                                cardsToAdd = loadResult.CardList;
                                break;

                            case DatabaseType.OMEGA:
                                var convertedCards = new List<Card>();
                                foreach (var omegaCard in loadResult.OmegaCardList)
                                {
                                    try
                                    {
                                        convertedCards.Add(CardConverter.ToCard(omegaCard));
                                    }
                                    catch (Exception)
                                    {
                                        ///
                                    }
                                }
                                cardsToAdd = convertedCards;
                                break;

                            default:
                                Debug.WriteLine($"Unsupported database type: {databaseFile}");
                                continue;
                        }

                        foreach (var card in cardsToAdd)
                        {
                            if (card == null || card.id == 0)
                                continue;

                            if (!cardPaths.TryGetValue(card.id, out var list))
                            {
                                list = new List<string>(1);
                                cardPaths[card.id] = list;
                                allCards.Add(card);
                            }

                            list.Add(databaseFile);
                        }




                        //if (loadResult.CardList.Count == 0) continue;

                        //foreach (var card in loadResult.CardList)
                        //{
                        //    if (card == null || card.id == 0)
                        //        continue;

                        //    if (!cardPaths.TryGetValue(card.id, out var list))
                        //    {
                        //        list = new List<string>(1);
                        //        cardPaths[card.id] = list;

                        //        allCards.Add(card);
                        //    }

                        //    list.Add(databaseFile);
                        //}
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading {databaseFile}: {ex.Message}");
                    }
                }
                result.Result = true;
                result.CardList = allCards;
                result.CardPaths = cardPaths;

                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;

                return result;
            }
        }

        public static async Task<LoadCardDataResult> LoadDatabaseCard(string filePath)
        {
            var result = new LoadCardDataResult();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();
                return result;
            }

            string connectionString = $"Data Source={filePath};Version=3;";
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    CheckDatabaseResult checkResult = CheckDatabase.CheckDatabaseValidity(connection);
                    if (!checkResult.Result)
                    {
                        result.Result = false;
                        result.Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText());
                        return result;
                    }

                    result.DBType = checkResult.DBType;
                    result.HasFlag = checkResult.HasFlag;

                    switch (checkResult.DBType)
                    {
                        case DatabaseType.YGO:
                            await LoadYGODatabaseCard(connection, result);
                            break;
                        case DatabaseType.OMEGA:
                            await LoadOMEGADatabaseCard(connection, result);
                            break;
                        default:
                            result.Result = false;
                            result.Message = $"Unsupported database type: {checkResult.DBType}";
                            return result;
                    }
                }
                result.Result = true;
                return result;
            }
            catch (SQLiteException ex)
            {
                result.Result = false;
                result.Message = $"{CMess.errorConDB.ToText()} {ex.Message}";
                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = $"{CMess.errorOcc.ToText()} {ex.Message}";
                return result;
            }
        }
        private static async Task LoadYGODatabaseCard(SQLiteConnection connection, LoadCardDataResult result)
        {
            string flagSelect = result.HasFlag ? "datas.flag" : "0 AS flag";

            string query = $@"
                    SELECT
                        texts.id, texts.name, texts.desc, texts.str1, texts.str2, texts.str3, texts.str4, texts.str5,
                        texts.str6, texts.str7, texts.str8, texts.str9, texts.str10, texts.str11, texts.str12,
                        texts.str13, texts.str14, texts.str15, texts.str16,
                        datas.ot, datas.alias, datas.setcode, datas.type, datas.atk, datas.def, datas.level,
                        datas.race, datas.attribute, datas.category, {flagSelect}
                    FROM texts
                    INNER JOIN datas ON texts.id = datas.id";

            using (SQLiteCommand command = new SQLiteCommand(query, connection))
            {
                using (SQLiteDataReader reader = await command.ExecuteReaderAsync() as SQLiteDataReader)
                {
                    int flagOrdinal = reader.GetOrdinal("flag");

                    while (await reader.ReadAsync())
                    {
                        try
                        {
                            if (reader.IsDBNull(0) || !ulong.TryParse(reader.GetValue(0)?.ToString(), out ulong idvar)) continue;
                            var card = new CardEditor.Models.Card
                            {
                                id = idvar,

                                name = ReadDatabaseHelper.ReadString(reader, 1),
                                desc = ReadDatabaseHelper.ReadString(reader, 2),
                                str1 = ReadDatabaseHelper.ReadString(reader, 3),
                                str2 = ReadDatabaseHelper.ReadString(reader, 4),
                                str3 = ReadDatabaseHelper.ReadString(reader, 5),
                                str4 = ReadDatabaseHelper.ReadString(reader, 6),
                                str5 = ReadDatabaseHelper.ReadString(reader, 7),
                                str6 = ReadDatabaseHelper.ReadString(reader, 8),
                                str7 = ReadDatabaseHelper.ReadString(reader, 9),
                                str8 = ReadDatabaseHelper.ReadString(reader, 10),
                                str9 = ReadDatabaseHelper.ReadString(reader, 11),
                                str10 = ReadDatabaseHelper.ReadString(reader, 12),
                                str11 = ReadDatabaseHelper.ReadString(reader, 13),
                                str12 = ReadDatabaseHelper.ReadString(reader, 14),
                                str13 = ReadDatabaseHelper.ReadString(reader, 15),
                                str14 = ReadDatabaseHelper.ReadString(reader, 16),
                                str15 = ReadDatabaseHelper.ReadString(reader, 17),
                                str16 = ReadDatabaseHelper.ReadString(reader, 18),

                                ot = ReadDatabaseHelper.ReadUInt64(reader, 19),
                                alias = ReadDatabaseHelper.ReadUInt64(reader, 20),
                                setcode = ReadDatabaseHelper.ReadUInt64(reader, 21),
                                type = ReadDatabaseHelper.ReadUInt64(reader, 22),
                                atk = ReadDatabaseHelper.ReadInt64(reader, 23),
                                def = ReadDatabaseHelper.ReadInt64(reader, 24),
                                level = ReadDatabaseHelper.ReadUInt64(reader, 25),
                                race = ReadDatabaseHelper.ReadUInt64(reader, 26),
                                attribute = ReadDatabaseHelper.ReadUInt64(reader, 27),
                                category = ReadDatabaseHelper.ReadUInt64(reader, 28),
                                flag = reader.IsDBNull(flagOrdinal) ? 0UL : (ulong.TryParse(reader.GetValue(flagOrdinal)?.ToString(), out ulong flagvar) ? flagvar : 0UL)
                            };
                            result.CardList.Add(card);
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
        private static async Task LoadOMEGADatabaseCard(SQLiteConnection connection, LoadCardDataResult result)
        {
            string query = $@"
                    SELECT
                        texts.id, texts.name, texts.desc, texts.str1, texts.str2, texts.str3, texts.str4, texts.str5,
                        texts.str6, texts.str7, texts.str8, texts.str9, texts.str10, texts.str11, texts.str12,
                        texts.str13, texts.str14, texts.str15, texts.str16,
                        datas.ot, datas.alias, datas.setcode, datas.type, datas.atk, datas.def, datas.level,
                        datas.race, datas.attribute, datas.category, datas.genre, datas.support
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

                            var cardOmega = new CardEditor.Models.CardOmega
                            {
                                id = idvar,

                                name = ReadDatabaseHelper.ReadString(reader, 1),
                                desc = ReadDatabaseHelper.ReadString(reader, 2),
                                str1 = ReadDatabaseHelper.ReadString(reader, 3),
                                str2 = ReadDatabaseHelper.ReadString(reader, 4),
                                str3 = ReadDatabaseHelper.ReadString(reader, 5),
                                str4 = ReadDatabaseHelper.ReadString(reader, 6),
                                str5 = ReadDatabaseHelper.ReadString(reader, 7),
                                str6 = ReadDatabaseHelper.ReadString(reader, 8),
                                str7 = ReadDatabaseHelper.ReadString(reader, 9),
                                str8 = ReadDatabaseHelper.ReadString(reader, 10),
                                str9 = ReadDatabaseHelper.ReadString(reader, 11),
                                str10 = ReadDatabaseHelper.ReadString(reader, 12),
                                str11 = ReadDatabaseHelper.ReadString(reader, 13),
                                str12 = ReadDatabaseHelper.ReadString(reader, 14),
                                str13 = ReadDatabaseHelper.ReadString(reader, 15),
                                str14 = ReadDatabaseHelper.ReadString(reader, 16),
                                str15 = ReadDatabaseHelper.ReadString(reader, 17),
                                str16 = ReadDatabaseHelper.ReadString(reader, 18),

                                ot = ReadDatabaseHelper.ReadUInt64(reader, 19),
                                alias = ReadDatabaseHelper.ReadUInt64(reader, 20),
                                setcode = ReadDatabaseHelper.ReadBlob(reader, 21),
                                type = ReadDatabaseHelper.ReadUInt64(reader, 22),
                                atk = ReadDatabaseHelper.ReadInt64(reader, 23),
                                def = ReadDatabaseHelper.ReadInt64(reader, 24),
                                level = ReadDatabaseHelper.ReadUInt64(reader, 25),
                                race = ReadDatabaseHelper.ReadUInt64(reader, 26),
                                attribute = ReadDatabaseHelper.ReadUInt64(reader, 27),
                                // Omega category = Flags
                                category = ReadDatabaseHelper.ReadUInt64(reader, 28),
                                // Omega genre = Effect category
                                genre = ReadDatabaseHelper.ReadUInt64(reader, 29),
                                // BLOB
                                support = ReadDatabaseHelper.ReadBlob(reader, 30),

                                //script = ReadDatabaseHelper.ReadBlob(reader, 30, allowText: true),
                                //support = ReadDatabaseHelper.ReadBlob(reader, 31)
                            };
                            result.OmegaCardList.Add(cardOmega);
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

        public static async Task<LoadCardDataResult> LoadClipboardCard()
        {
            if (!Clipboard.ContainsText())
            {
                return new LoadCardDataResult
                {
                    Result = false,
                    Message = CMess.noValiDataClip.ToText()
                };
            }

            try
            {
                string json = string.Empty;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    json = Clipboard.GetText();
                });

                var importList = JsonSerializer.Deserialize<List<CardEditor.Models.Card>>(json, SerializerOptions);

                if (importList == null || importList.Count == 0)
                {
                    return new LoadCardDataResult
                    {
                        Result = false,
                        Message = CMess.noValiDataFound.ToText()
                    };
                }

                return new LoadCardDataResult
                {
                    Result = true,
                    CardList = importList
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                return new LoadCardDataResult
                {
                    Result = false,
                    Message = ex.Message
                };
            }
        }
        public static async Task<LoadCardDataResult> LoadExcelCard(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return new LoadCardDataResult { Result = false, CardList = null, HasFlag = false, Message = CMess.fileNotExit.ToText() };

            try
            {
                const int bufferSize = 8192;
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var memoryStream = new MemoryStream();
                await fs.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var workbook = new XLWorkbook(memoryStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();

                var (isValid, hasFlagColumn) = CheckDatabase.CheckExcelValidity(worksheet);
                if (!isValid)
                {
                    return new LoadCardDataResult
                    {
                        Result = false,
                        CardList = null,
                        HasFlag = false,
                        Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText())
                    };
                }

                int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

                var importedCards = new List<Card>();
                int errorCount = 0;
                int strOffset = hasFlagColumn ? 14 : 13;

                for (int row = 2; row <= lastRow; row++)
                {
                    try
                    {
                        var idCell = worksheet.Cell(row, 1);
                        if (idCell.IsEmpty()) continue;
                        if (!ulong.TryParse(idCell.GetString(), out ulong id)) continue;

                        var card = new Card
                        {
                            id = id,

                            name = worksheet.Cell(row, 2).GetString(),
                            desc = worksheet.Cell(row, 3).GetString(),

                            ot = ulong.TryParse(worksheet.Cell(row, 4).GetString(), out ulong otvar) ? otvar : 0UL,
                            alias = ulong.TryParse(worksheet.Cell(row, 5).GetString(), out ulong aliasvar) ? aliasvar : 0UL,
                            setcode = ulong.TryParse(worksheet.Cell(row, 6).GetString(), out ulong setcodevar) ? setcodevar : 0UL,
                            type = ulong.TryParse(worksheet.Cell(row, 7).GetString(), out ulong typevar) ? typevar : 0UL,
                            atk = long.TryParse(worksheet.Cell(row, 8).GetString(), out long atkvar) ? atkvar : 0L,
                            def = long.TryParse(worksheet.Cell(row, 9).GetString(), out long defvar) ? defvar : 0L,
                            level = ulong.TryParse(worksheet.Cell(row, 10).GetString(), out ulong levelvar) ? levelvar : 0UL,
                            race = ulong.TryParse(worksheet.Cell(row, 11).GetString(), out ulong racevar) ? racevar : 0UL,
                            attribute = ulong.TryParse(worksheet.Cell(row, 12).GetString(), out ulong attributevar) ? attributevar : 0UL,
                            category = ulong.TryParse(worksheet.Cell(row, 13).GetString(), out ulong categoryvar) ? categoryvar : 0UL,
                            flag = hasFlagColumn && ulong.TryParse(worksheet.Cell(row, 14).GetString(), out ulong flagvar) ? flagvar : 0UL,

                            str1 = worksheet.Cell(row, strOffset + 1).GetString(),
                            str2 = worksheet.Cell(row, strOffset + 2).GetString(),
                            str3 = worksheet.Cell(row, strOffset + 3).GetString(),
                            str4 = worksheet.Cell(row, strOffset + 4).GetString(),
                            str5 = worksheet.Cell(row, strOffset + 5).GetString(),
                            str6 = worksheet.Cell(row, strOffset + 6).GetString(),
                            str7 = worksheet.Cell(row, strOffset + 7).GetString(),
                            str8 = worksheet.Cell(row, strOffset + 8).GetString(),
                            str9 = worksheet.Cell(row, strOffset + 9).GetString(),
                            str10 = worksheet.Cell(row, strOffset + 10).GetString(),
                            str11 = worksheet.Cell(row, strOffset + 11).GetString(),
                            str12 = worksheet.Cell(row, strOffset + 12).GetString(),
                            str13 = worksheet.Cell(row, strOffset + 13).GetString(),
                            str14 = worksheet.Cell(row, strOffset + 14).GetString(),
                            str15 = worksheet.Cell(row, strOffset + 15).GetString(),
                            str16 = worksheet.Cell(row, strOffset + 16).GetString()
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

                return new LoadCardDataResult
                {
                    Result = true,
                    CardList = importedCards,
                    HasFlag = hasFlagColumn,
                    Message = errorCount.ToString()
                };
            }
            catch (Exception ex)
            {
                return new LoadCardDataResult { Result = false, CardList = null, HasFlag = false, Message = ex.Message };
            }
        }
        public static async Task<LoadCardDataResult> LoadCedsCard(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return new LoadCardDataResult
                {
                    Result = false,
                    CardList = null,
                    HasFlag = false,
                    Message = CMess.fileNotExit.ToText()
                };
            }

            try
            {
                const int bufferSize = 8192;

                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);

                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true,
                    bufferSize: bufferSize, leaveOpen: false);

                string json = await reader.ReadToEndAsync().ConfigureAwait(false);

                var importedCards = JsonSerializer.Deserialize<List<CardEditor.Models.Card>>(json, SerializerOptions);
                importedCards ??= new List<CardEditor.Models.Card>();

                Debug.WriteLine($"Imported {importedCards.Count} cards from CEDS file.");

                return new LoadCardDataResult
                {
                    Result = true,
                    CardList = importedCards,
                    HasFlag = false,
                    Message = string.Empty
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                return new LoadCardDataResult
                {
                    Result = false,
                    CardList = null,
                    HasFlag = false,
                    Message = ex.Message
                };
            }
        }
        private static async Task<bool> ColumnExistsAsync(SQLiteConnection connection, string table, string column)
        {
            string sql = $"PRAGMA table_info({table})";
            using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
            using (SQLiteDataReader reader = await cmd.ExecuteReaderAsync() as SQLiteDataReader)
            {
                while (await reader.ReadAsync())
                {
                    // PRAGMA table_info trả về: cid, name, type, notnull, dflt_value, pk
                    string colName = reader.GetString(1);
                    if (string.Equals(colName, column, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        public static async Task<LoadBanListResult> LoadFileBanList(string filePath)
        {
            var result = new LoadBanListResult();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();
                return result;
            }

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

                result.Result = true;
                result.BanList = banList;
                result.Message = string.Empty;

                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;

                return result;
            }
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
        public static async Task<LoadCardBanDataResult> LoadDatabaseCardBanList(string filePath)
        {
            var result = new LoadCardBanDataResult();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();
                return result;
            }

            string connectionString = $"Data Source={filePath};Version=3;";
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    bool resultCheck = CheckDatabase.CheckDatabaseListNameValidity(connection);
                    if (!resultCheck)
                    {
                        result.Result = false;
                        result.Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText());
                        return result;
                    }

                    string query = @"SELECT id, name FROM texts";
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
                                    result.CardList.Add(card);
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
                result.Result = true;
                result.Message = string.Empty;
                return result;
            }
            catch (SQLiteException ex)
            {
                result.Result = false;
                result.Message = $"{CMess.errorConDB.ToText()} {ex.Message}";
                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = $"{CMess.errorOcc.ToText()} {ex.Message}";
                return result;
            }
        }
        public static async Task<LoadCardBanDataResult> LoadExcelCardBanList(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return new LoadCardBanDataResult
                {
                    Result = false,
                    Message = CMess.fileNotExit.ToText()
                };

            try
            {
                const int bufferSize = 8192;
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var memoryStream = new MemoryStream();
                await fs.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var workbook = new XLWorkbook(memoryStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();

                var (isValid, hasFlagColumn) = CheckDatabase.CheckExcelValidity(worksheet);
                if (!isValid)
                {
                    return new LoadCardBanDataResult
                    {
                        Result = false,
                        Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText())
                    };
                }

                int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
                if (lastRow < 2)
                    return new LoadCardBanDataResult
                    {
                        Result = false,
                        CardList = null,
                        Message = CMess.noValiCardFound.ToText()
                    };

                var importedCards = new List<CardBanList>();
                int errorCount = 0;

                for (int row = 2; row <= lastRow; row++)
                {
                    try
                    {
                        var idCell = worksheet.Cell(row, 1);
                        if (idCell.IsEmpty()) continue;
                        if (!ulong.TryParse(idCell.GetString(), out ulong id)) continue;

                        var card = new CardBanList
                        {
                            Id = id,
                            Name = worksheet.Cell(row, 2).GetString(),
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
                    return new LoadCardBanDataResult
                    {
                        Result = false,
                        Message = CMess.noValiDataFound.ToText()
                    };

                return new LoadCardBanDataResult
                {
                    Result = true,
                    CardList = importedCards,
                    Message = errorCount.ToString()
                };
            }
            catch (Exception ex)
            {
                return new LoadCardBanDataResult
                {
                    Result = false,
                    Message = ex.Message
                };
            }
        }
        public static async Task<LoadCardBanDataResult> LoadCedsCardBanList(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return new LoadCardBanDataResult
                {
                    Result = false,
                    Message = CMess.fileNotExit.ToText()
                };
            }

            try
            {
                const int bufferSize = 8192;

                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);

                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true,
                    bufferSize: bufferSize, leaveOpen: false);

                string json = await reader.ReadToEndAsync().ConfigureAwait(false);

                var importedCards = JsonSerializer.Deserialize<List<CardEditor.Models.CardBanList>>(json, SerializerOptions);

                if (importedCards == null || importedCards.Count == 0)
                {
                    return new LoadCardBanDataResult
                    {
                        Result = false,
                        Message = CMess.noValiDataFound.ToText()
                    };
                }

                Debug.WriteLine($"Imported {importedCards.Count} cards from CEDS file.");

                return new LoadCardBanDataResult
                {
                    Result = true,
                    CardList = importedCards,
                    Message = string.Empty
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                return new LoadCardBanDataResult
                {
                    Result = false,
                    Message = ex.Message
                };
            }
        }
        public static async Task<LoadCardBanDataResult> LoadClipboardCardBanList()
        {
            LoadCardBanDataResult result = new();

            if (!Clipboard.ContainsText())
            {
                return new LoadCardBanDataResult
                {
                    Result = false,
                    Message = CMess.noValiDataClip.ToText()
                };
            }

            try
            {
                string json = string.Empty;
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    json = Clipboard.GetText();
                });

                if (string.IsNullOrWhiteSpace(json))
                {
                    result.Result = false;
                    result.Message = CMess.noValiDataClip.ToText();
                    return result;
                }

                // 1. Thử đọc trực tiếp List<CardBanList>
                var banList = await Task.Run(() => JsonSerializer.Deserialize<List<CardBanList>>(json, SerializerOptions));

                if (banList != null && banList.Count > 0)
                {
                    result.Result = true;
                    result.CardList = banList;
                    return result;
                }

                // 2. Nếu không phải CardBanList thì thử List<Card>
                var cardList = await Task.Run(() => JsonSerializer.Deserialize<List<Card>>(json, SerializerOptions));

                if (cardList != null && cardList.Count > 0)
                {
                    var convertedList = cardList
                        .Select(card => new CardBanList
                        {
                            Id = card.id,
                            Name = card.name,
                            LimitedCount = 3
                        })
                        .ToList();

                    result.Result = true;
                    result.CardList = convertedList;
                    return result;
                }

                result.Result = false;
                result.Message = CMess.noValiDataFound.ToText();
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                return new LoadCardBanDataResult
                {
                    Result = false,
                    Message = ex.Message
                };
            }
        }
        public static async Task<LoadCardBanDataResult> LoadYdkCardBanList(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return new LoadCardBanDataResult
                {
                    Result = false,
                    Message = CMess.fileNotExit.ToText()
                };
            }

            try
            {
                string[] lines;
                var newCards = new List<CardBanList>();

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                using (var reader = new StreamReader(stream))
                {
                    var list = new List<string>();
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        list.Add(line);
                    }
                    lines = list.ToArray();
                }

                var filteredLines = lines.Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim()).Where(line =>
                    !(line.StartsWith("#main", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("#extra", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("!side", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("#created", StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                int hashLineCount = filteredLines.Count(line => line.StartsWith("#"));
                bool includesCardName = hashLineCount >= 2;

                if (includesCardName)
                {
                    string currentCardName = null;
                    foreach (var line in filteredLines)
                    {
                        if (line.StartsWith("#"))
                        {
                            currentCardName = line.Substring(1).Trim();
                        }
                        else if (ulong.TryParse(line, out ulong cardId))
                        {
                            newCards.Add(new CardBanList
                            {
                                Id = cardId,
                                Name = currentCardName ?? string.Empty,
                                LimitedCount = 3
                            });
                            currentCardName = null;
                        }
                    }
                }
                else
                {
                    foreach (var line in filteredLines)
                    {
                        if (ulong.TryParse(line, out ulong cardId))
                        {
                            newCards.Add(new CardBanList
                            {
                                Id = cardId,
                                Name = string.Empty,
                                LimitedCount = 3
                            });
                        }
                    }
                }

                return new LoadCardBanDataResult
                {
                    Result = true,
                    CardList = newCards,
                    Message = string.Empty
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                return new LoadCardBanDataResult
                {
                    Result = false,
                    Message = ex.Message
                };
            }
        }

        public static async Task<LoadCardRareDataResult> LoadDatabaseCardRare(string filePath)
        {
            var result = new LoadCardRareDataResult();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();
                return result;
            }

            string connectionString = $"Data Source={filePath};Version=3;";
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    bool resultCheck = CheckDatabase.CheckDatabaseListNameValidity(connection);
                    if (!resultCheck)
                    {
                        result.Result = false;
                        result.Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText());
                        return result;
                    }

                    var tableNames = new HashSet<string>();
                    using (var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            tableNames.Add(reader.GetString(0).ToLower());
                        }
                    }

                    if (tableNames.Contains("rarecard"))
                    {
                        using (var command = new SQLiteCommand("SELECT id, name, rare FROM RareCard", connection))
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ulong id = reader.IsDBNull(0) ? 0UL : unchecked((ulong)Convert.ToUInt64(reader.GetValue(0)));
                                string name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                                long rare = reader.IsDBNull(2) ? 0L : (long)reader.GetInt64(2);

                                var card = new RareCard
                                {
                                    id = id,
                                    name = name,
                                    rare = rare
                                };
                                result.CardList.Add(card);
                            }
                        }
                    }
                    else if (tableNames.Contains("texts"))
                    {
                        using (var command = new SQLiteCommand("SELECT id, name FROM texts", connection))
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ulong id = reader.IsDBNull(0) ? 0UL : unchecked((ulong)Convert.ToUInt64(reader.GetValue(0)));
                                string name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);

                                var card = new RareCard
                                {
                                    id = id,
                                    name = name,
                                    rare = 0
                                };
                                result.CardList.Add(card);
                            }
                        }
                    }
                }
                result.Result = true;
                result.Message = string.Empty;
                return result;
            }
            catch (SQLiteException ex)
            {
                result.Result = false;
                result.Message = $"{CMess.errorConDB.ToText()} {ex.Message}";
                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = $"{CMess.errorOcc.ToText()} {ex.Message}";
                return result;
            }
        }
        public static async Task<LoadCardRareDataResult> LoadExcelCardRare(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return new LoadCardRareDataResult
                {
                    Result = false,
                    Message = CMess.fileNotExit.ToText()
                };

            try
            {
                const int bufferSize = 8192;
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var memoryStream = new MemoryStream();
                await fs.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var workbook = new XLWorkbook(memoryStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();

                var (isValid, hasFlagColumn) = CheckDatabase.CheckExcelValidity(worksheet);
                if (!isValid)
                {
                    return new LoadCardRareDataResult
                    {
                        Result = false,
                        Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText())
                    };
                }

                int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
                if (lastRow < 2)
                    return new LoadCardRareDataResult
                    {
                        Result = false,
                        CardList = null,
                        Message = CMess.noValiCardFound.ToText()
                    };

                var importedCards = new List<RareCard>();
                int errorCount = 0;

                for (int row = 2; row <= lastRow; row++)
                {
                    try
                    {
                        var idCell = worksheet.Cell(row, 1);
                        if (idCell.IsEmpty()) continue;
                        if (!ulong.TryParse(idCell.GetString(), out ulong id)) continue;

                        var card = new RareCard
                        {
                            id = id,
                            name = worksheet.Cell(row, 2).GetString(),
                            rare = 0
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
                    return new LoadCardRareDataResult
                    {
                        Result = false,
                        Message = CMess.noValiDataFound.ToText()
                    };

                return new LoadCardRareDataResult
                {
                    Result = true,
                    CardList = importedCards,
                    Message = errorCount.ToString()
                };
            }
            catch (Exception ex)
            {
                return new LoadCardRareDataResult
                {
                    Result = false,
                    Message = ex.Message
                };
            }
        }
        public static async Task<LoadCardRareDataResult> LoadCedsCardRare(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return new LoadCardRareDataResult
                {
                    Result = false,
                    Message = CMess.fileNotExit.ToText()
                };
            }

            try
            {
                const int bufferSize = 8192;

                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);

                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true,
                    bufferSize: bufferSize, leaveOpen: false);

                string json = await reader.ReadToEndAsync().ConfigureAwait(false);

                var importedCards = JsonSerializer.Deserialize<List<CardEditor.Models.RareCard>>(json, SerializerOptions);

                if (importedCards == null || importedCards.Count == 0)
                {
                    return new LoadCardRareDataResult
                    {
                        Result = false,
                        Message = CMess.noValiDataFound.ToText()
                    };
                }

                Debug.WriteLine($"Imported {importedCards.Count} cards from CEDS file.");

                return new LoadCardRareDataResult
                {
                    Result = true,
                    CardList = importedCards,
                    Message = string.Empty
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                return new LoadCardRareDataResult
                {
                    Result = false,
                    Message = ex.Message
                };
            }
        }
        public static async Task<LoadCardRareDataResult> LoadYdkCardRare(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return new LoadCardRareDataResult
                {
                    Result = false,
                    Message = CMess.fileNotExit.ToText()
                };
            }

            try
            {
                string[] lines;
                var newCards = new List<RareCard>();

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                using (var reader = new StreamReader(stream))
                {
                    var list = new List<string>();
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        list.Add(line);
                    }
                    lines = list.ToArray();
                }

                var filteredLines = lines.Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim()).Where(line =>
                    !(line.StartsWith("#main", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("#extra", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("!side", StringComparison.OrdinalIgnoreCase) ||
                    line.StartsWith("#created", StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                int hashLineCount = filteredLines.Count(line => line.StartsWith("#"));
                bool includesCardName = hashLineCount >= 2;

                if (includesCardName)
                {
                    string currentCardName = null;
                    foreach (var line in filteredLines)
                    {
                        if (line.StartsWith("#"))
                        {
                            currentCardName = line.Substring(1).Trim();
                        }
                        else if (ulong.TryParse(line, out ulong cardId))
                        {
                            newCards.Add(new RareCard
                            {
                                id = cardId,
                                name = currentCardName ?? string.Empty,
                                rare = 0
                            });
                            currentCardName = null;
                        }
                    }
                }
                else
                {
                    foreach (var line in filteredLines)
                    {
                        if (ulong.TryParse(line, out ulong cardId))
                        {
                            newCards.Add(new RareCard
                            {
                                id = cardId,
                                name = string.Empty,
                                rare = 0
                            });
                        }
                    }
                }

                return new LoadCardRareDataResult
                {
                    Result = true,
                    CardList = newCards,
                    Message = string.Empty
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                return new LoadCardRareDataResult
                {
                    Result = false,
                    Message = ex.Message
                };
            }
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
