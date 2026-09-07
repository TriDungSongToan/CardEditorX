using System;
using CardEditor.Enums;

namespace CardEditor.Helpers
{
    public class LanguageDetectorHelper
    {
        public static LanguageArea CheckLanguageArea(string input)
        {
            if (string.IsNullOrEmpty(input)) return LanguageArea.Unknown;

            bool hasWestern = false;
            bool hasEastern = false;

            for (int i = 0; i < input.Length; i++)
            {
                int code = Char.ConvertToUtf32(input, i);

                // Nếu là surrogate pair, bỏ qua char thứ 2
                if (code > 0xFFFF) i++;

                // ----- Western -----
                bool isWesternHalf = (code >= 0x0020 && code <= 0x007E);
                bool isWesternFull = (code >= 0xFF01 && code <= 0xFF5E);
                bool isLatinExtended = (code >= 0x00C0 && code <= 0x024F) || (code >= 0x1E00 && code <= 0x1EFF);

                if (isWesternHalf || isWesternFull || isLatinExtended) hasWestern = true;

                // ----- Eastern -----
                bool isCJK = (code >= 0x4E00 && code <= 0x9FFF);
                bool isHiragana = (code >= 0x3040 && code <= 0x309F);
                bool isKatakana = (code >= 0x30A0 && code <= 0x30FF);
                bool isHalfKatakana = (code >= 0xFF61 && code <= 0xFF9F);

                if (isCJK || isHiragana || isKatakana || isHalfKatakana) hasEastern = true;

                // Nếu đã có cả hai loại → hỗn hợp → return null
                if (hasWestern && hasEastern) return LanguageArea.Mixed;
            }

            if (hasWestern && !hasEastern) return LanguageArea.TCG;
            if (!hasWestern && hasEastern) return LanguageArea.OCG;

            return LanguageArea.Unknown;
        }
    }
}
