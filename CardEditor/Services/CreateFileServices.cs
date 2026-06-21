using System;
using System.IO;
using System.Text.Json;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;
using System.Text;
using static CardEditor.Helpers.LanguageDetectorHelper;

namespace CardEditor.Services
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

            CardEditor.Models.Settings.UserSetting userSetting = new Models.Settings.UserSetting();
            CardEditor.Models.Settings.DataHandlingSetting dataHandlingSetting = new Models.Settings.DataHandlingSetting();
            CardEditor.Models.Settings.ImageSetting imageSetting = new Models.Settings.ImageSetting();
            CardEditor.Models.Settings.CodeEditSetting codeEditSetting = new Models.Settings.CodeEditSetting();
            CardEditor.Models.Settings.DeckEditSetting deckEditSetting = new Models.Settings.DeckEditSetting();

            InsertSetting(connection, "UserSetting", userSetting, jsonOptions);
            InsertSetting(connection, "DataHandlingSetting", dataHandlingSetting, jsonOptions);
            InsertSetting(connection, "ImageSetting", imageSetting, jsonOptions);
            InsertSetting(connection, "CodeEditSetting", codeEditSetting, jsonOptions);
            InsertSetting(connection, "DeckEditSetting", deckEditSetting, jsonOptions);
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
        public static (bool, string) CreateDatabase(string folderPath, string cdbFileName)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(cdbFileName))
                return (false, CMess.invaFolderPath.ToText());
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string dbFilePath = Path.Combine(folderPath, cdbFileName);
            string tempDbFilePath = dbFilePath + ".tmp";

            try
            {
                SQLiteConnection.CreateFile(tempDbFilePath);
                using (var connection = new SQLiteConnection($"Data Source={tempDbFilePath};Version=3;"))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"CREATE TABLE datas (
                            id INTEGER PRIMARY KEY,
                            ot INTEGER,
                            alias INTEGER,
                            setcode INTEGER,
                            type INTEGER,
                            atk INTEGER,
                            def INTEGER,
                            level INTEGER,
                            race INTEGER,
                            attribute INTEGER,
                            category INTEGER
                        );";
                        command.ExecuteNonQuery();

                        command.CommandText = @"CREATE TABLE texts (
                            id INTEGER PRIMARY KEY,
                            name TEXT,
                            desc TEXT,
                            str1 TEXT,
                            str2 TEXT,
                            str3 TEXT,
                            str4 TEXT,
                            str5 TEXT,
                            str6 TEXT,
                            str7 TEXT,
                            str8 TEXT,
                            str9 TEXT,
                            str10 TEXT,
                            str11 TEXT,
                            str12 TEXT,
                            str13 TEXT,
                            str14 TEXT,
                            str15 TEXT,
                            str16 TEXT
                        );";
                        command.ExecuteNonQuery();
                    }
                }

                if (File.Exists(dbFilePath)) File.Delete(dbFilePath);
                File.Move(tempDbFilePath, dbFilePath);
                return (true, dbFilePath);
            }
            catch (Exception ex)
            {
                if (File.Exists(tempDbFilePath)) File.Delete(tempDbFilePath);
                return (false, ex.Message);
            }
        }
        public static (bool, string) CreateChatDatabase(string folderPath, string FileName)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath) || string.IsNullOrWhiteSpace(FileName))
                return (false, CMess.invaFolderPath.ToText());

            string dbFilePath = Path.Combine(folderPath, FileName);
            string tempDbFilePath = dbFilePath + ".tmp";

            try
            {
                SQLiteConnection.CreateFile(tempDbFilePath);
                using (var connection = new SQLiteConnection($"Data Source={tempDbFilePath};Version=3;"))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS Chats (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            name TEXT NOT NULL,
                            created_at DATETIME NOT NULL
                        );
                        CREATE TABLE IF NOT EXISTS Messages (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            chat_id INTEGER NOT NULL,
                            sender TEXT NOT NULL,
                            message TEXT NOT NULL,
                            message_type INTEGER NOT NULL,
                            created_at DATETIME NOT NULL,
                            FOREIGN KEY (chat_id) REFERENCES Chats(id) ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS idx_messages_chatid ON Messages(chat_id);";
                        command.ExecuteNonQuery();
                    }
                }
                if (File.Exists(dbFilePath)) File.Delete(dbFilePath);
                File.Move(tempDbFilePath, dbFilePath);
                return (true, dbFilePath);
            }
            catch (Exception ex)
            {
                if (File.Exists(tempDbFilePath)) File.Delete(tempDbFilePath);
                return (false, ex.Message);
            }
        }
        public static (bool, string) CreateRaresListDatabase(string folderPath, string FileName)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(FileName))
                return (false, CMess.invaFolderPath.ToText());
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string dbFilePath = Path.Combine(folderPath, FileName);
            string tempDbFilePath = dbFilePath + ".tmp";

            try
            {
                SQLiteConnection.CreateFile(tempDbFilePath);
                using (var connection = new SQLiteConnection($"Data Source={tempDbFilePath};Version=3;"))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS RareList (
                            IdRare INTEGER PRIMARY KEY NOT NULL,
                            Name TEXT NOT NULL,
                            Code INTEGER NOT NULL UNIQUE,
                            ImagePath TEXT
                        );";
                        command.ExecuteNonQuery();
                    }
                }
                if (File.Exists(dbFilePath)) File.Delete(dbFilePath);
                File.Move(tempDbFilePath, dbFilePath);
                return (true, dbFilePath);
            }
            catch (Exception ex)
            {
                if (File.Exists(tempDbFilePath)) File.Delete(tempDbFilePath);
                return (false, ex.Message);
            }
        }
        public static (bool, string) CreateRareCardsDatabase(string folderPath, string FileName)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(FileName))
                return (false, CMess.invaFolderPath.ToText());
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string dbFilePath = Path.Combine(folderPath, FileName);
            string tempDbFilePath = dbFilePath + ".tmp";

            try
            {
                SQLiteConnection.CreateFile(tempDbFilePath);
                using (var connection = new SQLiteConnection($"Data Source={tempDbFilePath};Version=3;"))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS RareCard (
                            id INTEGER PRIMARY KEY NOT NULL,
                            name TEXT,
                            rare INTEGER
                        );";
                        command.ExecuteNonQuery();
                    }
                }
                if (File.Exists(dbFilePath)) File.Delete(dbFilePath);
                File.Move(tempDbFilePath, dbFilePath);
                return (true, dbFilePath);
            }
            catch (Exception ex)
            {
                if (File.Exists(tempDbFilePath)) File.Delete(tempDbFilePath);
                return (false, ex.Message);
            }
        }
        public static (bool, string) CreateGenesysCardsDatabase(string folderPath, string FileName)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(FileName))
                return (false, CMess.invaFolderPath.ToText());
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string dbFilePath = Path.Combine(folderPath, FileName);
            string tempDbFilePath = dbFilePath + ".tmp";

            try
            {
                SQLiteConnection.CreateFile(tempDbFilePath);
                using (var connection = new SQLiteConnection($"Data Source={tempDbFilePath};Version=3;"))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS GenesysCard (
                            id INTEGER PRIMARY KEY NOT NULL,
                            name TEXT,
                            point INTEGER
                        );";
                        command.ExecuteNonQuery();
                    }
                }
                if (File.Exists(dbFilePath)) File.Delete(dbFilePath);
                File.Move(tempDbFilePath, dbFilePath);
                return (true, dbFilePath);
            }
            catch (Exception ex)
            {
                if (File.Exists(tempDbFilePath)) File.Delete(tempDbFilePath);
                return (false, ex.Message);
            }
        }
        public static (bool, string) CreateScript(string folderPath, string fileName, bool newScript, string luaContent = "", LanguageArea Area = LanguageArea.Unknown)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(fileName))
                return (false, CMess.invaFolderPath.ToText());
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, fileName);
            string tempFilePath = filePath + ".tmp";

            try
            {
                using (StreamWriter writer = new StreamWriter(tempFilePath, false, new UTF8Encoding(false)))
                {
                    if (newScript && Regex.IsMatch(Path.GetFileName(filePath), @"^c\d+\.lua$", RegexOptions.IgnoreCase))
                    {
                        string safeLuaContent = luaContent.Replace("--", "").Replace("\n", " ");
                        string ocgComment = (Area == LanguageArea.OCG || Area == LanguageArea.Mixed) ? $"--{safeLuaContent}" : "-- add OCG card name";
                        string tcgComment = (Area == LanguageArea.TCG || Area == LanguageArea.Mixed) ? $"--{safeLuaContent}" : "-- add TCG card name";

                        writer.WriteLine(ocgComment);
                        writer.WriteLine(tcgComment);
                        writer.WriteLine("local s,id,o=GetID()");
                        writer.WriteLine("function s.initial_effect(c)");
                        writer.WriteLine("\t");
                        writer.WriteLine("end");
                    }
                }
                if (File.Exists(filePath)) File.Delete(filePath);
                File.Move(tempFilePath, filePath);
                return (true, filePath);
            }
            catch (Exception ex)
            {
                if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
                return (false, ex.Message);
            }
        }
        public static (bool, string) CreateDeck(string folderPath, string fileName, string userName)
        {
            return (true, string.Empty);
        }
        public static (bool, string) CreateBanList(string folderPath, string fileName)
        {

            return (true, string.Empty);
        }
    }
}
