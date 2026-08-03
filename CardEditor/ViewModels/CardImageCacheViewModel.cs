using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Helpers;

namespace CardEditor.ViewModels
{
    public class CardImageCacheViewModel : IDisposable
    {
        private static readonly Lazy<CardImageCacheViewModel> _instance = new Lazy<CardImageCacheViewModel>(() => new CardImageCacheViewModel());
        public static CardImageCacheViewModel Instance => _instance.Value;

        private readonly Dictionary<ulong, (BitmapImage image, DateTime lastUsed)> _imageCache;
        private readonly Dictionary<ulong, string> _imagePathDict;
        private readonly int _maxCacheSize = 500;
        private bool _isLoaded = false;
        private readonly object _lock = new object();

        // Resources
        private BitmapImage _blankImage;
        private BitmapImage _loadingImage;

        public BitmapImage BlankImage => _blankImage ??= CardImageHelper.BlankImage;
        public BitmapImage LoadingImage => _loadingImage ??= CardImageHelper.LoadingImage;
        public bool IsLoaded => _isLoaded;

        private CardImageCacheViewModel()
        {
            _imageCache = new Dictionary<ulong, (BitmapImage image, DateTime lastUsed)>();
            _imagePathDict = new Dictionary<ulong, string>();
        }

        public async Task PreloadImagesAsync(IProgress<(int current, int total, string status)> progress = null)
        {
            if (_isLoaded) return;

            await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (_isLoaded) return;

                    try
                    {
                        string dataSourcePath = ConfigViewModel.Instance.userSetting.DataSource;

                        if (string.IsNullOrWhiteSpace(dataSourcePath) || !Directory.Exists(dataSourcePath))
                        {
                            return;
                        }

                        // Tìm tất cả file ảnh có pattern: {số}.jpg hoặc {số}.png
                        string[] validExtensions = { ".jpg", ".png", ".jpeg", ".webp" };

                        var allFiles = Directory.EnumerateFiles(dataSourcePath, "*.*", SearchOption.AllDirectories);

                        var filteredFiles = allFiles.Where(f =>
                        {
                            string dir = Path.GetDirectoryName(f);
                            var dirs = dir.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                            return !dirs.Any(d =>
                                d.Equals("thumbnail", StringComparison.OrdinalIgnoreCase) ||
                                d.Equals("field", StringComparison.OrdinalIgnoreCase));
                        });

                        var imageFiles = filteredFiles
                        .Where(f => validExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                        .ToList();

                        progress?.Report((0, imageFiles.Count, "Scanning images..."));

                        int processed = 0;
                        foreach (var filePath in imageFiles)
                        {
                            string fileName = Path.GetFileNameWithoutExtension(filePath);
                            if (ulong.TryParse(fileName, out ulong cardId))
                            {
                                _imagePathDict[cardId] = filePath;
                            }

                            processed++;
                            if (processed % 100 == 0) // Report mỗi 100 files
                            {
                                progress?.Report((processed, imageFiles.Count, $"Loading images... {processed}/{imageFiles.Count}"));
                            }
                        }

                        _isLoaded = true;
                        progress?.Report((imageFiles.Count, imageFiles.Count, $"Loaded {_imageCache.Count} images"));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error preloading images: {ex.Message}");
                    }
                }
            });
        }

        private List<(ulong cardId, string filePath)> ScanImagePaths(string rootPath)
        {
            var results = new List<(ulong, string)>();
            string[] validExtensions = { ".jpg", ".png", ".jpeg", ".webp" };

            foreach (var ext in validExtensions)
            {
                var files = Directory.GetFiles(rootPath, $"*{ext}", SearchOption.AllDirectories);

                foreach (var filePath in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    if (ulong.TryParse(fileName, out ulong cardId))
                    {
                        results.Add((cardId, filePath));
                    }
                }
            }

            return results;
        }

        private BitmapImage LoadOptimizedBitmap(string filePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
            bitmap.DecodePixelWidth = 177; // Giảm kích thước
            bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load hết rồi đóng file
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile; // Faster
            bitmap.EndInit();
            bitmap.Freeze(); // Thread-safe + GC-friendly

            return bitmap;
        }

        private void ForceGarbageCollection()
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        }

        private long GetCurrentRAM()
        {
            using (var proc = System.Diagnostics.Process.GetCurrentProcess())
            {
                return proc.PrivateMemorySize64 / (1024 * 1024); // Convert to MB
            }
        }

        public BitmapImage GetCardImage(ulong cardId)
        {
            lock (_lock)
            {
                if (_imageCache.TryGetValue(cardId, out var entry))
                {
                    // cập nhật thời gian truy cập
                    _imageCache[cardId] = (entry.image, DateTime.Now);
                    return entry.image;
                }
            }

            if (_imagePathDict.TryGetValue(cardId, out var path))
            {
                try
                {
                    if (!File.Exists(path)) return BlankImage;

                    byte[] imgBytes = File.ReadAllBytes(path);
                    var bitmap = new BitmapImage();
                    using (var ms = new MemoryStream(imgBytes))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.DecodePixelWidth = 80;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                    lock (_lock)
                    {
                        _imageCache[cardId] = (bitmap, DateTime.Now);
                        EnsureCacheLimit();
                    }
                    return bitmap;
                }
                catch
                {
                    return BlankImage;
                }
            }
            return BlankImage;
        }

        public BitmapImage GetFullCardImage(ulong cardId)
        {
            if (_imagePathDict.TryGetValue(cardId, out var path))
            {
                try
                {
                    if (!File.Exists(path)) return BlankImage;
                    byte[] imgBytes = File.ReadAllBytes(path);
                    var bitmap = new BitmapImage();

                    using (var ms = new MemoryStream(imgBytes))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }

                    lock (_lock)
                    {
                        _imageCache[cardId] = (bitmap, DateTime.Now);
                        EnsureCacheLimit();
                    }
                    return bitmap;
                }
                catch
                {
                    return BlankImage;
                }
            }
            return BlankImage;
        }
        public string GetImagePath(ulong cardID)
        {
            return _imagePathDict.TryGetValue(cardID, out var path) ? path : string.Empty;
        }

        private void EnsureCacheLimit()
        {
            if (_imageCache.Count <= _maxCacheSize)
                return;

            // Lấy các phần tử cũ nhất
            var toRemove = _imageCache
                .OrderBy(kv => kv.Value.lastUsed)
                .Take(_imageCache.Count - _maxCacheSize)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in toRemove)
            {
                _imageCache.Remove(key);
            }
        }

        public (int totalImages, long memoryMB) GetCacheStats()
        {
            lock (_lock)
            {
                long estimatedMemory = _imageCache.Count * 177 * 258 * 4 / (1024 * 1024); // Rough estimate
                return (_imageCache.Count, estimatedMemory);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _imageCache.Clear();
                _imagePathDict.Clear();
                _blankImage = null;
                _loadingImage = null;

                _isLoaded = false;
            }
        }

        private BitmapImage LoadResourceImage(string uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

    }
}
