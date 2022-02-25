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
using System.Collections.Concurrent;
using System.Diagnostics;

namespace DokoTable.ViewModels;

public class DokoClient : IDisposable
{
    #region Fields

    protected TcpClient? _client;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private bool disposedValue;

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

    /// <summary>
    /// The collection used to communicate the reply values between tasks.
    /// </summary>
    public ConcurrentDictionary<string, object?> ReplyValues { get; init; }

    /// <summary>
    /// The collection used to communicate the reply values between tasks.
    /// </summary>
    public AutoResetEvent ReplyValuesUpdated { get; init; }

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
        ReplyValues = new(2, new []
        {
            new KeyValuePair<string, object?>("PlaceCard", null),
            new KeyValuePair<string, object?>("Cards", null),
            new KeyValuePair<string, object?>("Color", null),
            new KeyValuePair<string, object?>("YesNo", null),
            new KeyValuePair<string, object?>("Reservation", null),
        }, null);
        ReplyValuesUpdated = new(false);
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
            Message message = ReceiveMessage<Message>();

            switch (message)
            {
                case RequestPlaceCardMessage:
                    HandleRequestPlaceCard();
                    break;
                case RequestCardsMessage msg:
                    HandleRequestCards(msg.Amount);
                    break;
                case RequestColorMessage:
                    HandleRequestColor();
                    break;
                case RequestYesNoMessage:
                    HandleRequestYesNo();
                    break;
                case RequestReservationMessage msg:
                    HandleRequestReservation(msg.Possibilities!);
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
                Log.Debug("Message received:\n{msg}", JsonSerializer.Serialize(msg, Utils.BeautifyJsonOptions));
                MessageReceived?.Invoke(this, new(msg));
                return t;
            }
            else
            {
                Log.Error("Encountered invalid message. Expected message of type {type}.", typeof(T).Name);
            }
        }
    }

    private void HandleRequestPlaceCard()
    {

        CardBase? placedCard = ReplyValues["PlaceCard"] as CardBase;
        // If value is unset, we wait until it gets set
        while (placedCard == null)
        {
            ReplyValuesUpdated.WaitOne();
            placedCard = ReplyValues["PlaceCard"] as CardBase;
        }
        ReplyValues["PlaceCard"] = null;

        SendMessage(new ReplyCardsMessage()
        {
            CardIdentifiers = new[] { placedCard.Identifier }
        });
    }

    private void HandleRequestCards(int amount)
    {
        IList<CardBase>? cards = ReplyValues["Cards"] as IList<CardBase>;
        // If value is unset, we wait until it gets set and amount matches
        while (cards == null || cards.Count != amount)
        {
            ReplyValuesUpdated.WaitOne();
            cards = ReplyValues["Cards"] as IList<CardBase>;
        }
        ReplyValues["Cards"] = null;

        SendMessage(new ReplyCardsMessage()
        {
            CardIdentifiers = cards.ToIdentifiers().ToArray()
        });
    }

    private void HandleRequestColor()
    {
        CardColor? color = ReplyValues["Color"] as CardColor?;
        // If value is unset, we wait until it gets set
        while (color == null)
        {
            ReplyValuesUpdated.WaitOne();
            color = ReplyValues["Color"] as CardColor?;
        }
        ReplyValues["Color"] = null;

        SendMessage(new ReplyColorMessage()
        {
            Color = (CardColor)color
        });
    }

    private void HandleRequestYesNo()
    {
        bool? isYes = ReplyValues["YesNo"] as bool?;
        // If value is unset, we wait until it gets set
        while (isYes == null)
        {
            ReplyValuesUpdated.WaitOne();
            isYes = ReplyValues["YesNo"] as bool?;
        }
        ReplyValues["YesNo"] = null;

        SendMessage(new ReplyYesNoMessage()
        {
            IsYes = (bool)isYes
        });
    }

    private void HandleRequestReservation(IEnumerable<string> possibilities)
    {
        string? reservation = ReplyValues["Reservation"] as string;
        // If value is unset, we wait until it gets set
        while (reservation == null || !(reservation == string.Empty || possibilities.Contains(reservation)))
        {
            ReplyValuesUpdated.WaitOne();
            reservation = ReplyValues["Reservation"] as string;
        }
        ReplyValues["Reservation"] = null;

        SendMessage(new ReplyReservationMessage()
        {
            ReservationName = (reservation == string.Empty) ? null : reservation,
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
