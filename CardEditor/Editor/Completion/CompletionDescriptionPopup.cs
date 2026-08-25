using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using CardEditor.Models;
using CardEditor.Editor.Analysis;
using CardEditor.ViewModels;
using CardEditor.UserControls;

namespace CardEditor.Editor.Completion
{
    internal sealed class CompletionDescriptionPopup
    {
        private readonly Popup _popup;
        private readonly ScriptDescription _descriptionView;
        private readonly ScriptDescriptionViewModel _viewModel;
        public event EventHandler CloseRequested;

        public CompletionDescriptionPopup(UIElement placementTarget)
        {
            _viewModel = new ScriptDescriptionViewModel();
            _descriptionView = new ScriptDescription
            {
                DataContext = _viewModel
            };
            _descriptionView.CloseRequested += (_, __) => CloseRequested?.Invoke(this, EventArgs.Empty);

            _popup = new Popup
            {
                PlacementTarget = placementTarget,
                Placement = PlacementMode.Relative,
                AllowsTransparency = true,

                // Quan trọng: Popup riêng này phải nhận input.
                StaysOpen = true,

                Child = _descriptionView
                //Child = new ScriptDescription
                //{
                //    DataContext = _viewModel
                //}
            };
        }
        public bool IsOpen => _popup.IsOpen;

        public void Show(CompletionSymbol symbol, Point position)
        {
            if (symbol == null)
            {
                Hide();
                return;
            }

            var model = SymbolDescriptionModelFactory.From(symbol);
            _viewModel.SetDocument(
                Hover.SymbolDescriptionDocumentBuilder.Build(model));

            _popup.HorizontalOffset = position.X;
            _popup.VerticalOffset = position.Y;
            _popup.IsOpen = true;
        }

        public void Hide()
        {
            _viewModel.Clear();
            _popup.IsOpen = false;
        }
    }
}
