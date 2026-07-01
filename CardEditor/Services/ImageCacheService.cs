using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using CardEditor.Models;
using System.Threading;
using System.Threading.Tasks;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services
{
    public interface IImageCache : IDisposable
    {
        void Load();
        ImageSource Get(AppImage image);
    }
    public class ImageCacheService : IImageCache
    {
        private static readonly Lazy<ImageCacheService> _instance = new Lazy<ImageCacheService>(() =>
        {
            //var svc = new ImageCacheService();
            //svc.Load();
            //return svc;

            return new ImageCacheService();
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        public static ImageCacheService Instance => _instance.Value;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly Dictionary<AppImage, BitmapImage> ImageCache = new();
        private static readonly BitmapImage RollBack = CreateRollBack();
        private static BitmapImage CreateRollBack()
        {
            var wb = new WriteableBitmap(1, 1, 96, 96, PixelFormats.Bgra32, null);
            byte[] pixels = { 0, 0, 0, 0 };
            wb.WritePixels(new Int32Rect(0, 0, 1, 1), pixels, 4, 0);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(wb));

            using var ms = new MemoryStream();
            encoder.Save(ms);
            ms.Position = 0;

            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = ms;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();

            return image;
        }

        private static readonly Dictionary<AppImage, string> ImagePaths = new()
        {
            {AppImage.Error, "pack://application:,,,/CardEditor;component/Images/Message/Error.png" },
            {AppImage.Information, "pack://application:,,,/CardEditor;component/Images/Message/Information.png" },
            {AppImage.Notification, "pack://application:,,,/CardEditor;component/Images/Message/Notification.png" },
            {AppImage.Question, "pack://application:,,,/CardEditor;component/Images/Message/Question.png" },
            {AppImage.Warning, "pack://application:,,,/CardEditor;component/Images/Message/Warning.png" },

            {AppImage.Blank, "pack://application:,,,/Images/Blank.png" },
            {AppImage.Logo, "pack://application:,,,/Images/Logo.ico" },
            {AppImage.LevelStar, "pack://application:,,,/Images/DataEditor/LevelStar.ico" },
            {AppImage.RankStar, "pack://application:,,,/Images/DataEditor/RankStar.ico" },
            {AppImage.LevelRankStar, "pack://application:,,,/Images/DataEditor/LevelRankStar.ico" },
        };

        private bool _isLoaded;
        private Task _loadingTask;

        public async Task LoadAsync()
        {
            if (_isLoaded) return;

            await _semaphore.WaitAsync();
            string error = string.Empty;
            try
            {
                if (_isLoaded) return;

                await Task.Run(() =>
                {
                    foreach (var imgPath in ImagePaths)
                    {
                        var (bitmap, message) = LoadImage(imgPath.Value);
                        ImageCache[imgPath.Key] = bitmap;
                        error += string.IsNullOrWhiteSpace(message) ? string.Empty : $"{imgPath.Value} {message}";
                    }
                });

                if (!string.IsNullOrWhiteSpace(error))
                {
                    MessageBox.Show($"Error Load Image: {error}", "Error", MessageBoxButton.OK);
                }

                _isLoaded = true;
            }
            catch {  }
            finally
            {
                _semaphore.Release();
            }
        }
        public void Load()
        {
            LoadAsync().GetAwaiter().GetResult();
        }
        public static (BitmapImage, string) LoadImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return (RollBack, CMess.fileNotExit.ToText());
            try
            {
                Uri uri;

                if (path.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                    uri = new Uri(path, UriKind.Absolute);
                else if (System.IO.File.Exists(path))
                    uri = new Uri(System.IO.Path.GetFullPath(path), UriKind.Absolute);
                else
                    uri = new Uri(path, UriKind.Relative);

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = uri;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return (bmp, string.Empty);
            }
            catch (FileNotFoundException)
            {
                return (RollBack, CMess.fileNotExit.ToText());
            }
            catch (NotSupportedException)
            {
                return (RollBack, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()));
            }
            catch (IOException)
            {
                return (RollBack, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Operation.ToText()));
            }
            catch (Exception ex)
            {
                return (RollBack, ex.Message);
            }
        }
        public ImageSource Get(AppImage image)
        {
            if (!_isLoaded && _loadingTask == null) _loadingTask = LoadAsync();

            if (ImageCache.TryGetValue(image, out var img))  return img;

            return RollBack;
        }
        public BitmapImage GetBitMap(AppImage image)
        {
            return (ImageCache.TryGetValue(image, out var img) ? img : RollBack);
        }

        public static void Shutdown()
        {
            if (_instance.IsValueCreated)
                _instance.Value.Dispose();
        }
        public void Dispose()
        {
            ImageCache.Clear();
            _isLoaded = false;
        }
    }
}
