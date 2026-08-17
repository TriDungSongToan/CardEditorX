using System;
using System.IO;
using System.Text;
using System.Reflection;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Helpers
{
    public static class Archetype
    {
        public static (bool, string) GetEDOArchetypeList(string inputFilePath)
        {
            string exeFilePath = Assembly.GetExecutingAssembly().Location;
            string outputDirectory = Path.Combine(System.IO.Path.GetDirectoryName(exeFilePath), "data");
            string outputFilePath = Path.Combine(outputDirectory, "setnameEDOPro.txt");

            try
            {
                if (!File.Exists(inputFilePath)) return (false, CMess.fileNotExit.ToText());

                if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

                // Đọc toàn bộ các dòng từ file nguồn
                string[] lines = File.ReadAllLines(inputFilePath);

                using (StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    foreach (string line in lines)
                    {
                        // Kiểm tra nếu dòng bắt đầu với "SET" (không phân biệt hoa thường)
                        if (line.TrimStart().StartsWith("SET", StringComparison.OrdinalIgnoreCase))
                        {
                            // Tách phần tên và giá trị
                            string[] parts = line.Split(new[] { '=' }, 2);

                            if (parts.Length == 2)
                            {
                                // Loại bỏ phần "SET_" và trim các khoảng trắng
                                string name = parts[0].Trim().Substring(4); // Xóa "SET_"
                                string value = parts[1].Trim();

                                // Chuẩn hóa tên: Chuyển chữ cái đầu tiên của mỗi từ thành chữ hoa và thay gạch dưới thành khoảng trắng
                                name = CapitalizeWords(name);

                                // Ghi vào file kết quả theo định dạng "<name><tab><value>"
                                writer.WriteLine($"{value}\t{name}");
                            }
                        }
                    }
                }

                return (true, outputFilePath);
                //CMSG.Show(CMess.infoma.ToText(), CMSG.MessageBoxIconType.Information,
                    //$"{string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Export.ToText(), CMess.Data.ToText())}", new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
                //CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    //$"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
        static string CapitalizeWords(string input)
        {
            StringBuilder result = new StringBuilder();
            bool newWord = true;

            foreach (char c in input)
            {
                if (c == '_') // Nếu gặp dấu gạch dưới, thêm khoảng trắng
                {
                    result.Append(' ');
                    newWord = true;
                }
                else if (newWord)
                {
                    result.Append(char.ToUpper(c)); // Chuyển chữ cái đầu tiên thành hoa
                    newWord = false;
                }
                else
                {
                    result.Append(char.ToLower(c)); // Các chữ cái còn lại là chữ thường
                }
            }

            return result.ToString();
        }

        public static (bool, string) GetMDProArchetypeList(string inputFilePath)
        {
            string exeFilePath = Assembly.GetExecutingAssembly().Location;
            string outputDirectory = Path.Combine(System.IO.Path.GetDirectoryName(exeFilePath), "data");
            string outputFilePath = Path.Combine(outputDirectory, "setnameMDPro.txt");

            try
            {
                if (!File.Exists(inputFilePath)) return (false, CMess.fileNotExit.ToText());

                if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

                string[] lines = File.ReadAllLines(inputFilePath);
                using (StreamWriter writer = new StreamWriter(outputFilePath))
                {
                    foreach (var line in lines)
                    {
                        var trimmedLine = line.TrimStart();

                        if (trimmedLine.StartsWith("!setname", StringComparison.OrdinalIgnoreCase) ||
                            trimmedLine.StartsWith("#setname", StringComparison.OrdinalIgnoreCase))
                        {
                            var parts = trimmedLine.Substring(trimmedLine.IndexOf(' ') + 1).Split(new[] { ' ' }, 2);
                            if (parts.Length == 2)
                            {
                                string value = parts[0];
                                string name = parts[1];

                                name = CapitalizeWords(name);
                                // Ghi kết quả vào file với định dạng <abc><tab><def>
                                writer.WriteLine($"{value}\t{name}");
                            }
                        }
                    }
                }
                //CMSG.Show(CMess.infoma.ToText(), CMSG.MessageBoxIconType.Information,
                    //string.Format(CMess.TwoPlaceholderSuccess.ToText(), CMess.Export.ToText(), CMess.Data.ToText()), new[] { CMess.ok.ToText() });

                return (true, outputFilePath);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
                //CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    //$"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
            }
        }
    }
}
