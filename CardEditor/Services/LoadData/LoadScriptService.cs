using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using CardEditor.Models;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.LoadData
{
    public class LoadScriptService
    {
        public static Task<LoadScriptPathsResult> LoadAllScriptPaths()
        {
            return Task.Run(() =>
            {
                var result = new LoadScriptPathsResult();

                string dataSourcePath = CardEditor.ViewModels.ConfigViewModel.Instance.userSetting.DataSource;
                if (!Directory.Exists(dataSourcePath))
                {
                    result.Result = false;
                    result.Message =
                        $"{CMess.dataSourceErr.ToText()} {dataSourcePath}";

                    return result;
                }

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
                    result.Result = true;
                    result.Paths = new Dictionary<ulong, List<string>>(concurrentDict);

                    return result;
                }
                catch (Exception ex)
                {
                    result.Result = false;
                    result.Message = ex.Message;

                    return result;
                }
            });
        }
    }
}
