using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Collections.Generic;
using ScriptSupport.Legacy.Localization;
using CMess = ScriptSupport.Legacy.Localization.Language;

namespace ScriptSupport.Legacy.Services
{
    public static class LoadDataServices
    {
        public static async Task<(List<ScriptSupport.Legacy.Models.Card>, string)> LoadAllCdbFiles()
        {
            string dataSourcePath = ScriptSupport.Legacy.ViewModels.ConfigViewModel.Instance.userSetting.DataSource;
            if (!Directory.Exists(dataSourcePath)) return (null, $"{CMess.dataSourceErr.ToText()} {dataSourcePath}");

            var allCards = new List<ScriptSupport.Legacy.Models.Card>();
            var loadedIds = new HashSet<ulong>();
            var cardPaths = new Dictionary<ulong, string>();

            try
            {
                // Tìm tất cả *.cdb files (bao gồm subfolders)
                var cdbFiles = Directory.GetFiles(dataSourcePath, "*.cdb", SearchOption.AllDirectories);

                foreach (var cdbFile in cdbFiles)
                {
                    try
                    {
                        var (cards, message) = await LoadDatabaseCard(cdbFile);
                        if (cards == null) continue;

                        foreach (var card in cards)
                        {
                            if (card.id == 0) continue;

                            if (loadedIds.Add(card.id))
                            {
                                allCards.Add(card);
                                cardPaths[card.id] = cdbFile;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading {cdbFile}: {ex.Message}");
                    }
                }
                return (allCards, string.Empty);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
        public static async Task<(List<ScriptSupport.Legacy.Models.Card>, string)> LoadDatabaseCard(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return (null, CMess.fileNotExit.ToText());

            List<ScriptSupport.Legacy.Models.Card> Cards = new List<ScriptSupport.Legacy.Models.Card>();
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
                                    var card = new ScriptSupport.Legacy.Models.Card
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
                                    System.IO.File.AppendAllText(ScriptSupport.Legacy.Models.AppContext.Instance.ErrorLogFilePath,
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
    }
}
