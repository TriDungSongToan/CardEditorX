using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.Generic;

namespace CardEditor.Manager
{
    public class ControlState
    {
        public object Control { get; set; }
        public object State { get; set; }
        public string ControlType { get; set; }
    }

    public interface ICommand
    {
        void Execute();
        void Undo();
    }

    public class CommandManager
    {
        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                var command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                var command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
            }
        }

        public void ResetUndoRedo()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
    }

    public class UndoRedoManager
    {
        private Stack<ControlState> _undoStack = new Stack<ControlState>();
        private Stack<ControlState> _redoStack = new Stack<ControlState>();

        // Lưu trạng thái trước khi thay đổi
        public void SaveState(object control, object state, string controlType)
        {
            _undoStack.Push(new ControlState { Control = control, State = state, ControlType = controlType });
            _redoStack.Clear(); // Xóa Redo khi có thay đổi mới
        }

        // Hàm Undo
        public void Undo()
        {
            if (_undoStack.Count == 0) return;

            var state = _undoStack.Pop();
            ApplyState(state);
            _redoStack.Push(CaptureCurrentState(state.Control, state.ControlType));
        }

        // Hàm Redo
        public void Redo()
        {
            if (_redoStack.Count == 0) return;

            var state = _redoStack.Pop();
            ApplyState(state);
            _undoStack.Push(CaptureCurrentState(state.Control, state.ControlType));
        }

        // Hàm ResetUndo
        public void ResetUndo()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        // Lấy trạng thái hiện tại của điều khiển
        private ControlState CaptureCurrentState(object control, string controlType)
        {
            object state = null;
            if (controlType == "TextBox" && control is TextBox textBox)
            {
                state = textBox.Text;
            }
            else if (controlType == "RichTextBox" && control is RichTextBox richTextBox)
            {
                TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                state = range.Text;
            }
            //else if (controlType == "MultiSelectComboBox" && control is Sdl.MultiSelectComboBox.Themes.Generic.MultiSelectComboBox multiSelectComboBox)
            //{
            //    // Sao chép danh sách SelectedItems
            //    state = multiSelectComboBox.SelectedItems?.Cast<object>().ToList();
            //}

            return new ControlState { Control = control, State = state, ControlType = controlType };
        }

        // Áp dụng trạng thái cho điều khiển
        private void ApplyState(ControlState state)
        {
            if (state.ControlType == "TextBox" && state.Control is TextBox textBox)
            {
                textBox.Text = state.State?.ToString();
            }
            else if (state.ControlType == "RichTextBox" && state.Control is RichTextBox richTextBox)
            {
                richTextBox.Document.Blocks.Clear();
                richTextBox.Document.Blocks.Add(new Paragraph(new Run(state.State?.ToString())));
            }
            //else if (state.ControlType == "MultiSelectComboBox" && state.Control is Sdl.MultiSelectComboBox.Themes.Generic.MultiSelectComboBox multiSelectComboBox)
            //{
            //    multiSelectComboBox.SelectedItems.Clear();
            //    if (state.State is List<object> selectedItems)
            //    {
            //        foreach (var item in selectedItems)
            //        {
            //            multiSelectComboBox.SelectedItems.Add(item);
            //        }
            //    }
            //}
        }
    }
}
