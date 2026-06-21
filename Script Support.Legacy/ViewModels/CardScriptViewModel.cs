using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using CMess = ScriptSupport.Legacy.Localization.Language;
using ScriptSupport.Legacy.Localization;

namespace ScriptSupport.Legacy.ViewModels
{
    public class CardScriptViewModel : IDisposable
    {
        private static readonly Lazy<CardScriptViewModel> _instance = new Lazy<CardScriptViewModel>(() => new CardScriptViewModel());
        public static CardScriptViewModel Instance => _instance.Value;
        private readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);

        private Dictionary<ulong, List<string>> _scriptPathDict;
        public bool _isLoaded = false;

        public async Task<(bool, string)> LoadScriptPaths()
        {
            if (_isLoaded) return (true, string.Empty);
            await _loadLock.WaitAsync();

            try
            {
                if (_isLoaded) return (true, string.Empty);
                string DataSource = ConfigViewModel.Instance.userSetting.DataSource;
                if (string.IsNullOrWhiteSpace(DataSource) || !Directory.Exists(DataSource))
                {
                    _isLoaded = false;
                    return (false, CMess.dataSourceMiss.ToText());
                }

                var (loadedScripts, message) = await LoadAllScriptPaths();
                if (loadedScripts == null)
                {
                    CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                        $"{CMess.errorOcc.ToText()} {message}", new[] { CMess.ok.ToText() });
                    _isLoaded = false;
                    return (false, message);
                }
                _scriptPathDict = loadedScripts;
                _isLoaded = true;
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            finally
            {
                _loadLock.Release();
            }
        }
        public async Task<(Dictionary<ulong, List<string>>, string)> LoadAllScriptPaths()
        {
            return await Task.Run(() =>
            {
                string dataSourcePath = ScriptSupport.Legacy.ViewModels.ConfigViewModel.Instance.userSetting.DataSource;
                if (!Directory.Exists(dataSourcePath)) return (null, $"{CMess.dataSourceErr.ToText()} {dataSourcePath}");

                try
                {
                    var concurrentDict = new System.Collections.Concurrent.ConcurrentDictionary<ulong, List<string>>(
                            Environment.ProcessorCount, 25000);

                    var files = Directory.EnumerateFiles(dataSourcePath, "*.lua", SearchOption.AllDirectories);

                    Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, filePath =>
                    {
                        string fileName = Path.GetFileNameWithoutExtension(filePath);
                        if (fileName.Length <= 1 || fileName[0] != 'c') return;
                        // Parse số thủ công (nhanh hơn TryParse)
                        ulong cardId = 0;
                        for (int i = 1; i < fileName.Length; i++)
                        {
                            char c = fileName[i];
                            if (c < '0' || c > '9') return;
                            cardId = cardId * 10 + (ulong)(c - '0');
                        }
                        if (cardId == 0) return;
                        concurrentDict.AddOrUpdate(cardId, _ => new List<string>(1) { filePath }, (_, list) =>
                        {
                            lock (list)
                            {
                                list.Add(filePath);
                                return list;
                            }
                        });
                    });
                    return (new Dictionary<ulong, List<string>>(concurrentDict), string.Empty);
                }
                catch (Exception ex)
                {
                    return (null, ex.Message);
                }
            });
        }
        public void Dispose()
        {
            _scriptPathDict?.Clear();
            _scriptPathDict = null;
            _isLoaded = false;
        }

    }
}
