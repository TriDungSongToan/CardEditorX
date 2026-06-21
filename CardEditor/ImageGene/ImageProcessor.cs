#pragma warning disable CS0618
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Collections.Generic;
using SkiaSharp;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Manager;
using CardEditor.Services;
using CardEditor.ImageGene;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CardEditor.ImagesConfig;
using CMess = CardEditor.Localization.Language;
using CardAppContext = CardEditor.Models.AppContext;

namespace CardEditor.ImageGene
{
    public class ImageProcessor
    {
        public static string CreateArtWork0(string password, bool pendulum, string imagePath, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || string.IsNullOrWhiteSpace(targetPath))
                return string.Empty;
            if (!File.Exists(imagePath) || !ImageValidate.IsImageFile(imagePath))
                return string.Empty;

            int DPIImage = 100;
            int newimgwidth = pendulum ? 1205 : 1053;
            int newimgHight = pendulum ? 1205 : 1053;
            try
            {
                using (var imageStream = new MemoryStream(File.ReadAllBytes(imagePath)))
                using (var originalImage = Image.FromStream(imageStream))
                using (var newImage = new Bitmap(newimgwidth, newimgHight, PixelFormat.Format32bppArgb))
                {
                    newImage.SetResolution(DPIImage, DPIImage);
                    using (var graphics = Graphics.FromImage(newImage))
                    {
                        graphics.Clear(Color.Transparent);
                        graphics.DrawImage(originalImage, 0, 0, newimgwidth, newimgHight);
                    }

                    if(!System.IO.Directory.Exists(targetPath))
                        System.IO.Directory.CreateDirectory(targetPath);

                    var outputFilePath = Path.Combine(targetPath, $"{password}.png");
                    if (!ImageValidate.TryDeleteFile(outputFilePath))
                    {
                        CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                            CMess.unableDelete.ToText(), new[] { CMess.ok.ToText() });
                        return string.Empty;
                    }
                    newImage.Save(outputFilePath, ImageFormat.Png);
                    return outputFilePath;
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorCreaImg.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return string.Empty;
            }
        }
        public static async Task<string> CreateArtWork(string password, bool pendulum, string imagePath, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(targetPath) ||
                string.IsNullOrWhiteSpace(imagePath) || !System.IO.File.Exists(imagePath) || !ImageValidate.IsImageFile(imagePath))
                return string.Empty;

            int newimgwidth = pendulum ? 1205 : 1053;
            int newimgHight = pendulum ? 1205 : 1053;

            return await Task.Run(async () =>
            {
                try
                {
                    var info = new SKImageInfo(newimgwidth, newimgHight, SKColorType.Rgba8888, SKAlphaType.Premul);
                    using (var surface = SKSurface.Create(info))
                    {
                        var canvas = surface.Canvas;
                        canvas.Clear(SKColors.Transparent);

                        using var paint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };
                        using var stream = File.OpenRead(imagePath);
                        using var originalBitmap = SKBitmap.Decode(stream);
                        if (originalBitmap == null) return string.Empty;

                        var destRect = new SKRect(0, 0, newimgwidth, newimgHight);
                        canvas.DrawBitmap(originalBitmap, destRect, paint);

                        #region Save
                        var outputFilePath = System.IO.Path.Combine(targetPath, $"{password}.png");
                        if (!ImageValidate.TryDeleteFile(outputFilePath)) return string.Empty;

                        using (var image = surface.Snapshot())
                            await ImageValidate.SaveImage(image, outputFilePath);
                        #endregion

                        return outputFilePath;
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            });
        }

        public static string CreateImageCard0(string password, string imagePath, string targetPath, string stampPath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || string.IsNullOrWhiteSpace(targetPath))
                return string.Empty;
            if (!File.Exists(imagePath) || !ImageValidate.IsImageFile(imagePath))
                return string.Empty;

            // Đọc thông số từ setting
            System.Drawing.Size imageSize = ConfigViewModel.Instance.imageSetting.ImageSize;
            int DPIImage = 100;

            try
            {
                if (imageSize.Width > 0 && imageSize.Height > 0)
                {
                    // Tạo hình ảnh mới với kích thước từ setting
                    using (var imageStream = new MemoryStream(File.ReadAllBytes(imagePath)))
                    using (var originalImage = Image.FromStream(imageStream))
                    using (var newImage = new Bitmap(imageSize.Width, imageSize.Height, PixelFormat.Format32bppArgb))
                    {
                        newImage.SetResolution(DPIImage, DPIImage);

                        using (var graphics = Graphics.FromImage(newImage))
                        {
                            graphics.Clear(Color.Transparent);
                            graphics.DrawImage(originalImage, 0, 0, imageSize.Width, imageSize.Height);

                            // Kiểm tra và vẽ stamp nếu có
                            if (!string.IsNullOrWhiteSpace(stampPath) && File.Exists(stampPath) && ImageValidate.IsImageFile(stampPath))
                            {
                                System.Drawing.Size stampSize = ConfigViewModel.Instance.imageSetting.StampSize;
                                System.Drawing.Point stampMargin = ConfigViewModel.Instance.imageSetting.StampMarrgin;
                                int stampPosition = ConfigViewModel.Instance.imageSetting.StampPosition;

                                using (var stampStream = new MemoryStream(File.ReadAllBytes(stampPath)))
                                using (var stampImage = Image.FromStream(stampStream))
                                using (var resizedStamp = new Bitmap(stampImage, stampSize.Width, stampSize.Height))
                                {
                                    resizedStamp.SetResolution(DPIImage, DPIImage);
                                    var stampPositionCoordinates = GetStampPositionCoordinates(
                                        stampPosition,
                                        ConfigViewModel.Instance.imageSetting.ImageSize,
                                        stampSize,
                                        stampMargin.X,
                                        stampMargin.Y
                                    );
                                    graphics.DrawImage(resizedStamp, stampPositionCoordinates.X, stampPositionCoordinates.Y);
                                }


                            }
                        }

                        // Tạo và lưu file đầu ra
                        var picsFilePath = Path.Combine(targetPath, "pics");
                        if (!System.IO.Directory.Exists(picsFilePath))
                            Directory.CreateDirectory(picsFilePath);

                        var outputFilePath = Path.Combine(picsFilePath, $"{password}.png");
                        if (!ImageValidate.TryDeleteFile(outputFilePath))
                        {
                            CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                                CMess.unableDelete.ToText(), new[] {CMess.ok.ToText()});
                            return string.Empty;
                        }
                        newImage.Save(outputFilePath, ImageFormat.Png);

                        return outputFilePath;
                    }
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorCreaImg.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return string.Empty;
            }
            return string.Empty;
        }
        public static async Task<string> CreateImageCard(string password, string imagePath, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(targetPath) ||
                string.IsNullOrWhiteSpace(imagePath) || !System.IO.File.Exists(imagePath) || !ImageValidate.IsImageFile(imagePath))
                return string.Empty;

            return await Task.Run(async () =>
            {
                try
                {
                    var info = new SKImageInfo(ConfigViewModel.Instance.imageSetting.ImageSize.Width, ConfigViewModel.Instance.imageSetting.ImageSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
                    using (var surface = SKSurface.Create(info))
                    {
                        var canvas = surface.Canvas;
                        canvas.Clear(SKColors.Transparent);

                        using var paint = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.High };

                        using var stream = File.OpenRead(imagePath);
                        using var originalBitmap = SKBitmap.Decode(stream);
                        if (originalBitmap == null) return string.Empty;

                        var destRect = new SKRect(0, 0, ConfigViewModel.Instance.imageSetting.ImageSize.Width, ConfigViewModel.Instance.imageSetting.ImageSize.Height);
                        canvas.DrawBitmap(originalBitmap, destRect, paint);

                        #region Rarity
                        if (ConfigViewModel.Instance.imageSetting.IncludeRare && GeneraImageViewModel.Instance.CurrentImageInfo.RarityLabelRect.HasValue && ulong.TryParse(password, out ulong id))
                        {
                            long rarity = RareRawDataViewModel.Instance.GetRareByCardId(id);
                            SKBitmap selectedRarity = null;
                            if (rarity > 0 && RareRawDataViewModel.Instance.RarityCache.Count > 0)
                            {
                                foreach (var item in RareRawDataViewModel.Instance.RarityCache)
                                {
                                    if ((rarity & item.Key) == item.Key)
                                    {
                                        selectedRarity = item.Value;
                                        break;
                                    }
                                }
                            }
                            if (selectedRarity != null)
                            {
                                canvas.DrawBitmap(selectedRarity, GeneraImageViewModel.Instance.CurrentImageInfo.RarityLabelRect.Value, paint);
                            }
                        }
                        #endregion

                        #region Save
                        var outputFilePath = System.IO.Path.Combine(targetPath, $"{password}.png");
                        if (!ImageValidate.TryDeleteFile(outputFilePath)) return string.Empty;

                        using (var image = surface.Snapshot())
                            await ImageValidate.SaveImage(image, outputFilePath);
                        #endregion

                        return outputFilePath;
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            });
        }

        // Tính toán vị trí đặt stamp
        private static System.Drawing.Point GetStampPositionCoordinates(int position,
            System.Drawing.Size imageSize, System.Drawing.Size stampSize, int MarginVer, int MarginHori)
        {
            int x, y;
            switch (position)
            {
                case 1:
                    x = MarginVer;
                    y = MarginHori;
                    break;

                case 2:
                    x = imageSize.Width - stampSize.Width - MarginVer;
                    y = MarginHori;
                    break;

                case 3:
                    x = MarginVer;
                    y = imageSize.Height - stampSize.Height - MarginHori;
                    break;

                case 4:
                    x = imageSize.Width - stampSize.Width - MarginVer;
                    y = imageSize.Height - stampSize.Height - MarginHori;
                    break;

                case 5:
                    x = (imageSize.Width - stampSize.Width) / 2;
                    y = (imageSize.Height - stampSize.Height) / 2;
                    break;

                default:
                    x = imageSize.Width - stampSize.Width - MarginVer;
                    y = imageSize.Height - stampSize.Height - MarginHori;
                    break;
            }
            return new System.Drawing.Point(x, y);
        }

    }
}
#pragma warning restore CS0618