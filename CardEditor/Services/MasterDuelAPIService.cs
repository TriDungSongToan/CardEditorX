using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Converter;
using CardEditor.Interfaces;
using CardEditor.Models.MasterDuel;

namespace CardEditor.Services
{
    public class MasterDuelAPIService : IMasterDuelAPIInterface
    {
        private static readonly HttpClient _client = new HttpClient(new HttpClientHandler())
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Dùng chung cho cả serialize (lưu file) và deserialize (đọc API/file)
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true, // để file JSON offline dễ đọc/diff khi mở tay
            Converters =
        {
            new EnumMemberJsonConverter<CardEditor.Models.MasterDuel.CardType>(),
            new EnumMemberJsonConverter<CardEditor.Models.MasterDuel.Rarity>(),
            new EnumMemberJsonConverter<CardEditor.Models.MasterDuel.Attribute>(),
            new EnumMemberJsonConverter<CardEditor.Models.MasterDuel.LinkArrow>(),
            new EnumMemberJsonConverter<CardEditor.Models.MasterDuel.MonsterType>(),
            new EnumMemberJsonConverter<CardEditor.Models.MasterDuel.Race>(),
        }
        };

        private async Task<HttpResponseMessage?> GetWithRetryAsync(string url, CancellationToken ct)
        {
            const int maxRetries = 5;
            int delayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                HttpResponseMessage response = await _client.GetAsync(url, ct);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return response; // hết trang, để caller xử lý

                if (response.IsSuccessStatusCode)
                    return response;

                // 429 Too Many Requests -> tôn trọng Retry-After nếu API trả về
                if (response.StatusCode == (HttpStatusCode)429)
                {
                    int wait = response.Headers.RetryAfter?.Delta?.Milliseconds ?? delayMs;
                    await Task.Delay(wait, ct);
                    delayMs *= 2;
                    continue;
                }

                // Lỗi 5xx tạm thời -> retry
                if ((int)response.StatusCode >= 500)
                {
                    await Task.Delay(delayMs, ct);
                    delayMs *= 2;
                    continue;
                }

                // Lỗi khác (400, 401, 403...) -> không retry, trả lỗi luôn
                return response;
            }

            return null; // hết retry mà vẫn lỗi
        }
        public async Task<(bool Success, string Message, List<CardMasterDuel> Cards)> LoadCardMasterDuelList(
            string url, IProgress<(int page, int totalCards)>? progress = null, CancellationToken ct = default)
        {
            var allCards = new List<CardMasterDuel>();
            int page = 1;

            while (!ct.IsCancellationRequested)
            {
                string pageUrl = $"{url}?page={page}";

                HttpResponseMessage? response;
                try
                {
                    response = await GetWithRetryAsync(pageUrl, ct);
                }
                catch (Exception ex)
                {
                    return (false, $"Lỗi mạng tại trang {page}: {ex.Message}", allCards);
                }

                if (response is null)
                    return (false, $"Hết retry tại trang {page}", allCards);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    break; // hết trang, coi như thành công

                if (!response.IsSuccessStatusCode)
                    return (false, $"HTTP {(int)response.StatusCode} tại trang {page}", allCards);

                List<CardMasterDuel>? pageCards;

                try
                {
                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    {
                        pageCards = await JsonSerializer.DeserializeAsync<List<CardMasterDuel>>(
                            stream, _jsonOptions, ct);
                    }
                }
                catch (JsonException ex)
                {
                    return (false, $"Lỗi parse JSON tại trang {page}: {ex.Message}", allCards);
                }

                if (pageCards is null || pageCards.Count == 0) break;

                allCards.AddRange(pageCards);
                progress?.Report((page, allCards.Count));

                page++;
                await Task.Delay(200, ct);
            }

            return (true, $"Đã tải {allCards.Count} card từ {page - 1} trang", allCards);
        }
        public async Task<(bool Success, string Message)> SaveCardMasterDuelList(
            IEnumerable<CardMasterDuel> cardList, string filePath)
        {
            try
            {
                var cards = cardList as List<CardMasterDuel> ?? cardList.ToList();

                if (cards.Count == 0)
                    return (false, "Danh sách card rỗng, không lưu.");

                string? dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                var cache = new CardCache
                {
                    LastFetched = DateTime.UtcNow,
                    CardCount = cards.Count,
                    Cards = cards
                };

                // Ghi ra file tạm trước, rồi rename -> tránh hỏng file cache cũ nếu ghi giữa chừng bị crash
                string tempPath = filePath + ".tmp";

                using (FileStream fs = File.Create(tempPath))
                {
                    await JsonSerializer.SerializeAsync(fs, cache, _jsonOptions);
                }

                if (File.Exists(filePath)) File.Delete(filePath);

                File.Move(tempPath, filePath);

                return (true, $"Đã lưu {cards.Count} card vào {filePath}");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi lưu file: {ex.Message}");
            }
        }
    }

}
