namespace DokoSharp.Lib;


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
    /// Returns the trump rank of the card in the round.
    /// The trump rank of a non-trump card is -1.
    /// </summary>
    public int TrumpRank => IsTrump ? Round.Description.TrumpRanking.IndexOf(Base) : -1;

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

    public override string ToString()
    {
        return Base.ToString();
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
            foreach (string symbol in new[] {"A", "10", "K", "O", "U", "9"})
            {
                cards.Add(new Card(round, CardBase.Existing[color][symbol]));
                cards.Add(new Card(round, CardBase.Existing[color][symbol]));
            }
        }

        return cards;
    }
}