using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DokoSharp.Lib;
using DokoSharp.Lib.Rulings;
using DokoSharp.Server.Messaging;
using Serilog;

namespace DokoSharp.Server;

/// <summary>
/// 
/// </summary>
public class TcpServer : IDisposable
{
    #region Fields

    private readonly TcpListener _server;
    private bool disposedValue;
    private readonly List<TcpController> _connections;

    private readonly AutoResetEvent _gameReadyEvent = new(false);

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

    public TcpServer(string ipAddress, int port)
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

        // Create game
        Log.Information("Enough players joined, starting the game.");
        Game = new(new Tuple<string, IPlayerController>[]
        {
            new("player1", _connections[0]),
            //new("player2", _connections[1]),
            new("bot2", DummyController.Instance),
            new("bot3", DummyController.Instance),
            new("bot4", DummyController.Instance),
            //new("player2", _connections[1]),
            //new("player3", _connections[2]),
            //new("player4", _connections[3]),
        }, Rule.GetDefaults());
        GameCreated();

        // Start game
        Game.Start(4);

        Stop();
    }

    /// <summary>
    /// Stops the Doko server instance und closes all connections.
    /// Does nothing if the server isn't running.
    /// </summary>
    public void Stop()
    {
        if (!IsRunning)
        {
            Log.Debug("The server isn't running. Aborting.");
            return;
        }

        Log.Information("Stopping the server");
        _connections.ForEach(c => c.Dispose());
        _connections.Clear();
        _server.Stop();
        IsRunning = false;
    }

    #region Event Handlers

    private void GameCreated()
    {
        SendMessageToAll(new GameCreatedMessage()
        {
            Players = Game!.Players.ToIdentifiers().ToArray(),
            SpecialRules = Game!.Rules.ToIdentifiers().ToArray(),
        });

        Game.GameStarted += Game_GameStarted;
        Game.RoundCreated += Game_RoundCreated;
        Game.PointsUpdated += Game_PointsUpdated;
        Game.GameFinished += Game_GameFinished;
    }

    private void Game_GameStarted(object sender, Game.GameStartedEventArgs e)
    {
        SendMessageToAll(new GameStartedMessage()
        {
            NumberOfRounds = e.NumberOfRounds
        });
    }

    private void Game_RoundCreated(object sender, Game.RoundCreatedEventArgs e)
    {
        var round = e.Round;
        SendMessageToAll(new RoundCreatedMessage()
        {
            PlayerOrder = round.PlayersInOrder.ToIdentifiers().ToArray(),
            RoundNumber = e.RoundNumber
        });

        // Add event handlers
        round.RoundStarted += Round_RoundStarted;
        round.ReservationsPerformed += Round_ReservationsPerformed;
        round.RegistrationsApplied += Round_RegistrationsApplied;
        round.TrickCreated += Round_TrickCreated;
        round.AnnouncementMade += Round_AnnouncementMade;
        round.RoundFinished += Round_RoundFinished;
    }

    private void Game_PointsUpdated(object sender, Game.PointsUpdatedEventArgs e)
    {
        SendMessageToAll(new PointsUpdatedMessage()
        {
            PointChanges = new Dictionary<string, int>(e.PointChanges.Select(pc => 
                new KeyValuePair<string, int>(pc.Key.Identifier, pc.Value)
            ))
        });
    }

    private void Game_GameFinished(object sender, Game.GameFinishedEventArgs e)
    {
        SendMessageToAll(new GameFinishedMessage()
        {
            Ranking = e.Ranking.ToIdentifiers().ToArray(),
        });

        Game!.GameStarted -= Game_GameStarted;
        Game.RoundCreated -= Game_RoundCreated;
        Game.PointsUpdated -= Game_PointsUpdated;
        Game.GameFinished -= Game_GameFinished;
    }

    private void Round_RoundStarted(object sender, Round.RoundStartedEventArgs e)
    {
        SendMessageToAll(new RoundStartedMessage()
        {
            TrumpRanking = e.TrumpRanking.ToIdentifiers().ToArray()
        });
    }

    private void Round_ReservationsPerformed(object sender, Round.ReservationsPerformedEventArgs e)
    {
        SendMessageToAll(new ReservationsPerformedMessage()
        {
            ActiveReservation = e.ActiveReservation?.Name,
        });
    }

    private void Round_RegistrationsApplied(object sender, Round.RegistrationsAppliedEventArgs e)
    {
        SendMessageToAll(new RegistrationsAppliedMessage()
        {
            TrumpRanking = e.TrumpRanking?.ToIdentifiers().ToArray()
        });
    }

    private void Round_TrickCreated(object sender, Round.TrickCreatedEventArgs e)
    {
        var trick = e.Trick;
        SendMessageToAll(new TrickCreatedMessage()
        {
            TrickNumber = e.TrickNumber,
            PlayerOrder = trick.PlayersInOrder.Select(p => p.Name).ToArray()
        });

        // Add event handlers
        trick.CardPlaced += Trick_CardPlaced;
        trick.TrickFinished += Trick_TrickFinished;
    }

    private void Round_AnnouncementMade(object sender, Round.AnnouncementMadeEventArgs e)
    {
        SendMessageToAll(new AnnouncementMadeMessage()
        {
            Player = e.Player.Identifier,
            Announcement = e.Announcement
        });
    }

    private void Round_RoundFinished(object sender, Round.RoundFinishedEventArgs e)
    {
        var round = (Round)sender;
        SendMessageToAll(new RoundFinishedMessage()
        {
            RePartyWon = e.Result.RePartyWon,
            ReParty = round.Description.ReParty.ToIdentifiers().ToArray(),
            RePoints = e.Result.RePoints,
            ReValue = e.Result.ReValue,
            ContraParty = round.Description.ContraParty.ToIdentifiers().ToArray(),
            ContraPoints = e.Result.ContraPoints,
            ContraValue = e.Result.ContraValue,
            BasePoints = e.Result.BasePoints
        });

        // Remove event handlers
        round.RoundStarted -= Round_RoundStarted;
        round.ReservationsPerformed -= Round_ReservationsPerformed;
        round.RegistrationsApplied -= Round_RegistrationsApplied;
        round.TrickCreated -= Round_TrickCreated;
        round.AnnouncementMade -= Round_AnnouncementMade;
        round.RoundFinished -= Round_RoundFinished;
    }

    private void Trick_CardPlaced(object sender, Trick.CardPlacedEventArgs e)
    {
        SendMessageToAll(new CardPlacedMessage()
        {
            Player = e.Player.Name,
            PlacedCard = e.PlacedCard.Base.Identifier,
        });
    }

    private void Trick_TrickFinished(object sender, Trick.TrickFinishedEventArgs e)
    {
        SendMessageToAll(new TrickFinishedMessage()
        {
            Winner = e.Winner.Name,
            Value = e.Value
        });

        // Remove event handlers
        var trick = (Trick)sender;
        trick.CardPlaced -= Trick_CardPlaced;
        trick.TrickFinished -= Trick_TrickFinished;
    }

    #endregion

    protected void SendMessageToAll(Message msg)
    {
        _connections.ForEach(c => c.SendMessage(msg));
    }

    protected void ConnectionLoop()
    {
        // Wait for enough players
        while (IsRunning && Connections.Count < 1)
        {
            Log.Information("Waiting for {diff} more players to start the game.", 4 - Connections.Count);
            var client = _server.AcceptTcpClient();

            _connections.Add(new TcpController(client));
        }

        //if (Connections.Count == 1)
        //{
            _gameReadyEvent.Set();
        //}
    }

    #region Overrides & Implementations

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _server?.Stop();
                _connections.ForEach(c => c.Dispose());
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
