using System;
using System.Linq;
using System.Data.SQLite;
using System.Collections.Generic;
using ClosedXML.Excel;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Constants;

namespace CardEditor.Services
{
    public static class CheckDatabase
    {
        public static CheckDatabaseResult CheckDatabaseValidity(SQLiteConnection connection)
        {
            var result = new CheckDatabaseResult();

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
                result.DBType = DatabaseType.OMEGA;
                result.HasFlag = true;

                return result;
            }

            // 4. YGO
            if (!CheckTableStructure(connection, "datas", ConstantColumnDatabase.DataTableYGOColumn))
            {
                return result;
            }

            result.Result = true;
            result.DBType = DatabaseType.YGO;
            result.HasFlag = CheckTableStructure(connection, "datas", ConstantColumnDatabase.DataTableFlagColumn);

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

        public static (bool Success, bool HasFlag) CheckExcelValidity(IXLWorksheet worksheet)
        {
            if (worksheet == null) return (false, false);
            if (IsHeaderMatch(worksheet, ConstantColumnExcel.HeaderWithFlag)) return (true, true);
            if (IsHeaderMatch(worksheet, ConstantColumnExcel.HeaderNoFlag)) return (true, false);
            return (false, false);
        }
        private static bool IsHeaderMatch(IXLWorksheet worksheet, IReadOnlyList<string> expectedHeaders)
        {
            for (int col = 1; col <= expectedHeaders.Count; col++)
            {
                var header = worksheet.Cell(1, col).GetString();
                if (!string.Equals(header, expectedHeaders[col - 1], StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        }

    }
}
