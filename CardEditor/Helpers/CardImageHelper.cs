using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CardEditor.Models;
using CardEditor.Services;

namespace CardEditor.Helpers
{
    public static class CardImageHelper
    {
        private static BitmapImage _blankImage;
        private static BitmapImage _loadingImage;
        private static readonly object _lock = new();

        public static BitmapImage BlankImage
        {
            get
            {
                if (_blankImage == null)
                {
                    lock (_lock)
                    {
                        if (_blankImage == null)
                        {
                            _blankImage = ResizeBitmap(ImageCacheService.Instance.GetBitMap(AppImage.Blank), 80);
                        }
                    }
                }
                return _blankImage;
            }
        }
        public static BitmapImage LoadingImage
        {
            get
            {
                if (_loadingImage == null)
                {
                    _loadingImage = LoadResourceImage("pack://application:,,,/Images/Loading.gif");
                }
                return _loadingImage;
            }
        }
        private static BitmapImage LoadResourceImage(string uri)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(uri, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.DecodePixelWidth = 80;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        public static BitmapImage ResizeBitmap(BitmapImage source, int targetHeight)
        {
            double scale = (double)targetHeight / source.PixelHeight;

            var transformed = new TransformedBitmap(
                source,
                new ScaleTransform(scale, scale)
            );

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(transformed));

            using var ms = new MemoryStream();
            encoder.Save(ms);
            ms.Position = 0;

            var result = new BitmapImage();
            result.BeginInit();
            result.StreamSource = ms;
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.EndInit();
            result.Freeze();

            return result;
        }
    }
}
