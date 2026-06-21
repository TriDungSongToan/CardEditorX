using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Cmess = ScriptSupport.Legacy.Localization.Language;
using ScriptSupport.Legacy.Localization;
using ScriptSupport.Legacy.Services;
using ScriptSupport.Legacy.Models;

namespace ScriptSupport.Legacy.Services
{
    public class FindIInfoService
    {
        public class CardInfoResult
        {
            public List<(ulong bit, string name)> Data { get; set; } = new List<(ulong bit, string name)>();
            public bool Success { get; set; } = true;
            public string ErrorMessage { get; set; } = string.Empty;
        }
        public static string FindImagePath(string dataPath, string password)
        {
            if (string.IsNullOrWhiteSpace(dataPath) || string.IsNullOrWhiteSpace(password))
                return string.Empty;

            try
            {
                string[] extensions = { ".png", ".jpg", ".jpeg" };
                foreach (string file in Directory.EnumerateFiles(dataPath, "*.*", SearchOption.AllDirectories))
                {
                    if (Path.GetFileNameWithoutExtension(file).Equals(password, StringComparison.OrdinalIgnoreCase) &&
                        Array.Exists(extensions, ext => ext.Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase)))
                    {
                        return file;
                    }
                }
            }
            catch
            {
                ///
            }
            return string.Empty;
        }

        public static async Task<CardInfoResult> LoadCardInfo(string data, bool hasGame)
        {
            var result = new CardInfoResult();

            string language = PropertiesSettingService.GetStringSetting("Language", "English");
            string game = hasGame ? PropertiesSettingService.GetStringSetting("Game", "EDOPro") : string.Empty;

            string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"data\CardData\Language\{language}\cardinfo", $"{data}{game}.txt");
            // var datalist = new List<(ulong bit, string name)>();

            try
            {
                if (File.Exists(dataPath))
                {
                    using (var reader = new StreamReader(dataPath))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            var parts = line.Split('\t');
                            if (parts.Length >= 2)
                            {
                                if (ulong.TryParse(parts[0].Trim().Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var bitValue))
                                {
                                    string name = parts[1].Trim();
                                    result.Data.Add((bitValue, name));
                                    //datalist.Add((bitValue, name));
                                }
                            }
                        }
                    }
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage = $"{data} {Cmess.fileNotExit.ToText()}";
                    // throw new FileNotFoundException($"File {data}.txt does not exist.");
                }
            }
            catch (FileNotFoundException)
            {
                result.Success = false;
                result.ErrorMessage = $"{data} {Cmess.fileNotExit.ToText()}";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"{Cmess.errorOcc.ToText()} {ex.Message}";
                // MessageBox.Show($"Unexpected error: {ex.Message}.");
            }
            return result;
        }

        public static string FindCardInfo(List<(ulong bit, string name)> dataList, ulong value, bool separation, string separastring = "/")
        {
            if (dataList == null || dataList.Count == 0)
            {
                return string.Empty;
            }
            var result = dataList.Where(t => (value & t.bit) != 0).Select(t => t.name).ToList();

            return separation ? string.Join(separastring, result) : string.Join(" ", result);
        }

        public static (ulong , ulong , ulong , ulong ) FindLevelPenScale(ulong value, bool isLink)
        {
            ulong link, level, rightScale, leftScale;
            if (isLink)
            {
                link = value & 0xFF;
                level = (value >> 8) & 0xFF;
            }
            else
            {
                level = value & 0xFF;
                link = (value >> 8) & 0xFF;
            }
            rightScale = (value >> 16) & 0xFF;
            leftScale = (value >> 24) & 0xFF;

            return (level, link, rightScale, leftScale);
        }
        public static string FindSetcode(List<(ulong bit, string name)> setnamelist, ulong value)
        {
            Console.WriteLine($"FindSetcode: {value:X}");
            HashSet<string> result = new HashSet<string>();
            for (int i = 0; i < 4; i++) // Vì tối đa có 4 setcode con
            {
                ushort subCode = (ushort)((value >> (i * 16)) & 0xFFFF);
                Console.WriteLine($"SubCode {i}: {subCode:X}");
                if (subCode == 0) continue;

                var set = setnamelist.FirstOrDefault(s => s.bit == subCode);
                Console.WriteLine($"Found set: bit={set.bit}, name={set.name ?? "null"}");
                if (!string.IsNullOrEmpty(set.name))
                {
                    result.Add(set.name);
                }

                //int subCode = (value >> (i * 16)) & 0xFFFF;
                //if (subCode == 0) continue;

                //var setName = setnamelist.FirstOrDefault(set => set.bit == (ulong)subCode).name;
                //if (!string.IsNullOrEmpty(setName))
                //{
                //    result.Add(setName);
                //}
            }
            Console.WriteLine($"Result: {string.Join("/", result)}");
            return string.Join("/", result);
        }


        public static bool CheckCardInfo(ulong value, params CardType[] data)
        {
            foreach (var item in data)
            {
                if ((value & (ulong)item) != 0)
                {
                    return true; // Trả về true nếu tìm thấy bit phù hợp
                }
            }
            return false; // Trả về false nếu không có bit nào khớp
        }
    }
}
