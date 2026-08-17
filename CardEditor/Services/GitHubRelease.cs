using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CardEditor.Services
{
    public class GithubReleaseDownloader
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        static GithubReleaseDownloader()
        {
            // GitHub API bắt buộc phải có User-Agent, nếu không sẽ trả về 403
            _httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("CardEditorX", "1.0"));
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        }

        /// <summary>
        /// Tải file releaseName từ bản Release mới nhất của repoURL, giải nén vào folderPath.
        /// </summary>
        public async Task<(bool resultDownload, string messageDownload)> DownloadGithubRelease(
            string repoURL, string releaseName, string folderPath)
        {
            string tempZipPath = null;
            try
            {
                // 1. Parse owner/repo từ URL dạng https://github.com/{owner}/{repo}.git
                var match = Regex.Match(repoURL, @"github\.com/([^/]+)/([^/\.]+)(\.git)?/?$");
                if (!match.Success)
                    return (false, $"Không parse được owner/repo từ URL: {repoURL}");

                string owner = match.Groups[1].Value;
                string repo = match.Groups[2].Value;

                // 2. Gọi API lấy latest release
                string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/latest";
                HttpResponseMessage releaseResponse = await _httpClient.GetAsync(apiUrl);

                if (!releaseResponse.IsSuccessStatusCode)
                {
                    string errBody = await releaseResponse.Content.ReadAsStringAsync();
                    return (false, $"Lỗi gọi GitHub API ({(int)releaseResponse.StatusCode}): {errBody}");
                }

                string releaseJson = await releaseResponse.Content.ReadAsStringAsync();
                JObject root = JObject.Parse(releaseJson);
                JArray assets = (JArray)root["assets"];

                if (assets == null || assets.Count == 0)
                    return (false, "The latest release contains no assets.");

                // 3. Tìm asset có tên trùng releaseName
                string downloadUrl = null;
                foreach (var asset in assets)
                {
                    string assetName = asset["name"]?.ToString();
                    if (string.Equals(assetName, releaseName, StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset["browser_download_url"]?.ToString();
                        break;
                    }
                }

                if (downloadUrl == null)
                    return (false, $"Asset named '{releaseName}' not found in the latest release.");

                // 4. Tải file zip về vị trí tạm
                tempZipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{releaseName}");
                using (var zipResponse = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (!zipResponse.IsSuccessStatusCode)
                        return (false, $"Error loading asset file ({(int)zipResponse.StatusCode}).");

                    using (var fs = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await zipResponse.Content.CopyToAsync(fs);
                    }
                }

                // 5. Giải nén thủ công (tự xử lý overwrite vì Framework không có overload sẵn)
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                using (var archive = ZipFile.OpenRead(tempZipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        string destPath = Path.Combine(folderPath, entry.FullName);

                        // Entry là thư mục (FullName kết thúc bằng '/')
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            Directory.CreateDirectory(destPath);
                            continue;
                        }

                        string destDir = Path.GetDirectoryName(destPath);
                        if (!Directory.Exists(destDir))
                            Directory.CreateDirectory(destDir);

                        entry.ExtractToFile(destPath, overwrite: true);
                    }
                }

                return (true, $"Successfully downloaded and extracted to: {folderPath}");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                // 6. Dọn file tạm
                if (tempZipPath != null && File.Exists(tempZipPath))
                    File.Delete(tempZipPath);
            }
        }
    }
}
