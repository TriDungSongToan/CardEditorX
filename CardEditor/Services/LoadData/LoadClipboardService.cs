using System;
using System.Text.Json;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Services.CheckData;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.LoadData
{
    public class LoadClipboardService
    {
        public static async Task<LoadCardDataResult> LoadClipboardCard()
        {
            var result = new LoadCardDataResult();
            string clipboardText = Clipboard.GetText();

            if (string.IsNullOrWhiteSpace(clipboardText))
            {
                result.Result = false;
                result.Message = CMess.noCardFound.ToText();
                return result;
            }

            try
            {
                CheckCardListResult checkResult = CheckCeds.CheckJSONClipboardValidity(clipboardText);

                if (!checkResult.Result)
                {
                    result.Result = false;
                    result.Message = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText());
                    return result;
                }

                result.Format = checkResult.Format;
                result.HasFlag = checkResult.HasFlag;
                result.Result = checkResult.Format switch
                {
                    CardListFormat.YGONoFlag => LoadYGOClipboardCard(clipboardText, result),
                    CardListFormat.YGOHasFlag => LoadYGOClipboardCard(clipboardText, result),
                    CardListFormat.OMEGA => LoadOMEGAClipboardCard(clipboardText, result),
                    _ => LogHelper.LogUnsupported(result, checkResult.Format)
                };

                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;
                return result;
            }
        }
        private static bool LoadYGOClipboardCard(string clipboardText, LoadCardDataResult result)
        {
            try
            {
                result.CardList = JsonSerializer.Deserialize<List<Card>>(clipboardText);

                return result.CardList != null;
            }
            catch (JsonException ex)
            {
                result.Message = ex.Message;
                return false;
            }
        }
        private static bool LoadOMEGAClipboardCard(string clipboardText, LoadCardDataResult result)
        {
            try
            {
                result.OmegaCardList = JsonSerializer.Deserialize<List<CardOmega>>(clipboardText);

                return result.OmegaCardList != null;
            }
            catch (JsonException ex)
            {
                result.Message = ex.Message;
                return false;
            }
        }
    }
}
