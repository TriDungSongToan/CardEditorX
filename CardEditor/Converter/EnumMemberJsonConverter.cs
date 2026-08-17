using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.Serialization;
using System.Reflection;
using System.Collections.Generic;

namespace CardEditor.Converter
{
    public class EnumMemberJsonConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        private readonly Dictionary<string, T> _toEnum = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<T, string> _toString = new();

        public EnumMemberJsonConverter()
        {
            foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var value = (T)field.GetValue(null)!;
                var attr = field.GetCustomAttribute<EnumMemberAttribute>();
                var name = attr?.Value ?? field.Name;
                _toEnum[name] = value;
                _toString[value] = name;
            }
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var raw = reader.GetString();
            if (raw != null && _toEnum.TryGetValue(raw, out var result))
                return result;

            // Konami thêm giá trị mới mà enum chưa kịp cập nhật -> đừng crash cả object
            return default;
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => writer.WriteStringValue(_toString.TryGetValue(value, out var s) ? s : value.ToString());
    }
}
