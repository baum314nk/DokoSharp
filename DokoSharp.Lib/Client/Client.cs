using DokoSharp.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace DokoSharp.Lib.Client;

/// <summary>
/// 
/// </summary>
public class Client : IDisposable
{
    #region Fields

    protected TcpClient? _client;
    private bool disposedValue;

    #endregion

    #region Properties

    /// <summary>
    /// A flag that is true when the server is running and false otherwise.
    /// </summary>
    public bool IsRunning { get; set; } = false;

    #endregion

    public Client()
    {

    }

    /// <summary>
    /// Starts the Doko server instance and waits for incoming connections.
    /// </summary>
    public void Start(string ipAddress = "127.0.0.1", int port = 1234)
    {
        if (IsRunning)
        {
            Log.Debug("The server is already running. Aborting.");
            return;
        }

        Log.Information("Connecting to the Doko server on {ip}:{port}", ipAddress, port);
        _client = new(ipAddress, port);
        IsRunning = true;
        Log.Information("Connection successful.");

        // Wait for enough players
        while (IsRunning)
        {
            Log.Information("Waiting for messages from the server.");
        }

        Log.Information("Stopping the server");
        _client.Close();
        _client = null;
        IsRunning = false;
    }

    #region Overrides & Implementations

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _client?.Dispose();
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
