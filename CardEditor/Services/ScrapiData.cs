using System;
using System.IO;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services
{
    public static class ScrapiData
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

        public class NameSpaceData
        {
            public string name { get; set; }
            public string description { get; set; }
            public string value { get; set; }
        }


        public static void GetFunctionsData(string scrapiyardPath, string outputPath)
        {
            List<string> functionLines = new List<string>();

            // Tạo deserializer
            var deserializer = new DeserializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .IgnoreUnmatchedProperties()
                                    .Build();
            if (string.IsNullOrEmpty(scrapiyardPath) || !Directory.Exists(scrapiyardPath)) return;
            string[] yamlFiles = Directory.GetFiles(scrapiyardPath, "*.yml", SearchOption.AllDirectories);

            foreach (string file in yamlFiles)
            {
                try
                {
                    string content = File.ReadAllText(file);

                    if (content.StartsWith("---!function"))
                    {
                        int index = content.IndexOf('\n');
                        content = content.Substring(index + 1);
                    }

                    // Deserialize nội dung YAML thành đối tượng FunctionData
                    FunctionData func = deserializer.Deserialize<FunctionData>(content);

                    if (func != null)
                    {
                        // Xây dựng chuỗi tham số dạng: <kiểu> <tên>, ... 
                        List<string> paramList = new List<string>();
                        if (func.parameters != null)
                        {
                            foreach (var param in func.parameters)
                            {
                                // Nếu có nhiều kiểu, ghép lại với nhau bằng dấu "|"
                                string typeStr = (param.type != null && param.type.Count > 0)
                                                    ? string.Join("|", param.type)
                                                    : "";
                                // Nếu tên parameter trống hoặc null thì bỏ qua
                                if (!string.IsNullOrWhiteSpace(param.name))
                                {
                                    paramList.Add($"{typeStr} {param.name}");
                                }
                            }
                        }
                        string parametersStr = string.Join(", ", paramList);

                        // Xây dựng chuỗi kiểu trả về.
                        string returnStr = "nil";
                        if (func.returns != null && func.returns.Count > 0 && func.returns[0].type != null)
                        {
                            returnStr = string.Join("|", func.returns[0].type);
                        }

                        // Xây dựng signature: <namespace>.<name>(<parameters>) -> <returns>
                        string functionSignature;
                        if (func.@namespace == "Card" || func.@namespace == "Effect")
                        {
                            functionSignature = $"{func.name}({parametersStr}) -> {returnStr}";
                        }
                        else
                        {
                            functionSignature = $"{func.@namespace}.{func.name}({parametersStr}) -> {returnStr}";
                        }

                        // Xây dựng nội dung của function
                        string functionContent = $"{functionSignature}{Environment.NewLine}{func.description}";

                        // Thêm vào danh sách kết quả
                        functionLines.Add(functionContent);
                    }
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }

            // Ghi kết quả vào file functions.txt
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                using (StreamWriter writer = new StreamWriter(outputPath))
                {
                    for (int i = 0; i < functionLines.Count; i++)
                    {
                        writer.WriteLine(functionLines[i]);
                        if (i < functionLines.Count - 1)
                        {
                            writer.WriteLine("---");
                        }
                    }
                }
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, CMess.expoFunSuc.ToText(), new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText()});
            }
        }
        public static void GetConstantsData(string constantsPath, string outputPath)
        {
            List<string> constantLines = new List<string>();

            // Tạo deserializer
            var deserializer = new DeserializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .IgnoreUnmatchedProperties()
                                    .Build();
            if (string.IsNullOrEmpty(constantsPath) || !Directory.Exists(constantsPath)) return;

            // Lấy tất cả các file *.yml
            string[] yamlFiles = Directory.GetFiles(constantsPath, "*.yml", SearchOption.AllDirectories);

            foreach (string file in yamlFiles)
            {
                try
                {
                    string content = File.ReadAllText(file);

                    if (content.StartsWith("---!constant"))
                    {
                        int index = content.IndexOf('\n');
                        content = content.Substring(index + 1);
                    }

                    // Deserialize nội dung YAML thành đối tượng ConstantData
                    ConstantData constant = deserializer.Deserialize<ConstantData>(content);

                    if (constant != null)
                    {
                        // Xây dựng chuỗi đầu ra
                        string constantContent = $"{constant.name}{Environment.NewLine}" +
                                                 $"{constant.description}{Environment.NewLine}" +
                                                 $"value: {constant.value}";
                        constantLines.Add(constantContent);
                    }
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                using (StreamWriter writer = new StreamWriter(outputPath))
                {
                    for (int i = 0; i < constantLines.Count; i++)
                    {
                        writer.WriteLine(constantLines[i]);
                        if (i < constantLines.Count - 1)
                        {
                            writer.WriteLine("---");
                        }
                    }
                }
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, CMess.expoConsSuc.ToText(), new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        public static void GetNameSpaceData(string constantsPath, string outputPath)
        {
            // Danh sách chứa kết quả của từng hằng số
            List<string> constantLines = new List<string>();

            // Tạo deserializer với NamingConvention phù hợp
            var deserializer = new DeserializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .IgnoreUnmatchedProperties()
                                    .Build();
            if (string.IsNullOrEmpty(constantsPath) || !Directory.Exists(constantsPath))
                return;

            // Lấy tất cả các file *.yml (bao gồm cả các file trong các folder con)
            string[] yamlFiles = Directory.GetFiles(constantsPath, "*.yml", SearchOption.AllDirectories);

            foreach (string file in yamlFiles)
            {
                try
                {
                    string content = File.ReadAllText(file);

                    if (content.StartsWith("---!constant"))
                    {
                        int index = content.IndexOf('\n');
                        content = content.Substring(index + 1);
                    }

                    // Deserialize nội dung YAML thành đối tượng ConstantData
                    ConstantData constant = deserializer.Deserialize<ConstantData>(content);

                    if (constant != null)
                    {
                        // Xây dựng chuỗi đầu ra cho hằng số
                        string constantContent = $"{constant.name}{Environment.NewLine}" +
                                                 $"{constant.description}{Environment.NewLine}" +
                                                 $"value: {constant.value}";
                        constantLines.Add(constantContent);
                    }
                }
                catch (Exception ex)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                }
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                using (StreamWriter writer = new StreamWriter(outputPath))
                {
                    for (int i = 0; i < constantLines.Count; i++)
                    {
                        writer.WriteLine(constantLines[i]);
                        if (i < constantLines.Count - 1)
                        {
                            writer.WriteLine("---");
                        }
                    }
                }
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification, CMess.expoConsSuc.ToText(), new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, $"{string.Format(CMess.PlaceholderError.ToText(), CMess.Read.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
    }
}
