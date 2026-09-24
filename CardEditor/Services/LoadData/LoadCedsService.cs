using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.LoadData
{
    public class LoadCedsService
    {
        public static async Task<LoadCardDataResult> LoadCedsCard(string filePath)
        {
            LoadCardDataResult result = new();

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();
                return result;
            }

            try
            {
                JSONCeds cards = await Task.Run(() =>
                {
                    string json = File.ReadAllText(filePath, Encoding.UTF8);
                    var cards = JsonSerializer.Deserialize<JSONCeds>(json, JsonStringHelper.SerializerCEDSOptions) ?? new JSONCeds();

                    cards.CardList ??= new();
                    cards.CardOmegaList ??= new();
                    cards.CardBanlistList ??= new();

                    return cards;
                });

                if (cards == null)
                {
                    result.Result = false;
                    result.Message = "JSON data is null.";
                    return result;
                }

                //////////////

                result.CardList = cards.CardList;
                result.OmegaCardList = cards.CardOmegaList;
                result.CardBanlistList = cards.CardBanlistList;

                result.Format = cards.Format;
                result.HasFlag = cards.Format != CardListFormat.YGONoFlag;
                result.Message = string.Empty;
                result.Result = true;

                return result;
            }
            catch (JsonException ex)
            {
                result.Result = false;
                result.Message = ex.Message;
                return result;
            }
            catch (IOException ex)
            {
                result.Result = false;
                result.Message = ex.Message;
                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;
                return result;
            }
        }
    }
}
