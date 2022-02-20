namespace DokoSharp.Lib;


/// <summary>
/// An enumeration of card colors.
/// </summary>
public enum CardColor
{
    Eichel,
    Blatt,
    Herz,
    Schell
}

/// <summary>
/// Describes a basic card.
/// </summary>
public class CardBase : IIdentifiable
{
    #region Static Fields

    /// <summary>
    /// A mapping of colors and symbols to the existing card bases.
    /// </summary>
    public static readonly IReadOnlyDictionary<CardColor, IReadOnlyDictionary<string, CardBase>> Existing;

    /// <summary>
    /// Returns the card base with the given identifier or null if none is found.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static CardBase? GetByIdentifier(string id)
    {
        string[] split = id.Split('_');

        CardColor color = Enum.Parse<CardColor>(split[0]);
        string symbol = split[1];

        if (Existing.TryGetValue(color, out var nameMap))
        {
            return nameMap!.GetValueOrDefault(symbol, null);
        }

        return null;
    }

    /// <summary>
    /// Returns the cards bases with the given identifiers.
    /// Throws an exception if one identifier isn't found.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public static IEnumerable<CardBase> GetByIdentifiers(IEnumerable<string> ids)
    {
        return ids.Select(id => GetByIdentifier(id)!);
    }

    static CardBase()
    {
        var existing = new Dictionary<CardColor, IReadOnlyDictionary<string, CardBase>>();

        foreach (CardColor color in Enum.GetValues<CardColor>())
        {
            var colorDict = new Dictionary<string, CardBase>()
            {
                { "A", new("Ass", "A", color, 11) },
                { "10", new("Zehn", "10", color, 10) },
                { "K", new("König", "K", color, 4) },
                { "O", new("Ober", "O", color, 3) },
                { "U", new("Unter", "U", color, 2) },
                { "9", new("Neun", "9", color, 0) },
            };

            existing[color] = colorDict;
        }

        Existing = existing;
    }

    #endregion

    #region Properties

    /// <summary>
    /// The identifier of the card.
    /// Consists of the concatenation of <see cref="Color"/> and <see cref="Symbol"/>, e.g. EichelA.
    /// </summary>
    public string Identifier => $"{Color}_{Symbol}";

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

    public override string ToString()
    {
        return Identifier;
    }
}