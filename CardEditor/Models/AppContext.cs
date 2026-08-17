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
        public string BaseDirectory { get; }
        public string ConfigFolderPath { get; }
        public string ErrorLogFilePath { get; }
        public string DataFolderPath { get; }
        public string BanListFolderPath { get; }
        public string RaresFolderPath { get; }
        public string RaresListDBPath { get; }
        public string RareCardDBPath { get; }
        public string GenesysFolderPath { get; }
        public string GenesysDBPath { get; }
        public string PenDescLangFilePath { get; }
        public string CreditFolderPath { get; }
        public string CreditDBPath { get; }
        public string StampFolderPath { get; }
        public string KonamiIDFilePath { get; }

        private AppContext()
        {
            ExeFilePath = Assembly.GetExecutingAssembly().Location;
            BaseDirectory = System.IO.Path.GetDirectoryName(ExeFilePath) ?? AppDomain.CurrentDomain.BaseDirectory;
            ConfigFolderPath = System.IO.Path.Combine(BaseDirectory, "config");
            DataFolderPath = System.IO.Path.Combine(BaseDirectory, "data");
            ErrorLogFilePath = System.IO.Path.Combine(BaseDirectory, "ErrorLog.txt");
            BanListFolderPath = System.IO.Path.Combine(DataFolderPath, @"CardData\BanList");
            RaresFolderPath = System.IO.Path.Combine(DataFolderPath, "Rares");
            RaresListDBPath = System.IO.Path.Combine(RaresFolderPath, "RaresListDB.cdb");
            RareCardDBPath = System.IO.Path.Combine(RaresFolderPath, "RareCardsDB.cdb");
            GenesysFolderPath = System.IO.Path.Combine(DataFolderPath, @"CardData\Genesys");
            GenesysDBPath = System.IO.Path.Combine(GenesysFolderPath, "GenesysCardsDB.cdb");
            PenDescLangFilePath = System.IO.Path.Combine(DataFolderPath, $@"CardData\Language\PenDescFormat.json");
            CreditFolderPath = System.IO.Path.Combine(DataFolderPath, "Credit");
            CreditDBPath = System.IO.Path.Combine(CreditFolderPath, "CreditDB.cdb");
            StampFolderPath = System.IO.Path.Combine(DataFolderPath, "RareStamp");
            KonamiIDFilePath = System.IO.Path.Combine(ConfigFolderPath, $@"CardData\KonamiID\KonamiID.cdb");
        }
    }
}
