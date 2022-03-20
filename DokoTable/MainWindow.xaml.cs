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
using Serilog;
using DokoTable.ViewModels;

namespace DokoTable;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainViewModel MainContext => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Setup logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug(Serilog.Events.LogEventLevel.Debug)
            .WriteTo.RichTextBox(txtLog, Serilog.Events.LogEventLevel.Information)
            .CreateLogger();

        // Load default card images
        MainContext.ImageSettingsViewModel.LoadDefaultImageSetCommand.Execute(null);
        // Show connection dialog
        MainContext.ShowConnectionDialogCommand.Execute(null);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        MainContext.Dispose();
    }
}
