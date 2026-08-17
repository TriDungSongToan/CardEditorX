using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CardEditor.Helpers
{
    public static class APIHelper
    {
        // .NET Framework 4.8.1 không có SocketsHttpHandler (chỉ có từ .NET Core trở lên).
        // Dùng HttpClientHandler thay thế. Việc quản lý pooled connection lifetime
        // trên Framework thường được xử lý qua ServicePointManager (xem ghi chú bên dưới).
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

                // 429 Too Many Requests -> tôn trọng Retry-After nếu API trả về
                if ((int)response.StatusCode == 429)
                {
                    int wait = response.Headers.RetryAfter?.Delta?.Milliseconds ?? delayMs;
                    await Task.Delay(wait, ct).ConfigureAwait(false);
                    delayMs *= 2;
                    continue;
                }

                // Lỗi 5xx tạm thời -> retry
                if ((int)response.StatusCode >= 500)
                {
                    await Task.Delay(delayMs, ct).ConfigureAwait(false);
                    delayMs *= 2;
                    continue;
                }

                // Lỗi khác (400, 401, 403...) -> không retry, trả lỗi luôn
                return response;
            }

            return null; // hết retry mà vẫn lỗi
        }
    }
}
