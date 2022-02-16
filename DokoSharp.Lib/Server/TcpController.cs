using DokoSharp.Lib.Messaging;
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

namespace DokoSharp.Lib.Server;

/// <summary>
/// A player controller that receives input via a TCP connection.
/// </summary>
public class TcpController : IPlayerController, IDisposable
{
    private TcpClient _client;
    private NetworkStream _stream;
    private StreamReader _reader;
    private StreamWriter _writer;
    private bool disposedValue;

    public bool IsConnected => _client?.Connected ?? false;

    public TcpController(TcpClient tcpClient)
    {
        _client = tcpClient;
        _stream = tcpClient.GetStream();
        _reader = new(_stream);
        _writer = new(_stream);
    }

    public Card RequestHandCard(Player player)
    {
        // Send request
        RequestHandCardMessage request = new(player.Name);
        string jsonRequest = JsonSerializer.Serialize(request, new JsonSerializerOptions() { });
        _writer.WriteLine(jsonRequest);

        // Wait for reply
        ReplyHandCardMessage? reply = null;
        while (reply == null)
        {
            string jsonReply = _reader.ReadLine()!;
            try
            {
                reply = JsonSerializer.Deserialize<ReplyHandCardMessage>(jsonReply);
            }
            catch (JsonException)
            {
                Debug.WriteLine("Invalid reply received.");
            }

        }

        return player.HandCards.First(c => c.Base.Identifier == reply.CardIdentifier!);
    }

    public Reservation? RequestReservation(Player player)
    {
        return null;
    }

    public bool RequestYesNo(Player player, string requestText)
    {
        return false;
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
