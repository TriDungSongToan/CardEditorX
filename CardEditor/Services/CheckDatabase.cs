using System;
using System.Data.SQLite;
using System.Collections.Generic;
using OfficeOpenXml;

namespace CardEditor.Services
{
    public static class CheckDatabase
    {
        public static bool IsDatabaseFile(string filePath)
        {
            string[] dbExtensions = { ".ceds", ".xlsx", ".cdb", ".db", ".sqlite" };
            string fileExtension = System.IO.Path.GetExtension(filePath).ToLower();
            return Array.Exists(dbExtensions, ext => ext == fileExtension);
        }
        public static bool CheckDatabaseValidity(SQLiteConnection connection)
        {
            try
            {
                // Kiểm tra sự tồn tại của bảng "datas" và "texts"
                if (!TableExists(connection, "datas")) return false;
                if (!TableExists(connection, "texts")) return false;

                // Kiểm tra cấu trúc của bảng "datas"
                if (!CheckTableStructure(connection, "datas", new string[] { "id", "ot", "alias", "setcode", "type", "atk", "def", "level", "race", "attribute", "category" }))
                    return false;
                // Kiểm tra cấu trúc của bảng "texts"
                if (!CheckTableStructure(connection, "texts", new string[] { "id", "name", "desc", "str1", "str2", "str3", "str4", "str5", "str6", "str7", "str8", "str9", "str10", "str11", "str12", "str13", "str14", "str15", "str16" }))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Hàm kiểm tra sự tồn tại của bảng (bỏ qua Indices, Views, Triggers)
        private static bool TableExists(SQLiteConnection connection, string tableName)
        {
            using (var cmd = new SQLiteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'", connection))
            {
                var result = cmd.ExecuteScalar();
                return result != null;
            }
        }

        // Hàm kiểm tra cấu trúc bảng
        private static bool CheckTableStructure(SQLiteConnection connection, string tableName, string[] expectedColumns)
        {
            using (var cmd = new SQLiteCommand($"PRAGMA table_info({tableName})", connection))

            using (var reader = cmd.ExecuteReader())
            {
                var actualColumns = new List<string>();
                while (reader.Read())
                {
                    actualColumns.Add(reader["name"].ToString());
                }

                // Kiểm tra xem tất cả các cột mong đợi có mặt trong bảng không
                foreach (var column in expectedColumns)
                {
                    if (!actualColumns.Contains(column))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public static bool CheckExcelValidity(ExcelWorksheet worksheet)
        {
            if (worksheet == null)
            {
                return false;
            }

            // Kiểm tra header
            string[] expectedHeaders = { "CardEditorX", "name", "desc", "ot", "alias", "setcode", "type", "atk", "def", "level", "race", "attribute", "category", "str1", "str2", "str3", "str4", "str5", "str6", "str7", "str8", "str9", "str10", "str11", "str12", "str13", "str14", "str15", "str16" };
            for (int col = 1; col <= 29; col++)
            {
                var header = worksheet.Cells[1, col].Text;
                if (!string.Equals(header, expectedHeaders[col - 1], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
