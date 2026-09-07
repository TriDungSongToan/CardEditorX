using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using CardEditor.Helpers;
using CardEditor.Converter;
using CardEditor.Localization;
using CardEditor.Models.MasterDuel;

using CMess = CardEditor.Localization.Language;

namespace CardEditor.ViewModels
{
    public class MasterDuelAPIViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly Lazy<MasterDuelAPIViewModel> _instance = new Lazy<MasterDuelAPIViewModel>(() => new MasterDuelAPIViewModel());
        public static MasterDuelAPIViewModel Instance => _instance.Value;

        private List<CardMasterDuel> _cardList = new();
        public IReadOnlyList<CardMasterDuel> CardList => _cardList;

        private Dictionary<ulong, CardEditor.Enums.MasterDuel.Rarity> _mapRarity = new();
        public Dictionary<ulong, CardEditor.Enums.MasterDuel.Rarity> MapRarity => _mapRarity;



        public bool isLoaded = false;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            Converters =
            {
                new EnumMemberJsonConverter<CardEditor.Enums.MasterDuel.CardType>(),
                new EnumMemberJsonConverter<CardEditor.Enums.MasterDuel.Rarity>(),
                new EnumMemberJsonConverter<CardEditor.Enums.MasterDuel.Attribute>(),
                new EnumMemberJsonConverter<CardEditor.Enums.MasterDuel.LinkArrow>(),
                new EnumMemberJsonConverter<CardEditor.Enums.MasterDuel.MonsterType>(),
                new EnumMemberJsonConverter<CardEditor.Enums.MasterDuel.Race>()
            }
        };
        private static readonly JsonSerializerOptions RarityJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        public MasterDuelAPIViewModel()
        {

        }

        public async Task<(bool, string)> LoadJSON(string filePath)
        {
            try
            {
                CardCache cache = await LoadCardCacheAsync(filePath);

                _cardList = cache.Cards;
                OnPropertyChanged(nameof(CardList));

                isLoaded = true;

                return (true, $"Loaded {_cardList.Count} cards.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<CardCache> LoadCardCacheAsync(string filePath)
        {
            using var stream = System.IO.File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<CardCache>(stream, JsonOptions) ?? new CardCache();
        }
        public (bool, string) BuildRarityMap()
        {
            try
            {
                _mapRarity = _cardList.Where(
                    card => !string.IsNullOrWhiteSpace(card.KonamiId) &&
                    ulong.TryParse(card.KonamiId, out _) &&
                    card.Rarity.HasValue)

                    .GroupBy(card => ulong.Parse(card.KonamiId))
                    .ToDictionary(
                    group => group.Key,
                    group => group.First().Rarity!.Value);

                OnPropertyChanged(nameof(MapRarity));
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<(bool, string)> SaveRarityMap(string filePath)
        {
            try
            {
                var cache = new RarityCache
                {
                    LastFetched = DateTime.Now,
                    CardCount = _cardList.Count,
                    RarityCount = _mapRarity.Values.Distinct().Count(),
                    CardRarityCount = _mapRarity.Count,
                    CardsRarity = _mapRarity

                    .Select(x => new RarityMasterDuel
                    {
                        KonamiId = x.Key,
                        Rarity = x.Value
                    }).ToList()
                };

                var options = new JsonSerializerOptions(JsonOptions)
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(cache, options);

                using (var writer = new StreamWriter(filePath, false))
                {
                    await writer.WriteAsync(json);
                }

                return (true, $"Saved {_mapRarity.Count} rarity records successfully!");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public async Task<(bool result, List<RarityMasterDuel> rarityList, string message)> LoadMDRarityJSON(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
                    return (false, null, CMess.fileNotExit.ToText());

                using (StreamReader reader = new StreamReader(filePath))
                {
                    string json = await reader.ReadToEndAsync();

                    RarityCache? cache = JsonSerializer.Deserialize<RarityCache>(json, RarityJsonOptions);

                    if (cache == null)
                        return (false, null, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText()));

                    return (true, cache.CardsRarity, string.Empty);
                }
            }
            catch (JsonException ex)
            {
                return (false, null,
                    $"{string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.File.ToText(), CMess.Format.ToText())} {ex.Message}");
            }
            catch (IOException ex)
            {
                return (false, null,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Read.ToText(), CMess.Json.ToText())} {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
        public async Task<(bool, string)> LoadMDRarityFile()
        {
            try
            {
                string filePath = FileDiaLogHelper.OpenJSON();

                if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath)) return (true, string.Empty);

                var loadResult = await LoadMDRarityJSON(filePath);

                if (!loadResult.result) return (false, loadResult.message);

                var rarityList = loadResult.rarityList;
                Debug.WriteLine($"Rarity count: {rarityList?.Count ?? 0}");
                if (rarityList == null || rarityList.Count == 0) return (false, "Rarity List Empty");

                _mapRarity.Clear();

                foreach (var item in rarityList)
                {
                    if (!item.Rarity.HasValue) continue;
                    _mapRarity[item.KonamiId] = item.Rarity.Value;
                }
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public void Dispose()
        {
            _cardList.Clear();
            _cardList = null;
            _mapRarity.Clear();
            _mapRarity = null;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
