using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using SkiaSharp;
using CardEditor.Collections;
using CardEditor.ImagesConfig;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.ViewModels
{
    public class GeneraImageViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<GeneraImageViewModel> _instance = new Lazy<GeneraImageViewModel>(() =>  new GeneraImageViewModel());
        public static GeneraImageViewModel Instance => _instance.Value;

        private string DataFolderPath => CardEditor.Models.AppContext.Instance.DataFolderPath;
        public bool IsLoadedImageCache = false;
        public bool IsChangedSeries = false;
        public bool IsChangedRare = false;

        #region Cache Properties
        public BulkObservableCollection<string> Series {  get; set; }
        public List<KeyValuePair<ulong, SKBitmap>> BackgroundCache { get; set; } = new List<KeyValuePair<ulong, SKBitmap>>();
        public List<KeyValuePair<ulong, SKBitmap>> AttributeCache { get; set; } = new List<KeyValuePair<ulong, SKBitmap>>();
        public List<KeyValuePair<ulong, SKBitmap>> SpellTrapCache { get; set; } = new List<KeyValuePair<ulong, SKBitmap>>();
        public Dictionary<string, SKBitmap> BackgroundArtCache { get; set; } = new Dictionary<string, SKBitmap>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, SKBitmap> SecretCache { get; set; } = new Dictionary<string, SKBitmap>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, SKBitmap> FoildCache { get; set; } = new Dictionary<string, SKBitmap>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<ulong, SKBitmap> LevelCache { get; set; } = new Dictionary<ulong, SKBitmap>();
        public Dictionary<ulong, SKBitmap> RankCache { get; set; } = new Dictionary<ulong, SKBitmap>();
        public Dictionary<ulong, SKBitmap> LevelRankCache { get; set; } = new Dictionary<ulong, SKBitmap>();
        public Dictionary<int, SKBitmap> FrameCache { get; set; } = new Dictionary<int, SKBitmap>();
        public Dictionary<int, SKBitmap> PowerCache { get; set; } = new Dictionary<int, SKBitmap>();

        public Dictionary<string, SKBitmap> LinkArrowNormalCache { get; set; } = new Dictionary<string, SKBitmap>();
        public Dictionary<string, SKBitmap> LinkArrowPenCache { get; set; } = new Dictionary<string, SKBitmap>();

        public Dictionary<string, SKTypeface> FontOCGCache { get; set; } = new Dictionary<string, SKTypeface>();
        public Dictionary<string, SKTypeface> FontTCGCache { get; set; } = new Dictionary<string, SKTypeface>();

        public SKTypeface _fallbackTypefaceOCG;
        public SKTypeface _fallbackTypefaceTCG;
        #endregion

        #region Config
        public ImageInfo CurrentImageInfo { get; set; }
        public LinkArrowInfo CurrentLinkArrowInfo { get; set; }
        #endregion

        private GeneraImageViewModel()
        {
            Series = new BulkObservableCollection<string>();
        }
        public void Dispose()
        {
            foreach (var item in BackgroundCache)
            {
                item.Value?.Dispose();
            }
            BackgroundCache.Clear();
            BackgroundCache = null;

            foreach (var item in AttributeCache)
            {
                item.Value?.Dispose();
            }
            AttributeCache.Clear();
            AttributeCache = null;

            foreach (var item in SpellTrapCache)
            {
                item.Value?.Dispose();
            }
            SpellTrapCache.Clear();
            SpellTrapCache = null;

            foreach (var item in BackgroundArtCache)
            {
                item.Value?.Dispose();
            }
            BackgroundArtCache.Clear();
            BackgroundArtCache = null;

            foreach (var item in SecretCache)
            {
                item.Value?.Dispose();
            }
            SecretCache.Clear();
            SecretCache = null;

            foreach (var item in FoildCache)
            {
                item.Value?.Dispose();
            }
            FoildCache.Clear();
            FoildCache = null;

            foreach (var item in LevelCache)
            {
                item.Value?.Dispose();
            }
            LevelCache.Clear();
            LevelCache = null;

            foreach (var item in RankCache)
            {
                item.Value?.Dispose();
            }
            RankCache.Clear();
            RankCache = null;

            foreach (var item in LevelRankCache)
            {
                item.Value?.Dispose();
            }
            LevelRankCache.Clear();
            LevelRankCache = null;

            foreach (var item in FrameCache)
            {
                item.Value?.Dispose();
            }
            FrameCache.Clear();
            FrameCache = null;

            foreach (var item in PowerCache)
            {
                item.Value?.Dispose();
            }
            PowerCache.Clear();
            PowerCache = null;

            foreach (var item in LinkArrowNormalCache)
            {
                item.Value?.Dispose();
            }
            LinkArrowNormalCache.Clear();
            LinkArrowNormalCache = null;

            foreach (var item in LinkArrowPenCache)
            {
                item.Value?.Dispose();
            }
            LinkArrowPenCache.Clear();
            LinkArrowPenCache = null;

            foreach (var item in FontOCGCache)
            {
                item.Value?.Dispose();
            }
            foreach (var item in FontTCGCache)
            {
                item.Value?.Dispose();
            }
            FontOCGCache.Clear();
            FontOCGCache = null;
            FontTCGCache.Clear();
            FontTCGCache = null;

            IsLoadedImageCache = false;
        }
        
        public (bool, string) LoadSeriesList()
        {
            try
            {
                Series.Clear();
                List<string> tempSeries = new List<string>();
                string seriesFilePath = Path.Combine(DataFolderPath, $@"CardImage\Series.txt");
                if (File.Exists(seriesFilePath))
                {
                    using (StreamReader sr = new StreamReader(seriesFilePath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            tempSeries.Add(line);
                        }
                    }
                }
                else tempSeries.Add("Series 10");
                Series.AddRange(tempSeries);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            
        }
        public async Task<bool> LoadImageCache(int Series, int Rare)
        {
            Debug.WriteLine("Start Load Image Cache");
            if (IsChangedSeries)
            {
                if (!LoadImageConfig(Series)) return false;
            }

            var loadTasks = new List<Task<bool>>
            {
                LoadBackgroundCache(Series),
                LoadBackgroundArtCache(Series),
                LoadSecretCache(Series),
                LoadFoildCache(Series),
                LoadAttributeCache(Series),
                LoadSpellTrapCache(Series),
                LoadLevelCache(Series),
                LoadRankCache(Series),
                LoadLevelRankCache(Series),
                LoadFrameCache(Series, Rare),
                LoadLinkArrowNormalCache(Series, Rare),
                LoadLinkArrowPenCache(Series, Rare),
                LoadPowerCache(Series),
                LoadOCGFontCache(Series),
                LoadTCGFontCache(Series),
                LoadFallbackFontCache(Series)
            };
            
            var results = await Task.WhenAll(loadTasks);
            bool allTasksSucceeded = results.All(result => result);
            IsLoadedImageCache = allTasksSucceeded;
            IsChangedRare = false;
            return allTasksSucceeded;
        }
        public bool LoadImageConfig(int Series)
        {
            try
            {
                switch (Series)
                {
                    case 0: //Series 1
                        CurrentImageInfo = Image1Config.Create();
                        CurrentLinkArrowInfo = LinkArrow1Config.Create();
                        break;
                    case 1: //Series 2
                        CurrentImageInfo = Image2Config.Create();
                        CurrentLinkArrowInfo = LinkArrow2Config.Create();
                        break;
                    case 2: //Series 3~8
                        CurrentImageInfo = Image38Config.Create();
                        CurrentLinkArrowInfo = LinkArrow38Config.Create();
                        break;
                    case 3: //Series 9
                        CurrentImageInfo = Image9Config.Create();
                        CurrentLinkArrowInfo = LinkArrow9Config.Create();
                        break;
                    case 4: //Series 10
                        CurrentImageInfo = Image10Config.Create();
                        CurrentLinkArrowInfo = LinkArrow10Config.Create();
                        break;
                    case 5: //Rush
                        CurrentImageInfo = ImageRushConfig.Create();
                        CurrentLinkArrowInfo = LinkArrowRushConfig.Create();
                        break;
                    case 6: //ForKid
                        CurrentImageInfo = ImageForKidConfig.Create();
                        CurrentLinkArrowInfo = LinkArrowForKidConfig.Create();
                        break;
                    case 7: //Manga
                        CurrentImageInfo = ImageMangaConfig.Create();
                        CurrentLinkArrowInfo = LinkArrowMangaConfig.Create();
                        break;
                    default: throw new Exception("Series config not found.");
                }
                IsChangedSeries = false;
                return true;
            }
            catch
            {
                IsChangedSeries = true;
                return false;
            }
        }
        public async Task<bool> ReloadFrameImageCache(int Series, int Rare)
        {
            var loadTasks = new List<Task<bool>>
            {
                LoadFrameCache(Series, Rare),
                LoadLinkArrowNormalCache(Series, Rare),
                LoadLinkArrowPenCache(Series, Rare),
            };
            var results = await Task.WhenAll(loadTasks);
            bool allTasksSucceeded = results.All(result => result);
            IsChangedRare = allTasksSucceeded;
            return allTasksSucceeded;
        }
        private async Task<bool> LoadBackgroundCache(int Series)
        {
            string backgroundFolderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series}\Background");
            if (!Directory.Exists(backgroundFolderPath)) return false;

            try
            {
                foreach (var item in BackgroundCache)
                {
                    item.Value?.Dispose();
                }
                BackgroundCache.Clear();

                var imageFiles = Directory.GetFiles(backgroundFolderPath, "*.png").Select(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    return ulong.TryParse(name, out ulong mask) ? new { FileName = Path.GetFileName(f), Mask = mask } : null;
                }).Where(x => x != null).OrderByDescending(x => x.Mask).ToList();

                var tasks = imageFiles.Select(f => LoadImageAsync(backgroundFolderPath, f.FileName));
                var images = await Task.WhenAll(tasks);

                for (int i = 0; i < imageFiles.Count; i++)
                {
                    if (images[i] != null)
                    {
                        BackgroundCache.Add(new KeyValuePair<ulong, SKBitmap>(imageFiles[i].Mask, images[i]));
                    }
                }
                OnPropertyChanged(nameof(BackgroundCache));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> LoadBackgroundArtCache(int Series)
        {
            string backgroundArtFolderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series}\BackgroundArt");
            if (!Directory.Exists(backgroundArtFolderPath)) return false;

            try
            {
                foreach (var item in BackgroundArtCache)
                {
                    item.Value?.Dispose();
                }
                BackgroundArtCache.Clear();

                var imageFiles = Directory.GetFiles(backgroundArtFolderPath, "*.png")
                .Select(Path.GetFileName).ToList();

                var tasks = imageFiles.Select(fileName => LoadImageAsync(backgroundArtFolderPath, fileName));
                var images = await Task.WhenAll(tasks);
                for (int i = 0; i < imageFiles.Count; i++)
                {
                    if (images[i] != null)
                    {
                        var key = Path.GetFileNameWithoutExtension(imageFiles[i]);
                        BackgroundArtCache[key] = images[i];
                    }
                }
                OnPropertyChanged(nameof(BackgroundArtCache));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> LoadSecretCache(int Series)
        {
            string SecretFolderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series}\Secret");
            if (!Directory.Exists(SecretFolderPath)) return false;

            try
            {
                foreach (var item in SecretCache)
                {
                    item.Value?.Dispose();
                }
                SecretCache.Clear();

                var imageFiles = Directory.GetFiles(SecretFolderPath, "*.png")
                .Select(Path.GetFileName).ToList();

                var tasks = imageFiles.Select(fileName => LoadImageAsync(SecretFolderPath, fileName));
                var images = await Task.WhenAll(tasks);
                for (int i = 0; i < imageFiles.Count; i++)
                {
                    if (images[i] != null)
                    {
                        var key = Path.GetFileNameWithoutExtension(imageFiles[i]);
                        SecretCache[key] = images[i];
                    }
                }
                OnPropertyChanged(nameof(SecretCache));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> LoadFoildCache(int Series)
        {
            string FoildFolderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series}\Foild");
            if (!Directory.Exists(FoildFolderPath)) return false;

            try
            {
                foreach (var item in FoildCache)
                {
                    item.Value?.Dispose();
                }
                FoildCache.Clear();

                var imageFiles = Directory.GetFiles(FoildFolderPath, "*.png")
                .Select(Path.GetFileName).ToList();

                var tasks = imageFiles.Select(fileName => LoadImageAsync(FoildFolderPath, fileName));
                var images = await Task.WhenAll(tasks);
                for (int i = 0; i < imageFiles.Count; i++)
                {
                    if (images[i] != null)
                    {
                        var key = Path.GetFileNameWithoutExtension(imageFiles[i]);
                        FoildCache[key] = images[i];
                    }
                }
                OnPropertyChanged(nameof(FoildCache));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> LoadAttributeCache(int Series)
        {
            string attributeFolderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series}\Attribute");
            if (!Directory.Exists(attributeFolderPath)) return false;
            try
            {
                foreach (var item in AttributeCache)
                {
                    item.Value?.Dispose();
                }
                AttributeCache.Clear();

                var imageFiles = Directory.GetFiles(attributeFolderPath, "*.png").Select(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    return ulong.TryParse(name, out ulong mask) ? new { FileName = Path.GetFileName(f), Mask = mask } : null;
                }).Where(x => x != null).OrderByDescending(x => x.Mask).ToList();

                var tasks = imageFiles.Select(f => LoadImageAsync(attributeFolderPath, f.FileName));
                var images = await Task.WhenAll(tasks);

                for (int i = 0; i < imageFiles.Count; i++)
                {
                    if (images[i] != null)
                    {
                        AttributeCache.Add(new KeyValuePair<ulong, SKBitmap>(imageFiles[i].Mask, images[i]));
                    }
                }
                OnPropertyChanged(nameof(AttributeCache));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> LoadSpellTrapCache(int Series)
        {
            string spelltrapFolderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series}\SpellTrap");
            if (!Directory.Exists(spelltrapFolderPath)) return false;

            try
            {
                foreach (var item in SpellTrapCache)
                {
                    item.Value?.Dispose();
                }
                SpellTrapCache.Clear();

                var imageFiles = Directory.GetFiles(spelltrapFolderPath, "*.png").Select(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    return ulong.TryParse(name, out ulong mask) ? new { FileName = Path.GetFileName(f), Mask = mask } : null;
                }).Where(x => x != null).OrderByDescending(x => x.Mask).ToList();

                var tasks = imageFiles.Select(f => LoadImageAsync(spelltrapFolderPath, f.FileName));
                var images = await Task.WhenAll(tasks);

                for (int i = 0; i < imageFiles.Count; i++)
                {
                    if (images[i] != null)
                    {
                        SpellTrapCache.Add(new KeyValuePair<ulong, SKBitmap>(imageFiles[i].Mask, images[i]));
                    }
                }
                OnPropertyChanged(nameof(SpellTrapCache));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> LoadLevelCache(int Series)
        {
            string levelFolderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series}\Stars\Level");
            if (!Directory.Exists(levelFolderPath)) return false;

            try
            {
                foreach (var item in LevelCache)
                {
                    item.Value?.Dispose();
                }
                LevelCache.Clear();

                var imageFiles = Directory.GetFiles(levelFolderPath, "*.png").Select(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    return ulong.TryParse(name, out ulong key) ? new { FileName = Path.GetFileName(f), Key = key } : null;
                }).Where(x => x != null).ToList();

                var tasks = imageFiles.Select(f => LoadImageAsync(levelFolderPath, f.FileName));
                var images = await Task.WhenAll(tasks);

                for (int i = 0; i < imageFiles.Count; i++)
                {
                    if (images[i] != null)
                    {
                        LevelCache[imageFiles[i].Key] = images[i];
                    }
                }
                OnPropertyChanged(nameof(LevelCache));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> LoadRankCache(int Series)
        {
            string rankFolderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series}\Stars\Rank");
            if (!Directory.Exists(rankFolderPath)) return false;

            try
            {
                foreach (var item in RankCache)
                {
                    item.Value?.Dispose();
                }
                RankCache.Clear();

                var imageFiles = Directory.GetFiles(rankFolderPath, "*.png").Select(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    return ulong.TryParse(name, out ulong key) ? new { FileName = Path.GetFileName(f), Key = key } : null;
                }).Where(x => x != null).ToList();

                var tasks = imageFiles.Select(f => LoadImageAsync(rankFolderPath, f.FileName));
                var images = await Task.WhenAll(tasks);

                for (int i = 0; i < imageFiles.Count; i++)
                {
                    if (images[i] != null)
                    {
                        RankCache[imageFiles[i].Key] = images[i];
                    }
                }
                OnPropertyChanged(nameof(RankCache));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> LoadLevelRankCache(int Series)
        {
            string levelRankFolderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series}\Stars\LevelRank");
            if (!Directory.Exists(levelRankFolderPath)) return false;

            try
            {
                foreach (var item in LevelRankCache)
                {
                    item.Value?.Dispose();
                }
                LevelRankCache.Clear();

                var imageFiles = Directory.GetFiles(levelRankFolderPath, "*.png").Select(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    return ulong.TryParse(name, out ulong key) ? new { FileName = Path.GetFileName(f), Key = key } : null;
                }).Where(x => x != null).ToList();

                var tasks = imageFiles.Select(f => LoadImageAsync(levelRankFolderPath, f.FileName));
                var images = await Task.WhenAll(tasks);

                for (int i = 0; i < imageFiles.Count; i++)
                {
                    if (images[i] != null)
                    {
                        LevelRankCache[imageFiles[i].Key] = images[i];
                    }
                }
                OnPropertyChanged(nameof(LevelRankCache));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> LoadFrameCache(int Series, int Rare)
        {
            string frameFolderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series}\Frame\{Rare}");
            if (!Directory.Exists(frameFolderPath)) return false;
            try
            {
                foreach (var item in FrameCache)
                {
                    item.Value?.Dispose();
                }
                FrameCache.Clear();

                var imageFiles = Directory.GetFiles(frameFolderPath, "*.png").Select(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    return int.TryParse(name, out int key) ? new { FileName = Path.GetFileName(f), Key = key } : null;
                }).Where(x => x != null).ToList();

                var tasks = imageFiles.Select(f => LoadImageAsync(frameFolderPath, f.FileName));
                var images = await Task.WhenAll(tasks);

                for (int i = 0; i < imageFiles.Count; i++)
                {
                    if (images[i] != null)
                    {
                        FrameCache[imageFiles[i].Key] = images[i];
                    }
                }
                OnPropertyChanged(nameof(FrameCache));
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<SKBitmap> LoadImageAsync(string folderPath, string fileName)
        {
            try
            {
                return await Task.Run(() =>
                {
                    string fullPath = Path.Combine(folderPath, fileName);
                    using (var stream = new SKFileStream(fullPath))
                    {
                        var bitmap = SKBitmap.Decode(stream);
                        return bitmap; // SKBitmap sẽ được quản lý bởi caller
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading background {fileName}: {ex.Message}");
                return null;
            }
        }

        private async Task<bool> LoadLinkArrowNormalCache(int Series, int Rare)
        {
            string folderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series.ToString()}\LinkArrow\Normal\{Rare.ToString()}");
            if (!Directory.Exists(folderPath)) return false;

            try
            {
                foreach (var item in LinkArrowNormalCache)
                {
                    item.Value?.Dispose();
                }
                LinkArrowNormalCache.Clear();

                var codes = new[] { "bl", "b", "br", "l", "r", "tl", "t", "tr" };
                var states = new[] { "on", "off" };

                var tasks = new List<Task<(string key, SKBitmap image)>>();

                foreach (var code in codes)
                {
                    foreach (var state in states)
                    {
                        string fileName = $"{code}_{state}.png";
                        tasks.Add(LoadArrowImage(folderPath, code, state, fileName));
                    }
                }
                var results = await Task.WhenAll(tasks);
                foreach (var (key, image) in results)
                {
                    if (image != null)
                        LinkArrowNormalCache[key] = image;
                }
                OnPropertyChanged(nameof(LinkArrowNormalCache));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> LoadLinkArrowPenCache(int Series, int Rare)
        {
            string folderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series}\LinkArrow\Pendulum\{Rare}");
            if (!Directory.Exists(folderPath)) return false;

            try
            {
                foreach (var item in LinkArrowPenCache)
                {
                    item.Value?.Dispose();
                }
                LinkArrowPenCache.Clear();

                var codes = new[] { "bl", "b", "br", "l", "r", "tl", "t", "tr" };
                var states = new[] { "on", "off" };

                var tasks = new List<Task<(string key, SKBitmap image)>>();
                foreach (var code in codes)
                {
                    foreach (var state in states)
                    {
                        string fileName = $"{code}_{state}.png";
                        tasks.Add(LoadArrowImage(folderPath, code, state, fileName));
                    }
                }
                var results = await Task.WhenAll(tasks);
                foreach (var (key, image) in results)
                {
                    if (image != null)
                        LinkArrowPenCache[key] = image;
                }
                OnPropertyChanged(nameof(LinkArrowPenCache));
                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<(string key, SKBitmap image)> LoadArrowImage(string folder, string code, string state, string fileName)
        {
            var image = await LoadImageAsync(folder, fileName);
            return ($"{code}_{state}", image);
        }
        private async Task<bool> LoadPowerCache(int Series)
        {
            string powerFolderPath = Path.Combine(DataFolderPath, $@"CardImage\{Series}\Power");
            if (!Directory.Exists(powerFolderPath)) return false;
            try
            {
                foreach (var item in PowerCache)
                {
                    item.Value?.Dispose();
                }
                PowerCache.Clear();

                var imageFiles = Directory.GetFiles(powerFolderPath, "*.png").Select(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    return int.TryParse(name, out int key) ? new { FileName = Path.GetFileName(f), Key = key } : null;
                }).Where(x => x != null).ToList();


                var tasks = imageFiles.Select(f => LoadImageAsync(powerFolderPath, f.FileName));
                var images = await Task.WhenAll(tasks);

                for (int i = 0; i < imageFiles.Count; i++)
                {
                    if (images[i] != null)
                    {
                        PowerCache[imageFiles[i].Key] = images[i];
                    }
                }
                OnPropertyChanged(nameof(PowerCache));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LoadOCGFontCache(int series)
        {
            string fontFolder = Path.Combine(DataFolderPath, $@"CardImage\{series}\Font\0");
            if (!Directory.Exists(fontFolder))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    $"Font folder not found: {fontFolder}.", new[] { CMess.ok.ToText() });
                return false;
            }
            return await Task.Run(() =>
            {
                try
                {
                    foreach (var item in FontOCGCache)
                    {
                        item.Value?.Dispose();
                    }
                    FontOCGCache.Clear();

                    var fontFiles = Directory.GetFiles(fontFolder, "*.ttf").Concat(Directory.GetFiles(fontFolder, "*.otf"));
                    foreach (var fontPath in fontFiles)
                    {
                        try
                        {
                            var typeface = SKTypeface.FromFile(fontPath);
                            string fontName = Path.GetFileNameWithoutExtension(fontPath);
                            FontOCGCache[fontName] = typeface;
                        }
                        catch (Exception ex)
                        {
                            // Log the error or show a message
                            Debug.WriteLine($"Failed to load font {fontPath}: {ex.Message}");
                        }
                    }
                    OnPropertyChanged(nameof(FontOCGCache));
                    if (FontOCGCache.Count == 0) return false;
                    return true;
                }
                catch
                {
                    return false;
                }
            }).ContinueWith(task =>
            {
                OnPropertyChanged(nameof(FontOCGCache));
                return task.Result;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        public async Task<bool> LoadTCGFontCache(int series)
        {
            string fontFolder = Path.Combine(DataFolderPath, $@"CardImage\{series}\Font\1");
            if (!Directory.Exists(fontFolder))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    $"Font folder not found: {fontFolder}.", new[] { CMess.ok.ToText() });
                return false;
            }
            return await Task.Run(() =>
            {
                try
                {
                    foreach (var item in FontTCGCache)
                    {
                        item.Value?.Dispose();
                    }
                    FontTCGCache.Clear();

                    var fontFiles = Directory.GetFiles(fontFolder, "*.ttf").Concat(Directory.GetFiles(fontFolder, "*.otf"));
                    foreach (var fontPath in fontFiles)
                    {
                        try
                        {
                            var typeface = SKTypeface.FromFile(fontPath);
                            string fontName = Path.GetFileNameWithoutExtension(fontPath);
                            FontTCGCache[fontName] = typeface;
                        }
                        catch (Exception ex)
                        {
                            // Log the error or show a message
                            Debug.WriteLine($"Failed to load font {fontPath}: {ex.Message}");
                        }
                    }
                    OnPropertyChanged(nameof(FontTCGCache));
                    if (FontTCGCache.Count == 0) return false;
                    return true;
                }
                catch
                {
                    return false;
                }
            }).ContinueWith(task =>
            {
                OnPropertyChanged(nameof(FontTCGCache));
                return task.Result;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        public async Task<bool> LoadFallbackFontCache(int series)
        {
            string fontFolder = Path.Combine(DataFolderPath, $@"CardImage\{series}\FontFallback");
            if (!Directory.Exists(fontFolder))
            {
                CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                    $"Font folder not found: {fontFolder}.", new[] { CMess.ok.ToText() });
                return false;
            }

            return await Task.Run(() =>
            {
                try
                {
                    _fallbackTypefaceOCG = null;
                    _fallbackTypefaceTCG = null;

                    string _fontfallbackOCG = Path.Combine(fontFolder, "Fallback0.ttf");
                    string _fontfallbackTCG = Path.Combine(fontFolder, "Fallback1.ttf");
                    if (!System.IO.File.Exists(_fontfallbackOCG) || !System.IO.File.Exists(_fontfallbackTCG)) return false;

                    _fallbackTypefaceOCG = SKTypeface.FromFile(_fontfallbackOCG);
                    _fallbackTypefaceTCG = SKTypeface.FromFile(_fontfallbackTCG);

                    return true;
                }
                catch
                {
                    return false;
                }
            }).ContinueWith(task =>
            {
                OnPropertyChanged(nameof(FontTCGCache));
                return task.Result;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public SKTypeface GetFont(string fontName, int Format, SKFontStyleWeight weight = SKFontStyleWeight.Normal)
        {
            var targetCache = Format == 0 ? FontOCGCache : FontTCGCache;
            if (targetCache.TryGetValue(fontName, out var typeface) && typeface != null)
                return typeface;
            CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning, $"Font not in cache: {fontName}. Using fallback.", new[] { CMess.ok.ToText() });
            return SKTypeface.FromFamilyName("Arial", weight, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        }
        public void ClearFontCache()
        {
            foreach (var typeface in FontOCGCache.Values)
            {
                typeface?.Dispose();
            }
            FontOCGCache.Clear();
            foreach (var typeface in FontTCGCache.Values)
            {
                typeface?.Dispose();
            }
            FontTCGCache.Clear();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
