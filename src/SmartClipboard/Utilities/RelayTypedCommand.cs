using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SmartClipboard.Utilities
{
    internal class RelayTypedCommand<T> : ICommand
    {
        private readonly Action<T> _execute;

        public RelayTypedCommand(Action<T> execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter is T t) _execute(t);
        }
    }
}
