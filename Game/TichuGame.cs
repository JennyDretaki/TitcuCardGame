using TichuWinForms_Smooth.Models;

namespace TichuWinForms_Smooth.Game;

public sealed class TichuGame
{
    private readonly Random random = new();
    private readonly List<Card> trickPile = new();

    private int passesInRow;
    private int lastPlayerWhoPlayed = -1;

    public List<Player> Players { get; } = new();

    public int CurrentPlayerIndex { get; private set; }
    public int? LastPlayPlayerIndex { get; private set; }

    public Combination? TableCombination { get; private set; }

    // Only the most recent play. This is what the UI renders.
    public List<Card> CurrentPlayCards { get; } = new();

    public int[] TeamScores { get; } = new int[2];

    public int FinishCounter { get; private set; }
    public bool RoundOver { get; private set; }
    public bool MatchOver => TeamScores[0] >= 1000 || TeamScores[1] >= 1000;

    public bool ExchangeCompleted { get; private set; }
    public int? MahJongWishRank { get; private set; }

    public TichuGame()
    {
        Players.Add(new Player { Name = "You", Seat = 0, IsHuman = true });
        Players.Add(new Player { Name = "Bot Left", Seat = 1 });
        Players.Add(new Player { Name = "Partner", Seat = 2 });
        Players.Add(new Player { Name = "Bot Right", Seat = 3 });
    }

    public void StartRound()
    {
        foreach (var player in Players)
        {
            player.Hand.Clear();
            player.Captured.Clear();
            player.FinishOrder = 0;
            player.CalledTichu = false;
            player.CalledGrandTichu = false;
            player.HasPlayedAnyCard = false;
        }

        trickPile.Clear();
        CurrentPlayCards.Clear();
        TableCombination = null;

        passesInRow = 0;
        lastPlayerWhoPlayed = -1;
        LastPlayPlayerIndex = null;
        FinishCounter = 0;
        RoundOver = false;
        ExchangeCompleted = false;
        MahJongWishRank = null;

        var deck = DeckFactory.CreateDeck();
        DeckFactory.Shuffle(deck, random);

        // First 8 cards for Grand Tichu decision.
        for (int i = 0; i < 32; i++)
            Players[i % 4].Hand.Add(deck[i]);

        foreach (var player in Players)
            SortHand(player.Hand);

        for (int i = 1; i < 4; i++)
        {
            if (ShouldBotCallGrandTichu(Players[i]))
                Players[i].CalledGrandTichu = true;
        }

        // Finish deal to 14 cards.
        for (int i = 32; i < deck.Count; i++)
            Players[i % 4].Hand.Add(deck[i]);

        foreach (var player in Players)
            SortHand(player.Hand);

        CurrentPlayerIndex = Players.FindIndex(
            p => p.Hand.Any(c => c.Special == SpecialCard.MahJong));
    }

    public static void SortHand(List<Card> hand)
    {
        hand.Sort((a, b) =>
        {
            int first = SortValue(a).CompareTo(SortValue(b));
            return first != 0 ? first : a.Suit.CompareTo(b.Suit);
        });
    }

    public static int SortValue(Card card) => card.Special switch
    {
        SpecialCard.Dog => 0,
        SpecialCard.MahJong => 1,
        SpecialCard.Phoenix => 15,
        SpecialCard.Dragon => 16,
        _ => card.Rank
    };

    public bool CallGrandTichu(int playerIndex, out string error)
    {
        error = "";
        var player = Players[playerIndex];

        if (ExchangeCompleted)
        {
            error = "Grand Tichu must be called before the exchange is completed.";
            return false;
        }

        if (player.CalledGrandTichu || player.CalledTichu)
        {
            error = "A declaration has already been made.";
            return false;
        }

        player.CalledGrandTichu = true;
        return true;
    }

    public bool CallTichu(int playerIndex, out string error)
    {
        error = "";
        var player = Players[playerIndex];

        if (player.HasPlayedAnyCard)
        {
            error = "Tichu must be called before your first play.";
            return false;
        }

        if (player.CalledTichu || player.CalledGrandTichu)
        {
            error = "A declaration has already been made.";
            return false;
        }

        player.CalledTichu = true;
        return true;
    }

    public bool CompleteHumanExchange(
        Card toLeft,
        Card toPartner,
        Card toRight,
        out string error)
    {
        error = "";

        if (ExchangeCompleted)
        {
            error = "Exchange is already complete.";
            return false;
        }

        var human = Players[0];
        var selected = new[] { toLeft, toPartner, toRight };

        if (selected.Distinct().Count() != 3 ||
            selected.Any(c => !human.Hand.Contains(c)))
        {
            error = "Choose three different cards from your hand.";
            return false;
        }

        var outgoing = new Dictionary<(int From, int To), Card>
        {
            [(0, 1)] = toLeft,
            [(0, 2)] = toPartner,
            [(0, 3)] = toRight
        };

        for (int from = 1; from < 4; from++)
        {
            var gifts = ChooseBotExchangeCards(from);

            outgoing[(from, (from + 1) % 4)] = gifts.Left;
            outgoing[(from, (from + 2) % 4)] = gifts.Partner;
            outgoing[(from, (from + 3) % 4)] = gifts.Right;
        }

        foreach (var pair in outgoing)
            Players[pair.Key.From].Hand.Remove(pair.Value);

        foreach (var pair in outgoing)
            Players[pair.Key.To].Hand.Add(pair.Value);

        foreach (var player in Players)
            SortHand(player.Hand);

        ExchangeCompleted = true;

        CurrentPlayerIndex = Players.FindIndex(
            p => p.Hand.Any(c => c.Special == SpecialCard.MahJong));

        return true;
    }

    public bool TryPlayCards(
        int playerIndex,
        List<Card> selected,
        out string error,
        int? mahJongWish = null,
        bool outOfTurnBomb = false)
    {
        error = "";

        if (!ExchangeCompleted)
        {
            error = "Complete the 3-card exchange first.";
            return false;
        }

        if (RoundOver)
        {
            error = "The round is over.";
            return false;
        }

        if (playerIndex != CurrentPlayerIndex && !outOfTurnBomb)
        {
            error = "It is not this player's turn.";
            return false;
        }

        var player = Players[playerIndex];

        if (selected.Count == 0 || selected.Any(c => !player.Hand.Contains(c)))
        {
            error = "Select valid cards.";
            return false;
        }

        double previousSingle = TableCombination?.Type == ComboType.Single
            ? TableCombination.Value
            : 0;

        var combo = CombinationEvaluator.Evaluate(selected, previousSingle);

        if (combo.Type == ComboType.Invalid)
        {
            error = "This is not a valid Tichu combination.";
            return false;
        }

        if (outOfTurnBomb && !combo.IsBomb)
        {
            error = "Only a bomb can be played out of turn.";
            return false;
        }

        if (MahJongWishRank.HasValue &&
            CanSatisfyWish(playerIndex, MahJongWishRank.Value) &&
            !ContainsWish(selected, MahJongWishRank.Value))
        {
            error = $"You must satisfy the Mah Jong wish ({RankName(MahJongWishRank.Value)}) if possible.";
            return false;
        }

        if (combo.Type == ComboType.Dog)
        {
            if (TableCombination is not null)
            {
                error = "Dog can only be played when leading.";
                return false;
            }

            player.Hand.Remove(selected[0]);
            player.HasPlayedAnyCard = true;

            // UI shows Dog briefly as the latest play.
            CurrentPlayCards.Clear();
            CurrentPlayCards.Add(selected[0]);
            LastPlayPlayerIndex = playerIndex;

            CheckPlayerOut(playerIndex);

            trickPile.Clear();
            TableCombination = null;
            passesInRow = 0;
            lastPlayerWhoPlayed = -1;

            CurrentPlayerIndex = NextActivePlayer((playerIndex + 2) % 4);
            return true;
        }

        if (!CombinationEvaluator.CanBeat(combo, TableCombination))
        {
            error = TableCombination is null
                ? "Invalid lead."
                : $"You must beat {TableCombination.Type} ({TableCombination.Value}).";
            return false;
        }

        bool playedMahJong = selected.Any(c => c.Special == SpecialCard.MahJong);

        foreach (var card in selected)
            player.Hand.Remove(card);

        // Keep every played card internally for trick scoring.
        trickPile.AddRange(selected);

        // But render ONLY the latest play.
        CurrentPlayCards.Clear();
        CurrentPlayCards.AddRange(selected);

        TableCombination = combo;
        lastPlayerWhoPlayed = playerIndex;
        LastPlayPlayerIndex = playerIndex;
        passesInRow = 0;
        player.HasPlayedAnyCard = true;

        if (playedMahJong && mahJongWish is >= 2 and <= 14)
            MahJongWishRank = mahJongWish;

        if (MahJongWishRank.HasValue &&
            ContainsWish(selected, MahJongWishRank.Value))
            MahJongWishRank = null;

        CheckPlayerOut(playerIndex);

        if (!RoundOver)
            CurrentPlayerIndex = NextActivePlayer((playerIndex + 1) % 4);

        return true;
    }

    public bool TryPass(int playerIndex, out string error)
    {
        error = "";

        if (!ExchangeCompleted)
        {
            error = "Complete the exchange first.";
            return false;
        }

        if (playerIndex != CurrentPlayerIndex)
        {
            error = "It is not this player's turn.";
            return false;
        }

        if (TableCombination is null)
        {
            error = "You cannot pass when you have the lead.";
            return false;
        }

        if (MahJongWishRank.HasValue &&
            CanSatisfyWish(playerIndex, MahJongWishRank.Value))
        {
            error = $"You can satisfy the Mah Jong wish ({RankName(MahJongWishRank.Value)}), so you cannot pass.";
            return false;
        }

        passesInRow++;

        int activePlayers = Players.Count(p => !p.IsOut);

        if (passesInRow >= Math.Max(1, activePlayers - 1))
        {
            ResolveTrick();
            return true;
        }

        CurrentPlayerIndex = NextActivePlayer((playerIndex + 1) % 4);
        return true;
    }

    private void ResolveTrick()
    {
        if (lastPlayerWhoPlayed < 0)
            return;

        var winner = Players[lastPlayerWhoPlayed];

        if (trickPile.Any(c => c.Special == SpecialCard.Dragon))
        {
            var receiver = Players
                .Where(p => p.Team != winner.Team)
                .OrderBy(p => p.Hand.Count)
                .First();

            receiver.Captured.AddRange(trickPile);
        }
        else
        {
            winner.Captured.AddRange(trickPile);
        }

        trickPile.Clear();
        CurrentPlayCards.Clear();
        TableCombination = null;
        LastPlayPlayerIndex = null;
        passesInRow = 0;

        CurrentPlayerIndex = winner.IsOut
            ? NextActivePlayer((lastPlayerWhoPlayed + 1) % 4)
            : lastPlayerWhoPlayed;

        lastPlayerWhoPlayed = -1;
    }

    private int NextActivePlayer(int start)
    {
        for (int offset = 0; offset < 4; offset++)
        {
            int index = (start + offset) % 4;

            if (!Players[index].IsOut)
                return index;
        }

        return start;
    }

    private void CheckPlayerOut(int playerIndex)
    {
        var player = Players[playerIndex];

        if (!player.IsOut || player.FinishOrder != 0)
            return;

        FinishCounter++;
        player.FinishOrder = FinishCounter;

        if (FinishCounter == 2)
        {
            var first = Players.First(p => p.FinishOrder == 1);
            var second = Players.First(p => p.FinishOrder == 2);

            if (first.Team == second.Team)
            {
                TeamScores[first.Team] += 200;
                ApplyDeclarations();
                RoundOver = true;
                return;
            }
        }

        if (FinishCounter >= 3)
        {
            ScoreNormalRound();
            RoundOver = true;
        }
    }

    private void ScoreNormalRound()
    {
        var last = Players.First(p => p.FinishOrder == 0);
        var first = Players.First(p => p.FinishOrder == 1);

        TeamScores[1 - last.Team] += last.Hand.Sum(c => c.Points);

        first.Captured.AddRange(last.Captured);
        last.Captured.Clear();

        for (int team = 0; team <= 1; team++)
        {
            TeamScores[team] += Players
                .Where(p => p.Team == team)
                .SelectMany(p => p.Captured)
                .Sum(c => c.Points);
        }

        ApplyDeclarations();
    }

    private void ApplyDeclarations()
    {
        foreach (var player in Players)
        {
            if (player.CalledGrandTichu)
                TeamScores[player.Team] += player.FinishOrder == 1 ? 200 : -200;
            else if (player.CalledTichu)
                TeamScores[player.Team] += player.FinishOrder == 1 ? 100 : -100;
        }
    }

    private (Card Left, Card Partner, Card Right) ChooseBotExchangeCards(int playerIndex)
    {
        var hand = Players[playerIndex].Hand;

        var protectedCards = new HashSet<Card>();

        foreach (var group in hand.Where(c => !c.IsSpecial).GroupBy(c => c.Rank))
        {
            if (group.Count() >= 2)
            {
                foreach (var card in group)
                    protectedCards.Add(card);
            }
        }

        foreach (var card in hand.Where(c =>
                     c.Special is SpecialCard.Phoenix or SpecialCard.Dragon))
            protectedCards.Add(card);

        var available = hand.Where(c => !protectedCards.Contains(c)).ToList();

        if (available.Count < 3)
            available = hand.ToList();

        double Weakness(Card card)
        {
            if (card.Special == SpecialCard.Dog) return -30;
            if (card.Special == SpecialCard.MahJong) return -20;
            if (card.Special == SpecialCard.Phoenix) return 100;
            if (card.Special == SpecialCard.Dragon) return 110;

            double score = SortValue(card);

            if (hand.Count(x => !x.IsSpecial && x.Rank == card.Rank) >= 2)
                score += 10;

            return score;
        }

        var weak = available.OrderBy(Weakness).ToList();

        Card left = weak[0];
        Card right = weak[1];

        Card partner = hand
            .Where(c => c != left && c != right)
            .Where(c => c.Special is not SpecialCard.Phoenix and not SpecialCard.Dragon)
            .OrderByDescending(SortValue)
            .First();

        return (left, partner, right);
    }

    public List<Card>? ChooseBotPlay(int playerIndex)
    {
        if (TableCombination is null)
            return ChooseBestLead(playerIndex);

        var legal = FindLegalResponses(playerIndex);

        if (MahJongWishRank.HasValue &&
            CanSatisfyWish(playerIndex, MahJongWishRank.Value))
        {
            legal = legal
                .Where(c => ContainsWish(c, MahJongWishRank.Value))
                .ToList();
        }

        return legal
            .OrderBy(c => CombinationCost(c, playerIndex))
            .FirstOrDefault();
    }

    public List<Card>? ChooseBotOutOfTurnBomb(int playerIndex)
    {
        if (!ExchangeCompleted ||
            RoundOver ||
            playerIndex == CurrentPlayerIndex ||
            Players[playerIndex].IsOut)
            return null;

        var bombs = FindBombs(Players[playerIndex].Hand)
            .Where(b => CombinationEvaluator.CanBeat(
                CombinationEvaluator.Evaluate(b), TableCombination))
            .ToList();

        if (bombs.Count == 0)
            return null;

        bool opponentDanger = Players.Any(p =>
            p.Team != Players[playerIndex].Team &&
            !p.IsOut &&
            p.Hand.Count <= 3);

        bool selfClose = Players[playerIndex].Hand.Count <= 5;

        if (!opponentDanger && !selfClose && random.NextDouble() > 0.10)
            return null;

        return bombs
            .OrderBy(b => CombinationEvaluator.Evaluate(b).CardCount)
            .ThenBy(b => CombinationEvaluator.Evaluate(b).Value)
            .First();
    }

    private List<Card> ChooseBestLead(int playerIndex)
    {
        var hand = Players[playerIndex].Hand;
        var candidates = new List<List<Card>>();

        var dog = hand.FirstOrDefault(c => c.Special == SpecialCard.Dog);

        if (dog is not null &&
            hand.Count >= 8 &&
            !Players[(playerIndex + 2) % 4].IsOut)
            return new List<Card> { dog };

        candidates.AddRange(FindStraights(hand));
        candidates.AddRange(FindFullHouses(hand));
        candidates.AddRange(FindTriples(hand));
        candidates.AddRange(FindPairs(hand));
        candidates.AddRange(hand.Select(c => new List<Card> { c }));

        return candidates
            .Where(c => CombinationEvaluator.Evaluate(c).Type != ComboType.Invalid)
            .OrderByDescending(c => c.Count)
            .ThenBy(c => CombinationCost(c, playerIndex))
            .First();
    }

    private List<List<Card>> FindLegalResponses(int playerIndex)
    {
        var hand = Players[playerIndex].Hand;
        var result = new List<List<Card>>();
        int targetCount = TableCombination?.CardCount ?? 1;

        if (targetCount <= 7)
        {
            foreach (var cards in Combinations(hand, targetCount))
            {
                var combo = CombinationEvaluator.Evaluate(
                    cards,
                    TableCombination?.Type == ComboType.Single
                        ? TableCombination.Value
                        : 0);

                if (CombinationEvaluator.CanBeat(combo, TableCombination))
                    result.Add(cards);
            }
        }

        foreach (var bomb in FindBombs(hand))
        {
            if (CombinationEvaluator.CanBeat(
                    CombinationEvaluator.Evaluate(bomb),
                    TableCombination))
                result.Add(bomb);
        }

        return result;
    }

    private double CombinationCost(List<Card> cards, int playerIndex)
    {
        var player = Players[playerIndex];
        double cost = 0;

        foreach (var card in cards)
        {
            cost += SortValue(card);

            if (card.Special == SpecialCard.Dragon) cost += 40;
            if (card.Special == SpecialCard.Phoenix) cost += 32;

            if (!card.IsSpecial &&
                player.Hand.Count(x => !x.IsSpecial && x.Rank == card.Rank) >= 3)
                cost += 8;
        }

        cost -= cards.Count * 6;

        if ((player.CalledTichu || player.CalledGrandTichu) &&
            player.Hand.Count <= 6)
            cost -= cards.Count * 6;

        return cost;
    }

    private bool ShouldBotCallGrandTichu(Player player)
    {
        double strength = 0;

        foreach (var card in player.Hand)
        {
            if (card.Special == SpecialCard.Dragon) strength += 4;
            else if (card.Special == SpecialCard.Phoenix) strength += 3;
            else if (!card.IsSpecial && card.Rank == 14) strength += 2.3;
            else if (!card.IsSpecial && card.Rank == 13) strength += 1.5;
            else if (!card.IsSpecial && card.Rank >= 11) strength += 0.7;
        }

        strength += player.Hand
            .Where(c => !c.IsSpecial)
            .GroupBy(c => c.Rank)
            .Count(g => g.Count() >= 2) * 0.8;

        return strength >= 10.5;
    }

    private bool CanSatisfyWish(int playerIndex, int wishRank)
    {
        var hand = Players[playerIndex].Hand;

        if (!hand.Any(c => !c.IsSpecial && c.Rank == wishRank))
            return false;

        int maxCount = Math.Min(7, hand.Count);

        for (int count = 1; count <= maxCount; count++)
        {
            foreach (var cards in Combinations(hand, count))
            {
                if (!ContainsWish(cards, wishRank))
                    continue;

                var combo = CombinationEvaluator.Evaluate(
                    cards,
                    TableCombination?.Type == ComboType.Single
                        ? TableCombination.Value
                        : 0);

                if (CombinationEvaluator.CanBeat(combo, TableCombination))
                    return true;
            }
        }

        return false;
    }

    private static bool ContainsWish(IEnumerable<Card> cards, int rank) =>
        cards.Any(c => !c.IsSpecial && c.Rank == rank);

    public static string RankName(int rank) => rank switch
    {
        11 => "J",
        12 => "Q",
        13 => "K",
        14 => "A",
        _ => rank.ToString()
    };

    private static IEnumerable<List<Card>> FindPairs(List<Card> hand)
    {
        foreach (var group in hand.Where(c => !c.IsSpecial).GroupBy(c => c.Rank))
            if (group.Count() >= 2)
                yield return group.Take(2).ToList();
    }

    private static IEnumerable<List<Card>> FindTriples(List<Card> hand)
    {
        foreach (var group in hand.Where(c => !c.IsSpecial).GroupBy(c => c.Rank))
            if (group.Count() >= 3)
                yield return group.Take(3).ToList();
    }

    private static IEnumerable<List<Card>> FindFullHouses(List<Card> hand)
    {
        var groups = hand
            .Where(c => !c.IsSpecial)
            .GroupBy(c => c.Rank)
            .ToList();

        foreach (var triple in groups.Where(g => g.Count() >= 3))
        {
            foreach (var pair in groups.Where(g => g.Key != triple.Key && g.Count() >= 2))
                yield return triple.Take(3).Concat(pair.Take(2)).ToList();
        }
    }

    private static IEnumerable<List<Card>> FindStraights(List<Card> hand)
    {
        var natural = hand
            .Where(c => !c.IsSpecial)
            .GroupBy(c => c.Rank)
            .Select(g => g.First())
            .OrderBy(c => c.Rank)
            .ToList();

        for (int i = 0; i < natural.Count; i++)
        {
            var run = new List<Card> { natural[i] };

            for (int j = i + 1; j < natural.Count; j++)
            {
                if (natural[j].Rank == run[^1].Rank + 1)
                {
                    run.Add(natural[j]);

                    if (run.Count >= 5)
                        yield return run.ToList();
                }
                else if (natural[j].Rank > run[^1].Rank + 1)
                {
                    break;
                }
            }
        }
    }

    private static IEnumerable<List<Card>> FindBombs(List<Card> hand)
    {
        foreach (var group in hand.Where(c => !c.IsSpecial).GroupBy(c => c.Rank))
            if (group.Count() == 4)
                yield return group.ToList();

        foreach (var suitGroup in hand.Where(c => !c.IsSpecial).GroupBy(c => c.Suit))
        {
            var cards = suitGroup.OrderBy(c => c.Rank).ToList();

            for (int i = 0; i < cards.Count; i++)
            {
                var run = new List<Card> { cards[i] };

                for (int j = i + 1; j < cards.Count; j++)
                {
                    if (cards[j].Rank == run[^1].Rank + 1)
                    {
                        run.Add(cards[j]);

                        if (run.Count >= 5)
                            yield return run.ToList();
                    }
                    else if (cards[j].Rank > run[^1].Rank + 1)
                    {
                        break;
                    }
                }
            }
        }
    }

    private static IEnumerable<List<Card>> Combinations(List<Card> source, int choose)
    {
        if (choose <= 0 || choose > source.Count)
            yield break;

        var buffer = new Card[choose];

        IEnumerable<List<Card>> Recurse(int start, int depth)
        {
            if (depth == choose)
            {
                yield return buffer.ToList();
                yield break;
            }

            for (int i = start; i <= source.Count - (choose - depth); i++)
            {
                buffer[depth] = source[i];

                foreach (var result in Recurse(i + 1, depth + 1))
                    yield return result;
            }
        }

        foreach (var result in Recurse(0, 0))
            yield return result;
    }
}
