#pragma warning disable CS0612
#pragma warning disable CS0618
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using SkiaSharp;
using CardEditor.Models;
using CardEditor.Manager;
using CardEditor.Services;
using CardEditor.ViewModels;
using CardEditor.ImagesConfig;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Collections.Concurrent;

namespace CardEditor.ImageGene
{
    public static class ImageGenerator
    {
        private static readonly TextLayoutEngine _textLayoutEngine = new TextLayoutEngine();

        public static string outputFolderPath = string.Empty;
        public static string originalFolderPath = string.Empty;
        public const char OpenBracketTCG = '\uE001';
        public const char CloseBracketTCG = '\uE003';

        #region Type Face

        #region OCG
        private static SKTypeface _nameTypefaceOCG;
        private static SKTypeface _penScaleTypefaceOCG;
        private static SKTypeface _penEffectTypefaceOCG;
        private static SKTypeface _monsTypefaceOCG;
        private static SKTypeface _descTypefaceOCG;
        private static SKTypeface _loreTypefaceOCG;
        private static SKTypeface _ATKTypefaceOCG;
        private static SKTypeface _LINKTypefaceOCG;
        private static SKTypeface _PassTypefaceOCG;
        #endregion

        #region TCG
        private static SKTypeface _nameTypefaceTCG;
        private static SKTypeface _penScaleTypefaceTCG;
        private static SKTypeface _penEffectTypefaceTCG;
        private static SKTypeface _monsTypefaceTCG;
        private static SKTypeface _descTypefaceTCG;
        private static SKTypeface _loreTypefaceTCG;
        private static SKTypeface _ATKTypefaceTCG;
        private static SKTypeface _LINKTypefaceTCG;
        private static SKTypeface _PassTypefaceTCG;
        #endregion

        #endregion

        #region Paint
        private static SKPaint _namePaintWhiteOCG;
        private static SKPaint _namePaintWhiteTCG;
        private static SKPaint _namePaintBlackOCG;
        private static SKPaint _namePaintBlackTCG;

        private static SKPaint _namePaintTogoBackOCG;
        private static SKPaint _namePaintTogoBackTCG;
        private static SKPaint _namePaintTogoFrontOCG;
        private static SKPaint _namePaintTogoFrontTCG;

        private static SKPaint _namePaintGoldOCG;
        private static SKPaint _namePaintGoldTCG;
        private static SKPaint _namePaintGoldOutOCG;
        private static SKPaint _namePaintGoldOutTCG;
        private static SKPaint _namePaintPlatiumOCG;
        private static SKPaint _namePaintPlatiumTCG;
        private static SKPaint _namePaintPlatiumOutOCG;
        private static SKPaint _namePaintPlatiumOutTCG;

        private static SKPaint _penScaleLeftPaintOCG;
        private static SKPaint _penScaleLeftPaintTCG;
        private static SKPaint _penScaleRightPaintOCG;
        private static SKPaint _penScaleRightPaintTCG;
        private static SKPaint _penEffectPaintOCG;
        private static SKPaint _penEffectPaintTCG;

        private static SKPaint _monsTypePaintOCG;
        private static SKPaint _monsTypePaintTCG;
        private static SKPaint _descPaintOCG;
        private static SKPaint _descPaintTCG;
        private static SKPaint _lorePaintOCG;
        private static SKPaint _lorePaintTCG;

        private static SKPaint _ATKPaintOCG;
        private static SKPaint _ATKPaintTCG;
        private static SKPaint _LINKPaintOCG;
        private static SKPaint _LINKPaintTCG;

        private static SKPaint _PassPaintBlackOCG;
        private static SKPaint _PassPaintBlackTCG;
        private static SKPaint _PassPaintWhiteOCG;
        private static SKPaint _PassPaintWhiteTCG;
        #endregion

        private static readonly object _initLock = new();
        private static bool _initialized = false;
        private static readonly ConcurrentDictionary<string, string> _artworkCache = new ConcurrentDictionary<string, string>();

        public static void EnsureInitialized()
        {
            if (_initialized) return;
            lock (_initLock)
            {
                if (_initialized) return;

                #region Type Face
                _nameTypefaceOCG = GeneraImageViewModel.Instance.GetFont("CardName", 0, SKFontStyleWeight.Normal);
                _penScaleTypefaceOCG = GeneraImageViewModel.Instance.GetFont("PendulumScale", 0, SKFontStyleWeight.Normal);
                _penEffectTypefaceOCG = GeneraImageViewModel.Instance.GetFont("CardEffect", 0, SKFontStyleWeight.Normal);
                _monsTypefaceOCG = GeneraImageViewModel.Instance.GetFont("CardTypeAbility", 0, SKFontStyleWeight.Normal);
                _descTypefaceOCG = GeneraImageViewModel.Instance.GetFont("CardEffect", 0, SKFontStyleWeight.Normal);
                _loreTypefaceOCG = GeneraImageViewModel.Instance.GetFont("CardLore", 0, SKFontStyleWeight.Normal);
                _ATKTypefaceOCG = GeneraImageViewModel.Instance.GetFont("MonsterPower", 0, SKFontStyleWeight.Medium);
                _LINKTypefaceOCG = GeneraImageViewModel.Instance.GetFont("LinkNumber", 0, SKFontStyleWeight.Normal);
                _PassTypefaceOCG = GeneraImageViewModel.Instance.GetFont("Passcode", 0, SKFontStyleWeight.Light);

                _nameTypefaceTCG = GeneraImageViewModel.Instance.GetFont("CardName", 1, SKFontStyleWeight.Normal);
                _penScaleTypefaceTCG = GeneraImageViewModel.Instance.GetFont("PendulumScale", 1, SKFontStyleWeight.Normal);
                _penEffectTypefaceTCG = GeneraImageViewModel.Instance.GetFont("CardEffect", 1, SKFontStyleWeight.Normal);
                _monsTypefaceTCG = GeneraImageViewModel.Instance.GetFont("CardTypeAbility", 1, SKFontStyleWeight.Normal);
                _descTypefaceTCG = GeneraImageViewModel.Instance.GetFont("CardEffect", 1, SKFontStyleWeight.Normal);
                _loreTypefaceTCG = GeneraImageViewModel.Instance.GetFont("CardLore", 1, SKFontStyleWeight.Normal);
                _ATKTypefaceTCG = GeneraImageViewModel.Instance.GetFont("MonsterPower", 1, SKFontStyleWeight.Medium);
                _LINKTypefaceTCG = GeneraImageViewModel.Instance.GetFont("LinkNumber", 1, SKFontStyleWeight.Normal);
                _PassTypefaceTCG = GeneraImageViewModel.Instance.GetFont("Passcode", 1, SKFontStyleWeight.Light);
                #endregion

                #region name

                #region Common
                _namePaintWhiteOCG = new SKPaint
                {
                    Typeface = _nameTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    Color = new SKColor(0xf2, 0xf2, 0xf2),
                    IsAntialias = true
                };
                _namePaintWhiteTCG = new SKPaint
                {
                    Typeface = _nameTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    Color = new SKColor(0xf2, 0xf2, 0xf2),
                    IsAntialias = true
                };
                _namePaintBlackOCG = new SKPaint
                {
                    Typeface = _nameTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    Color = new SKColor(0x0f, 0x0f, 0x0f),
                    IsAntialias = true
                };
                _namePaintBlackTCG = new SKPaint
                {
                    Typeface = _nameTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    Color = new SKColor(0x0f, 0x0f, 0x0f),
                    IsAntialias = true
                };

                _namePaintTogoBackOCG = new SKPaint
                {
                    Typeface = _nameTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    Color = new SKColor(0xFF, 0xC1, 0x07),
                    IsAntialias = true
                };
                _namePaintTogoBackTCG = new SKPaint
                {
                    Typeface = _nameTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    Color = new SKColor(0xFF, 0xC1, 0x07),
                    IsAntialias = true
                };

                _namePaintTogoFrontOCG = new SKPaint
                {
                    Typeface = _nameTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName * 0.02f,
                    Color = new SKColor(0xFF, 0xF9, 0xC4)
                };
                _namePaintTogoFrontTCG = new SKPaint
                {
                    Typeface = _nameTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName * 0.02f,
                    Color = new SKColor(0xFF, 0xF9, 0xC4)
                };
                #endregion

                #region Gold
                _namePaintGoldOCG = new SKPaint
                {
                    Typeface = _nameTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    Color = new SKColor(255, 248, 185),
                    IsAntialias = true
                };
                _namePaintGoldOutOCG = new SKPaint
                {
                    Typeface = _nameTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName * 0.04f,
                    Color = new SKColor(255, 255, 200) // vàng nhạt
                };
                _namePaintGoldTCG = new SKPaint
                {
                    Typeface = _nameTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    Color = new SKColor(255, 248, 185),
                    IsAntialias = true
                };
                _namePaintGoldOutTCG = new SKPaint
                {
                    Typeface = _nameTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName * 0.04f,
                    Color = new SKColor(255, 255, 200) // vàng nhạt
                };
                #endregion

                #region platium
                _namePaintPlatiumOCG = new SKPaint
                {
                    Typeface = _nameTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    Color = new SKColor(192, 192, 192),
                    IsAntialias = true,
                };
                _namePaintPlatiumOutOCG = new SKPaint
                {
                    Typeface = _nameTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName * 0.04f,
                    Color = new SKColor(120, 120, 120)
                };
                _namePaintPlatiumTCG = new SKPaint
                {
                    Typeface = _nameTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    Color = new SKColor(192, 192, 192),
                    IsAntialias = true,
                };
                _namePaintPlatiumOutTCG = new SKPaint
                {
                    Typeface = _nameTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName,
                    TextAlign = SKTextAlign.Left,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeName * 0.04f,
                    Color = new SKColor(120, 120, 120)
                };
                #endregion

                #endregion

                #region Pendulum
                _penScaleLeftPaintOCG = new SKPaint
                {
                    Typeface = _penScaleTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizePenScale,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };
                _penScaleLeftPaintTCG = new SKPaint
                {
                    Typeface = _penScaleTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizePenScale,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };
                _penScaleRightPaintOCG = new SKPaint
                {
                    Typeface = _penScaleTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizePenScale,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };
                _penScaleRightPaintTCG = new SKPaint
                {
                    Typeface = _penScaleTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizePenScale,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };
                _penEffectPaintOCG = new SKPaint
                {
                    Typeface = _penEffectTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeDesc,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                _penEffectPaintTCG = new SKPaint
                {
                    Typeface = _penEffectTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeDesc,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                #endregion

                #region Desc
                _monsTypePaintOCG = new SKPaint
                {
                    Typeface = _monsTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeMonsterType,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                _monsTypePaintTCG = new SKPaint
                {
                    Typeface = _monsTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeMonsterType,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                _descPaintOCG = new SKPaint
                {
                    Typeface = _descTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeDesc,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                _descPaintTCG = new SKPaint
                {
                    Typeface = _descTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeDesc,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                _lorePaintOCG = new SKPaint
                {
                    Typeface = _loreTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeDesc,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                _lorePaintTCG = new SKPaint
                {
                    Typeface = _loreTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeDesc,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };

                var metricsOCG = _descPaintOCG.FontMetrics;
                var metricsTCG = _descPaintTCG.FontMetrics;
                #endregion

                #region Power
                _ATKPaintOCG = new SKPaint
                {
                    Typeface = _ATKTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeATK,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                _ATKPaintTCG = new SKPaint
                {
                    Typeface = _ATKTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeATK,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                _LINKPaintOCG = new SKPaint
                {
                    Typeface = _LINKTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeLink,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                _LINKPaintTCG = new SKPaint
                {
                    Typeface = _LINKTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeLink,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                #endregion

                #region Passcode
                _PassPaintBlackOCG = new SKPaint
                {
                    Typeface = _PassTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizePass,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                _PassPaintBlackTCG = new SKPaint
                {
                    Typeface = _PassTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizePass,
                    Color = SKColors.Black,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                _PassPaintWhiteOCG = new SKPaint
                {
                    Typeface = _PassTypefaceOCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizePass,
                    Color = SKColors.White,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                _PassPaintWhiteTCG = new SKPaint
                {
                    Typeface = _PassTypefaceTCG,
                    TextSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizePass,
                    Color = SKColors.White,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                #endregion

                _initialized = true;
            }
        }
        public static void Dispose()
        {
            DisposePaint(ref _namePaintWhiteOCG);
            DisposePaint(ref _namePaintWhiteTCG);
            DisposePaint(ref _namePaintBlackOCG);
            DisposePaint(ref _namePaintBlackTCG);

            DisposePaint(ref _namePaintTogoBackOCG);
            DisposePaint(ref _namePaintTogoBackTCG);
            DisposePaint(ref _namePaintTogoFrontOCG);
            DisposePaint(ref _namePaintTogoFrontTCG);

            DisposePaint(ref _namePaintGoldOCG);
            DisposePaint(ref _namePaintGoldTCG);
            DisposePaint(ref _namePaintGoldOutOCG);
            DisposePaint(ref _namePaintGoldOutTCG);
            DisposePaint(ref _namePaintPlatiumOCG);
            DisposePaint(ref _namePaintPlatiumTCG);
            DisposePaint(ref _namePaintPlatiumOutOCG);
            DisposePaint(ref _namePaintPlatiumOutTCG);

            DisposePaint(ref _penScaleLeftPaintOCG);
            DisposePaint(ref _penScaleLeftPaintTCG);
            DisposePaint(ref _penScaleRightPaintOCG);
            DisposePaint(ref _penScaleRightPaintTCG);
            DisposePaint(ref _penEffectPaintOCG);
            DisposePaint(ref _penEffectPaintTCG);

            DisposePaint(ref _monsTypePaintOCG);
            DisposePaint(ref _monsTypePaintTCG);
            DisposePaint(ref _descPaintOCG);
            DisposePaint(ref _descPaintTCG);
            DisposePaint(ref _lorePaintOCG);
            DisposePaint(ref _lorePaintTCG);

            DisposePaint(ref _ATKPaintOCG);
            DisposePaint(ref _ATKPaintTCG);
            DisposePaint(ref _LINKPaintOCG);
            DisposePaint(ref _LINKPaintTCG);

            DisposePaint(ref _PassPaintBlackOCG);
            DisposePaint(ref _PassPaintBlackTCG);
            DisposePaint(ref _PassPaintWhiteOCG);
            DisposePaint(ref _PassPaintWhiteTCG);

            DisposeTypeface(ref _nameTypefaceOCG);
            DisposeTypeface(ref _penScaleTypefaceOCG);
            DisposeTypeface(ref _penEffectTypefaceOCG);
            DisposeTypeface(ref _monsTypefaceOCG);
            DisposeTypeface(ref _descTypefaceOCG);
            DisposeTypeface(ref _loreTypefaceOCG);
            DisposeTypeface(ref _ATKTypefaceOCG);
            DisposeTypeface(ref _LINKTypefaceOCG);
            DisposeTypeface(ref _PassTypefaceOCG);

            DisposeTypeface(ref _nameTypefaceTCG);
            DisposeTypeface(ref _penScaleTypefaceTCG);
            DisposeTypeface(ref _penEffectTypefaceTCG);
            DisposeTypeface(ref _monsTypefaceTCG);
            DisposeTypeface(ref _descTypefaceTCG);
            DisposeTypeface(ref _loreTypefaceTCG);
            DisposeTypeface(ref _ATKTypefaceTCG);
            DisposeTypeface(ref _LINKTypefaceTCG);
            DisposeTypeface(ref _PassTypefaceTCG);
        }
        private static void DisposePaint(ref SKPaint paint)
        {
            paint?.Dispose();
            paint = null;
        }
        private static void DisposeTypeface(ref SKTypeface typeface)
        {
            typeface?.Dispose();
            typeface = null;
        }

        public static async Task<(bool, string)> GenerateImage10(CardEditor.Models.Card cardItem)
        {
            if (cardItem == null || string.IsNullOrWhiteSpace(outputFolderPath)) return (false, CMess.cardNotExit.ToText());
            EnsureInitialized();

            SKFileStream artWorkStream = null;
            SKBitmap artWorkBitmap = null;
            SKPaint SelectedNamePaint = null;
            SKPaint SelectedNamePaintOut = null;

            try
            {
                //var info = new SKImageInfo(Image10Info.CardWidth, Image10Info.CardHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
                var info = new SKImageInfo(GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth, GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
                using (var surface = SKSurface.Create(info))
                using (var paint = new SKPaint { IsAntialias = true })
                using (var detailPaint = new SKPaint { IsAntialias = false })
                {
                    var canvas = surface.Canvas;
                    canvas.Clear(SKColors.Transparent);
                    var CardInfo = new CardItemInfo(cardItem.type);

                    #region Background

                    #region Background Card
                    SKBitmap selectedBackground = null;
                    if (GeneraImageViewModel.Instance.BackgroundCache.Count > 0)
                    {
                        foreach (var item in GeneraImageViewModel.Instance.BackgroundCache)
                        {
                            if ((cardItem.type & item.Key) == item.Key)
                            {
                                selectedBackground = item.Value;
                                break;
                            }
                        }
                    }
                    if (selectedBackground != null)
                    {
                        canvas.DrawBitmap(selectedBackground, GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundRect, paint);
                    }
                    #endregion

                    #region Background Artwork
                    if (!string.IsNullOrEmpty(ConfigViewModel.Instance.imageSetting.BackgroundArt) &&
                    ConfigViewModel.Instance.imageSetting.BackgroundArt != "None")
                    {
                        if (GeneraImageViewModel.Instance.BackgroundArtCache?.TryGetValue(
                            ConfigViewModel.Instance.imageSetting.BackgroundArt, out var selectedBackgroundArt) == true)
                            canvas.DrawBitmap(selectedBackgroundArt, CardInfo.IsPendulum
                                ? GeneraImageViewModel.Instance.CurrentImageInfo.AWPendulumRect
                                : GeneraImageViewModel.Instance.CurrentImageInfo.AWNormalRect, paint);
                    }
                    if (ConfigViewModel.Instance.imageSetting.Secret == 1)
                    {
                        if (CardInfo.IsPendulum)
                        {
                            if (GeneraImageViewModel.Instance.SecretCache?.TryGetValue("2", out var selectedSecret) == true)
                                canvas.DrawBitmap(selectedSecret, GeneraImageViewModel.Instance.CurrentImageInfo.AWPendulumRect, paint);
                        }
                        else
                        {
                            if (GeneraImageViewModel.Instance.SecretCache?.TryGetValue("1", out var selectedSecret) == true)
                                canvas.DrawBitmap(selectedSecret, GeneraImageViewModel.Instance.CurrentImageInfo.AWNormalRect, paint);
                        }
                    }
                    #endregion

                    #endregion

                    #region Frame
                    if (CardInfo.IsPendulum)
                    {
                        // Pendulum BackGround
                        if (GeneraImageViewModel.Instance.FrameCache?.TryGetValue(0, out var selectedPen) == true)
                            canvas.DrawBitmap(selectedPen, GeneraImageViewModel.Instance.CurrentImageInfo.FrameRect, paint);

                        if (GeneraImageViewModel.Instance.FrameCache?.TryGetValue(CardInfo.IsSkill ? 9 : 5, out var selectedFrame) == true)
                            canvas.DrawBitmap(selectedFrame, GeneraImageViewModel.Instance.CurrentImageInfo.FrameRect, paint);
                    }
                    else
                    {
                        if (GeneraImageViewModel.Instance.FrameCache?.TryGetValue(CardInfo.IsSkill ? 7 : 3, out var selectedFrame) == true)
                            canvas.DrawBitmap(selectedFrame, GeneraImageViewModel.Instance.CurrentImageInfo.FrameRect, paint);
                    }
                    #endregion

                    #region InitializeArtWork
                    bool artWorkTransparent = false;
                    string artWorkPath = FindArtworkPath(cardItem.id.ToString());
                    
                    if (!string.IsNullOrEmpty(artWorkPath) && File.Exists(artWorkPath))
                    {
                        using (var stream = File.OpenRead(artWorkPath))
                        {
                            artWorkBitmap = SKBitmap.Decode(stream);
                        }
                        artWorkTransparent = HasTransparentArtWork(artWorkBitmap);

                        //artWorkBitmap = await Task.Run(() =>
                        //{
                        //    using (var stream = new SKFileStream(artWorkPath))
                        //    {
                        //        return SKBitmap.Decode(stream);
                        //    }
                        //});
                        //artWorkTransparent = HasTransparentArtWork(artWorkBitmap);
                    }
                    #endregion

                    #region Name Box
                    // Full Art hoặc ArtWork trong suốt
                    if (!ConfigViewModel.Instance.imageSetting.FullArt || artWorkTransparent)
                    {
                        if (GeneraImageViewModel.Instance.FrameCache?.TryGetValue(CardInfo.IsSkill ? 2 : 1, out var selectedNameBox) == true)
                        {
                            canvas.DrawBitmap(selectedNameBox, GeneraImageViewModel.Instance.CurrentImageInfo.NameBoxRect, paint);
                        }
                    }
                    #endregion

                    #region Links Arrow Off
                    // Chỉ vẽ trước các Link Arrow Off khi Artwork trong suốt
                    // Bỏ qua các Link Arrow Bottom Off khi Pendulum
                    if (CardInfo.IsLink && artWorkTransparent)
                    {
                        if (CardInfo.IsPendulum)
                        {
                            // Artwork trong suốt
                            // Vẽ các Link Arrow Off, ngoại trừ Bottom Off
                            foreach (var arrow in GeneraImageViewModel.Instance.CurrentLinkArrowInfo.LinkArrowPenInfo)
                            {
                                if ((arrow.Mask & GeneraImageViewModel.Instance.CurrentLinkArrowInfo.SkipMark) != 0) continue;
                                bool isOn = (cardItem.def & (long)arrow.Mask) == (long)arrow.Mask;
                                string key = $"{arrow.Code}_off";
                                if (!isOn && GeneraImageViewModel.Instance.LinkArrowPenCache.TryGetValue(key, out var img))
                                {
                                    canvas.DrawBitmap(img, arrow.Position, detailPaint);
                                }
                            }
                        }
                        else
                        {
                            // Artqwork trong suốt
                            // Vẽ toàn bộ các Link Arrow Off
                            foreach (var arrow in GeneraImageViewModel.Instance.CurrentLinkArrowInfo.LinkArrowNormalInfo)
                            {
                                //if ((arrow.Mask & LinkArrowInfo.SkipMark) != 0) continue;
                                bool isOn = (cardItem.def & (long)arrow.Mask) == (long)arrow.Mask;
                                string key = $"{arrow.Code}_off";
                                if (!isOn && GeneraImageViewModel.Instance.LinkArrowNormalCache.TryGetValue(key, out var img))
                                {
                                    canvas.DrawBitmap(img, arrow.Position, detailPaint);
                                }
                            }
                        }
                    }
                    #endregion

                    #region ArtWork
                    if (artWorkBitmap != null)
                    {
                        if (ConfigViewModel.Instance.imageSetting.FullArt)
                        {
                            int width = artWorkBitmap.Width;
                            int height = artWorkBitmap.Height;
                            int diff = Math.Abs(width - height);

                            if (artWorkTransparent)
                            {
                                if (diff < 20 && GeneraImageViewModel.Instance.CurrentImageInfo.AWFullSquareTransparenRect.HasValue) // Ảnh vuông
                                    canvas.DrawBitmap(artWorkBitmap, GeneraImageViewModel.Instance.CurrentImageInfo.AWFullSquareTransparenRect.Value, detailPaint);

                                else if (width > height) // Ảnh ngang
                                {
                                    // Resize theo chiều cao = CardHeight, giữ tỷ lệ
                                    float scaleFactor = (float)GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight / height;
                                    int resizedWidth = (int)(width * scaleFactor);

                                    using var resizedBitmap = new SKBitmap(resizedWidth, GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight);
                                    using (var tempCanvas = new SKCanvas(resizedBitmap))
                                    {
                                        var srcRect = new SKRect(0, 0, width, height);
                                        var dstRect = new SKRect(0, 0, resizedWidth, GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight);
                                        tempCanvas.DrawBitmap(artWorkBitmap, srcRect, dstRect, detailPaint);
                                    }

                                    // Cắt phần giữa
                                    int cropX = (resizedWidth - GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth) / 2;
                                    var srcCrop = new SKRect(cropX, 0, cropX + GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth, GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight);
                                    canvas.DrawBitmap(resizedBitmap, srcCrop, GeneraImageViewModel.Instance.CurrentImageInfo.FrameRect, detailPaint);
                                }
                                else // Ảnh dọc (height > width)
                                {
                                    // Resize theo chiều rộng = CardWidth, giữ tỷ lệ
                                    float scaleFactor = (float)GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth / width;
                                    int resizedHeight = (int)(height * scaleFactor);

                                    using var resizedBitmap = new SKBitmap(GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth, resizedHeight);
                                    using (var tempCanvas = new SKCanvas(resizedBitmap))
                                    {
                                        var srcRect = new SKRect(0, 0, width, height);
                                        var dstRect = new SKRect(0, 0, GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth, resizedHeight);
                                        tempCanvas.DrawBitmap(artWorkBitmap, srcRect, dstRect, detailPaint);
                                    }

                                    // Cắt phần giữa
                                    int cropY = (resizedHeight - GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight) / 2;
                                    var srcCrop = new SKRect(0, cropY, GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth, cropY + GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight);
                                    canvas.DrawBitmap(resizedBitmap, srcCrop, GeneraImageViewModel.Instance.CurrentImageInfo.FrameRect, detailPaint);
                                }
                            }
                            else
                            {
                                if (diff < 20 && GeneraImageViewModel.Instance.CurrentImageInfo.AWFullSquareOpaqueRect.HasValue) // Ảnh vuông
                                    canvas.DrawBitmap(artWorkBitmap, GeneraImageViewModel.Instance.CurrentImageInfo.AWFullSquareOpaqueRect.Value, detailPaint);

                                else if (width > height) // Ảnh ngang
                                {
                                    // Resize theo chiều cao = BackgroundHeight, giữ tỷ lệ
                                    float scaleFactor = (float)GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundHeight / height;
                                    int resizedWidth = (int)(width * scaleFactor);

                                    using var resizedBitmap = new SKBitmap(resizedWidth, GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundHeight);
                                    using (var tempCanvas = new SKCanvas(resizedBitmap))
                                    {
                                        var srcRect = new SKRect(0, 0, width, height);
                                        var dstRect = new SKRect(0, 0, resizedWidth, GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundHeight);
                                        tempCanvas.DrawBitmap(artWorkBitmap, srcRect, dstRect, detailPaint);
                                    }

                                    // Cắt phần giữa
                                    int cropX = (resizedWidth - GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundWidth) / 2;
                                    var srcCrop = new SKRect(cropX, 0, cropX + GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundWidth, GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundHeight);
                                    canvas.DrawBitmap(resizedBitmap, srcCrop, GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundRect, detailPaint);
                                }
                                else // Ảnh dọc (height > width)
                                {
                                    // Resize theo chiều rộng = BackgroundWidth, giữ tỷ lệ
                                    float scaleFactor = (float)GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundWidth / width;
                                    int resizedHeight = (int)(height * scaleFactor);

                                    using var resizedBitmap = new SKBitmap(GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundWidth, resizedHeight);
                                    using (var tempCanvas = new SKCanvas(resizedBitmap))
                                    {
                                        var srcRect = new SKRect(0, 0, width, height);
                                        var dstRect = new SKRect(0, 0, GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundWidth, resizedHeight);
                                        tempCanvas.DrawBitmap(artWorkBitmap, srcRect, dstRect, detailPaint);
                                    }

                                    // Cắt phần giữa
                                    int cropY = (resizedHeight - GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundHeight) / 2;
                                    var srcCrop = new SKRect(0, cropY, GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundWidth, cropY + GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundHeight);
                                    canvas.DrawBitmap(resizedBitmap, srcCrop, GeneraImageViewModel.Instance.CurrentImageInfo.BackgroundRect, detailPaint);
                                }
                            }
                        }
                        else
                        {
                            if (CardInfo.IsPendulum)
                            {
                                float scaleFactor = (float)GeneraImageViewModel.Instance.CurrentImageInfo.AWPenWidth / artWorkBitmap.Width;
                                int resizedHeight = (int)(artWorkBitmap.Height * scaleFactor);

                                using var resizedBitmap = new SKBitmap(GeneraImageViewModel.Instance.CurrentImageInfo.AWPenWidth, resizedHeight);
                                using (var tempCanvas = new SKCanvas(resizedBitmap))
                                {
                                    var srcRect = new SKRect(0, 0, artWorkBitmap.Width, artWorkBitmap.Height);
                                    var dstRect = new SKRect(0, 0, resizedBitmap.Width, resizedBitmap.Height);
                                    tempCanvas.DrawBitmap(artWorkBitmap, srcRect, dstRect);
                                }
                                int drawHeight = Math.Min(resizedBitmap.Height, GeneraImageViewModel.Instance.CurrentImageInfo.AWPenMinHeight);
                                var srcCrop = new SKRect(0, 0, resizedBitmap.Width, drawHeight);

                                var destRect = new SKRect(
                                    GeneraImageViewModel.Instance.CurrentImageInfo.PenPosition.X,
                                    GeneraImageViewModel.Instance.CurrentImageInfo.PenPosition.Y,
                                    GeneraImageViewModel.Instance.CurrentImageInfo.PenPosition.X + GeneraImageViewModel.Instance.CurrentImageInfo.AWPenWidth,
                                    GeneraImageViewModel.Instance.CurrentImageInfo.PenPosition.Y + drawHeight);

                                canvas.DrawBitmap(resizedBitmap, srcCrop, destRect);
                            }
                            else canvas.DrawBitmap(artWorkBitmap, GeneraImageViewModel.Instance.CurrentImageInfo.AWNormalRect, detailPaint);
                        }
                    }
                    #endregion

                    #region Effect Box
                    if (CardInfo.IsPendulum)
                    {
                        if (GeneraImageViewModel.Instance.FrameCache?.TryGetValue(CardInfo.IsSkill ? 10 : 6, out var selectedBox) == true)
                            canvas.DrawBitmap(selectedBox, GeneraImageViewModel.Instance.CurrentImageInfo.EffectBoxPen, paint);
                    }
                    else
                    {
                        if (GeneraImageViewModel.Instance.FrameCache?.TryGetValue(CardInfo.IsSkill ? 8 : 4, out var selectedBox) == true)
                            canvas.DrawBitmap(selectedBox, GeneraImageViewModel.Instance.CurrentImageInfo.EffectBoxNor, paint);
                    }
                    #endregion

                    #region Links Arrow Off
                    // Chỉ vẽ trước các Link Arrow Off khi Artwork mờ đục
                    // Không bỏ qua các Link Arrow Bottom Off khi Pendulum
                    if (CardInfo.IsLink && !artWorkTransparent)
                    {
                        if (CardInfo.IsPendulum)
                        {
                            // Artwork mờ đục
                            // Vẽ toàn bộ các Link Arrow Off
                            foreach (var arrow in GeneraImageViewModel.Instance.CurrentLinkArrowInfo.LinkArrowPenInfo)
                            {
                                //if ((arrow.Mask & LinkArrowInfo.SkipMark) != 0) continue;
                                bool isOn = (cardItem.def & (long)arrow.Mask) == (long)arrow.Mask;
                                string key = $"{arrow.Code}_off";
                                if (!isOn && GeneraImageViewModel.Instance.LinkArrowPenCache.TryGetValue(key, out var img))
                                {
                                    canvas.DrawBitmap(img, arrow.Position, detailPaint);
                                }
                            }
                        }
                        else
                        {
                            // Artwork mờ đục
                            // Vẽ toàn bộ các Link Arrow Off
                            foreach (var arrow in GeneraImageViewModel.Instance.CurrentLinkArrowInfo.LinkArrowNormalInfo)
                            {
                                //if ((arrow.Mask & LinkArrowInfo.SkipMark) != 0) continue;
                                bool isOn = (cardItem.def & (long)arrow.Mask) == (long)arrow.Mask;
                                string key = $"{arrow.Code}_off";
                                if (!isOn && GeneraImageViewModel.Instance.LinkArrowNormalCache.TryGetValue(key, out var img))
                                {
                                    canvas.DrawBitmap(img, arrow.Position, detailPaint);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Link Arrows On
                    if (CardInfo.IsLink)
                    {
                        // Vẽ các Link Arrows Off khi: Pendulum và Artwork trong suốt
                        // Vẽ toàn bộ Link Arrows On
                        if (CardInfo.IsPendulum)
                        {
                            if (artWorkTransparent)
                            {
                                foreach (var arrow in GeneraImageViewModel.Instance.CurrentLinkArrowInfo.LinkArrowPenInfo)
                                {
                                    if ((arrow.Mask & GeneraImageViewModel.Instance.CurrentLinkArrowInfo.SkipMark) != 0)
                                    {
                                        bool isOn = (cardItem.def & (long)arrow.Mask) == (long)arrow.Mask;
                                        string key = $"{arrow.Code}_off";
                                        if (!isOn && GeneraImageViewModel.Instance.LinkArrowPenCache.TryGetValue(key, out var img))
                                        {
                                            canvas.DrawBitmap(img, arrow.Position, detailPaint);
                                        }
                                    }
                                }
                            }
                            foreach (var arrow in GeneraImageViewModel.Instance.CurrentLinkArrowInfo.LinkArrowPenInfo)
                            {
                                bool isOn = (cardItem.def & (long)arrow.Mask) == (long)arrow.Mask;
                                string key = $"{arrow.Code}_on";
                                if (isOn && GeneraImageViewModel.Instance.LinkArrowPenCache.TryGetValue(key, out var img))
                                {
                                    canvas.DrawBitmap(img, arrow.Position, detailPaint);
                                }
                            }
                        }
                        else
                        {
                            foreach (var arrow in GeneraImageViewModel.Instance.CurrentLinkArrowInfo.LinkArrowNormalInfo)
                            {
                                bool isOn = (cardItem.def & (long)arrow.Mask) == (long)arrow.Mask;
                                string key = $"{arrow.Code}_on";
                                if (isOn && GeneraImageViewModel.Instance.LinkArrowNormalCache.TryGetValue(key, out var img))
                                {
                                    canvas.DrawBitmap(img, arrow.Position, detailPaint);
                                }
                            }
                        }
                    }
                    #endregion

                    #region Link Arrrows
                    //if (isLink)
                    //{
                    //    if (isPendulum)
                    //    {
                    //        foreach (var arrow in LinkArrowInfo.LinkArrowPenInfo)
                    //        {
                    //            bool isOn = (cardItem.def & (long)arrow.Mask) == (long)arrow.Mask;
                    //            string key = $"{arrow.Code}_{(isOn ? "on" : "off")}";
                    //            if (GeneraImageViewModel.Instance.LinkArrowPenCache.TryGetValue(key, out var img))
                    //            {
                    //                canvas.DrawBitmap(img, arrow.Position, detailPaint);
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        foreach (var arrow in LinkArrowInfo.LinkArrowNormalInfo)
                    //        {
                    //            bool isOn = (cardItem.def & (long)arrow.Mask) == (long)arrow.Mask;
                    //            string key = $"{arrow.Code}_{(isOn ? "on" : "off")}";
                    //            if (GeneraImageViewModel.Instance.LinkArrowNormalCache.TryGetValue(key, out var img))
                    //            {
                    //                canvas.DrawBitmap(img, arrow.Position, detailPaint);
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion

                    #region Attribute
                    if (CardInfo.IsMonster)
                    {
                        SKBitmap selectedAttribute = null;
                        if (GeneraImageViewModel.Instance.AttributeCache.Count > 0)
                        {
                            foreach (var item in GeneraImageViewModel.Instance.AttributeCache)
                            {
                                if ((cardItem.attribute & item.Key) == item.Key)
                                {
                                    selectedAttribute = item.Value;
                                    break;
                                }
                            }
                        }
                        if (selectedAttribute != null)
                        {
                            canvas.DrawBitmap(selectedAttribute, GeneraImageViewModel.Instance.CurrentImageInfo.AttriRect, detailPaint);
                        }
                    }
                    else
                    {
                        SKBitmap selectedSpellTrap = null;
                        if (GeneraImageViewModel.Instance.SpellTrapCache.Count > 0)
                        {
                            foreach (var item in GeneraImageViewModel.Instance.SpellTrapCache)
                            {
                                if ((cardItem.type & item.Key) == item.Key)
                                {
                                    selectedSpellTrap = item.Value;
                                    break;
                                }
                            }
                        }
                        if (selectedSpellTrap != null)
                        {
                            canvas.DrawBitmap(selectedSpellTrap, GeneraImageViewModel.Instance.CurrentImageInfo.SpellTrap, detailPaint);
                        }
                    }
                    #endregion

                    #region Level
                    if (CardInfo.IsMonster)
                    {
                        ulong stars = CardInfo.IsLink ? ((cardItem.level >> 8) & 0xff) : (cardItem.level & 0xff);
                        if (stars > 13) stars = 13;
                        if (stars > 0)
                        {
                            if (CardInfo.IsXyz && !CardInfo.IsNonXyz) // Rank
                            {
                                if (GeneraImageViewModel.Instance.RankCache?.TryGetValue(stars, out var selectedRank) == true)
                                    canvas.DrawBitmap(selectedRank, CardInfo.IsLink
                                        ? GeneraImageViewModel.Instance.CurrentImageInfo.LevelLink
                                        : GeneraImageViewModel.Instance.CurrentImageInfo.LevelNormal, detailPaint);
                            }
                            else if (CardInfo.IsXyz && CardInfo.IsNonXyz) // Level Rank
                            {
                                if (GeneraImageViewModel.Instance.LevelRankCache?.TryGetValue(stars, out var selectedLevelRank) == true)
                                    canvas.DrawBitmap(selectedLevelRank, CardInfo.IsLink
                                        ? GeneraImageViewModel.Instance.CurrentImageInfo.LevelLink
                                        : GeneraImageViewModel.Instance.CurrentImageInfo.LevelNormal, detailPaint);
                            }
                            else // Level
                            {
                                if (GeneraImageViewModel.Instance.LevelCache?.TryGetValue(stars, out var selectedLevel) == true)
                                    canvas.DrawBitmap(selectedLevel, CardInfo.IsLink
                                        ? GeneraImageViewModel.Instance.CurrentImageInfo.LevelLink
                                        : GeneraImageViewModel.Instance.CurrentImageInfo.LevelNormal, detailPaint);
                            }
                        }
                    }
                    #endregion

                    #region Name Box
                    // Full Art hoặc ArtWork trong suốt
                    if (ConfigViewModel.Instance.imageSetting.FullArt && !artWorkTransparent)
                    {
                        if (GeneraImageViewModel.Instance.FrameCache?.TryGetValue(CardInfo.IsSkill ? 2 : 1, out var selectedNameBox) == true)
                        {
                            canvas.DrawBitmap(selectedNameBox, GeneraImageViewModel.Instance.CurrentImageInfo.NameBoxRect, paint);
                        }
                    }
                    #endregion

                    #region Secret
                    if (ConfigViewModel.Instance.imageSetting.Secret == 2)
                    {
                        if (GeneraImageViewModel.Instance.SecretCache?.TryGetValue(CardInfo.IsPendulum ? "4" : "3", out var selectedSecret) == true)
                            canvas.DrawBitmap(selectedSecret, GeneraImageViewModel.Instance.CurrentImageInfo.FrameRect, paint);
                    }
                    #endregion

                    #region Name
                    if (!artWorkTransparent)  // Artwork trong suốt, vẽ NameBox bên trên
                    {
                        if (GeneraImageViewModel.Instance.FrameCache?.TryGetValue(CardInfo.IsSkill ? 2 : 1, out var selectedNameBox) == true)
                        {
                            canvas.DrawBitmap(selectedNameBox, GeneraImageViewModel.Instance.CurrentImageInfo.NameBoxRect, paint);
                        }
                    }

                    if (ConfigViewModel.Instance.imageSetting.FormatName == 0) // OCG
                    {
                        switch (ConfigViewModel.Instance.imageSetting.Rare)
                        {
                            case 1:
                                SelectedNamePaint = _namePaintGoldOCG;
                                SelectedNamePaintOut = _namePaintGoldOutOCG;
                                TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectOCG, SelectedNamePaintOut);
                                TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectOCG, SelectedNamePaint);
                                break;
                            case 2:
                                SelectedNamePaint = _namePaintPlatiumOCG;
                                SelectedNamePaintOut = _namePaintPlatiumOutOCG;
                                TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectOCG, SelectedNamePaintOut);
                                TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectOCG, SelectedNamePaint);
                                break;
                            default:
                                if (CardInfo.IsMonster)
                                {
                                    // Normal, Effect, Ritual, Fusion, Synchro, Token: đen
                                    // Xyz, Link: trắng
                                    if ((CardInfo.IsXyz && CardInfo.IsNonXyz) || (CardInfo.IsLink && CardInfo.IsNonXyz))
                                    {
                                        SelectedNamePaint = _namePaintTogoFrontOCG;
                                        SelectedNamePaintOut = _namePaintTogoBackOCG;

                                        TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectOCG, SelectedNamePaintOut);
                                        TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectOCG, SelectedNamePaint);
                                    }
                                    else
                                    {
                                        if (CardInfo.IsNormal || CardInfo.IsEffect || CardInfo.IsNonXyz || CardInfo.IsToken)
                                            SelectedNamePaint = _namePaintBlackOCG;
                                        else
                                            SelectedNamePaint = _namePaintWhiteOCG;
                                        TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectOCG, SelectedNamePaint);
                                    }
                                }
                                else
                                {
                                    SelectedNamePaint = _namePaintWhiteOCG;
                                    TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectOCG, SelectedNamePaint);
                                }
                                break;
                        }
                    }
                    else // TCG
                    {
                        switch (ConfigViewModel.Instance.imageSetting.Rare) 
                        {
                            case 1:
                                SelectedNamePaint = _namePaintGoldTCG;
                                SelectedNamePaintOut = _namePaintGoldOutTCG;
                                TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectTCG, SelectedNamePaintOut);
                                TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectTCG, SelectedNamePaint);
                                break;
                            case 2:
                                SelectedNamePaint = _namePaintPlatiumTCG;
                                SelectedNamePaintOut = _namePaintPlatiumOutTCG;
                                TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectTCG, SelectedNamePaintOut);
                                TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectTCG, SelectedNamePaint);
                                break;
                            default:
                                if (CardInfo.IsMonster)
                                {
                                    // Normal, Effect, Ritual, Fusion, Synchro, Token: đen
                                    // Xyz, Link: trắng
                                    if ((CardInfo.IsXyz && CardInfo.IsNonXyz) || (CardInfo.IsLink && CardInfo.IsNonXyz))
                                    {
                                        SelectedNamePaint = _namePaintTogoFrontTCG;
                                        SelectedNamePaintOut = _namePaintTogoBackTCG;

                                        TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectTCG, SelectedNamePaintOut);
                                        TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectTCG, SelectedNamePaint);
                                    }
                                    else
                                    {
                                        if (CardInfo.IsNormal || CardInfo.IsEffect || CardInfo.IsNonXyz || CardInfo.IsToken)
                                            SelectedNamePaint = _namePaintBlackTCG;
                                        else
                                            SelectedNamePaint = _namePaintWhiteTCG;
                                        TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectTCG, SelectedNamePaint);
                                    }
                                }
                                else
                                {
                                    SelectedNamePaint = _namePaintWhiteTCG;
                                    TextRenderer.DrawCardName(canvas, cardItem.name, GeneraImageViewModel.Instance.CurrentImageInfo.NameRectTCG, SelectedNamePaint);
                                }
                                break;
                        }
                    }
                    #endregion

                    #region Card Desc
                    var (pendulumEffect, monsterEffect) = CardTextExtractor.ExtractEffects(cardItem.desc);
                    var format = ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? TextFormat.OCG : TextFormat.TCG;

                    if (CardInfo.IsPendulum)
                    {
                        TextRenderer.DrawSingleLineCenterText(canvas, ((cardItem.level >> 24) & 0xff).ToString(), GeneraImageViewModel.Instance.CurrentImageInfo.PenScaleLeftRect,
                            ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _penScaleLeftPaintOCG : _penScaleLeftPaintTCG);
                        TextRenderer.DrawSingleLineCenterText(canvas, ((cardItem.level >> 16) & 0xff).ToString(), GeneraImageViewModel.Instance.CurrentImageInfo.PenScaleRightRect,
                            ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _penScaleRightPaintOCG : _penScaleRightPaintTCG);

                        var pendulumBasePaint = format == TextFormat.OCG ? _penEffectPaintOCG : _penEffectPaintTCG;
                        var pendulumProfile = TextLayoutEngine.BuildProfileFromBasePaint(pendulumBasePaint,
                            new[]{
                                new EffectFontLevel(50f, 56f, 2),
                                new EffectFontLevel(35f, 39f, 3),
                                new EffectFontLevel(26f, 29f, 4),
                                new EffectFontLevel(24f, 24f, 5),
                                new EffectFontLevel(19f, 20f, 6),});

                        if (!string.IsNullOrWhiteSpace(pendulumEffect))
                        {
                            var option = TextLayoutOption.Default(format, isNormal: false, useItalic: false);
                            var layout = _textLayoutEngine.FitLayout(canvas, pendulumEffect, GeneraImageViewModel.Instance.CurrentImageInfo.PenDescRect, pendulumProfile, option);
                            _textLayoutEngine.DrawLayout(canvas, layout, GeneraImageViewModel.Instance.CurrentImageInfo.PenDescRect);
                        }
                    }

                    if (CardInfo.IsMonster || CardInfo.IsSkill)
                    {
                        string monsterType = string.Empty, typeFind = string.Empty, raceFind = string.Empty, charFind = string.Empty;
                        if (ConfigViewModel.Instance.imageSetting.FormatEffect == 0) // OCG
                        {
                            if (CardDataViewModel.Instance.listtype.Count > 0)
                                typeFind = FindIInfoService.FindCardInfo(CardDataViewModel.Instance.listtype, cardItem.type, true, "／").Trim();
                            if (CardInfo.IsSkill && CardDataViewModel.Instance.listchar.Count > 0)
                                charFind = FindIInfoService.FindCardInfo(CardDataViewModel.Instance.listchar, cardItem.race, true, "／").Trim();
                            if (CardInfo.IsMonster && CardDataViewModel.Instance.listrace.Count > 0)
                                raceFind = FindIInfoService.FindCardInfo(CardDataViewModel.Instance.listrace, cardItem.race, true, "／").Trim();

                            if (!string.IsNullOrEmpty(charFind)) charFind += "／";
                            if (!string.IsNullOrEmpty(raceFind)) raceFind += "／";
                            monsterType = CardInfo.IsSkill ? $"【{charFind}{typeFind}】" : $"【{raceFind}{typeFind}】";

                            if (!string.IsNullOrEmpty(monsterType))
                                TextRenderer.DrawMonsterTypeLineText(canvas, monsterType, GeneraImageViewModel.Instance.CurrentImageInfo.MonsterTypeRect, _monsTypePaintOCG);
                        }
                        else // TCG
                        {
                            if (CardDataViewModel.Instance.listtype.Count > 0)
                                typeFind = FindIInfoService.FindCardInfo(CardDataViewModel.Instance.listtype, cardItem.type, true, "/").Trim();
                            if (CardInfo.IsSkill && CardDataViewModel.Instance.listchar.Count > 0)
                                charFind = FindIInfoService.FindCardInfo(CardDataViewModel.Instance.listchar, cardItem.race, true, "/").Trim();
                            if (CardInfo.IsMonster && CardDataViewModel.Instance.listrace.Count > 0)
                                raceFind = FindIInfoService.FindCardInfo(CardDataViewModel.Instance.listrace, cardItem.race, true, "/").Trim();

                            if (!string.IsNullOrEmpty(charFind)) charFind += "/";
                            if (!string.IsNullOrEmpty(raceFind)) raceFind += "/";
                            monsterType = CardInfo.IsSkill
                                ? $"{OpenBracketTCG}{charFind}{typeFind}{CloseBracketTCG}"
                                : $"{OpenBracketTCG}{raceFind}{typeFind}{CloseBracketTCG}";

                            if (!string.IsNullOrEmpty(monsterType))
                                TextRenderer.DrawMonsterTypeLineText(canvas, monsterType, GeneraImageViewModel.Instance.CurrentImageInfo.MonsterTypeRect, _monsTypePaintTCG);
                        }

                        var descBasePaint = CardInfo.IsNormal
                            ? (format == TextFormat.OCG ? _lorePaintOCG : _lorePaintTCG)
                            : (format == TextFormat.OCG ? _descPaintOCG : _descPaintTCG);

                        var descProfile = TextLayoutEngine.BuildProfileFromBasePaint(descBasePaint, new[] {
                            new EffectFontLevel(40f, 42f, 5),
                            new EffectFontLevel(33f, 35f, 6),
                            new EffectFontLevel(28f, 30f, 7),
                            new EffectFontLevel(24f, 25f, 8),
                            new EffectFontLevel(20f, 21f, 10)});

                        if (!string.IsNullOrEmpty(monsterEffect))
                        {
                            var useItalic = CardInfo.IsNormal; // lore mode
                            var option = TextLayoutOption.Default(format, isNormal: CardInfo.IsNormal, useItalic: useItalic);
                            var layout = _textLayoutEngine.FitLayout(canvas, monsterEffect, GeneraImageViewModel.Instance.CurrentImageInfo.MonsterDescRect, descProfile, option);
                            _textLayoutEngine.DrawLayout(canvas, layout, GeneraImageViewModel.Instance.CurrentImageInfo.MonsterDescRect);
                        }
                    }
                    else
                    {
                        var descBasePaint = CardInfo.IsNormal
                            ? (format == TextFormat.OCG ? _lorePaintOCG : _lorePaintTCG)
                            : (format == TextFormat.OCG ? _descPaintOCG : _descPaintTCG);

                        var descProfile = TextLayoutEngine.BuildProfileFromBasePaint(descBasePaint, new[] {
                            new EffectFontLevel(40f, 42f, 5),
                            new EffectFontLevel(33f, 35f, 6),
                            new EffectFontLevel(28f, 30f, 7),
                            new EffectFontLevel(24f, 25f, 8),
                            new EffectFontLevel(20f, 21f, 10)});

                        if (!string.IsNullOrEmpty(monsterEffect))
                        {
                            var useItalic = CardInfo.IsNormal; // lore mode
                            var option = TextLayoutOption.Default(format, isNormal: CardInfo.IsNormal, useItalic: useItalic);
                            var layout = _textLayoutEngine.FitLayout(canvas, monsterEffect, GeneraImageViewModel.Instance.CurrentImageInfo.SpellTrapDescRect, descProfile, option);
                            _textLayoutEngine.DrawLayout(canvas, layout, GeneraImageViewModel.Instance.CurrentImageInfo.SpellTrapDescRect);
                        }

                        //if (!string.IsNullOrEmpty(monsterEffect))
                        //    TextRenderer.DrawJustifiedEffect(canvas, ConfigViewModel.Instance.imageSetting.FormatEffect == 0
                        //        ? _descPaintOCG : _descPaintTCG, monsterEffect, Image10Info.SpellTrapDescRect);
                    }
                    #endregion

                    #region Power
                    if (CardInfo.IsMonster)
                    {
                        if (CardInfo.IsLink)
                        {
                            var (DecoLinkarrow, DecoDef, notHasATK) = GetInfoService.DecodeDef(cardItem.def);

                            if (notHasATK)
                            {
                                if (DecoDef.HasValue)
                                {
                                    if (GeneraImageViewModel.Instance.PowerCache?.TryGetValue(3, out var selectedPower) == true)
                                        canvas.DrawBitmap(selectedPower, GeneraImageViewModel.Instance.CurrentImageInfo.PowerRect, paint);
                                    string defText = DecoDef.Value >= 0 ? DecoDef.Value.ToString() : "？";
                                    TextRenderer.DrawSingleLineText(canvas, defText, GeneraImageViewModel.Instance.CurrentImageInfo.ATK2Rect, ConfigViewModel.Instance.imageSetting.FormatEffect == 0
                                        ? _ATKPaintOCG : _ATKPaintTCG);
                                    TextRenderer.DrawSingleLineText(canvas, (cardItem.level & 0xff).ToString(), GeneraImageViewModel.Instance.CurrentImageInfo.LinkRect,
                                        ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _LINKPaintOCG : _LINKPaintTCG);
                                }
                                else
                                {
                                    if (GeneraImageViewModel.Instance.PowerCache?.TryGetValue(1, out var selectedPower) == true)
                                        canvas.DrawBitmap(selectedPower, GeneraImageViewModel.Instance.CurrentImageInfo.PowerRect, paint);
                                    TextRenderer.DrawSingleLineText(canvas, (cardItem.level & 0xff).ToString(), GeneraImageViewModel.Instance.CurrentImageInfo.LinkRect,
                                        ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _LINKPaintOCG : _LINKPaintTCG);
                                }
                            }
                            else
                            {
                                if (DecoDef.HasValue)
                                {
                                    if (GeneraImageViewModel.Instance.PowerCache?.TryGetValue(4, out var selectedPower) == true)
                                        canvas.DrawBitmap(selectedPower, GeneraImageViewModel.Instance.CurrentImageInfo.PowerRect, paint);
                                    string atkText = cardItem.atk >= 0 ? cardItem.atk.ToString() : "？";
                                    TextRenderer.DrawSingleLineText(canvas, atkText, GeneraImageViewModel.Instance.CurrentImageInfo.ATK1Rect,
                                        ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _ATKPaintOCG : _ATKPaintTCG);
                                    string defText = DecoDef.Value >= 0 ? DecoDef.Value.ToString() : "？";
                                    TextRenderer.DrawSingleLineText(canvas, defText, GeneraImageViewModel.Instance.CurrentImageInfo.ATK2Rect,
                                        ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _ATKPaintOCG : _ATKPaintTCG);
                                    TextRenderer.DrawSingleLineText(canvas, (cardItem.level & 0xff).ToString(), GeneraImageViewModel.Instance.CurrentImageInfo.LinkRect,
                                        ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _LINKPaintOCG : _LINKPaintTCG);
                                }
                                else
                                {
                                    if (GeneraImageViewModel.Instance.PowerCache?.TryGetValue(2, out var selectedPower) == true)
                                        canvas.DrawBitmap(selectedPower, GeneraImageViewModel.Instance.CurrentImageInfo.PowerRect, paint);
                                    string atkText = cardItem.atk >= 0 ? cardItem.atk.ToString() : "？";
                                    TextRenderer.DrawSingleLineText(canvas, atkText, GeneraImageViewModel.Instance.CurrentImageInfo.ATK2Rect,
                                        ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _ATKPaintOCG : _ATKPaintTCG);
                                    TextRenderer.DrawSingleLineText(canvas, (cardItem.level & 0xff).ToString(), GeneraImageViewModel.Instance.CurrentImageInfo.LinkRect,
                                        ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _LINKPaintOCG : _LINKPaintTCG);
                                }
                            }
                        }
                        else
                        {
                            if (GeneraImageViewModel.Instance.PowerCache?.TryGetValue(0, out var selectedPower) == true)
                                canvas.DrawBitmap(selectedPower, GeneraImageViewModel.Instance.CurrentImageInfo.PowerRect, paint);


                            string atkText = cardItem.atk >= 0 ? cardItem.atk.ToString() : "？";
                            string defText = cardItem.def >= 0 ? cardItem.def.ToString() : "？";
                            TextRenderer.DrawSingleLineText(canvas, atkText, GeneraImageViewModel.Instance.CurrentImageInfo.ATK2Rect,
                                ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _ATKPaintOCG : _ATKPaintTCG);
                            TextRenderer.DrawSingleLineText(canvas, defText, GeneraImageViewModel.Instance.CurrentImageInfo.DEFRect,
                                ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _ATKPaintOCG : _ATKPaintTCG);
                        }
                    }
                    #endregion

                    #region Passcode
                    var selectedPassPaint = CardInfo.IsPendulum
                        ? (ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _PassPaintBlackOCG : _PassPaintBlackTCG)
                        : (CardInfo.IsXyz
                            ? (ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _PassPaintWhiteOCG : _PassPaintWhiteTCG)
                            : (ConfigViewModel.Instance.imageSetting.FormatEffect == 0 ? _PassPaintBlackOCG : _PassPaintBlackTCG));
                    TextRenderer.DrawSingleLineText(canvas, cardItem.id.ToString(),
                        (CardInfo.IsLink && CardInfo.IsPendulum)
                        ? GeneraImageViewModel.Instance.CurrentImageInfo.PassRectPenLink
                        : GeneraImageViewModel.Instance.CurrentImageInfo.PassRect, selectedPassPaint);
                    #endregion

                    #region Foild
                    if (!string.IsNullOrEmpty(ConfigViewModel.Instance.imageSetting.Foild) &&
                        ConfigViewModel.Instance.imageSetting.Foild != "None")
                    {
                        if (GeneraImageViewModel.Instance.FoildCache?.TryGetValue(ConfigViewModel.Instance.imageSetting.Foild, out var selectedFoild) == true)
                            canvas.DrawBitmap(selectedFoild, GeneraImageViewModel.Instance.CurrentImageInfo.FrameRect, paint);
                    }
                    #endregion

                    #region Rarity
                    if (ConfigViewModel.Instance.imageSetting.IncludeRare && GeneraImageViewModel.Instance.CurrentImageInfo.RarityLabelRect.HasValue)
                    {
                        long rarity = RareRawDataViewModel.Instance.GetRareByCardId(cardItem.id);
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
                    var outputFilePath = Path.Combine(outputFolderPath, $"{cardItem.id}.png");
                    if (!ImageValidate.TryDeleteFile(outputFilePath)) return (false, CMess.unableDelete.ToText());

                    using (var image = surface.Snapshot())
                    {
                        if (ConfigViewModel.Instance.imageSetting.ImageSize.Width != GeneraImageViewModel.Instance.CurrentImageInfo.CardWidth ||
                            ConfigViewModel.Instance.imageSetting.ImageSize.Height != GeneraImageViewModel.Instance.CurrentImageInfo.CardHeight)
                        {
                            using (var resizedImage = ResizeImage(image,
                                ConfigViewModel.Instance.imageSetting.ImageSize.Width,
                                ConfigViewModel.Instance.imageSetting.ImageSize.Height))
                            {
                                await ImageValidate.SaveImage(resizedImage, outputFilePath);
                            }
                        }
                        else
                        {
                            await ImageValidate.SaveImage(image, outputFilePath);
                        }
                    }
                    #endregion

                    return (true, outputFilePath);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                artWorkBitmap?.Dispose();
                artWorkStream?.Dispose();
                //SelectedNamePaint?.Dispose();
                //SelectedNamePaintOut?.Dispose();
            }
        }
        public static async Task<string> GenerateImageRare(CardEditor.Models.RareCard cardItem)
        {
            if (cardItem == null || string.IsNullOrWhiteSpace(outputFolderPath) || string.IsNullOrWhiteSpace(originalFolderPath))
                return string.Empty;

            return await Task.Run(async () =>
            {
                var info = new SKImageInfo(ConfigViewModel.Instance.imageSetting.ImageSize.Width,
                    ConfigViewModel.Instance.imageSetting.ImageSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
                using (var surface = SKSurface.Create(info))
                {
                    var canvas = surface.Canvas;
                    canvas.Clear(SKColors.Transparent);

                    string imageOriPath = FindIInfoService.FindImagePath(cardItem.id.ToString(), originalFolderPath);

                    if(string.IsNullOrEmpty(imageOriPath) || !File.Exists(imageOriPath))
                        return string.Empty;

                    using (var stream = File.OpenRead(imageOriPath))
                    using (var bitmap = SKBitmap.Decode(stream))
                    {
                        var destRect = new SKRect(0, 0, info.Width, info.Height);
                        canvas.DrawBitmap(bitmap, destRect);
                    }

                    #region Rarity
                    if (GeneraImageViewModel.Instance.CurrentImageInfo.RarityLabelRect.HasValue)
                    {
                        var rect = GeneraImageViewModel.Instance.CurrentImageInfo.RarityLabelRect.Value;
                    }

                    if (ConfigViewModel.Instance.imageSetting.IncludeRare && GeneraImageViewModel.Instance.CurrentImageInfo.RarityLabelRect.HasValue)
                    {
                        long rarity = RareRawDataViewModel.Instance.GetRareByCardId(cardItem.id);
                        SKBitmap selectedRarity = null;

                        if (rarity > 0 && RareRawDataViewModel.Instance.RarityCache?.Count > 0)
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
                            canvas.DrawBitmap(selectedRarity, GeneraImageViewModel.Instance.CurrentImageInfo.RarityLabelRect.Value, null);
                        }
                    }
                    #endregion

                    #region Savve
                    var outputFilePath = Path.Combine(outputFolderPath, $"{cardItem.id}.png");
                    if (!ImageValidate.TryDeleteFile(outputFilePath)) return string.Empty;

                    using (var image = surface.Snapshot())
                        await ImageValidate.SaveImage(image, outputFilePath);
                    #endregion

                    return outputFilePath;
                }
            });
        }

        private static string FindArtworkPath(string cardId)
        {
            if (_artworkCache.TryGetValue(cardId, out var path) && File.Exists(path)) return path;

            path = Directory.EnumerateFiles(ConfigViewModel.Instance.imageSetting.ArtworkFolder, $"{cardId}.*", SearchOption.AllDirectories)
                .FirstOrDefault(file => file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                        file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                        file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
            if (!string.IsNullOrEmpty(path))
                _artworkCache[cardId] = path;
            return path;
        }
        private static SKImage ResizeImage(SKImage originalImage, int targetWidth, int targetHeight)
        {
            var info = new SKImageInfo(targetWidth, targetHeight, SKColorType.Rgba8888, SKAlphaType.Premul);

            using (var surface = SKSurface.Create(info))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);

                using (var paint = new SKPaint())
                {
                    paint.IsAntialias = true;
                    paint.FilterQuality = SKFilterQuality.High; // Chất lượng resize tốt

                    var destRect = new SKRect(0, 0, targetWidth, targetHeight);
                    canvas.DrawImage(originalImage, destRect, paint);
                }

                return surface.Snapshot();
            }
        }
        
        public static async Task<string> GenerateArtWork10(string password, bool pendulum, string imagePath, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || string.IsNullOrWhiteSpace(targetPath) || string.IsNullOrWhiteSpace(password))
                return string.Empty;
            if (!File.Exists(imagePath) || !ImageValidate.IsImageFile(imagePath))
                return string.Empty;

            return await Task.Run(() =>
            {
                using var stream = new SKFileStream(imagePath);
                using var originalBitmap = SKBitmap.Decode(stream);
                
                if (originalBitmap == null) return string.Empty;
                
                int oriWidth = originalBitmap.Width;
                int oriHeight = originalBitmap.Height;
                int artworkWidth, artworkHeight;
                if (pendulum)
                {
                    artworkWidth = GeneraImageViewModel.Instance.CurrentImageInfo.AWPenWidth;
                    float scale = (float)artworkWidth / oriWidth;
                    int scaledHeight = (int)(oriHeight * scale);
                    artworkHeight = scaledHeight < GeneraImageViewModel.Instance.CurrentImageInfo.AWPenMinHeight
                    ? GeneraImageViewModel.Instance.CurrentImageInfo.AWPenMinHeight : scaledHeight;
                }
                else
                {
                    artworkWidth = GeneraImageViewModel.Instance.CurrentImageInfo.AWNorWidth;
                    artworkHeight = GeneraImageViewModel.Instance.CurrentImageInfo.AWNorWidth;
                }

                using var resizedBitmap = new SKBitmap(artworkWidth, artworkHeight, true);
                using var canvas = new SKCanvas(resizedBitmap);
                canvas.Clear(SKColors.Transparent);
                var destRect = new SKRect(0, 0, artworkWidth, artworkHeight);
                canvas.DrawBitmap(originalBitmap, destRect);

                Directory.CreateDirectory(targetPath);
                string outputPath = Path.Combine(targetPath, $"{password}.png");

                if (!ImageValidate.TryDeleteFile(outputPath))
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error, CMess.unableDelete.ToText(), new[] { CMess.ok.ToText() });
                    return string.Empty;
                }
                using var image = SKImage.FromBitmap(resizedBitmap);
                using var output = File.OpenWrite(outputPath);
                image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(output);
                return outputPath;
            });
        }
        public static async Task<(bool, string)> CopyArtWork(string password, string imagePath, string targetPath)
        {
            if (!Directory.Exists(targetPath)) return (false, CMess.folderNotExit.ToText());

            try
            {
                string newFilePath = Path.Combine(targetPath, $"{password}.png");

                using (FileStream sourceStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    using (FileStream destinationStream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write))
                    {
                        await sourceStream.CopyToAsync(destinationStream);
                    }
                }
                return (true, newFilePath);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private static bool HasTransparentArtWork(SKBitmap bitmap)
        {
            if (bitmap.AlphaType == SKAlphaType.Opaque)
                return false;

            // Nếu ảnh nhỏ hơn 1000px thì kiểm tra hết
            int step = bitmap.Width > 1000 ? 10 : 1;

            for (int x = 0; x < bitmap.Width; x += step)
            {
                SKColor pixel = bitmap.GetPixel(x, 9);
                if (pixel.Alpha < 230)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#pragma warning restore CS0612
#pragma warning restore CS0618