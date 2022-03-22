using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DokoSharp.Lib;
using Serilog;

namespace DokoTable.Models;

public record AnnouncementDto(Announcement Announcement, string Name)
{
    public static readonly IReadOnlyList<AnnouncementDto> Available;

    static AnnouncementDto()
    {
        Available = Enum.GetValues<Announcement>().Select(a => new AnnouncementDto(a, a.GetName() ?? string.Empty)).ToList();
    }
}

/// <summary>
/// Contains the state for a game of Doko.
/// </summary>
public class GameState : INotifyPropertyChanged
{
    #region Fields

    private int _numberOfRounds = 0;
    private IReadOnlyList<string>? _playerOrder = null;
    private string? _playerStartingRound;
    private int _roundNumber = 0;
    private string? _playerStartingTrick;
    private int _trickNumber = 0;
    private IReadOnlyList<CardBase> _trumpCards = new List<CardBase>();
    private IReadOnlyList<CardBase> _handCards = new List<CardBase>();
    private IReadOnlyList<CardBase> _placedCards = new List<CardBase>();
    private IEnumerable<CardBase> _placeableCards = new List<CardBase>();
    // Announcements
    private bool _canMakeAnnouncement = false;
    private IEnumerable<AnnouncementDto> _availableAnnouncements = new List<AnnouncementDto>();
    private Announcement _currentAnnouncement = Announcement.None;

    #endregion

    #region Properties

    /// <summary>
    /// The number of rounds of Doko that should be played.
    /// </summary>
    public int NumberOfRounds
    {
        get => _numberOfRounds;
        set
        {
            _numberOfRounds = value;
            RaisePropertyChanged(nameof(NumberOfRounds));
        }
    }

    /// <summary>
    /// The order of the players on the table starting with this player.
    /// </summary>
    public IReadOnlyList<string>? PlayerOrder
    {
        get => _playerOrder;
        set
        {
            _playerOrder = value;
            RaisePropertyChanged(nameof(PlayerOrder));
        }
    }

    /// <summary>
    /// The player starting the current round.
    /// </summary>
    public string? PlayerStartingRound
    {
        get => _playerStartingRound;
        set
        {
            _playerStartingRound = value;
            RaisePropertyChanged(nameof(PlayerStartingRound));
        }
    }

    /// <summary>
    /// The number of the current Doko round.
    /// </summary>
    public int RoundNumber
    {
        get => _roundNumber;
        set
        {
            _roundNumber = value;
            RaisePropertyChanged(nameof(RoundNumber));
        }
    }

    /// <summary>
    /// The player starting the current trick.
    /// </summary>
    public string? PlayerStartingTrick
    {
        get => _playerStartingTrick;
        set
        {
            _playerStartingTrick = value;
            RaisePropertyChanged(nameof(PlayerStartingTrick));
        }
    }

    /// <summary>
    /// The number of the current Doko trick.
    /// </summary>
    public int TrickNumber
    {
        get => _trickNumber;
        set
        {
            _trickNumber = value;
            RaisePropertyChanged(nameof(TrickNumber));
        }
    }

    /// <summary>
    /// The Trump cards of the current round.
    /// </summary>
    public IReadOnlyList<CardBase> TrumpCards
    {
        get => _trumpCards;
        set
        {
            _trumpCards = value;
            RaisePropertyChanged(nameof(TrumpCards));
        }
    }

    /// <summary>
    /// The hand cards of the player.
    /// </summary>
    public IReadOnlyList<CardBase> HandCards
    {
        get => _handCards;
        set
        {
            _handCards = value;
            RaisePropertyChanged(nameof(HandCards));
        }
    }

    /// <summary>
    /// The already placed cards of the trick.
    /// </summary>
    public IReadOnlyList<CardBase> PlacedCards
    {
        get => _placedCards;
        set
        {
            _placedCards = value;
            RaisePropertyChanged(nameof(PlacedCards));
        }
    }

    /// <summary>
    /// An enumeration of all cards that can be placed on the already placed cards.
    /// </summary>
    public IEnumerable<CardBase> PlaceableHandCards
    {
        get => _placeableCards;
        set
        {
            _placeableCards = value;
            RaisePropertyChanged(nameof(PlaceableHandCards));
        }
    }

    /// <summary>
    /// The available announcements.
    /// </summary>
    public IEnumerable<AnnouncementDto> AvailableAnnouncements
    {
        get => _availableAnnouncements;
        set
        {
            _availableAnnouncements = value;
            RaisePropertyChanged(nameof(AvailableAnnouncements));
        }
    }

    /// <summary>
    /// A flag wether an announcement can be made.
    /// </summary>
    public bool CanMakeAnnouncement
    {
        get => _canMakeAnnouncement;
        set
        {
            _canMakeAnnouncement = value;
            RaisePropertyChanged(nameof(CanMakeAnnouncement));
        }
    }

    /// <summary>
    /// The current announcement of the player.
    /// </summary>
    public Announcement Announcement
    {
        get => _currentAnnouncement;
        set
        {
            _currentAnnouncement = value;
            RaisePropertyChanged(nameof(Announcement));
        }
    }

    #endregion

    #region Constructor

    public GameState()
    {
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Convinience method that resets all trick-related properties.
    /// </summary>
    public void EndTrick()
    {
        PlayerStartingTrick = null;
        PlacedCards = new List<CardBase>();
        PlaceableHandCards = new List<CardBase>();
    }

    /// <summary>
    /// Convinience method that resets all round-related properties.
    /// </summary>
    public void EndRound()
    {
        PlayerStartingRound = null;
        TrickNumber = 0;
        TrumpCards = new List<CardBase>();
        HandCards = new List<CardBase>();
        AvailableAnnouncements = new List<AnnouncementDto>();
        Announcement = Announcement.None;
    }

    /// <summary>
    /// Removes the given card from the hand of the player.
    /// Only the first occurence is removed.
    /// </summary>
    /// <param name="card"></param>
    public void RemoveHandCard(CardBase card)
    {
        var cardIdx = HandCards.IndexOf(card);
        var newHandCards = HandCards.Take(cardIdx).Concat(HandCards.Skip(cardIdx + 1)).ToList();

        HandCards = newHandCards;
    }


    public IEnumerable<CardBase> GetPlaceableHandCards()
    {
        if (PlacedCards.Count == 0) return HandCards;

        var firstCard = PlacedCards[0];

        if (TrumpCards.Contains(firstCard))
        {
            var trump = HandCards.Where(c => TrumpCards.Contains(c));

            // Only trump can be placed on trump
            if (trump.Any()) return trump;
            // ... except when player has none
            else return HandCards;
        }
        else
        {
            CardColor color = firstCard.Color;
            var colored = HandCards.Where(c => c.Color == color && !TrumpCards.Contains(c));

            // Same color has to be placed
            if (colored.Any()) return colored;
            // ... except when player has none
            else return HandCards;
        }
    }

    public IEnumerable<AnnouncementDto> GetAvailableAnnouncements()
    {
        return AnnouncementDto.Available.Where(dto => dto.Announcement > Announcement || dto.Announcement == Announcement.None);
    }

    public IEnumerable<CardBase> SortCardsByRanking(IEnumerable<CardBase> cards)
    {
        return cards.OrderBy(c => TrumpCards.IndexOf(c)).ThenBy(c => c.Color).ThenBy(c => c.Value).ToList();
    }

    #endregion

    #region Overrides & Implementations

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged(string property)
    {
        Log.Debug($"Property {property} of DokoViewModel changed.");
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    #endregion
}
