using System;
using System.Text.Json;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Helpers;

namespace CardEditor.Services.ClipboardData
{
    public static class CopyCardService
    {
        public static async Task<SetClipboardResult> CopyCardToJSON(List<CardEditor.Models.Card> cardList, bool hasFlag)
        {
            SetClipboardResult result = new();
            try
            {
                if (cardList == null || cardList.Count == 0)
                {
                    result.Result = false;
                    result.Messenger = "Card list is empty.";
                    return result;
                }

                JSONCeds jsonCeds = new JSONCeds
                {
                    CardList = cardList,
                    CardOmegaList = new(),
                    CardBanlistList = new(),
                    Format = hasFlag ? CardListFormat.YGOHasFlag : CardListFormat.YGONoFlag,
                };

                string json = await Task.Run(() => JsonSerializer.Serialize(jsonCeds, JsonStringHelper.SerializerCEDSOptions));

                Clipboard.SetText(json);

                result.Result = true;
                result.Messenger = string.Empty;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static async Task<SetClipboardResult> CopyCardOmegaToJSON(List<CardEditor.Models.CardOmega> cardOmegaList, bool hasFlag)
        {
            SetClipboardResult result = new();
            try
            {
                if (cardOmegaList == null || cardOmegaList.Count == 0)
                {
                    result.Result = false;
                    result.Messenger = "Card list is empty.";
                    return result;
                }

                JSONCeds jsonCeds = new JSONCeds
                {
                    CardList = new(),
                    CardOmegaList = cardOmegaList,
                    CardBanlistList = new(),
                    Format = CardListFormat.OMEGA,
                };

                string json = await Task.Run(() => JsonSerializer.Serialize(jsonCeds, JsonStringHelper.SerializerCEDSOptions));

                Clipboard.SetText(json);

                result.Result = true;
                result.Messenger = string.Empty;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static async Task<SetClipboardResult> CopyCardBanlistToJSON(List<CardEditor.Models.CardBanList> cardBanlistList)
        {
            SetClipboardResult result = new();
            try
            {
                if (cardBanlistList == null || cardBanlistList.Count == 0)
                {
                    result.Result = false;
                    result.Messenger = "Card list is empty.";
                    return result;
                }

                JSONCeds jsonCeds = new JSONCeds
                {
                    CardList = new(),
                    CardOmegaList = new(),
                    CardBanlistList = cardBanlistList,
                    Format = CardListFormat.BanList,
                };

                string json = await Task.Run(() => JsonSerializer.Serialize(jsonCeds, JsonStringHelper.SerializerCEDSOptions));

                Clipboard.SetText(json);

                result.Result = true;
                result.Messenger = string.Empty;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
    }
}
