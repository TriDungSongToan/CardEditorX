using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Helpers
{
    public static class YDKEURLHelper32
    {
        public static (bool result, List<uint> MainDeck, List<uint> ExtraDeck, List<uint> SideDeck, string message)Base64ToPasscodes(string input)
        {
            List<uint> MainDeck = new List<uint>();
            List<uint> ExtraDeck = new List<uint>();
            List<uint> SideDeck = new List<uint>();

            if (string.IsNullOrWhiteSpace(input))
                return (true, MainDeck, ExtraDeck, SideDeck, string.Empty);

            try
            {
                if (input.StartsWith("ydke://", StringComparison.OrdinalIgnoreCase))
                    input = input.Substring(7);

                var parts = input.Split('!');
                if (parts.Length < 4)
                {
                    return (false, null, null, null, "Invalid YDKE format. Expected 3 components separated by '!'");
                }
                MainDeck = DecodeBase64ToPasscodes(parts[0]);
                ExtraDeck = DecodeBase64ToPasscodes(parts[1]);
                SideDeck = DecodeBase64ToPasscodes(parts[2]);

                return (true, MainDeck, ExtraDeck, SideDeck, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, null, null, null, ex.Message);
            }
        }
        private static List<uint> DecodeBase64ToPasscodes(string base64)
        {
            List<uint> result = new();
            if (string.IsNullOrWhiteSpace(base64)) return result;

            byte[] data;
            try
            {
                data = Convert.FromBase64String(base64);

                if (data.Length % 4 != 0)
                    throw new ArgumentException("Base64-decoded byte length is not a multiple of 4.");

                for (int i = 0; i < data.Length; i += 4)
                {
                    uint value =
                          (uint)data[i]
                        | (uint)data[i + 1] << 8
                        | (uint)data[i + 2] << 16
                        | (uint)data[i + 3] << 24;

                    result.Add(value);
                }
                return result;
            }
            catch (FormatException ex)
            {
                throw new ArgumentException($"Invalid base64 string: {ex.Message}", ex);
            }
        }

        public static (bool result, string message) PasscodesToBase64(List<ulong> Main64, List<ulong> Extra64, List<ulong> Side64)
        {
            try
            {
                List<uint> main32 = Convert64To32(Main64);
                List<uint> extra32 = Convert64To32(Extra64);
                List<uint> side32 = Convert64To32(Side64);

                string mainBase64 = PasscodesToBase64(main32, extra32, side32);
                return (true, mainBase64);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        private static List<uint> Convert64To32(List<ulong> list)
        {
            List<uint> result = new();

            if (list == null)
                return result;

            foreach (ulong v in list)
            {
                uint lower32 = (uint)(v & 0xFFFFFFFF); // lấy bit 0–31
                result.Add(lower32);
            }

            return result;
        }

        public static string PasscodesToBase64(List<uint> MainDeck, List<uint> ExtraDeck, List<uint> SideDeck)
        {
            if (MainDeck == null) MainDeck = new List<uint>();
            if (ExtraDeck == null) ExtraDeck = new List<uint>();
            if (SideDeck == null) SideDeck = new List<uint>();

            string mainBase64 = EncodePasscodesToBase64(MainDeck);
            string extraBase64 = EncodePasscodesToBase64(ExtraDeck);
            string sideBase64 = EncodePasscodesToBase64(SideDeck);

            return $"ydke://{mainBase64}!{extraBase64}!{sideBase64}!";
        }
        private static string EncodePasscodesToBase64(List<uint> deck)
        {
            if (deck == null || deck.Count == 0) return string.Empty;

            byte[] bytes = new byte[deck.Count * 4];

            for (int i = 0; i < deck.Count; i++)
            {
                uint v = deck[i];
                bytes[i * 4] = (byte)(v & 0xFF);
                bytes[i * 4 + 1] = (byte)((v >> 8) & 0xFF);
                bytes[i * 4 + 2] = (byte)((v >> 16) & 0xFF);
                bytes[i * 4 + 3] = (byte)((v >> 24) & 0xFF);
            }

            return Convert.ToBase64String(bytes);
        }
    }
    public static class YDKEURLHelper64
    {
        public static (bool result, List<ulong> MainDeck, List<ulong> ExtraDeck, List<ulong> SideDeck, string message) Base64ToPasscodes(string input)
        {
            List<ulong> MainDeck = new List<ulong>();
            List<ulong> ExtraDeck = new List<ulong>();
            List<ulong> SideDeck = new List<ulong>();

            if (string.IsNullOrWhiteSpace(input))
                return (true, MainDeck, ExtraDeck, SideDeck, string.Empty);

            try
            {
                if (input.StartsWith("ydke://", StringComparison.OrdinalIgnoreCase))
                    input = input.Substring(7);

                var parts = input.Split('!');
                if (parts.Length < 4)
                {
                    return (false, null, null, null, "Invalid YDKE format. Expected 3 components separated by '!'");
                }
                MainDeck = DecodeBase64ToPasscodes(parts[0]);
                ExtraDeck = DecodeBase64ToPasscodes(parts[1]);
                SideDeck = DecodeBase64ToPasscodes(parts[2]);

                return (true, MainDeck, ExtraDeck, SideDeck, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, null, null, null, ex.Message);
            }
        }
        private static List<ulong> DecodeBase64ToPasscodes(string base64)
        {
            List<ulong> result = new();
            if (string.IsNullOrWhiteSpace(base64)) return result;

            byte[] data;
            try
            {
                data = Convert.FromBase64String(base64);

                if (data.Length % 8 != 0)
                    throw new ArgumentException("Base64-decoded byte length is not a multiple of 8.");

                for (int i = 0; i < data.Length; i += 8)
                {
                    ulong value =
                          (ulong)data[i]
                        | (ulong)data[i + 1] << 8
                        | (ulong)data[i + 2] << 16
                        | (ulong)data[i + 3] << 24
                        | (ulong)data[i + 4] << 32
                        | (ulong)data[i + 5] << 40
                        | (ulong)data[i + 6] << 48
                        | (ulong)data[i + 7] << 56;
                    result.Add(value);
                }
                return result;
            }
            catch (FormatException ex)
            {
                throw new ArgumentException($"Invalid base64 string: {ex.Message}", ex);
            }
        }
        public static (bool result, string message) PasscodesToBase64(List<ulong> MainDeck, List<ulong> ExtraDeck, List<ulong> SideDeck)
        {
            if (MainDeck == null) MainDeck = new List<ulong>();
            if (ExtraDeck == null) ExtraDeck = new List<ulong>();
            if (SideDeck == null) SideDeck = new List<ulong>();

            try
            {
                string mainBase64 = EncodePasscodesToBase64(MainDeck);
                string extraBase64 = EncodePasscodesToBase64(ExtraDeck);
                string sideBase64 = EncodePasscodesToBase64(SideDeck);

                return (true, $"ydke://{mainBase64}!{extraBase64}!{sideBase64}!64");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        private static string EncodePasscodesToBase64(List<ulong> deck)
        {
            if (deck == null || deck.Count == 0) return string.Empty;

            byte[] bytes = new byte[deck.Count * 8];

            for (int i = 0; i < deck.Count; i++)
            {
                ulong v = deck[i];
                bytes[i * 8] = (byte)(v & 0xFF);
                bytes[i * 8 + 1] = (byte)((v >> 8) & 0xFF);
                bytes[i * 8 + 2] = (byte)((v >> 16) & 0xFF);
                bytes[i * 8 + 3] = (byte)((v >> 24) & 0xFF);
                bytes[i * 8 + 4] = (byte)((v >> 32) & 0xFF);
                bytes[i * 8 + 5] = (byte)((v >> 40) & 0xFF);
                bytes[i * 8 + 6] = (byte)((v >> 48) & 0xFF);
                bytes[i * 8 + 7] = (byte)((v >> 56) & 0xFF);
            }
            return Convert.ToBase64String(bytes);
        }
    }
}
