using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Serilog;

namespace DokoTable.ViewModels;

/// <summary>
/// A basic implementation of the IViewModel interface.
/// </summary>
public abstract class BaseViewModel : IViewModel
{
    #region Fields

    protected readonly Dispatcher _dispatcher;

    #endregion

    protected BaseViewModel(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    #region Overrides & Implementations

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged(string property)
    {
        Log.Debug($"Property {property} of DokoViewModel changed.");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
    public bool IsSynchronized => throw new NotImplementedException();

    public void Invoke(Action action)
    {
        _dispatcher.Invoke(action);
    }

    public void BeginInvoke(Action action)
    {
        _dispatcher.BeginInvoke(action);
    }

    #endregion
}
