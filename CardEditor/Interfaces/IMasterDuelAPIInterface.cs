using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Models.MasterDuel;

namespace CardEditor.Interfaces
{
    public interface IMasterDuelAPIInterface
    {
        Task<(bool Success, string Message, List<CardMasterDuel> Cards)> LoadCardMasterDuelList(
            string url, IProgress<(int page, int totalCards)>? progress = null, CancellationToken ct = default);
        Task<(bool Success, string Message)> SaveCardMasterDuelList(
            IEnumerable<CardMasterDuel> cardList, string filePath);
    }
}
