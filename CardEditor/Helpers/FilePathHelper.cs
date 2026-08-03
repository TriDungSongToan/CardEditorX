using System;
using System.IO;

namespace CardEditor.Helpers
{
    public static class FilePathHelper
    {
        public static string GetRelativePath(string rootFolder, string fullPath)
        {
            if (!rootFolder.EndsWith(Path.DirectorySeparatorChar.ToString()))
                rootFolder += Path.DirectorySeparatorChar;

            var rootUri = new Uri(rootFolder);
            var fullUri = new Uri(fullPath);

            var relativeUri = rootUri.MakeRelativeUri(fullUri);

            return Uri.UnescapeDataString(relativeUri.ToString())
                      .Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
