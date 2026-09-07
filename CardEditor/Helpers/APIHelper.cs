using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CardEditor.Helpers
{
    public static class APIHelper
    {
        private static readonly HttpClient _client = new HttpClient(new HttpClientHandler())
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        public static async Task<HttpResponseMessage> GetWithRetryAsync(string url, CancellationToken ct)
        {
            const int maxRetries = 5;
            int delayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                HttpResponseMessage response = await _client.GetAsync(url, ct).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return response; // hết trang, để caller xử lý

                if (response.IsSuccessStatusCode)
                    return response;

                // 429 Too Many Requests -> Retry-After
                if ((int)response.StatusCode == 429)
                {
                    int wait = response.Headers.RetryAfter?.Delta?.Milliseconds ?? delayMs;
                    await Task.Delay(wait, ct).ConfigureAwait(false);
                    delayMs *= 2;
                    continue;
                }

                // Lỗi 5xx -> retry
                if ((int)response.StatusCode >= 500)
                {
                    await Task.Delay(delayMs, ct).ConfigureAwait(false);
                    delayMs *= 2;
                    continue;
                }

                // Lỗi khác (400, 401, 403...) -> không retry
                return response;
            }

            return null; // hết retry mà vẫn lỗi
        }
    }
}
