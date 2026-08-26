using TichuWinForms_Smooth.Game;
using TichuWinForms_Smooth.Models;

namespace TichuWinForms_Smooth;

public partial class MainForm : Form
{
    private readonly TichuGame game = new();

    private readonly HashSet<Card> selectedCards = new();
    private readonly Dictionary<Card, Panel> handCardPanels = new();

    private bool botsRunning;

    public MainForm()
    {
        InitializeComponent();

        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);

        StartRound();
    }

    private void StartRound()
    {
        selectedCards.Clear();
        game.StartRound();

        RenderHumanHand();
        RenderCurrentPlay();
        UpdateAllStatus();
    }

    // -----------------------------------------------------------------
    // IMPORTANT PERFORMANCE CHANGE:
    // The hand is NOT rebuilt whenever the UI changes.
    // It is rebuilt only when the cards in the human hand actually change.
    // -----------------------------------------------------------------
    private void RenderHumanHand()
    {
        flpHand.SuspendLayout();

        try
        {
            flpHand.Controls.Clear();
            handCardPanels.Clear();

            foreach (var card in game.Players[0].Hand)
            {
                var panel = CreateCardControl(card, selectable: true);
                handCardPanels[card] = panel;
                flpHand.Controls.Add(panel);
            }
        }
        finally
        {
            flpHand.ResumeLayout(true);
        }

        UpdateSelectionLabel();
    }

    // -----------------------------------------------------------------
    // The table renders ONLY the latest play.
    // Previous plays disappear visually, while the game engine keeps
    // the whole trick internally for correct scoring.
    // -----------------------------------------------------------------
    private void RenderCurrentPlay()
    {
        flpCurrentPlay.SuspendLayout();

        try
        {
            flpCurrentPlay.Controls.Clear();

            foreach (var card in game.CurrentPlayCards)
                flpCurrentPlay.Controls.Add(CreateCardControl(card, selectable: false));
        }
        finally
        {
            flpCurrentPlay.ResumeLayout(true);
        }

        if (game.LastPlayPlayerIndex.HasValue)
        {
            lblPlayOwner.Text =
                $"{game.Players[game.LastPlayPlayerIndex.Value].Name} played";
        }
        else
        {
            lblPlayOwner.Text = "";
        }
    }

    private void UpdateAllStatus()
    {
        UpdateScores();
        UpdatePlayerStatusLabels();
        UpdateTableStatus();
        UpdateButtons();
        UpdateSelectionLabel();
    }

    private void UpdateScores()
    {
        lblYourScore.Text = $"Your team  {game.TeamScores[0]}";
        lblOpponentScore.Text = $"Opponents  {game.TeamScores[1]}";
    }

    private void UpdatePlayerStatusLabels()
    {
        lblLeft.Text = PlayerText(game.Players[1]);
        lblPartner.Text = PlayerText(game.Players[2]);
        lblRight.Text = PlayerText(game.Players[3]);
    }

    private static string PlayerText(Player player)
    {
        if (player.FinishOrder > 0)
            return $"{player.Name}\nFinished #{player.FinishOrder}";

        string declaration = player.CalledGrandTichu
            ? " • GRAND TICHU"
            : player.CalledTichu
                ? " • TICHU"
                : "";

        return $"{player.Name}\n{player.Hand.Count} cards{declaration}";
    }

    private void UpdateTableStatus()
    {
        if (game.RoundOver)
        {
            lblTurn.Text = game.MatchOver ? "MATCH OVER" : "ROUND OVER";

            lblCombination.Text = game.MatchOver
                ? (game.TeamScores[0] > game.TeamScores[1]
                    ? "Your team wins the match."
                    : "Opponents win the match.")
                : "Press NEW ROUND to continue.";

            lblWish.Text = "";
            return;
        }

        if (!game.ExchangeCompleted)
        {
            lblTurn.Text = "EXCHANGE PHASE";
            lblCombination.Text = "Select exactly 3 cards and press EXCHANGE 3.";
            lblWish.Text = "";
            return;
        }

        var current = game.Players[game.CurrentPlayerIndex];

        lblTurn.Text = current.IsHuman
            ? "YOUR TURN"
            : $"{current.Name.ToUpperInvariant()}'S TURN";

        lblCombination.Text = game.TableCombination is null
            ? "New trick — any legal combination can be played."
            : $"{FriendlyCombo(game.TableCombination.Type)} • value {game.TableCombination.Value}";

        lblWish.Text = game.MahJongWishRank.HasValue
            ? $"MAH JONG WISH: {TichuGame.RankName(game.MahJongWishRank.Value)}"
            : "";
    }

    private static string FriendlyCombo(ComboType type) => type switch
    {
        ComboType.ConsecutivePairs => "Consecutive pairs",
        ComboType.FourBomb => "Four-card bomb",
        ComboType.StraightFlushBomb => "Straight-flush bomb",
        ComboType.FullHouse => "Full house",
        _ => type.ToString()
    };

    private void UpdateButtons()
    {
        bool exchangePhase = !game.ExchangeCompleted && !game.RoundOver;
        bool humanTurn =
            game.ExchangeCompleted &&
            !game.RoundOver &&
            game.CurrentPlayerIndex == 0;

        btnExchange.Visible = exchangePhase;
        btnExchange.Enabled = exchangePhase && selectedCards.Count == 3;

        btnPlay.Visible = !exchangePhase;
        btnPass.Visible = !exchangePhase;
        btnBomb.Visible = !exchangePhase;

        btnPlay.Enabled = humanTurn;
        btnPass.Enabled = humanTurn && game.TableCombination is not null;

        btnBomb.Enabled =
            game.ExchangeCompleted &&
            !game.RoundOver &&
            selectedCards.Count > 0;

        btnTichu.Enabled =
            !game.RoundOver &&
            !game.Players[0].HasPlayedAnyCard &&
            !game.Players[0].CalledTichu &&
            !game.Players[0].CalledGrandTichu;

        btnGrandTichu.Enabled =
            exchangePhase &&
            !game.Players[0].CalledGrandTichu &&
            !game.Players[0].CalledTichu;
    }

    private void ToggleCard(Card card)
    {
        if (game.RoundOver)
            return;

        if (!game.ExchangeCompleted &&
            !selectedCards.Contains(card) &&
            selectedCards.Count >= 3)
            return;

        if (!selectedCards.Add(card))
            selectedCards.Remove(card);

        // No RenderHumanHand() here.
        // Only update the affected card panel.
        ApplyCardSelectionVisual(card);

        UpdateSelectionLabel();
        UpdateButtons();
    }

    private void ApplyCardSelectionVisual(Card card)
    {
        if (!handCardPanels.TryGetValue(card, out var panel))
            return;

        bool selected = selectedCards.Contains(card);

        panel.Margin = selected
            ? new Padding(3, 2, 3, 18)
            : new Padding(3, 18, 3, 2);

        panel.BackColor = selected
            ? Color.FromArgb(255, 244, 204)
            : Color.White;
    }

    private void UpdateSelectionLabel()
    {
        if (!game.ExchangeCompleted)
        {
            lblSelection.Text =
                $"{selectedCards.Count}/3 cards selected for exchange.";
            return;
        }

        if (selectedCards.Count == 0)
        {
            lblSelection.Text =
                "Select cards. Use BOMB for a valid bomb, even out of turn.";
            return;
        }

        double previousSingle = game.TableCombination?.Type == ComboType.Single
            ? game.TableCombination.Value
            : 0;

        var combo = CombinationEvaluator.Evaluate(
            selectedCards.ToList(),
            previousSingle);

        lblSelection.Text = combo.Type == ComboType.Invalid
            ? $"{selectedCards.Count} selected • invalid combination"
            : $"{selectedCards.Count} selected • {FriendlyCombo(combo.Type)}";
    }

    private Panel CreateCardControl(Card card, bool selectable)
    {
        var panel = new Panel
        {
            Width = 66,
            Height = 118,
            Margin = new Padding(3, 18, 3, 2),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Cursor = selectable ? Cursors.Hand : Cursors.Default,
            Tag = card
        };

        var rank = new Label
        {
            Width = 58,
            Height = 30,
            Left = 4,
            Top = 5,
            Text = card.RankText,
            Font = new Font(
                "Segoe UI",
                card.IsSpecial ? 9F : 14F,
                FontStyle.Bold),
            ForeColor = CardColor(card),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };

        var suit = new Label
        {
            Width = 58,
            Height = 62,
            Left = 4,
            Top = 40,
            Text = card.IsSpecial
                ? SpecialSymbol(card)
                : card.SuitSymbol,
            Font = new Font(
                "Segoe UI Symbol",
                card.IsSpecial ? 19F : 27F,
                FontStyle.Bold),
            ForeColor = CardColor(card),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };

        panel.Controls.Add(rank);
        panel.Controls.Add(suit);

        if (selectable)
        {
            void Toggle(object? _, EventArgs __) => ToggleCard(card);

            panel.Click += Toggle;
            rank.Click += Toggle;
            suit.Click += Toggle;
        }

        return panel;
    }

    private static Color CardColor(Card card)
    {
        if (card.Special == SpecialCard.Dragon) return Color.DarkOrange;
        if (card.Special == SpecialCard.Phoenix) return Color.MediumVioletRed;
        if (card.Special == SpecialCard.Dog) return Color.SaddleBrown;
        if (card.Special == SpecialCard.MahJong) return Color.DarkGreen;

        return card.Suit is Suit.Pagoda or Suit.Star
            ? Color.Firebrick
            : Color.FromArgb(24, 24, 24);
    }

    private static string SpecialSymbol(Card card) => card.Special switch
    {
        SpecialCard.Dragon => "龍",
        SpecialCard.Phoenix => "鳳",
        SpecialCard.Dog => "犬",
        SpecialCard.MahJong => "一",
        _ => ""
    };

    private int? AskMahJongWishIfNeeded(IEnumerable<Card> cards)
    {
        if (!cards.Any(c => c.Special == SpecialCard.MahJong))
            return null;

        using var dialog = new Form
        {
            Text = "Mah Jong Wish",
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(300, 155),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var label = new Label
        {
            Text = "Choose the wished rank:",
            Left = 20,
            Top = 20,
            Width = 250
        };

        var combo = new ComboBox
        {
            Left = 20,
            Top = 50,
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        for (int rank = 2; rank <= 14; rank++)
            combo.Items.Add(new RankChoice(rank));

        combo.SelectedIndex = 0;

        var ok = new Button
        {
            Text = "Wish",
            Left = 170,
            Top = 100,
            Width = 100,
            DialogResult = DialogResult.OK
        };

        dialog.Controls.Add(label);
        dialog.Controls.Add(combo);
        dialog.Controls.Add(ok);
        dialog.AcceptButton = ok;

        return dialog.ShowDialog(this) == DialogResult.OK
            ? ((RankChoice)combo.SelectedItem!).Rank
            : null;
    }

    private void btnPlay_Click(object? sender, EventArgs e)
    {
        if (game.CurrentPlayerIndex != 0 || !game.ExchangeCompleted)
            return;

        var cards = selectedCards.ToList();

        if (cards.Count == 0)
            return;

        int? wish = AskMahJongWishIfNeeded(cards);

        if (cards.Any(c => c.Special == SpecialCard.MahJong) && wish is null)
            return;

        if (!game.TryPlayCards(0, cards, out string error, wish))
        {
            ShowInfo(error);
            return;
        }

        selectedCards.Clear();

        // Human hand changed -> rebuild hand once.
        RenderHumanHand();

        // Latest table play changed -> rebuild only center cards.
        RenderCurrentPlay();

        UpdateAllStatus();
        BeginBotsIfNeeded();
    }

    private void btnPass_Click(object? sender, EventArgs e)
    {
        if (!game.TryPass(0, out string error))
        {
            ShowInfo(error);
            return;
        }

        selectedCards.Clear();
        ClearSelectionVisuals();

        RenderCurrentPlay();
        UpdateAllStatus();

        BeginBotsIfNeeded();
    }

    private void btnBomb_Click(object? sender, EventArgs e)
    {
        var cards = selectedCards.ToList();

        if (cards.Count == 0)
            return;

        var combo = CombinationEvaluator.Evaluate(cards);

        if (!combo.IsBomb)
        {
            ShowInfo("The selected cards are not a bomb.");
            return;
        }

        bool outOfTurn = game.CurrentPlayerIndex != 0;

        if (!game.TryPlayCards(
                0,
                cards,
                out string error,
                null,
                outOfTurn))
        {
            ShowInfo(error);
            return;
        }

        selectedCards.Clear();
        RenderHumanHand();
        RenderCurrentPlay();
        UpdateAllStatus();

        BeginBotsIfNeeded();
    }

    private void btnExchange_Click(object? sender, EventArgs e)
    {
        if (selectedCards.Count != 3)
            return;

        using var dialog = new ExchangeAssignmentForm(selectedCards.ToList());

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        if (!game.CompleteHumanExchange(
                dialog.ToLeft!,
                dialog.ToPartner!,
                dialog.ToRight!,
                out string error))
        {
            ShowInfo(error);
            return;
        }

        selectedCards.Clear();

        RenderHumanHand();
        RenderCurrentPlay();
        UpdateAllStatus();

        BeginBotsIfNeeded();
    }

    private void btnTichu_Click(object? sender, EventArgs e)
    {
        if (!game.CallTichu(0, out string error))
        {
            ShowInfo(error);
            return;
        }

        UpdatePlayerStatusLabels();
        UpdateButtons();
    }

    private void btnGrandTichu_Click(object? sender, EventArgs e)
    {
        if (!game.CallGrandTichu(0, out string error))
        {
            ShowInfo(error);
            return;
        }

        UpdatePlayerStatusLabels();
        UpdateButtons();
    }

    private void btnNewRound_Click(object? sender, EventArgs e)
    {
        if (game.MatchOver)
        {
            var answer = MessageBox.Show(
                "Start a new match from 0 - 0?",
                "New Match",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
                return;

            game.TeamScores[0] = 0;
            game.TeamScores[1] = 0;
        }

        StartRound();
    }

    private async void BeginBotsIfNeeded()
    {
        if (botsRunning ||
            game.RoundOver ||
            !game.ExchangeCompleted)
            return;

        botsRunning = true;

        try
        {
            while (!game.RoundOver &&
                   game.ExchangeCompleted &&
                   game.CurrentPlayerIndex != 0)
            {
                // Bots may bomb out of turn.
                bool interrupted = false;

                for (int bot = 1; bot < 4; bot++)
                {
                    if (bot == game.CurrentPlayerIndex ||
                        game.Players[bot].IsOut)
                        continue;

                    var bomb = game.ChooseBotOutOfTurnBomb(bot);

                    if (bomb is null)
                        continue;

                    await Task.Delay(180);

                    if (game.TryPlayCards(bot, bomb, out _, null, true))
                    {
                        interrupted = true;

                        // Only update the controls that changed.
                        RenderCurrentPlay();
                        UpdatePlayerStatusLabels();
                        UpdateTableStatus();
                        UpdateButtons();

                        break;
                    }
                }

                if (interrupted)
                    continue;

                await Task.Delay(420);

                int currentBot = game.CurrentPlayerIndex;
                var play = game.ChooseBotPlay(currentBot);

                if (play is not null)
                {
                    int? wish = play.Any(c => c.Special == SpecialCard.MahJong)
                        ? ChooseBotWish(currentBot)
                        : null;

                    if (!game.TryPlayCards(
                            currentBot,
                            play,
                            out _,
                            wish))
                    {
                        game.TryPass(currentBot, out _);
                    }
                }
                else
                {
                    game.TryPass(currentBot, out _);
                }

                // Human hand did not change during a bot move.
                // Do NOT rebuild it.
                RenderCurrentPlay();
                UpdateScores();
                UpdatePlayerStatusLabels();
                UpdateTableStatus();
                UpdateButtons();
            }
        }
        finally
        {
            botsRunning = false;
        }
    }

    private int ChooseBotWish(int botIndex)
    {
        var held = game.Players[botIndex].Hand
            .Where(c => !c.IsSpecial)
            .Select(c => c.Rank)
            .ToHashSet();

        for (int rank = 14; rank >= 6; rank--)
            if (!held.Contains(rank))
                return rank;

        return 14;
    }

    private void ClearSelectionVisuals()
    {
        foreach (var card in selectedCards.ToList())
        {
            selectedCards.Remove(card);
            ApplyCardSelectionVisual(card);
        }

        UpdateSelectionLabel();
    }

    private void ShowInfo(string text)
    {
        MessageBox.Show(
            text,
            "Tichu",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private sealed class RankChoice
    {
        public int Rank { get; }

        public RankChoice(int rank)
        {
            Rank = rank;
        }

        public override string ToString() => TichuGame.RankName(Rank);
    }
}

internal sealed class ExchangeAssignmentForm : Form
{
    private readonly ComboBox cmbLeft = new();
    private readonly ComboBox cmbPartner = new();
    private readonly ComboBox cmbRight = new();

    public Card? ToLeft { get; private set; }
    public Card? ToPartner { get; private set; }
    public Card? ToRight { get; private set; }

    public ExchangeAssignmentForm(List<Card> cards)
    {
        Text = "Exchange Cards";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(390, 245);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        Controls.Add(new Label
        {
            Text = "Assign one selected card to each player:",
            Left = 20,
            Top = 18,
            Width = 340
        });

        SetupRow("Bot Left:", cmbLeft, 60, cards);
        SetupRow("Partner:", cmbPartner, 105, cards);
        SetupRow("Bot Right:", cmbRight, 150, cards);

        cmbLeft.SelectedIndex = 0;
        cmbPartner.SelectedIndex = 1;
        cmbRight.SelectedIndex = 2;

        var confirm = new Button
        {
            Text = "Exchange",
            Left = 250,
            Top = 195,
            Width = 110
        };

        confirm.Click += (_, _) =>
        {
            var left = (Card)cmbLeft.SelectedItem!;
            var partner = (Card)cmbPartner.SelectedItem!;
            var right = (Card)cmbRight.SelectedItem!;

            if (new[] { left, partner, right }.Distinct().Count() != 3)
            {
                MessageBox.Show(
                    "Each player must receive a different card.",
                    "Exchange",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            ToLeft = left;
            ToPartner = partner;
            ToRight = right;

            DialogResult = DialogResult.OK;
            Close();
        };

        Controls.Add(confirm);
    }

    private void SetupRow(
        string title,
        ComboBox combo,
        int top,
        List<Card> cards)
    {
        Controls.Add(new Label
        {
            Text = title,
            Left = 20,
            Top = top + 4,
            Width = 90
        });

        combo.Left = 115;
        combo.Top = top;
        combo.Width = 245;
        combo.DropDownStyle = ComboBoxStyle.DropDownList;

        foreach (var card in cards)
            combo.Items.Add(card);

        Controls.Add(combo);
    }
}
