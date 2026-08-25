namespace TichuWinForms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private Panel pnlTop;
    private Label lblTitle;
    private Label lblTeamScore;
    private Label lblOpponentScore;
    private Button btnNewRound;
    private Button btnTichu;
    private Button btnGrandTichu;

    private Panel pnlTable;
    private Label lblTurn;
    private Label lblTableCombo;
    private Label lblWish;
    private FlowLayoutPanel flpTableCards;

    private GroupBox grpLeft;
    private Label lblLeftCards;
    private GroupBox grpPartner;
    private Label lblPartnerCards;
    private GroupBox grpRight;
    private Label lblRightCards;

    private FlowLayoutPanel flpHand;
    private Button btnPlay;
    private Button btnPass;
    private Button btnExchange;
    private Button btnBomb;
    private Label lblHint;
    private ListBox lstLog;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlTop = new Panel();
        lblTitle = new Label();
        lblTeamScore = new Label();
        lblOpponentScore = new Label();
        btnNewRound = new Button();
        btnTichu = new Button();
        btnGrandTichu = new Button();

        pnlTable = new Panel();
        lblTurn = new Label();
        lblTableCombo = new Label();
        lblWish = new Label();
        flpTableCards = new FlowLayoutPanel();

        grpLeft = new GroupBox();
        lblLeftCards = new Label();
        grpPartner = new GroupBox();
        lblPartnerCards = new Label();
        grpRight = new GroupBox();
        lblRightCards = new Label();

        flpHand = new FlowLayoutPanel();
        btnPlay = new Button();
        btnPass = new Button();
        btnExchange = new Button();
        btnBomb = new Button();
        lblHint = new Label();
        lstLog = new ListBox();

        SuspendLayout();

        pnlTop.BackColor = Color.FromArgb(20, 36, 31);
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Height = 82;
        pnlTop.Controls.Add(lblTitle);
        pnlTop.Controls.Add(lblTeamScore);
        pnlTop.Controls.Add(lblOpponentScore);
        pnlTop.Controls.Add(btnNewRound);
        pnlTop.Controls.Add(btnTichu);
        pnlTop.Controls.Add(btnGrandTichu);

        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
        lblTitle.ForeColor = Color.White;
        lblTitle.Location = new Point(22, 14);
        lblTitle.Text = "TICHU";

        lblTeamScore.AutoSize = true;
        lblTeamScore.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblTeamScore.ForeColor = Color.White;
        lblTeamScore.Location = new Point(190, 15);
        lblTeamScore.Text = "Your team: 0";

        lblOpponentScore.AutoSize = true;
        lblOpponentScore.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblOpponentScore.ForeColor = Color.White;
        lblOpponentScore.Location = new Point(190, 43);
        lblOpponentScore.Text = "Opponents: 0";

        btnGrandTichu.Location = new Point(665, 20);
        btnGrandTichu.Size = new Size(135, 40);
        btnGrandTichu.Text = "Grand Tichu";
        btnGrandTichu.Click += btnGrandTichu_Click;

        btnTichu.Location = new Point(810, 20);
        btnTichu.Size = new Size(110, 40);
        btnTichu.Text = "Call Tichu";
        btnTichu.Click += btnTichu_Click;

        btnNewRound.Location = new Point(930, 20);
        btnNewRound.Size = new Size(130, 40);
        btnNewRound.Text = "New Round";
        btnNewRound.Click += btnNewRound_Click;

        grpLeft.Text = "Bot Left";
        grpLeft.ForeColor = Color.White;
        grpLeft.Location = new Point(18, 112);
        grpLeft.Size = new Size(145, 92);
        grpLeft.Controls.Add(lblLeftCards);

        lblLeftCards.AutoSize = true;
        lblLeftCards.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblLeftCards.Location = new Point(18, 34);
        lblLeftCards.Text = "14 cards";

        grpPartner.Text = "Partner";
        grpPartner.ForeColor = Color.White;
        grpPartner.Location = new Point(458, 90);
        grpPartner.Size = new Size(145, 92);
        grpPartner.Controls.Add(lblPartnerCards);

        lblPartnerCards.AutoSize = true;
        lblPartnerCards.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblPartnerCards.Location = new Point(18, 34);
        lblPartnerCards.Text = "14 cards";

        grpRight.Text = "Bot Right";
        grpRight.ForeColor = Color.White;
        grpRight.Location = new Point(898, 112);
        grpRight.Size = new Size(145, 92);
        grpRight.Controls.Add(lblRightCards);

        lblRightCards.AutoSize = true;
        lblRightCards.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
        lblRightCards.Location = new Point(18, 34);
        lblRightCards.Text = "14 cards";

        pnlTable.BackColor = Color.FromArgb(28, 73, 55);
        pnlTable.BorderStyle = BorderStyle.FixedSingle;
        pnlTable.Location = new Point(185, 204);
        pnlTable.Size = new Size(690, 240);
        pnlTable.Controls.Add(lblTurn);
        pnlTable.Controls.Add(lblTableCombo);
        pnlTable.Controls.Add(lblWish);
        pnlTable.Controls.Add(flpTableCards);

        lblTurn.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTurn.ForeColor = Color.White;
        lblTurn.Location = new Point(10, 10);
        lblTurn.Size = new Size(668, 34);
        lblTurn.TextAlign = ContentAlignment.MiddleCenter;
        lblTurn.Text = "Turn";

        lblTableCombo.Font = new Font("Segoe UI", 10F);
        lblTableCombo.ForeColor = Color.Gainsboro;
        lblTableCombo.Location = new Point(10, 44);
        lblTableCombo.Size = new Size(668, 26);
        lblTableCombo.TextAlign = ContentAlignment.MiddleCenter;
        lblTableCombo.Text = "Table is empty";

        lblWish.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblWish.ForeColor = Color.Gold;
        lblWish.Location = new Point(10, 70);
        lblWish.Size = new Size(668, 24);
        lblWish.TextAlign = ContentAlignment.MiddleCenter;
        lblWish.Text = "";

        flpTableCards.Location = new Point(18, 100);
        flpTableCards.Size = new Size(650, 120);
        flpTableCards.WrapContents = false;
        flpTableCards.AutoScroll = true;

        flpHand.BackColor = Color.FromArgb(16, 26, 23);
        flpHand.Location = new Point(18, 470);
        flpHand.Size = new Size(1025, 160);
        flpHand.AutoScroll = true;
        flpHand.WrapContents = false;

        btnPlay.Location = new Point(18, 650);
        btnPlay.Size = new Size(130, 46);
        btnPlay.Text = "PLAY";
        btnPlay.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnPlay.Click += btnPlay_Click;

        btnPass.Location = new Point(158, 650);
        btnPass.Size = new Size(130, 46);
        btnPass.Text = "PASS";
        btnPass.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnPass.Click += btnPass_Click;

        btnBomb.Location = new Point(298, 650);
        btnBomb.Size = new Size(130, 46);
        btnBomb.Text = "BOMB";
        btnBomb.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnBomb.Click += btnBomb_Click;

        btnExchange.Location = new Point(438, 650);
        btnExchange.Size = new Size(160, 46);
        btnExchange.Text = "EXCHANGE 3";
        btnExchange.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnExchange.Click += btnExchange_Click;

        lblHint.ForeColor = Color.Gainsboro;
        lblHint.Font = new Font("Segoe UI", 9F);
        lblHint.Location = new Point(610, 647);
        lblHint.Size = new Size(435, 50);
        lblHint.Text = "Select cards from your hand.";

        lstLog.Location = new Point(1060, 90);
        lstLog.Size = new Size(320, 606);
        lstLog.BackColor = Color.FromArgb(25, 25, 28);
        lstLog.ForeColor = Color.Gainsboro;
        lstLog.BorderStyle = BorderStyle.FixedSingle;
        lstLog.Font = new Font("Consolas", 9F);

        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(13, 47, 35);
        ClientSize = new Size(1400, 720);
        Controls.Add(pnlTop);
        Controls.Add(grpLeft);
        Controls.Add(grpPartner);
        Controls.Add(grpRight);
        Controls.Add(pnlTable);
        Controls.Add(flpHand);
        Controls.Add(btnPlay);
        Controls.Add(btnPass);
        Controls.Add(btnBomb);
        Controls.Add(btnExchange);
        Controls.Add(lblHint);
        Controls.Add(lstLog);

        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Tichu - Enhanced Offline WinForms";

        ResumeLayout(false);
    }
}
