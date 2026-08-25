namespace TichuWinForms.Models;

public sealed class Player
{
    public string Name { get; init; } = "";
    public int Seat { get; init; }
    public int Team => Seat % 2;
    public bool IsHuman { get; init; }

    public List<Card> Hand { get; } = new();
    public List<Card> Captured { get; } = new();

    public bool IsOut => Hand.Count == 0;
    public int FinishOrder { get; set; }

    public bool CalledTichu { get; set; }
    public bool CalledGrandTichu { get; set; }
    public bool HasPlayedAnyCard { get; set; }

    public override string ToString() => Name;
}
