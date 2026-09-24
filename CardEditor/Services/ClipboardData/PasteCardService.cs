using System;
using System.Text.Json;
using System.Windows;
using System.Threading.Tasks;
using CardEditor.Models;
using CardEditor.Helpers;

namespace CardEditor.Services.ClipboardData
{
    public class PasteCardService
    {
        public static async Task<LoadClipboardResult> LoadFromClipboard()
        {
            LoadClipboardResult result = new();

            try
            {
                if (!Clipboard.ContainsText())
                {
                    result.Result = false;
                    result.Message = "Clipboard does not contain text.";
                    return result;
                }

                string json = Clipboard.GetText();

                if (string.IsNullOrWhiteSpace(json))
                {
                    result.Result = false;
                    result.Message = "Clipboard text is empty.";
                    return result;
                }

                JSONCeds? cards = await Task.Run(() => JsonSerializer.Deserialize<JSONCeds>(json, JsonStringHelper.SerializerCEDSOptions));

                if (cards == null)
                {
                    result.Result = false;
                    result.Message = "Invalid JSON data.";
                    return result;
                }

                result.Result = true;
                result.Cards = cards;
                result.Message = string.Empty;
            }
            catch (JsonException ex)
            {
                result.Result = false;
                result.Message = $"Invalid JSON format: {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
