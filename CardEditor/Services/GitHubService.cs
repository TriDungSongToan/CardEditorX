using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using LibGit2Sharp;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using GitCommands = LibGit2Sharp.Commands;

namespace CardEditor.Services
{
    public class GitHubService
    {
        static GitHubService()
        {
            // GitHub API yêu cầu User-Agent, nếu không sẽ bị từ chối request
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("CardEditor-App");
        }

        #region 1
        /// <summary>
        /// Folder chứa DATA THUẦN (không .git, không lịch sử). Service này:
        /// 1) Kiểm tra commit mới nhất trên remote bằng ls-remote (rất nhẹ, không tải object).
        ///    Commit SHA đó chính là "flag" để biết data local đã mới nhất chưa.
        /// 2) Nếu có bản mới, tải snapshot zip của đúng commit đó và MERGE (ghi đè) vào folder,
        ///    KHÔNG xoá bất kỳ file/folder nào không có trong zip.
        ///    => User có thể tự thêm/sửa/xoá file trong folder, những gì user tự thêm sẽ không bị đụng tới.
        ///    Lưu ý: nếu user chỉnh sửa 1 file mà repo cũng update đúng file đó, bản update sẽ ghi đè lên.
        /// Yêu cầu: repo phải nằm trên GitHub (dùng archive zip endpoint của GitHub).
        /// </summary>


        // Timeout mặc định của HttpClient là 100s -> quá ngắn cho repo ảnh/dữ liệu lớn khi tải full snapshot.
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
        private const string CommitFileName = ".last_commit_sha";
        private const int MaxFilesForIncrementalSync = 290; // GitHub Compare API giới hạn ~300 file/response



        /// <summary>
        /// Kiểm tra remote có commit mới hơn local hay không.
        /// Không tải bất kỳ file/object nào -> rất nhanh (tương đương "git ls-remote").
        /// </summary>
        public static async Task<(bool hasUpdate, string remoteSha, string branch)> CheckForUpdateAsync(
            string folderPath, string gitHubUrl, string preferredBranch = null)
        {
            return await Task.Run(() =>
            {
                var refs = Repository.ListRemoteReferences(gitHubUrl).ToList();

                string branch = preferredBranch;
                DirectReference targetRef = null;

                // Nếu không chỉ định nhánh, dùng HEAD symbolic ref của remote (nhánh mặc định thật sự)
                if (string.IsNullOrEmpty(branch))
                {
                    var headRef = refs.FirstOrDefault(r => r.CanonicalName == "HEAD") as SymbolicReference;
                    if (headRef?.Target != null)
                    {
                        branch = headRef.Target.CanonicalName.Replace("refs/heads/", "");
                        targetRef = headRef.ResolveToDirectReference();
                    }
                }

                if (targetRef == null)
                {
                    branch = string.IsNullOrEmpty(branch) ? "main" : branch;
                    targetRef = refs.FirstOrDefault(r => r.CanonicalName == $"refs/heads/{branch}") as DirectReference;

                    if (targetRef == null && branch == "main")
                    {
                        branch = "master";
                        targetRef = refs.FirstOrDefault(r => r.CanonicalName == $"refs/heads/{branch}") as DirectReference;
                    }
                }

                if (targetRef == null)
                    throw new Exception($"Không tìm thấy nhánh '{branch}' trên remote.");

                string remoteSha = targetRef.TargetIdentifier;
                string localSha = ReadLocalSha(folderPath);

                bool folderEmpty = !Directory.Exists(folderPath)
                                    || !Directory.EnumerateFileSystemEntries(folderPath).Any();

                bool hasUpdate = folderEmpty || !string.Equals(remoteSha, localSha, StringComparison.OrdinalIgnoreCase);

                return (hasUpdate, remoteSha, branch);
            });
        }

        /// <summary>
        /// Tải snapshot (zip) của đúng commit đã xác định ở bước Check, rồi MERGE vào folder:
        /// - File trùng đường dẫn với repo -> bị ghi đè bằng bản mới.
        /// - File/folder KHÔNG có trong repo (user tự thêm) -> giữ nguyên, không đụng tới.
        /// - File đã bị xoá khỏi repo trên remote -> bản cũ local vẫn còn (không tự xoá),
        ///   vì không có cách nào chắc chắn phân biệt "file mồ côi do repo xoá" với "file user tự thêm".
        /// </summary>
        public static async Task<(bool success, string message)> SyncSnapshotAsync(
            string folderPath, string gitHubUrl, string commitSha)
        {
            var (owner, repo) = ParseOwnerRepo(gitHubUrl);
            string localSha = ReadLocalSha(folderPath);
            bool folderHasContent = Directory.Exists(folderPath) && Directory.EnumerateFileSystemEntries(folderPath).Any();

            // Chưa từng có data local -> phải tải full snapshot (không có gì để so sánh diff)
            if (string.IsNullOrEmpty(localSha) || !folderHasContent)
            {
                return await FullSyncAsync(folderPath, owner, repo, commitSha);
            }

            // Đã có data từ lần sync trước -> chỉ tải phần thay đổi giữa localSha -> commitSha
            var (incrementalSuccess, incrementalMessage, fallbackToFull) =
                await IncrementalSyncAsync(folderPath, owner, repo, localSha, commitSha);

            if (incrementalSuccess || !fallbackToFull)
                return (incrementalSuccess, incrementalMessage);

            // Diff quá lớn / localSha không còn hợp lệ trên remote (vd force-push) -> tải lại full cho chắc
            return await FullSyncAsync(folderPath, owner, repo, commitSha);
        }

        /// <summary>
        /// Chỉ tải những file có thay đổi giữa 2 commit, dùng GitHub Compare API.
        /// Nhanh hơn nhiều so với tải full zip khi chỉ vài file thay đổi.
        /// </summary>
        private static async Task<(bool success, string message, bool fallbackToFull)> IncrementalSyncAsync(
            string folderPath, string owner, string repo, string baseSha, string headSha)
        {
            try
            {
                var changedFiles = await GetChangedFilesAsync(owner, repo, baseSha, headSha);

                if (changedFiles == null)
                    return (false, string.Empty, fallbackToFull: true);

                if (changedFiles.Count >= MaxFilesForIncrementalSync)
                    return (false, string.Empty, fallbackToFull: true);

                foreach (var (filename, status) in changedFiles)
                {
                    // Yêu cầu: file bị xoá trên repo -> không đụng tới bản local
                    if (status == "removed")
                        continue;

                    string rawUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{headSha}/{filename}";
                    string destPath = Path.Combine(folderPath, filename.Replace('/', Path.DirectorySeparatorChar));

                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                    using (var response = await _http.GetAsync(rawUrl))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            Debug.WriteLine($"Bỏ qua, không tải được '{filename}': {response.StatusCode}");
                            continue;
                        }

                        using (var fs = File.Create(destPath))
                        {
                            await response.Content.CopyToAsync(fs);
                        }
                    }
                }

                SaveLocalSha(folderPath, headSha);
                return (true, string.Empty, fallbackToFull: false);
            }
            catch (Exception ex)
            {
                // Compare API lỗi (vd baseSha không còn tồn tại do force-push/rebase) -> để caller tải full lại
                Debug.WriteLine($"IncrementalSync thất bại, sẽ fallback full sync: {ex.Message}");
                return (false, ex.Message, fallbackToFull: true);
            }
        }

        private static async Task<System.Collections.Generic.List<(string filename, string status)>> GetChangedFilesAsync(
            string owner, string repo, string baseSha, string headSha)
        {
            string url = $"https://api.github.com/repos/{owner}/{repo}/compare/{baseSha}...{headSha}";

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Accept.ParseAdd("application/vnd.github+json");

                using (var response = await _http.SendAsync(request))
                {
                    if (!response.IsSuccessStatusCode)
                        return null; // caller sẽ fallback sang full sync

                    string json = await response.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(json))
                    {
                        var result = new System.Collections.Generic.List<(string, string)>();

                        if (doc.RootElement.TryGetProperty("files", out var files))
                        {
                            foreach (var f in files.EnumerateArray())
                            {
                                string filename = f.GetProperty("filename").GetString();
                                string status = f.GetProperty("status").GetString();
                                result.Add((filename, status));
                            }
                        }

                        return result;
                    }
                }
            }
        }

        /// <summary>
        /// Tải toàn bộ snapshot zip của 1 commit và merge vào folder (dùng cho lần sync đầu tiên,
        /// hoặc khi incremental sync không khả thi).
        /// </summary>
        private static async Task<(bool success, string message)> FullSyncAsync(
            string folderPath, string owner, string repo, string commitSha)
        {
            string tempZip = null;
            try
            {
                Directory.CreateDirectory(folderPath);

                string zipUrl = $"https://github.com/{owner}/{repo}/archive/{commitSha}.zip";

                tempZip = Path.Combine(Path.GetTempPath(), $"{repo}_{commitSha}.zip");

                using (var response = await _http.GetAsync(zipUrl))
                {
                    response.EnsureSuccessStatusCode();
                    using (var fs = File.Create(tempZip))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }

                using (var archive = ZipFile.OpenRead(tempZip))
                {
                    if (archive.Entries.Count == 0)
                        throw new Exception("File zip tải về rỗng.");

                    // GitHub zip luôn bọc mọi thứ trong 1 thư mục gốc dạng "{repo}-{sha}/..."
                    string rootFolder = archive.Entries[0].FullName.Split('/')[0];

                    foreach (var entry in archive.Entries)
                    {
                        string relativePath = entry.FullName.Substring(rootFolder.Length).TrimStart('/');
                        if (string.IsNullOrEmpty(relativePath))
                            continue;

                        string destPath = Path.Combine(folderPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            // entry là thư mục
                            Directory.CreateDirectory(destPath);
                        }
                        else
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                            entry.ExtractToFile(destPath, overwrite: true);
                        }
                    }
                }

                SaveLocalSha(folderPath, commitSha);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                if (tempZip != null && File.Exists(tempZip))
                {
                    try { File.Delete(tempZip); } catch { /* ignore */ }
                }
            }
        }

        private static (string owner, string repo) ParseOwnerRepo(string gitHubUrl)
        {
            // Hỗ trợ dạng: https://github.com/{owner}/{repo} hoặc .../{repo}.git
            var uri = new Uri(gitHubUrl);
            var segments = uri.AbsolutePath.Trim('/').Split('/');
            if (segments.Length < 2)
                throw new Exception($"URL GitHub không hợp lệ: {gitHubUrl}");

            string owner = segments[0];
            string repo = segments[1].EndsWith(".git") ? segments[1].Substring(0, segments[1].Length - 4) : segments[1];
            return (owner, repo);
        }

        private static string ReadLocalSha(string folderPath)
        {
            string file = Path.Combine(folderPath, CommitFileName);
            if (File.Exists(file))
                return File.ReadAllText(file).Trim();

            // Migration: data cũ được tạo bằng Repository.Clone (code cũ) sẽ có folder .git
            // nhưng chưa có file version -> đọc trực tiếp HEAD sha từ .git để tránh báo "có update" oan.
            string gitFolder = Path.Combine(folderPath, ".git");
            if (Directory.Exists(gitFolder))
            {
                try
                {
                    using (var repo = new Repository(folderPath))
                    {
                        return repo.Head?.Tip?.Sha;
                    }
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static void SaveLocalSha(string folderPath, string sha)
        {
            // Đặt file version thành Hidden để đỡ gây rối cho user khi họ mở folder data
            string file = Path.Combine(folderPath, CommitFileName);
            File.WriteAllText(file, sha);
            try { File.SetAttributes(file, FileAttributes.Hidden); } catch { /* không sao nếu OS không hỗ trợ */ }
        }
        #endregion

        #region 2
        public static async Task<(bool, string)> CheckForUpdatesAsync(string FolderPath, string GitHubUrl)
        {
            // Đảm bảo thư mục tồn tại
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            // Kiểm tra xem folder data có rỗng không
            bool isDataFolderEmpty = !Directory.EnumerateFileSystemEntries(FolderPath).Any();
            // Kiểm tra xem folder data có chứa Git repository không
            bool isGitRepository = Directory.Exists(Path.Combine(FolderPath, ".git"));

            try
            {
                if (isDataFolderEmpty)
                {
                    // Nếu folder rỗng, thực hiện clone
                    await Task.Run(() => DownLoadRepository(FolderPath, GitHubUrl));
                    return (true, string.Empty);
                }
                else
                {
                    if (isGitRepository)
                    {
                        // Nếu đã có Git repository, thử cập nhật
                        try
                        {
                            return await Task.Run(() => FetchAndResetToLatest(FolderPath, GitHubUrl));
                        }
                        catch (Exception ex) when (ex.Message.Contains("object not found") ||
                                                  ex.Message.Contains("no match for id"))
                        {
                            // Nếu gặp lỗi "object not found", thực hiện clone lại
                            CleanDirectory(FolderPath);
                            var (result, message) = await Task.Run(() => DownLoadRepository(FolderPath, GitHubUrl));
                            return (result, message);
                        }
                        catch (Exception ex)
                        {
                            return (false, ex.Message);
                        }
                    }
                    else
                    {
                        CleanDirectory(FolderPath);
                        var (result, message) = await Task.Run(() => DownLoadRepository(FolderPath, GitHubUrl));
                        return (result, message);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private static (bool, string) DownLoadRepository(string FolderPath, string GitHubUrl)
        {
            try
            {
                // Xóa dữ liệu cũ nếu có
                if (!CleanDirectory(FolderPath))
                {
                    throw new Exception("Cannot Delete Folder");
                }

                // Thiết lập tùy chọn cho việc clone
                var options = new CloneOptions
                {
                    Checkout = true,
                    RecurseSubmodules = false
                };

                // Cài đặt thuộc tính TagFetchMode
                try
                {
                    var fetchOptions = new FetchOptions();
                    if (fetchOptions.GetType().GetProperty("TagFetchMode") != null)
                    {
                        fetchOptions.TagFetchMode = TagFetchMode.All;
                    }
                }
                catch
                {
                    ///
                }

                // Clone repository với tùy chọn đã thiết lập
                Repository.Clone(GitHubUrl, FolderPath, options);

                // Cấu hình repository để lấy tất cả các nhánh
                using (var repo = new Repository(FolderPath))
                {
                    // Lấy tất cả các nhánh từ remote
                    var remote = repo.Network.Remotes["origin"];
                    if (remote != null)
                    {
                        var refSpecs = new string[] { "+refs/heads/*:refs/remotes/origin/*" };
                        GitCommands.Fetch(repo, remote.Name, refSpecs, null, "Fetching all branches");

                        var defaultBranch = repo.Branches["master"] ?? repo.Branches["main"];
                        if (defaultBranch != null)
                        {
                            GitCommands.Checkout(repo, defaultBranch);
                        }
                    }
                }
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private static (bool, string) FetchAndResetToLatest(string FolderPath, string GitHubUrl)
        {
            bool hasChanges = false;

            try
            {
                using (var repo = new Repository(FolderPath))
                {
                    // Kiểm tra và sửa chữa remote nếu cần
                    var remote = repo.Network.Remotes["origin"];
                    if (remote == null)
                    {
                        // Thêm remote nếu không tồn tại
                        remote = repo.Network.Remotes.Add("origin", GitHubUrl);
                    }
                    else if (remote.Url != GitHubUrl)
                    {
                        // Cập nhật URL nếu đã thay đổi
                        repo.Network.Remotes.Update("origin", r => r.Url = GitHubUrl);
                    }

                    // Thiết lập fetch options
                    var fetchOptions = new FetchOptions
                    {
                        Prune = false
                    };

                    // Cài đặt TagFetchMode nếu có trong phiên bản này
                    if (fetchOptions.GetType().GetProperty("TagFetchMode") != null)
                    {
                        fetchOptions.TagFetchMode = TagFetchMode.All;
                    }

                    // Đảm bảo chúng ta lấy tất cả các nhánh
                    var refSpecs = new string[] { "+refs/heads/*:refs/remotes/origin/*" };

                    // Thực hiện fetch từ remote
                    GitCommands.Fetch(repo, remote.Name, refSpecs, fetchOptions, "Fetching updates");

                    // Lấy nhánh hiện tại
                    var currentBranch = repo.Head.FriendlyName;

                    // Lấy remote tracking branch tương ứng
                    var trackingBranch = repo.Branches[$"origin/{currentBranch}"];

                    if (trackingBranch != null && trackingBranch.Tip != null)
                    {
                        // Lấy commit ID của HEAD hiện tại
                        var currentCommitId = repo.Head.Tip.Id;

                        // Lấy commit ID của remote tracking branch
                        var remoteCommitId = trackingBranch.Tip.Id;

                        // Kiểm tra xem có cập nhật không
                        hasChanges = currentCommitId != remoteCommitId;

                        if (hasChanges)
                        {
                            try
                            {
                                // Hard reset về commit mới nhất của nhánh remote
                                repo.Reset(ResetMode.Hard, trackingBranch.Tip);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Lỗi khi reset: {ex.Message}");
                                throw;
                            }
                        }
                    }
                    else
                    {
                        // Trường hợp không tìm thấy nhánh tương ứng trên remote
                        var defaultRemoteBranch = repo.Branches["origin/master"] ?? repo.Branches["origin/main"];
                        if (defaultRemoteBranch != null)
                        {
                            GitCommands.Checkout(repo, defaultRemoteBranch.FriendlyName.Replace("origin/", ""));
                            hasChanges = true;
                        }
                        else
                        {
                            throw new Exception("Không tìm thấy nhánh mặc định trên remote");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
                throw;
            }
            return (hasChanges, string.Empty);
        }

        private static bool CleanDirectory(string directoryPath)
        {
            // Đảm bảo thư mục tồn tại
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                return true;
            }
            try
            {
                DirectoryInfo di = new DirectoryInfo(directoryPath);
                foreach (FileInfo file in di.GetFiles())
                {
                    try
                    {
                        file.Attributes = FileAttributes.Normal;
                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Không thể xóa file {file.FullName}: {ex.Message}");
                    }
                }

                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    try
                    {
                        dir.Attributes = FileAttributes.Normal;
                        dir.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Không thể xóa thư mục {dir.FullName}: {ex.Message}");
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region 3
        private static string GitPath()
        {
            string gitPath = null;
            try
            {
                gitPath = ConfigurationManager.AppSettings["gitpath"];
                if (string.IsNullOrEmpty(gitPath))
                {
                    throw new ConfigurationErrorsException(CMess.gitPathMiss.ToText());
                }
                return gitPath;
            }
            catch (ConfigurationErrorsException ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{string.Format(CMess.TwoPlaceholderError.ToText(), CMess.Read.ToText(), CMess.Config.ToText())} {ex.Message}", new[] { CMess.ok.ToText() });
                gitPath = @"C:\Program Files\Git\cmd\git.exe";
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message} {CMess.useDefault.ToText()}", new[] { CMess.ok.ToText() });
                gitPath = @"C:\Program Files\Git\cmd\git.exe";
            }
            return gitPath;
        }

        // Kiểm tra thư mục có phải là kho Git hợp lệ không
        public static bool IsGitRepository(string repoPath)
        {
            return Directory.Exists(System.IO.Path.Combine(repoPath, ".git"));
        }

        // Clone kho Git vào thư mục chỉ định
        public static bool CloneRepository(string repoUrl, string targetPath)
        {
            try
            {
                if (Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath, true);  // Xóa thư mục và tất cả các tệp bên trong
                }

                // Đảm bảo đường dẫn tới git.exe
                string gitPath = GitPath();
                if (string.IsNullOrEmpty(gitPath))
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        CMess.gitNotFound.ToText(), new[] { CMess.ok.ToText() });
                    return false;
                }

                string gitCloneCommand = $"clone --no-single-branch --depth 1 --filter=tree:0 \"{repoUrl}\" \"{targetPath}\"";

                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = gitPath,
                    Arguments = gitCloneCommand,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = new Process { StartInfo = processStartInfo };
                process.Start();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        CMess.cloneRepoSuc.ToText(), new[] { CMess.ok.ToText() });
                    return true;
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        CMess.errorCloneRepo.ToText(), new[] { CMess.ok.ToText() });
                    return false;
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return false;
            }
        }

        // Kiểm tra sự thay đổi từ kho Git
        public static bool CheckForUpdates(string localRepoPath)
        {
            string gitPath = GitPath();
            // string gitFetchCommand = "fetch"; // Lệnh git fetch để cập nhật thông tin từ GitHub
            string gitDiffCommand = "diff origin/main"; // Kiểm tra sự khác biệt với nhánh chính (main)

            // Chạy lệnh git fetch
            ProcessStartInfo fetchProcessInfo = new ProcessStartInfo
            {
                FileName = gitPath,
                Arguments = $"fetch", // Lệnh git fetch để tải các thay đổi mới
                WorkingDirectory = localRepoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process fetchProcess = new Process();
            fetchProcess.StartInfo = fetchProcessInfo;
            fetchProcess.Start();
            fetchProcess.WaitForExit();

            // Kiểm tra nếu lệnh fetch thành công
            if (fetchProcess.ExitCode != 0)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    CMess.errorFetData.ToText(), new[] { CMess.ok.ToText() });
                return false;
            }

            // Chạy lệnh git diff để kiểm tra sự khác biệt
            ProcessStartInfo diffProcessInfo = new ProcessStartInfo
            {
                FileName = gitPath,
                Arguments = gitDiffCommand, // Lệnh git diff để kiểm tra sự thay đổi
                WorkingDirectory = localRepoPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process diffProcess = new Process();
            diffProcess.StartInfo = diffProcessInfo;
            diffProcess.Start();

            string diffOutput = diffProcess.StandardOutput.ReadToEnd();
            diffProcess.WaitForExit();

            // Nếu có sự khác biệt, git diff sẽ có output, nghĩa là có cập nhật
            return !string.IsNullOrWhiteSpace(diffOutput);
        }

        // Tải và ghi đè dữ liệu vào thư mục data
        public static bool DownloadAndUpdate(string localRepoPath)
        {
            try
            {
                string gitPath = GitPath();
                if (string.IsNullOrEmpty(gitPath))
                {
                    CMSG.Show(CMess.warning.ToText(), CMSG.MessageBoxIconType.Warning,
                        CMess.gitNotFound.ToText(), new[] { CMess.ok.ToText() });
                    return false;
                }

                string gitPullCommand = "pull origin main"; // Lệnh git pull để lấy cập nhật mới nhất từ GitHub

                ProcessStartInfo pullProcessInfo = new ProcessStartInfo
                {
                    FileName = gitPath,
                    Arguments = gitPullCommand, // Lệnh git pull để tải các thay đổi mới
                    WorkingDirectory = localRepoPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process pullProcess = new Process();
                pullProcess.StartInfo = pullProcessInfo;
                pullProcess.Start();
                pullProcess.WaitForExit();

                if (pullProcess.ExitCode == 0)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                        CMess.dataUpdateSuc.ToText(), new[] { CMess.ok.ToText() });
                    return true;
                }
                else
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        string.Format(CMess.PlaceholderError.ToText(), CMess.Update.ToText()), new[] { CMess.ok.ToText() });
                    return false;
                }
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return false;
            }
        }
        #endregion

    }
}
