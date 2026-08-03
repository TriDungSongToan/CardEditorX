using System.IO;
using System.Collections.Generic;

namespace CardEditor.Helpers
{
    public static class FindNameHelper
    {
        public static List<string> LoadNameArtList(string folderPath)
        {
            List<string> backgroundArtList = new List<string> { "None" };

            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath, "*.png");
                foreach (var file in files)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                    backgroundArtList.Add(fileName);
                }
            }
            return backgroundArtList;
        }
    }
}
