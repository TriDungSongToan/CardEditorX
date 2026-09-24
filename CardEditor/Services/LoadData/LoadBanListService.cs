using System;
using System.IO;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using ClosedXML.Excel;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Services.CheckData;
using CardEditor.Services.ClipboardData;
using CardEditor.Converter;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.LoadData
{
    public class LoadBanListService
    {
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
            var result = new LoadCardBanDataResult();

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

                result.CardList = importedCards;
                result.Message = errorCount.ToString();
                result.Result = true;
                return result;
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
            var result = new LoadCardBanDataResult();

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

                if (resultLoadCeds.Format != CardListFormat.BanList)
                {
                    result.Result = false;
                    result.Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Data.ToText(), CMess.Format.ToText());
                    return result;
                }

                result.CardList = resultLoadCeds.CardBanlistList;
                result.Message = string.Empty;
                result.Result = true;

                return result;
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
        public static async Task<LoadCardBanDataResult> LoadClipboardCardBanList()
        {
            LoadCardBanDataResult result = new();

            if (!Clipboard.ContainsText())
            {
                result.Result = false;
                result.Message = CMess.noValiDataClip.ToText();
                return result;
            }

            try
            {
                LoadClipboardResult resultLoadClipboard = await PasteCardService.LoadFromClipboard();
                if (!resultLoadClipboard.Result)
                {
                    result.Result = false;
                    result.Message = resultLoadClipboard.Message;
                    return result;
                }

                List<CardBanList> CardList = resultLoadClipboard.Cards.Format switch
                {
                    CardListFormat.YGONoFlag => CardConverter.ListCardToListCardBanList(resultLoadClipboard.Cards.CardList),
                    CardListFormat.YGOHasFlag => CardConverter.ListCardToListCardBanList(resultLoadClipboard.Cards.CardList),
                    CardListFormat.OMEGA => CardConverter.ListCardOmegaToListCardBanList(resultLoadClipboard.Cards.CardOmegaList),
                    CardListFormat.BanList => resultLoadClipboard.Cards.CardBanlistList,
                    _ => new List<CardBanList>()
                };

                result.CardList = CardList;
                result.Message = resultLoadClipboard.Message;
                result.Result = true;
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
            LoadCardBanDataResult result = new();

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();
                return result;
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

                result.Result = true;
                result.CardList = newCards;
                result.Message = string.Empty;
                return result;
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
    }
}
