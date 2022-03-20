using DokoTable.ViewModels;
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
using System.Windows.Threading;

namespace DokoTable.Views;
internal class ImageViewModelDT : ImageSettingsViewModel
{
    public ImageViewModelDT() : base(Dispatcher.CurrentDispatcher)
    {
    }
}


/// <summary>
/// Interaction logic for ImageView.xaml
/// </summary>
public partial class ImageSettingsView : UserControl
{
    public ImageSettingsView()
    {
        InitializeComponent();
    }
}
