using System.Text;

namespace CardEditor.Helpers
{
    public static class WriteDatabaseHelper
    {
        #region Setcode (ulong ↔ BLOB)

        /// <summary>
        /// Convert ulong → byte array để lưu vào BLOB column
        /// </summary>
        public static byte[] SetCodeToBlob(ulong value)
        {
            if (value == 0UL)
                return null;  // ← Consistent: null cho zero value

            // Tính số bytes cần thiết
            int byteCount = 0;
            ulong temp = value;
            while (temp > 0 && byteCount < 8)
            {
                byteCount++;
                temp >>= 8;
            }

            // Convert ulong → byte array
            byte[] result = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                result[i] = (byte)((value >> (i * 8)) & 0xFF);
            }

            return result;
        }

        /// <summary>
        /// Convert byte array thành SQL BLOB literal string
        /// </summary>
        public static string SetCodeToBlobSqlLiteral(byte[] blobData)
        {
            if (blobData == null || blobData.Length == 0)
                return "null";

            StringBuilder sb = new();
            sb.Append("x'");
            foreach (byte b in blobData)
            {
                sb.Append(b.ToString("x02"));
            }
            sb.Append('\'');
            return sb.ToString();
        }

        /// <summary>
        /// Overload: Convert ulong trực tiếp thành SQL literal
        /// </summary>
        public static string SetCodeToBlobSqlLiteral(ulong value)
        {
            return SetCodeToBlobSqlLiteral(SetCodeToBlob(value));
        }

        #endregion

        #region Script (string ↔ BLOB)

        /// <summary>
        /// Convert string (Lua code) → byte array để lưu vào BLOB column
        /// </summary>
        public static byte[] ScriptToBlob(string script)
        {
            if (string.IsNullOrEmpty(script))
                return null;

            return Encoding.UTF8.GetBytes(script);
        }

        /// <summary>
        /// Convert byte array thành SQL BLOB literal string
        /// </summary>
        public static string ScriptToBlobSqlLiteral(byte[] blobData)
        {
            if (blobData == null || blobData.Length == 0)
                return "null";

            StringBuilder sb = new();
            sb.Append("x'");
            foreach (byte b in blobData)
            {
                sb.Append(b.ToString("x02"));
            }
            sb.Append('\'');
            return sb.ToString();
        }

        /// <summary>
        /// Overload: Convert string trực tiếp thành SQL literal
        /// </summary>
        public static string ScriptToBlobSqlLiteral(string script)
        {
            return ScriptToBlobSqlLiteral(ScriptToBlob(script));
        }

        #endregion

        #region Support (ulong ↔ BLOB)

        /// <summary>
        /// Convert ulong → byte array để lưu vào BLOB column
        /// </summary>
        public static byte[] SupportToBlob(ulong support)
        {
            if (support == 0UL)
                return null;

            // Tính số bytes cần thiết
            int byteCount = 0;
            ulong temp = support;
            while (temp > 0 && byteCount < 8)
            {
                byteCount++;
                temp >>= 8;
            }

            // Convert ulong → byte array
            byte[] result = new byte[byteCount];
            for (int i = 0; i < byteCount; i++)
            {
                result[i] = (byte)((support >> (i * 8)) & 0xFF);
            }

            return result;
        }

        /// <summary>
        /// Convert byte array thành SQL BLOB literal string
        /// </summary>
        public static string SupportToBlobSqlLiteral(byte[] blobData)
        {
            if (blobData == null || blobData.Length == 0)
                return "null";

            StringBuilder sb = new();
            sb.Append("x'");
            foreach (byte b in blobData)
            {
                sb.Append(b.ToString("x02"));
            }
            sb.Append('\'');
            return sb.ToString();
        }

        /// <summary>
        /// Overload: Convert ulong trực tiếp thành SQL literal
        /// </summary>
        public static string SupportToBlobSqlLiteral(ulong value)
        {
            return SupportToBlobSqlLiteral(SupportToBlob(value));
        }

        #endregion
    }
}
