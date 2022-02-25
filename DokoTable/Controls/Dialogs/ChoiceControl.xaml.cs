using DokoTable.ViewModels.WindowDialogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DokoTable.Controls.Dialogs;

/// <summary>
/// Interaction logic for ChoiceControl.xaml
/// </summary>
public partial class ChoiceControl : UserControl, IChoiceControl
{
    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(ChoiceControl),
            new PropertyMetadata(null)
        );

    public IEnumerable<object>? Choices
    {
        get => (IEnumerable<object>?)GetValue(ChoicesProperty);
        set => SetValue(ChoicesProperty, value);
    }
    public static readonly DependencyProperty ChoicesProperty =
        DependencyProperty.Register(
            "Choices",
            typeof(IEnumerable<object>),
            typeof(ChoiceControl),
            new PropertyMetadata(null)
        );

    public object? SelectedChoice
    {
        get => GetValue(SelectedChoiceProperty);
        set => SetValue(SelectedChoiceProperty, value);
    }
    public static readonly DependencyProperty SelectedChoiceProperty =
        DependencyProperty.Register(
            "SelectedChoice",
            typeof(object),
            typeof(ChoiceControl),
            new PropertyMetadata(null)
        );


    public ChoiceControl()
    {
        InitializeComponent();
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);

        window.DialogResult = true;

        window.Close();
    }
}
