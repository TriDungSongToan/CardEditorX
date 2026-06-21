using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardEditor.Editor.Completion
{
    internal readonly struct CompletionContext
    {
        public string Qualifier { get; }
        public string Prefix { get; }
        public bool IsDotCompletion => Qualifier != null;

        public CompletionContext(string qualifier, string prefix)
        {
            Qualifier = qualifier;
            Prefix = prefix;
        }
    }
}
