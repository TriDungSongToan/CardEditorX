#pragma warning disable CS0618
using System;
using System.Linq;
using System.Text;
using SkiaSharp;
using System.Threading.Tasks;
using CardEditor.ImagesConfig;
using System.Collections.Generic;
using System.Diagnostics;
using CardEditor.ViewModels;

namespace CardEditor.ImageGene
{
    public class TextRenderer
    {
        public static void DrawCardName(SKCanvas canvas, string text, SKRect rect, SKPaint paint)
        {
            if (canvas == null || string.IsNullOrEmpty(text) || rect == null || paint == null) return;

            float textWidth = paint.MeasureText(text);
            var metrics = paint.FontMetrics;
            float textHeight = metrics.Descent - metrics.Ascent;

            float scaleX = textWidth > rect.Width ? rect.Width / textWidth : 1f;
            float scaleY = textHeight > rect.Height ? rect.Height / textHeight : 1f;

            float scaledTextHeight = textHeight * scaleY;
            float x = rect.Left;
            float y = rect.Top + (rect.Height + scaledTextHeight) / 2 - metrics.Descent * scaleY;

            canvas.Save();
            canvas.Translate(x, y);
            canvas.Scale(scaleX, scaleY);

            var fallbackTypeface = ConfigViewModel.Instance.imageSetting.FormatName == 0
                ? GeneraImageViewModel.Instance._fallbackTypefaceOCG
                : GeneraImageViewModel.Instance._fallbackTypefaceTCG;
            DrawTextWithFallback(canvas, text, 0, 0, paint, fallbackTypeface);
            //canvas.DrawText(text, 0, 0, paint);

            canvas.Restore();
        }
        private static void DrawTextWithFallback(SKCanvas canvas, string text, float x, float y, SKPaint paint, SKTypeface fallbackTypeface)
        {
            var primaryTypeface = paint.Typeface;
            float currentX = x;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                string charStr = c.ToString();

                bool usesPrimary = CharExistsInTypeface(charStr, paint, primaryTypeface);
                if (usesPrimary)
                {
                    canvas.DrawText(charStr, currentX, y, paint);
                    currentX += paint.MeasureText(charStr);
                }
                else
                {
                    // Vẽ với fallback font
                    using var fallbackPaint = paint.Clone();
                    fallbackPaint.Typeface = fallbackTypeface;
                    canvas.DrawText(charStr, currentX, y, fallbackPaint);
                    currentX += fallbackPaint.MeasureText(charStr);
                }
            }
        }
        private static bool CharExistsInTypeface(string text, SKPaint basePaint, SKTypeface typeface)
        {
            using var testPaint = basePaint.Clone();
            testPaint.Typeface = typeface;

            // Cách 1: Dùng GetGlyphs đúng API
            ushort[] glyphs = testPaint.GetGlyphs(text);

            return glyphs != null && glyphs.Length > 0 && glyphs[0] != 0;
        }

        public static void DrawMonsterTypeLineText(SKCanvas canvas, string text, SKRect rect, SKPaint paint)
        {
            if (canvas == null || string.IsNullOrEmpty(text) || rect == null || paint == null) return;

            float textWidth = paint.MeasureText(text);
            var metrics = paint.FontMetrics;
            float textHeight = metrics.Descent - metrics.Ascent;

            float scaleX = textWidth > rect.Width ? rect.Width / textWidth : 1f;
            float scaleY = textHeight > rect.Height ? rect.Height / textHeight : 1f;

            float scaledTextHeight = textHeight * scaleY;
            float x = rect.Left;
            float y = rect.Top + (rect.Height + scaledTextHeight) / 2 - metrics.Descent * scaleY;

            canvas.Save();
            canvas.Translate(x, y);
            canvas.Scale(scaleX, scaleY);
            canvas.DrawText(text, 0, 0, paint);
            canvas.Restore();
        }
        public static void DrawSingleLineText(SKCanvas canvas, string text, SKRect rect, SKPaint paint)
        {
            if (canvas == null || string.IsNullOrEmpty(text) || rect == null || paint == null) return;

            float textWidth = paint.MeasureText(text);
            var metrics = paint.FontMetrics;
            float textHeight = metrics.Descent - metrics.Ascent;

            float scaleX = textWidth > rect.Width ? rect.Width / textWidth : 1f;
            //float scaleY = 1f;

            float baseline = rect.Top + rect.Height / 2 - (metrics.Ascent + metrics.Descent) / 2;

            canvas.Save();
            canvas.Translate(rect.Left, baseline);
            canvas.Scale(scaleX, 1f); // Chỉ scale X, giữ nguyên Y = 1f
            canvas.DrawText(text, 0, 0, paint);
            canvas.Restore();
        }
        public static void DrawSingleLineCenterText(SKCanvas canvas, string text, SKRect rect, SKPaint paint)
        {
            if (canvas == null || string.IsNullOrEmpty(text) || paint == null) return;

            var metrics = paint.FontMetrics;

            // Đo text
            float textWidth = paint.MeasureText(text);
            float scaleX = textWidth > rect.Width ? rect.Width / textWidth : 1f;

            // Center chuẩn theo rect
            float x = rect.MidX;
            float y = rect.MidY - (metrics.Ascent + metrics.Descent) / 2;

            canvas.Save();
            canvas.Translate(x, y);
            canvas.Scale(scaleX, 1f);
            canvas.DrawText(text, 0, 0, paint);
            canvas.Restore();
        }
        public static void DrawJustifiedEffect(SKCanvas canvas, SKPaint paint, string monsterEffect, SKRect rect)
        {
            if (string.IsNullOrEmpty(monsterEffect) || canvas == null || paint == null)
                return;

            float maxFontSize = GeneraImageViewModel.Instance.CurrentImageInfo.FontSizeDesc;
            float minFontSize = 5;
            float currentFontSize = maxFontSize;
            float lineSpacing = 1.2f;

            using (var tempPaint = new SKPaint
            {
                Typeface = paint.Typeface,
                TextSize = paint.TextSize,
                Color = paint.Color,
                IsAntialias = paint.IsAntialias,
                TextAlign = paint.TextAlign
            })
            {
                bool textFits = false;
                List<(string line, bool isJustified)> linesToDraw = null;

                // Thử các font size cho đến khi text vừa
                while (currentFontSize >= minFontSize && !textFits)
                {
                    tempPaint.TextSize = currentFontSize;
                    linesToDraw = new List<(string, bool)>();
                    float totalHeight = 0;

                    // Chia text thành các dòng
                    var lines = monsterEffect.Split(new[] { '\n' }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        float lineWidth = tempPaint.MeasureText(line);
                        if (lineWidth <= rect.Width)
                        {
                            // Dòng ngắn, căn trái
                            linesToDraw.Add((line, false));
                            totalHeight += currentFontSize * lineSpacing;
                        }
                        else
                        {
                            // Dòng dài, chia thành các dòng con
                            var wrappedLines = WrapText(line, tempPaint, rect.Width);
                            for (int i = 0; i < wrappedLines.Count; i++)
                            {
                                // Căn đều cho các dòng con, trừ dòng cuối (căn trái)
                                bool isJustified = i < wrappedLines.Count - 1;
                                linesToDraw.Add((wrappedLines[i], isJustified));
                                totalHeight += currentFontSize * lineSpacing;
                            }
                        }
                    }

                    // Kiểm tra xem tổng chiều cao có vừa rect không
                    if (totalHeight <= rect.Height)
                    {
                        textFits = true;
                    }
                    else
                    {
                        currentFontSize -= 0.5f; // Giảm font size
                    }
                }

                if (!textFits)
                {
                    // Text vẫn không vừa, có thể ghi log hoặc bỏ qua
                    return;
                }

                // Vẽ các dòng
                float y = rect.Top;
                foreach (var (line, isJustified) in linesToDraw)
                {
                    if (isJustified)
                    {
                        DrawJustifiedText(canvas, line, rect.Left, y, rect.Width, tempPaint);
                    }
                    else
                    {
                        canvas.DrawText(line, rect.Left, y + currentFontSize, tempPaint);
                    }
                    y += currentFontSize * lineSpacing;
                }
            }
        }
        private static List<string> WrapText(string text, SKPaint paint, float maxWidth)
        {
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var lines = new List<string>();
            string currentLine = "";

            foreach (var word in words)
            {
                string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                if (paint.MeasureText(testLine) <= maxWidth)
                {
                    currentLine = testLine;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        lines.Add(currentLine);
                    }
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }

            return lines;
        }
        private static void DrawJustifiedText(SKCanvas canvas, string text, float x, float y, float maxWidth, SKPaint paint)
        {
            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
            {
                canvas.DrawText(text, x, y + paint.TextSize, paint);
                return;
            }

            float totalWordWidth = words.Sum(w => paint.MeasureText(w));
            float spaceWidth = (maxWidth - totalWordWidth) / (words.Length - 1);
            float currentX = x;

            for (int i = 0; i < words.Length; i++)
            {
                canvas.DrawText(words[i], currentX, y + paint.TextSize, paint);
                currentX += paint.MeasureText(words[i]) + spaceWidth;
            }
        }

    }
}
#pragma warning restore CS0618
