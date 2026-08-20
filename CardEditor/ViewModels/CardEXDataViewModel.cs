using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Models;
using CardEditor.Services;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.ViewModels
{
    public class CardEXDataViewModel : IDisposable
    {
        private static readonly Lazy<CardEXDataViewModel> _instance = new Lazy<CardEXDataViewModel>(() => new CardEXDataViewModel());
        public static CardEXDataViewModel Instance => _instance.Value;

        public Dictionary<ulong, List<string>> CardPaths;
        public Dictionary<ulong, List<string>> ScriptPaths;
        private Dictionary<ulong, CardEX> _allCards;
        public IReadOnlyDictionary<ulong, CardEX> AllCards => _allCards ?? new Dictionary<ulong, CardEX>();

        private Dictionary<ulong, List<ulong>> _idToAlias;
        private Dictionary<ulong, ulong> _aliasToId;
        public IReadOnlyDictionary<ulong, List<ulong>> IdToAlias => _idToAlias ?? new Dictionary<ulong, List<ulong>>();
        public IReadOnlyDictionary<ulong, ulong> AliasToID => _aliasToId ?? new Dictionary<ulong, ulong>();

        public bool IsLoadedCard;
        public bool IsLoadedScript;

        private CardEXDataViewModel()
        {
            CardPaths = new Dictionary<ulong, List<string>>();
            ScriptPaths = new Dictionary<ulong, List<string>>();
            IsLoadedCard = false;
            IsLoadedScript = false;
        }

        public async Task LoadCardsEXAsync()
        {
            if (IsLoadedCard) return;

            await Task.Run(async () =>
            {
                var (allCards, allPaths, message) = await LoadDataServices.LoadAllCdbFiles();
                if (allCards == null)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                    return;
                }

                _idToAlias = new Dictionary<ulong, List<ulong>>();
                _aliasToId = new Dictionary<ulong, ulong>();
                foreach (var item in allCards)
                {
                    if (item.alias == 0) continue;

                    // ID -> Aliases
                    if (!_idToAlias.TryGetValue(item.id, out var aliases))
                    {
                        aliases = new List<ulong>();
                        _idToAlias[item.id] = aliases;
                    }

                    aliases.Add(item.alias);

                    // Alias -> ID
                    _aliasToId[item.alias] = item.id;
                }

                var rareDict = RareRawDataViewModel.Instance.RareCardsData;
                var genesysDict = GenesysRawDataViewModel.Instance.GenesysCardsData;

                // Merge data
                var cardEXList = allCards.Select(card => new CardEX
                {
                    BaseCard  = card,
                    Rare = rareDict.TryGetValue(card.id, out var rareCard) ? rareCard.rare : 0,
                    GPoint = genesysDict.TryGetValue(card.id, out var gCard) ? gCard.GPoints : 0
                });

                _allCards = cardEXList.ToDictionary(c => c.ID);
                CardPaths = allPaths;
            });
            IsLoadedCard = true;
        }
        public async Task LoadScriptAsync()
        {
            if (IsLoadedScript) return;

            await Task.Run(async () =>
            {
                var (loadedScripts, message) = await LoadDataServices.LoadAllScriptPaths();
                if (loadedScripts == null)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                    return;
                }
                ScriptPaths = loadedScripts;
            });
            IsLoadedScript = true;
        }

        public void AddScriptPath(ulong id, string filePath)
        {
            if (!ScriptPaths.TryGetValue(id, out List<string> paths))
            {
                paths = new List<string>();
                ScriptPaths[id] = paths;
            }

            paths.Add(filePath);
        }
        public bool TryGetCard(ulong id, out CardEX card)
        {
            if (_allCards != null)
            {
                return _allCards.TryGetValue(id, out card);
            }
            card = null;
            return false;
        }
        public IReadOnlyList<string> TryGetCardPath(ulong id)
        {
            if (CardPaths == null) return Array.Empty<string>();
            if (id == 0) return Array.Empty<string>();
            return CardPaths.TryGetValue(id, out var paths) ? paths : Array.Empty<string>();
        }
        public IReadOnlyList<string> TryGetScriptpath(ulong id)
        {
            if (ScriptPaths == null) return Array.Empty<string>();
            if (id == 0) return Array.Empty<string>();
            return ScriptPaths.TryGetValue(id, out var paths) ? paths : Array.Empty<string>();
        }
        public CardEX GetCardOrNull(ulong id)
        {
            if (_allCards != null && _allCards.TryGetValue(id, out var card))
                return card;
            return null;
        }
        public List<CardInstance> GetListCardInstance(List<ulong> ids)
        {
            var result = new List<CardInstance>(ids.Count);

            foreach (var id in ids)
            {
                if (AllCards.TryGetValue(id, out var card))
                {
                    result.Add(new CardInstance { Card = card });
                }
            }
            return result;
        }
        public async Task ReloadCardsAsync()
        {
            IsLoadedCard = false;
            _allCards = null;
            await LoadCardsEXAsync();
        }

        public void Dispose()
        {
            _allCards?.Clear();
            _allCards = null;
            CardPaths?.Clear();
            CardPaths = null;
            IsLoadedCard = false;
            IsLoadedScript = false;
        }
    }
}
