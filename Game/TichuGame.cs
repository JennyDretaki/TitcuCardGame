using TichuWinForms.Models;

namespace TichuWinForms.Game;

public sealed class TichuGame
{
    private readonly Random random = new();

    private int passesInRow;
    private int lastPlayerWhoPlayed = -1;

    public List<Player> Players { get; } = new();
    public int CurrentPlayerIndex { get; private set; }
    public Combination? TableCombination { get; private set; }
    public List<Card> TableCards { get; } = new();

    public int[] TeamScores { get; } = new int[2];
    public int FinishCounter { get; private set; }
    public bool RoundOver { get; private set; }
    public bool MatchOver => TeamScores[0] >= 1000 || TeamScores[1] >= 1000;

    public bool ExchangeCompleted { get; private set; }
    public bool HumanMayCallGrandTichu { get; private set; }

    public int? MahJongWishRank { get; private set; }

    public event Action<string>? Message;

    public TichuGame()
    {
        Players.Add(new Player { Name = "You", Seat = 0, IsHuman = true });
        Players.Add(new Player { Name = "Bot Left", Seat = 1 });
        Players.Add(new Player { Name = "Partner", Seat = 2 });
        Players.Add(new Player { Name = "Bot Right", Seat = 3 });
    }

    public void StartRound()
    {
        foreach (var p in Players)
        {
            p.Hand.Clear();
            p.Captured.Clear();
            p.FinishOrder = 0;
            p.CalledTichu = false;
            p.CalledGrandTichu = false;
            p.HasPlayedAnyCard = false;
        }

        TableCards.Clear();
        TableCombination = null;
        passesInRow = 0;
        lastPlayerWhoPlayed = -1;
        FinishCounter = 0;
        RoundOver = false;
        ExchangeCompleted = false;
        MahJongWishRank = null;

        var deck = DeckFactory.CreateDeck();
        DeckFactory.Shuffle(deck, random);

        // Deal first 8 cards to everyone.
        for (int i = 0; i < 32; i++)
            Players[i % 4].Hand.Add(deck[i]);

        foreach (var p in Players)
            SortHand(p.Hand);

        HumanMayCallGrandTichu = true;

        // Bots decide Grand Tichu from first 8 cards.
        for (int i = 1; i < 4; i++)
        {
            if (ShouldBotCallGrandTichu(Players[i]))
            {
                Players[i].CalledGrandTichu = true;
                Message?.Invoke($"{Players[i].Name} called GRAND TICHU!");
            }
        }

        // Complete the deal to 14 cards each.
        for (int i = 32; i < deck.Count; i++)
            Players[i % 4].Hand.Add(deck[i]);

        foreach (var p in Players)
            SortHand(p.Hand);

        HumanMayCallGrandTichu = false;
        CurrentPlayerIndex = Players.FindIndex(p =>
            p.Hand.Any(c => c.Special == SpecialCard.MahJong));

        Message?.Invoke("Cards dealt. Select exactly 3 cards for the exchange.");
    }

    public static void SortHand(List<Card> hand)
    {
        hand.Sort((a, b) =>
        {
            int av = SortValue(a);
            int bv = SortValue(b);
            int cmp = av.CompareTo(bv);
            return cmp != 0 ? cmp : a.Suit.CompareTo(b.Suit);
        });
    }

    public static int SortValue(Card c) => c.Special switch
    {
        SpecialCard.Dog => 0,
        SpecialCard.MahJong => 1,
        SpecialCard.Phoenix => 15,
        SpecialCard.Dragon => 16,
        _ => c.Rank
    };

    public bool CallGrandTichu(int playerIndex, out string error)
    {
        error = "";

        if (playerIndex != 0)
        {
            error = "Only the human player calls Grand Tichu from the UI.";
            return false;
        }

        var p = Players[playerIndex];

        if (!HumanMayCallGrandTichu)
        {
            // Usability concession: allow it until exchange is complete.
            if (ExchangeCompleted)
            {
                error = "Grand Tichu is no longer available.";
                return false;
            }
        }

        if (p.CalledGrandTichu || p.CalledTichu)
        {
            error = "You already called Tichu / Grand Tichu.";
            return false;
        }

        if (p.HasPlayedAnyCard)
        {
            error = "Grand Tichu must be called before playing.";
            return false;
        }

        p.CalledGrandTichu = true;
        Message?.Invoke($"{p.Name} called GRAND TICHU!");
        return true;
    }

    public bool CallTichu(int playerIndex, out string error)
    {
        error = "";
        var p = Players[playerIndex];

        if (p.HasPlayedAnyCard)
        {
            error = "Tichu must be called before playing your first card.";
            return false;
        }

        if (p.CalledTichu || p.CalledGrandTichu)
        {
            error = "A Tichu declaration has already been made.";
            return false;
        }

        p.CalledTichu = true;
        Message?.Invoke($"{p.Name} called TICHU!");
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
            error = "The exchange is already complete.";
            return false;
        }

        var human = Players[0];
        var chosen = new[] { toLeft, toPartner, toRight };

        if (chosen.Distinct().Count() != 3 || chosen.Any(c => !human.Hand.Contains(c)))
        {
            error = "Select three different cards from your hand.";
            return false;
        }

        var outgoing = new Dictionary<(int from, int to), Card>
        {
            [(0, 1)] = toLeft,
            [(0, 2)] = toPartner,
            [(0, 3)] = toRight
        };

        for (int from = 1; from < 4; from++)
        {
            var gifts = ChooseBotExchangeCards(from);

            int left = (from + 1) % 4;
            int partner = (from + 2) % 4;
            int right = (from + 3) % 4;

            outgoing[(from, left)] = gifts.left;
            outgoing[(from, partner)] = gifts.partner;
            outgoing[(from, right)] = gifts.right;
        }

        foreach (var x in outgoing)
            Players[x.Key.from].Hand.Remove(x.Value);

        foreach (var x in outgoing)
            Players[x.Key.to].Hand.Add(x.Value);

        foreach (var p in Players)
            SortHand(p.Hand);

        ExchangeCompleted = true;

        CurrentPlayerIndex = Players.FindIndex(
            p => p.Hand.Any(c => c.Special == SpecialCard.MahJong));

        Message?.Invoke("3-card exchange completed.");
        Message?.Invoke($"{Players[CurrentPlayerIndex].Name} starts with Mah Jong.");
        return true;
    }

    private (Card left, Card partner, Card right) ChooseBotExchangeCards(int playerIndex)
    {
        var p = Players[playerIndex];
        var cards = p.Hand.ToList();

        // AI principle:
        // - preserve bombs, Phoenix, Dragon and strong pair/triple structure
        // - send weakest isolated cards to opponents
        // - give partner a useful medium/high card if possible

        var protectedCards = FindStrategicProtectedCards(cards);
        var available = cards.Where(c => !protectedCards.Contains(c)).ToList();

        if (available.Count < 3)
            available = cards.ToList();

        double Weakness(Card c)
        {
            double value = SortValue(c);

            if (c.Special == SpecialCard.Dog) return -20;
            if (c.Special == SpecialCard.MahJong) return -10;
            if (c.Special == SpecialCard.Phoenix) return 100;
            if (c.Special == SpecialCard.Dragon) return 110;

            int sameRank = cards.Count(x => !x.IsSpecial && x.Rank == c.Rank);
            if (sameRank >= 2) value += 8;

            return value;
        }

        var ordered = available.OrderBy(Weakness).ToList();

        Card left = ordered[0];
        Card right = ordered.Count > 1 ? ordered[1] : cards.First(c => c != left);

        var partnerCandidates = cards
            .Where(c => c != left && c != right)
            .Where(c => c.Special is not SpecialCard.Phoenix and not SpecialCard.Dragon)
            .OrderByDescending(c => SortValue(c))
            .ToList();

        Card partner = partnerCandidates.FirstOrDefault()
            ?? cards.First(c => c != left && c != right);

        return (left, partner, right);
    }

    private static HashSet<Card> FindStrategicProtectedCards(List<Card> cards)
    {
        var protectedCards = new HashSet<Card>();

        foreach (var g in cards.Where(c => !c.IsSpecial).GroupBy(c => c.Rank))
        {
            if (g.Count() >= 2)
                foreach (var c in g)
                    protectedCards.Add(c);
        }

        foreach (var c in cards.Where(c =>
                     c.Special is SpecialCard.Phoenix or SpecialCard.Dragon))
            protectedCards.Add(c);

        return protectedCards;
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
            error = "The round has finished.";
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
            error = "That is not a valid Tichu combination.";
            return false;
        }

        if (outOfTurnBomb && !combo.IsBomb)
        {
            error = "Only a bomb may be played out of turn.";
            return false;
        }

        if (MahJongWishRank.HasValue &&
            CanSatisfyWish(playerIndex, MahJongWishRank.Value) &&
            !ContainsWish(selected, MahJongWishRank.Value))
        {
            error = $"Mah Jong wish is active. You must play rank {RankName(MahJongWishRank.Value)} if possible.";
            return false;
        }

        if (combo.Type == ComboType.Dog)
        {
            if (TableCombination is not null)
            {
                error = "Dog can only be played as the lead.";
                return false;
            }

            player.Hand.Remove(selected[0]);
            player.HasPlayedAnyCard = true;

            Message?.Invoke($"{player.Name} played Dog.");
            CheckPlayerOut(playerIndex);

            TableCards.Clear();
            TableCombination = null;
            passesInRow = 0;
            lastPlayerWhoPlayed = -1;

            CurrentPlayerIndex = NextActivePlayer((playerIndex + 2) % 4);
            Message?.Invoke($"Lead passes to {Players[CurrentPlayerIndex].Name}.");
            return true;
        }

        if (!CombinationEvaluator.CanBeat(combo, TableCombination))
        {
            error = TableCombination is null
                ? "Invalid lead."
                : $"Your play must beat {TableCombination.Type} ({TableCombination.Value}).";
            return false;
        }

        bool playedMahJong = selected.Any(c => c.Special == SpecialCard.MahJong);

        foreach (var card in selected)
            player.Hand.Remove(card);

        if (outOfTurnBomb)
        {
            // Bomb interrupts immediately and takes control of the trick.
            Message?.Invoke($"{player.Name} INTERRUPTED with a BOMB!");
            passesInRow = 0;
        }

        TableCards.AddRange(selected);
        TableCombination = combo;
        lastPlayerWhoPlayed = playerIndex;
        passesInRow = 0;
        player.HasPlayedAnyCard = true;

        if (playedMahJong && mahJongWish is >= 2 and <= 14)
        {
            MahJongWishRank = mahJongWish;
            Message?.Invoke(
                $"{player.Name} wishes for {RankName(mahJongWish.Value)}.");
        }

        if (MahJongWishRank.HasValue &&
            ContainsWish(selected, MahJongWishRank.Value))
        {
            Message?.Invoke(
                $"Mah Jong wish for {RankName(MahJongWishRank.Value)} was fulfilled.");
            MahJongWishRank = null;
        }

        Message?.Invoke(
            $"{player.Name} played {combo.Type}: {string.Join(" ", selected)}");

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
            error = $"You cannot pass because you can satisfy the Mah Jong wish ({RankName(MahJongWishRank.Value)}).";
            return false;
        }

        Message?.Invoke($"{Players[playerIndex].Name} passed.");
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

        // MVP behaviour: Dragon trick is automatically given to the opponent
        // with fewer cards. This isn't one of the requested new features.
        if (TableCards.Any(c => c.Special == SpecialCard.Dragon))
        {
            var opponents = Players
                .Where(p => p.Team != winner.Team)
                .OrderBy(p => p.Hand.Count)
                .ToList();

            opponents[0].Captured.AddRange(TableCards);
            Message?.Invoke(
                $"{winner.Name} won with Dragon; trick given to {opponents[0].Name}.");
        }
        else
        {
            winner.Captured.AddRange(TableCards);
            Message?.Invoke($"{winner.Name} won the trick.");
        }

        TableCards.Clear();
        TableCombination = null;
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
            int idx = (start + offset) % 4;
            if (!Players[idx].IsOut)
                return idx;
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
        Message?.Invoke($"{player.Name} finished #{FinishCounter}.");

        if (FinishCounter == 2)
        {
            var first = Players.First(p => p.FinishOrder == 1);
            var second = Players.First(p => p.FinishOrder == 2);

            if (first.Team == second.Team)
            {
                TeamScores[first.Team] += 200;
                ApplyTichuBets();
                RoundOver = true;
                Message?.Invoke(
                    $"Double victory! Team {first.Team + 1} scores 200.");
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

        int remainingPoints = last.Hand.Sum(c => c.Points);
        TeamScores[1 - last.Team] += remainingPoints;

        first.Captured.AddRange(last.Captured);
        last.Captured.Clear();

        for (int team = 0; team <= 1; team++)
        {
            TeamScores[team] += Players
                .Where(p => p.Team == team)
                .SelectMany(p => p.Captured)
                .Sum(c => c.Points);
        }

        ApplyTichuBets();
        Message?.Invoke("Round scoring completed.");
    }

    private void ApplyTichuBets()
    {
        foreach (var p in Players)
        {
            if (p.CalledGrandTichu)
                TeamScores[p.Team] += p.FinishOrder == 1 ? 200 : -200;
            else if (p.CalledTichu)
                TeamScores[p.Team] += p.FinishOrder == 1 ? 100 : -100;
        }
    }

    // -------------------- MAH JONG WISH --------------------

    private bool CanSatisfyWish(int playerIndex, int wishRank)
    {
        var hand = Players[playerIndex].Hand;

        if (!hand.Any(c => !c.IsSpecial && c.Rank == wishRank))
            return false;

        // Test many possible legal plays that contain the wished rank.
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

    // -------------------- BOT AI --------------------

    public List<Card>? ChooseBotPlay(int playerIndex)
    {
        var player = Players[playerIndex];
        var hand = player.Hand;

        if (TableCombination is null)
            return ChooseBestLead(playerIndex);

        var legal = FindLegalResponses(playerIndex);

        if (legal.Count == 0)
            return null;

        // If wish is active, only candidates fulfilling it remain.
        if (MahJongWishRank.HasValue && CanSatisfyWish(playerIndex, MahJongWishRank.Value))
            legal = legal.Where(c => ContainsWish(c, MahJongWishRank.Value)).ToList();

        if (legal.Count == 0)
            return null;

        // Prefer the cheapest winning combination.
        return legal
            .OrderBy(c => CombinationCost(c, playerIndex))
            .First();
    }

    public List<Card>? ChooseBotOutOfTurnBomb(int playerIndex)
    {
        if (!ExchangeCompleted || RoundOver || playerIndex == CurrentPlayerIndex)
            return null;

        var bombs = FindBombs(Players[playerIndex].Hand)
            .Where(b => CombinationEvaluator.CanBeat(
                CombinationEvaluator.Evaluate(b), TableCombination))
            .ToList();

        if (bombs.Count == 0)
            return null;

        // Strategic use:
        // bomb more readily if an opponent is close to going out,
        // or bot itself has few cards remaining.
        bool danger = Players.Any(p =>
            p.Team != Players[playerIndex].Team &&
            !p.IsOut &&
            p.Hand.Count <= 3);

        bool selfClose = Players[playerIndex].Hand.Count <= 5;

        if (!danger && !selfClose && random.NextDouble() > 0.12)
            return null;

        return bombs
            .OrderBy(b => CombinationEvaluator.Evaluate(b).CardCount)
            .ThenBy(b => CombinationEvaluator.Evaluate(b).Value)
            .First();
    }

    private List<Card> ChooseBestLead(int playerIndex)
    {
        var p = Players[playerIndex];
        var hand = p.Hand;

        // Dog is good when partner can profit and bot still has many cards.
        var dog = hand.FirstOrDefault(c => c.Special == SpecialCard.Dog);
        if (dog is not null && hand.Count >= 8 && !Players[(playerIndex + 2) % 4].IsOut)
            return new List<Card> { dog };

        var candidates = new List<List<Card>>();

        // Preserve bombs unless close to finishing.
        candidates.AddRange(FindPairs(hand));
        candidates.AddRange(FindTriples(hand));
        candidates.AddRange(FindFullHouses(hand));
        candidates.AddRange(FindStraights(hand));
        candidates.AddRange(hand.Select(c => new List<Card> { c }));

        candidates = candidates
            .Where(c => CombinationEvaluator.Evaluate(c).Type != ComboType.Invalid)
            .ToList();

        // Prefer shedding more cards, but avoid wasting premium cards.
        return candidates
            .OrderByDescending(c => c.Count)
            .ThenBy(c => CombinationCost(c, playerIndex))
            .First();
    }

    private List<List<Card>> FindLegalResponses(int playerIndex)
    {
        var hand = Players[playerIndex].Hand;
        var result = new List<List<Card>>();

        int targetCount = TableCombination?.CardCount ?? 1;

        // Generate exact card-count combinations for ordinary responses.
        // Cap at 7 because Tichu hands are only 14 cards, and this is sufficient
        // for most normal play while keeping bot turns responsive.
        if (targetCount <= 7)
        {
            foreach (var set in Combinations(hand, targetCount))
            {
                var combo = CombinationEvaluator.Evaluate(
                    set,
                    TableCombination?.Type == ComboType.Single
                        ? TableCombination.Value
                        : 0);

                if (CombinationEvaluator.CanBeat(combo, TableCombination))
                    result.Add(set);
            }
        }

        // Bombs may beat a non-bomb regardless of card count.
        foreach (var bomb in FindBombs(hand))
        {
            var combo = CombinationEvaluator.Evaluate(bomb);
            if (CombinationEvaluator.CanBeat(combo, TableCombination))
                result.Add(bomb);
        }

        return result;
    }

    private double CombinationCost(List<Card> cards, int playerIndex)
    {
        var p = Players[playerIndex];
        double cost = 0;

        foreach (var c in cards)
        {
            cost += SortValue(c);

            if (c.Special == SpecialCard.Dragon) cost += 40;
            if (c.Special == SpecialCard.Phoenix) cost += 32;
            if (c.Special == SpecialCard.Dog) cost -= 10;

            if (!c.IsSpecial)
            {
                int sameRank = p.Hand.Count(x => !x.IsSpecial && x.Rank == c.Rank);
                if (sameRank >= 3)
                    cost += 8;
            }
        }

        // Shedding cards is valuable.
        cost -= cards.Count * 6;

        // If bot called Tichu and has few cards, become more aggressive.
        if ((p.CalledTichu || p.CalledGrandTichu) && p.Hand.Count <= 6)
            cost -= cards.Count * 6;

        return cost;
    }

    private bool ShouldBotCallGrandTichu(Player p)
    {
        double strength = 0;

        foreach (var c in p.Hand)
        {
            if (c.Special == SpecialCard.Dragon) strength += 4;
            else if (c.Special == SpecialCard.Phoenix) strength += 3;
            else if (!c.IsSpecial && c.Rank == 14) strength += 2.3;
            else if (!c.IsSpecial && c.Rank == 13) strength += 1.5;
            else if (!c.IsSpecial && c.Rank >= 11) strength += 0.7;
        }

        var groups = p.Hand
            .Where(c => !c.IsSpecial)
            .GroupBy(c => c.Rank)
            .Select(g => g.Count());

        strength += groups.Count(x => x >= 2) * 0.8;
        strength += groups.Count(x => x >= 3) * 1.2;

        return strength >= 10.5;
    }

    private static IEnumerable<List<Card>> FindPairs(List<Card> hand)
    {
        foreach (var g in hand.Where(c => !c.IsSpecial).GroupBy(c => c.Rank))
            if (g.Count() >= 2)
                yield return g.Take(2).ToList();
    }

    private static IEnumerable<List<Card>> FindTriples(List<Card> hand)
    {
        foreach (var g in hand.Where(c => !c.IsSpecial).GroupBy(c => c.Rank))
            if (g.Count() >= 3)
                yield return g.Take(3).ToList();
    }

    private static IEnumerable<List<Card>> FindFullHouses(List<Card> hand)
    {
        var groups = hand
            .Where(c => !c.IsSpecial)
            .GroupBy(c => c.Rank)
            .ToList();

        foreach (var triple in groups.Where(g => g.Count() >= 3))
        {
            foreach (var pair in groups.Where(g =>
                         g.Key != triple.Key && g.Count() >= 2))
            {
                yield return triple.Take(3).Concat(pair.Take(2)).ToList();
            }
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
        foreach (var g in hand
                     .Where(c => !c.IsSpecial)
                     .GroupBy(c => c.Rank))
        {
            if (g.Count() == 4)
                yield return g.ToList();
        }

        foreach (var suitGroup in hand
                     .Where(c => !c.IsSpecial)
                     .GroupBy(c => c.Suit))
        {
            var cards = suitGroup
                .OrderBy(c => c.Rank)
                .ToList();

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

    private static IEnumerable<List<Card>> Combinations(
        List<Card> source,
        int choose)
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

            for (int i = start;
                 i <= source.Count - (choose - depth);
                 i++)
            {
                buffer[depth] = source[i];

                foreach (var item in Recurse(i + 1, depth + 1))
                    yield return item;
            }
        }

        foreach (var x in Recurse(0, 0))
            yield return x;
    }
}
