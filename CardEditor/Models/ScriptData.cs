using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CardEditor.Services;

namespace CardEditor.Models
{
    public sealed class CompletionSymbol
    {
        // ===== Immutable data (từ JSON) =====
        public SymbolKind Kind { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Namespace { get; init; } = string.Empty;
        public string Summary { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string supertype { get; init; } = string.Empty;
        public List<SuggestedLink> Links { get; set; } = new();
        public string OwnerEnum { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public TypeYaml DeclaredType { get; init; }
        public List<SymbolOverload> Overloads { get; init; } = new();
        public List<string> Tags { get; init; } = new();
        public bool IsBitmask { get; init; } = false;
        public Dictionary<string, string> Status { get; init; }

        // ===== Semantic graph (MUTABLE) =====
        public CompletionSymbol Owner { get; internal set; }

        private readonly List<CompletionSymbol> _members = new();
        public IReadOnlyList<CompletionSymbol> Members => _members;

        internal void AddMember(CompletionSymbol member)
        {
            _members.Add(member);
        }
    }
    public enum SymbolKind
    {
        Function,
        Constant,
        Enum,
        NameSpace,
        Tag,
        EnumMember,
        Keyword,
        Type,
        Variable
    }
    public sealed class SymbolOverload
    {
        public string Description { get; init; }
        public IReadOnlyList<SymbolParameter> Parameters { get; init; }
        public IReadOnlyList<SymbolReturn> Returns { get; init; }
    }
    public sealed class SymbolParameter
    {
        public string Name { get; init; }
        public List<string> Types { get; init; }
        public bool IsOptional { get; init; }
        public bool IsVariadic { get; init; }
        public string Description { get; init; }
    }
    public sealed class SymbolReturn
    {
        public string Name { get; init; }
        public List<string> Types { get; init; }
        public string Description { get; init; }
    }

    public sealed class FunctionYaml
    {
        public string name { get; set; }
        public string @namespace { get; set; }
        public string description { get; set; }
        public string summary { get; set; }

        public List<YamlParameter> parameters { get; set; }
        public List<YamlOverload> overloads { get; set; }
        public List<YamlReturn> returns { get; set; }
        public Dictionary<string, string> status { get; init; }
    }
    public sealed class TypeYaml
    {
        public string name { get; set; } = string.Empty;
        public string summary { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public List<string> tags { get; set; } = new();
        public Dictionary<string, string> status { get; init; }
        public string supertype { get; set; } = string.Empty;
        public List<TypeParameter> parameters { get; set; }
        public List<TypeValue> values { get; set; }
        public List<SuggestedLink> suggestedLinks { get; set; }
        public string guide { get; set; } = string.Empty;
        public List<TypeReturn> returns { get; set; }
    }
    public sealed class TypeParameter
    {
        public string name { get; set; }
        public List<string> type { get; set; }
        public string description { get; set; }
        public List<string> constraints { get; set; } = new();
    }
    public sealed class TypeValue
    {
        public List<string> types { get; set; } = new();
        public string description { get; set; } = string.Empty;
    }
    public sealed class TypeReturn
    {
        public List<string> type { get; set; }
        public string description { get; set; }
    }
    public sealed class YamlParameter
    {
        public string name { get; set; }
        public List<string> type { get; set; }
        public string description { get; set; }
        public bool? required { get; set; }
        public string defaultValue { get; set; }
    }
    public sealed class YamlReturn
    {
        public string name { get; set; }
        public List<string> type { get; set; }
        public string description { get; set; }
    }
    public sealed class YamlOverload
    {
        public string description { get; set; }
        public List<YamlParameter> parameters { get; set; }
    }

    sealed class ConstantYaml
    {
        public string name { get; set; }
        public string @enum { get; set; }
        public string value { get; set; }

        public string description { get; set; }
        public string summary { get; set; }

        public Dictionary<string, string> status { get; init; }
    }
    sealed class EnumYaml
    {
        public string name { get; set; }
        public string description { get; set; }
        public string summary { get; set; }
        public bool bitmaskInt { get; set; }
        public List<string> tags { get; set; }
    }
    sealed class NameSpaceYaml
    {
        public string name { get; set; }
        public string description { get; set; }
        public string summary { get; set; }
        public Dictionary<string, string> status { get; init; }
    }
    sealed class TagYaml
    {
        public string name { get; set; }
        public string description { get; set; }
        public string summary { get; set; }
        public List<SuggestedLink> suggestedLinks { get; set; }
    }
    public sealed class SuggestedLink
    {
        public string name { get; set; }
        public string link { get; set; }
    }
    public sealed class SymbolTypeInfo
    {
        public string Name { get; set; }
        public List<string> Returns { get; set; } = new();
    }


    public sealed class SymbolDescriptionModel
    {
        public SymbolKind Kind { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Namespace { get; init; } = string.Empty;
        public string Summary { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string SuperType { get; init; } = string.Empty;
        public List<SuggestedLink> Links { get; init; } = new();
        public string OwnerEnum { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public TypeYaml DeclaredType { get; init; }
        public List<SymbolOverload> Overloads { get; init; } = new();
        public List<string> Tags { get; init; } = new();
        public bool IsBitmask { get; init; } = false;
        public Dictionary<string, string> Status { get; init; }
    }
}
