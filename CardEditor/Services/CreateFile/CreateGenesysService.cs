using System;
using System.IO;
using System.Data.SQLite;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.CreateFile
{
    public class CreateGenesysService
    {
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
    }
}
