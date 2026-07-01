#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public int Total { get; set; }           // Tổng card Pendulum được xử lý
        public int Success { get; set; }         // Match được rule ngôn ngữ chính thức
        public int EmptyDesc { get; set; }       // Desc rỗng -> Unknown
        public int EdoProFallback { get; set; }  // Desc không rỗng nhưng không match rule nào -> EDOPro
        public int Error { get; set; }           // Exception trong quá trình xử lý
    }
}
