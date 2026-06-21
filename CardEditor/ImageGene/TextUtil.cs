#pragma warning disable CS0618
using System;
using System.Linq;
using System.Text;
using SkiaSharp;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace CardEditor.ImageGene
{
    public static class TextUtil
    {
        public static List<string> WordWrap(SKPaint paint, string text, float maxWidth)
        {
            var lines = new List<string>();
            var words = text.Split(' ');
            string line = "";
            foreach (var word in words)
            {
                var testLine = string.IsNullOrEmpty(line) ? word : $"{line} {word}";
                if (paint.MeasureText(testLine) > maxWidth)
                {
                    if (!string.IsNullOrEmpty(line))
                        lines.Add(line);
                    line = word;
                }
                else
                {
                    line = testLine;
                }
            }
            if (!string.IsNullOrEmpty(line))
                lines.Add(line);
            return lines;
        }

        public static float GetTextBlockHeight(SKPaint paint, List<List<string>> allLines, float lineSpacing = 1.15f)
        {
            var lineHeight = paint.FontMetrics.Descent - paint.FontMetrics.Ascent;
            int totalLines = allLines.Sum(x => x.Count);
            float blockHeight = totalLines * lineHeight * lineSpacing;
            Debug.WriteLine($"LineHeight: {lineHeight}, TotalLines: {totalLines}, BlockHeight: {blockHeight}, LineSpacing: {lineSpacing}");
            return totalLines * lineHeight * lineSpacing;
        }

        public static float FindFittingTextSize(SKPaint paint, string desc, SKRect rect, float minSize = 5, float maxSize = 42, float lineSpacing = 1.15f)
        {
            float textSize = maxSize;
            while (textSize >= minSize)
            {
                paint.TextSize = textSize;

                // Tách từng đoạn (dòng gốc), wrap từng đoạn, rồi cộng lại số dòng
                var paragraphs = desc.Replace("\r", "").Split('\n');
                var allLines = new List<List<string>>();
                foreach (var para in paragraphs)
                {
                    allLines.Add(WordWrap(paint, para, rect.Width));
                }
                float blockHeight = GetTextBlockHeight(paint, allLines, lineSpacing);
                if (blockHeight <= rect.Height)
                    return textSize;
                textSize -= 1;
            }
            return minSize;
        }

        public static void DrawMultiLineJustified(SKCanvas canvas, SKPaint paint, string desc, SKRect rect, float lineSpacing = 1.15f)
        {
            var lineHeight = paint.FontMetrics.Descent - paint.FontMetrics.Ascent;
            float y = rect.Top - paint.FontMetrics.Ascent;
            int lineCount = 0;
            var paragraphs = desc.Replace("\r", "").Split('\n');
            foreach (var para in paragraphs)
            {
                var lines = WordWrap(paint, para, rect.Width);

                for (int i = 0; i < lines.Count; i++)
                {
                    if (y + lineHeight * lineSpacing > rect.Bottom)
                    {
                        break;
                    }
                    string line = lines[i];
                    float x = rect.Left;
                    float lineWidth = paint.MeasureText(line);

                    bool isLastSubline = (i == lines.Count - 1);

                    if (!isLastSubline && line.Trim().Contains(' '))
                    {
                        // Căn đều cho các dòng con không phải dòng cuối cùng
                        var words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int gaps = words.Length - 1;
                        float totalWordsWidth = words.Sum(w => paint.MeasureText(w));
                        float totalSpace = rect.Width - totalWordsWidth;
                        float gapWidth = totalSpace / gaps;

                        float wordX = x;
                        for (int j = 0; j < words.Length; j++)
                        {
                            canvas.DrawText(words[j], wordX, y, paint);
                            wordX += paint.MeasureText(words[j]);
                            if (j != words.Length - 1)
                                wordX += gapWidth;
                        }
                    }
                    else
                    {
                        // Dòng con cuối cùng của đoạn (hoặc chỉ có 1 dòng) => vẽ bình thường, không kéo dãn
                        canvas.DrawText(line, x, y, paint);
                    }

                    y += lineHeight * lineSpacing;
                    lineCount++;
                    if (y > rect.Bottom) return;
                }
            }
        }
    }
}
#pragma warning restore CS0618