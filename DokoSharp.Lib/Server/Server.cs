using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DokoSharp.Lib.Server;

/// <summary>
/// 
/// </summary>
public class Server : IDisposable
{
    #region Fields

    private readonly TcpListener _server;
    private bool disposedValue;
    private List<TcpController> _connections;

    private AutoResetEvent _gameReadyEvent = new(false);

    #endregion

    #region Properties

    /// <summary>
    /// The active connections of the server.
    /// </summary>
    public IReadOnlyList<TcpController> Connections => _connections;

    /// <summary>
    /// A flag that is true when the server is running and false otherwise.
    /// </summary>
    public bool IsRunning { get; set; } = false;

    /// <summary>
    /// The local endpoint the server is bound to.
    /// </summary>
    public IPEndPoint LocalEndpoint => (IPEndPoint)_server.LocalEndpoint;

    public Game? Game { get; protected set; }

    #endregion

    public Server(string ipAddress = "127.0.0.1", int port = 1234)
    {
        _server = new(IPAddress.Parse(ipAddress), port);
        _connections = new();
    }

    /// <summary>
    /// Starts the Doko server instance and waits for incoming connections.
    /// </summary>
    public void Start()
    {
        if (IsRunning)
        {
            Log.Debug("The server is already running. Aborting.");
            return;
        }

        Log.Information("Starting the Doko server on {ip}:{port}", LocalEndpoint.Address, LocalEndpoint.Port);
        _server.Start();
        IsRunning = true;

        // Start connection loop
        Task.Run(ConnectionLoop);

        // Wait until game is ready
        _gameReadyEvent.WaitOne();

        // Start game
        Log.Information("Enough player joined, starting the game.");
        Game = new(new Tuple<string, IPlayerController>[]
        {
            new("player1", _connections[0]),
            new("player2", _connections[1]),
            new("player3", _connections[2]),
            new("player4", _connections[3]),
        }, SpecialRule.GetDefaults());

        Log.Information("Stopping the server");
        _server.Stop();
        IsRunning = false;
    }

    protected void ConnectionLoop()
    {
        // Wait for enough players
        while (IsRunning && Connections.Count < 4)
        {
            Log.Information("Waiting for {diff} more players to start the game.", 4 - Connections.Count);
            var client = _server.AcceptTcpClient();

            _connections.Add(new TcpController(client));
        }

        if (Connections.Count == 4)
        {
            _gameReadyEvent.Set();
        }
    }

    #region Overrides & Implementations

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _server?.Stop();
            }

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
