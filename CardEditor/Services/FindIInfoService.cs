using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CardEditor.Localization;
using CardEditor.Models;
using CardEditor.ViewModels;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services
{
    public class FindIInfoService
    {
        private const int IMAGE_SEARCH_DEPTH = 4;
        private const int SCRIPT_SEARCH_DEPTH = 7;
        private static readonly string[] IMAGE_EXTENSIONS = { ".png", ".jpg", ".jpeg" };

        public static string FindImagePath(string password, string dataPath = null )
        {
            if (string.IsNullOrWhiteSpace(password)) return string.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(dataPath) && Directory.Exists(dataPath))
                {
                    string parentPath = Directory.GetParent(dataPath)?.FullName;
                    if (!string.IsNullOrWhiteSpace(parentPath))
                    {
                        string result = SearchImageInDirectory(parentPath, password, IMAGE_EXTENSIONS, IMAGE_SEARCH_DEPTH);
                        if (!string.IsNullOrWhiteSpace(result)) return result;
                    }
                }
                string settingsPath = ConfigViewModel.Instance.userSetting.DataSource;
                if (!string.IsNullOrWhiteSpace(settingsPath) && Directory.Exists(settingsPath))
                {
                    return SearchImageInDirectory(settingsPath, password, IMAGE_EXTENSIONS, IMAGE_SEARCH_DEPTH);
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }
        public static string FindScriptPath(string dataPath, string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return string.Empty;

            try
            {
                // Tên file cần tìm: c{password}.lua
                string targetFileName = $"c{password}.lua";

                // Kiểm tra thư mục cha
                if(!string.IsNullOrWhiteSpace(dataPath) && Directory.Exists(dataPath))
                {
                    string parentPath = Directory.GetParent(dataPath)?.FullName;
                    if (!string.IsNullOrWhiteSpace(parentPath))
                    {
                        string result = SearchScriptInDirectory(parentPath, targetFileName, SCRIPT_SEARCH_DEPTH);
                        if (!string.IsNullOrWhiteSpace(result)) return result;
                    }
                }
                // Kiểm tra thư mục script trong settings (nếu có)
                string settingsPath = ConfigViewModel.Instance.userSetting.DataSource;
                if (!string.IsNullOrWhiteSpace(settingsPath) && Directory.Exists(settingsPath))
                {
                    return SearchScriptInDirectory(settingsPath, targetFileName, 7);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in FindScriptPath: {ex.Message}");
            }

            return string.Empty;
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
        public static string FindSetcode(List<(ulong bit, string name)> setnamelist, ulong value)
        {
            HashSet<string> result = new HashSet<string>();
            for (int i = 0; i < 4; i++)
            {
                ushort subCode = (ushort)((value >> (i * 16)) & 0xFFFF);
                if (subCode == 0) continue;

                var set = setnamelist.FirstOrDefault(s => s.bit == subCode);
                if (!string.IsNullOrEmpty(set.name))
                {
                    result.Add(set.name);
                }
            }
            return string.Join("/", result);
        }
        private static string SearchImageInDirectory(string directoryPath, string password, string[] extensions, int maxDepth)
        {
            try
            {
                Queue<(string path, int depth)> folders = new Queue<(string, int)>();
                folders.Enqueue((directoryPath, 0));

                while (folders.Count > 0)
                {
                    var (currentDir, depth) = folders.Dequeue();
                    try
                    {
                        foreach (string file in Directory.EnumerateFiles(currentDir))
                        {
                            string fileName = Path.GetFileNameWithoutExtension(file);
                            string extension = Path.GetExtension(file);
                            if(fileName.Equals(password, StringComparison.OrdinalIgnoreCase) &&
                                extensions.Contains(extension, StringComparer.OrdinalIgnoreCase) &&
                                File.Exists(file))
                            {
                                return file;
                            }
                        }

                        if (depth < maxDepth)
                        {
                            foreach (string subDir in Directory.EnumerateDirectories(currentDir))
                            {
                                folders.Enqueue((subDir, depth + 1));
                            }
                        }
                    }
                    catch (UnauthorizedAccessException) { continue; }
                    catch (Exception) { continue; }
                }
            }
            catch (Exception)
            {
                ////
            }
            return string.Empty;
        }
        private static string SearchScriptInDirectory(string directoryPath, string targetFileName, int maxDepth)
        {
            try
            {
                Queue<(string path, int depth)> folders = new Queue<(string, int)>();
                folders.Enqueue((directoryPath, 0));

                while (folders.Count > 0)
                {
                    var (currentDir, depth) = folders.Dequeue();

                    try
                    {
                        // Tìm file có đúng tên cần tìm trong thư mục hiện tại
                        foreach (string file in Directory.EnumerateFiles(currentDir, "*.lua"))
                        {
                            if (Path.GetFileName(file).Equals(targetFileName, StringComparison.OrdinalIgnoreCase) &&
                                File.Exists(file))
                            {
                                return file;
                            }
                        }

                        // Nếu chưa đạt maxDepth, tiếp tục tìm trong thư mục con
                        if (depth < maxDepth)
                        {
                            foreach (string subDir in Directory.EnumerateDirectories(currentDir))
                            {
                                folders.Enqueue((subDir, depth + 1));
                            }
                        }
                    }
                    catch (UnauthorizedAccessException) { continue; } // Bỏ qua lỗi truy cập
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error accessing {currentDir}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SearchScriptInDirectory: {ex.Message}");
            }

            return string.Empty;
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

        public static (bool topleft, bool top, bool topright, bool left, bool right, bool botleft, bool bot, bool botright) GetLinkMarker(long value)
        {
            return (
                topleft: (value & 0x40) != 0,    // ↖
                top: (value & 0x80) != 0,        // ↑
                topright: (value & 0x100) != 0,  // ↗
                left: (value & 0x8) != 0,        // ←
                right: (value & 0x20) != 0,      // →
                botleft: (value & 0x1) != 0,     // ↙
                bot: (value & 0x2) != 0,         // ↓
                botright: (value & 0x4) != 0     // ↘
            );
        }

        public static string FindOrCreateLuaFile(string directoryPath, string password, string luaContent)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException(CMess.invaEnco.ToText());

            string targetFileName = $"c{password}.lua";
            try
            {
                var foundFile = Directory.EnumerateFiles(directoryPath, targetFileName, SearchOption.AllDirectories).FirstOrDefault();
                if (foundFile != null) return foundFile;

                // string dataSourcePath = SettingService.GetStringSetting("DataSourcePath", @"C:\ProjectIgnis");
                string dataSourcePath = ConfigViewModel.Instance.userSetting.DataSource;
                var foundFileSource = Directory.EnumerateFiles(dataSourcePath, targetFileName, SearchOption.AllDirectories).FirstOrDefault();
                if (foundFileSource != null) return foundFileSource;

                string scriptFolder = Path.Combine(directoryPath, "script");
                if (!Directory.Exists(scriptFolder)) Directory.CreateDirectory(scriptFolder);

                string newLuaFilePath = Path.Combine(scriptFolder, targetFileName);

                if (!File.Exists(newLuaFilePath))
                {
                    string safeLuaContent = luaContent.Replace("--", "").Replace("\n", " ");

                    File.WriteAllText(newLuaFilePath,
                        "-- add OCG card name\n" +
                        $"-- {safeLuaContent}\n" +
                        "local s,id,o=GetID()\n" +
                        "function s.initial_effect(c)\n" +
                        "\t\n" +
                        "end");

                }
                return newLuaFilePath;
            }
            catch (Exception ex)
            {
                throw new IOException($"{CMess.errorCreaLua.ToText()} {ex.Message}");
            }
        }

        public static string FindScriptFile(string password, string directoryPath = null, bool searchInDataSource = true)
        {
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException(CMess.invaCardID.ToText());

            string targetFileName = $"c{password}.lua";
            var roots = new List<string>();

            // 1️⃣ Thêm directoryPath nếu hợp lệ
            if (!string.IsNullOrWhiteSpace(directoryPath) && Directory.Exists(directoryPath))
                roots.Add(directoryPath);

            if (searchInDataSource)
            {
                string dataSourcePath = ConfigViewModel.Instance.userSetting.DataSource;
                if (!string.IsNullOrWhiteSpace(dataSourcePath) && Directory.Exists(dataSourcePath))
                    roots.Add(dataSourcePath);
            }

            string foundFile = null;
            object lockObj = new object();

            foreach (var root in roots)
            {
                var directories = Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories).Prepend(root);
                Parallel.ForEach(directories, new ParallelOptions{ MaxDegreeOfParallelism = Environment.ProcessorCount }, (dir, state) =>
                {
                    if (Volatile.Read(ref foundFile) != null)
                    {
                        state.Stop();
                        return;
                    }
                    var filePath = Path.Combine(dir, targetFileName);
                    if (File.Exists(filePath))
                    {
                        lock (lockObj)
                        {
                            if (foundFile == null) foundFile = filePath;
                        }
                        state.Stop();
                    }
                });
                if (foundFile != null) return foundFile;
            }
            return string.Empty;


            //try
            //{
            //    if (!string.IsNullOrWhiteSpace(directoryPath) && Directory.Exists(directoryPath))
            //    {
            //        var foundFile = Directory.EnumerateFiles(directoryPath, targetFileName, SearchOption.AllDirectories).FirstOrDefault();
            //        if (foundFile != null) return foundFile;
            //    }

            //    if (searchInDataSource)
            //    {
            //        string dataSourcePath = ConfigViewModel.Instance.userSetting.DataSource;
            //        if (!string.IsNullOrWhiteSpace(dataSourcePath) && Directory.Exists(dataSourcePath))
            //        {
            //            var foundFileSource = Directory.EnumerateFiles(dataSourcePath, targetFileName, SearchOption.AllDirectories).FirstOrDefault();
            //            if (foundFileSource != null) return foundFileSource;
            //        }
            //    }
            //}
            //catch
            //{
            //    // throw new IOException($"{CMess.errorCreaLua.ToText()} {ex.Message}");
            //}
            //return string.Empty;
        }

        public static string CreateScriptFile(string directoryPath, string password, string luaContent)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException(CMess.invaEnco.ToText());

            string targetFileName = $"c{password}.lua";
            try
            {
                string scriptFolder = Path.Combine(directoryPath, "script");
                if (!Directory.Exists(scriptFolder)) Directory.CreateDirectory(scriptFolder);
                string newLuaFilePath = Path.Combine(scriptFolder, targetFileName);

                if (!File.Exists(newLuaFilePath))
                {
                    string safeLuaContent = luaContent.Replace("--", "").Replace("\n", " ");

                    File.WriteAllText(newLuaFilePath,
                        "-- add OCG card name\n" +
                        $"-- {safeLuaContent}\n" +
                        "local s,id,o=GetID()\n" +
                        "function s.initial_effect(c)\n" +
                        "\t\n" +
                        "end");

                }
                return newLuaFilePath;
            }
            catch (Exception ex)
            {
                throw new IOException($"{CMess.errorCreaLua.ToText()} {ex.Message}");
            }
        }
    }
}
