using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using CardEditor.Models;
using CardEditor.Services.CreateFile;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.SaveData
{
    public class SaveArchiveService
    {
        private static class ResourceIndexBuilder
        {
            private const int IMAGE_SEARCH_DEPTH = 4;
            private const int SCRIPT_SEARCH_DEPTH = 7;
            private static readonly string[] IMAGE_EXTENSIONS = { ".png", ".jpg", ".jpeg" };

            public class Index
            {
                public Dictionary<string, string> Images { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                public Dictionary<string, string> Scripts { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            public static Index Build(string rootDirectory)
            {
                var index = new Index();
                if (string.IsNullOrWhiteSpace(rootDirectory) || !Directory.Exists(rootDirectory)) return index;

                int maxDepth = Math.Max(IMAGE_SEARCH_DEPTH, SCRIPT_SEARCH_DEPTH); // 7, duyệt 1 lần cho cả 2 loại
                var folders = new Queue<(string path, int depth)>();
                folders.Enqueue((rootDirectory, 0));

                while (folders.Count > 0)
                {
                    var (currentDir, depth) = folders.Dequeue();
                    try
                    {
                        foreach (string file in Directory.EnumerateFiles(currentDir))
                        {
                            string ext = Path.GetExtension(file);

                            if (depth <= IMAGE_SEARCH_DEPTH &&
                                IMAGE_EXTENSIONS.Contains(ext, StringComparer.OrdinalIgnoreCase))
                            {
                                string key = Path.GetFileNameWithoutExtension(file);
                                if (!index.Images.ContainsKey(key)) index.Images[key] = file;
                            }

                            if (depth <= SCRIPT_SEARCH_DEPTH && ext.Equals(".lua", StringComparison.OrdinalIgnoreCase))
                            {
                                string key = Path.GetFileName(file); // "c{id}.lua"
                                if (!index.Scripts.ContainsKey(key)) index.Scripts[key] = file;
                            }
                        }

                        if (depth < maxDepth)
                        {
                            foreach (string subDir in Directory.EnumerateDirectories(currentDir))
                                folders.Enqueue((subDir, depth + 1));
                        }
                    }
                    catch (UnauthorizedAccessException) { continue; }
                    catch (Exception) { continue; }
                }
                return index;
            }

            public static (string ImagePath, string ScriptPath) Lookup(Index primary, Index fallback, string cardId)
            {
                string scriptKey = $"c{cardId}.lua";

                string imagePath = primary.Images.TryGetValue(cardId, out var img) ? img
                    : (fallback != null && fallback.Images.TryGetValue(cardId, out var img2)) ? img2 : null;

                string scriptPath = primary.Scripts.TryGetValue(scriptKey, out var scr) ? scr
                    : (fallback != null && fallback.Scripts.TryGetValue(scriptKey, out var scr2)) ? scr2 : null;

                return (imagePath, scriptPath);
            }
        }

        public static async Task<(bool, string)> SaveCompressedYGO(List<CardEditor.Models.Card> cardList, string extension, bool hasFlag,
            string targetPath, string? dbFilePath = null)
        {
            if (cardList == null || !cardList.Any()) return (false, CMess.noCardExport.ToText());
            if (string.IsNullOrEmpty(targetPath)) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText()));

            string outPutCdbPath = string.Empty;

            try
            {
                string directoryZIPPath = System.IO.Path.GetDirectoryName(targetPath);
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(targetPath);
                string fileWithCdbExtension = System.IO.Path.Combine(directoryZIPPath, fileNameWithoutExtension + $".{extension}");

                if (System.IO.File.Exists(fileWithCdbExtension)) return (false, $"{CMess.File.ToText()} {fileWithCdbExtension} {CMess.filealreadyExit.ToText()}");

                CreateCardListResult resultCreate = CreateDatabaseService.CreateDatabaseYGO(directoryZIPPath, $"{fileNameWithoutExtension}.{extension}", hasFlag);
                if (resultCreate.Result) outPutCdbPath = resultCreate.FilePath;
                else return (false, resultCreate.Messenger);

                if (string.IsNullOrEmpty(outPutCdbPath)) return (false, string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText()));

                await SaveDatabaseYGOService.SaveCardList(cardList, outPutCdbPath).ConfigureAwait(false);

                // === Build index 1 lần, thay cho việc gọi FindImagePath/FindScriptPath cho từng card ===
                string? resourceDirectory = GetResourceDirectory(dbFilePath);

                // Tái tạo đúng bước Directory.GetParent(dataPath) bên trong FindImagePath/FindScriptPath gốc
                string? primaryRoot = null;
                if (!string.IsNullOrWhiteSpace(resourceDirectory) && Directory.Exists(resourceDirectory))
                    primaryRoot = Directory.GetParent(resourceDirectory)?.FullName;

                var primaryIndex = ResourceIndexBuilder.Build(primaryRoot);

                var cardIds = cardList.Select(c => c.id.ToString()).ToList();

                bool needFallback = cardIds.Any(id =>
                    !primaryIndex.Images.ContainsKey(id) && !primaryIndex.Scripts.ContainsKey($"c{id}.lua"));

                ResourceIndexBuilder.Index fallbackIndex = null;
                if (needFallback)
                {
                    string settingsPath = ConfigViewModel.Instance.userSetting.DataSource;
                    if (!string.IsNullOrWhiteSpace(settingsPath) && Directory.Exists(settingsPath) && settingsPath != primaryRoot)
                        fallbackIndex = ResourceIndexBuilder.Build(settingsPath);
                }

                var fileGroups = cardIds
                    .Select(id => ResourceIndexBuilder.Lookup(primaryIndex, fallbackIndex, id))
                    .Where(x => !string.IsNullOrEmpty(x.ImagePath) || !string.IsNullOrEmpty(x.ScriptPath))
                    .ToList();

                using (var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(outPutCdbPath, Path.GetFileName(outPutCdbPath), System.IO.Compression.CompressionLevel.Fastest);

                    foreach (var files in fileGroups)
                    {
                        if (!string.IsNullOrEmpty(files.ImagePath))
                            zip.CreateEntryFromFile(files.ImagePath, $"pics/{Path.GetFileName(files.ImagePath)}", System.IO.Compression.CompressionLevel.Fastest);

                        if (!string.IsNullOrEmpty(files.ScriptPath))
                            zip.CreateEntryFromFile(files.ScriptPath, $"script/{Path.GetFileName(files.ScriptPath)}", System.IO.Compression.CompressionLevel.Fastest);
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                TryDeleteFile(outPutCdbPath);
            }
        }

        public static async Task<(bool, string)> SaveCompressedOMEGA(List<CardEditor.Models.CardOmega> cardList, string extension,
            string targetPath, string? dbFilePath = null)
        {
            if (cardList == null || !cardList.Any()) return (false, CMess.noCardExport.ToText());
            if (string.IsNullOrEmpty(targetPath)) return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Path.ToText()));

            string outPutCdbPath = string.Empty;

            try
            {
                string directoryZIPPath = System.IO.Path.GetDirectoryName(targetPath);
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(targetPath);
                string fileWithCdbExtension = System.IO.Path.Combine(directoryZIPPath, fileNameWithoutExtension + $".{extension}");

                if (System.IO.File.Exists(fileWithCdbExtension)) return (false, $"{CMess.File.ToText()} {fileWithCdbExtension} {CMess.filealreadyExit.ToText()}");

                CreateCardListResult resultCreate = CreateDatabaseService.CreateDatabaseOMEGA(directoryZIPPath, $"{fileNameWithoutExtension}.{extension}");
                if (resultCreate.Result) outPutCdbPath = resultCreate.FilePath;
                else return (false, resultCreate.Messenger);

                if (string.IsNullOrEmpty(outPutCdbPath)) return (false, string.Format(CMess.ThreePlaceholderError.ToText(), CMess.Create.ToText(), CMess.CardDB.ToText(), CMess.File.ToText()));

                await SaveDatabaseOMEGAService.SaveCardList(cardList, outPutCdbPath).ConfigureAwait(false);

                // === Build index 1 lần, thay cho việc gọi FindImagePath/FindScriptPath cho từng card ===
                string? resourceDirectory = GetResourceDirectory(dbFilePath);

                // Tái tạo đúng bước Directory.GetParent(dataPath) bên trong FindImagePath/FindScriptPath gốc
                string? primaryRoot = null;
                if (!string.IsNullOrWhiteSpace(resourceDirectory) && Directory.Exists(resourceDirectory))
                    primaryRoot = Directory.GetParent(resourceDirectory)?.FullName;

                var primaryIndex = ResourceIndexBuilder.Build(primaryRoot);

                var cardIds = cardList.Select(c => c.id.ToString()).ToList();

                bool needFallback = cardIds.Any(id =>
                    !primaryIndex.Images.ContainsKey(id) && !primaryIndex.Scripts.ContainsKey($"c{id}.lua"));

                ResourceIndexBuilder.Index fallbackIndex = null;
                if (needFallback)
                {
                    string settingsPath = ConfigViewModel.Instance.userSetting.DataSource;
                    if (!string.IsNullOrWhiteSpace(settingsPath) && Directory.Exists(settingsPath) && settingsPath != primaryRoot)
                        fallbackIndex = ResourceIndexBuilder.Build(settingsPath);
                }

                var fileGroups = cardIds
                    .Select(id => ResourceIndexBuilder.Lookup(primaryIndex, fallbackIndex, id))
                    .Where(x => !string.IsNullOrEmpty(x.ImagePath) || !string.IsNullOrEmpty(x.ScriptPath))
                    .ToList();

                using (var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var zip = new ZipArchive(fileStream, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(outPutCdbPath, Path.GetFileName(outPutCdbPath), System.IO.Compression.CompressionLevel.Fastest);

                    foreach (var files in fileGroups)
                    {
                        if (!string.IsNullOrEmpty(files.ImagePath))
                            zip.CreateEntryFromFile(files.ImagePath, $"pics/{Path.GetFileName(files.ImagePath)}", System.IO.Compression.CompressionLevel.Fastest);

                        if (!string.IsNullOrEmpty(files.ScriptPath))
                            zip.CreateEntryFromFile(files.ScriptPath, $"script/{Path.GetFileName(files.ScriptPath)}", System.IO.Compression.CompressionLevel.Fastest);
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                TryDeleteFile(outPutCdbPath);
            }
        }


        private static string? GetResourceDirectory(string? dbFilePath)
        {
            if (!string.IsNullOrWhiteSpace(dbFilePath))
            {
                string? directory = Path.GetDirectoryName(dbFilePath);
                if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory)) return directory;
                return null;
            }
            string? dataSource = ConfigViewModel.Instance.userSetting.DataSource;
            if (string.IsNullOrWhiteSpace(dataSource)) return null;
            if (!Directory.Exists(dataSource)) return null;
            return dataSource;
        }
        private static void TryDeleteFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;
            try
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not delete temporary file: {filePath}. {ex}");
            }
        }
    }
}
