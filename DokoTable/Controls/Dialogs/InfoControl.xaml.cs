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
/// Interaction logic for InfoControl.xaml
/// </summary>
public partial class InfoControl : UserControl, IInfoControl
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
            typeof(InfoControl),
            new PropertyMetadata(null)
        );

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(InfoControl),
            new PropertyMetadata(null)
        );

    public InfoControl()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        var window = Window.GetWindow(this);

        window.DialogResult = true;

        window.Close();
    }
}
