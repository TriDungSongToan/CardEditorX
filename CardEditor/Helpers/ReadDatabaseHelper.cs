using System;
using System.Linq;
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
        public static ulong ReadSetCodeFromBlob(SQLiteDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return 0UL;

            try
            {
                // Lấy BLOB dưới dạng byte array
                byte[] blobData = (byte[])reader.GetValue(ordinal);

                if (blobData == null || blobData.Length == 0) return 0UL;

                // Convert byte array → ulong
                // Byte thứ 0 = LSB (Least Significant Byte)
                ulong result = 0UL;
                for (int i = 0; i < blobData.Length && i < 8; i++)
                {
                    result |= (ulong)blobData[i] << (i * 8);
                }

                return result;
            }
            catch
            {
                // Fallback: cố gắng parse nếu là string hoặc text
                try
                {
                    return ulong.TryParse(reader.GetValue(ordinal)?.ToString(), out ulong value)
                        ? value
                        : 0UL;
                }
                catch
                {
                    return 0UL;
                }
            }
        }
        public static string ReadScriptFromBlob(SQLiteDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return string.Empty;

            try
            {
                // Script lưu dưới dạng TEXT trong BLOB
                return reader.GetString(ordinal);
            }
            catch
            {
                return string.Empty;
            }
        }
        public static ulong ReadSupportFromBlob(SQLiteDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return 0UL;

            try
            {
                // Support lưu dưới dạng hex string hoặc binary
                string supportStr = reader.GetString(ordinal);

                if (string.IsNullOrEmpty(supportStr) || supportStr == "\0")
                    return 0UL;

                // Convert string bytes → ulong
                ulong result = 0UL;
                int byteCount = 0;

                foreach (char c in supportStr.Reverse())
                {
                    result |= (ulong)(byte)c << (byteCount * 8);
                    byteCount++;
                }

                return result;
            }
            catch
            {
                return 0UL;
            }
        }
    }
}
