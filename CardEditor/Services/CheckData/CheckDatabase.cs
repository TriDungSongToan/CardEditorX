using System;
using System.Linq;
using System.Data.SQLite;
using System.Collections.Generic;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Constants;

namespace CardEditor.Services.CheckData
{
    public static class CheckDatabase
    {
        /// <summary>
        /// Check whether the database file structure meets the requirements.
        /// Database will first be checked against OMEGA structure to ensure safety.
        /// Afterwards, database will be checked using "YGO has flag" structure. (`flag` column (INTEGER) is placed in the `datas` table, after `category`.)
        /// Finally, the database is checked against the "YGO no flag" structure.
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static CheckCardListResult CheckDatabaseCardListValidity(SQLiteConnection connection)
        {
            var result = new CheckCardListResult();

            // 1. Required tables
            foreach (var table in ConstantColumnDatabase.DatabaseTable)
            {
                if (!TableExists(connection, table)) return result;
            }

            // 2. texts structure
            if (!CheckTableStructure(connection, "texts", ConstantColumnDatabase.TextsTableColumn))
            {
                return result;
            }

            // 3. OMEGA
            if (CheckTableStructure(connection, "datas", ConstantColumnDatabase.DataTableOMegaColumn))
            {
                result.Result = true;
                result.Format = CardListFormat.OMEGA;
                result.HasFlag = true;

                return result;
            }

            // 4. YGO
            if (!CheckTableStructure(connection, "datas", ConstantColumnDatabase.DataTableYGOColumn))
            {
                return result;
            }

            result.Result = true;
            result.HasFlag = CheckTableStructure(connection, "datas", ConstantColumnDatabase.DataTableFlagColumn);
            result.Format = result.HasFlag ? CardListFormat.YGOHasFlag : CardListFormat.YGONoFlag;

            return result;
        }
        public static bool CheckDatabaseListNameValidity(SQLiteConnection connection)
        {
            var tableName = ConstantColumnDatabase.DatabaseTable.First();

            if (!TableExists(connection, tableName)) return false;
            return CheckTableStructure(connection, tableName, ConstantColumnDatabase.ListNameColumn);
        }


        // Hàm kiểm tra sự tồn tại của bảng (bỏ qua Indices, Views, Triggers)
        private static bool TableExists(SQLiteConnection connection, string tableName)
        {
            const string query = @"SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = @tableName LIMIT 1";

            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@tableName", tableName);

            return cmd.ExecuteScalar() != null;
        }
        // Hàm kiểm tra cấu trúc bảng
        private static bool CheckTableStructure(SQLiteConnection connection, string tableName, HashSet<string> expectedColumns)
        {
            using (var cmd = new SQLiteCommand($"PRAGMA table_info({tableName})", connection))
            using (var reader = cmd.ExecuteReader())
            {
                var actualColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                while (reader.Read())
                {
                    actualColumns.Add(reader["name"].ToString());
                }

                return expectedColumns.IsSubsetOf(actualColumns);
            }
        }
    }
}
