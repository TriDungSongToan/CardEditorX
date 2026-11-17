using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Data.Sqlite;
using ScriptSupport.Models;
using ScriptSupport.Stores;
using ScriptSupport.States;
using ScriptSupport.Helpers;
using ScriptSupport.Interfaces;
using ScriptSupport.Environment;
using ScriptSupport.Localization;
using CMess = ScriptSupport.Localization.Language;

namespace ScriptSupport.Services
{
    public class CardService : ICardInterface
    {
        #region Fields
        private readonly AppEnvironment _env;
        private readonly ConfigStore _config;
        private readonly CardStore _cardStore;
        private FilterCardState _filterState { get; }
        private FilterConfigState _filterConfig { get; }
        private IGenesysInterface _genesysService;
        private CardDataFilter CurrentData = new();
        private CardTextFilter CurrentText = new();
        private bool CurrentIsLink = false;
        #endregion

        public CardService(AppEnvironment env, ConfigStore config,
            CardStore cardStore, FilterCardState filterState,
            FilterConfigState filterConfig, IGenesysInterface genesysService)
        {
            _env = env;
            _config = config;
            _cardStore = cardStore;
            _filterState = filterState;
            _filterConfig = filterConfig;
            _genesysService = genesysService;
        }

        #region Load Data
        public async Task<(bool Success, string Message)> LoadCardDBAsync()
        {
            string dataSourcePath = _config.UserSetting.DataSource;
            if (string.IsNullOrEmpty(dataSourcePath) || !Directory.Exists(dataSourcePath))
                return (false, CMess.dataSourceMiss.ToText());

            var (datas, texts, logs, message) = await LoadAllCdbFiles(dataSourcePath).ConfigureAwait(false);
            if (datas == null || texts == null) return (false, message);

            if (datas != null && datas.Count > 0) await _cardStore.SetCardDatas(datas);
            if (texts != null && texts.Count > 0) _cardStore.SetCardTexts(texts);
            if (logs != null && logs.Count > 0) await File.AppendAllLinesAsync(_env.ErrorLogFilePath, logs);
            return (true, string.Empty);
        }

        public async Task<(Dictionary<ulong, CardData>?, Dictionary<ulong, IReadOnlyList<CardText>>?, List<string>?, string)> LoadAllCdbFiles(string dataSourcePath)
        {
            if (string.IsNullOrEmpty(dataSourcePath) || !Directory.Exists(dataSourcePath))
                return (null, null, null, CMess.dataSourceMiss.ToText());

            var cdbFiles = Directory.EnumerateFiles(dataSourcePath, "*.cdb", SearchOption.AllDirectories).OrderBy(x => x).ToList();
            if (cdbFiles.Count == 0) return (new(), new(), new(), string.Empty);

            int maxConcurrency = Math.Min(System.Environment.ProcessorCount * 2, cdbFiles.Count);
            using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

            var tasks = cdbFiles.Select(async cdbFile =>
            {
                await semaphore.WaitAsync().ConfigureAwait(false);
                try
                {
                    return await LoadDatabaseCard(cdbFile).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading {cdbFile}: {ex.Message}");
                    return (null, null, new List<string> { ex.Message }, cdbFile);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            var allDatas = new Dictionary<ulong, CardData>(25000);
            var allTexts = new Dictionary<ulong, List<CardText>>(25000);
            var allLogs = new List<string>();

            foreach (var (datas, texts, rowLogs, globalError) in results)
            {
                if (rowLogs != null && rowLogs.Count > 0) allLogs.AddRange(rowLogs);
                if (!string.IsNullOrEmpty(globalError)) allLogs.Add(globalError);

                if (datas != null)
                {
                    foreach (var data in datas)
                    {
                        if (data.id == 0) continue;
                        allDatas[data.id] = data;
                    }
                }

                if (texts != null)
                {
                    foreach (var text in texts)
                    {
                        if (text.id == 0) continue;

                        if (!allTexts.TryGetValue(text.id, out var list))
                        {
                            list = new List<CardText>(2);
                            allTexts[text.id] = list;
                        }
                        list.Add(text);
                    }
                }
            }
            var finalTexts = new Dictionary<ulong, IReadOnlyList<CardText>>(allTexts.Count);
            foreach (var kv in allTexts)
                finalTexts[kv.Key] = kv.Value;

            return (allDatas, finalTexts, allLogs, string.Empty);
        }
        public async Task<(List<CardData>?, List<CardText>?, List<string>?, string)> LoadDatabaseCard(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return (null, null, null, CMess.fileNotExit.ToText());

            var cardDatas = new List<CardData>(1024);
            var cardTexts = new List<CardText>(1024);
            var logs = new List<string>();

            try
            {
                string connStr = $"Data Source={filePath};Mode=ReadOnly;Pooling=True;";
                await using var conn = new SqliteConnection(connStr);
                await conn.OpenAsync().ConfigureAwait(false);

                await using (var pragma = conn.CreateCommand())
                {
                    pragma.CommandText = @"
                    PRAGMA journal_mode=OFF;
                    PRAGMA synchronous=OFF;
                    PRAGMA temp_store=MEMORY;
                    PRAGMA cache_size=-10000;
                    PRAGMA mmap_size=268435456;
                ";
                    await pragma.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                var t1 = LoadDatas(conn, cardDatas, logs);
                var t2 = LoadTexts(conn, cardTexts, logs, filePath);

                await Task.WhenAll(t1, t2).ConfigureAwait(false);

                return (cardDatas, cardTexts, logs, string.Empty);





                //// PRAGMA tối ưu cho read-only, chạy 1 lần khi mở connection
                //await using (var pragma = new SqliteCommand(
                //"PRAGMA cache_size=-8000; PRAGMA mmap_size=268435456;", conn))
                //    await pragma.ExecuteNonQueryAsync().ConfigureAwait(false);

                //await using var command = new SqliteCommand(BaseQuery, conn);
                //await using var reader = (SqliteDataReader)await command.ExecuteReaderAsync(
                //System.Data.CommandBehavior.SequentialAccess).ConfigureAwait(false);

                //while (await reader.ReadAsync().ConfigureAwait(false))
                //{
                //    try
                //    {
                //        if (reader.IsDBNull(0) || !ulong.TryParse(reader.GetValue(0)?.ToString(), out ulong id)) continue;

                //        cardDatas.Add(new CardData
                //        {
                //            id = id,
                //            ot = reader.GetULong(19),
                //            alias = reader.GetULong(20),
                //            setcode = reader.GetULong(21),
                //            type = reader.GetULong(22),
                //            atk = reader.GetLong(23),
                //            def = reader.GetLong(24),
                //            level = reader.GetULong(25),
                //            race = reader.GetULong(26),
                //            attribute = reader.GetULong(27),
                //            category = reader.GetULong(28),
                //        });
                //        cardTexts.Add(new CardText
                //        {
                //            id = id,
                //            name = reader.GetStringSafe(1),
                //            desc = reader.GetStringSafe(2),
                //            str1 = reader.GetStringSafe(3),
                //            str2 = reader.GetStringSafe(4),
                //            str3 = reader.GetStringSafe(5),
                //            str4 = reader.GetStringSafe(6),
                //            str5 = reader.GetStringSafe(7),
                //            str6 = reader.GetStringSafe(8),
                //            str7 = reader.GetStringSafe(9),
                //            str8 = reader.GetStringSafe(10),
                //            str9 = reader.GetStringSafe(11),
                //            str10 = reader.GetStringSafe(12),
                //            str11 = reader.GetStringSafe(13),
                //            str12 = reader.GetStringSafe(14),
                //            str13 = reader.GetStringSafe(15),
                //            str14 = reader.GetStringSafe(16),
                //            str15 = reader.GetStringSafe(17),
                //            str16 = reader.GetStringSafe(18),
                //            DBPath = filePath,
                //        });
                //    }
                //    catch (Exception ex)
                //    {
                //        string cardId = reader.IsDBNull(0) ? "Unknown" : reader.GetValue(0)?.ToString() ?? "Unknown";
                //        logs.Add($"Conversion error ID={cardId}: {ex.Message} {filePath}");
                //    }
                //}
                //// await File.AppendAllLinesAsync(_env.ErrorLogFilePath, logs);
                //return (cardDatas, cardTexts, logs, string.Empty);
            }
            catch (SQLiteException ex) { return (null, null, null, $"{CMess.errorConDB.ToText()} {ex.Message}"); }
            catch (Exception ex) { return (null, null, null, $"{CMess.errorOcc.ToText()} {ex.Message}"); }
        }

        private static async Task LoadDatas(SqliteConnection conn, List<CardData> datas, List<string> logs)
        {
            const string sql = @"SELECT id, ot, alias, setcode, type, atk, def, level, race, attribute, category FROM datas";

            await using var cmd = new SqliteCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

            while (await reader.ReadAsync())
            {
                try
                {
                    if (reader.IsDBNull(0)) continue;

                    ulong id = reader.GetULong(0);

                    datas.Add(new CardData
                    {
                        id = id,
                        ot = reader.GetULong(1),
                        alias = reader.GetULong(2),
                        setcode = reader.GetULong(3),
                        type = reader.GetULong(4),
                        atk = reader.GetLong(5),
                        def = reader.GetLong(6),
                        level = reader.GetULong(7),
                        race = reader.GetULong(8),
                        attribute = reader.GetULong(9),
                        category = reader.GetULong(10)
                    });
                }
                catch (Exception ex)
                {
                    logs.Add($"Data parse error: {ex.Message}");
                }
            }
        }
        private static async Task LoadTexts(SqliteConnection conn, List<CardText> texts, List<string> logs, string file)
        {
            const string sql = @"SELECT id, name, desc,
                                   str1,str2,str3,str4,str5,str6,str7,str8,
                                   str9,str10,str11,str12,str13,str14,str15,str16
                            FROM texts";

            await using var cmd = new SqliteCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

            while (await reader.ReadAsync())
            {
                try
                {
                    if (reader.IsDBNull(0)) continue;

                    ulong id = reader.GetULong(0);

                    texts.Add(new CardText
                    {
                        id = id,
                        name = reader.GetStringSafe(1),
                        desc = reader.GetStringSafe(2),
                        str1 = reader.GetStringSafe(3),
                        str2 = reader.GetStringSafe(4),
                        str3 = reader.GetStringSafe(5),
                        str4 = reader.GetStringSafe(6),
                        str5 = reader.GetStringSafe(7),
                        str6 = reader.GetStringSafe(8),
                        str7 = reader.GetStringSafe(9),
                        str8 = reader.GetStringSafe(10),
                        str9 = reader.GetStringSafe(11),
                        str10 = reader.GetStringSafe(12),
                        str11 = reader.GetStringSafe(13),
                        str12 = reader.GetStringSafe(14),
                        str13 = reader.GetStringSafe(15),
                        str14 = reader.GetStringSafe(16),
                        str15 = reader.GetStringSafe(17),
                        str16 = reader.GetStringSafe(18),
                        DBPath = file
                    });
                }
                catch (Exception ex)
                {
                    logs.Add($"Text parse error: {ex.Message}");
                }
            }
        }
        #endregion

        #region Filter

        #region Filter By Datas
        private List<ulong> GetFoundIdsByData(CancellationToken token)
        {
            // Chọn hàm filter tương ứng với mode hiện tại
            Func<CardData, bool> filterFunc = _config.DataHandlingSetting.FilterMode switch
            {
                1 => FilterDataOR,
                2 => FilterDataMixedAND_OR,
                3 => FilterDataMixedOR_AND,
                _ => FilterDataAND   // mode 0 và mọi giá trị khác
            };

            return _cardStore.CardDatas.Values.AsParallel().WithCancellation(token)
                .Where(cd => filterFunc(cd)).Select(cd => cd.id).ToList();
        }
        private bool FilterDataAND(CardData card)
        {
            if (CurrentData.Id.HasValue && card.id != CurrentData.Id.Value && card.alias != CurrentData.Id.Value) return false;
            if (CurrentData.Alias.HasValue && card.alias != CurrentData.Alias.Value) return false;
            if (CurrentData.ot != 0 && (card.ot & CurrentData.ot) != CurrentData.ot) return false;
            if (CurrentData.Type != 0 && (card.type & CurrentData.Type) != CurrentData.Type) return false;
            if (CurrentData.Attribute != 0 && (card.attribute & CurrentData.Attribute) != CurrentData.Attribute) return false;
            if (CurrentData.Race != 0 && (card.race & CurrentData.Race) != CurrentData.Race) return false;
            if (MatchesSetcode.MatchesSetCode_AND(card.setcode, CurrentData.SetCode) == false) return false;
            if (CurrentData.Category != 0 && (card.category & CurrentData.Category) != CurrentData.Category) return false;
            if (CurrentData.Flag != 0 && (card.flag & CurrentData.Flag) != CurrentData.Flag) return false;
            if (CurrentData.Rarity != 0 && (card.rarity & CurrentData.Rarity) != CurrentData.Rarity) return false;
            ///////////////////////
            int low8 = (int)(card.level & 0xFF);
            int high8 = (int)((card.level >> 8) & 0xFF);
            if (CurrentIsLink)
            {
                if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value != low8) return false;
                if (CurrentData.Level.HasValue && CurrentData.Level.Value != high8) return false;
                if (CurrentData.Def.HasValue || CurrentData.LinkMaker != 0)
                {
                    var (matchDEF, matchLink) = MatchesDEFLink_AND(card.def);
                    if (!matchDEF || !matchLink) return false;
                }
            }
            else
            {
                if (CurrentData.Level.HasValue && CurrentData.Level.Value != low8) return false;
                // if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value != high8) return false;
                if (CurrentData.Def.HasValue)
                {
                    if (CurrentData.Def.Value >= 0 && card.def != CurrentData.Def.Value) return false;
                    if (CurrentData.Def.Value < 0 && card.def >= 0) return false;
                }
            }
            if (CurrentData.LeftScale.HasValue && CurrentData.LeftScale.Value != (int)((card.level >> 24) & 0xFF)) return false;
            if (CurrentData.RightScale.HasValue && CurrentData.RightScale.Value != (int)((card.level >> 16) & 0xFF)) return false;

            if (CurrentData.Atk.HasValue)
            {
                if (CurrentData.Atk.Value >= 0 && card.atk != CurrentData.Atk.Value) return false;
                if (CurrentData.Atk.Value < 0 && card.atk >= 0) return false;
            }

            //if (CurrentData.GPoint.HasValue && card.GPoint != CurrentData.GPoint.Value) return false;
            if (CurrentData.GPoint.HasValue)
            {
                var gPoint = _genesysService.GetGPoint(card.id);
                if (gPoint.HasValue && gPoint.Value != CurrentData.GPoint) return false;
            } 

            //if (!MatchString(card.BaseCard.name, CurrentData.CardDesc) && !MatchString(card.BaseCard.desc, CurrentData.CardDesc)) return false;

            return true;
        }
        private bool FilterDataOR(CardData card)
        {
            if (CurrentData.Id.HasValue && (card.id == CurrentData.Id.Value || card.alias == CurrentData.Id.Value)) return true;
            if (CurrentData.ot != 0 && (card.ot & CurrentData.ot) != 0) return true;
            if (CurrentData.Type != 0 && (card.type & CurrentData.Type) != 0) return true;
            if (CurrentData.Attribute != 0 && (card.attribute & CurrentData.Attribute) != 0) return true;
            if (CurrentData.Race != 0 && (card.race & CurrentData.Race) != 0) return true;
            if (MatchesSetcode.MatchesSetCode_OR(card.setcode, CurrentData.SetCode) == true) return true;
            if (CurrentData.Category != 0 && (card.category & CurrentData.Category) != 0) return true;
            if (CurrentData.Flag != 0 && (card.flag & CurrentData.Flag) != 0) return true;
            if (CurrentData.Rarity != 0 && (card.rarity & CurrentData.Rarity) != 0) return true;
            ///////////////////////
            int low8 = (int)(card.level & 0xFF);
            int high8 = (int)((card.level >> 8) & 0xFF);
            if (CurrentIsLink)
            {
                if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value == low8) return true;
                if (CurrentData.Level.HasValue && CurrentData.Level.Value == high8) return true;
                if (CurrentData.Def.HasValue || CurrentData.LinkMaker != 0)
                {
                    var (matchDEF, matchLink) = MatchesDEFLink_OR(card.def);
                    if (matchDEF || matchLink) return true;
                }
            }
            else
            {
                if (CurrentData.Level.HasValue && CurrentData.Level.Value == low8) return true;
                if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value == high8) return true;
                if (CurrentData.Def.HasValue)
                {
                    if (CurrentData.Def.Value >= 0 && card.def == CurrentData.Def.Value) return true;
                    if (CurrentData.Def.Value < 0 && card.def < 0) return true;
                }
            }
            if (CurrentData.LeftScale.HasValue && CurrentData.LeftScale.Value == (int)((card.level >> 24) & 0xFF)) return true;
            if (CurrentData.RightScale.HasValue && CurrentData.RightScale.Value == (int)((card.level >> 16) & 0xFF)) return true;
            if (CurrentData.Atk.HasValue)
            {
                if (CurrentData.Atk.Value >= 0 && card.atk == CurrentData.Atk.Value) return true;
                if (CurrentData.Atk.Value < 0 && card.atk < 0) return true;
            }

            if (CurrentData.GPoint.HasValue)
            {
                var gPoint = _genesysService.GetGPoint(card.id);
                if (gPoint.HasValue && gPoint.Value == CurrentData.GPoint) return true;
            }

            //if (CurrentData.GPoint.HasValue && card.GPoint == CurrentData.GPoint.Value) return true;
            //if (MatchString(card.BaseCard.name, CurrentData.CardDesc)) return true;
            //if (MatchString(card.BaseCard.desc, CurrentData.CardDesc)) return true;

            return false;
        }
        private bool FilterDataMixedAND_OR(CardData cd)
        {
            if (CurrentData.ot != 0)
                if ((cd.ot & CurrentData.ot) == 0) return false;    // không khớp bất kỳ bit nào

            // Nhóm ID
            if (CurrentData.Id.HasValue)
                if (cd.id != CurrentData.Id.Value && cd.alias != CurrentData.Id.Value) return false;

            // Nhóm Type — card phải có ít nhất 1 bit trùng với filter
            if (CurrentData.Type != 0)
                if ((cd.type & CurrentData.Type) == 0) return false;

            // Nhóm Attribute
            if (CurrentData.Attribute != 0)
                if ((cd.attribute & CurrentData.Attribute) == 0) return false;

            // Nhóm Race
            if (CurrentData.Race != 0)
                if ((cd.race & CurrentData.Race) == 0) return false;

            // Nhóm SetCode
            if (CurrentData.SetCode != 0)
                if (!MatchesSetcode.MatchesSetCode_OR(cd.setcode, CurrentData.SetCode)) return false;

            // Nhóm Category
            if (CurrentData.Category != 0)
                if ((cd.category & CurrentData.Category) == 0) return false;

            // Nhóm Flag
            if (CurrentData.Flag != 0)
                if ((cd.flag & CurrentData.Flag) == 0) return false;

            // Nhóm Genesys Point
            if (CurrentData.GPoint.HasValue && CurrentData.GPoint.Value != 0)
            {
                var gPoint = _genesysService.GetGPoint(cd.id);
                if (gPoint.HasValue && gPoint.Value == CurrentData.GPoint) return true;
            }

            // Nhóm Level / Scale — bất kỳ sub-field nào khớp là nhóm này pass
            bool hasLevelFilter = CurrentData.Level.HasValue ||
                                  CurrentData.LinkRating.HasValue ||
                                  CurrentData.LeftScale.HasValue ||
                                  CurrentData.RightScale.HasValue;
            if (hasLevelFilter)
            {
                int low8 = (int)(cd.level & 0xFF);
                int high8 = (int)((cd.level >> 8) & 0xFF);
                int rightScale = (int)((cd.level >> 16) & 0xFF);
                int leftScale = (int)((cd.level >> 24) & 0xFF);
                bool levelGroupPass = false;

                if (CurrentIsLink)
                {
                    if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value == low8) levelGroupPass = true;
                    if (CurrentData.Level.HasValue && CurrentData.Level.Value == high8) levelGroupPass = true;
                }
                else
                {
                    if (CurrentData.Level.HasValue && CurrentData.Level.Value == low8) levelGroupPass = true;
                }
                if (CurrentData.LeftScale.HasValue && CurrentData.LeftScale.Value == leftScale) levelGroupPass = true;
                if (CurrentData.RightScale.HasValue && CurrentData.RightScale.Value == rightScale) levelGroupPass = true;

                if (!levelGroupPass) return false;
            }

            // Nhóm ATK / DEF / LinkArrows — bất kỳ sub-field nào khớp là nhóm này pass
            bool hasPowerFilter = CurrentData.Atk.HasValue ||
                                  CurrentData.Def.HasValue ||
                                  CurrentData.LinkMaker != 0;
            if (hasPowerFilter)
            {
                bool powerGroupPass = false;

                if (CurrentData.Atk.HasValue)
                {
                    if (CurrentData.Atk.Value >= 0 && cd.atk == CurrentData.Atk.Value) powerGroupPass = true;
                    if (CurrentData.Atk.Value < 0 && cd.atk < 0) powerGroupPass = true;
                }
                if (CurrentIsLink)
                {
                    if (CurrentData.Def.HasValue || CurrentData.LinkMaker != 0)
                    {
                        var (matchDEF, matchLink) = MatchesDEFLink_OR(cd.def);
                        if (matchDEF || matchLink) powerGroupPass = true;
                    }
                }
                else if (CurrentData.Def.HasValue)
                {
                    if (CurrentData.Def.Value >= 0 && cd.def == CurrentData.Def.Value) powerGroupPass = true;
                    if (CurrentData.Def.Value < 0 && cd.def < 0) powerGroupPass = true;
                }

                if (!powerGroupPass) return false;
            }

            return true;
        }
        private bool FilterDataMixedOR_AND(CardData cd)
        {
            // Nhóm Format — tất cả bit filter phải có mặt
            if (CurrentData.ot != 0)
            {
                bool pass = (cd.ot & CurrentData.ot) == CurrentData.ot;
                if (pass) return true;
            }

            // Nhóm ID
            if (CurrentData.Id.HasValue)
                if (cd.id == CurrentData.Id.Value || cd.alias == CurrentData.Id.Value) return true;

            // Nhóm Type — tất cả bit được chọn phải có mặt
            if (CurrentData.Type != 0)
                if ((cd.type & CurrentData.Type) == CurrentData.Type) return true;

            // Nhóm Attribute
            if (CurrentData.Attribute != 0)
                if ((cd.attribute & CurrentData.Attribute) == CurrentData.Attribute) return true;

            // Nhóm Race
            if (CurrentData.Race != 0)
                if ((cd.race & CurrentData.Race) == CurrentData.Race) return true;

            // Nhóm SetCode
            if (CurrentData.SetCode != 0)
                if (MatchesSetcode.MatchesSetCode_AND(cd.setcode, CurrentData.SetCode)) return true;

            // Nhóm Category
            if (CurrentData.Category != 0)
                if ((cd.category & CurrentData.Category) == CurrentData.Category) return true;

            // Nhóm Flag
            if (CurrentData.Flag != 0)
                if ((cd.flag & CurrentData.Flag) == CurrentData.Flag) return true;

            // Nhóm Genesys Point
            if (CurrentData.GPoint.HasValue && CurrentData.GPoint.Value != 0)
            {
                var gPoint = _genesysService.GetGPoint(cd.id);
                if (gPoint.HasValue && gPoint.Value == CurrentData.GPoint) return true;
            }

            // Nhóm Level / Scale — tất cả sub-field được đặt đều phải khớp
            bool hasLevelFilter = CurrentData.Level.HasValue ||
                                  CurrentData.LinkRating.HasValue ||
                                  CurrentData.LeftScale.HasValue ||
                                  CurrentData.RightScale.HasValue;
            if (hasLevelFilter)
            {
                int low8 = (int)(cd.level & 0xFF);
                int high8 = (int)((cd.level >> 8) & 0xFF);
                int rightScale = (int)((cd.level >> 16) & 0xFF);
                int leftScale = (int)((cd.level >> 24) & 0xFF);
                bool levelGroupPass = true;   // AND: bắt đầu true, nếu bất kỳ sub-field nào sai → false

                if (CurrentIsLink)
                {
                    if (CurrentData.LinkRating.HasValue && CurrentData.LinkRating.Value != low8) levelGroupPass = false;
                    if (CurrentData.Level.HasValue && CurrentData.Level.Value != high8) levelGroupPass = false;
                }
                else
                {
                    if (CurrentData.Level.HasValue && CurrentData.Level.Value != low8) levelGroupPass = false;
                }
                if (CurrentData.LeftScale.HasValue && CurrentData.LeftScale.Value != leftScale) levelGroupPass = false;
                if (CurrentData.RightScale.HasValue && CurrentData.RightScale.Value != rightScale) levelGroupPass = false;

                if (levelGroupPass) return true;
            }

            // Nhóm ATK / DEF / LinkArrows — tất cả sub-field được đặt đều phải khớp
            bool hasPowerFilter = CurrentData.Atk.HasValue ||
                                  CurrentData.Def.HasValue ||
                                  CurrentData.LinkMaker != 0;
            if (hasPowerFilter)
            {
                bool powerGroupPass = true;   // AND: bắt đầu true

                if (CurrentData.Atk.HasValue)
                {
                    if (CurrentData.Atk.Value >= 0 && cd.atk != CurrentData.Atk.Value) powerGroupPass = false;
                    if (CurrentData.Atk.Value < 0 && cd.atk >= 0) powerGroupPass = false;
                }
                if (powerGroupPass)   // chỉ check tiếp nếu ATK vẫn pass
                {
                    if (CurrentIsLink)
                    {
                        if (CurrentData.Def.HasValue || CurrentData.LinkMaker != 0)
                        {
                            var (matchDEF, matchLink) = MatchesDEFLink_AND(cd.def);
                            if (!matchDEF || !matchLink) powerGroupPass = false;
                        }
                    }
                    else if (CurrentData.Def.HasValue)
                    {
                        if (CurrentData.Def.Value >= 0 && cd.def != CurrentData.Def.Value) powerGroupPass = false;
                        if (CurrentData.Def.Value < 0 && cd.def >= 0) powerGroupPass = false;
                    }
                }

                if (powerGroupPass) return true;
            }

            return false;
        }

        #region Match DEF
        private const long maskHasDEF = 1L << 31;
        private const long maskLinkArrow = 1L << 4;
        public (bool MatchDEF, bool MatchLink) MatchesDEFLink_AND(long baseDEF)
        {
            bool matchesDEF = true;
            bool matchesLinkArrows = true;

            if (CurrentData.Def.HasValue)
            {
                if ((baseDEF & maskHasDEF) != 0) // hasDEF
                {
                    var decoded = GetInfoHelper.DecodeDef(baseDEF);
                    if (decoded.deffromtext is long defFromCard)
                    {
                        if (CurrentData.Def.Value >= 0) matchesDEF = defFromCard == CurrentData.Def.Value;
                        else matchesDEF = defFromCard < 0;
                    }
                }
            }

            if (CurrentData.LinkMaker != 0)
            {
                long linkArrowText = baseDEF & 0x1FFL;

                if (linkArrowText == CurrentData.LinkMaker) matchesLinkArrows = true;
                else
                {
                    long merged = linkArrowText | maskLinkArrow;
                    matchesLinkArrows = merged == CurrentData.LinkMaker;
                }
            }
            return (matchesDEF, matchesLinkArrows);
        }
        public (bool MatchDEF, bool MatchLink) MatchesDEFLink_OR(long baseDEF)
        {
            bool matchesDEF = true;
            bool matchesLinkArrows = true;

            if (CurrentData.Def.HasValue)
            {
                if ((baseDEF & maskHasDEF) != 0) // hasDEF
                {
                    var decoded = GetInfoHelper.DecodeDef(baseDEF);
                    if (decoded.deffromtext is long defFromCard)
                    {
                        if (CurrentData.Def.Value >= 0) matchesDEF = defFromCard == CurrentData.Def.Value;
                        else matchesDEF = defFromCard < 0;
                    }
                }
            }

            if (CurrentData.LinkMaker != 0)
            {
                long linkArrowText = baseDEF & 0x1FFL;

                matchesLinkArrows = (linkArrowText & CurrentData.LinkMaker) == CurrentData.LinkMaker;
                if (!matchesLinkArrows)
                {
                    linkArrowText |= maskLinkArrow;
                    matchesLinkArrows = (linkArrowText & CurrentData.LinkMaker) == CurrentData.LinkMaker;
                }
            }
            return (matchesDEF, matchesLinkArrows);
        }
        #endregion

        #endregion

        #region Filter By Texts
        private bool _isRegexDirty = true;
        private StringComparison _comparison;
        private Dictionary<string, Regex>? _compiledRegexes;
        private Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();
        private List<CardText> GetFoundCardsByText(List<ulong> foundIds, CancellationToken token)
        {
            var result = new List<CardText>();

            foreach (var id in foundIds)
            {
                token.ThrowIfCancellationRequested();

                // Không tìm thấy ID này trong CardTexts → bỏ qua
                if (!_cardStore.CardTexts.TryGetValue(id, out var textList)) continue;

                // Với mỗi CardText của ID này (mỗi ngôn ngữ là 1 item)
                foreach (var cardText in textList)
                {
                    if (FilterText(cardText)) result.Add(cardText);
                }
            }

            return result;
        }
        private bool FilterText(CardText ct)
        {
            // Không có filter text nào được đặt → pass hết
            if (CurrentText == null) return true;

            // Filter theo Id
            if (CurrentText.Id.HasValue)
                if (ct.id != CurrentText.Id.Value) return false;

            // Filter theo Name
            if (!string.IsNullOrWhiteSpace(CurrentText.Name))
                if (!MatchString(ct.name, CurrentText.Name)) return false;

            // Filter theo Desc
            if (!string.IsNullOrWhiteSpace(CurrentText.Desc))
                if (!MatchString(ct.desc, CurrentText.Desc)) return false;

            // Filter theo Str — thỏa mãn nếu bất kỳ str nào (str1..str16) match
            if (!string.IsNullOrWhiteSpace(CurrentText.Str))
            {
                bool anyStrMatch =
                    MatchString(ct.str1, CurrentText.Str) ||
                    MatchString(ct.str2, CurrentText.Str) ||
                    MatchString(ct.str3, CurrentText.Str) ||
                    MatchString(ct.str4, CurrentText.Str) ||
                    MatchString(ct.str5, CurrentText.Str) ||
                    MatchString(ct.str6, CurrentText.Str) ||
                    MatchString(ct.str7, CurrentText.Str) ||
                    MatchString(ct.str8, CurrentText.Str) ||
                    MatchString(ct.str9, CurrentText.Str) ||
                    MatchString(ct.str10, CurrentText.Str) ||
                    MatchString(ct.str11, CurrentText.Str) ||
                    MatchString(ct.str12, CurrentText.Str) ||
                    MatchString(ct.str13, CurrentText.Str) ||
                    MatchString(ct.str14, CurrentText.Str) ||
                    MatchString(ct.str15, CurrentText.Str) ||
                    MatchString(ct.str16, CurrentText.Str);

                if (!anyStrMatch) return false;
            }

            return true;
        }

        private bool MatchString(string source, string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern)) return true;
            if (string.IsNullOrWhiteSpace(source)) return false;

            string normalizedSource = NormalizeString(source);
            string normalizedPattern = NormalizeString(pattern);

            if (!_filterConfig.Advanced)
                return normalizedSource.IndexOf(normalizedPattern, _comparison) >= 0;

            if (_filterConfig.Wildcards || _filterConfig.MatchWhole)
            {
                if (_compiledRegexes != null && _compiledRegexes.TryGetValue(pattern, out var regex))
                    return regex.IsMatch(normalizedSource);
                string rp = _filterConfig.Wildcards
                    ? BuildRegexPattern(normalizedPattern, _filterConfig.Prefix, _filterConfig.Suffix, _filterConfig.MatchWhole)
                    : $@"\b{Regex.Escape(normalizedPattern)}\b";
                return GetOrCreateRegex(rp, _filterConfig.MatchCase).IsMatch(normalizedSource);
            }

            int index = normalizedSource.IndexOf(normalizedPattern, _comparison);
            if (index < 0) return false;
            if (_filterConfig.Prefix && index != 0) return false;
            if (_filterConfig.Suffix && (index + normalizedPattern.Length) != normalizedSource.Length) return false;
            return true;
        }
        private string NormalizeString(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;
            string normalized = input;
            if (_filterConfig.Ignpunct) normalized = new string(normalized.Where(c => !char.IsPunctuation(c)).ToArray());
            if (_filterConfig.Ignpspace) normalized = Regex.Replace(normalized, @"\s+", "");
            return normalized;
        }
        private string BuildRegexPattern(string pattern, bool matchPrefix, bool matchSuffix, bool wholeWords)
        {
            string rp = Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".");
            if (matchPrefix) rp = "^" + rp;
            if (matchSuffix) rp += "$";
            if (wholeWords) rp = $@"\b{rp}\b";
            return rp;
        }
        private Regex GetOrCreateRegex(string pattern, bool matchCase)
        {
            string key = $"{pattern}|{matchCase}";
            if (!_regexCache.TryGetValue(key, out Regex? regex))
            {
                var options = matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
                regex = new Regex(pattern, options | RegexOptions.Compiled);
                _regexCache[key] = regex;
            }
            return regex;
        }
        private void EnsureRegexCompiled()
        {
            if (!_isRegexDirty) return;
            _compiledRegexes = new Dictionary<string, Regex>();
            if (_filterConfig.Advanced && (_filterConfig.Wildcards || _filterConfig.MatchWhole))
            {
                CompileIfNotEmpty(CurrentText.Name);
                CompileIfNotEmpty(CurrentText.Desc);
                CompileIfNotEmpty(CurrentText.Str);
            }
            _isRegexDirty = false;

            void CompileIfNotEmpty(string value)
            {
                if (string.IsNullOrWhiteSpace(value)) return;
                string normalized = NormalizeString(value);
                string rp = _filterConfig.Wildcards
                    ? BuildRegexPattern(normalized, _filterConfig.Prefix, _filterConfig.Suffix, _filterConfig.MatchWhole)
                    : $@"\b{Regex.Escape(normalized)}\b";
                _compiledRegexes[value] = GetOrCreateRegex(rp, _filterConfig.MatchCase);
            }
        }
        #endregion

        private CancellationTokenSource? _filterCts;
        private SemaphoreSlim _filterLock = new SemaphoreSlim(1, 1);
        public async Task<List<CardText>?> ApplyFilterAsync()
        {
            //if (!_isFilterDirty) return new List<CardText>();
            //_isFilterDirty = false;
            EnsureRegexCompiled();

            _filterCts?.Cancel();
            _filterCts = new CancellationTokenSource();
            var token = _filterCts.Token;

            if (!await _filterLock.WaitAsync(2000)) return null;

            try
            {
                CurrentData = _filterState.FilterCardData;
                CurrentText = _filterState.FilterCardText;
                CurrentIsLink = _filterState.Link;

                return await Task.Run(() =>
                {
                    List<ulong> foundIds;

                    if (CurrentData.IsDefault())
                    {
                        foundIds = _cardStore.CardTexts.Keys.ToList();
                    }
                    else
                    {
                        foundIds = GetFoundIdsByData(token);
                    }

                    token.ThrowIfCancellationRequested();
                    return GetFoundCardsByText(foundIds, token);
                }, token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                _filterLock.Release();
            }
        }

        #endregion

        #region Get Data
        public CardData? GetCardDataByID(ulong cardID)
        {
            if (_cardStore.CardDatas.TryGetValue(cardID, out var datas)) return datas;
            return null;
        }
        public IReadOnlyList<CardData>? GetListCardDataByAlias(ulong alias)
        {
            if (_cardStore.AliasIndex.TryGetValue(alias, out var list)) return list;
            return null;
        }
        public IReadOnlyList<CardText>? GetListCardTextByID(ulong cardID)
        {
            if (_cardStore.CardTexts.TryGetValue(cardID, out var texts)) return texts;
            return null;
        }
        public List<ulong> GetListIDByID(ulong id)
        {
            var result = new List<ulong>();

            if (_cardStore.CardDatas.ContainsKey(id)) result.Add(id);
            if (_cardStore.AliasIndex.TryGetValue(id, out var aliasList))
            {
                foreach (var card in aliasList)
                {
                    result.Add(card.id);
                }
            }
            return result;
        }
        #endregion
    }

    internal static class SqliteReaderExtensions
    {
        public static ulong GetULong(this SqliteDataReader r, int i)
        {
            if (r.IsDBNull(i)) return 0UL;
            object v = r.GetValue(i);
            return v switch
            {
                long l => (ulong)l,
                int n => (ulong)n,
                ulong ul => ul,
                string s when ulong.TryParse(s, out var parsed) => parsed,
                _ => 0UL
            };
        }
        public static long GetLong(this SqliteDataReader r, int i)
        {
            if (r.IsDBNull(i)) return 0L;
            object v = r.GetValue(i);
            return v switch
            {
                long l => l,
                int n => n,
                string s when long.TryParse(s, out var parsed) => parsed,
                _ => 0L
            };
        }
        public static string GetStringSafe(this SqliteDataReader r, int i) => r.IsDBNull(i)
            ? string.Empty : r.GetString(i);
    }
}
