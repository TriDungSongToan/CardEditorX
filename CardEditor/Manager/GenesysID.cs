using System;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using CardEditor.Helpers;
using CardEditor.ViewModels;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Manager
{
    public static class GenesysID
    {
        public static async Task<(bool Result, string FilePath, string Message)> ProcessCardDataAsync(string filePath = null)
        {
            //1.
            string selectedSource = ConfigViewModel.Instance.userSetting.DataSource;

            //2.
            filePath ??= FileDiaLogHelper.OpenText();

            if (string.IsNullOrEmpty(filePath)) return (false, null, CMess.fileNotExit.ToText());

            try
            {
                string txtFilePath = filePath;

                //3.
                var inputCards = new List<(string Name, int Point)>();
                int totalLines = 0;
                int validLines = 0;

                var failureSplit = new List<string>();
                var failureFindId = new List<string>();

                using (var sr = new StreamReader(txtFilePath))
                {
                    string line;
                    int lineNumber = 0;
                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        lineNumber++;
                        totalLines++;

                        line = new string(line.Where(c => c == '\t' || !char.IsControl(c)).ToArray()).Trim();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var parts = line.Split('\t');
                        if (parts.Length < 2)
                        {
                            failureSplit.Add($"{lineNumber}|{line}");
                            continue;
                        }

                        string name = parts[0].Trim();
                        if (!int.TryParse(parts[1], out int point)) point = 0;

                        if (!string.IsNullOrEmpty(name))
                        {
                            inputCards.Add((name, point));
                            validLines++;
                        }
                        else
                        {
                            failureSplit.Add($"{lineNumber}|{line}");
                        }
                    }
                }

                //4.
                var nameToId = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
                var cdbFiles = Directory.GetFiles(selectedSource, "*.cdb", SearchOption.AllDirectories);

                //5.
                foreach (var cdbFile in cdbFiles)
                {
                    try
                    {
                        using var connection = new SQLiteConnection($"Data Source={cdbFile};Version=3;");
                        await connection.OpenAsync();

                        using var checkCmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='texts';", connection);
                        var tableName = await checkCmd.ExecuteScalarAsync();
                        if (tableName == null) continue;

                        using var cmd = new SQLiteCommand("SELECT id, name FROM texts;", connection);
                        using var reader = await cmd.ExecuteReaderAsync();

                        while (await reader.ReadAsync())
                        {
                            ulong id = reader.IsDBNull(0) ? 0UL : Convert.ToUInt64(reader.GetValue(0));
                            string name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1).Trim();

                            if (!string.IsNullOrEmpty(name))
                            {
                                // Ghi đè nếu name trùng
                                nameToId[name] = id;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi khi đọc file {cdbFile}: {ex.Message}");
                    }
                }

                //6.
                var results = new List<(ulong Id, string Name, int Point)>();
                foreach (var card in inputCards)
                {
                    if (nameToId.TryGetValue(card.Name, out ulong id))
                    {
                        results.Add((id, card.Name, card.Point));
                    }
                    else
                    {
                        failureFindId.Add(card.Name);
                    }
                }

                //7.
                string outputFile = Path.Combine(CardEditor.Models.AppContext.Instance.GenesysFolderPath, "resultGenesys.txt");

                using (var writer = new StreamWriter(outputFile))
                {
                    await writer.WriteLineAsync($"Total lines: {totalLines}");
                    await writer.WriteLineAsync($"Valid lines: {validLines}");

                    foreach (var item in results)
                    {
                        await writer.WriteLineAsync($"{item.Id}\t{item.Name}\t{item.Point}");
                    }
                }

                string failureLogPath = Path.Combine(CardEditor.Models.AppContext.Instance.GenesysFolderPath, "FailureLog.txt");
                using (var logWriter = new StreamWriter(failureLogPath))
                {
                    await logWriter.WriteLineAsync("#FailureSplit");
                    foreach (var entry in failureSplit)
                    {
                        await logWriter.WriteLineAsync(entry);
                    }

                    await logWriter.WriteLineAsync();
                    await logWriter.WriteLineAsync("#FailureFindID");
                    foreach (var name in failureFindId)
                    {
                        await logWriter.WriteLineAsync(name);
                    }
                }

                return (true, outputFile, "Extract Genesys Point Database Successfully!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return (false, null, ex.Message);
            }
        }
    }
}
