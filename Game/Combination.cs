using TichuWinForms.Models;

namespace TichuWinForms.Game;

public enum ComboType
{
    Invalid,
    Single,
    Pair,
    Triple,
    FullHouse,
    Straight,
    ConsecutivePairs,
    FourBomb,
    StraightFlushBomb,
    Dog
}

public sealed class Combination
{
    public ComboType Type { get; init; }
    public double Value { get; init; }
    public int CardCount { get; init; }
    public List<Card> Cards { get; init; } = new();

    public bool IsBomb => Type is ComboType.FourBomb or ComboType.StraightFlushBomb;

    public override string ToString()
    {
        if (Type == ComboType.Invalid) return "Invalid";
        return $"{Type} ({string.Join(" ", Cards)})";
    }
}
