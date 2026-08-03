using System;
using System.IO;
using System.Linq;
using System.Data;
using System.Text.RegularExpressions;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using CardEditor.Models;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services
{
    public class GetKonamiIDService
    {
        private static string KonamiIDDBPath;
        private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties().Build();

        public static async Task<(bool, string)> CreateDatabase()
        {
            KonamiIDDBPath = System.IO.Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, $@"KonamiID.cdb");
            try
            {
                if (File.Exists(KonamiIDDBPath)) File.Delete(KonamiIDDBPath);

                SQLiteConnection.CreateFile(KonamiIDDBPath);
                string connectionString = $"Data Source={KonamiIDDBPath};Version=3;";

                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string createDataOfficialTable = @"CREATE TABLE IF NOT EXISTS dataOfficial (konami_id INTEGER PRIMARY KEY, password INTEGER);";
                    using (var command = new SQLiteCommand(createDataOfficialTable, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }

                    string createDataRushTable = @"CREATE TABLE IF NOT EXISTS dataRush (konami_id INTEGER PRIMARY KEY, name TEXT);";
                    using (var command = new SQLiteCommand(createDataRushTable, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static async Task GetOfficialKonamiID(string yankYugiFolderPath)
        {
            KonamiIDDBPath = System.IO.Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, $@"KonamiID.cdb");
            try
            {
                var CardList = await GetListOfficialData(yankYugiFolderPath);
                if (CardList == null || CardList.Count == 0)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Warning,
                        $"No valid YAML files found in {yankYugiFolderPath}", new[] { CMess.ok.ToText() });
                    return;
                }
                string connectionString = $"Data Source={KonamiIDDBPath};Version=3;";
                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        string sql = @"INSERT OR REPLACE INTO dataOfficial (konami_id, password) VALUES (@konami_id, @password);";

                        using (var command = new SQLiteCommand(sql, connection, transaction))
                        {
                            var konamiIdParam = command.Parameters.Add("@konami_id", DbType.Int64);
                            var passwordParam = command.Parameters.Add("@password", DbType.Int64);

                            foreach (var item in CardList)
                            {
                                konamiIdParam.Value = item.KonamiID;
                                passwordParam.Value = item.Password;
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                        transaction.Commit();
                    }
                }
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    $"Get Official Konami ID Suc", new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return;
            }
        }
        public static async Task GetRushKonamiID(string yankYugiFolderPath)
        {
            KonamiIDDBPath = System.IO.Path.Combine(CardEditor.Models.AppContext.Instance.DataFolderPath, $@"KonamiID.cdb");
            try
            {
                var CardList = await GetListRushData(yankYugiFolderPath);
                if (CardList == null || CardList.Count == 0)
                {
                    CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Warning,
                        $"No valid YAML files found in {yankYugiFolderPath}", new[] { CMess.ok.ToText() });
                    return;
                }
                string connectionString = $"Data Source={KonamiIDDBPath};Version=3;";
                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var transaction = connection.BeginTransaction())
                    {
                        string sql = @"INSERT OR REPLACE INTO dataRush (konami_id, name) VALUES (@konami_id, @name);";

                        using (var command = new SQLiteCommand(sql, connection, transaction))
                        {
                            var konamiIdParam = command.Parameters.Add("@konami_id", DbType.Int64);
                            var nameParam = command.Parameters.Add("@name", DbType.String);

                            foreach (var item in CardList)
                            {
                                konamiIdParam.Value = item.KonamiID;
                                nameParam.Value = item.Name.En;
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                        transaction.Commit();
                    }
                }
                CMSG.Show(CMess.notifi.ToText(), CMSG.MessageBoxIconType.Notification,
                    $"Get Rush Konami ID Suc", new[] { CMess.ok.ToText() });
            }
            catch (Exception ex)
            {
                CMSG.Show(CMess.error.ToText(), CMSG.MessageBoxIconType.Error,
                    $"{CMess.errorOcc.ToText()} {ex.Message}", new[] { CMess.ok.ToText() });
                return;
            }
        }

        private static async Task<List<OfficialData>> GetListOfficialData(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return new List<OfficialData>();

            return await Task.Run(() =>
            {
                var files = Directory.EnumerateFiles(folderPath, "*.yaml", SearchOption.AllDirectories).ToList();
                var result = new ConcurrentBag<OfficialData>();

                Parallel.ForEach(files, file =>
                {
                    var data = TryReadOfficialYaml(file);
                    if (data != null)
                    {
                        result.Add(data);
                    }
                });

                return result.ToList();
            });
        }
        private static async Task<List<RushData>> GetListRushData(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return new List<RushData>();

            return await Task.Run(() =>
            {
                var files = Directory.EnumerateFiles(folderPath, "*.yaml", SearchOption.AllDirectories).ToList();
                var result = new ConcurrentBag<RushData>();

                Parallel.ForEach(files, file =>
                {
                    var data = TryReadRushYaml(file);
                    if (data != null)
                    {
                        result.Add(data);
                    }
                });

                return result.ToList();
            });
        }
        private static OfficialData TryReadOfficialYaml(string filePath)
        {
            try
            {
                var yamlContent = File.ReadAllText(filePath);
                var result = YamlDeserializer.Deserialize<OfficialData>(yamlContent);

                if (result.KonamiID > 0 && result.Password > 0)
                {
                    return result;
                }
                return null;

                //return YamlDeserializer.Deserialize<OfficialData>(yamlContent);
            }
            catch
            {
                return null;
            }
        }
        private static RushData TryReadRushYaml(string filePath)
        {
            try
            {
                var yamlContent = File.ReadAllText(filePath);
                var result = YamlDeserializer.Deserialize<RushData>(yamlContent);

                if (result.KonamiID > 0 && !string.IsNullOrEmpty(result.Name.En))
                {
                    return result;
                }
                return null;

                //return YamlDeserializer.Deserialize<RushData>(yamlContent);
            }
            catch
            {
                return null;
            }
        }







        // Kiểm tra tính hợp lệ của file yaml
        private static bool IsValidOfficialYamlFile(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (!Regex.IsMatch(fileName, @"^\d+$"))
                return false;

            var lines = File.ReadLines(filePath).Take(2).ToArray();
            return lines.Length >= 2 &&
                Regex.IsMatch(lines[0], @"^konami_id:\s*\d+$") &&
                Regex.IsMatch(lines[1], @"^password:\s*\d+$");
        }
        private static bool IsValidRushYamlFile(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            if (!Regex.IsMatch(fileName, @"^\d+$"))
                return false;

            var lines = File.ReadLines(filePath).Take(3).ToArray();
            return lines.Length == 3 &&
                Regex.IsMatch(lines[0], @"^konami_id:\s*\d+$") &&
                Regex.IsMatch(lines[1], @"^name:$") &&
                Regex.IsMatch(lines[2], @"^\s*en:\s*.+$");

        }

        // Đọc file yaml
        private static (string konamiId, string password) ReadOfficialYamlFile(string filePath)
        {
            var lines = File.ReadLines(filePath).Take(2).ToArray();
            var konamiId = lines[0].Split(':')[1].Trim();
            var password = lines[1].Split(':')[1].Trim();

            return (konamiId, password);
        }
        private static (string konamiId, string nameEn) ReadRushYamlFile(string filePath)
        {
            var lines = File.ReadLines(filePath).Take(3).ToArray();
            var konamiId = lines[0].Split(':')[1].Trim();
            var nameEn = lines[2].Split(new[] { "en:" }, StringSplitOptions.None)[1].Trim();

            return (konamiId, nameEn);
        }

        // Lưu vào database
        private static void SaveToDataOfficial(SQLiteConnection connection, string konamiId, string password)
        {
            string insertQuery = "INSERT OR REPLACE INTO dataOfficial (konami_id, password) VALUES (@konami_id, @password)";
            using (var command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@konami_id", konamiId);
                command.Parameters.AddWithValue("@password", password);
                command.ExecuteNonQuery();
            }
        }
        private static void SaveToDataRush(SQLiteConnection connection, string konamiID, string nameEN)
        {
            string insertQuery = "INSERT OR REPLACE INTO dataRush (konami_id, name) VALUES (@konami_id, @name)";
            using (var command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@konami_id", konamiID);
                command.Parameters.AddWithValue("@name", nameEN);
                command.ExecuteNonQuery();
            }
        }

        private static void LogError(string message)
        {
            //using (StreamWriter writer = new StreamWriter(_errorPath, true))
            //{
            //    writer.WriteLine($"{DateTime.Now}: {message}");
            //}
        }

    }
}
