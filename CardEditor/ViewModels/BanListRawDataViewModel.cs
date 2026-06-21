using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Generic;
using CardEditor.Models;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using CardAppContext = CardEditor.Models.AppContext;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;

namespace CardEditor.ViewModels
{
    public class BanListRawDataViewModel : IDisposable
    {
        private static readonly Lazy<BanListRawDataViewModel> _instance
            = new Lazy<BanListRawDataViewModel>(() => new BanListRawDataViewModel());
        public static BanListRawDataViewModel Instance => _instance.Value;

        public BulkObservableCollection<CardEditor.Models.BanList> BanLists { get; set; }
            = new BulkObservableCollection<CardEditor.Models.BanList>();

        public readonly Dictionary<int, BitmapImage> _limitImages;
        private readonly BitmapImage _blankImage;

        public bool IsLoaded = false;

        private BanListRawDataViewModel()
        {
            _limitImages = new Dictionary<int, BitmapImage>();
            _blankImage = CreateBlankImage();
        }
        public void LoadLimitImages()
        {
            string[] imagePaths = new[]
            {
                "Images/Textures/0.png",
                "Images/Textures/1.png",
                "Images/Textures/2.png",
                "Images/Textures/3.png"
            };

            for (int i = 0; i < imagePaths.Length; i++)
            {
                try
                {
                    var image = LoadOptimizedImage(imagePaths[i]);
                    if (image != null)
                    {
                        _limitImages[i] = image;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load limit image {i}: {ex.Message}");
                }
            }
        }
        private BitmapImage LoadOptimizedImage(string path)
        {
            if (!File.Exists(path)) return null;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 25;
            bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }
        private BitmapImage CreateBlankImage()
        {
            // Tạo WriteableBitmap 1x1 pixel transparent
            var wb = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Pbgra32, null);

            // Tạo pixel trong suốt ARGB = 0
            byte[] pixel = new byte[4] { 0, 0, 0, 0 }; // B G R A
            wb.WritePixels(new Int32Rect(0, 0, 1, 1), pixel, 4, 0);

            // Chuyển WriteableBitmap sang BitmapImage
            var bitmap = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wb));
                encoder.Save(stream);

                stream.Position = 0;
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
            }

            return bitmap;
        }
        public BitmapImage GetLimitImage(int limitCount)
        {
            if (limitCount > 3) return null; // Không hiển thị
            if (limitCount <= 0) return _limitImages.TryGetValue(0, out var ingBan) ? ingBan : _blankImage;
            return _limitImages.TryGetValue(limitCount, out var image) ? image : _blankImage;
        }
        public bool HasImage(int limitCount)
        {
            if (limitCount <= 0) return _limitImages.ContainsKey(0);

            return limitCount <= 3 && _limitImages.ContainsKey(limitCount);
        }

        public async Task<(bool, string)> LoadBanLists(CancellationToken cancellationToken = default)
        {
            try
            {
                if (!System.IO.Directory.Exists(CardAppContext.Instance.BanListFolderPath))
                    Directory.CreateDirectory(CardAppContext.Instance.BanListFolderPath);
                var files = Directory.GetFiles(CardAppContext.Instance.BanListFolderPath, "*.lflist.conf");

                var templateList = new List<BanList>();

                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var banList = await LoadFileBanList(file);
                    if (banList != null)
                    {
                        templateList.Add(banList);
                    }
                }
                BanLists.AddRange(templateList);
                IsLoaded = true;
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<BanList> LoadFileBanList(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            var banList = new BanList
            {
                Name = string.Empty,
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                CardList = new Dictionary<ulong, CardBanList>(),
                WhiteList = false
            };

            try
            {
                const int bufferSize = 4096;
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true);
                using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: bufferSize, leaveOpen: false);

                string line;

                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

                    switch (line[0])
                    {
                        case '!': banList.Name = ParseBanListName(line); break;
                        case '$': banList.WhiteList = true; break;
                        default:
                            var card = ParseCardLine(line);
                            if (card != null)
                                banList.CardList[card.Id] = card;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading banlist: {ex.Message}");
                return null;
            }
            return banList;
        }
        private string ParseBanListName(string rawLine)
        {
            if (string.IsNullOrWhiteSpace(rawLine))
                return string.Empty;
            var name = rawLine.Substring(1).Trim();
            if (name.StartsWith("["))
                name = name.Substring(1).Trim();
            if (name.EndsWith("]"))
                name = name.Substring(0, name.Length - 1).Trim();

            return name;
        }
        private CardBanList ParseCardLine(string line)
        {
            var parts = line.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return null;

            if (!ulong.TryParse(parts[0], out var id)) return null;
            if (!int.TryParse(parts[1], out var count)) return null;
            int countFinal;
            if (count <= 0) countFinal = 0;
            else if (count > 3) countFinal = 3;
            else countFinal = count;

            var name = parts[2].TrimStart('-', ' ');
            return new CardBanList
            {
                Id = id,
                Name = name,
                LimitedCount = countFinal
            };
        }

        public BanList FindById(Guid id, bool useCache = false)
        {
            var options = new SearchOptions
            {
                UseCache = useCache,
                CacheKey = useCache ? $"banlist_id_{id}" : null
            };

            return BanLists.Search(bl => bl.ID == id, options).FirstOrDefault();
        }

        public (bool, string) UpdateBanList(Guid banListId, string name = null, string fileName = null, string filePath = null,
            bool? whiteList = null, CardBanList card = null)
        {
            try
            {
                var banList = BanLists.FirstOrDefault(b => b.ID == banListId);
                if (banList == null) return (false, "BanList is does not exist.");
                if (name != null) banList.Name = name;
                if (fileName != null) banList.FileName = fileName;
                if (filePath != null) banList.FilePath = filePath;
                if (whiteList != null) banList.WhiteList = whiteList == true;
                if (card != null)
                {
                    banList.CardList[card.Id] = card;
                }
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<(bool, string)> SaveBanListFile(BanList banList, string path = null)
        {
            try
            {
                string filePath = (!string.IsNullOrWhiteSpace(path)) ? path : banList.FilePath;
                if (!System.IO.Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                var (banned, limited, semiLimited, unLimited) = SplitCardList(banList.CardList);

                const int bufferSize = 4096;
                using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write,
                    FileShare.None, bufferSize, useAsync: true);
                using var writer = new StreamWriter(fs, Encoding.UTF8, bufferSize, leaveOpen: false);

                StringBuilder banlistString = new StringBuilder();
                banlistString.AppendLine($"#[{banList.Name}]");
                banlistString.AppendLine($"![{banList.Name}]");
                if (banList.WhiteList)
                    banlistString.AppendLine("$whitelist");

                banlistString.AppendLine("#Forbidden");
                foreach (var card in banned)
                {
                    banlistString.AppendLine($"{card.Id} {card.LimitedCount} --{card.Name}");
                }

                banlistString.AppendLine("#Limited");
                foreach (var card in limited)
                {
                    banlistString.AppendLine($"{card.Id} {card.LimitedCount} --{card.Name}");
                }

                banlistString.AppendLine("#Semi-limited");
                foreach (var card in semiLimited)
                {
                    banlistString.AppendLine($"{card.Id} {card.LimitedCount} --{card.Name}");
                }

                banlistString.AppendLine("#Unlimited");
                foreach (var card in unLimited)
                {
                    banlistString.AppendLine($"{card.Id} {card.LimitedCount} --{card.Name}");
                }
                await writer.WriteLineAsync(banlistString.ToString()).ConfigureAwait(false);

                await writer.FlushAsync().ConfigureAwait(false);
                return (true, filePath);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (List<CardBanList> banned, List<CardBanList> limited,
            List<CardBanList> semiLimited, List<CardBanList> unLimited)
        SplitCardList(Dictionary<ulong, CardBanList> cardList)
        {
            var banned = new List<CardBanList>();
            var limited = new List<CardBanList>();
            var semi = new List<CardBanList>();
            var unLimited = new List<CardBanList>();

            foreach (var card in cardList.Values)
            {
                if (card == null) continue;
                switch (card.LimitedCount)
                {
                    case <= 0: banned.Add(card); break;
                    case 1: limited.Add(card); break;
                    case 2: semi.Add(card); break;
                    default: unLimited.Add(card); break;
                }
            }
            return (banned, limited, semi, unLimited);
        }

        public (bool, string) DeleteBanList(BanList banList)
        {
            if (banList == null) return (false, CMess.fileNotExit.ToText());
            try
            {
                string path = banList.FilePath;
                if (!string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        return (false, $"{CMess.errorOcc.ToText()} {ex.Message}");
                    }
                }
                if (BanLists.Contains(banList))
                {
                    BanLists.Remove(banList);
                }
                BanLists.InvalidateSearchCache();
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public void Dispose()
        {
            BanLists.Clear();
            _limitImages.Clear();
        }
    }
}
