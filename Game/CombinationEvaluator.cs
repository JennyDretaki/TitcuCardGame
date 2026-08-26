using TichuWinForms_Smooth.Models;

namespace TichuWinForms_Smooth.Game;

public static class CombinationEvaluator
{
    public static Combination Evaluate(IReadOnlyList<Card> cards, double previousSingleValue = 0)
    {
        if (cards.Count == 0)
            return Invalid(cards);

        var list = cards.ToList();

        if (list.Count == 1)
            return EvaluateSingle(list[0], previousSingleValue);

        if (list.Any(c => c.Special is SpecialCard.Dog or SpecialCard.Dragon))
            return Invalid(list);

        if (list.All(c => !c.IsSpecial))
        {
            if (IsFourBomb(list, out var fourValue))
                return New(ComboType.FourBomb, fourValue, list);

            if (IsStraightFlush(list, out var straightFlushValue))
                return New(ComboType.StraightFlushBomb, straightFlushValue, list);
        }

        int phoenixCount = list.Count(c => c.Special == SpecialCard.Phoenix);
        if (phoenixCount > 1)
            return Invalid(list);

        if (list.Count == 2 && IsPair(list, out var pairValue))
            return New(ComboType.Pair, pairValue, list);

        if (list.Count == 3 && IsTriple(list, out var tripleValue))
            return New(ComboType.Triple, tripleValue, list);

        if (list.Count == 5 && IsFullHouse(list, out var fullHouseValue))
            return New(ComboType.FullHouse, fullHouseValue, list);

        if (list.Count >= 5 && IsStraight(list, out var straightValue))
            return New(ComboType.Straight, straightValue, list);

        if (list.Count >= 4 && list.Count % 2 == 0 &&
            IsConsecutivePairs(list, out var pairSequenceValue))
            return New(ComboType.ConsecutivePairs, pairSequenceValue, list);

        return Invalid(list);
    }

    public static bool CanBeat(Combination play, Combination? table)
    {
        if (play.Type == ComboType.Invalid)
            return false;

        if (table is null)
            return true;

        if (play.Type == ComboType.Dog)
            return false;

        if (play.IsBomb)
        {
            if (!table.IsBomb)
                return true;

            if (play.Type == ComboType.StraightFlushBomb && table.Type == ComboType.FourBomb)
                return true;

            if (play.Type == table.Type)
            {
                if (play.Type == ComboType.StraightFlushBomb && play.CardCount != table.CardCount)
                    return play.CardCount > table.CardCount;

                return play.Value > table.Value;
            }

            return false;
        }

        if (table.IsBomb)
            return false;

        if (play.Type != table.Type || play.CardCount != table.CardCount)
            return false;

        return play.Value > table.Value;
    }

    private static Combination EvaluateSingle(Card card, double previous)
    {
        if (card.Special == SpecialCard.Dog)
            return New(ComboType.Dog, 0, new List<Card> { card });

        double value = card.Special switch
        {
            SpecialCard.MahJong => 1,
            SpecialCard.Phoenix => previous <= 0 ? 1.5 : Math.Min(previous + 0.5, 14.5),
            SpecialCard.Dragon => 16,
            _ => card.Rank
        };

        return New(ComboType.Single, value, new List<Card> { card });
    }

    private static bool IsPair(List<Card> cards, out double value)
    {
        value = 0;

        if (cards.Any(c => c.Special == SpecialCard.MahJong))
            return false;

        bool phoenix = cards.Any(c => c.Special == SpecialCard.Phoenix);
        var normals = cards.Where(c => !c.IsSpecial).ToList();

        if (phoenix && normals.Count == 1)
        {
            value = normals[0].Rank;
            return true;
        }

        if (!phoenix && normals.Count == 2 && normals[0].Rank == normals[1].Rank)
        {
            value = normals[0].Rank;
            return true;
        }

        return false;
    }

    private static bool IsTriple(List<Card> cards, out double value)
    {
        value = 0;

        if (cards.Any(c => c.Special == SpecialCard.MahJong))
            return false;

        bool phoenix = cards.Any(c => c.Special == SpecialCard.Phoenix);
        var normals = cards.Where(c => !c.IsSpecial).ToList();

        if (!phoenix && normals.Count == 3 && normals.Select(c => c.Rank).Distinct().Count() == 1)
        {
            value = normals[0].Rank;
            return true;
        }

        if (phoenix && normals.Count == 2 && normals[0].Rank == normals[1].Rank)
        {
            value = normals[0].Rank;
            return true;
        }

        return false;
    }

    private static bool IsFullHouse(List<Card> cards, out double value)
    {
        value = 0;

        if (cards.Any(c => c.Special == SpecialCard.MahJong))
            return false;

        bool phoenix = cards.Any(c => c.Special == SpecialCard.Phoenix);
        var normals = cards.Where(c => !c.IsSpecial).ToList();

        var groups = normals
            .GroupBy(c => c.Rank)
            .ToDictionary(g => g.Key, g => g.Count());

        if (!phoenix)
        {
            if (groups.Count == 2 && groups.Values.OrderBy(x => x).SequenceEqual(new[] { 2, 3 }))
            {
                value = groups.First(kv => kv.Value == 3).Key;
                return true;
            }

            return false;
        }

        if (groups.Count == 2)
        {
            var counts = groups.Values.OrderBy(x => x).ToArray();

            if (counts.SequenceEqual(new[] { 2, 2 }))
            {
                value = groups.Keys.Max();
                return true;
            }

            if (counts.SequenceEqual(new[] { 1, 3 }))
            {
                value = groups.First(kv => kv.Value == 3).Key;
                return true;
            }
        }

        return false;
    }

    private static bool IsStraight(List<Card> cards, out double value)
    {
        value = 0;

        if (cards.Any(c => c.Special is SpecialCard.Dog or SpecialCard.Dragon))
            return false;

        bool phoenix = cards.Any(c => c.Special == SpecialCard.Phoenix);
        var ranks = new List<int>();

        foreach (var card in cards.Where(c => c.Special != SpecialCard.Phoenix))
            ranks.Add(card.Special == SpecialCard.MahJong ? 1 : card.Rank);

        if (ranks.Distinct().Count() != ranks.Count)
            return false;

        ranks.Sort();

        if (!phoenix)
        {
            for (int i = 1; i < ranks.Count; i++)
                if (ranks[i] != ranks[i - 1] + 1)
                    return false;

            value = ranks[^1];
            return true;
        }

        for (int wild = 2; wild <= 14; wild++)
        {
            if (ranks.Contains(wild))
                continue;

            var test = ranks.Append(wild).OrderBy(x => x).ToArray();
            bool valid = true;

            for (int i = 1; i < test.Length; i++)
            {
                if (test[i] != test[i - 1] + 1)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                value = test[^1];
                return true;
            }
        }

        return false;
    }

    private static bool IsConsecutivePairs(List<Card> cards, out double value)
    {
        value = 0;

        if (cards.Any(c => c.Special == SpecialCard.MahJong))
            return false;

        bool phoenix = cards.Any(c => c.Special == SpecialCard.Phoenix);
        var groups = cards
            .Where(c => !c.IsSpecial)
            .GroupBy(c => c.Rank)
            .OrderBy(g => g.Key)
            .ToList();

        if (!phoenix)
        {
            if (groups.Any(g => g.Count() != 2) || groups.Count * 2 != cards.Count)
                return false;

            for (int i = 1; i < groups.Count; i++)
                if (groups[i].Key != groups[i - 1].Key + 1)
                    return false;

            value = groups[^1].Key;
            return true;
        }

        if (groups.Count(g => g.Count() == 1) != 1 ||
            groups.Any(g => g.Count() > 2))
            return false;

        for (int i = 1; i < groups.Count; i++)
            if (groups[i].Key != groups[i - 1].Key + 1)
                return false;

        value = groups[^1].Key;
        return true;
    }

    private static bool IsFourBomb(List<Card> cards, out double value)
    {
        value = 0;

        if (cards.Count == 4 && cards.Select(c => c.Rank).Distinct().Count() == 1)
        {
            value = cards[0].Rank;
            return true;
        }

        return false;
    }

    private static bool IsStraightFlush(List<Card> cards, out double value)
    {
        value = 0;

        if (cards.Count < 5 || cards.Select(c => c.Suit).Distinct().Count() != 1)
            return false;

        var ranks = cards.Select(c => c.Rank).OrderBy(x => x).ToArray();

        if (ranks.Distinct().Count() != ranks.Length)
            return false;

        for (int i = 1; i < ranks.Length; i++)
            if (ranks[i] != ranks[i - 1] + 1)
                return false;

        value = ranks[^1];
        return true;
    }

    private static Combination New(ComboType type, double value, List<Card> cards) =>
        new()
        {
            Type = type,
            Value = value,
            CardCount = cards.Count,
            Cards = cards.ToList()
        };

    private static Combination Invalid(IEnumerable<Card> cards) =>
        new()
        {
            Type = ComboType.Invalid,
            Value = 0,
            CardCount = cards.Count(),
            Cards = cards.ToList()
        };
}
