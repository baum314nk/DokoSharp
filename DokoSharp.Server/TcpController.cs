using DokoSharp.Lib;
using DokoSharp.Lib.Messaging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DokoSharp.Server;

/// <summary>
/// A player controller that receives input via a TCP connection.
/// </summary>
public class TcpController : IPlayerController, IDisposable
{
    #region Fields

    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private bool disposedValue;

    #endregion

    public bool IsConnected => _client?.Connected ?? false;

    public TcpController(TcpClient tcpClient)
    {
        _client = tcpClient;
        _stream = tcpClient.GetStream();
        _reader = new(_stream);
        _writer = new(_stream);
    }

    public void SignalReceivedCards(Player player, IEnumerable<Card> receivedCards)
    {
        // Send message
        SendMessage(new ReceivedCardsMessage()
        {
            ReceivedCards = receivedCards.Select(c => c.Base.Identifier).ToArray()
        });
    }

    public Card RequestPlaceCard(Player player, Trick trick)
    {
        // Send request
        SendMessage(new RequestPlaceCardMessage()
        {
            TrickCards = trick.PlacedCards.Select(c => c.Base.Identifier).ToArray(),
        });

        // Wait for reply
        var reply = ReceiveMessage<ReplyCardMessage>();

        return player.Cards.First(c => c.Base.Identifier == reply.CardIdentifier!);
    }

    public Card RequestCard(Player player)
    {
        // Send request
        SendMessage(new RequestCardMessage());

        // Wait for reply
        var reply = ReceiveMessage<ReplyCardMessage>();

        return player.Cards.First(c => c.Base.Identifier == reply.CardIdentifier!);
    }

    public Reservation? RequestReservation(Player player)
    {
        return null;
    }

    public bool RequestYesNo(Player player, string requestText)
    {
        return false;
    }

    /// <summary>
    /// Send a generic message via TCP to the client.
    /// </summary>
    /// <param name="msg"></param>
    public void SendMessage(Message msg)
    {
        string msgJson = JsonSerializer.Serialize(msg, Utils.DefaultJsonOptions);
        _writer.WriteLine(msgJson);
        _writer.Flush();
    }

    /// <summary>
    /// Waits for a generic message via TCP from the client.
    /// Blocks the current thread until a valid message is received.
    /// </summary>
    /// <param name="msg"></param>
    public T ReceiveMessage<T>() where T : Message
    {
        while (true)
        {
            string? msgJson = _reader.ReadLine();
            if (msgJson == null) throw new EndOfStreamException();

            Message? msg = JsonSerializer.Deserialize<Message>(msgJson, Utils.DefaultJsonOptions);
            if (msg != null && msg is T t) return t;
            else
            {
                Log.Error("Encountered invalid message. Expected message of type {type}.", typeof(T).Name);
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _reader.Dispose();
                _writer.Dispose();
                _client.Dispose();
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
}
