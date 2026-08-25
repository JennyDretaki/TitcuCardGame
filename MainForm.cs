using TichuWinForms.Game;
using TichuWinForms.Models;

namespace TichuWinForms;

public partial class MainForm : Form
{
    private readonly TichuGame game = new();
    private readonly HashSet<Card> selectedCards = new();
    private bool botsRunning;

    public MainForm()
    {
        InitializeComponent();
        game.Message += AddLog;
        StartRound();
    }

    private void StartRound()
    {
        selectedCards.Clear();
        lstLog.Items.Clear();
        game.StartRound();
        RefreshUi();
    }

    private void AddLog(string text)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AddLog(text));
            return;
        }

        lstLog.Items.Add(text);
        lstLog.TopIndex = Math.Max(0, lstLog.Items.Count - 1);
    }

    private void RefreshUi()
    {
        lblTeamScore.Text = $"Your team: {game.TeamScores[0]}";
        lblOpponentScore.Text = $"Opponents: {game.TeamScores[1]}";

        lblLeftCards.Text = PlayerStatus(game.Players[1]);
        lblPartnerCards.Text = PlayerStatus(game.Players[2]);
        lblRightCards.Text = PlayerStatus(game.Players[3]);

        lblWish.Text = game.MahJongWishRank.HasValue
            ? $"MAH JONG WISH: {TichuGame.RankName(game.MahJongWishRank.Value)}"
            : "";

        bool exchangePhase = !game.ExchangeCompleted;

        if (game.RoundOver)
        {
            lblTurn.Text = game.MatchOver ? "MATCH OVER" : "ROUND OVER";
            lblTableCombo.Text = game.MatchOver
                ? (game.TeamScores[0] > game.TeamScores[1]
                    ? "Your team wins!"
                    : "Opponents win!")
                : "Click New Round to continue.";
        }
        else if (exchangePhase)
        {
            lblTurn.Text = "EXCHANGE PHASE";
            lblTableCombo.Text = "Select 3 cards, then click EXCHANGE 3.";
        }
        else
        {
            var current = game.Players[game.CurrentPlayerIndex];
            lblTurn.Text = current.IsHuman
                ? "YOUR TURN"
                : $"{current.Name}'s turn";

            lblTableCombo.Text = game.TableCombination is null
                ? "Lead — play any legal combination."
                : $"{game.TableCombination.Type} • value {game.TableCombination.Value} • {game.TableCombination.CardCount} card(s)";
        }

        bool humanTurn = !game.RoundOver &&
                         game.ExchangeCompleted &&
                         game.CurrentPlayerIndex == 0;

        btnPlay.Enabled = humanTurn;
        btnPass.Enabled = humanTurn && game.TableCombination is not null;
        btnBomb.Enabled = !game.RoundOver &&
                          game.ExchangeCompleted &&
                          selectedCards.Count > 0;
        btnExchange.Enabled = exchangePhase && selectedCards.Count == 3;

        btnTichu.Enabled =
            !game.RoundOver &&
            !game.Players[0].HasPlayedAnyCard &&
            !game.Players[0].CalledTichu &&
            !game.Players[0].CalledGrandTichu;

        btnGrandTichu.Enabled =
            exchangePhase &&
            !game.Players[0].CalledGrandTichu &&
            !game.Players[0].CalledTichu;

        RenderTable();
        RenderHand();
    }

    private static string PlayerStatus(Player p)
    {
        if (p.FinishOrder > 0)
            return $"Finished #{p.FinishOrder}";

        string call = p.CalledGrandTichu
            ? " • GT"
            : p.CalledTichu
                ? " • T"
                : "";

        return $"{p.Hand.Count} cards{call}";
    }

    private void RenderTable()
    {
        flpTableCards.SuspendLayout();
        flpTableCards.Controls.Clear();

        foreach (var card in game.TableCards.TakeLast(12))
            flpTableCards.Controls.Add(CreateCardControl(card, false));

        flpTableCards.ResumeLayout();
    }

    private void RenderHand()
    {
        flpHand.SuspendLayout();
        flpHand.Controls.Clear();

        foreach (var card in game.Players[0].Hand)
        {
            var panel = CreateCardControl(card, true);

            if (selectedCards.Contains(card))
            {
                panel.Margin = new Padding(3, 2, 3, 18);
                panel.BackColor = Color.FromArgb(255, 240, 190);
            }

            flpHand.Controls.Add(panel);
        }

        flpHand.ResumeLayout();
    }

    private Control CreateCardControl(Card card, bool selectable)
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

        var lblRank = new Label
        {
            Width = 58,
            Height = 32,
            Left = 4,
            Top = 5,
            Text = card.RankText,
            Font = new Font(
                "Segoe UI",
                card.IsSpecial ? 9F : 14F,
                FontStyle.Bold),
            ForeColor = CardColor(card),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblSuit = new Label
        {
            Width = 58,
            Height = 58,
            Left = 4,
            Top = 42,
            Text = card.IsSpecial
                ? SpecialSymbol(card)
                : card.SuitSymbol,
            Font = new Font(
                "Segoe UI Symbol",
                card.IsSpecial ? 19F : 27F,
                FontStyle.Bold),
            ForeColor = CardColor(card),
            TextAlign = ContentAlignment.MiddleCenter
        };

        panel.Controls.Add(lblRank);
        panel.Controls.Add(lblSuit);

        if (selectable)
        {
            void Toggle(object? _, EventArgs __) => ToggleCard(card);
            panel.Click += Toggle;
            lblRank.Click += Toggle;
            lblSuit.Click += Toggle;
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
            : Color.FromArgb(25, 25, 25);
    }

    private static string SpecialSymbol(Card card) => card.Special switch
    {
        SpecialCard.Dragon => "龍",
        SpecialCard.Phoenix => "鳳",
        SpecialCard.Dog => "犬",
        SpecialCard.MahJong => "一",
        _ => ""
    };

    private void ToggleCard(Card card)
    {
        if (game.RoundOver)
            return;

        if (!selectedCards.Add(card))
            selectedCards.Remove(card);

        if (!game.ExchangeCompleted && selectedCards.Count > 3)
        {
            selectedCards.Remove(card);
            MessageBox.Show(
                "During the exchange, select exactly 3 cards.",
                "Exchange",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        RenderHand();
        UpdateHint();
        RefreshButtonsOnly();
    }

    private void UpdateHint()
    {
        if (!game.ExchangeCompleted)
        {
            lblHint.Text =
                $"{selectedCards.Count}/3 selected for exchange.";
            return;
        }

        if (selectedCards.Count == 0)
        {
            lblHint.Text =
                "Select cards. BOMB may also be used out of turn.";
            return;
        }

        double prev = game.TableCombination?.Type == ComboType.Single
            ? game.TableCombination.Value
            : 0;

        var combo = CombinationEvaluator.Evaluate(
            selectedCards.ToList(),
            prev);

        lblHint.Text = combo.Type == ComboType.Invalid
            ? $"{selectedCards.Count} selected • invalid combination"
            : $"{selectedCards.Count} selected • {combo.Type} • value {combo.Value}";
    }

    private void RefreshButtonsOnly()
    {
        btnExchange.Enabled =
            !game.ExchangeCompleted && selectedCards.Count == 3;

        btnBomb.Enabled =
            game.ExchangeCompleted &&
            !game.RoundOver &&
            selectedCards.Count > 0;
    }

    private int? AskMahJongWishIfNeeded(IEnumerable<Card> cards)
    {
        if (!cards.Any(c => c.Special == SpecialCard.MahJong))
            return null;

        using var dialog = new Form
        {
            Text = "Mah Jong Wish",
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(300, 150),
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

        for (int r = 2; r <= 14; r++)
            combo.Items.Add(new RankChoice(r));

        combo.SelectedIndex = 0;

        var ok = new Button
        {
            Text = "Wish",
            Left = 170,
            Top = 95,
            Width = 100,
            DialogResult = DialogResult.OK
        };

        dialog.Controls.Add(label);
        dialog.Controls.Add(combo);
        dialog.Controls.Add(ok);
        dialog.AcceptButton = ok;

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return 2;

        return ((RankChoice)combo.SelectedItem!).Rank;
    }

    private void btnPlay_Click(object? sender, EventArgs e)
    {
        if (game.CurrentPlayerIndex != 0 ||
            !game.ExchangeCompleted)
            return;

        var cards = selectedCards.ToList();
        int? wish = AskMahJongWishIfNeeded(cards);

        if (!game.TryPlayCards(
                0,
                cards,
                out string error,
                wish,
                false))
        {
            MessageBox.Show(
                error,
                "Invalid play",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        selectedCards.Clear();
        RefreshUi();
        BeginBotsIfNeeded();
    }

    private void btnPass_Click(object? sender, EventArgs e)
    {
        if (!game.TryPass(0, out string error))
        {
            MessageBox.Show(
                error,
                "Cannot pass",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        selectedCards.Clear();
        RefreshUi();
        BeginBotsIfNeeded();
    }

    private void btnBomb_Click(object? sender, EventArgs e)
    {
        if (!game.ExchangeCompleted || selectedCards.Count == 0)
            return;

        var cards = selectedCards.ToList();
        var combo = CombinationEvaluator.Evaluate(cards);

        if (!combo.IsBomb)
        {
            MessageBox.Show(
                "The selected cards are not a bomb.",
                "Bomb",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
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
            MessageBox.Show(
                error,
                "Bomb",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        selectedCards.Clear();
        RefreshUi();
        BeginBotsIfNeeded();
    }

    private void btnExchange_Click(object? sender, EventArgs e)
    {
        if (selectedCards.Count != 3)
            return;

        var selected = selectedCards.ToList();

        using var dialog = new ExchangeAssignmentForm(selected);

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        if (!game.CompleteHumanExchange(
                dialog.ToLeft!,
                dialog.ToPartner!,
                dialog.ToRight!,
                out string error))
        {
            MessageBox.Show(
                error,
                "Exchange",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        selectedCards.Clear();
        RefreshUi();
        BeginBotsIfNeeded();
    }

    private void btnTichu_Click(object? sender, EventArgs e)
    {
        if (!game.CallTichu(0, out string error))
        {
            MessageBox.Show(
                error,
                "Tichu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        RefreshUi();
    }

    private void btnGrandTichu_Click(object? sender, EventArgs e)
    {
        if (!game.CallGrandTichu(0, out string error))
        {
            MessageBox.Show(
                error,
                "Grand Tichu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        RefreshUi();
    }

    private void btnNewRound_Click(object? sender, EventArgs e)
    {
        if (game.MatchOver)
        {
            var answer = MessageBox.Show(
                "A team has reached 1000 points. Start a new match?",
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
                // Give every other bot a chance to interrupt with a bomb.
                bool interrupted = false;

                for (int i = 1; i < 4; i++)
                {
                    if (i == game.CurrentPlayerIndex ||
                        game.Players[i].IsOut)
                        continue;

                    var bomb = game.ChooseBotOutOfTurnBomb(i);

                    if (bomb is not null)
                    {
                        await Task.Delay(250);

                        if (game.TryPlayCards(
                                i,
                                bomb,
                                out _,
                                null,
                                true))
                        {
                            interrupted = true;
                            RefreshUi();
                            break;
                        }
                    }
                }

                if (interrupted)
                    continue;

                await Task.Delay(450);

                int bot = game.CurrentPlayerIndex;
                var chosen = game.ChooseBotPlay(bot);

                if (chosen is not null)
                {
                    int? wish = null;

                    if (chosen.Any(c =>
                            c.Special == SpecialCard.MahJong))
                    {
                        wish = ChooseBotWish(bot);
                    }

                    if (!game.TryPlayCards(
                            bot,
                            chosen,
                            out _,
                            wish,
                            false))
                    {
                        game.TryPass(bot, out _);
                    }
                }
                else
                {
                    game.TryPass(bot, out _);
                }

                RefreshUi();
            }
        }
        finally
        {
            botsRunning = false;
        }
    }

    private int ChooseBotWish(int botIndex)
    {
        var hand = game.Players[botIndex].Hand;

        // Wish for a rank the bot doesn't currently hold,
        // prioritising middle/high ranks.
        var held = hand
            .Where(c => !c.IsSpecial)
            .Select(c => c.Rank)
            .ToHashSet();

        for (int r = 14; r >= 6; r--)
        {
            if (!held.Contains(r))
                return r;
        }

        return 14;
    }

    private sealed class RankChoice
    {
        public int Rank { get; }

        public RankChoice(int rank)
        {
            Rank = rank;
        }

        public override string ToString() =>
            TichuGame.RankName(Rank);
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
        Text = "Assign Exchange Cards";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(390, 245);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        Controls.Add(new Label
        {
            Text = "Choose who receives each selected card:",
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

        var ok = new Button
        {
            Text = "Exchange",
            Left = 250,
            Top = 195,
            Width = 110
        };

        ok.Click += (_, _) =>
        {
            var left = (Card)cmbLeft.SelectedItem!;
            var partner = (Card)cmbPartner.SelectedItem!;
            var right = (Card)cmbRight.SelectedItem!;

            if (new[] { left, partner, right }.Distinct().Count() != 3)
            {
                MessageBox.Show(
                    "Each recipient must receive a different card.",
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

        Controls.Add(ok);
    }

    private void SetupRow(
        string text,
        ComboBox combo,
        int top,
        List<Card> cards)
    {
        Controls.Add(new Label
        {
            Text = text,
            Left = 20,
            Top = top + 4,
            Width = 90
        });

        combo.Left = 115;
        combo.Top = top;
        combo.Width = 245;
        combo.DropDownStyle = ComboBoxStyle.DropDownList;

        foreach (var c in cards)
            combo.Items.Add(c);

        Controls.Add(combo);
    }
}
