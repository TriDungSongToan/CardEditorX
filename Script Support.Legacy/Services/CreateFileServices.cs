using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ScriptSupport.Legacy.Services
{
    public static class CreateFileServices
    {
        public static (bool, string) CreateSetting(string folderPath, string fileName)
        {
            try
            {
                string dbPath = Path.Combine(folderPath, fileName);
                string _connectionString = $"Data Source={dbPath};Version=3;";

                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();

                string createTable = @"CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL,
                    LastModified DATETIME DEFAULT CURRENT_TIMESTAMP)";

                using (var command = new SQLiteCommand(createTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                string checkQuery = "SELECT COUNT(*) FROM Settings";
                using (var checkCommand = new SQLiteCommand(checkQuery, connection))
                {
                    long count = (long)checkCommand.ExecuteScalar();

                    if (count == 0)
                    {
                        CreateDefaultSettings(connection);
                    }
                }
                return (true, "Setting Database created and initialized successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public static void CreateDefaultSettings(SQLiteConnection connection)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false
            };

            ScriptSupport.Legacy.Models.Settings.UserSetting userSetting = new Models.Settings.UserSetting();
            ScriptSupport.Legacy.Models.Settings.DataHandlingSetting dataHandlingSetting = new Models.Settings.DataHandlingSetting();

            InsertSetting(connection, "UserSetting", userSetting, jsonOptions);
            InsertSetting(connection, "DataHandlingSetting", dataHandlingSetting, jsonOptions);
        }
        private static void InsertSetting<T>(SQLiteConnection connection, string key, T settingObject, JsonSerializerOptions options)
        {
            string json = JsonSerializer.Serialize(settingObject, options);

            string insertQuery = @"INSERT INTO Settings (Key, Value, LastModified) 
                                  VALUES (@key, @value, @modified)";

            using var command = new SQLiteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@key", key);
            command.Parameters.AddWithValue("@value", json);
            command.Parameters.AddWithValue("@modified", DateTime.Now);
            command.ExecuteNonQuery();
        }
    }
}
