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
    public DokoViewModel DokoContext => (DokoViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Setup logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug(Serilog.Events.LogEventLevel.Debug)
            .WriteTo.RichTextBox(txtLog, Serilog.Events.LogEventLevel.Debug)
            .CreateLogger();

        // Load default card images
        DokoContext.LoadDefaultImageSetCommand.Execute(null);
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        DokoContext.Dispose();
    }
}
