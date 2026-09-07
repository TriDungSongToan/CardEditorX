using System;
using System.Data.SQLite;

namespace CardEditor.Helpers
{
    public static class ReadDatabaseHelper
    {
        public static string ReadString(SQLiteDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
        }
        public static ulong ReadUInt64(SQLiteDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal)) return 0UL;
            return ulong.TryParse(reader.GetValue(ordinal)?.ToString(), out ulong value)
                ? value
                : 0UL;
        }
        public static long ReadInt64(SQLiteDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal)) return 0L;
            return long.TryParse(reader.GetValue(ordinal)?.ToString(), out long value)
                ? value
                : 0L;
        }
        public static byte[]? ReadBlob(SQLiteDataReader reader, int ordinal, bool allowText = false)
        {
            if (reader.IsDBNull(ordinal)) return null;
            object value = reader.GetValue(ordinal);
            if (value is byte[] bytes) return bytes;

            // script có thể được SQLite lưu dưới dạng TEXT
            // dù schema khai báo cột là BLOB.
            if (allowText && value is string text)
                return System.Text.Encoding.UTF8.GetBytes(text);

            throw new InvalidOperationException(
                $"Column {reader.GetName(ordinal)} is not a valid BLOB.");
        }

    }
}
