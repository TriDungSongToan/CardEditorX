using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;
using CardEditor.Models;

namespace CardEditor.Editor.Hover
{
    public static class SymbolDescriptionDocumentBuilder
    {
        public static TextDocument Build(SymbolDescriptionModel model)
        {
            if (model == null) return new TextDocument();

            TextDocument textDocument = model.Kind switch
            {
                SymbolKind.Function => BuildFunction(model),
                SymbolKind.Constant => BuildConstant(model),
                SymbolKind.Enum => BuildEnum(model),
                SymbolKind.NameSpace => BuildNameSpace(model),
                SymbolKind.Tag => BuildTag(model),
                SymbolKind.Type => BuildType(model),
                _ => BuildConstant(model),
            };

            return textDocument;
        }
        private static TextDocument BuildFunction(SymbolDescriptionModel model)
        {
            if (model == null) return new TextDocument();

            var sb = new StringBuilder();

            sb.AppendLine($"{model.Kind} {model.Namespace}.{model.Name}");
            sb.AppendLine();

            var Signatures = BuildSignatures(model);
            foreach ( var signature in Signatures)
            {
                sb.AppendLine(signature);
            }

            if (!string.IsNullOrWhiteSpace(model.Description)) sb.AppendLine($"Description: {model.Description}");
            if (!string.IsNullOrWhiteSpace(model.Summary)) sb.AppendLine($"Summary: {model.Summary}");
            if (model.Status != null && model.Status.Count > 0)
            {
                sb.AppendLine("Status:");
                foreach (var statu in model.Status)
                {
                    sb.AppendLine($"{statu.Key}: {statu.Value}");
                }
            }
            return new TextDocument(sb.ToString());
        }
        private static TextDocument BuildConstant(SymbolDescriptionModel model)
        {
            if (model == null) return new TextDocument();

            var sb = new StringBuilder();

            sb.AppendLine($"{model.Name}");
            sb.AppendLine($"Enum: {model.OwnerEnum}");
            sb.AppendLine($"Value: {model.Value}");
            sb.AppendLine($"{model.Description}");
            sb.AppendLine($"Summary: {model.Summary}");

            if (model.Status != null && model.Status.Count > 0)
            {
                sb.AppendLine("Status:");
                foreach (var statu in model.Status)
                {
                    sb.AppendLine($"{statu.Key}: {statu.Value}");
                }
            }
            return new TextDocument(sb.ToString());
        }
        private static TextDocument BuildEnum(SymbolDescriptionModel model)
        {
            if (model == null) return new TextDocument();

            var sb = new StringBuilder();

            sb.AppendLine($"{model.Name}");
            sb.AppendLine($"{model.Description}");
            sb.AppendLine($"Summary: {model.Summary}");
            if (model.Tags != null && model.Tags.Count > 0)
            {
                sb.AppendLine($"Tags: {string.Join(", ", model.Tags)}");
            }
            if (model.Status != null && model.Status.Count > 0)
            {
                sb.AppendLine("Status:");
                foreach (var statu in model.Status)
                {
                    sb.AppendLine($"{statu.Key}: {statu.Value}");
                }
            }

            return new TextDocument(sb.ToString());
        }
        private static TextDocument BuildNameSpace(SymbolDescriptionModel model)
        {
            if (model == null) return new TextDocument();

            var sb = new StringBuilder();

            sb.AppendLine($"{model.Name}");
            sb.AppendLine($"{model.Description}");
            sb.AppendLine($"Summary: {model.Summary}");

            if (model.Status != null && model.Status.Count > 0)
            {
                sb.AppendLine("Status:");
                foreach (var statu in model.Status)
                {
                    sb.AppendLine($"{statu.Key}: {statu.Value}");
                }
            }
            if (model.Tags != null && model.Tags.Count > 0)
            {
                sb.AppendLine($"Tags: {string.Join(", ", model.Tags)}");
            }

            return new TextDocument(sb.ToString());
        }
        private static TextDocument BuildTag(SymbolDescriptionModel model)
        {
            if (model == null) return new TextDocument();

            var sb = new StringBuilder();

            sb.AppendLine($"{model.Name}");
            sb.AppendLine($"{model.Description}");
            sb.AppendLine($"Summary: {model.Summary}");
            
            if (model.Links != null && model.Links.Count > 0)
            {
                sb.AppendLine($"Suggested Links:");
                foreach (var link in model.Links)
                {
                    //sb.AppendLine($"{link.name}: {link.link}");
                    if (!string.IsNullOrWhiteSpace(link?.name) && !string.IsNullOrWhiteSpace(link.link))
                    {
                        sb.AppendLine($"[{link.name}]({link.link})");
                    }
                }
            }

            return new TextDocument(sb.ToString());
        }
        private static TextDocument BuildType(SymbolDescriptionModel model)
        {
            if (model == null) return new TextDocument();

            var sb = new StringBuilder();

            sb.AppendLine($"{model.Name}");
            sb.AppendLine(BuildTypeSignature(model));
            sb.AppendLine();
            if (!string.IsNullOrWhiteSpace(model.Description)) sb.AppendLine(model.Description);
            if (!string.IsNullOrWhiteSpace(model.Summary)) sb.AppendLine(model.Summary);
            if (!string.IsNullOrWhiteSpace(model.SuperType)) sb.AppendLine($"Super Type: {model.SuperType}");

            if (model.Status != null && model.Status.Count > 0)
            {
                sb.AppendLine();
                foreach (var kv in model.Status)
                {
                    sb.AppendLine($"{kv.Key}: {kv.Value}");
                }
            }
            if (model.Tags != null && model.Tags.Count > 0)
            {
                sb.AppendLine($"Tags: {string.Join(", ", model.Tags)}");
            }

            return new TextDocument(sb.ToString());
        }

        private static IReadOnlyList<string> BuildSignatures(SymbolDescriptionModel symbol)
        {
            if (symbol.Kind != SymbolKind.Function || symbol.Overloads == null) return Array.Empty<string>();

            return symbol.Overloads.Select(o => FormatSignature($"{symbol.Namespace}.{symbol.Name}", o)).ToList();
        }
        private static string FormatSignature(string fullName, SymbolOverload o)
        {
            var returnType = FormatReturnType(o);
            var parameters = (o.Parameters == null || o.Parameters.Count == 0)
                ? string.Empty : string.Join(", ", o.Parameters.Select(p => p.IsOptional ? $"[{p.Name}]" : p.Name));

            return $"{returnType} {fullName}({parameters});";
        }
        private static string FormatReturnType(SymbolOverload o)
        {
            if (o.Returns == null || o.Returns.Count == 0) return "void";

            var allTypes = o.Returns.SelectMany(r => r.Types).ToList();
            if (allTypes.Count == 1) return allTypes[0];
            return $"({string.Join(", ", allTypes)})";
        }

        private static string BuildTypeSignature(SymbolDescriptionModel model)
        {
            var name = model.Name;

            var parameters = model.DeclaredType?.parameters;
            if (parameters != null && parameters.Count > 0)
            {
                var paramText = string.Join(", ", parameters.Select(FormatTypeParameter));
                name += $"({paramText})";
            }

            if (!string.IsNullOrWhiteSpace(model.SuperType))
            {
                name += $" : {model.SuperType}";
            }

            return name;
        }
        private static string FormatTypeParameter(TypeParameter p)
        {
            var type = (p.type == null || p.type.Count == 0)
                ? "any"
                : string.Join(" | ", p.type);

            return $"{p.name}: {type}";
        }
    }
    public static class SymbolDescriptionBuilder
    {
        public static SymbolDescriptionModel Build(CompletionSymbol symbol)
        {
            if (symbol == null) return null;

            return new SymbolDescriptionModel
            {
                Kind = symbol.Kind,
                Name = symbol.Name,
                Namespace = symbol.Namespace,
                Summary = symbol.Summary,
                Description = symbol.Description,
                SuperType = symbol.supertype,
                Links = symbol.Links,
                OwnerEnum = symbol.OwnerEnum,
                Value = symbol.Value,
                DeclaredType = symbol.DeclaredType,
                Overloads = symbol.Overloads,
                Tags = symbol.Tags,
                IsBitmask = symbol.IsBitmask,
                Status = symbol.Status,
            };
        }

        private static IReadOnlyList<string> BuildSignatures(CompletionSymbol symbol)
        {
            if (symbol.Kind != SymbolKind.Function || symbol.Overloads == null)
                return Array.Empty<string>();

            return symbol.Overloads.Select(o =>
            {
                var parameters = o.Parameters == null
                    ? ""
                    : string.Join(", ", o.Parameters.Select(p =>
                    {
                        var optional = p.IsOptional ? "?" : "";
                        var types = p.Types != null && p.Types.Count > 0
                            ? string.Join("|", p.Types)
                            : "any";
                        return $"{p.Name}{optional}: {types}";
                    }));

                return $"function {symbol.Name}({parameters})";
            }).ToList();
        }
    }
}
