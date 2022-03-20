using DokoSharp.Lib;
using DokoSharp.Server.Messaging;
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

    public void SignalReceivedCards(Player player, IEnumerable<Card> receivedCards, bool clearedOldCards)
    {
        // Send message
        SendMessage(new CardsReceivedMessage()
        {
            ReceivedCards = receivedCards.Select(c => c.Base.Identifier).ToArray(),
            ClearedOldCards = clearedOldCards
        });
    }

    public void SignalRemovedCards(Player player, IEnumerable<Card> droppedCards)
    {
        // Send message
        SendMessage(new CardsDroppedMessage()
        {
            DroppedCards = droppedCards.Select(c => c.Base.Identifier).ToArray()
        });
    }

    public Tuple<Card, Announcement> RequestPlaceCard(Player player, Trick trick, bool canMakeAnnouncement)
    {
        // Send request
        SendMessage(new RequestPlaceCardMessage()
        {
            TrickCards = trick.PlacedCards.Select(c => c.Base.Identifier).ToArray(),
            CanMakeAnnouncement = canMakeAnnouncement
        });

        // Wait for reply
        var reply = ReceiveMessage<ReplyPlaceCardMessage>();

        return new(player.Cards.First(c => c.Base.Identifier == reply.CardIdentifiers![0]), reply.Announcement);
    }

    public IEnumerable<Card> RequestCards(Player player, int amount, string requestText)
    {
        IList<string>? cardIds = null;
        while (cardIds == null || cardIds.Count != amount)
        {
            // Send request
            SendMessage(new RequestCardsMessage()
            {
                RequestText = requestText
            });

            // Wait for reply
            var reply = ReceiveMessage<ReplyCardsMessage>();
            cardIds = reply.CardIdentifiers;
        }

        var result = new Card[amount];
        for (int i = 0; i < amount; i++)
        {
            var id = cardIds[i];
            result[i] = player.Cards.First(c => c.Base.Identifier == id && !result.Contains(c));
        }

        return result;
    }

    public string? RequestReservation(Player player, IEnumerable<string> possibilities)
    {
        // Send request
        SendMessage(new RequestReservationMessage()
        {
            Possibilities = possibilities.ToArray()
        });

        // Wait for reply
        var reply = ReceiveMessage<ReplyReservationMessage>();

        return reply.ReservationName;
    }

    public bool RequestYesNo(Player player, string requestText)
    {
        // Send request
        SendMessage(new RequestYesNoMessage()
        {
            RequestText = requestText
        });

        // Wait for reply
        var reply = ReceiveMessage<ReplyYesNoMessage>();

        return reply.IsYes;
    }

    public CardColor RequestColor(Player player, string requestText)
    {
        // Send request
        SendMessage(new RequestColorMessage()
        {
            RequestText = requestText
        });

        // Wait for reply
        var reply = ReceiveMessage<ReplyColorMessage>();

        return reply.Color;
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

        Log.Verbose("Sent message to player {ip}:\n{msg}", _client.Client.RemoteEndPoint, JsonSerializer.Serialize(msg, Utils.BeautifyJsonOptions));
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
            if (msg != null && msg is T typedMsg)
            {
                Log.Verbose("Received message from player {ip}:\n{msg}", _client.Client.RemoteEndPoint, JsonSerializer.Serialize(msg, Utils.BeautifyJsonOptions));
                return typedMsg;
            }
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
