using DokoSharp.Lib;
using DokoTable.ViewModels.WindowDialogService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Serilog;
using System.Windows.Threading;
using DokoTable.Models;
using System.Windows.Input;
using DokoTable.ViewModels.Commands;
using DokoTable.Views;

namespace DokoTable.ViewModels;

/// <summary>
/// The main viewmodel of the application.
/// </summary>
public class MainViewModel : BaseViewModel, IDisposable
{
    #region Fields

    private GameViewModel? _gameViewModel = null;
    private IReadOnlyDictionary<CardBase, BitmapImage>? _cardImageSet = null;
    private DokoTcpClient? _client = null;
    private string? _accountName = null;

    private bool disposedValue = false;

    #endregion

    #region Properties

    /// <summary>
    /// The dialog service used to show dialogs.
    /// </summary>
    public IWindowDialogService? DialogService { get; set; }

    /// <summary>
    /// The client instance that handles the connection to the Doko server.
    /// </summary>
    public DokoTcpClient? Client
    {
        get => _client;
        internal set
        {
            _client = value;
            RaisePropertyChanged(nameof(Client));
        }
    }
    
    /// <summary>
    /// The name of the account currently connected to the server.
    /// Is null if the client isn't connected.
    /// </summary>
    public string? AccountName
    {
        get => _accountName;
        internal set
        {
            _accountName = value;
            RaisePropertyChanged(nameof(AccountName));
        }
    }

    /// <summary>
    /// A mapping from card identifiers to images.
    /// </summary>
    public IReadOnlyDictionary<CardBase, BitmapImage>? CardImageSet
    {
        get => _cardImageSet;
        internal set
        {
            _cardImageSet = value;
            RaisePropertyChanged(nameof(CardImageSet));
        }
    }

    #region Sub-ViewModels

    public ImageSettingsViewModel ImageSettingsViewModel { get; private set; }

    public ConnectionViewModel ConnectionViewModel { get; private set; }

    public GameViewModel? GameViewModel
    {
        get => _gameViewModel;
        private set
        {
            _gameViewModel = value;
            RaisePropertyChanged(nameof(GameViewModel));
        }
    }

    #endregion

    #endregion

    #region Commands

    public ICommand ShowConnectionDialogCommand { get; init; }
    private void DoShowConnectionView()
    {
        DialogService!.ShowDialog("Connection", new ConnectionView() { DataContext = ConnectionViewModel }, false);
    }

    public ICommand ShowImageDialogCommand { get; init; }
    private void DoShowImageDialogCommand()
    {
        DialogService!.ShowDialog("Image Settings", new ImageSettingsView() { DataContext = ImageSettingsViewModel }, false);
    }

    #endregion

    #region Constructor

    public MainViewModel() : base(Dispatcher.CurrentDispatcher)
    {
        ImageSettingsViewModel = new(_dispatcher);
        ImageSettingsViewModel.PropertyChanged += ImageViewModel_PropertyChanged;

        ConnectionViewModel = new(_dispatcher);
        ConnectionViewModel.PropertyChanged += ConnectionViewModel_PropertyChanged;

        ShowImageDialogCommand = new Command(DoShowImageDialogCommand);
        ShowConnectionDialogCommand = new Command(DoShowConnectionView);
    }

    #endregion

    #region Event Handlers

    private void ImageViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ImageSettingsViewModel.CardImageSet))
        {
            CardImageSet = ImageSettingsViewModel.CardImageSet;
        }
    }

    private void ConnectionViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch(e.PropertyName)
        {
            case nameof(ConnectionViewModel.Client):
                Client = ConnectionViewModel.Client;
                break;
            case nameof(ConnectionViewModel.AccountName):
                AccountName = ConnectionViewModel.AccountName;
                if (AccountName is not null)
                {
                    GameViewModel = new(_dispatcher, this);
                    GameViewModel.StartCommand.Execute(null);
                }
                break;
        }        
    }

    private void GameViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        //if (e.PropertyName == nameof(GameViewModel.GameState))
        //{
        //    GameState = GameViewModel!.GameState;
        //}
    }

    #endregion

    #region Overrides & Implements

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                ConnectionViewModel.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
