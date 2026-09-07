using System;
using CardAppContent = CardEditor.Models.AppContext;

namespace CardEditor.Helpers
{
    public class LogHelper
    {
        public static void WriteLog(string message)
        {
            var logPath = System.IO.Path.Combine(CardAppContent.Instance.BaseDirectory, "Log.txt");
            System.IO.File.AppendAllText(logPath,
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}{Environment.NewLine}");
        }
    }
}
