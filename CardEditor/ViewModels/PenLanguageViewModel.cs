#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Collections;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using CardAppContext = CardEditor.Models.AppContext;

namespace CardEditor.ViewModels
{
    public class PenLanguageViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<PenLanguageViewModel> _instance = new Lazy<PenLanguageViewModel>(() => new PenLanguageViewModel());
        public static PenLanguageViewModel Instance => _instance.Value;

        #region Data Storage
        private List<PendulumLanguageRule> _rules = new();
        public IReadOnlyList<PendulumLanguageRule> OrderedPenMaps => _rules;
        public BulkObservableCollection<PendulumLanguageRule> OrderedPenMapsUI { get; set; } = new();

        public PendulumLanguageRule EdoProNoPenRule { get; } = new()
        {
            Locale = PendulumLocales.EDOProNonPen,
            HasPenScalePattern = false,
            PenHeaderMode = PenHeaderMode.Never,
            HasSeparator = false,
            HasMonsterHeader = false,
            MonsterHeaders = new()
        };
        public PendulumLanguageRule UnknownRule { get; } = new()
        {
            Locale = PendulumLocales.Unknown,
            HasPenScalePattern = false,
            PenHeaderMode = PenHeaderMode.Never,
            HasSeparator = false,
            HasMonsterHeader = false,
            MonsterHeaders = new()
        };

        private Dictionary<Card, string>? _lastSnapshot;
        private readonly object _snapshotLock = new object();
        /// <summary>
        /// True nếu đang có 1 snapshot chưa được rollback (do lần chạy trước để lại).
        /// </summary>
        public bool HasPendingRollback
        {
            get { lock (_snapshotLock) return _lastSnapshot != null; }
        }
        #endregion

        #region Load
        public (bool, string) SetLanguage(List<PendulumLanguageRule> rules)
        {
            try
            {
                _rules.Clear();
                // Giữ nguyên thứ tự trong JSON, nhưng luôn đặt EDOPro xuống cuối.
                _rules = rules.Where(r => r.Locale != PendulumLocales.EDOPro)
                    .Concat(rules.Where(r => r.Locale == PendulumLocales.EDOPro))
                    .Append(EdoProNoPenRule).Append(UnknownRule).ToList();

                OrderedPenMapsUI.Clear();
                OrderedPenMapsUI.AddRange(_rules);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<(bool Success, string Error)> LoadAsync()
        {
            try
            {
                string filePath = CardAppContext.Instance.PenDescLangFilePath;
                var (lang, error) = await LoadDataAsync(filePath);

                if (lang is null) return (false, error ?? "Failed to load language data.");

                return SetLanguage(lang);
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }
        public async Task<(List<PendulumLanguageRule>? Data, string? Error)> LoadDataAsync(string filePath)
        {
            try
            {
                return await Task.Run(() =>
                {
                    var json = File.ReadAllText(filePath);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    options.Converters.Add(new JsonStringEnumConverter());

                    var result = JsonSerializer.Deserialize<PendulumLanguageFile>(json, options);

                    if (result?.Languages == null)
                        return ((List<PendulumLanguageRule>?)null, "Languages section not found.");

                    foreach (var lang in result.Languages)
                        Compile(lang);

                    return (result.Languages, (string?)null);
                });
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
        private static void Compile(PendulumLanguageRule lang)
        {
            if (lang.HasPenScalePattern && !string.IsNullOrWhiteSpace(lang.PenScalePattern))
                lang.CompiledPenScaleRegex = new Regex(lang.PenScalePattern, RegexOptions.Compiled);
        }
        #endregion

        #region Process Desc
        public PenDescProcessSummary ProcessPenDesc(IEnumerable<Card>? cards)
        {
            var summary = new PenDescProcessSummary();
            if (cards == null) return summary;

            foreach (var card in cards)
            {
                if ((card.type & (ulong)CardType.Pendulum) == 0) continue;
                summary.Total++;

                try
                {
                    if (string.IsNullOrWhiteSpace(card.desc))
                    {
                        summary.EmptyDesc++;
                        continue;
                    }

                    var analysisResult = Analyze(card.desc);

                    if (analysisResult.RuleResult.Locale == EdoProNoPenRule.Locale)
                    {
                        summary.EdoProFallback++;
                        // Vẫn build lại desc bình thường nếu cần, chỉ tách riêng để theo dõi tỷ lệ fallback
                    }

                    card.desc = BuildDesc(analysisResult.RuleResult, analysisResult.DescResult, card.level, card.type);
                    summary.Success++;
                }
                catch (Exception)
                {
                    summary.Error++;
                }
            }
            return summary;
        }
        public Task<PenDescProcessSummary> ProcessPenDesc(IEnumerable<Card>? cards, PendulumLanguageRule newRule)
        {
            return Task.Run(() =>
            {
                var summary = new PenDescProcessSummary();
                if (cards == null) return summary;

                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 2)
                };

                Parallel.ForEach(cards, options, card =>
                {
                    if ((card.type & (ulong)CardType.Pendulum) == 0)
                        return;

                    Interlocked.Increment(ref summary.Total);

                    try
                    {
                        if (string.IsNullOrWhiteSpace(card.desc))
                        {
                            Interlocked.Increment(ref summary.EmptyDesc);
                            return;
                        }

                        var analysisResult = Analyze(card.desc);

                        if (analysisResult.RuleResult.Locale == EdoProNoPenRule.Locale)
                        {
                            Interlocked.Increment(ref summary.EdoProFallback);
                        }

                        card.desc = BuildDesc(newRule, analysisResult.DescResult, card.level, card.type);
                        Interlocked.Increment(ref summary.Success);
                    }
                    catch (Exception)
                    {
                        Interlocked.Increment(ref summary.Error);
                    }
                });

                return summary;
            });
        }

        public Task<PenDescProcessSummary> ProcessPenDesc(IEnumerable<Card> cards, PendulumLanguageRule newRule,
            CancellationToken token, bool allowOverwritePendingSnapshot = false)
        {
            return Task.Run(() =>
            {
                var summary = new PenDescProcessSummary();
                if (cards == null) return summary;

                lock (_snapshotLock)
                {
                    if (_lastSnapshot != null && !allowOverwritePendingSnapshot)
                    {
                        throw new InvalidOperationException(
                            "There is one snapshot that hasn't been rolled back from the previous run." +
                            "Call Rollback() first, or pass allowOverwritePendingSnapshot: true if you accept losing the ability to roll back the previous time.");
                    }
                }

                var cardList = cards.ToList();

                var snapshot = new Dictionary<Card, string>();
                foreach (var card in cardList)
                {
                    snapshot[card] = card.desc;
                }

                var errorIdsLock = new object();

                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 2),
                    CancellationToken = token
                };

                try
                {
                    Parallel.ForEach(cardList, options, card =>
                    {
                        if ((card.type & (ulong)CardType.Pendulum) == 0) return;
                        Interlocked.Increment(ref summary.Total);

                        try
                        {
                            if (string.IsNullOrWhiteSpace(card.desc))
                            {
                                Interlocked.Increment(ref summary.EmptyDesc);
                                return;
                            }

                            var analysisResult = Analyze(card.desc);

                            if (analysisResult.RuleResult.Locale == EdoProNoPenRule.Locale)
                            {
                                Interlocked.Increment(ref summary.EdoProFallback);
                            }

                            card.desc = BuildDesc(newRule, analysisResult.DescResult, card.level, card.type);
                            Interlocked.Increment(ref summary.Success);
                        }
                        catch (Exception)
                        {
                            Interlocked.Increment(ref summary.Error);
                            lock (errorIdsLock)
                            {
                                summary.ErrorCardIds.Add(card.id);
                            }
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    RollbackFromDictionary(snapshot);
                    summary.Cancelled = true;
                    summary.RolledBack = true;
                    summary.Message = summary.BuildMessage();
                    return summary;
                }
                catch (Exception ex)
                {
                    RollbackFromDictionary(snapshot);
                    summary.RolledBack = true;
                    summary.Result = false;
                    summary.Message = ex.Message;
                    return summary;
                }

                lock (_snapshotLock)
                {
                    _lastSnapshot = snapshot;
                }

                summary.Result = true;
                summary.Message = summary.BuildMessage();
                return summary;
            }, token);
        }

        /// <summary>
        /// Rollback thủ công, gọi sau khi ProcessPenDesc đã chạy xong (thành công hoặc không).
        /// </summary>
        public (bool, string) PerformManualRollback()
        {
            try
            {
                lock (_snapshotLock)
                {
                    if (_lastSnapshot == null) return (false, CMess.noValiDataFound.ToText());
                    RollbackFromDictionary(_lastSnapshot);
                    _lastSnapshot = null;
                    return (true, string.Empty);
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        private static void RollbackFromDictionary(Dictionary<Card, string> snapshot)
        {
            foreach (var kvp in snapshot)
            {
                kvp.Key.desc = kvp.Value;
            }
        }
        #endregion

        #region Parser
        public PenAnalysisResult Analyze(string cardDesc)
        {
            if (string.IsNullOrWhiteSpace(cardDesc))
                return MakeEmptyResult(UnknownRule);

            var lines = cardDesc.Split(new[] { '\n' }, StringSplitOptions.None)
                .Select(l => l.TrimEnd()).ToArray();

            // OrderedPenMaps đảm bảo rule "EDOPro" (có Pen Effect) luôn ở cuối cùng,
            // vì nó không bắt buộc Scale nên dễ "nuốt nhầm" desc ngôn ngữ khác nếu thử trước.
            foreach (var rule in OrderedPenMaps)
            {
                var result = TryMatch(lines, rule);
                if (result is not null)
                    return result;
            }

            // Không rule nào match, desc không rỗng -> EDOPro không có Pendulum Effect.
            // Toàn bộ desc được coi là Monster Effect thuần.
            return MakeRawMonsterResult(cardDesc, EdoProNoPenRule);
        }

        private static PenAnalysisResult? TryMatch(string[] lines, PendulumLanguageRule rule)
        {
            int scaleLine = -1;
            int penHeadLine = -1;
            int sepLine = -1;
            int monsHeadLine = -1;

            // --- Bước 1: scaleLine — quét XUÔI từ đầu file, lấy match đầu tiên ---
            int searchStart = 0;
            if (rule.HasPenScalePattern)
            {
                if (rule.CompiledPenScaleRegex is null) return null;

                for (int i = 0; i < lines.Length; i++)
                {
                    if (rule.CompiledPenScaleRegex.IsMatch(lines[i]))
                    {
                        scaleLine = i;
                        break;
                    }
                }

                if (scaleLine == -1) return null; // bắt buộc phải có, không tìm thấy -> loại rule
                searchStart = scaleLine + 1;
            }

            // --- Bước 2: penHeadLine — quét XUÔI từ sau scaleLine, lấy match đầu tiên ---
            if (rule.PenHeaderMode != PenHeaderMode.Never)
            {
                if (rule.PendulumHeader is null) return null;

                for (int i = searchStart; i < lines.Length; i++)
                {
                    if (lines[i] == rule.PendulumHeader)
                    {
                        penHeadLine = i;
                        break;
                    }
                }

                // Always: bắt buộc phải có. Optional: không tìm thấy không có nghĩa là rule sai,
                // chỉ có nghĩa là card này không có Pendulum Effect.
                if (rule.PenHeaderMode == PenHeaderMode.Always && penHeadLine == -1) return null;
            }

            // --- Bước 3: monsHeadLine — quét NGƯỢC từ cuối file, lấy match đầu tiên gặp được ---
            // Một ngôn ngữ có thể có nhiều biến thể header (Effect Monster vs Normal Monster),
            // nên so khớp với toàn bộ rule.MonsterHeaders, không chỉ 1 string cố định.
            // Lưu ý: KHÔNG dùng biến thể match được để build lại desc (xem BuildDesc) —
            // biến thể cần dùng khi build phụ thuộc vào card.type (Normal/Effect),
            // không phụ thuộc vào text đã parse từ ngôn ngữ nguồn (tránh lẫn ngôn ngữ khi convert).
            if (!rule.HasMonsterHeader || rule.MonsterHeaders.Count == 0) return null;

            for (int i = lines.Length - 1; i >= 0; i--)
            {
                if (rule.MonsterHeaders.Contains(lines[i]))
                {
                    monsHeadLine = i;
                    break;
                }
            }

            if (monsHeadLine == -1) return null;

            // --- Bước 4: sepLine — quét NGƯỢC từ (monsHeadLine - 1) lùi về, lấy match gần Monster Header nhất ---
            if (rule.HasSeparator)
            {
                if (rule.Separator is null) return null;

                for (int i = monsHeadLine - 1; i >= 0; i--)
                {
                    if (lines[i] == rule.Separator)
                    {
                        sepLine = i;
                        break;
                    }
                }

                if (sepLine == -1) return null;
            }

            // --- Bước 5: Validate thứ tự — các mốc phải nằm đúng vị trí tương đối ---
            if (scaleLine != -1 && scaleLine >= monsHeadLine) return null;
            if (penHeadLine != -1 && penHeadLine >= monsHeadLine) return null;
            if (sepLine != -1 && penHeadLine != -1 && sepLine <= penHeadLine) return null;

            // --- Bước 6: Tách nội dung Pendulum Effect ---
            int penContentStart = penHeadLine != -1 ? penHeadLine + 1
                : scaleLine != -1 ? scaleLine + 1
                : 0;

            int penContentEnd = sepLine != -1 ? sepLine : monsHeadLine;

            string? pendulumEffect = null;
            if (penContentStart < penContentEnd)
            {
                var count = penContentEnd - penContentStart;
                var penLines = new string[count];
                Array.Copy(lines, penContentStart, penLines, 0, count);

                var penText = string.Join("\n", penLines).Trim();

                //var penText = string.Join('\n', lines[penContentStart..penContentEnd]).Trim();
                pendulumEffect = penText.Length > 0 ? penText : null;
            }

            // --- Bước 7: Tách nội dung Monster Effect — toàn bộ phần còn lại sau Monster Header ---
            string? monsterEffect = null;
            int monsContentStart = monsHeadLine + 1;
            if (monsContentStart < lines.Length)
            {
                var count = lines.Length - monsContentStart;
                var monsLines = new string[count];
                Array.Copy(lines, monsContentStart, monsLines, 0, count);
                var monsText = string.Join("\n", monsLines).Trim();

                //var monsText = string.Join('\n', lines[monsContentStart..]).Trim();
                monsterEffect = monsText.Length > 0 ? monsText : null;
            }

            return new PenAnalysisResult
            {
                RuleResult = rule,
                DescResult = new PenDescResult
                {
                    PendulumEffect = pendulumEffect,
                    MonsterEffect = monsterEffect
                },
                IsResolved = true
            };
        }

        private static PenAnalysisResult MakeEmptyResult(PendulumLanguageRule rule)
        {
            return new PenAnalysisResult
            {
                RuleResult = rule,
                DescResult = new PenDescResult
                {
                    PendulumEffect = null,
                    MonsterEffect = null
                },
                IsResolved = false
            };
        }
        private static PenAnalysisResult MakeRawMonsterResult(string cardDesc, PendulumLanguageRule rule)
        {
            var text = cardDesc.Trim();
            return new PenAnalysisResult
            {
                RuleResult = rule,
                DescResult = new PenDescResult
                {
                    PendulumEffect = null,
                    MonsterEffect = text.Length > 0 ? text : null
                },
                IsResolved = true
            };
        }
        #endregion

        #region Build
        /// <summary>
        /// Build desc hoàn chỉnh từ rule (ngôn ngữ ĐÍCH) + nội dung effect đã tách (PendulumEffect/MonsterEffect).
        /// isNormal lấy từ card.type (bất biến qua mọi ngôn ngữ)
        /// </summary>
        public string BuildDesc(PendulumLanguageRule rule, PenDescResult descResult, ulong level, ulong type)
        {
            PenScale scale = GetPenScaleHelp.GetPenScale(level);
            bool isNormal = (type & (ulong)CardType.Normal) != 0;
            return BuildDesc(rule, descResult, scale, isNormal);
        }
        /// <summary>
        /// Build desc hoàn chỉnh từ rule (ngôn ngữ ĐÍCH) + nội dung effect đã tách (PendulumEffect/MonsterEffect).
        /// rule có thể khác với rule đã dùng để Analyze ban đầu — đây chính là cơ chế đổi ngôn ngữ.
        ///
        /// isNormal quyết định biến thể Monster Header nào được dùng:
        /// - false (Effect Monster): luôn dùng rule.MonsterHeaders[0]
        /// - true  (Normal Monster): dùng rule.MonsterHeaders[1] nếu có, fallback về [0] nếu ngôn ngữ
        ///   đó chưa khai báo biến thể Normal Monster riêng.
        ///
        /// isNormal lấy từ card.type (bất biến qua mọi ngôn ngữ), KHÔNG lấy từ text đã parse của
        /// ngôn ngữ nguồn — nếu không, khi đổi ngôn ngữ sẽ vô tình ghi nhầm text ngôn ngữ cũ vào desc mới.
        /// </summary>
        public string BuildDesc(PendulumLanguageRule rule, PenDescResult descResult, PenScale scale, bool isNormal)
        {
            var sb = new StringBuilder();

            if (rule.HasPenScalePattern && rule.ScaleLineTemplate is not null)
            {
                var scaleLine = rule.ScaleLineTemplate
                    .Replace("{left}", scale.LeftScale.ToString())
                    .Replace("{right}", scale.RightScale.ToString());
                sb.AppendLine(scaleLine);
            }

            bool writeHeader = rule.PenHeaderMode switch
            {
                PenHeaderMode.Always => true,
                PenHeaderMode.Optional => descResult.PendulumEffect is not null,
                PenHeaderMode.Never => false,
                _ => false
            };

            if (writeHeader && rule.PendulumHeader is not null)
                sb.AppendLine(rule.PendulumHeader);

            if (descResult.PendulumEffect is not null)
                sb.AppendLine(descResult.PendulumEffect);

            if (rule.HasSeparator && rule.Separator is not null)
                sb.AppendLine(rule.Separator);

            if (rule.HasMonsterHeader)
            {
                var headerToWrite = SelectMonsterHeader(rule, isNormal);
                if (headerToWrite is not null)
                    sb.AppendLine(headerToWrite);
            }

            if (descResult.MonsterEffect is not null)
                sb.Append(descResult.MonsterEffect);

            return sb.ToString();
        }
        private static string? SelectMonsterHeader(PendulumLanguageRule rule, bool isNormal)
        {
            if (!isNormal) return rule.MonsterHeaders.FirstOrDefault();

            return rule.MonsterHeaders.Count > 1
                ? rule.MonsterHeaders[1]
                : rule.MonsterHeaders.FirstOrDefault();
        }
        #endregion

        #region Helper
        public bool HasLastSnapshot()
        {
            return _lastSnapshot != null && _lastSnapshot.Count > 0;
        }
        #endregion

        #region Event
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {

        }
        #endregion

    }
}
