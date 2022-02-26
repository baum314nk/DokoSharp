using DokoTable.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DokoTable.ViewModels.WindowDialogService;

/// <summary>
/// A window dialog service that provides support for WPF dialogs.
/// </summary>
public class WpfWindowDialogService : IWindowDialogService
{
    public Type? ChoiceControlType { get; set; }
    public Type? YesNoControlType { get; set; }

    public bool ShowChoiceDialog<T>(string title, IEnumerable<T> choices, out T result)
    {
        if (ChoiceControlType == null) throw new Exception("Can't show a choice dialog because no choice control is specified.");

        var choiceControl = (IChoiceControl)Activator.CreateInstance(ChoiceControlType)!;
        choiceControl.Title = title;
        choiceControl.Choices = choices.Cast<object>();

        var dialogResult = ShowDialog(title, choiceControl);

        result = (T)choiceControl.SelectedChoice!;
        return dialogResult;
    }

    public bool ShowYesNoDialog(string title, out bool result)
    {
        if (YesNoControlType == null) throw new Exception("Can't show a yes-no dialog because no yes-no control is specified.");

        var yesNoControl = (IYesNoControl)Activator.CreateInstance(YesNoControlType)!;
        yesNoControl.Title = title;

        var dialogResult = ShowDialog(title, yesNoControl);

        result = yesNoControl.IsYes;
        return dialogResult;
    }

    public bool ShowDialog(string title, object content)
    {
        var window = new WindowDialog
        {
            Title = title,
            DataContext = content,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Application.Current.MainWindow,
        };

        // Return null if dialog was canceled
        return window.ShowDialog() ?? false;
    }
}
