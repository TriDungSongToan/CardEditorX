using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardEditor.Converter
{
    public static class StringConvert
    {
        private static readonly Dictionary<char, char> SuperscriptMap = new()
        {
            // Numbers
            ['0'] = '⁰',
            ['1'] = '¹',
            ['2'] = '²',
            ['3'] = '³',
            ['4'] = '⁴',
            ['5'] = '⁵',
            ['6'] = '⁶',
            ['7'] = '⁷',
            ['8'] = '⁸',
            ['9'] = '⁹',

            // Lowercase
            ['a'] = 'ᵃ',
            ['b'] = 'ᵇ',
            ['c'] = 'ᶜ',
            ['d'] = 'ᵈ',
            ['e'] = 'ᵉ',
            ['f'] = 'ᶠ',
            ['g'] = 'ᵍ',
            ['h'] = 'ʰ',
            ['i'] = 'ⁱ',
            ['j'] = 'ʲ',
            ['k'] = 'ᵏ',
            ['l'] = 'ˡ',
            ['m'] = 'ᵐ',
            ['n'] = 'ⁿ',
            ['o'] = 'ᵒ',
            ['p'] = 'ᵖ',
            // q does not exist
            ['r'] = 'ʳ',
            ['s'] = 'ˢ',
            ['t'] = 'ᵗ',
            ['u'] = 'ᵘ',
            ['v'] = 'ᵛ',
            ['w'] = 'ʷ',
            ['x'] = 'ˣ',
            ['y'] = 'ʸ',
            ['z'] = 'ᶻ',

            // Uppercase (partial support)
            ['A'] = 'ᴬ',
            ['B'] = 'ᴮ',
            ['D'] = 'ᴰ',
            ['E'] = 'ᴱ',
            ['G'] = 'ᴳ',
            ['H'] = 'ᴴ',
            ['I'] = 'ᴵ',
            ['J'] = 'ᴶ',
            ['K'] = 'ᴷ',
            ['L'] = 'ᴸ',
            ['M'] = 'ᴹ',
            ['N'] = 'ᴺ',
            ['O'] = 'ᴼ',
            ['P'] = 'ᴾ',
            ['R'] = 'ᴿ',
            ['T'] = 'ᵀ',
            ['U'] = 'ᵁ',
            ['W'] = 'ᵂ',

            // Math symbols
            ['+'] = '⁺',
            ['-'] = '⁻',
            ['='] = '⁼',
            ['('] = '⁽',
            [')'] = '⁾',

            // Extra
            [' '] = ' '
        };
        private static readonly Dictionary<char, char> ReverseSuperscriptMap = SuperscriptMap.ToDictionary(pair => pair.Value, pair => pair.Key);

        private static readonly Dictionary<char, char> SubscriptMap = new()
        {
            // Numbers
            ['0'] = '₀',
            ['1'] = '₁',
            ['2'] = '₂',
            ['3'] = '₃',
            ['4'] = '₄',
            ['5'] = '₅',
            ['6'] = '₆',
            ['7'] = '₇',
            ['8'] = '₈',
            ['9'] = '₉',

            // Lowercase (Unicode hỗ trợ hạn chế)
            ['a'] = 'ₐ',
            ['e'] = 'ₑ',
            ['h'] = 'ₕ',
            ['i'] = 'ᵢ',
            ['j'] = 'ⱼ',
            ['k'] = 'ₖ',
            ['l'] = 'ₗ',
            ['m'] = 'ₘ',
            ['n'] = 'ₙ',
            ['o'] = 'ₒ',
            ['p'] = 'ₚ',
            ['r'] = 'ᵣ',
            ['s'] = 'ₛ',
            ['t'] = 'ₜ',
            ['u'] = 'ᵤ',
            ['v'] = 'ᵥ',
            ['x'] = 'ₓ',

            // ❌ Không tồn tại: b c d f g q w y z

            // Math symbols
            ['+'] = '₊',
            ['-'] = '₋',
            ['='] = '₌',
            ['('] = '₍',
            [')'] = '₎',

            // Space
            [' '] = ' '
        };
        private static readonly Dictionary<char, char> ReverseSubscriptMap = SubscriptMap.ToDictionary(pair => pair.Value, pair => pair.Key);

        public static string ConvertToFullWidth(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (c == ' ')
                    result.Append('\u3000');
                else if (c >= 33 && c <= 126)
                    result.Append((char)(c + 65248));
                else
                    result.Append(c);
            }
            return result.ToString();
        }
        public static string ConvertToHalfWidth(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (c == '\u3000')
                    result.Append(' ');
                else if (c >= 65281 && c <= 65374)
                    result.Append((char)(c - 65248));
                else
                    result.Append(c);
            }
            return result.ToString();
        }

        public static string ConvertToSuperscript(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var result = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (SuperscriptMap.TryGetValue(c, out char superChar))
                {
                    result.Append(superChar);
                }
                else result.Append(c);
            }
            return result.ToString();
        }
        public static string ConvertFromSuperscript(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var result = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (ReverseSuperscriptMap.TryGetValue(c, out char normalChar))
                {
                    result.Append(normalChar);
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        public static string ConvertToSubscript(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var result = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (SubscriptMap.TryGetValue(c, out char subChar))
                {
                    result.Append(subChar);
                }
                else result.Append(c);
            }
            return result.ToString();
        }
        public static string ConvertFromSubscript(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var result = new StringBuilder(input.Length);
            foreach (char c in input)
            {
                if (ReverseSubscriptMap.TryGetValue(c, out char normalChar))
                {
                    result.Append(normalChar);
                }
                else result.Append(c);
            }
            return result.ToString();
        }
    }
}
