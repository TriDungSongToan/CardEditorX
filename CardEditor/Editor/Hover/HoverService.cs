using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using CardEditor.Models;
using CardEditor.ViewModels;
using CardEditor.UserControls;

namespace CardEditor.Editor.Hover
{
    public sealed class HoverService
    {
        private readonly TextEditor _editor;
        private readonly ISymbolResolver _resolver;
        private Popup _popup;
        private Point _lastMousePosition;
        private CompletionSymbol _lastSymbol;
        private ScriptDescription _descriptionView;
        private ScriptDescriptionViewModel _descriptionVm;
        private CancellationTokenSource _hoverDelayCts;
        private CancellationTokenSource _closeDelayCts;

        public HoverService(TextEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _resolver = new SymbolResolver();

            var textView = _editor.TextArea.TextView;
            textView.MouseHover += OnMouseHover;
            textView.MouseHoverStopped += OnMouseHoverStopped;
            textView.MouseLeave += OnTextViewMouseLeave;
        }

        private void EnsurePopup()
        {
            if (_popup != null) return;

            _descriptionVm = new ScriptDescriptionViewModel();
            _descriptionView = new ScriptDescription
            {
                DataContext = _descriptionVm
            };

            _popup = new Popup
            {
                PlacementTarget = _editor.TextArea.TextView,
                Placement = PlacementMode.Relative,
                AllowsTransparency = true,
                StaysOpen = true,
                Child = _descriptionView
            };
            _popup.MouseEnter += OnPopupMouseEnter;
            _popup.MouseLeave += OnPopupMouseLeave;
            _popup.Closed += (s, e) =>
            {
                _lastSymbol = null;
            };
        }
        private void OnTextViewMouseLeave(object sender, MouseEventArgs e)
        {
            StartCloseDelay();
        }
        private void OnPopupMouseEnter(object sender, MouseEventArgs e)
        {
            _closeDelayCts?.Cancel();  // Keep open while mouse over popup
        }
        private void OnPopupMouseLeave(object sender, MouseEventArgs e)
        {
            StartCloseDelay();  // Start close timer when leave popup
        }


        private void ShowHover(int documentOffset, VisualLine visualLine)
        {
            var document = _editor.Document;
            var expression = ExpressionExtractor.Extract(document, documentOffset);
            if (string.IsNullOrEmpty(expression))
            {
                CloseToolTip();
                return;
            }

            var symbol = _resolver.ResolveExpression(expression);
            if (symbol == null)
            {
                CloseToolTip();
                return;
            }

            if (_lastSymbol == symbol && _popup?.IsOpen == true) return;
            CloseToolTip();
            _lastSymbol = symbol;

            EnsurePopup();
            var model = SymbolDescriptionBuilder.Build(symbol);
            var textDocument = SymbolDescriptionDocumentBuilder.Build(model);

            _descriptionVm.SetDocument(textDocument);
            var textView = _editor.TextArea.TextView;

            double popupY = visualLine.VisualTop + visualLine.Height - textView.ScrollOffset.Y + 2;
            double popupX = _lastMousePosition.X;

            _popup.HorizontalOffset = popupX;
            _popup.VerticalOffset = popupY;

            if (!_popup.IsOpen) _popup.IsOpen = true;
        }
        private void CloseToolTip()
        {
            if (_popup == null || !_popup.IsOpen) return;

            _descriptionVm.Clear();
            _popup.IsOpen = false;
        }

        private void OnMouseHover(object sender, MouseEventArgs e)
        {
            _closeDelayCts?.Cancel();
            _hoverDelayCts?.Cancel();

            var textView = _editor.TextArea.TextView;
            var position = e.GetPosition(textView);
            _lastMousePosition = position;
            if (position.X < 0 || position.Y < 0) return;

            textView.EnsureVisualLines();
            var visualLine = textView.GetVisualLineFromVisualTop(position.Y + textView.ScrollOffset.Y);
            if (visualLine == null) return;

            int visualColumn = visualLine.GetVisualColumn(position);
            int relativeOffset = visualLine.GetRelativeOffset(visualColumn);
            int documentOffset = visualLine.FirstDocumentLine.Offset + relativeOffset;

            StartHoverDelay(documentOffset, visualLine);
        }

        private void StartHoverDelay(int documentOffset, VisualLine visualLine)
        {
            _hoverDelayCts?.Cancel();
            _hoverDelayCts = new CancellationTokenSource();
            var token = _hoverDelayCts.Token;

            Task.Delay(300, token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                _editor.Dispatcher.Invoke(() =>
                {
                    CloseToolTip();
                    ShowHover(documentOffset, visualLine);
                });
            }, TaskScheduler.Default);
        }

        private void OnMouseHoverStopped(object sender, MouseEventArgs e)
        {
            StartCloseDelay();
        }
        private void StartCloseDelay()
        {
            if (_popup == null) return;
            _closeDelayCts?.Cancel();
            _closeDelayCts = new CancellationTokenSource();
            var token = _closeDelayCts.Token;

            Task.Delay(1000, token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;

                _editor.Dispatcher.Invoke(() =>
                {
                    if (_popup.IsMouseOver) return;
                    CloseToolTip();
                });
            });
        }
    }
}
