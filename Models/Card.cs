namespace TichuWinForms_Smooth.Models;

public enum Suit
{
    Jade,
    Sword,
    Pagoda,
    Star,
    Special
}

public enum SpecialCard
{
    None,
    MahJong,
    Dog,
    Phoenix,
    Dragon
}

public sealed class Card
{
    public Suit Suit { get; init; }
    public int Rank { get; init; }
    public SpecialCard Special { get; init; }

    public bool IsSpecial => Special != SpecialCard.None;

    public int Points => Special switch
    {
        SpecialCard.Dragon => 25,
        SpecialCard.Phoenix => -25,
        _ => Rank switch
        {
            5 => 5,
            10 => 10,
            13 => 10,
            _ => 0
        }
    };

    public string RankText => Special switch
    {
        SpecialCard.MahJong => "1",
        SpecialCard.Dog => "DOG",
        SpecialCard.Phoenix => "PHX",
        SpecialCard.Dragon => "DRG",
        _ => Rank switch
        {
            11 => "J",
            12 => "Q",
            13 => "K",
            14 => "A",
            _ => Rank.ToString()
        }
    };

    public string SuitSymbol => Suit switch
    {
        Suit.Jade => "♣",
        Suit.Sword => "♠",
        Suit.Pagoda => "♦",
        Suit.Star => "♥",
        _ => ""
    };

    public override string ToString() => IsSpecial ? RankText : $"{RankText}{SuitSymbol}";
}
