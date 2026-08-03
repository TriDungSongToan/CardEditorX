using System;
using System.Windows;
using CardEditor.Models;
using CardEditor.Editor.Hover;
using CardEditor.ViewModels;
using CardEditor.UserControls;

namespace CardEditor.Editor.Analysis
{
    public interface ISymbolDescriptionPresenter
    {
        FrameworkElement Create(CompletionSymbol symbol);
    }
    public sealed class ScriptDescriptionPresenter : ISymbolDescriptionPresenter
    {
        public FrameworkElement Create(CompletionSymbol symbol)
        {
            if (symbol == null) return null;

            // 1. Map CompletionSymbol → SymbolDescriptionModel
            var model = SymbolDescriptionModelFactory.From(symbol);

            // 2. Build TextDocument
            var document = SymbolDescriptionDocumentBuilder.Build(model);

            // 3. Create ViewModel (KHÔNG dùng constructor)
            var vm = new ScriptDescriptionViewModel();
            vm.SetDocument(document);

            // 4. Bind vào View
            return new ScriptDescription
            {
                DataContext = vm
            };
        }
    }
    public static class SymbolDescriptionModelFactory
    {
        public static SymbolDescriptionModel From(CompletionSymbol symbol)
        {
            if (symbol == null) throw new ArgumentNullException(nameof(symbol));
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
    }
}
