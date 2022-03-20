using DokoTable.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace DokoTable.ViewModels;

/// <summary>
/// A viewmodel that handles the connection with the Doko server.
/// </summary>
public class ConnectionViewModel : BaseViewModel, IDisposable
{
    #region Fields

    private string _serverHostname = "127.0.0.1";
    private int _serverPort = 1234;
    private DokoTcpClient? _client;
    private string? _accountName = null;
    private bool disposedValue;

    #endregion

    #region Properties

    /// <summary>
    /// The hostname of the server to connect to.
    /// </summary>
    public string ServerHostname
    {
        get => _serverHostname;
        set
        {
            if (value == _serverHostname) return;

            _serverHostname = value;
            RaisePropertyChanged(nameof(ServerHostname));
        }
    }

    /// <summary>
    /// The port of the server to connect to.
    /// </summary>
    public string ServerPort
    {
        get => _serverPort.ToString();
        set
        {
            if (value == ServerPort) return;

            _serverPort = int.Parse(value);
            RaisePropertyChanged(nameof(ServerPort));
        }
    }

    /// <summary>
    /// The client instance that handles the connection to the Doko server.
    /// </summary>
    public DokoTcpClient? Client
    {
        get => _client;
        set
        {
            _client = value;
            RaisePropertyChanged(nameof(Client));
        }
    }

    /// <summary>
    /// The name of the account currently logged in on the server.
    /// Is null if the client isn't connected.
    /// </summary>
    public string? AccountName
    {
        get => _accountName;
        set
        {
            _accountName = value;
            RaisePropertyChanged(nameof(AccountName));
        }
    }

    #endregion

    #region Commands

    public ICommand ConnectCommand { get; init; }
    private void DoConnect()
    {
        var client = new DokoTcpClient(ServerHostname, _serverPort);
        try
        {
            client.Connect();
            Client = client;
        }
        catch (SocketException)
        {
            client.Dispose();
        }
    }

    public ICommand DisconnectCommand { get; init; }
    private void DoDisconnect()
    {
        Client?.Close();
        Client = null;
    }

    #endregion

    #region Constructors

    public ConnectionViewModel(Dispatcher dispatcher) : base(dispatcher)
    {
        ConnectCommand = new Command(DoConnect, () => _client == null);
        DisconnectCommand = new Command(DoDisconnect, () => _client != null);
    }

    #endregion

    #region Overrides & Implements

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _client?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ConnectionViewModel()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}