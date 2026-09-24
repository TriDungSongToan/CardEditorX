using System;
using System.IO;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Services.CheckData;
using CardEditor.Converter;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.LoadData
{
    public class LoadDatabaseService
    {
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
                        LoadCardDataResult loadResult = await LoadDatabaseCard(databaseFile);

                        if (!loadResult.Result)
                        {
                            Debug.WriteLine($"Error loading {databaseFile}: {loadResult.Message}");
                            continue;
                        }

                        IEnumerable<Card> cardsToAdd;

                        switch (loadResult.Format)
                        {
                            case CardListFormat.YGONoFlag:
                            case CardListFormat.YGOHasFlag:
                                cardsToAdd = loadResult.CardList;
                                break;

                            case CardListFormat.OMEGA:
                                cardsToAdd = CardConverter.ListCardOmegaToListCard(loadResult.OmegaCardList);
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

                    CheckCardListResult checkResult = CheckDatabase.CheckDatabaseCardListValidity(connection);
                    if (!checkResult.Result)
                    {
                        result.Result = false;
                        result.Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText());
                        return result;
                    }

                    result.Format = checkResult.Format;
                    result.HasFlag = checkResult.HasFlag;

                    switch (checkResult.Format)
                    {
                        case CardListFormat.YGONoFlag:
                        case CardListFormat.YGOHasFlag:
                            await LoadYGODatabaseCard(connection, result);
                            break;
                        case CardListFormat.OMEGA:
                            await LoadOMEGADatabaseCard(connection, result);
                            break;
                        default:
                            result.Result = false;
                            result.Message = $"Unsupported database type: {checkResult.Format}";
                            return result;
                    }
                }
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
            try
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
                result.Result = true;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;
            }
        }
        private static async Task LoadOMEGADatabaseCard(SQLiteConnection connection, LoadCardDataResult result)
        {
            try
            {
                string query = $@"
                    SELECT
                        texts.id, texts.name, texts.desc, texts.str1, texts.str2, texts.str3, texts.str4, texts.str5,
                        texts.str6, texts.str7, texts.str8, texts.str9, texts.str10, texts.str11, texts.str12,
                        texts.str13, texts.str14, texts.str15, texts.str16,
                        datas.ot, datas.alias, datas.setcode, datas.type, datas.atk, datas.def, datas.level,
                        datas.race, datas.attribute, datas.category, datas.genre, datas.script, datas.support
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
                                    setcode = ReadDatabaseHelper.ReadSetCodeFromBlob(reader, 21),
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
                                    script = ReadDatabaseHelper.ReadScriptFromBlob(reader, 30),
                                    support = ReadDatabaseHelper.ReadSupportFromBlob(reader, 31)
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
                result.Result = true;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;
            }
        }
    }
}
