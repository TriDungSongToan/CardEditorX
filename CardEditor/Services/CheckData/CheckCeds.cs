using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Constants;

namespace CardEditor.Services.CheckData
{
    public class CheckCeds
    {
        /// <summary>
        /// Check whether the structure of the first object in the JSON file (*.ceds, *.json, or *.txt) meets the requirements.
        /// First, the JSON file is validated against the PropertyOmegaName structure to ensure safety.
        /// Next, the JSON file is checked against the PropertyWithFlagName structure (where a string-type `flag` column immediately follows the `category` column).
        /// Finally, the JSON file is checked against the PropertyNoFlagName structure.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static async Task<CheckCardListResult> CheckJSONValidity(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 65536, useAsync: true);

            byte[] buffer = new byte[65536];
            int totalRead = 0;
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, totalRead, buffer.Length - totalRead)) > 0)
            {
                totalRead += bytesRead;

                if (totalRead == buffer.Length) Array.Resize(ref buffer, buffer.Length * 2);

                // Gọi hàm đồng bộ để xử lý Span/Utf8JsonReader — KHÔNG có await nào bên trong
                var (done, propNames) = TryParseFirstObjectProperties(buffer, totalRead);

                if (done) return BuildResult(propNames);
            }
            return new CheckCardListResult { Result = false };
        }
        public static CheckCardListResult CheckJSONClipboardValidity(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new CheckCardListResult { Result = false };

            try
            {
                byte[] utf8Json = Encoding.UTF8.GetBytes(json);

                var (done, propNames) =
                    TryParseFirstObjectProperties(utf8Json, utf8Json.Length);

                if (!done)
                    return new CheckCardListResult { Result = false };

                return BuildResult(propNames);
            }
            catch (JsonException)
            {
                return new CheckCardListResult { Result = false };
            }
        }

        private static (bool done, HashSet<string> propNames) TryParseFirstObjectProperties(byte[] buffer, int length)
        {
            var propNames = new HashSet<string>();
            var span = new ReadOnlySpan<byte>(buffer, 0, length);
            var jsonReader = new Utf8JsonReader(span, isFinalBlock: false, state: default);

            bool foundFirstObject = false;
            int depth = 0;

            try
            {
                while (jsonReader.Read())
                {
                    switch (jsonReader.TokenType)
                    {
                        case JsonTokenType.StartArray when depth == 0:
                            depth++;
                            break;

                        case JsonTokenType.StartObject when depth == 1:
                            foundFirstObject = true;
                            depth++;
                            break;

                        case JsonTokenType.PropertyName when foundFirstObject && depth == 2:
                            propNames.Add(jsonReader.GetString()!);
                            break;

                        case JsonTokenType.EndObject when foundFirstObject && depth == 2:
                            return (true, propNames);
                    }
                }
            }
            catch (JsonException)
            {
                // Chưa đủ dữ liệu, cần đọc thêm chunk — trả về done=false
            }

            return (false, propNames);
        }
        private static CheckCardListResult BuildResult(HashSet<string> propNames)
        {
            if (propNames.Count == 0)
                return new CheckCardListResult { Result = false };

            if (propNames.SetEquals(ConstantColumnCeds.PropertyOmegaName))
                return new CheckCardListResult { Result = true, Format = CardListFormat.OMEGA, HasFlag = true };

            if (propNames.SetEquals(ConstantColumnCeds.PropertyWithFlagName))
                return new CheckCardListResult { Result = true, Format = CardListFormat.YGOHasFlag, HasFlag = true };

            if (propNames.SetEquals(ConstantColumnCeds.PropertyNoFlagName))
                return new CheckCardListResult { Result = true, Format = CardListFormat.YGONoFlag, HasFlag = false };

            return new CheckCardListResult { Result = false };
        }
    }
}
