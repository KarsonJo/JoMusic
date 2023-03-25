using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JoMusicCenter.Commands
{
    //get from here：https://stackoverflow.com/questions/1468791/icommand-mvvm-implementation
    //async version: https://johnthiriet.com/mvvm-going-async-with-async-command/
    //: https://johnthiriet.com/removing-async-void/
    public class RelayCommand : IAsyncCommand
    {
        private readonly Predicate<object?>? _canExecute;
        private readonly Action<object?>? _execute;
        private readonly Func<object?, Task>? _task;
        private bool _isExecuting;
        private readonly IErrorHandler? _errorHandler;

        public RelayCommand(Predicate<object?>? canExecute, Action<object?> execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public RelayCommand(Predicate<object?>? canExecute, Func<object?,Task> task, IErrorHandler? errorHandler = null)
        {
            _canExecute = canExecute;
            _task = task;
            _errorHandler = errorHandler;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute(parameter));
        }

        public void Execute(object? parameter)
        {
            if (_execute != null)
            {
                _execute(parameter);
            }
            else
            {
                ExecuteAsync(parameter).FireAndForgetSafeAsync(_errorHandler);
            }
        }

        public async Task ExecuteAsync(object? parameter)
        {
            if (_task == null)
            {
                return;
            }
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    await _task(parameter);
                }
                finally
                {
                    _isExecuting = false;
                }
            }
        }
    }
}
