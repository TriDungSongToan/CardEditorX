using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SkiaSharp;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Manager;
using CardEditor.Services;
using CardEditor.ImageGene;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CardEditor.Models.Settings;
using CMess = CardEditor.Localization.Language;
using CardAppContext = CardEditor.Models.AppContext;

namespace CardEditor.ImageGene
{
    public class ImageValidate
    {
        public static bool isValid = false;
        private static readonly object _lock = new object();
        private static readonly string _sizePattern = @"^\d+,\d+$";
        public static void MarkDirty()
        {
            isValid = false;
        }
        public static async Task<(bool, string)> ReLoadImageData()
        {
            try
            {
                bool[] results = await Task.WhenAll(
                GeneraImageViewModel.Instance.LoadImageCache(
                    ConfigViewModel.Instance.imageSetting.Series,
                    ConfigViewModel.Instance.imageSetting.Rare),
                RareRawDataViewModel.Instance.LoadImageCache());

                if (results.Any(result => result == false))
                    return (false, CMess.errorLoadImageCache.ToText());

                RareRawDataViewModel.Instance.SetRarityLabelRect();
                lock (_lock)
                {
                    isValid = true;
                }
                return (true, string.Empty);
            }
            catch
            {
                lock (_lock)
                {
                    isValid = false;
                }
                return (false, CMess.errorLoadImageCache.ToText());
            }
        }
        public static async Task<(bool, string)> ReloadFrameImageCache()
        {
            if (!GeneraImageViewModel.Instance.IsChangedRare) return (true, string.Empty);

            bool result = await GeneraImageViewModel.Instance.ReloadFrameImageCache(
                ConfigViewModel.Instance.imageSetting.Series,
                ConfigViewModel.Instance.imageSetting.Rare);
            if (!result) return (false, CMess.errorLoadImageCache.ToText());
            lock (_lock)
            {
                isValid = true;
            }
            return (true, string.Empty);
        }
        public static async Task<(bool, string)> CheckValidate()
        {
            try
            {
                if (isValid) return (true, string.Empty);

                if (!Regex.IsMatch(ConfigViewModel.Instance.imageSetting.ImageSizeString, _sizePattern) ||
                    !Regex.IsMatch(ConfigViewModel.Instance.imageSetting.StampSizeString, _sizePattern) ||
                    !Regex.IsMatch(ConfigViewModel.Instance.imageSetting.StampMarrginString, _sizePattern))
                    return (false, CMess.invaSetting.ToText());

                if (string.IsNullOrWhiteSpace(ConfigViewModel.Instance.imageSetting.OutPutFolder) ||
                    ConfigViewModel.Instance.imageSetting.OutPutFolder.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0 ||
                    !System.IO.Path.IsPathRooted(ConfigViewModel.Instance.imageSetting.OutPutFolder) ||
                    !Directory.Exists(ConfigViewModel.Instance.imageSetting.OutPutFolder) ||
                    !await HasReadWritePermission(ConfigViewModel.Instance.imageSetting.OutPutFolder))
                    return (false, CMess.invaSetting.ToText());

                if (ConfigViewModel.Instance.imageSetting.StampPosition < 0 ||
                    string.IsNullOrEmpty(ConfigViewModel.Instance.imageSetting.BackgroundArt) ||
                    string.IsNullOrEmpty(ConfigViewModel.Instance.imageSetting.Foild) ||
                    ConfigViewModel.Instance.imageSetting.Secret < 0 || ConfigViewModel.Instance.imageSetting.Secret > 3)
                    return (false, CMess.invaSetting.ToText());

                isValid = true;
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                isValid = false;
                return (false, $"{CMess.errorReadConf.ToText()} {ex.Message}");
            }
        }
        private static async Task<bool> HasReadWritePermission(string folderPath)
        {
            string tempFilePath = System.IO.Path.Combine(folderPath, System.IO.Path.GetRandomFileName());
            try
            {
                await Task.Run(() =>
                {
                    using (var stream = new FileStream(tempFilePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        stream.WriteByte(0x0);
                    }
                    using (var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                    {
                        int b = stream.ReadByte();
                    }
                    System.IO.File.Delete(tempFilePath);
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool TryDeleteFile(string filePath, int maxAttempts = 3)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    return true;
                }
                catch (IOException)
                {
                    if (i == maxAttempts - 1) throw;
                    Thread.Sleep(100); // Đợi 100ms trước khi thử lại
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
            return false;
        }
        public static async Task SaveImage(SKImage image, string filePath)
        {
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
            await data.AsStream().CopyToAsync(fs);
        }
        public static bool IsImageFile(string filePath)
        {
            try
            {
                using (var imageStream = new MemoryStream(File.ReadAllBytes(filePath)))
                using (var img = Image.FromStream(imageStream))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
