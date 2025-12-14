namespace QuantumIdleDesktop
{
    partial class FrmMain
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            panelTopNav = new Panel();
            btnTgLogin = new Button();
            btnScheme = new Button();
            btnSettings = new Button();
            btnOrderLog = new Button();
            btnOdds = new Button();
            btnHistory = new Button();
            chkSimulation = new CheckBox();
            btnStartAll = new Button();
            panelMainContent = new Panel();
            panelLogWrapper = new Panel();
            richTextBoxLog = new RichTextBox();
            panelBottomBar = new Panel();
            splitContainer1 = new SplitContainer();
            statusLabelBalance = new Label();
            statusLabelProfit = new Label();
            statusLabelTurnover = new Label();
            statusLabelSimProfit = new Label();
            statusLabelSimTurnover = new Label();
            statusLabelExpireTime = new Label();
            timerState = new System.Windows.Forms.Timer(components);
            panelTopNav.SuspendLayout();
            panelLogWrapper.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // panelTopNav
            // 
            panelTopNav.Controls.Add(btnTgLogin);
            panelTopNav.Controls.Add(btnScheme);
            panelTopNav.Controls.Add(btnSettings);
            panelTopNav.Controls.Add(btnOrderLog);
            panelTopNav.Controls.Add(btnOdds);
            panelTopNav.Controls.Add(btnHistory);
            panelTopNav.Controls.Add(chkSimulation);
            panelTopNav.Controls.Add(btnStartAll);
            panelTopNav.Dock = DockStyle.Top;
            panelTopNav.Location = new Point(0, 0);
            panelTopNav.Name = "panelTopNav";
            panelTopNav.Size = new Size(1000, 56);
            panelTopNav.TabIndex = 2;
            // 
            // btnTgLogin
            // 
            btnTgLogin.Location = new Point(0, 0);
            btnTgLogin.Name = "btnTgLogin";
            btnTgLogin.Size = new Size(75, 23);
            btnTgLogin.TabIndex = 0;
            // 
            // btnScheme
            // 
            btnScheme.Location = new Point(0, 0);
            btnScheme.Name = "btnScheme";
            btnScheme.Size = new Size(75, 23);
            btnScheme.TabIndex = 1;
            // 
            // btnSettings
            // 
            btnSettings.Location = new Point(0, 0);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(75, 23);
            btnSettings.TabIndex = 2;
            // 
            // btnOrderLog
            // 
            btnOrderLog.Location = new Point(0, 0);
            btnOrderLog.Name = "btnOrderLog";
            btnOrderLog.Size = new Size(75, 23);
            btnOrderLog.TabIndex = 3;
            // 
            // btnOdds
            // 
            btnOdds.Location = new Point(0, 0);
            btnOdds.Name = "btnOdds";
            btnOdds.Size = new Size(75, 23);
            btnOdds.TabIndex = 4;
            // 
            // btnOdds
            // 
            btnHistory.Location = new Point(0, 0);
            btnHistory.Name = "btnHistory";
            btnHistory.Size = new Size(75, 23);
            btnHistory.TabIndex = 5;
            // 
            // chkSimulation
            // 
            chkSimulation.Location = new Point(0, 0);
            chkSimulation.Name = "chkSimulation";
            chkSimulation.Size = new Size(104, 24);
            chkSimulation.TabIndex = 5;
            // 
            // btnStartAll
            // 
            btnStartAll.Location = new Point(0, 0);
            btnStartAll.Name = "btnStartAll";
            btnStartAll.Size = new Size(75, 23);
            btnStartAll.TabIndex = 6;
            // 
            // panelMainContent
            // 
            panelMainContent.Dock = DockStyle.Fill;
            panelMainContent.Location = new Point(0, 0);
            panelMainContent.Name = "panelMainContent";
            panelMainContent.Size = new Size(1000, 355);
            panelMainContent.TabIndex = 0;
            // 
            // panelLogWrapper
            // 
            panelLogWrapper.Controls.Add(richTextBoxLog);
            panelLogWrapper.Dock = DockStyle.Fill;
            panelLogWrapper.Location = new Point(0, 0);
            panelLogWrapper.Name = "panelLogWrapper";
            panelLogWrapper.Size = new Size(1000, 141);
            panelLogWrapper.TabIndex = 0;
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Dock = DockStyle.Fill;
            richTextBoxLog.Location = new Point(0, 0);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.Size = new Size(1000, 141);
            richTextBoxLog.TabIndex = 0;
            richTextBoxLog.Text = "";
            // 
            // panelBottomBar
            // 
            panelBottomBar.Dock = DockStyle.Bottom;
            panelBottomBar.Location = new Point(0, 556);
            panelBottomBar.Name = "panelBottomBar";
            panelBottomBar.Size = new Size(1000, 44);
            panelBottomBar.TabIndex = 1;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 56);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(panelMainContent);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(panelLogWrapper);
            splitContainer1.Size = new Size(1000, 500);
            splitContainer1.SplitterDistance = 355;
            splitContainer1.TabIndex = 0;
            // 
            // statusLabelBalance
            // 
            statusLabelBalance.Location = new Point(0, 0);
            statusLabelBalance.Name = "statusLabelBalance";
            statusLabelBalance.Size = new Size(100, 23);
            statusLabelBalance.TabIndex = 0;
            // 
            // statusLabelProfit
            // 
            statusLabelProfit.Location = new Point(0, 0);
            statusLabelProfit.Name = "statusLabelProfit";
            statusLabelProfit.Size = new Size(100, 23);
            statusLabelProfit.TabIndex = 0;
            // 
            // statusLabelTurnover
            // 
            statusLabelTurnover.Location = new Point(0, 0);
            statusLabelTurnover.Name = "statusLabelTurnover";
            statusLabelTurnover.Size = new Size(100, 23);
            statusLabelTurnover.TabIndex = 0;
            // 
            // statusLabelSimProfit
            // 
            statusLabelSimProfit.Location = new Point(0, 0);
            statusLabelSimProfit.Name = "statusLabelSimProfit";
            statusLabelSimProfit.Size = new Size(100, 23);
            statusLabelSimProfit.TabIndex = 0;
            // 
            // statusLabelSimTurnover
            // 
            statusLabelSimTurnover.Location = new Point(0, 0);
            statusLabelSimTurnover.Name = "statusLabelSimTurnover";
            statusLabelSimTurnover.Size = new Size(100, 23);
            statusLabelSimTurnover.TabIndex = 0;
            // 
            // statusLabelExpireTime
            // 
            statusLabelExpireTime.Location = new Point(0, 0);
            statusLabelExpireTime.Name = "statusLabelExpireTime";
            statusLabelExpireTime.Size = new Size(100, 23);
            statusLabelExpireTime.TabIndex = 0;
            // 
            // FrmMain
            // 
            ClientSize = new Size(1000, 600);
            Controls.Add(splitContainer1);
            Controls.Add(panelBottomBar);
            Controls.Add(panelTopNav);
            FormBorderStyle = FormBorderStyle.Fixed3D;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "FrmMain";
            StartPosition = FormStartPosition.CenterScreen;
            base.Text = "QuantumIdle 智能控制台";
            panelTopNav.ResumeLayout(false);
            panelLogWrapper.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Panel panelTopNav;
        private Panel panelMainContent;
        private Panel panelLogWrapper;
        private Panel panelBottomBar;
        private SplitContainer splitContainer1;

        private Button btnTgLogin;
        private Button btnScheme;
        private Button btnSettings;
        private Button btnOrderLog;
        private Button btnOdds;
        private Button btnHistory;

        private CheckBox chkSimulation;
        private Button btnStartAll;

        private RichTextBox richTextBoxLog;

        public Label statusLabelBalance;
        public Label statusLabelProfit;
        public Label statusLabelTurnover;
        public Label statusLabelSimProfit;
        public Label statusLabelSimTurnover;
        public Label statusLabelExpireTime;

        private System.Windows.Forms.Timer timerState;
    }
}