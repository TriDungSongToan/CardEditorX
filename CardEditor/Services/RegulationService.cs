using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using CardEditor.Models;
using CardEditor.Helpers;
using CardEditor.Interfaces;
using CardEditor.ViewModels;

namespace CardEditor.Services
{
    public class RegulationService : IRegulationInterface
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public async Task<(bool Success, string Error, YamiYugiRegulation Data)> LoadRegulation(string url,
            CancellationToken cancellationToken = default)
        {
            // 1. Đảm bảo KonamiID map đã load xong
            var (resultKonamiID, messageKonamiID) = await KonamiIDViewModel.Instance.LoadKonamiID();
            if (!resultKonamiID) return (false, messageKonamiID, null);

            // 2. Gọi API và parse JSON
            var (resultURL, errorURL, regulationURL) = await LoadFromUrl(url, cancellationToken);
            if (!resultURL) return (false, errorURL, null);

            // 3. Map CardID cho từng CardLimit
            var (resultMapID, errorMapID, regulationMapID) = await MappingCardID(regulationURL);
            if (!resultMapID || regulationMapID == null) { return (false, errorMapID, null); }

            // 4. Map CardID -> CardName
            var (resultMapName, errorMapName, regulationMapName) = await MappingCardName(regulationMapID);
            if (!resultMapName || regulationMapName == null) { return (false, errorMapName, null); }

            return (true, string.Empty, regulationMapName);
        }

        private async Task<(bool Success, string Error, YamiYugiRegulation Data)> LoadFromUrl(string url,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                return (false, "Empty URL.", null);

            string json;
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return (false, $"The API returned an error:{(int)response.StatusCode} {response.ReasonPhrase}", null);

                json = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return (false, "API call timed out.", null);
            }
            catch (TaskCanceledException)
            {
                return (false, "The request has been cancelled.", null);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Connection error: {ex.Message}", null);
            }

            try
            {
                var regulation = RegulationParser.Parse(json);
                return (true, string.Empty, regulation);
            }
            catch (JsonException ex)
            {
                return (false, $"Invalid JSON: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Map CardID cho từng CardLimit
        /// </summary>
        /// <param name="regulation"></param>
        /// <returns></returns>
        private async Task<(bool Success, string Error, YamiYugiRegulation Data)> MappingCardID(YamiYugiRegulation regulation)
        {
            if (regulation == null) return (false, "Regulation must not be null.", null);

            var resultList = new List<CardLimit>();

            foreach (var itemRegula in regulation.Regulation)
            {
                if (KonamiIDViewModel.Instance.ReverseOfficialMapID.TryGetValue(itemRegula.KonamiID, out var cardIds)
                    && cardIds.Count > 0)
                {
                    foreach (var id in cardIds)
                    {
                        resultList.Add(new CardLimit
                        {
                            KonamiID = itemRegula.KonamiID,
                            LimitedCount = itemRegula.LimitedCount,
                            CardID = id
                        });
                    }
                }
                else
                {
                    // Không tìm thấy CardID nào cho KonamiID này -> vẫn giữ lại, CardID = 0
                    resultList.Add(new CardLimit
                    {
                        KonamiID = itemRegula.KonamiID,
                        LimitedCount = itemRegula.LimitedCount,
                        CardID = 0
                    });
                }
            }

            regulation.Regulation = resultList;
            return (true, string.Empty, regulation);
        }

        /// <summary>
        /// Map CardName cho từng CardLimit
        /// </summary>
        /// <param name="regulation"></param>
        /// <returns></returns>
        private async Task<(bool Success, string Error, YamiYugiRegulation Data)> MappingCardName(YamiYugiRegulation regulation)
        {
            if (regulation == null) return (false, "Regulation must not be null.", null);

            try
            {
                var (resultLoadEX, messageLoadEX) =  await CardEXDataViewModel.Instance.LoadCardsEXAsync();
                if (!resultLoadEX) return (true, messageLoadEX, regulation);

                foreach (var itemRegula in regulation.Regulation)
                {
                    if (itemRegula.CardID == 0) continue;
                    var card = CardEXDataViewModel.Instance.TryGetCard(itemRegula.CardID);
                    if (card == null) continue;

                    itemRegula.CardName = card.BaseCard.name;
                }

                return (true, string.Empty, regulation);
            }
            catch (Exception ex)
            {
                return (true, ex.Message, regulation);
            }
        }

        public async Task<(bool Success, string Error, string filePath)> CreateBanListRegulation(
            YamiYugiRegulation regulation, string banlistName, bool whiteList, string filePath)
        {
            Dictionary<ulong, CardBanList> cardList = new();

            try
            {
                foreach (var card in regulation.Regulation)
                {
                    if (card == null || card.CardID == 0) continue;
                    CardBanList cardBan = new CardBanList
                    {
                        Id = card.CardID,
                        Name = card.CardName,
                        LimitedCount = card.LimitedCount,
                    };
                    cardList[card.CardID] = cardBan;
                }

                BanList banList = new BanList
                {
                    Name = banlistName,
                    FileName = System.IO.Path.GetFileName(filePath),
                    FilePath = filePath,
                    CardList = cardList,
                    WhiteList = whiteList,
                };

                var (resultSave, messageSasve) = await BanListRawDataViewModel.Instance.SaveBanListFile(banList);
                if (resultSave) return (true, string.Empty, messageSasve);
                else return (false, messageSasve, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
    }
}
