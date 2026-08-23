using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace CardEditor.Commands
{
    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Func<object, Task> _executeAsync;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(
            Func<object, Task> executeAsync,
            Func<object, bool> canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public RelayCommand(
            Action<object> execute,
            Func<object, bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _executeAsync = param =>
            {
                execute(param);
                return Task.CompletedTask;
            };
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public async void Execute(object parameter) => await _executeAsync(parameter);

        public event EventHandler CanExecuteChanged;

        //public event EventHandler CanExecuteChanged
        //{
        //    add => CommandManager.RequerySuggested += value;
        //    remove => CommandManager.RequerySuggested -= value;
        //}

        //public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
