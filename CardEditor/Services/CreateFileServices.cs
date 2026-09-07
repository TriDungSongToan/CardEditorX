using System;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using ClosedXML.Excel;
using CardEditor.Enums;
using CardEditor.Constants;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

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
        public static async Task<(bool, string)> CreateDatabaseCommand(string filePath, bool hasFlag = false)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath)) return (false, CMess.fileNotExit.ToText());

                string dbExtension = ConstantExtension.CardDBExtensions.First();
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                string dbFilePath;
                if (ConstantExtension.CardDBExtensions.Contains(extension)) dbFilePath = filePath;
                else dbFilePath = Path.ChangeExtension(filePath, dbExtension);

                string folderPath = System.IO.Path.GetDirectoryName(dbFilePath);
                string cdbFileName = System.IO.Path.GetFileName(dbFilePath);

                var (resultCreate, messageCreate) = await Task.Run(() => CreateDatabase(folderPath, cdbFileName, hasFlag));
                if (resultCreate) return (true, messageCreate);
                else return (false, messageCreate);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public static (bool, string) CreateDatabase(string folderPath, string cdbFileName, bool hasFlag = false)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(cdbFileName))
                return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText()));
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
                        command.CommandText = $@"CREATE TABLE datas (
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
                            category INTEGER{(hasFlag ? ",\n                    flag INTEGER" : "")}
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
        public static async Task<(bool, string)> CreateExcelCommand(string filePath, bool hasFlag = false)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath)) return (false, CMess.fileNotExit.ToText());

                string excelExtension = ConstantExtension.ExcelExtensions.First();
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                string excelFilePath;
                if (ConstantExtension.ExcelExtensions.Contains(extension)) excelFilePath = filePath;
                else excelFilePath = Path.ChangeExtension(filePath, excelExtension);

                string folderPath = System.IO.Path.GetDirectoryName(excelFilePath);
                string xlsxFileName = System.IO.Path.GetFileName(excelFilePath);

                var (resultCreate, messageCreate) = await Task.Run(() => CreateExcelTemplate(folderPath, xlsxFileName, hasFlag));
                if (resultCreate) return (true, messageCreate);
                else return (false, messageCreate);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public static (bool, string) CreateExcelTemplate(string folderPath, string xlsxFilename, bool hasFlag = false)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(xlsxFilename))
                return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText()));
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string xlsxFilePath = Path.Combine(folderPath, xlsxFilename);
            string tempXlsxFilePath = xlsxFilePath + ".tmp.xlsx";

            try
            {
                // (header, isIntegerColumn) theo đúng thứ tự cột, bắt đầu từ cột B
                var headers = new List<(string Name, bool IsInteger)>
                {
                    ("name", false), ("desc", false),
                    ("ot", false), ("alias", true), ("setcode", false), ("type", false),
                    ("atk", true), ("def", true),
                    ("level", false), ("race", false), ("attribute", false), ("category", false)
                };

                if (hasFlag) headers.Add(("flag", false));

                for (int i = 1; i <= 16; i++)
                    headers.Add(($"str{i}", false));

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Sheet1");

                    // Cột A (id): Integer format
                    worksheet.Column(1).Style.NumberFormat.Format = "0";

                    // A1 cố định
                    worksheet.Cell(1, 1).Value = "CardEditorX";

                    for (int i = 0; i < headers.Count; i++)
                    {
                        int colIndex = i + 2; // cột B trở đi
                        worksheet.Cell(1, colIndex).Value = headers[i].Name;
                        worksheet.Column(colIndex).Style.NumberFormat.Format = headers[i].IsInteger ? "0" : "@";
                    }

                    workbook.SaveAs(tempXlsxFilePath);
                }

                if (File.Exists(xlsxFilePath)) File.Delete(xlsxFilePath);
                File.Move(tempXlsxFilePath, xlsxFilePath);
                return (true, xlsxFilePath);
            }
            catch (Exception ex)
            {
                if (File.Exists(tempXlsxFilePath)) File.Delete(tempXlsxFilePath);
                return (false, ex.Message);
            }
        }
        public static async Task<(bool, string)> CreateCedsCommand(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath)) return (false, CMess.fileNotExit.ToText());

                string cedsExtension = ConstantExtension.CedsExtensions.First();
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                string cedsFilePath;
                if (ConstantExtension.CedsExtensions.Contains(extension)) cedsFilePath = filePath;
                else cedsFilePath = Path.ChangeExtension(filePath, cedsExtension);

                string folderPath = System.IO.Path.GetDirectoryName(cedsFilePath);
                string cdbFileName = System.IO.Path.GetFileName(cedsFilePath);

                var (resultCreate, messageCreate) = await Task.Run(() => CreateCeds(folderPath, cdbFileName));
                if (resultCreate) return (true, messageCreate);
                else return (false, messageCreate);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public static (bool, string) CreateCeds(string folderPath, string cedsFileName)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(cedsFileName))
                return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText()));
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string cedsFilePath = Path.Combine(folderPath, cedsFileName);
            string tempCedsFilePath = cedsFilePath + ".tmp";

            try
            {
                File.WriteAllText(tempCedsFilePath, "[]", Encoding.UTF8);

                if (File.Exists(cedsFilePath)) File.Delete(cedsFilePath);
                File.Move(tempCedsFilePath, cedsFilePath);
                return (true, cedsFilePath);
            }
            catch (Exception ex)
            {
                if (File.Exists(tempCedsFilePath)) File.Delete(tempCedsFilePath);
                return (false, ex.Message);
            }
        }
        public static (bool, string) CreateChatDatabase(string folderPath, string FileName)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath) || string.IsNullOrWhiteSpace(FileName))
                return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText()));

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
                return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText()));
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
                return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText()));
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
                return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText()));
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
        public static (bool, string) CreateCreditsDatabase(string folderPath, string FileName)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(FileName))
                return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText()));
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
                            CREATE TABLE IF NOT EXISTS Credit (
                            Id INTEGER PRIMARY KEY NOT NULL,
                            Name TEXT,
                            Header TEXT NOT NULL,
                            Footer TEXT,
                            Description TEXT
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
        public static (bool, string) CreateScriptCommand(string filePath, bool newScript)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath)) return (false, CMess.fileNotExit.ToText());

                string scriptExtension = ConstantExtension.ScriptExtensions.First();
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                string scriptFilePath = ConstantExtension.ScriptExtensions.Contains(extension)
                    ? filePath : Path.ChangeExtension(filePath, scriptExtension);

                string folderPath = System.IO.Path.GetDirectoryName(scriptFilePath);
                string scriptFileName = System.IO.Path.GetFileName(scriptFilePath);

                var (resultCreate, messageCreate) = CreateScript(folderPath, scriptFileName, newScript);
                if (resultCreate) return (true, messageCreate);
                else return (false, messageCreate);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        public static (bool, string) CreateScript(string folderPath, string fileName, bool newScript, string luaContent = "", LanguageArea Area = LanguageArea.Unknown)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(fileName))
                return (false, string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText()));
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, fileName);
            string tempFilePath = filePath + ".tmp";

            try
            {
                using (StreamWriter writer = new StreamWriter(tempFilePath, false, new UTF8Encoding(false)))
                {
                    if (newScript && Regex.IsMatch(Path.GetFileName(filePath), @"^c\d+\.lua$", RegexOptions.IgnoreCase))
                    {
                        string safeLuaContent = (luaContent ?? "")
                            .Replace("--", "")
                            .Replace("\n", " ")
                            .Replace("\r", "");

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
