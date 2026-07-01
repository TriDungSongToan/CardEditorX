using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardEditor.Helpers
{
    /// <summary>
    /// Phát hiện các hệ chữ viết (script) xuất hiện trong một chuỗi text hỗn hợp
    /// nhiều ngôn ngữ (Anh, Việt, Trung, Nhật, Hàn, ...).
    ///
    /// Nguyên lý: phân loại theo Unicode code point range của từng ký tự,
    /// thay vì phân loại "ngôn ngữ của cả đoạn text" như các thư viện
    /// language-detection thông thường (vốn không phù hợp với text hỗn hợp).
    /// </summary>
    /// 
    public enum ScriptType
    {
        Latin,       // chữ Latin cơ bản, không dấu (thường là tiếng Anh)
        Vietnamese,  // chữ Latin có dấu tiếng Việt
        Han,         // Hán tự (dùng chung cho Trung + Kanji của Nhật)
        Hiragana,
        Katakana,
        Hangul,      // tiếng Hàn
        Digit,
        Other
    }

    public class DetectionResult
    {
        public Dictionary<ScriptType, int> Counts { get; } = new();

        public bool Contains(ScriptType type) => Counts.ContainsKey(type) && Counts[type] > 0;

        public bool ContainsVietnamese => Contains(ScriptType.Vietnamese);
        public bool ContainsEnglishOrLatin => Contains(ScriptType.Latin);
        public bool ContainsKorean => Contains(ScriptType.Hangul);
        public bool ContainsJapanese => Contains(ScriptType.Hiragana) || Contains(ScriptType.Katakana);

        // Có Hán tự nhưng KHÔNG có Hiragana/Katakana => suy đoán là tiếng Trung.
        // (Lưu ý: văn bản Nhật chỉ dùng toàn Kanji, không Kana, sẽ bị nhận nhầm
        // thành tiếng Trung - trường hợp này hiếm gặp trong thực tế.)
        public bool ContainsChinese => Contains(ScriptType.Han) && !ContainsJapanese;
    }

    public static class MixedLanguageDetector
    {
        /// <summary>
        /// Kết quả phân tích: tập hợp các script xuất hiện + số ký tự của mỗi loại.
        /// </summary>
        
        public static DetectionResult Detect(string text)
        {
            var result = new DetectionResult();
            if (string.IsNullOrEmpty(text)) return result;

            // Dùng EnumerateRunes để xử lý đúng các ký tự nằm ngoài BMP (surrogate pairs) (tương thích .NET Core)
            //foreach (var rune in text.EnumerateRunes())
            //{
            //    var type = Classify(rune.Value);
            //    result.Counts.TryGetValue(type, out var c);
            //    result.Counts[type] = c + 1;
            //}


            // Xử lý surrogate pairs thủ công để đọc đúng code point (tương thích .NET Framework 4.x)
            for (int i = 0; i < text.Length; i++)
            {
                int cp;
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    cp = char.ConvertToUtf32(text[i], text[i + 1]);
                    i++; // bỏ qua low surrogate
                }
                else
                {
                    cp = text[i];
                }

                var type = Classify(cp);
                result.Counts.TryGetValue(type, out var c);
                result.Counts[type] = c + 1;
            }


            return result;
        }

        private static ScriptType Classify(int cp)
        {
            // ===== Hangul (Hàn) =====
            if ((cp >= 0xAC00 && cp <= 0xD7A3) ||   // Hangul Syllables
                (cp >= 0x1100 && cp <= 0x11FF) ||   // Hangul Jamo
                (cp >= 0x3130 && cp <= 0x318F))     // Hangul Compatibility Jamo
                return ScriptType.Hangul;

            // ===== Hiragana (Nhật) =====
            if (cp >= 0x3040 && cp <= 0x309F)
                return ScriptType.Hiragana;

            // ===== Katakana (Nhật) =====
            if (cp >= 0x30A0 && cp <= 0x30FF)
                return ScriptType.Katakana;

            // ===== Hán tự: dùng chung Trung Quốc + Kanji Nhật =====
            if ((cp >= 0x4E00 && cp <= 0x9FFF) ||   // CJK Unified Ideographs
                (cp >= 0x3400 && cp <= 0x4DBF) ||   // CJK Extension A
                (cp >= 0xF900 && cp <= 0xFAFF))     // CJK Compatibility Ideographs
                return ScriptType.Han;

            // ===== Tiếng Việt: ký tự Latin có dấu =====
            if (IsVietnameseDiacritic(cp))
                return ScriptType.Vietnamese;

            // ===== Latin cơ bản (a-z, A-Z) =====
            if ((cp >= 0x0041 && cp <= 0x005A) || (cp >= 0x0061 && cp <= 0x007A))
                return ScriptType.Latin;

            if (cp >= 0x0030 && cp <= 0x0039)
                return ScriptType.Digit;

            return ScriptType.Other;
        }

        /// <summary>
        /// Kiểm tra ký tự có thuộc bộ ký tự đặc trưng của tiếng Việt không.
        /// Bao gồm: ă, â, đ, ê, ô, ơ, ư (và dạng hoa) + các nguyên âm có dấu thanh
        /// nằm trong block Latin-1 Supplement, Latin Extended-A/B,
        /// và Latin Extended Additional (nơi chứa hầu hết tổ hợp dấu thanh tiếng Việt).
        /// </summary>
        private static bool IsVietnameseDiacritic(int cp)
        {
            // Latin Extended Additional: chứa phần lớn ký tự Việt có dấu thanh
            // (ví dụ: ạ ả ấ ầ ẩ ẫ ậ ắ ằ ẳ ẵ ặ ẹ ẻ ẽ ế ề ể ễ ệ ỉ ị ọ ỏ ố ồ ổ ỗ ộ
            //  ớ ờ ở ỡ ợ ụ ủ ứ ừ ử ữ ự ỳ ỵ ỷ ỹ ...)
            if (cp >= 0x1EA0 && cp <= 0x1EF9)
                return true;

            // Các ký tự nền đặc trưng: ă â đ ê ô ơ ư (Latin Extended-A/B)
            int[] viBaseChars =
            {
                0x0103, 0x00E2, 0x0111, 0x00EA, 0x00F4, 0x01A1, 0x01B0, // chữ thường
                0x0102, 0x00C2, 0x0110, 0x00CA, 0x00D4, 0x01A0, 0x01AF  // chữ hoa
            };
            if (viBaseChars.Contains(cp))
                return true;

            // Nguyên âm có dấu huyền/sắc/hỏi/ngã đơn giản trong Latin-1 Supplement
            // (à á ả ã, è é ẻ ẽ, ì í ỉ ĩ, ò ó ỏ õ, ù ú ủ ũ, ỳ ý...)
            int[] viLatin1 =
            {
                0x00E0, 0x00E1, 0x00E3, 0x00E8, 0x00E9, 0x00EC, 0x00ED,
                0x00F2, 0x00F3, 0x00F5, 0x00F9, 0x00FA, 0x00FD,
                0x00C0, 0x00C1, 0x00C3, 0x00C8, 0x00C9, 0x00CC, 0x00CD,
                0x00D2, 0x00D3, 0x00D5, 0x00D9, 0x00DA, 0x00DD
            };
            if (viLatin1.Contains(cp))
                return true;

            return false;
        }
    }
}
