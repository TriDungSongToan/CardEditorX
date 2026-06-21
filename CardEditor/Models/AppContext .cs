using System;
using System.Reflection;
using System.Threading;

namespace CardEditor.Models
{
    public class AppContext
    {
        private static readonly Lazy<AppContext> _instance = new Lazy<AppContext>(() => new AppContext());
        public static AppContext Instance => _instance.Value;
        private readonly object _lock = new object();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public string ExeFilePath { get; }
        public string ConfigFolderPath { get; }
        public string ErrorLogFilePath { get; }
        public string DataFolderPath { get; }
        public string BanListFolderPath { get; }
        public string RaresFolderPath { get; }
        public string RaresListDBPath { get; }
        public string RareCardDBPath { get; }
        public string GenesysFolderPath { get; }
        public string GenesysDBPath { get; }
        public string StampFolderPath { get; }
        public string KonamiIDFilePath { get; }

        private AppContext()
        {
            ExeFilePath = Assembly.GetExecutingAssembly().Location;
            ConfigFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExeFilePath), "config");
            DataFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExeFilePath), "data");
            ErrorLogFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ExeFilePath), "ErrorLog.txt");
            BanListFolderPath = System.IO.Path.Combine(DataFolderPath, @"CardData\BanList");
            RaresFolderPath = System.IO.Path.Combine(DataFolderPath, "Rares");
            RaresListDBPath = System.IO.Path.Combine(RaresFolderPath, "RaresListDB.cdb");
            RareCardDBPath = System.IO.Path.Combine(RaresFolderPath, "RareCardsDB.cdb");
            GenesysFolderPath = System.IO.Path.Combine(DataFolderPath, @"CardData\Genesys");
            GenesysDBPath = System.IO.Path.Combine(GenesysFolderPath, "GenesysCardsDB.cdb");
            StampFolderPath = System.IO.Path.Combine(DataFolderPath, "RareStamp");
            KonamiIDFilePath = System.IO.Path.Combine(ConfigFolderPath, $@"CardData\KonamiID\KonamiID.cdb");
        }
    }
}
