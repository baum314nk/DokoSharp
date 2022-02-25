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
/// Interaction logic for YesNoControl.xaml
/// </summary>
public partial class YesNoControl : UserControl, IYesNoControl
{    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(YesNoControl),
            new PropertyMetadata(null)
        );

    public bool IsYes { get; set; }

    public YesNoControl()
    {
        InitializeComponent();
    }

    private void YesButton_Click(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);

        IsYes = true;
        window.DialogResult = true;

        window.Close();
    }

    private void NoButton_Click(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);

        IsYes = false;
        window.DialogResult = false;

        window.Close();
    }
}
