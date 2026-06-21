using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;

namespace ScriptSupport.Legacy.Services
{
    public class GetIDService
    {
        private static string _databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"data\CardData\KonamiID", "KonamiID.cdb");
        private static string _errorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "error.log");
        private static string _yamlFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "yaml-yugi");
        private static string _connectionString;

        public static int? GetKonamiOfficialID(string password)
        {
            if (string.IsNullOrEmpty(_databasePath) || !File.Exists(_databasePath)) return null;

            string connectionString = $"Data Source={_databasePath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT konami_id FROM dataOfficial WHERE password = @password";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@password", password);
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi kết nối cơ sở dữ liệu: {ex.Message}.");
                    return null;
                }
            }
        }
        public static int? GetKonamiRushID(string name)
        {
            if (string.IsNullOrEmpty(_databasePath) || !File.Exists(_databasePath)) return null;

            string connectionString = $"Data Source={_databasePath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string searchName = name;
                    string modifiedName = null;
                    if (name.Contains(" (Rush)"))
                    {
                        modifiedName = name.Replace(" (Rush)", string.Empty);
                    }

                    string query = "SELECT konami_id FROM dataRush WHERE name = @name OR name = @modifiedName";
                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", searchName);
                        if (modifiedName != null)
                        {
                            command.Parameters.AddWithValue("@modifiedName", modifiedName);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@modifiedName", DBNull.Value);
                        }
                        object result = command.ExecuteScalar();

                        if (result != null && result is int)
                        {
                            return Convert.ToInt32(result);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (SQLiteException sqlEx)
                {
                    MessageBox.Show($"Lỗi SQLite: {sqlEx.Message}");
                    return null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi kết nối cơ sở dữ liệu: {ex.Message}.");
                    return null;
                }
            }
        }

        public static void GetID()
        {
            _connectionString = $"Data Source={_databasePath};Version=3;";
            CreateDatabase();
        }
        private static void CreateDatabase()
        {
            try
            {
                if (File.Exists(_databasePath))
                {
                    File.Delete(_databasePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}.");
                return;
            }
            SQLiteConnection.CreateFile(_databasePath);

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string createDataOfficialTable = @"CREATE TABLE IF NOT EXISTS dataOfficial (konami_id INTEGER PRIMARY KEY, password INTEGER);";
                using (var command = new SQLiteCommand(createDataOfficialTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                string createDataRushTable = @"CREATE TABLE IF NOT EXISTS dataRush (konami_id INTEGER PRIMARY KEY, name TEXT);";
                using (var command = new SQLiteCommand(createDataRushTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            try
            {
                if (!File.Exists(_errorPath))
                {
                    File.Create(_errorPath).Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}.");
            }
            ProcessYamlFiles();
        }

        public static void ProcessYamlFiles()
        {
            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    // var files = Directory.GetFiles(_yamlFolderPath, "*.yaml", SearchOption.AllDirectories);

                    foreach (var file in Directory.GetFiles(_yamlFolderPath, "*.yaml", SearchOption.AllDirectories))
                    {
                        if (IsValidOfficialYamlFile(file))
                        {
                            var (konamiId, password) = ReadOfficialYamlFile(file);
                            SaveToDataOfficial(connection, konamiId, password);
                        }
                        else if (IsValidRushYamlFile(file))
                        {
                            var (konamiID, name) = ReadRushYamlFile(file);
                            SaveToDataRush(connection, konamiID, name);
                        }
                        else
                        {
                            ///
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Lỗi khi xử lý file YAML: {ex.Message}");
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
            using (StreamWriter writer = new StreamWriter(_errorPath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }

    }
}
