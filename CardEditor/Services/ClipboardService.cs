using System;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CardEditor.Services
{
    public static class ClipboardService
    {
        private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();
        private static JsonSerializerOptions CreateSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
        }

        public static async Task<(bool, string)> CopyCardJSON(List<CardEditor.Models.Card> cardList)
        {
            try
            {
                if (cardList == null || cardList.Count == 0) return ( false, "Card list is empty.");

                string json = JsonSerializer.Serialize(cardList, SerializerOptions);

                Clipboard.SetText(json);

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static (bool, string) CopyTextToClipboard(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(text)) return (false, "Text is empty.");
                Clipboard.SetText(text);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
