using System;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Services.CheckData;
using CardEditor.Converter;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.LoadData
{
    internal class LoadRarityService
    {
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
            var result = new LoadCardRareDataResult();

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();

                return result;
            }

            try
            {
                const int bufferSize = 8192;
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var memoryStream = new MemoryStream();
                await fs.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using var workbook = new XLWorkbook(memoryStream);
                var worksheet = workbook.Worksheets.FirstOrDefault();

                CheckCardListResult checkResult = CheckExcel.CheckExcelCardListValidity(worksheet);
                if (!checkResult.Result)
                {
                    result.Result = false;
                    result.Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText());
                    return result;
                }

                int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
                if (lastRow < 2)
                {
                    result.Result = false;
                    result.CardList = new();
                    result.Message = CMess.noCardFound.ToText();

                    return result;
                }

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

                result.CardList = importedCards;
                result.Message = errorCount.ToString();
                result.Result = true;
                return result;
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
            LoadCardRareDataResult result = new();

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();
                return result;
            }

            try
            {
                LoadCardDataResult resultLoadCeds = await LoadCedsService.LoadCedsCard(filePath);
                if (!resultLoadCeds.Result)
                {
                    result.Result = false;
                    result.Message = resultLoadCeds.Message;
                    return result;
                }

                List<RareCard> cardRareList = resultLoadCeds.Format switch
                {
                    CardListFormat.YGONoFlag => CardConverter.ListCardToListCardRare(resultLoadCeds.CardList),
                    CardListFormat.YGOHasFlag => CardConverter.ListCardToListCardRare(resultLoadCeds.CardList),
                    CardListFormat.OMEGA => CardConverter.ListCardOmegaToListCardRare(resultLoadCeds.OmegaCardList),
                    CardListFormat.BanList => CardConverter.ListCardBanlistToListRareCard(resultLoadCeds.CardBanlistList),
                    _ => new List<RareCard>()
                };

                result.CardList = cardRareList;
                result.Message = string.Empty;
                result.Result = true;

                return result;
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
        public static async Task<LoadCardRareDataResult> LoadYdkCardRare(string filePath)
        {
            var result = new LoadCardRareDataResult();

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();
                return result;
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

                result.Result = true;
                result.CardList = newCards;
                result.Message = string.Empty;
                return result;
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
    }
}
