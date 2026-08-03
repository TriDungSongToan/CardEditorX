using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using LibGit2Sharp;
using CardEditor.Localization;
using GitCommands = LibGit2Sharp.Commands;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services
{
    public class GitHubService
    {
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
                            var (result, message) =  await Task.Run(() => DownLoadRepository(FolderPath, GitHubUrl));
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


        /// <summary>
        /// /////////////////////////////////////////////////////////
        /// </summary>


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
    }
}
