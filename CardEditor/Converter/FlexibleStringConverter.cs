using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CardEditor.Converter
{
    public class FlexibleStringConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString();

                case JsonTokenType.Number:
                    // Giữ nguyên dạng số gốc (tránh mất số 0 đầu, hay lỗi định dạng)
                    return reader.TryGetInt64(out long l) ? l.ToString() : reader.GetDouble().ToString();

                case JsonTokenType.Null:
                    return null;

                default:
                    throw new JsonException($"Không thể convert {reader.TokenType} sang string.");
            }
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
            => writer.WriteStringValue(value);
    }
}
