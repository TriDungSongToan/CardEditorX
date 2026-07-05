#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Models
{
    public class PendulumLanguageFile
    {
        public List<PendulumLanguageRule> Languages { get; set; } = new();
    }

    public class PendulumLanguageRule
    {
        public string Locale { get; set; } = string.Empty;

        // --- Scale ---
        public bool HasPenScalePattern { get; set; }
        public string? PenScalePattern { get; set; }
        public string? ScaleLineTemplate { get; set; }

        // --- Pendulum Header ---
        public PenHeaderMode PenHeaderMode { get; set; }
        public string? PendulumHeader { get; set; }

        // --- Separator ---
        public bool HasSeparator { get; set; }
        public string? Separator { get; set; }

        // --- Monster Header ---
        // Hầu hết mọi ngôn ngữ chính thức đều bắt buộc có Monster Header.
        // Rule fallback "EDOPro-NoPenEffect" là trường hợp duy nhất không có
        // (toàn bộ desc được coi là Monster Effect thuần).
        //
        // LƯU Ý: một ngôn ngữ có thể có NHIỀU biến thể header tùy loại quái thú
        // (Effect Monster dùng header "Effect", Normal Monster dùng header
        // "Description"/"Lore"). Vì vậy đây là danh sách, không phải 1 string cố định.
        public bool HasMonsterHeader { get; set; } = true;
        public List<string> MonsterHeaders { get; set; } = new();

        [JsonIgnore]
        public Regex? CompiledPenScaleRegex { get; set; }
    }

    public enum PenHeaderMode
    {
        /// <summary>Không bao giờ có header (ja-JP, zh-CN).</summary>
        Never,

        /// <summary>Luôn có header, kể cả khi Pendulum Effect rỗng (es-ES, ko-KR, zh-TW).</summary>
        Always,

        /// <summary>Có header khi có Pendulum Effect, biến mất hoàn toàn khi không có
        /// (en-US, de-DE, fr-FR, it-IT, pt-PT, vi-VN, EDOPro-có-Pen-Eff).</summary>
        Optional
    }

    public class PenDescResult
    {
        public string? PendulumEffect { get; set; }
        public string? MonsterEffect { get; set; }
    }

    public sealed record PenAnalysisResult
    {
        public PendulumLanguageRule RuleResult { get; init; } = default!;
        public PenDescResult DescResult { get; init; } = default!;
        public bool IsResolved { get; init; }
    }

    public class PenScale
    {
        public int LeftScale { get; set; }
        public int RightScale { get; set; }
    }

    public class PenDescProcessSummary
    {
        /// <summary>
        /// Tổng số card có cờ CardType.Pendulum được đưa vào xử lý (đếm ngay từ đầu vòng lặp, trước khi biết kết quả thành hay bại). Đây là mẫu số để tính tỷ lệ % cho các số liệu còn lại.
        /// </summary>
        public int Total;
        /// <summary>
        ///  Số card đã build lại desc thành công bằng newRule, tức là toàn bộ pipeline Analyze → BuildDesc chạy trót lọt không throw exception, và card.desc đã được gán giá trị mới.
        /// </summary>
        public int Success;
        /// <summary>
        /// Số card mà Analyze hoặc BuildDesc throw exception trong lúc xử lý. Card này bị bỏ qua, card.desc giữ nguyên giá trị gốc (không được gán lại). ID của các card này nằm trong ErrorCardIds.
        /// </summary>
        public int Error;
        /// <summary>
        /// Số card có desc rỗng hoặc chỉ toàn khoảng trắng (string.IsNullOrWhiteSpace) → bị skip ngay, không gọi Analyze/BuildDesc, không tính là lỗi. Comment gốc "-> Unknown" ý là: không xác định được ngôn ngữ vì không có gì để phân tích.
        /// </summary>
        public int EmptyDesc;
        /// <summary>
        /// Số card mà Analyze chạy được (không lỗi), nhưng kết quả phân tích rơi vào rule mặc định EdoProNoPenRule — tức là desc gốc không khớp với bất kỳ rule ngôn ngữ chính thức nào, phải fallback về format chung của EDOPro
        /// Card này vẫn được build lại bằng newRule bình thường (không bị loại khỏi Success), số này chỉ để theo dõi tỷ lệ desc "không rõ nguồn gốc ngôn ngữ".
        /// 1 card có thể vừa nằm trong EdoProFallback vừa nằm trong Success (2 số này không loại trừ nhau, vì EdoProFallback đếm ở giữa quá trình, Success đếm ở cuối nếu không exception).
        /// </summary>
        public int EdoProFallback;
        /// <summary>
        /// true nếu quá trình xử lý bị dừng giữa chừng do CancellationToken được yêu cầu hủy (user bấm Stop). Khi đó tất cả thay đổi đã tự động rollback.
        /// </summary>
        public bool Cancelled;
        /// <summary>
        /// true nếu đã tự động rollback toàn bộ card.desc về giá trị gốc bằng _lastSnapshot, do gặp Cancelled hoặc lỗi nghiêm trọng ngoài dự kiến (không phải lỗi per-card)
        /// Không liên quan đến rollback thủ công qua processor.Rollback()
        /// </summary>
        public bool RolledBack;
        /// <summary>
        /// Danh sách id của các card rơi vào nhóm Error, để tầng gọi (UI) có thể hiển thị chi tiết "những card nào bị lỗi, cần xem lại thủ công".
        /// </summary>
        public List<ulong> ErrorCardIds { get; } = new List<ulong>();
        /// <summary>
        /// Result kết quả tổng quái
        /// </summary>
        public bool Result { get; set; } = false;
        /// <summary>
        /// Message kết quả tổng quát
        /// </summary>
        public string Message { get; set; } = string.Empty;

        public string BuildMessage()
        {
            if (Cancelled) return CMess.cancelled.ToText();
            if (RolledBack) return CMess.rollbacked.ToText();

            var sb = new StringBuilder();
            sb.AppendLine(string.Format(CMess.changeSuc.ToText(), CMess.PendulumLanguage.ToText(), Success, Total));

            if (EmptyDesc > 0) sb.AppendLine(string.Format(CMess.emptyDesc.ToText(), EmptyDesc));

            if (EdoProFallback > 0) sb.AppendLine(string.Format(CMess.fallbackEDORule.ToText(), EdoProFallback));

            if (Error > 0)
            {
                sb.AppendLine(string.Format(CMess.changeLangFail.ToText(), Error));
                sb.AppendLine(CMess.failListID.ToText());

                foreach (var id in ErrorCardIds)
                {
                    sb.AppendLine(id.ToString());
                }
            }

            return sb.ToString();
        }
    }

    public static class PendulumLocales
    {
        public const string EDOPro = "EDOPro";
        public const string EDOProNonPen = "EDOProNonPen";
        public const string Unknown = "Unknown";
    }

}
