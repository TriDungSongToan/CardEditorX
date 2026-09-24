using System.IO;
using System.Collections.Generic;
using CardEditor.Models;
using CardEditor.Interfaces;

namespace CardEditor.Services
{
    public class FileService : IFileInterface
    {
        public void WriteToFile(IEnumerable<string> listItem, string filePath)
        {
            System.IO.File.WriteAllLines(filePath, listItem);
        }
        public void WriteToFile(string json, string filePath)
        {
            System.IO.File.WriteAllText(filePath, json);
        }
    }

    public static class FileLocationService
    {
        public static AppTitle BuildTitlePhysicalFile(string PhysicalFile)
        {
            var appTitle = new AppTitle();

            appTitle.PhysicalFullPath = PhysicalFile;
            appTitle.DisplayTitle = PhysicalFile;
            appTitle.DisplayTabItemHeader = Path.GetFileName(PhysicalFile);

            return appTitle;
        }

        public static AppTitle BuildTitleZipEntry(string archiveFilePath, string archiveEntryName)
        {
            var appTitle = new AppTitle();

            var entryPath = archiveEntryName.Replace('/', Path.DirectorySeparatorChar);

            appTitle.PhysicalFullPath = archiveFilePath;
            appTitle.DisplayTitle = $"{archiveFilePath} ! {entryPath}";
            appTitle.DisplayTabItemHeader = entryPath;

            return appTitle;
        }

        public static AppTitle BuildTitleDBCell(string dbFilePath, string table, string column, ulong primaryKey)
        {
            var appTitle = new AppTitle();

            var cellPath = $"{table}.{column} [id={primaryKey}]";

            appTitle.PhysicalFullPath = dbFilePath;
            appTitle.DisplayTitle = $"{dbFilePath} # {cellPath}";
            appTitle.DisplayTabItemHeader = cellPath;
            return appTitle;
        }

        public static AppTitle BuildTitleDBCellZipEntry(string dbFilePath, string table, string column, ulong primaryKey,
            string archiveFilePath, string archiveEntryName)
        {
            var appTitle = new AppTitle();

            var entryPath = archiveEntryName.Replace('/', Path.DirectorySeparatorChar);
            var cellPath = $"{table}.{column} [id={primaryKey}]";

            appTitle.PhysicalFullPath = dbFilePath;
            appTitle.DisplayTitle = $"{archiveFilePath} ! {entryPath} # {cellPath}";
            appTitle.DisplayTabItemHeader = $"{entryPath} # {cellPath}";

            return appTitle;
        }
    }
}
