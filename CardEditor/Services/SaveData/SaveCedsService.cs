using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.SaveData
{
    public class SaveCedsYGOService
    {
        private static async Task<LoadJSONCedsResult> LoadJsonInternal(string filePath)
        {
            LoadJSONCedsResult result = new();
            if (!File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();
                return result;  // File chưa tồn tại -> coi như danh sách rỗng
            }
            try
            {
                var cards = await Task.Run(() =>
                {
                    string json = File.ReadAllText(filePath, Encoding.UTF8);
                    var cards = JsonSerializer.Deserialize<JSONCeds>(json, JsonStringHelper.SerializerCEDSOptions) ?? new JSONCeds();

                    cards.CardList ??= new();
                    cards.CardOmegaList ??= new();
                    cards.CardBanlistList ??= new();

                    return cards;
                });

                result.Result = true;
                result.Cards = cards;
                result.Message = string.Empty;
                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;
                return result;
            }
        }
        private static async Task<WriteResult> WriteJsonInternal(List<Card> cards, string filePath, bool hasFlag)
        {
            WriteResult result = new();

            string tempPath = filePath + ".tmp";
            try
            {
                JSONCeds jsonCeds = new JSONCeds
                {
                    CardList = cards,
                    CardOmegaList = new(),
                    CardBanlistList = new(),
                    Format = hasFlag ? CardListFormat.YGOHasFlag : CardListFormat.YGONoFlag,
                };

                string json = await Task.Run(() => JsonSerializer.Serialize(jsonCeds, JsonStringHelper.SerializerCEDSOptions));
                await Task.Run(() =>
                {
                    File.WriteAllText(tempPath, json, Encoding.UTF8);
                    File.Copy(tempPath, filePath, overwrite: true); // hoặc File.Move nếu không cần giữ file gốc lúc lỗi
                });
                result.Result = true;
                result.Messenger = string.Empty;

                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
                return result;
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        public static async Task<WriteResult> AddNewCard(Card newCard, string filePath, bool hasFlag)
        {
            WriteResult result = new();
            if (newCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }
            LoadJSONCedsResult resultLoad = await LoadJsonInternal(filePath);
            if (!resultLoad.Result)
            {
                result.Result = false;
                result.Messenger = resultLoad.Message;
                return result;
            }
            if (resultLoad.Cards.CardList.Any(c => c.id == newCard.id))
            {
                result.Result = false;
                result.Messenger = CMess.cardIDExist.ToText();
                return result;
            }

            resultLoad.Cards.CardList.Add(newCard);

            return await WriteJsonInternal(resultLoad.Cards.CardList, filePath, hasFlag);
        }
        public static async Task<WriteResult> ModifyCurrentCard(Card currentCard, string filePath, bool hasFlag)
        {
            WriteResult result = new();
            if (currentCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }

            LoadJSONCedsResult resultLoad = await LoadJsonInternal(filePath);
            if (!resultLoad.Result)
            {
                result.Result = false;
                result.Messenger = resultLoad.Message;
                return result;
            }

            int index = resultLoad.Cards.CardList.FindIndex(c => c.id == currentCard.id);
            if (index == -1)
            {
                result.Result = false;
                result.Messenger = CMess.cardIDNotExist.ToText();
                return result;
            }

            resultLoad.Cards.CardList[index] = currentCard;
            return await WriteJsonInternal(resultLoad.Cards.CardList, filePath, hasFlag);
        }
        public static async Task<WriteResult> DeleteCard(ulong cardId, string filePath, bool hasFlag)
        {
            WriteResult result = new();

            LoadJSONCedsResult resultLoad = await LoadJsonInternal(filePath);
            if (!resultLoad.Result)
            {
                result.Result = false;
                result.Messenger = resultLoad.Message;
                return result;
            }

            int index = resultLoad.Cards.CardList.FindIndex(c => c.id == cardId);

            if (index == -1)
            {
                result.Result = false;
                result.Messenger = CMess.cardIDNotExist.ToText();
                return result;
            }
            resultLoad.Cards.CardList.RemoveAt(index);

            return await WriteJsonInternal(resultLoad.Cards.CardList, filePath, hasFlag);
        }

        public static async Task<WriteResult> SaveCardList(List<Card> cardList, string filePath, bool hasFlag)
        {
            if (cardList == null)
            {
                return new WriteResult
                {
                    Result = false,
                    Messenger = "Card list is null"
                };
            }
            return await WriteJsonInternal(cardList, filePath, hasFlag);
        }
    }

    public class SaveCedsOMEGAService
    {
        private static async Task<LoadJSONCedsResult> LoadJsonInternal(string filePath)
        {
            LoadJSONCedsResult result = new();
            if (!File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();
                return result;  // File chưa tồn tại -> coi như danh sách rỗng
            }
            try
            {
                var cardsOmega = await Task.Run(() =>
                {
                    string json = File.ReadAllText(filePath, Encoding.UTF8);
                    var cardsOmega = JsonSerializer.Deserialize<JSONCeds>(json, JsonStringHelper.SerializerCEDSOptions) ?? new JSONCeds();

                    cardsOmega.CardList ??= new();
                    cardsOmega.CardOmegaList ??= new();
                    cardsOmega.CardBanlistList ??= new();

                    return cardsOmega;
                });

                result.Result = true;
                result.Cards = cardsOmega;
                result.Message = string.Empty;
                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;
                return result;
            }
        }
        private static async Task<WriteResult> WriteJsonInternal(List<CardOmega> cardsOmega, string filePath)
        {
            WriteResult result = new();

            string tempPath = filePath + ".tmp";
            try
            {
                JSONCeds jsonCeds = new JSONCeds
                {
                    CardList = new(),
                    CardOmegaList = cardsOmega,
                    CardBanlistList = new(),
                    Format = CardListFormat.OMEGA,
                };

                string json = await Task.Run(() => JsonSerializer.Serialize(jsonCeds, JsonStringHelper.SerializerCEDSOptions));
                await Task.Run(() =>
                {
                    File.WriteAllText(tempPath, json, Encoding.UTF8);
                    File.Copy(tempPath, filePath, overwrite: true); // hoặc File.Move nếu không cần giữ file gốc lúc lỗi
                });
                result.Result = true;
                result.Messenger = string.Empty;

                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
                return result;
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        public static async Task<WriteResult> AddNewCard(CardOmega newCard, string filePath, bool hasFlag)
        {
            WriteResult result = new();
            if (newCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }
            LoadJSONCedsResult resultLoad = await LoadJsonInternal(filePath);
            if (!resultLoad.Result)
            {
                result.Result = false;
                result.Messenger = resultLoad.Message;
                return result;
            }
            if (resultLoad.Cards.CardOmegaList.Any(c => c.id == newCard.id))
            {
                result.Result = false;
                result.Messenger = CMess.cardIDExist.ToText();
                return result;
            }

            resultLoad.Cards.CardOmegaList.Add(newCard);

            return await WriteJsonInternal(resultLoad.Cards.CardOmegaList, filePath);
        }
        public static async Task<WriteResult> ModifyCurrentCard(CardOmega currentCard, string filePath, bool hasFlag)
        {
            WriteResult result = new();
            if (currentCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }

            LoadJSONCedsResult resultLoad = await LoadJsonInternal(filePath);
            if (!resultLoad.Result)
            {
                result.Result = false;
                result.Messenger = resultLoad.Message;
                return result;
            }

            int index = resultLoad.Cards.CardOmegaList.FindIndex(c => c.id == currentCard.id);
            if (index == -1)
            {
                result.Result = false;
                result.Messenger = CMess.cardIDNotExist.ToText();
                return result;
            }

            resultLoad.Cards.CardOmegaList[index] = currentCard;
            return await WriteJsonInternal(resultLoad.Cards.CardOmegaList, filePath);
        }
        public static async Task<WriteResult> DeleteCard(ulong cardId, string filePath, bool hasFlag)
        {
            WriteResult result = new();

            LoadJSONCedsResult resultLoad = await LoadJsonInternal(filePath);
            if (!resultLoad.Result)
            {
                result.Result = false;
                result.Messenger = resultLoad.Message;
                return result;
            }

            int index = resultLoad.Cards.CardOmegaList.FindIndex(c => c.id == cardId);

            if (index == -1)
            {
                result.Result = false;
                result.Messenger = CMess.cardIDNotExist.ToText();
                return result;
            }
            resultLoad.Cards.CardOmegaList.RemoveAt(index);

            return await WriteJsonInternal(resultLoad.Cards.CardOmegaList, filePath);
        }

        public static async Task<WriteResult> SaveCardList(List<CardOmega> cardOmegaList, string filePath, bool hasFlag)
        {
            if (cardOmegaList == null)
            {
                return new WriteResult
                {
                    Result = false,
                    Messenger = "Card list is null"
                };
            }
            return await WriteJsonInternal(cardOmegaList, filePath);
        }
    }

    public class SaveCedsBanlistService
    {
        private static async Task<LoadJSONCedsResult> LoadJsonInternal(string filePath)
        {
            LoadJSONCedsResult result = new();
            if (!File.Exists(filePath))
            {
                result.Result = false;
                result.Message = CMess.fileNotExit.ToText();
                return result;  // File chưa tồn tại -> coi như danh sách rỗng
            }
            try
            {
                var cardsBanlist = await Task.Run(() =>
                {
                    string json = File.ReadAllText(filePath, Encoding.UTF8);
                    var cardsBanlist = JsonSerializer.Deserialize<JSONCeds>(json, JsonStringHelper.SerializerCEDSOptions) ?? new JSONCeds();
                    cardsBanlist.CardList ??= new();
                    cardsBanlist.CardOmegaList ??= new();
                    cardsBanlist.CardBanlistList ??= new();

                    return cardsBanlist;
                });

                result.Result = true;
                result.Cards = cardsBanlist;
                result.Message = string.Empty;
                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Message = ex.Message;
                return result;
            }
        }
        private static async Task<WriteResult> WriteJsonInternal(List<CardBanList> cardsBanlist, string filePath)
        {
            WriteResult result = new();

            string tempPath = filePath + ".tmp";
            try
            {
                JSONCeds jsonCeds = new JSONCeds
                {
                    CardList = new(),
                    CardOmegaList = new(),
                    CardBanlistList = cardsBanlist,
                    Format = CardListFormat.BanList,
                };

                string json = await Task.Run(() => JsonSerializer.Serialize(jsonCeds, JsonStringHelper.SerializerCEDSOptions));
                await Task.Run(() =>
                {
                    File.WriteAllText(tempPath, json, Encoding.UTF8);
                    File.Copy(tempPath, filePath, overwrite: true); // hoặc File.Move nếu không cần giữ file gốc lúc lỗi
                });
                result.Result = true;
                result.Messenger = string.Empty;

                return result;
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
                return result;
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        public static async Task<WriteResult> AddNewCard(CardBanList newCard, string filePath)
        {
            WriteResult result = new();
            if (newCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }
            LoadJSONCedsResult resultLoad = await LoadJsonInternal(filePath);
            if (!resultLoad.Result)
            {
                result.Result = false;
                result.Messenger = resultLoad.Message;
                return result;
            }
            if (resultLoad.Cards.CardBanlistList.Any(c => c.Id == newCard.Id))
            {
                result.Result = false;
                result.Messenger = CMess.cardIDExist.ToText();
                return result;
            }

            resultLoad.Cards.CardBanlistList.Add(newCard);

            return await WriteJsonInternal(resultLoad.Cards.CardBanlistList, filePath);
        }
        public static async Task<WriteResult> ModifyCurrentCard(CardBanList currentCard, string filePath)
        {
            WriteResult result = new();
            if (currentCard == null)
            {
                result.Result = false;
                result.Messenger = "Card is null";
                return result;
            }

            LoadJSONCedsResult resultLoad = await LoadJsonInternal(filePath);
            if (!resultLoad.Result)
            {
                result.Result = false;
                result.Messenger = resultLoad.Message;
                return result;
            }

            int index = resultLoad.Cards.CardBanlistList.FindIndex(c => c.Id == currentCard.Id);
            if (index == -1)
            {
                result.Result = false;
                result.Messenger = CMess.cardIDNotExist.ToText();
                return result;
            }

            resultLoad.Cards.CardBanlistList[index] = currentCard;
            return await WriteJsonInternal(resultLoad.Cards.CardBanlistList, filePath);
        }
        public static async Task<WriteResult> DeleteCard(ulong cardId, string filePath)
        {
            WriteResult result = new();

            LoadJSONCedsResult resultLoad = await LoadJsonInternal(filePath);
            if (!resultLoad.Result)
            {
                result.Result = false;
                result.Messenger = resultLoad.Message;
                return result;
            }

            int index = resultLoad.Cards.CardBanlistList.FindIndex(c => c.Id == cardId);

            if (index == -1)
            {
                result.Result = false;
                result.Messenger = CMess.cardIDNotExist.ToText();
                return result;
            }
            resultLoad.Cards.CardBanlistList.RemoveAt(index);

            return await WriteJsonInternal(resultLoad.Cards.CardBanlistList, filePath);
        }

        public static async Task<WriteResult> SaveCardList(List<CardBanList> cardBanlistList, string filePath)
        {
            if (cardBanlistList == null)
            {
                return new WriteResult
                {
                    Result = false,
                    Messenger = "Card list is null"
                };
            }
            return await WriteJsonInternal(cardBanlistList, filePath);
        }
    }

}
