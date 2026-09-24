using System;
using System.IO;
using System.Data.SQLite;
using CardEditor.Localization;
using CMess = CardEditor.Localization.Language;

namespace CardEditor.Services.CreateFile
{
    public class CreateChatService
    {
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
    }
}
