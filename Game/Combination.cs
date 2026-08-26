using TichuWinForms_Smooth.Models;

namespace TichuWinForms_Smooth.Game;

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
}
