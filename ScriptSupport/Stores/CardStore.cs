using ScriptSupport.Models;

namespace ScriptSupport.Stores
{
    public class CardStore : IDisposable
    {
        private Dictionary<ulong, CardData> _cardDatas = new();
        private Dictionary<ulong, IReadOnlyList<CardText>> _cardTexts = new();
        private Dictionary<ulong, IReadOnlyList<CardData>> _aliasIndex = new();
        private Dictionary<ulong, int> _genesysData = new();
        public IReadOnlyDictionary<ulong, CardData> CardDatas => _cardDatas;
        public IReadOnlyDictionary<ulong, IReadOnlyList<CardText>> CardTexts => _cardTexts;
        public IReadOnlyDictionary<ulong, IReadOnlyList<CardData>> AliasIndex => _aliasIndex;
        public IReadOnlyDictionary<ulong, int> GenesysData => _genesysData;

        public async Task SetCardDatas(Dictionary<ulong, CardData> cardDatas)
        {
            if (cardDatas == null) return;
            Interlocked.Exchange(ref _cardDatas, cardDatas);

            var aliasIndex = await Task.Run(() =>
            {
                var dict = new Dictionary<ulong, List<CardData>>();

                foreach (var card in cardDatas.Values)
                {
                    if (!dict.TryGetValue(card.alias, out var list))
                    {
                        list = new List<CardData>();
                        dict[card.alias] = list;
                    }
                    list.Add(card);
                }

                return dict.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlyList<CardData>)kvp.Value.AsReadOnly());
            });
            Interlocked.Exchange(ref _aliasIndex, aliasIndex);
        }
        public void SetCardTexts(Dictionary<ulong, IReadOnlyList<CardText>> cardTexts)
        {
            if (cardTexts == null) return;
            Interlocked.Exchange(ref _cardTexts, cardTexts);
        }
        public void SetGenesysData(Dictionary<ulong, int> cardGenesys)
        {
            if (cardGenesys == null) return;
            Interlocked.Exchange(ref _genesysData, cardGenesys);
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _cardDatas, new Dictionary<ulong, CardData>());
            Interlocked.Exchange(ref _cardTexts, new Dictionary<ulong, IReadOnlyList<CardText>>());
            Interlocked.Exchange(ref _aliasIndex, new Dictionary<ulong, IReadOnlyList<CardData>>());
        }
    }
}
