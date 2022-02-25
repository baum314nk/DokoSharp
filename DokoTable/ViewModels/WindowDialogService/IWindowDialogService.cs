using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoTable.ViewModels.WindowDialogService;

public interface IWindowDialogService
{
    /// <summary>
    /// Shows a window dialog and returns true if the dialog result was set.
    /// The result is returned via an out parameter.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="title"></param>
    /// <param name="datacontext"></param>
    /// <returns></returns>
    bool ShowDialog(string title, object content);

    bool ShowChoiceDialog<T>(string title, IEnumerable<T> choices, out T result);

    bool ShowYesNoDialog(string title, out bool result);
}

public interface IDialogControl
{
    public string? Title { set; }
}

public interface IChoiceControl : IDialogControl
{
    public IEnumerable<object>? Choices { set; }

    public object? SelectedChoice { get; }
}

public interface IYesNoControl : IDialogControl
{
    public bool IsYes { get; }
}

