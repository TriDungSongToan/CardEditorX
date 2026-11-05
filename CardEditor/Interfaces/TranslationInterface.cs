using CardEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardEditor.Interfaces
{
    public interface TranslationInterface
    {
        TranslationConf LoadTranslationConfig(string filePath);
        Task<TranslationConf> LoadTranslationConfigAsync(string filePath);
        Dictionary<string, string> LoadDictionaryTranslation(string filePath);
        Dictionary<string, string> BuildDictionary(TranslationConf conf);
        Task<Dictionary<string, string>> LoadDictionaryTranslationAsync(string filePath);
        TranslationCombine Merge(TranslationConf en, Dictionary<string, string> vi);
    }
}
