using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
