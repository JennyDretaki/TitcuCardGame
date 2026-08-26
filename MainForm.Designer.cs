using TichuWinForms_Smooth.Controls;

namespace TichuWinForms_Smooth;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlHeader;
    private Label lblTitle;
    private Label lblYourScore;
    private Label lblOpponentScore;
    private Button btnTichu;
    private Button btnGrandTichu;
    private Button btnNewRound;

    private Label lblPartner;
    private Label lblLeft;
    private Label lblRight;

    private BufferedPanel pnlTable;
    private Label lblTurn;
    private Label lblPlayOwner;
    private Label lblCombination;
    private Label lblWish;
    private BufferedFlowLayoutPanel flpCurrentPlay;

    private BufferedFlowLayoutPanel flpHand;
    private Label lblSelection;

    private Button btnPlay;
    private Button btnPass;
    private Button btnBomb;
    private Button btnExchange;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlHeader = new Panel();
        lblTitle = new Label();
        lblYourScore = new Label();
        lblOpponentScore = new Label();
        btnTichu = new Button();
        btnGrandTichu = new Button();
        btnNewRound = new Button();

        lblPartner = new Label();
        lblLeft = new Label();
        lblRight = new Label();

        pnlTable = new BufferedPanel();
        lblTurn = new Label();
        lblPlayOwner = new Label();
        lblCombination = new Label();
        lblWish = new Label();
        flpCurrentPlay = new BufferedFlowLayoutPanel();

        flpHand = new BufferedFlowLayoutPanel();
        lblSelection = new Label();

        btnPlay = new Button();
        btnPass = new Button();
        btnBomb = new Button();
        btnExchange = new Button();

        SuspendLayout();

        // Header
        pnlHeader.BackColor = Color.FromArgb(18, 34, 29);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 78;

        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 23F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(24, 14);
        lblTitle.Text = "TICHU";

        lblYourScore.AutoSize = true;
        lblYourScore.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblYourScore.ForeColor = Color.FromArgb(190, 240, 210);
        lblYourScore.Location = new Point(190, 15);
        lblYourScore.Text = "Your team  0";

        lblOpponentScore.AutoSize = true;
        lblOpponentScore.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblOpponentScore.ForeColor = Color.FromArgb(245, 200, 200);
        lblOpponentScore.Location = new Point(190, 43);
        lblOpponentScore.Text = "Opponents  0";

        SetupHeaderButton(btnGrandTichu, "GRAND TICHU", 650, 144);
        SetupHeaderButton(btnTichu, "TICHU", 804, 105);
        SetupHeaderButton(btnNewRound, "NEW ROUND", 919, 130);

        btnGrandTichu.Click += btnGrandTichu_Click;
        btnTichu.Click += btnTichu_Click;
        btnNewRound.Click += btnNewRound_Click;

        pnlHeader.Controls.AddRange(new Control[]
        {
            lblTitle, lblYourScore, lblOpponentScore,
            btnGrandTichu, btnTichu, btnNewRound
        });

        // Player labels
        SetupPlayerLabel(lblPartner, "Partner", 430, 97, 230);
        SetupPlayerLabel(lblLeft, "Bot Left", 25, 235, 190);
        SetupPlayerLabel(lblRight, "Bot Right", 835, 235, 190);

        // Table
        pnlTable.BackColor = Color.FromArgb(27, 82, 59);
        pnlTable.Location = new Point(225, 150);
        pnlTable.Size = new Size(600, 320);
        pnlTable.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        lblTurn.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        lblTurn.ForeColor = Color.White;
        lblTurn.Location = new Point(20, 17);
        lblTurn.Size = new Size(558, 32);
        lblTurn.TextAlign = ContentAlignment.MiddleCenter;
        lblTurn.Text = "YOUR TURN";

        lblPlayOwner.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblPlayOwner.ForeColor = Color.FromArgb(205, 235, 220);
        lblPlayOwner.Location = new Point(20, 60);
        lblPlayOwner.Size = new Size(558, 24);
        lblPlayOwner.TextAlign = ContentAlignment.MiddleCenter;

        lblCombination.Font = new Font("Segoe UI", 9.5F);
        lblCombination.ForeColor = Color.Gainsboro;
        lblCombination.Location = new Point(20, 86);
        lblCombination.Size = new Size(558, 23);
        lblCombination.TextAlign = ContentAlignment.MiddleCenter;

        lblWish.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblWish.ForeColor = Color.Gold;
        lblWish.Location = new Point(20, 111);
        lblWish.Size = new Size(558, 24);
        lblWish.TextAlign = ContentAlignment.MiddleCenter;

        flpCurrentPlay.BackColor = Color.Transparent;
        flpCurrentPlay.Location = new Point(18, 145);
        flpCurrentPlay.Size = new Size(562, 150);
        flpCurrentPlay.WrapContents = false;
        flpCurrentPlay.AutoScroll = false;
        flpCurrentPlay.FlowDirection = FlowDirection.LeftToRight;
        flpCurrentPlay.Padding = new Padding(0, 8, 0, 0);

        pnlTable.Controls.AddRange(new Control[]
        {
            lblTurn, lblPlayOwner, lblCombination, lblWish, flpCurrentPlay
        });

        // Hand
        flpHand.BackColor = Color.FromArgb(14, 28, 23);
        flpHand.Location = new Point(25, 500);
        flpHand.Size = new Size(1000, 165);
        flpHand.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        flpHand.AutoScroll = true;
        flpHand.WrapContents = false;
        flpHand.FlowDirection = FlowDirection.LeftToRight;
        flpHand.Padding = new Padding(8, 0, 8, 0);

        lblSelection.ForeColor = Color.FromArgb(215, 225, 220);
        lblSelection.Font = new Font("Segoe UI", 9.5F);
        lblSelection.Location = new Point(25, 674);
        lblSelection.Size = new Size(490, 30);
        lblSelection.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
        lblSelection.Text = "Select cards from your hand.";

        SetupActionButton(btnPlay, "PLAY", 540, 668, 115);
        SetupActionButton(btnPass, "PASS", 665, 668, 115);
        SetupActionButton(btnBomb, "BOMB", 790, 668, 115);
        SetupActionButton(btnExchange, "EXCHANGE 3", 915, 668, 110);

        btnPlay.Click += btnPlay_Click;
        btnPass.Click += btnPass_Click;
        btnBomb.Click += btnBomb_Click;
        btnExchange.Click += btnExchange_Click;

        // Form
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(10, 44, 31);
        ClientSize = new Size(1050, 720);
        MinimumSize = new Size(940, 690);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Tichu";

        Controls.AddRange(new Control[]
        {
            pnlHeader,
            lblPartner, lblLeft, lblRight,
            pnlTable,
            flpHand,
            lblSelection,
            btnPlay, btnPass, btnBomb, btnExchange
        });

        ResumeLayout(false);
    }

    private static void SetupHeaderButton(
        Button button,
        string text,
        int x,
        int width)
    {
        button.Text = text;
        button.Location = new Point(x, 19);
        button.Size = new Size(width, 40);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = Color.FromArgb(85, 120, 105);
        button.BackColor = Color.FromArgb(31, 59, 49);
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
    }

    private static void SetupPlayerLabel(
        Label label,
        string text,
        int x,
        int y,
        int width)
    {
        label.Text = text;
        label.Location = new Point(x, y);
        label.Size = new Size(width, 48);
        label.TextAlign = ContentAlignment.MiddleCenter;
        label.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        label.ForeColor = Color.White;
        label.BackColor = Color.FromArgb(18, 59, 43);
        label.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void SetupActionButton(
        Button button,
        string text,
        int x,
        int y,
        int width)
    {
        button.Text = text;
        button.Location = new Point(x, y);
        button.Size = new Size(width, 40);
        button.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Color.FromArgb(225, 239, 232);
        button.ForeColor = Color.FromArgb(18, 48, 36);
        button.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
    }
}
