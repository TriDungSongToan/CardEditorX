using System.IO;
using System.Diagnostics;
using ScriptSupport.Stores;
using ScriptSupport.Interfaces;
using ScriptSupport.Models;
using ScriptSupport.Helpers;
using ScriptSupport.Localization;
using CMess = ScriptSupport.Localization.Language;

namespace ScriptSupport.Services
{
    public class LauncherService : ILauncherInterface
    {
        private readonly ConfigStore _configStore;
        public LauncherService(ConfigStore store)
        {
            _configStore = store;
        }

        public (bool, string) OpenWeb(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return (false, string.Format(CMess.PlaceholderInva.ToText(), "URL"));

            string? browser = _configStore.UserSetting?.BrowserPath;

            try
            {
                if (!string.IsNullOrWhiteSpace(browser) && File.Exists(browser))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = browser,
                        Arguments = url,
                        UseShellExecute = false
                    };

                    Process.Start(psi);
                    return (true, string.Empty);
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (bool, string) OpenFileOrFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return (false, "Path is null or empty.");

            try
            {
                var psi = new ProcessStartInfo
                {
                    UseShellExecute = true
                };

                if (Directory.Exists(path))
                {
                    psi.FileName = path;
                }
                else if (File.Exists(path))
                {
                    psi.FileName = "explorer.exe";
                    psi.Arguments = $"/select,\"{path}\"";
                }
                else
                {
                    throw new FileNotFoundException("Path does not exist.", path);
                }

                Process.Start(psi);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public (bool, string) OpenLink(string input)
        {
            try
            {
                LinkType urlType = StringHelper.DetectURL(input);

                if (urlType == LinkType.External) return OpenWeb(input);
                else if (urlType == LinkType.Api || urlType == LinkType.Internal)
                {
                    string baseUrl = $"https://github.com";
                    string path = $"/ProjectIgnis/scrapiyard/blob/master{input}.yml";
                    return OpenWeb(baseUrl + path);
                }
                else if (urlType == LinkType.Unknown)
                {
                    return (false, "Unsupported link type.");
                    // return OpenFileOrFolder(input);
                }
                else
                {
                    return (false, "Unsupported link type.");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
