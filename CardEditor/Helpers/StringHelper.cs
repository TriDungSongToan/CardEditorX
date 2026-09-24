using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CardEditor.Helpers
{
    public static class JsonStringHelper
    {
        public static readonly JsonSerializerOptions SerializerCEDSOptions = CreateSerializerCedsOptions();
        private static JsonSerializerOptions CreateSerializerCedsOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public static readonly JsonSerializerOptions SerializerBanListOptions = CreateSerializerBanlistOptions();
        private static JsonSerializerOptions CreateSerializerBanlistOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters = { new JsonStringEnumConverter() }
            };
        }
    }

    public class JoinStringHelper
    {
        public static string JoinWithSeparator(string separator, params string[] parts)
        {
            return string.Join(separator, parts.Where(p => !string.IsNullOrEmpty(p)));
        }
        public static string SanitizationString(string input)
        {
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var sanitized = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                if (!invalidChars.Contains(c))
                {
                    sanitized.Append(c);
                }
            }
            return sanitized.ToString();
        }
    }
    public class TrimStringHelper
    {
        public static string ShortenTitle(string text, int maxLength = 30)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }
    }
    public class SanitizeStringHelper
    {
        public static string SanitizeWindowsFileName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Xóa các ký tự Windows không cho phép trong file name
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();

            string result = new string(
                input.Where(c => !invalidChars.Contains(c)).ToArray()
            );

            // Windows không cho phép tên file kết thúc bằng '.' hoặc ' '
            result = result.TrimEnd('.', ' ');

            return result;
        }
    }
}
