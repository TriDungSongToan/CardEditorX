using System;
using System.IO;
using System.IO.Compression;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using ClosedXML.Excel;
using CardEditor.Models;
using CardEditor.Constants;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services
{
    public static class SaveDatabaseServices
    {
        private static async Task<bool> CardExistsAsync(SQLiteConnection connection, ulong id, SQLiteTransaction transaction)
        {
            using (SQLiteCommand cmd = new SQLiteCommand("SELECT 1 FROM datas WHERE id = @id LIMIT 1", connection, transaction))
            {
                cmd.Parameters.AddWithValue("@id", (long)id);
                var result = await cmd.ExecuteScalarAsync();
                return result != null;
            }
        }

        private static void AddTextsParameters(SQLiteCommand cmd, Card card)
        {
            cmd.Parameters.AddWithValue("@id", (long)card.id);
            cmd.Parameters.AddWithValue("@name", card.name);
            cmd.Parameters.AddWithValue("@desc", card.desc);
            cmd.Parameters.AddWithValue("@str1", card.str1);
            cmd.Parameters.AddWithValue("@str2", card.str2);
            cmd.Parameters.AddWithValue("@str3", card.str3);
            cmd.Parameters.AddWithValue("@str4", card.str4);
            cmd.Parameters.AddWithValue("@str5", card.str5);
            cmd.Parameters.AddWithValue("@str6", card.str6);
            cmd.Parameters.AddWithValue("@str7", card.str7);
            cmd.Parameters.AddWithValue("@str8", card.str8);
            cmd.Parameters.AddWithValue("@str9", card.str9);
            cmd.Parameters.AddWithValue("@str10", card.str10);
            cmd.Parameters.AddWithValue("@str11", card.str11);
            cmd.Parameters.AddWithValue("@str12", card.str12);
            cmd.Parameters.AddWithValue("@str13", card.str13);
            cmd.Parameters.AddWithValue("@str14", card.str14);
            cmd.Parameters.AddWithValue("@str15", card.str15);
            cmd.Parameters.AddWithValue("@str16", card.str16);
        }
        private static void AddDatasParameters(SQLiteCommand cmd, Card card, bool hasFlag)
        {
            cmd.Parameters.AddWithValue("@id", (long)card.id);
            cmd.Parameters.AddWithValue("@ot", (long)card.ot);
            cmd.Parameters.AddWithValue("@alias", (long)card.alias);
            cmd.Parameters.AddWithValue("@setcode", (long)card.setcode);
            cmd.Parameters.AddWithValue("@type", (long)card.type);
            cmd.Parameters.AddWithValue("@atk", card.atk);
            cmd.Parameters.AddWithValue("@def", card.def);
            cmd.Parameters.AddWithValue("@level", (long)card.level);
            cmd.Parameters.AddWithValue("@race", (long)card.race);
            cmd.Parameters.AddWithValue("@attribute", (long)card.attribute);
            cmd.Parameters.AddWithValue("@category", (long)card.category);

            // Chỉ bind @flag khi database thực sự có cột này
            if (hasFlag)
                cmd.Parameters.AddWithValue("@flag", (long)card.flag);
        }

        private const string InsertTextsSql = @"
            INSERT INTO texts (id, name, desc, str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12, str13, str14, str15, str16)
            VALUES (@id, @name, @desc, @str1, @str2, @str3, @str4, @str5, @str6, @str7, @str8, @str9, @str10, @str11, @str12, @str13, @str14, @str15, @str16)";
        private const string UpdateTextsSql = @"
            UPDATE texts SET name=@name, desc=@desc, str1=@str1, str2=@str2, str3=@str3, str4=@str4, str5=@str5,
                str6=@str6, str7=@str7, str8=@str8, str9=@str9, str10=@str10, str11=@str11, str12=@str12,
                str13=@str13, str14=@str14, str15=@str15, str16=@str16
            WHERE id=@id";
        private const string UpsertTextsSql = @"
            INSERT INTO texts (id, name, desc, str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12, str13, str14, str15, str16)
            VALUES (@id, @name, @desc, @str1, @str2, @str3, @str4, @str5, @str6, @str7, @str8, @str9, @str10, @str11, @str12, @str13, @str14, @str15, @str16)
            ON CONFLICT(id) DO UPDATE SET
                name=excluded.name, desc=excluded.desc, str1=excluded.str1, str2=excluded.str2, str3=excluded.str3,
                str4=excluded.str4, str5=excluded.str5, str6=excluded.str6, str7=excluded.str7, str8=excluded.str8,
                str9=excluded.str9, str10=excluded.str10, str11=excluded.str11, str12=excluded.str12,
                str13=excluded.str13, str14=excluded.str14, str15=excluded.str15, str16=excluded.str16";

        private static string BuildInsertDatasSql(bool hasFlag)
        {
            string cols = "id, ot, alias, setcode, type, atk, def, level, race, attribute, category" + (hasFlag ? ", flag" : "");
            string vals = "@id, @ot, @alias, @setcode, @type, @atk, @def, @level, @race, @attribute, @category" + (hasFlag ? ", @flag" : "");
            return $"INSERT INTO datas ({cols}) VALUES ({vals})";
        }
        private static string BuildUpdateDatasSql(bool hasFlag)
        {
            string flagSet = hasFlag ? ", flag=@flag" : "";
            return $@"
                UPDATE datas SET ot=@ot, alias=@alias, setcode=@setcode, type=@type, atk=@atk, def=@def,
                    level=@level, race=@race, attribute=@attribute, category=@category{flagSet}
                WHERE id=@id";
        }
        private static string BuildUpsertDatasSql(bool hasFlag)
        {
            string cols = "id, ot, alias, setcode, type, atk, def, level, race, attribute, category" + (hasFlag ? ", flag" : "");
            string vals = "@id, @ot, @alias, @setcode, @type, @atk, @def, @level, @race, @attribute, @category" + (hasFlag ? ", @flag" : "");
            string updateSet = "ot=excluded.ot, alias=excluded.alias, setcode=excluded.setcode, type=excluded.type, atk=excluded.atk, def=excluded.def, level=excluded.level, race=excluded.race, attribute=excluded.attribute, category=excluded.category" + (hasFlag ? ", flag=excluded.flag" : "");
            return $@"
                INSERT INTO datas ({cols}) VALUES ({vals})
                ON CONFLICT(id) DO UPDATE SET {updateSet}";
        }
        private static async Task<bool> ResolveHasFlagAsync(SQLiteConnection connection, bool? hasFlag)
        {
            if (hasFlag.HasValue) return hasFlag.Value;
            return await ColumnExistsAsync(connection, "datas", "flag");
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

        public static async Task<(bool, string)> AddNewCard(Card newCard, string dbFilePath, bool? hasFlag = null)
        {
            if (newCard == null) return (false, "Card is null");
            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath)) return (false, CMess.fileNotExit.ToText());

            string connectionString = $"Data Source={dbFilePath};Version=3;";
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    bool resolvedHasFlag = await ResolveHasFlagAsync(connection, hasFlag);

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            if (await CardExistsAsync(connection, newCard.id, transaction))
                            {
                                transaction.Rollback();
                                return (false, CMess.cardIDExist.ToText());
                            }

                            using (SQLiteCommand cmd = new SQLiteCommand(InsertTextsSql, connection, transaction))
                            {
                                AddTextsParameters(cmd, newCard);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            using (SQLiteCommand cmd = new SQLiteCommand(BuildInsertDatasSql(resolvedHasFlag), connection, transaction))
                            {
                                AddDatasParameters(cmd, newCard, resolvedHasFlag);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();
                            return (true, string.Empty);
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                return (false, $"{CMess.errorConDB.ToText()} {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"{CMess.errorOcc.ToText()} {ex.Message}");
            }
        }
        public static async Task<(bool, string)> ModifyCurrentCard(Card currentCard, string dbFilePath, bool? hasFlag = null)
        {
            if (currentCard == null) return (false, "Card is null");
            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath)) return (false, CMess.fileNotExit.ToText());

            string connectionString = $"Data Source={dbFilePath};Version=3;";
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    bool resolvedHasFlag = await ResolveHasFlagAsync(connection, hasFlag);

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            if (!await CardExistsAsync(connection, currentCard.id, transaction))
                            {
                                transaction.Rollback();
                                return (false, CMess.cardIDNotExist.ToText());
                            }

                            using (SQLiteCommand cmd = new SQLiteCommand(UpdateTextsSql, connection, transaction))
                            {
                                AddTextsParameters(cmd, currentCard);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            using (SQLiteCommand cmd = new SQLiteCommand(BuildUpdateDatasSql(resolvedHasFlag), connection, transaction))
                            {
                                AddDatasParameters(cmd, currentCard, resolvedHasFlag);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();
                            return (true, string.Empty);
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                return (false, $"{CMess.errorConDB.ToText()} {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"{CMess.errorOcc.ToText()} {ex.Message}");
            }
        }
        public static async Task<(bool, string)> DeleteCard(ulong cardId, string dbFilePath)
        {
            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath))
                return (false, CMess.fileNotExit.ToText());

            string connectionString = $"Data Source={dbFilePath};Version=3;";

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            if (!await CardExistsAsync(connection, cardId, transaction))
                            {
                                transaction.Rollback();
                                return (false, CMess.cardIDNotExist.ToText());
                            }

                            using (SQLiteCommand textsCmd = new SQLiteCommand("DELETE FROM texts WHERE id = @id", connection, transaction))
                            {
                                textsCmd.Parameters.AddWithValue("@id", (long)cardId);
                                await textsCmd.ExecuteNonQueryAsync();
                            }
                            using (SQLiteCommand datasCmd = new SQLiteCommand("DELETE FROM datas WHERE id = @id", connection, transaction))
                            {
                                datasCmd.Parameters.AddWithValue("@id", (long)cardId);
                                await datasCmd.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();
                            return (true, string.Empty);
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                return (false, $"{CMess.errorConDB.ToText()} {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"{CMess.errorOcc.ToText()} {ex.Message}");
            }
        }

        public static async Task<(bool, string)> SaveCardList(List<Card> cardList, string dbFilePath, bool? hasFlag = null)
        {
            if (cardList == null) return (false, "Card list is null");
            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath)) return (false, CMess.fileNotExit.ToText());

            string connectionString = $"Data Source={dbFilePath};Version=3;";
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    bool resolvedHasFlag = await ResolveHasFlagAsync(connection, hasFlag);

                    using (SQLiteTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string datasSql = BuildUpsertDatasSql(resolvedHasFlag);

                            using (SQLiteCommand textsCmd = new SQLiteCommand(UpsertTextsSql, connection, transaction))
                            using (SQLiteCommand datasCmd = new SQLiteCommand(datasSql, connection, transaction))
                            {
                                foreach (var card in cardList)
                                {
                                    textsCmd.Parameters.Clear();
                                    AddTextsParameters(textsCmd, card);
                                    await textsCmd.ExecuteNonQueryAsync();

                                    datasCmd.Parameters.Clear();
                                    AddDatasParameters(datasCmd, card, resolvedHasFlag);
                                    await datasCmd.ExecuteNonQueryAsync();
                                }
                            }

                            transaction.Commit();
                            return (true, string.Empty);
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                return (false, $"{CMess.errorConDB.ToText()} {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"{CMess.errorOcc.ToText()} {ex.Message}");
            }
        }

    }

    public static class SaveExcelService
    {
        private static string[] GetHeaders(bool hasFlag) => hasFlag ? ConstantColumnExcel.HeaderWithFlag.ToArray() : ConstantColumnExcel.HeaderNoFlag.ToArray();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new(StringComparer.OrdinalIgnoreCase);
        private static SemaphoreSlim GetLock(string filePath) => _fileLocks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));

        private static void WriteCardToRow(IXLWorksheet ws, int row, Card card, bool hasFlag)
        {
            ws.Cell(row, 1).SetValue(card.id.ToString());
            ws.Cell(row, 2).SetValue(card.name);
            ws.Cell(row, 3).SetValue(card.desc);
            ws.Cell(row, 4).SetValue(card.ot.ToString());
            ws.Cell(row, 5).SetValue(card.alias.ToString());
            ws.Cell(row, 6).SetValue(card.setcode.ToString());
            ws.Cell(row, 7).SetValue(card.type.ToString());
            ws.Cell(row, 8).SetValue(card.atk.ToString());
            ws.Cell(row, 9).SetValue(card.def.ToString());
            ws.Cell(row, 10).SetValue(card.level.ToString());
            ws.Cell(row, 11).SetValue(card.race.ToString());
            ws.Cell(row, 12).SetValue(card.attribute.ToString());
            ws.Cell(row, 13).SetValue(card.category.ToString());

            int strOffset = 13;
            if (hasFlag)
            {
                ws.Cell(row, 14).SetValue(card.flag.ToString());
                strOffset = 14;
            }

            ws.Cell(row, strOffset + 1).SetValue(card.str1);
            ws.Cell(row, strOffset + 2).SetValue(card.str2);
            ws.Cell(row, strOffset + 3).SetValue(card.str3);
            ws.Cell(row, strOffset + 4).SetValue(card.str4);
            ws.Cell(row, strOffset + 5).SetValue(card.str5);
            ws.Cell(row, strOffset + 6).SetValue(card.str6);
            ws.Cell(row, strOffset + 7).SetValue(card.str7);
            ws.Cell(row, strOffset + 8).SetValue(card.str8);
            ws.Cell(row, strOffset + 9).SetValue(card.str9);
            ws.Cell(row, strOffset + 10).SetValue(card.str10);
            ws.Cell(row, strOffset + 11).SetValue(card.str11);
            ws.Cell(row, strOffset + 12).SetValue(card.str12);
            ws.Cell(row, strOffset + 13).SetValue(card.str13);
            ws.Cell(row, strOffset + 14).SetValue(card.str14);
            ws.Cell(row, strOffset + 15).SetValue(card.str15);
            ws.Cell(row, strOffset + 16).SetValue(card.str16);
        }

        private static int FindRowById(IXLWorksheet ws, ulong id, int lastRow)
        {
            for (int row = 2; row <= lastRow; row++)
            {
                var cell = ws.Cell(row, 1);
                if (!cell.IsEmpty() && ulong.TryParse(cell.GetString(), out ulong rowId) && rowId == id)
                    return row;
            }
            return -1;
        }

        // Mở file có sẵn, hoặc tạo mới nếu chưa tồn tại, kèm ghi header đúng theo hasFlag
        private static (XLWorkbook workbook, IXLWorksheet worksheet, bool hasFlag) OpenOrCreate(string filePath, bool? hasFlagHint)
        {
            if (File.Exists(filePath))
            {
                var workbook = new XLWorkbook(filePath);
                var ws = workbook.Worksheets.FirstOrDefault();
                var (isValid, hasFlag) = CheckDatabase.CheckExcelValidity(ws);
                if (!isValid)
                {
                    workbook.Dispose();
                    throw new InvalidOperationException(CMess.TwoPlaceholderInva.ToText());
                }
                return (workbook, ws!, hasFlag);
            }
            else
            {
                var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Cards");
                bool hasFlag = hasFlagHint ?? false;
                var headers = GetHeaders(hasFlag);
                for (int col = 0; col < headers.Length; col++)
                    ws.Cell(1, col + 1).SetValue(headers[col]);
                return (workbook, ws, hasFlag);
            }
        }

        public static async Task<(bool, string)> AddNewCard(Card newCard, string filePath, bool? hasFlagHint = null)
        {
            if (newCard == null) return (false, "Card is null");
            var fileLock = GetLock(filePath);
            await fileLock.WaitAsync();
            try
            {
                var (workbook, ws, hasFlag) = OpenOrCreate(filePath, hasFlagHint);
                using (workbook)
                {
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
                    if (lastRow >= 2 && FindRowById(ws, newCard.id, lastRow) != -1)
                        return (false, CMess.cardIDExist.ToText());

                    WriteCardToRow(ws, lastRow + 1, newCard, hasFlag);
                    await Task.Run(() => workbook.SaveAs(filePath));
                    return (true, string.Empty);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                fileLock.Release();
            }
        }
        public static async Task<(bool, string)> ModifyCurrentCard(Card currentCard, string filePath, bool? hasFlagHint = null)
        {
            if (currentCard == null) return (false, "Card is null");
            if (!File.Exists(filePath)) return (false, CMess.fileNotExit.ToText());
            var fileLock = GetLock(filePath);
            await fileLock.WaitAsync();
            try
            {
                var (workbook, ws, hasFlag) = OpenOrCreate(filePath, hasFlagHint);
                using (workbook)
                {
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
                    int row = FindRowById(ws, currentCard.id, lastRow);
                    if (row == -1) return (false, CMess.cardIDNotExist.ToText());

                    WriteCardToRow(ws, row, currentCard, hasFlag);
                    await Task.Run(() => workbook.SaveAs(filePath));
                    return (true, string.Empty);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                fileLock.Release();
            }
        }
        public static async Task<(bool, string)> DeleteCard(ulong cardId, string filePath, bool? hasFlagHint = null)
        {
            if (!File.Exists(filePath)) return (false, CMess.fileNotExit.ToText());

            var fileLock = GetLock(filePath);
            await fileLock.WaitAsync();

            try
            {
                var (workbook, ws, hasFlag) = OpenOrCreate(filePath, hasFlagHint);

                using (workbook)
                {
                    int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

                    int row = FindRowById(ws, cardId, lastRow);

                    if (row == -1) return (false, CMess.cardIDNotExist.ToText());

                    ws.Row(row).Delete();

                    await Task.Run(() => workbook.SaveAs(filePath));

                    return (true, string.Empty);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                fileLock.Release();
            }
        }

        public static async Task<(bool, string)> SaveCardList(List<Card> cardList, string filePath, bool hasFlag = false)
        {
            if (cardList == null) return (false, "Card list is null");
            var fileLock = GetLock(filePath);
            await fileLock.WaitAsync();
            try
            {
                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Cards");

                var headers = GetHeaders(hasFlag);
                for (int col = 0; col < headers.Length; col++)
                    ws.Cell(1, col + 1).SetValue(headers[col]);

                for (int i = 0; i < cardList.Count; i++)
                    WriteCardToRow(ws, i + 2, cardList[i], hasFlag);

                await Task.Run(() => workbook.SaveAs(filePath));
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                fileLock.Release();
            }
        }
    }

    public static class SaveCedsService
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

        private static async Task<(List<Card>, string)> LoadJsonInternal(string filePath)
        {
            if (!File.Exists(filePath)) return (new List<Card>(), string.Empty); // File chưa tồn tại -> coi như danh sách rỗng

            try
            {
                string json = await Task.Run(() => File.ReadAllText(filePath, Encoding.UTF8));
                var cards = JsonSerializer.Deserialize<List<Card>>(json, SerializerOptions) ?? new List<Card>();
                return (cards, string.Empty);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
        private static async Task<(bool, string)> WriteJsonInternal(List<Card> cards, string filePath)
        {
            string tempPath = filePath + ".tmp";
            try
            {
                string json = JsonSerializer.Serialize(cards, SerializerOptions);
                await Task.Run(() =>
                {
                    File.WriteAllText(tempPath, json, Encoding.UTF8);
                    File.Copy(tempPath, filePath, overwrite: true); // hoặc File.Move nếu không cần giữ file gốc lúc lỗi
                });
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        public static async Task<(bool, string)> AddNewCard(Card newCard, string filePath)
        {
            if (newCard == null) return (false, "Card is null");

            var (cards, error) = await LoadJsonInternal(filePath);
            if (cards == null) return (false, error);

            if (cards.Any(c => c.id == newCard.id))
                return (false, CMess.cardIDExist.ToText());

            cards.Add(newCard);
            return await WriteJsonInternal(cards, filePath);
        }
        public static async Task<(bool, string)> ModifyCurrentCard(Card currentCard, string filePath)
        {
            if (currentCard == null) return (false, "Card is null");

            var (cards, error) = await LoadJsonInternal(filePath);
            if (cards == null) return (false, error);

            int index = cards.FindIndex(c => c.id == currentCard.id);
            if (index == -1)
                return (false, CMess.cardIDNotExist.ToText());

            cards[index] = currentCard;
            return await WriteJsonInternal(cards, filePath);
        }
        public static async Task<(bool, string)> DeleteCard(ulong cardId, string filePath)
        {
            var (cards, error) = await LoadJsonInternal(filePath);

            if (cards == null) return (false, error);

            int index = cards.FindIndex(c => c.id == cardId);

            if (index == -1) return (false, CMess.cardIDNotExist.ToText());

            cards.RemoveAt(index);

            return await WriteJsonInternal(cards, filePath);
        }

        public static async Task<(bool, string)> SaveCardList(List<Card> cardList, string filePath)
        {
            if (cardList == null) return (false, "Card list is null");
            return await WriteJsonInternal(cardList, filePath);
        }
    }

    public static class SaveArchiveService
    {
        private static class ResourceIndexBuilder
        {
            private const int IMAGE_SEARCH_DEPTH = 4;
            private const int SCRIPT_SEARCH_DEPTH = 7;
            private static readonly string[] IMAGE_EXTENSIONS = { ".png", ".jpg", ".jpeg" };

            public class Index
            {
                public Dictionary<string, string> Images { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                public Dictionary<string, string> Scripts { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            public static Index Build(string rootDirectory)
            {
                var index = new Index();
                if (string.IsNullOrWhiteSpace(rootDirectory) || !Directory.Exists(rootDirectory)) return index;

                int maxDepth = Math.Max(IMAGE_SEARCH_DEPTH, SCRIPT_SEARCH_DEPTH); // 7, duyệt 1 lần cho cả 2 loại
                var folders = new Queue<(string path, int depth)>();
                folders.Enqueue((rootDirectory, 0));

                while (folders.Count > 0)
                {
                    var (currentDir, depth) = folders.Dequeue();
                    try
                    {
                        foreach (string file in Directory.EnumerateFiles(currentDir))
                        {
                            string ext = Path.GetExtension(file);

                            if (depth <= IMAGE_SEARCH_DEPTH &&
                                IMAGE_EXTENSIONS.Contains(ext, StringComparer.OrdinalIgnoreCase))
                            {
                                string key = Path.GetFileNameWithoutExtension(file);
                                if (!index.Images.ContainsKey(key)) index.Images[key] = file;
                            }

                            if (depth <= SCRIPT_SEARCH_DEPTH && ext.Equals(".lua", StringComparison.OrdinalIgnoreCase))
                            {
                                string key = Path.GetFileName(file); // "c{id}.lua"
                                if (!index.Scripts.ContainsKey(key)) index.Scripts[key] = file;
                            }
                        }

                        if (depth < maxDepth)
                        {
                            foreach (string subDir in Directory.EnumerateDirectories(currentDir))
                                folders.Enqueue((subDir, depth + 1));
                        }
                    }
                    catch (UnauthorizedAccessException) { continue; }
                    catch (Exception) { continue; }
                }
                return index;
            }

            public static (string ImagePath, string ScriptPath) Lookup(Index primary, Index fallback, string cardId)
            {
                string scriptKey = $"c{cardId}.lua";

                string imagePath = primary.Images.TryGetValue(cardId, out var img) ? img
                    : (fallback != null && fallback.Images.TryGetValue(cardId, out var img2)) ? img2 : null;

                string scriptPath = primary.Scripts.TryGetValue(scriptKey, out var scr) ? scr
                    : (fallback != null && fallback.Scripts.TryGetValue(scriptKey, out var scr2)) ? scr2 : null;

                return (imagePath, scriptPath);
            }
        }

        public static async Task<(bool, string)> SaveCompressed(List<CardEditor.Models.Card> cardList, string targetPath, string? dbFilePath = null)
        {
            if (cardList == null || !cardList.Any()) return (false, CMess.noCardExport.ToText());
            if (string.IsNullOrEmpty(targetPath)) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText()));

            string outPutCdbPath = string.Empty;

            try
            {
                string directoryZIPPath = System.IO.Path.GetDirectoryName(targetPath);
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(targetPath);
                string fileWithCdbExtension = System.IO.Path.Combine(directoryZIPPath, fileNameWithoutExtension + ".cdb");

                if (System.IO.File.Exists(fileWithCdbExtension)) return (false, $"{CMess.File.ToText()} {fileWithCdbExtension} {CMess.filealreadyExit.ToText()}");

                var (resultCreate, messageCreate) = CreateFileServices.CreateDatabase(directoryZIPPath, $"{fileNameWithoutExtension}.cdb");
                if (resultCreate) outPutCdbPath = messageCreate;
                else return (false, messageCreate);

                if (string.IsNullOrEmpty(outPutCdbPath)) return (false, string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText()));

                await SaveDatabaseServices.SaveCardList(cardList, outPutCdbPath).ConfigureAwait(false);

                // === Build index 1 lần, thay cho việc gọi FindImagePath/FindScriptPath cho từng card ===
                string? resourceDirectory = GetResourceDirectory(dbFilePath);

                // Tái tạo đúng bước Directory.GetParent(dataPath) bên trong FindImagePath/FindScriptPath gốc
                string? primaryRoot = null;
                if (!string.IsNullOrWhiteSpace(resourceDirectory) && Directory.Exists(resourceDirectory))
                    primaryRoot = Directory.GetParent(resourceDirectory)?.FullName;

                var primaryIndex = ResourceIndexBuilder.Build(primaryRoot);

                var cardIds = cardList.Select(c => c.id.ToString()).ToList();

                bool needFallback = cardIds.Any(id =>
                    !primaryIndex.Images.ContainsKey(id) && !primaryIndex.Scripts.ContainsKey($"c{id}.lua"));

                ResourceIndexBuilder.Index fallbackIndex = null;
                if (needFallback)
                {
                    string settingsPath = ConfigViewModel.Instance.userSetting.DataSource;
                    if (!string.IsNullOrWhiteSpace(settingsPath) && Directory.Exists(settingsPath) && settingsPath != primaryRoot)
                        fallbackIndex = ResourceIndexBuilder.Build(settingsPath);
                }

                var fileGroups = cardIds
                    .Select(id => ResourceIndexBuilder.Lookup(primaryIndex, fallbackIndex, id))
                    .Where(x => !string.IsNullOrEmpty(x.ImagePath) || !string.IsNullOrEmpty(x.ScriptPath))
                    .ToList();

                using (var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(outPutCdbPath, Path.GetFileName(outPutCdbPath), System.IO.Compression.CompressionLevel.Fastest);

                    foreach (var files in fileGroups)
                    {
                        if (!string.IsNullOrEmpty(files.ImagePath))
                            zip.CreateEntryFromFile(files.ImagePath, $"pics/{Path.GetFileName(files.ImagePath)}", System.IO.Compression.CompressionLevel.Fastest);

                        if (!string.IsNullOrEmpty(files.ScriptPath))
                            zip.CreateEntryFromFile(files.ScriptPath, $"script/{Path.GetFileName(files.ScriptPath)}", System.IO.Compression.CompressionLevel.Fastest);
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                TryDeleteFile(outPutCdbPath);
            }
        }

        private static string? GetResourceDirectory(string? dbFilePath)
        {
            if (!string.IsNullOrWhiteSpace(dbFilePath))
            {
                string? directory = Path.GetDirectoryName(dbFilePath);
                if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory)) return directory;
                return null;
            }
            string? dataSource = ConfigViewModel.Instance.userSetting.DataSource;
            if (string.IsNullOrWhiteSpace(dataSource)) return null;
            if (!Directory.Exists(dataSource)) return null;
            return dataSource;
        }
        private static void TryDeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;
            try
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not delete temporary file: {filePath}. {ex}");
            }
        }
    }
}
