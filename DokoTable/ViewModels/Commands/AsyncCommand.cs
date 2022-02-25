using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DokoTable.ViewModels.Commands;

public class AsyncCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public AsyncCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute();
    }

    public Task ExecuteAsync()
    {
        return _execute();
    }

    public async void Execute(object? parameter)
    {
        await ExecuteAsync();
    }

    //protected void RaiseCanExecuteChanged()
    //{
    //    CommandManager.InvalidateRequerySuggested();
    //}
}

