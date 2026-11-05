using CardEditor.Models;

namespace CardEditor.Interfaces
{
    public interface StringInterface
    {
        StringConf LoadStringConf(string filePath);
        StringCombine Merge(StringConf en, StringConf vi);
    }
}
