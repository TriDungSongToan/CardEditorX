using System;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using System.Threading.Tasks;
using CardEditor.Enums;
using CardEditor.Models;
using CardEditor.Constants;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.CreateFile
{
    public class CreateDatabaseService
    {
        public static async Task<CreateCardListResult> CreateDatabaseYGOCommand(string filePath, bool hasFlag = false)
        {
            CreateCardListResult result = new();
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    result.Result = false;
                    result.Messenger = CMess.fileNotExit.ToText();
                    return result;
                }

                string dbExtension = ConstantExtension.CardDBExtensions.First();
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                string dbFilePath;
                if (ConstantExtension.CardDBExtensions.Contains(extension)) dbFilePath = filePath;
                else dbFilePath = Path.ChangeExtension(filePath, dbExtension);

                string folderPath = System.IO.Path.GetDirectoryName(dbFilePath);
                string cdbFileName = System.IO.Path.GetFileName(dbFilePath);

                result = await Task.Run(() => CreateDatabaseYGO(folderPath, cdbFileName, hasFlag));
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static CreateCardListResult CreateDatabaseYGO(string folderPath, string cdbFileName, bool hasFlag = false)
        {
            CreateCardListResult result = new();

            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(cdbFileName))
            {
                result.Result = false;
                result.Messenger = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText());
                return result;
            }
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

                result.Result = true;
                result.Messenger = string.Empty;
                result.FilePath = dbFilePath;
                result.Format = hasFlag ? CardListFormat.YGOHasFlag : CardListFormat.YGONoFlag;
                result.HasFlag = hasFlag;
            }
            catch (Exception ex)
            {
                if (File.Exists(tempDbFilePath)) File.Delete(tempDbFilePath);
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }


        public static async Task<CreateCardListResult> CreateDatabaseOMEGACommand(string filePath)
        {
            CreateCardListResult result = new();
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    result.Result = false;
                    result.Messenger = CMess.fileNotExit.ToText();
                    return result;
                }

                string dbExtension = ConstantExtension.CardDBExtensions.First();
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                string dbFilePath;
                if (ConstantExtension.CardDBExtensions.Contains(extension)) dbFilePath = filePath;
                else dbFilePath = Path.ChangeExtension(filePath, dbExtension);

                string folderPath = System.IO.Path.GetDirectoryName(dbFilePath);
                string cdbFileName = System.IO.Path.GetFileName(dbFilePath);

                result = await Task.Run(() => CreateDatabaseOMEGA(folderPath, cdbFileName));
            }
            catch (Exception ex)
            {
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
        public static CreateCardListResult CreateDatabaseOMEGA(string folderPath, string cdbFileName)
        {
            CreateCardListResult result = new();

            if (string.IsNullOrWhiteSpace(folderPath) || string.IsNullOrWhiteSpace(cdbFileName))
            {
                result.Result = false;
                result.Messenger = string.Format(CMess.TwoPlaceholderInva.ToText(), CMess.Folder.ToText(), CMess.Path.ToText());
                return result;
            }
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
                            setcode BLOB,
                            type INTEGER,
                            atk INTEGER,
                            def INTEGER,
                            level INTEGER,
                            race INTEGER,
                            attribute INTEGER,
                            category INTEGER,
                            genre INTEGER,
                            script BLOB,
                            support BLOB
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

                result.Result = true;
                result.Messenger = string.Empty;
                result.FilePath = dbFilePath;
                result.Format = CardListFormat.OMEGA;
                result.HasFlag = true;
            }
            catch (Exception ex)
            {
                if (File.Exists(tempDbFilePath)) File.Delete(tempDbFilePath);
                result.Result = false;
                result.Messenger = ex.Message;
            }
            return result;
        }
    }
}
