using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardEditor.Editor.Analysis;
using CardEditor.Models;
using CardEditor.Services;
using HandyControl.Controls;

namespace CardEditor.ViewModels
{
    public class ScriptViewModel : IDisposable
    {
        private static readonly Lazy<ScriptViewModel> _instance = new Lazy<ScriptViewModel>(() => new ScriptViewModel());
        public static ScriptViewModel Instance => _instance.Value;
        public bool IsLoaded = false;

        #region Raw Data Storage
        private List<CompletionSymbol> _allSymbols = new List<CompletionSymbol>();
        public IReadOnlyList<CompletionSymbol> AllSymbols => _allSymbols;

        private Dictionary<string, IReadOnlyList<CompletionSymbol>> _symbolByName = new Dictionary<string, IReadOnlyList<CompletionSymbol>>();
        public IReadOnlyDictionary<string, IReadOnlyList<CompletionSymbol>> SymbolByName => _symbolByName;

        private Dictionary<SymbolKind, IReadOnlyList<CompletionSymbol>> _symbolsByKind = new Dictionary<SymbolKind, IReadOnlyList<CompletionSymbol>>();
        public IReadOnlyDictionary<SymbolKind, IReadOnlyList<CompletionSymbol>> SymbolsByKind => _symbolsByKind;
        #endregion

        public List<string> ListFileYmlSkiped = new List<string>();

        private ScriptViewModel() { }

        public (bool, string) LoadScriptData()
        {
            _allSymbols.Clear();
            var (symbols, message) = ScriptData.LoadALlData();
            if (symbols == null) return (false, message);


            _allSymbols.AddRange(symbols);
            ScriptSymbolProcessor.Process(_allSymbols);

            _symbolByName = symbols.GroupBy(s => NormalizeName(s.Name))
                .ToDictionary(g => g.Key, g => (IReadOnlyList<CompletionSymbol>)g.ToList());

            _symbolsByKind = symbols.GroupBy(s => s.Kind)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<CompletionSymbol>)g.ToList());

            IsLoaded = true;
            Debug.WriteLine($"_allSymbols co: {_allSymbols.Count()}");

            return (true, string.Empty);
        }
        private static string NormalizeName(string name) => name.ToLowerInvariant();

        public void Dispose()
        {
            _allSymbols.Clear();
            _symbolByName.Clear();
            _symbolsByKind.Clear();
        }
    }
}
