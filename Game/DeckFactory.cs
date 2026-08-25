using TichuWinForms.Models;

namespace TichuWinForms.Game;

public static class DeckFactory
{
    public static List<Card> CreateDeck()
    {
        var cards = new List<Card>();

        var suits = new[] { Suit.Jade, Suit.Sword, Suit.Pagoda, Suit.Star };

        foreach (var suit in suits)
        {
            for (int rank = 2; rank <= 14; rank++)
            {
                cards.Add(new Card
                {
                    Suit = suit,
                    Rank = rank,
                    Special = SpecialCard.None
                });
            }
        }

        cards.Add(new Card { Suit = Suit.Special, Rank = 1, Special = SpecialCard.MahJong });
        cards.Add(new Card { Suit = Suit.Special, Rank = 0, Special = SpecialCard.Dog });
        cards.Add(new Card { Suit = Suit.Special, Rank = 0, Special = SpecialCard.Phoenix });
        cards.Add(new Card { Suit = Suit.Special, Rank = 0, Special = SpecialCard.Dragon });

        return cards;
    }

    public static void Shuffle<T>(IList<T> list, Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
