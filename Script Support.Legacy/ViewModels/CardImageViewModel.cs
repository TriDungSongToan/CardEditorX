using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using ScriptSupport.Legacy.Localization;
using System.Collections.Concurrent;
using CMess = ScriptSupport.Legacy.Localization.Language;

namespace ScriptSupport.Legacy.ViewModels
{
    public class CardImageViewModel : IDisposable
    {
        private static readonly Lazy<CardImageViewModel> _instance = new Lazy<CardImageViewModel>(() => new CardImageViewModel());
        public static CardImageViewModel Instance => _instance.Value;
        private readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);

        private Dictionary<ulong, string> _imagePathDict;
        public bool _isLoaded = false;

        public async Task<(bool, string)> LoadImagesPaths()
        {
            if (_isLoaded) return (true, string.Empty);
            await _loadLock.WaitAsync();

            try
            {
                if (_isLoaded) return (true, string.Empty);
                string DataSource = ConfigViewModel.Instance.userSetting.DataSource;
                if (string.IsNullOrWhiteSpace(DataSource) || !Directory.Exists(DataSource))
                {
                    _isLoaded = false;
                    return (false, CMess.dataSourceMiss.ToText());
                }

                var (loadedImages, message) = await ScanImagePathsNetFxAsync(DataSource);
                if (loadedImages == null)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                    _isLoaded = false;
                    return (false, message);
                }
                _imagePathDict = loadedImages;
                _isLoaded = true;
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                _loadLock.Release();
            }
        }
        public async Task<(Dictionary<ulong, string>, string)> ScanImagePathsNetFxAsync(string rootPath, int estimatedCount = 50000, CancellationToken token = default)
        {
            string dataSourcePath = ScriptSupport.Legacy.ViewModels.ConfigViewModel.Instance.userSetting.DataSource;
            if (!Directory.Exists(dataSourcePath)) return (null, $"{CMess.dataSourceErr.ToText()} {dataSourcePath}");

            return await Task.Run(() =>
            {
                // Các extension hợp lệ dùng HashSet để lookup O(1)
                var validExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png" };

                // Các folder cần bỏ qua
                var ignoredFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "thumbnail", "field" };

                var result = new ConcurrentDictionary<ulong, string>(Environment.ProcessorCount, estimatedCount);

                // Lấy toàn bộ top-level directories để phân chia song song
                // (EnumerateFiles với AllDirectories không song song được tốt)
                var topDirs = new List<string> { rootPath };
                try
                {
                    topDirs.AddRange(Directory.EnumerateDirectories(rootPath, "*", SearchOption.AllDirectories).Where(dir =>
                    {
                        // Bỏ qua folder có tên nằm trong ignoredFolders
                        string folderName = Path.GetFileName(dir);
                        return !ignoredFolders.Contains(folderName);
                    }));
                }
                catch (UnauthorizedAccessException) { /* Bỏ qua folder không có quyền */ }

                token.ThrowIfCancellationRequested();

                // Song song hóa việc scan từng folder
                Parallel.ForEach(topDirs, new ParallelOptions
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                }, dir =>
                {
                    // Kiểm tra lại folder hiện tại (phòng trường hợp rootPath chính là ignored)
                    string dirName = Path.GetFileName(dir);
                    if (ignoredFolders.Contains(dirName)) return;

                    IEnumerable<string> files;
                    try
                    {
                        // Chỉ scan shallow (TopDirectoryOnly) vì đã enumerate dirs ở trên
                        files = Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly);
                    }
                    catch (UnauthorizedAccessException) { return; }

                    foreach (var filePath in files)
                    {
                        token.ThrowIfCancellationRequested();
                        string ext = Path.GetExtension(filePath);
                        if (!validExtensions.Contains(ext)) continue;
                        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                        if (!ulong.TryParse(fileNameWithoutExt, out ulong cardId)) continue;
                        // TryAdd: nếu trùng key thì bỏ qua (file đầu tiên tìm được sẽ được giữ)
                        result.TryAdd(cardId, filePath);
                    }
                });

                // Convert sang Dictionary thường để lookup sau này nhanh hơn ConcurrentDictionary
                return (new Dictionary<ulong, string>(result, EqualityComparer<ulong>.Default), string.Empty);

            }, token);
        }

        public void Dispose()
        {
            _imagePathDict?.Clear();
            _imagePathDict = null;
            _isLoaded = false;
        }
    }
}
