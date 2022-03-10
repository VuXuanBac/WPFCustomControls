using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TestControl
{
public class RelayCommand<T> : ICommand
{
    private readonly Predicate<T> _can_execute;
    private readonly Action<T> _execute;

    public RelayCommand(Action<T> execute, Predicate<T> can_execute = null)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");
        _execute = execute;
        _can_execute = can_execute;
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
        return _can_execute == null ? true : _can_execute((T)parameter);
    }

    public void Execute(object parameter)
    {
        _execute((T)parameter);
    }
}
 
}
