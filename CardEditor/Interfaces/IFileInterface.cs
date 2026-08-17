using System.Collections.Generic;

namespace CardEditor.Interfaces
{
    public interface IFileInterface
    {
        void WriteToFile(IEnumerable<string> listItem, string filePath);
        void WriteToFile(string json, string filePath);
    }
}
