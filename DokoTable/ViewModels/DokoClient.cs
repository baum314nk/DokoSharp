using DokoSharp.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using DokoSharp.Lib.Messaging;
using System.Text.Json;
using System.Threading;
using System.IO;

namespace DokoTable.ViewModels;

public class DokoClient : IDisposable
{
    #region Fields

    protected TcpClient? _client;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private bool disposedValue;

    private readonly AutoResetEvent _cardSelectedEvent = new(false);

    private CardBase? _selectedCard = null;

    #endregion

    #region Properties

    /// <summary>
    /// The endpoint of the server the client should connect to.
    /// </summary>
    public IPEndPoint ServerEndpoint { get; set; }

    /// <summary>
    /// A flag that is true when the server is running and false otherwise.
    /// </summary>
    public bool IsRunning { get; set; } = false;

    public CardBase? SelectedCard
    {
        get => _selectedCard;
        set
        {
            if (_selectedCard == value) return;

            _selectedCard = value;
            if (_selectedCard != null) _cardSelectedEvent.Set();
        }
    }

    #endregion

    #region Events

    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// The received message.
        /// </summary>
        public Message Message { get; init; }

        public MessageEventArgs(Message msg)
        {
            Message = msg;
        }

    }
    public delegate void MessageEventHandler(object sender, MessageEventArgs e);
    public event MessageEventHandler? MessageReceived;
    public event MessageEventHandler? MessageSent;

    #endregion

    public DokoClient(string ipAddress, int port)
    {
        ServerEndpoint = new(IPAddress.Parse(ipAddress), port);
    }

    /// <summary>
    /// Starts the Doko server instance and waits for incoming connections.
    /// </summary>
    public void Start()
    {
        if (IsRunning)
        {
            Log.Debug("The client is already running. Aborting.");
            return;
        }

        Log.Information("Connecting to the Doko server on {ip}:{port}", ServerEndpoint.Address, ServerEndpoint.Port);
        _client = new();
        try
        {
            _client.Connect(ServerEndpoint);
        } catch(SocketException ex)
        {
            Log.Error("Error while trying to connect to the server: {error}", ex.Message);
            return;
        }
        _stream = _client.GetStream();
        _reader = new(_stream);
        _writer = new(_stream);
        IsRunning = true;
        Log.Information("Connection successful.");

        Task.Run(MessageLoop);
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            Log.Debug("The client isn't running. Aborting.");
            return;
        }

        Log.Information("Stopping the client.");
        IsRunning = false;
        _client!.Close();
        _client = null;
    }

    /// <summary>
    /// Send a generic message via TCP to the server.
    /// </summary>
    /// <param name="msg"></param>
    public void SendMessage(Message msg)
    {
        string msgJson = JsonSerializer.Serialize(msg, Utils.DefaultJsonOptions);
        _writer!.WriteLine(msgJson);
        _writer.Flush();

        Log.Debug("Message sent: {msg}", JsonSerializer.Serialize(msg, Utils.BeautifyJsonOptions));
        MessageSent?.Invoke(this, new(msg));
    }

    #region Overrides & Implementations

    protected void MessageLoop()
    {
        while (IsRunning)
        {
            Message msg = ReceiveMessage<Message>();

            switch (msg)
            {
                case RequestCardMessage:
                    HandleRequestCard();
                    break;
                case RequestPlaceCardMessage:
                    HandleRequestCard();
                    break;
            }
        }

    }

    /// <summary>
    /// Waits for a generic message via TCP from the server.
    /// Blocks the current thread until a valid message is received.
    /// </summary>
    /// <param name="msg"></param>
    protected T ReceiveMessage<T>() where T : Message
    {
        while (true)
        {
            string? msgJson = _reader!.ReadLine();
            if (msgJson == null) throw new EndOfStreamException();

            Message? msg = JsonSerializer.Deserialize<Message>(msgJson, Utils.DefaultJsonOptions);
            if (msg != null && msg is T t)
            {
                Log.Debug("Message received: {msg}", JsonSerializer.Serialize(msg, Utils.BeautifyJsonOptions));
                MessageReceived?.Invoke(this, new(msg));
                return t;
            }
            else
            {
                Log.Error("Encountered invalid message. Expected message of type {type}.", typeof(T).Name);
            }
        }
    }

    private void HandleRequestCard()
    {
        // Wait until card is selected
        Log.Information("Waiting until a valid card is selected.");
        _cardSelectedEvent.WaitOne();

        SendMessage(new ReplyCardMessage()
        {
            CardIdentifier = SelectedCard!.Identifier
        });
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _client?.Dispose();
                _client = null;
                _stream = null;
                _reader = null;
                _writer = null;
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
