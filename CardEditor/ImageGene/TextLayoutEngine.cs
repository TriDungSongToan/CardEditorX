#pragma warning disable CS0612
#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using SkiaSharp;
using System.Management;
using System.Globalization;

namespace CardEditor.ImageGene
{
    /// <summary>
    /// Text layout engine scaffold inspired by ygocarder effect/lore/pendulum pipeline:
    /// normalize -> split -> fit -> draw.
    ///
    /// Milestone 1 implementation target:
    /// - Plain text wrapping
    /// - Font level fallback
    /// - Horizontal condense (SKCanvas scale)
    ///
    /// TODO milestones:
    /// - Rich token/tag support
    /// - Furigana/ruby and icon tags
    /// - OCG kinsoku rule parity
    /// </summary>
    public sealed class TextLayoutEngine
    {
        public const int MaxCondenseThreshold = 1000;

        /// <summary>
        /// Compatible paint copy helper for environments where <c>new SKPaint(existingPaint)</c>
        /// is unavailable or conflicts with SKFont overloads.
        /// </summary>
        public static SKPaint CopyBasePaint(SKPaint basePaint)
        {
            if (basePaint is null) throw new ArgumentNullException(nameof(basePaint));

            return new SKPaint
            {
                Typeface = basePaint.Typeface,
                TextSize = basePaint.TextSize,
                Color = basePaint.Color,
                IsAntialias = basePaint.IsAntialias,
                TextAlign = basePaint.TextAlign,
                Style = basePaint.Style,
                StrokeWidth = basePaint.StrokeWidth,
                StrokeCap = basePaint.StrokeCap,
                StrokeJoin = basePaint.StrokeJoin,
                TextScaleX = basePaint.TextScaleX,
                TextSkewX = basePaint.TextSkewX,
                FakeBoldText = basePaint.FakeBoldText,
                IsStroke = basePaint.IsStroke,
                IsDither = basePaint.IsDither,
                LcdRenderText = basePaint.LcdRenderText,
                SubpixelText = basePaint.SubpixelText,
                HintingLevel = basePaint.HintingLevel,
                FilterQuality = basePaint.FilterQuality,
            };
        }

        /// <summary>
        /// Convenience helper to build a profile from one base paint and multiple levels.
        /// </summary>
        public static EffectFontProfile BuildProfileFromBasePaint(SKPaint basePaint, IReadOnlyList<EffectFontLevel> levels,
            Func<SKPaint, bool, SKPaint> styleTransform = null)
        {
            if (basePaint is null) throw new ArgumentNullException(nameof(basePaint));
            if (levels is null || levels.Count == 0) throw new ArgumentException("Font levels cannot be empty", nameof(levels));

            return new EffectFontProfile(
                levels,
                (level, useItalic) =>
                {
                    var paint = CopyBasePaint(basePaint);
                    paint.TextSize = level.FontSize;
                    paint.IsAntialias = true;
                    paint.TextAlign = SKTextAlign.Left;
                    return styleTransform?.Invoke(paint, useItalic) ?? paint;
                }
            );
        }

        public string NormalizeCardText(string text, TextFormat format)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var normalized = text
                .Replace("\r\n", "\n")
                .Replace('"', '”')
                .Replace("--", "—")
                .Replace("● ", "●");

            // Milestone 1: keep normalizer minimal and stable.
            // Add OCG/TCG letter swap maps in next milestones when needed.
            return normalized.Trim();
        }

        public EffectSplitResult SplitEffect(string normalizedEffect, bool isNormal)
        {
            if (string.IsNullOrWhiteSpace(normalizedEffect))
            {
                return new EffectSplitResult(Array.Empty<string>(), string.Empty);
            }

            var effectText = normalizedEffect;
            var flavorCondition = string.Empty;

            // Milestone 1 lore condition heuristic:
            // extract last paragraph if separated by >=2 line breaks on Normal monster.
            if (isNormal)
            {
                var match = Regex.Match(effectText, "\\n\\s*\\n(?<cond>[^\\n].+)$", RegexOptions.Singleline);
                if (match.Success)
                {
                    flavorCondition = match.Groups["cond"].Value.Trim();
                    effectText = effectText.Substring(0, match.Index).TrimEnd();
                }
            }

            var lines = effectText
                .Split('\n')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            if (!string.IsNullOrEmpty(flavorCondition))
            {
                lines.Add(EffectSplitResult.FlavorLinePlaceholder);
            }

            return new EffectSplitResult(lines, flavorCondition);
        }

        public EffectLayoutResult FitLayout(SKCanvas canvas, string rawText, SKRect rect,
            EffectFontProfile fontProfile, TextLayoutOption option)
        {
            var normalized = NormalizeCardText(rawText, option.Format);
            var split = SplitEffect(normalized, option.IsNormal);
            var lineCandidates = split.LineList;
            if (lineCandidates.Count == 0)
            {
                return EffectLayoutResult.Empty with
                {
                    Format = option.Format,
                    EnableJustify = option.Format == TextFormat.TCG,
                };
            }

            var level = Math.Max(0, Math.Min(option.DefaultSizeLevel, fontProfile.FontLevels.Count - 1));
            //var level = Math.Clamp(option.DefaultSizeLevel, 0, fontProfile.FontLevels.Count - 1);
            var maxLevel = fontProfile.FontLevels.Count - 1;

            while (level <= maxLevel)
            {
                var fontLevel = fontProfile.FontLevels[level];
                using var paint = fontProfile.CreatePaint(fontLevel, option.UseItalic);

                var med = Condense(median =>
                {
                    var test = BuildLineLayout(paint, lineCandidates, split.FlavorCondition, rect, fontLevel, median,
                        () => fontProfile.CreatePaint(fontLevel, option.UseItalic));
                    return test.TotalLineCount <= fontLevel.LineCount;
                }, option.MinCondenseThreshold);

                var layout = BuildLineLayout(paint, lineCandidates, split.FlavorCondition, rect, fontLevel, med,
                    () => fontProfile.CreatePaint(fontLevel, option.UseItalic));
                if (layout.TotalLineCount <= fontLevel.LineCount && med >= option.TargetCondenseThreshold)
                {
                    return layout with
                    {
                        FontLevelIndex = level,
                        EffectiveMedian = med,
                        Format = option.Format,
                        EnableJustify = option.Format == TextFormat.TCG,
                    };
                }

                level += 1;
            }

            // fallback: smallest font with strongest condense
            {
                var fallbackLevel = fontProfile.FontLevels[maxLevel];
                using var paint = fontProfile.CreatePaint(fallbackLevel, option.UseItalic);
                var med = option.MinCondenseThreshold;
                var layout = BuildLineLayout(paint, lineCandidates, split.FlavorCondition, rect, fallbackLevel, med,
                    () => fontProfile.CreatePaint(fallbackLevel, option.UseItalic));
                return layout with
                {
                    FontLevelIndex = maxLevel,
                    EffectiveMedian = med,
                    Format = option.Format,
                    EnableJustify = option.Format == TextFormat.TCG,
                };
            }
        }

        public void DrawLayout(SKCanvas canvas, EffectLayoutResult layout, SKRect rect)
        {
            if (layout.Lines.Count == 0) return;

            using var paint = layout.PaintFactory();

            var baseline = rect.Top + layout.LineHeight;
            foreach (var line in layout.Lines)
            {
                canvas.Save();
                var scaleX = line.XRatio;
                canvas.Scale(scaleX, 1f);

                var drawX = rect.Left / scaleX;
                var scaledWidth = rect.Width / scaleX;
                var shouldJustify = layout.Format == TextFormat.TCG
                    && layout.EnableJustify
                    && !line.IsLastInParagraph;
                if (shouldJustify)
                {
                    DrawJustifiedLine(canvas, paint, line.Content, drawX, baseline, scaledWidth);
                }
                else
                {
                    canvas.DrawText(line.Content, drawX, baseline, paint);
                }

                canvas.Restore();
                baseline += layout.LineHeight;
            }
        }

        private static EffectLayoutResult BuildLineLayout(SKPaint paint, IReadOnlyList<string> lineCandidates,
            string flavorCondition, SKRect rect, EffectFontLevel fontLevel, int median, Func<SKPaint> paintFactory)
        {
            var xRatio = Math.Max(0.1f, median / 1000f);
            var scaledWidth = rect.Width / xRatio;
            var output = new List<EffectLineLayout>();

            foreach (var source in lineCandidates)
            {
                if (source == EffectSplitResult.FlavorLinePlaceholder)
                {
                    var cond = flavorCondition ?? string.Empty;
                    var wrapped = WrapSimple(paint, cond, scaledWidth).ToList();
                    for (var i = 0; i < wrapped.Count; i++)
                    {
                        output.Add(new EffectLineLayout(wrapped[i], xRatio, IsLastInParagraph: i == wrapped.Count - 1));
                    }
                    continue;
                }

                var lines = WrapSimple(paint, source, scaledWidth).ToList();
                for (var i = 0; i < lines.Count; i++)
                {
                    output.Add(new EffectLineLayout(lines[i], xRatio, IsLastInParagraph: i == lines.Count - 1));
                }
            }

            return new EffectLayoutResult(output, fontLevel.LineHeight, output.Count, xRatio, -1, median,
                paintFactory, TextFormat.TCG, EnableJustify: true);
        }

        private static IEnumerable<string> WrapSimple(SKPaint paint, string paragraph, float maxWidth)
        {
            if (string.IsNullOrWhiteSpace(paragraph)) yield break;

            // Prefer word wrapping for space-delimited text (TCG/latin).
            var hasWhitespace = paragraph.Any(char.IsWhiteSpace);
            if (hasWhitespace)
            {
                var words = paragraph.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length == 0) yield break;

                var current = words[0];
                for (var i = 1; i < words.Length; i++)
                {
                    var test = current + " " + words[i];
                    if (paint.MeasureText(test) <= maxWidth)
                    {
                        current = test;
                    }
                    else
                    {
                        yield return current;
                        current = words[i];
                    }
                }

                yield return current;
                yield break;
            }

            // Fallback for scripts with no spaces (OCG/Japanese/CJK): wrap by grapheme cluster.
            var textElementEnum = StringInfo.GetTextElementEnumerator(paragraph);
            var currentCjkLine = string.Empty;
            while (textElementEnum.MoveNext())
            {
                var token = textElementEnum.GetTextElement();
                var test = currentCjkLine + token;
                if (!string.IsNullOrEmpty(currentCjkLine) && paint.MeasureText(test) > maxWidth)
                {
                    yield return currentCjkLine;
                    currentCjkLine = token;
                }
                else
                {
                    currentCjkLine = test;
                }
            }

            if (!string.IsNullOrEmpty(currentCjkLine))
                yield return currentCjkLine;
        }

        private static void DrawJustifiedLine(SKCanvas canvas, SKPaint paint, string line, float drawX, float baseline, float width)
        {
            var words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length <= 1)
            {
                canvas.DrawText(line, drawX, baseline, paint);
                return;
            }

            var lineWidth = paint.MeasureText(line);
            var totalExtra = Math.Max(0, width - lineWidth);
            var gapCount = words.Length - 1;
            var extraPerGap = totalExtra / gapCount;

            var cursor = drawX;
            for (var i = 0; i < words.Length; i++)
            {
                var word = words[i];
                canvas.DrawText(word, cursor, baseline, paint);
                cursor += paint.MeasureText(word);
                if (i < words.Length - 1)
                {
                    cursor += paint.MeasureText(" ") + extraPerGap;
                }
            }
        }

        public static int Condense(Func<int, bool> worker, int minThreshold = 100)
        {
            var effective = MaxCondenseThreshold;
            var median = MaxCondenseThreshold;
            var step = 100;
            var iterate = 30;

            while (iterate-- > 0)
            {
                var ok = worker(median);
                if (!ok)
                {
                    median -= step;
                    if (median <= minThreshold)
                    {
                        effective = minThreshold;
                        break;
                    }
                    continue;
                }

                effective = median;
                if (median == MaxCondenseThreshold) break;
                median += step;
                step = Math.Max(1, step / 10);
                median -= step;
            }

            var forced = Math.Max(minThreshold, Math.Min(effective, MaxCondenseThreshold));
            worker(forced);
            return forced;
        }
    }

    public enum TextFormat
    {
        OCG,
        TCG,
    }

    public readonly record struct TextLayoutOption(
        TextFormat Format,
        bool IsNormal,
        bool UseItalic,
        int DefaultSizeLevel,
        int TargetCondenseThreshold,
        int MinCondenseThreshold)
    {
        public static TextLayoutOption Default(TextFormat format, bool isNormal, bool useItalic = false)
            => new(format, isNormal, useItalic, DefaultSizeLevel: 0, TargetCondenseThreshold: 600, MinCondenseThreshold: 200);
    }

    public sealed record EffectFontProfile(
        IReadOnlyList<EffectFontLevel> FontLevels,
        Func<EffectFontLevel, bool, SKPaint> CreatePaint);

    public readonly record struct EffectFontLevel(float FontSize, float LineHeight, int LineCount);

    public sealed record EffectSplitResult(IReadOnlyList<string> LineList, string FlavorCondition)
    {
        public const string FlavorLinePlaceholder = "__FLAVOR_LINE__";
    }

    public readonly record struct EffectLineLayout(string Content, float XRatio, bool IsLastInParagraph = true);

    public sealed record EffectLayoutResult(
        IReadOnlyList<EffectLineLayout> Lines,
        float LineHeight,
        int TotalLineCount,
        float XRatio,
        int FontLevelIndex,
        int EffectiveMedian,
        Func<SKPaint> PaintFactory,
        TextFormat Format,
        bool EnableJustify)
    {
        public static EffectLayoutResult Empty => new(
            Array.Empty<EffectLineLayout>(),
            0,
            0,
            1,
            0,
            TextLayoutEngine.MaxCondenseThreshold,
            () => new SKPaint(),
            TextFormat.TCG,
            EnableJustify: true);
    }
}
#pragma warning restore CS0612
#pragma warning restore CS0618