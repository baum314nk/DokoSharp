namespace DokoLib;

/// <summary>
/// An enumeration of card colors.
/// </summary>
public enum CardColor
{
    Kreuz,
    Pik, 
    Herz,
    Karo
}

/// <summary>
/// Describes a basic card.
/// </summary>
public class CardBase
{
    #region Static Fields

    /// <summary>
    /// A mapping of colors and symbols to the existing card bases.
    /// </summary>
    public static readonly IReadOnlyDictionary<CardColor, IReadOnlyDictionary<string, CardBase>> Existing;

    static CardBase()
    {
        var existing = new Dictionary<CardColor, IReadOnlyDictionary<string, CardBase>>();

        foreach(CardColor color in Enum.GetValues<CardColor>())
        {
            var colorDict = new Dictionary<string, CardBase>()
            {
                { "A", new("Ass", "A", color, 11) },
                { "10", new("Zehn", "10", color, 10) },
                { "K", new("König", "K", color, 4) },
                { "D", new("Dame", "D", color, 3) },
                { "B", new("Bube", "B", color, 2) },
                { "9", new("Neun", "9", color, 0) },
            };

            existing[color] = colorDict;
        }

        Existing = existing;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The name of the card, e.g. Ass.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// The symbol of the card, e.g. A for Ass.
    /// </summary>
    public string Symbol { get; init; }

    /// <summary>
    /// The color of the card.
    /// </summary>
    public CardColor Color { get; init; }

    /// <summary>
    /// The value of the card, e.g. 11 for Ass.
    /// </summary>
    public int Value { get; init; }

    #endregion

    protected CardBase(string name, string symbol, CardColor color, int value)
    {
        Name = name;
        Symbol = symbol;
        Color = color;
        Value = value;
    }
}

/// <summary>
/// Describes a card in particular Doko round.
/// </summary>
public class Card
{
    /// <summary>
    /// The base of the card.
    /// </summary>
    public CardBase Base { get; init; }

    /// <summary>
    /// The game the card belongs to.
    /// </summary>
    public Round Round { get; init; }

    /// <summary>
    /// Returns true if the card is trump otherwise false.
    /// </summary>
    public bool IsTrump => Round.Description.TrumpRanking.Contains(Base);

    /// <summary>
    /// Creates a new card for the given round from the given card base.
    /// </summary>
    /// <param name="game"></param>
    /// <param name="cardBase"></param>
    public Card(Round round, CardBase cardBase)
    {
        Round = round;
        Base = cardBase;
    }

    /// <summary>
    /// Returns a full deck of Doko cards for the given game sorted by the colors and then their values from highest to lowest.
    /// </summary>
    /// <returns></returns>
    public static IList<Card> GetDeckOfCards(Round round)
    {
        List<Card> cards = new();

        foreach (CardColor color in Enum.GetValues<CardColor>())
        {
            foreach (string symbol in new[] {"A", "10", "K", "D", "B", "9"})
            {
                cards.Add(new Card(round, CardBase.Existing[color][symbol]));
                cards.Add(new Card(round, CardBase.Existing[color][symbol]));
            }
        }

        return cards;
    }
}