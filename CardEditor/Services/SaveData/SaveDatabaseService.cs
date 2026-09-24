using System;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.SaveData
{
    public class SaveDatabaseYGOService
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

        public static async Task<WriteResult> AddNewCard(Card newCard, string dbFilePath, bool? hasFlag = null)
        {
            WriteResult result = new();
            if (newCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }
            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath))
            {
                result.Result = false;
                result.Messenger = CMess.fileNotExit.ToText();
                return result;
            }

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
                                result.Result = false;
                                result.Messenger = CMess.cardIDExist.ToText();
                                return result;
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

                            result.Result = true;
                            result.Messenger = string.Empty;
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
                result.Result = false;
                result.Messenger = $"{CMess.errorConDB.ToText()} {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static async Task<WriteResult> ModifyCurrentCard(Card currentCard, string dbFilePath, bool? hasFlag = null)
        {
            WriteResult result = new();
            if (currentCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }
            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath))
            {
                result.Result = false;
                result.Messenger = CMess.fileNotExit.ToText();
                return result;
            }

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
                                result.Result = false;
                                result.Messenger = CMess.cardIDNotExist.ToText();
                                return result;
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

                            result.Result = true;
                            result.Messenger = string.Empty;
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
                result.Result = false;
                result.Messenger = $"{CMess.errorConDB.ToText()} {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static async Task<WriteResult> DeleteCard(ulong cardId, string dbFilePath)
        {
            WriteResult result = new();

            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath))
            {
                result.Result = false;
                result.Messenger = CMess.fileNotExit.ToText();
                return result;
            }

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

                                result.Result = false;
                                result.Messenger = CMess.cardIDNotExist.ToText();
                                return result;
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

                            result.Result = true;
                            result.Messenger = string.Empty;
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
                result.Result = false;
                result.Messenger = $"{CMess.errorConDB.ToText()} {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }

        public static async Task<WriteResult> SaveCardList(List<Card> cardList, string dbFilePath, bool? hasFlag = null)
        {
            WriteResult result = new();

            if (cardList == null)
            {
                result.Result = false;
                result.Messenger = "Card list is null";
            }
            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath))
            {
                result.Result = false;
                result.Messenger = CMess.fileNotExit.ToText();
                return result;
            }

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

                            result.Result = true;
                            result.Messenger = string.Empty;
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
                result.Result = false;
                result.Messenger = $"{CMess.errorConDB.ToText()} {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
    }

    public class SaveDatabaseOMEGAService
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

        private static void AddTextsParameters(SQLiteCommand cmd, CardOmega card)
        {
            cmd.Parameters.AddWithValue("@id", (long)card.id);
            cmd.Parameters.AddWithValue("@name", card.name ?? string.Empty);
            cmd.Parameters.AddWithValue("@desc", card.desc ?? string.Empty);
            cmd.Parameters.AddWithValue("@str1", card.str1 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str2", card.str2 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str3", card.str3 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str4", card.str4 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str5", card.str5 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str6", card.str6 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str7", card.str7 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str8", card.str8 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str9", card.str9 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str10", card.str10 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str11", card.str11 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str12", card.str12 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str13", card.str13 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str14", card.str14 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str15", card.str15 ?? string.Empty);
            cmd.Parameters.AddWithValue("@str16", card.str16 ?? string.Empty);
        }
        private static void AddDatasParameters(SQLiteCommand cmd, CardOmega card)
        {
            cmd.Parameters.AddWithValue("@id", (long)card.id);
            cmd.Parameters.AddWithValue("@ot", (long)card.ot);
            cmd.Parameters.AddWithValue("@alias", (long)card.alias);

            var setcodeBlob = WriteDatabaseHelper.SetCodeToBlob(card.setcode);
            cmd.Parameters.Add("@setcode", DbType.Binary).Value = (object?)setcodeBlob ?? DBNull.Value;

            cmd.Parameters.AddWithValue("@type", (long)card.type);
            cmd.Parameters.AddWithValue("@atk", card.atk);
            cmd.Parameters.AddWithValue("@def", card.def);
            cmd.Parameters.AddWithValue("@level", (long)card.level);
            cmd.Parameters.AddWithValue("@race", (long)card.race);
            cmd.Parameters.AddWithValue("@attribute", (long)card.attribute);
            cmd.Parameters.AddWithValue("@category", (long)card.category);
            cmd.Parameters.AddWithValue("@genre", (long)card.genre);

            var scriptBlob = WriteDatabaseHelper.ScriptToBlob(card.script);
            cmd.Parameters.Add("@script", DbType.Binary).Value = (object?)scriptBlob ?? DBNull.Value;
            var supportBlob = WriteDatabaseHelper.SupportToBlob(card.support);
            cmd.Parameters.Add("@support", DbType.Binary).Value = (object?)supportBlob ?? DBNull.Value;
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

        private static string BuildInsertDatasSql()
        {
            string cols = "id, ot, alias, setcode, type, atk, def, level, race, attribute, category, genre, script, support";
            string vals = "@id, @ot, @alias, @setcode, @type, @atk, @def, @level, @race, @attribute, @category, @genre, @script, @support";
            return $"INSERT INTO datas ({cols}) VALUES ({vals})";
        }
        private static string BuildUpdateDatasSql()
        {
            return $@"
                UPDATE datas SET
                    ot=@ot, alias=@alias, setcode=@setcode, type=@type, atk=@atk, def=@def,
                    level=@level, race=@race, attribute=@attribute, category=@category,
                    genre=@genre, script=@script, support=@support
                WHERE id=@id";
        }
        private static string BuildUpsertDatasSql()
        {
            string cols = "id, ot, alias, setcode, type, atk, def, level, race, attribute, category, genre, script, support";
            string vals = "@id, @ot, @alias, @setcode, @type, @atk, @def, @level, @race, @attribute, @category, @genre, @script, @support";
            string updateSet = "ot=excluded.ot, alias=excluded.alias, setcode=excluded.setcode, type=excluded.type," +
                "atk=excluded.atk, def=excluded.def, level=excluded.level, race=excluded.race, attribute=excluded.attribute, category=excluded.category," +
                "genre=excluded.genre, script=excluded.script, support=excluded.support";
            return $@"
            INSERT INTO datas ({cols}) VALUES ({vals})
            ON CONFLICT(id) DO UPDATE SET {updateSet}";
        }

        public static async Task<WriteResult> AddNewCard(CardOmega newCard, string dbFilePath, bool? hasFlag = null)
        {
            WriteResult result = new();
            if (newCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }
            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath))
            {
                result.Result = false;
                result.Messenger = CMess.fileNotExit.ToText();
                return result;
            }

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
                            if (await CardExistsAsync(connection, newCard.id, transaction))
                            {
                                transaction.Rollback();
                                result.Result = false;
                                result.Messenger = CMess.cardIDExist.ToText();
                                return result;
                            }

                            using (SQLiteCommand cmd = new SQLiteCommand(InsertTextsSql, connection, transaction))
                            {
                                AddTextsParameters(cmd, newCard);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            using (SQLiteCommand cmd = new SQLiteCommand(BuildInsertDatasSql(), connection, transaction))
                            {
                                AddDatasParameters(cmd, newCard);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();

                            result.Result = true;
                            result.Messenger = string.Empty;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                result.Result = false;
                result.Messenger = $"{CMess.errorConDB.ToText()} {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static async Task<WriteResult> ModifyCurrentCard(CardOmega currentCard, string dbFilePath, bool? hasFlag = null)
        {
            WriteResult result = new();
            if (currentCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }
            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath))
            {
                result.Result = false;
                result.Messenger = CMess.fileNotExit.ToText();
                return result;
            }

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
                            if (!await CardExistsAsync(connection, currentCard.id, transaction))
                            {
                                transaction.Rollback();
                                result.Result = false;
                                result.Messenger = CMess.cardIDNotExist.ToText();
                                return result;
                            }

                            using (SQLiteCommand cmd = new SQLiteCommand(UpdateTextsSql, connection, transaction))
                            {
                                AddTextsParameters(cmd, currentCard);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            using (SQLiteCommand cmd = new SQLiteCommand(BuildUpdateDatasSql(), connection, transaction))
                            {
                                AddDatasParameters(cmd, currentCard);
                                await cmd.ExecuteNonQueryAsync();
                            }

                            transaction.Commit();

                            result.Result = true;
                            result.Messenger = string.Empty;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                result.Result = false;
                result.Messenger = $"{CMess.errorConDB.ToText()} {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static async Task<WriteResult> DeleteCard(ulong cardId, string dbFilePath)
        {
            WriteResult result = new();

            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath))
            {
                result.Result = false;
                result.Messenger = CMess.fileNotExit.ToText();
                return result;
            }

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

                                result.Result = false;
                                result.Messenger = CMess.cardIDNotExist.ToText();
                                return result;
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

                            result.Result = true;
                            result.Messenger = string.Empty;
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
                result.Result = false;
                result.Messenger = $"{CMess.errorConDB.ToText()} {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }

        public static async Task<WriteResult> SaveCardList(List<CardOmega> cardList, string dbFilePath, bool? hasFlag = null)
        {
            WriteResult result = new();

            if (cardList == null)
            {
                result.Result = false;
                result.Messenger = "Card list is null";
            }
            if (string.IsNullOrWhiteSpace(dbFilePath) || !File.Exists(dbFilePath))
            {
                result.Result = false;
                result.Messenger = CMess.fileNotExit.ToText();
                return result;
            }

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
                            string datasSql = BuildUpsertDatasSql();

                            using (SQLiteCommand textsCmd = new SQLiteCommand(UpsertTextsSql, connection, transaction))
                            using (SQLiteCommand datasCmd = new SQLiteCommand(datasSql, connection, transaction))
                            {
                                foreach (var card in cardList)
                                {
                                    textsCmd.Parameters.Clear();
                                    AddTextsParameters(textsCmd, card);
                                    await textsCmd.ExecuteNonQueryAsync();

                                    datasCmd.Parameters.Clear();
                                    AddDatasParameters(datasCmd, card);
                                    await datasCmd.ExecuteNonQueryAsync();
                                }
                            }

                            transaction.Commit();

                            result.Result = true;
                            result.Messenger = string.Empty;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (SQLiteException ex)
            {
                result.Result = false;
                result.Messenger = $"{CMess.errorConDB.ToText()} {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
    }
}
