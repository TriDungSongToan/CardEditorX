using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Windows;
using System.Reflection;
using YamlDotNet.RepresentationModel;
using System.Windows.Interop;
using CMess = ScriptSupport.Legacy.Localization.Language;
using ScriptSupport.Legacy.Localization;

namespace ScriptSupport.Legacy.Models
{
    public static class ScrapiyardData
    {
        public class FunctionData
        {
            public string name { get; set; }
            public string @namespace { get; set; }
            public string description { get; set; }
            public List<Parameter> parameters { get; set; }
            public List<Return> returns { get; set; }
        }
        public class Parameter
        {
            public string name { get; set; }
            public List<string> type { get; set; }
            public string description { get; set; }
        }
        public class Return
        {
            public List<string> type { get; set; }
            public string description { get; set; }
        }
        public class ConstantData
        {
            public string name { get; set; }
            public string description { get; set; }
            public string value { get; set; }
        }

        private static string GetFunctionData(string ymlFilePath)
        {
            // Kiểm tra điều kiện đầu vào
            if (string.IsNullOrWhiteSpace(ymlFilePath) ||
                !ymlFilePath.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) ||
                !ymlFilePath.Contains("functions"))
            {
                return string.Empty;
            }

            try
            {
                // Đọc nội dung file
                string content = File.ReadAllText(ymlFilePath);
                // Loại bỏ dòng đầu chứa tag nếu có
                if (content.StartsWith("---!function"))
                {
                    int index = content.IndexOf('\n');
                    content = content.Substring(index + 1);
                }

                // Tạo deserializer 
                var deserializer = new DeserializerBuilder()
                                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                .IgnoreUnmatchedProperties()
                                .Build();

                // Deserialize nội dung YAML
                FunctionData func = deserializer.Deserialize<FunctionData>(content);
                if (func == null)
                {
                    return string.Empty;
                }

                // Xây dựng danh sách tham số
                List<string> paramList = func.parameters?
                    .Where(p => !string.IsNullOrWhiteSpace(p.name))
                    .Select(param => $"{(param.type != null ? string.Join("|", param.type) : "")} {param.name}")
                    .ToList() ?? new List<string>();

                // Xác định kiểu trả về
                string returnStr = func.returns?.FirstOrDefault()?.type != null
                    ? string.Join("|", func.returns[0].type)
                    : "nil";

                // Tạo signature
                string functionSignature = (func.@namespace == "Card" || func.@namespace == "Effect")
                    ? $"{func.name}({string.Join(", ", paramList)}) -> {returnStr}"
                    : $"{func.@namespace}.{func.name}({string.Join(", ", paramList)}) -> {returnStr}";

                return $"{functionSignature}{Environment.NewLine}{func.description}";
            }
            catch
            {
                return string.Empty;
            }
        }
        private static string GetConstantData(string ymlFilePath)
        {
            if (string.IsNullOrWhiteSpace(ymlFilePath) ||
                !ymlFilePath.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) ||
                !ymlFilePath.Contains("constants"))
            {
                return string.Empty;
            }

            try
            {
                // Đọc nội dung file
                string content = File.ReadAllText(ymlFilePath);
                // Loại bỏ dòng đầu tiên nếu bắt đầu bằng tag ---!constant
                if (content.StartsWith("---!constant"))
                {
                    int index = content.IndexOf('\n');
                    content = content.Substring(index + 1);
                }

                // Tạo deserializer với cấu hình phù hợp
                var deserializer = new DeserializerBuilder()
                                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                        .IgnoreUnmatchedProperties()
                                        .Build();

                // Deserialize nội dung YAML thành đối tượng ConstantData
                ConstantData constant = deserializer.Deserialize<ConstantData>(content);
                // Nếu không thể deserialize hoặc đối tượng null, trả về chuỗi rỗng
                if (constant == null)
                {
                    return string.Empty;
                }

                return $"{constant.name}{Environment.NewLine}" +
                       $"{constant.description}{Environment.NewLine}" +
                       $"value: {constant.value}";
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetScrapiyardText(string ymlFilePath)
        {
            if (string.IsNullOrWhiteSpace(ymlFilePath) || !ymlFilePath.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
                return $"{CMess.invaFilePath.ToText()} {ymlFilePath}";

            if (ymlFilePath.Contains("functions"))
                return GetFunctionData(ymlFilePath);
            else if (ymlFilePath.Contains("constants"))
                return GetConstantData(ymlFilePath);
            else
            {
                try
                {
                    return File.ReadAllText(ymlFilePath);
                }
                catch
                {
                    return $"{CMess.errorRead.ToText()} {ymlFilePath}";
                }
            }
        }
    }
}
