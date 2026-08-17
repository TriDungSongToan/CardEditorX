using System.Collections.Generic;
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

    
}
