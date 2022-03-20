using DokoSharp.Lib;
using DokoTable.ViewModels;
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
using System.Windows.Threading;
using Serilog;

namespace DokoTable.Views;

internal class GameViewModelDT : GameViewModel
{
    public GameViewModelDT() : base(
        Dispatcher.CurrentDispatcher,
        new()
        {
            Client = new("0.0.0.0", 0),
            AccountName = "TestName",
            DialogService = new WpfWindowDialogService(),
            CardImageSet = new Dictionary<CardBase, BitmapImage>()
        })
    {
    }
}

/// <summary>
/// Interaction logic for GameView.xaml
/// </summary>
public partial class GameView : UserControl
{
    public GameView()
    {
        InitializeComponent();
    }

}

