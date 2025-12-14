namespace QuantumIdleDesktop.Views
{
    partial class ViewOddsSetting
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle headerStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle rowStyle = new System.Windows.Forms.DataGridViewCellStyle();

            this.pnlLeft = new System.Windows.Forms.Panel();
            this.lbGames = new System.Windows.Forms.ListBox();
            this.pnlFill = new System.Windows.Forms.Panel();
            this.dgvOdds = new System.Windows.Forms.DataGridView();
            this.colMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();

            this.pnlLeft.SuspendLayout();
            this.pnlFill.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOdds)).BeginInit();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();

            // --- 左侧游戏列表 ---
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeft.Width = 200;
            this.pnlLeft.BackColor = System.Drawing.Color.FromArgb(15, 22, 32);
            this.pnlLeft.Controls.Add(this.lbGames);

            this.lbGames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbGames.BackColor = System.Drawing.Color.FromArgb(15, 22, 32);
            this.lbGames.ForeColor = System.Drawing.Color.Cyan;
            this.lbGames.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbGames.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lbGames.ItemHeight = 30;

            // --- 底部保存按钮 ---
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Height = 60;
            this.pnlBottom.BackColor = System.Drawing.Color.FromArgb(10, 15, 25);
            this.pnlBottom.Controls.Add(this.btnSave);

            this.btnSave.Text = "💾 保存配置";
            this.btnSave.Size = new System.Drawing.Size(120, 40);
            this.btnSave.Location = new System.Drawing.Point(20, 10);
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.ForeColor = System.Drawing.Color.LimeGreen;
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(20, 40, 25);
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;

            // --- 右侧表格 ---
            this.pnlFill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFill.Controls.Add(this.dgvOdds);

            this.dgvOdds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvOdds.BackgroundColor = System.Drawing.Color.FromArgb(20, 25, 35);
            this.dgvOdds.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvOdds.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvOdds.AllowUserToAddRows = false;
            this.dgvOdds.AllowUserToDeleteRows = false;

            // 样式
            headerStyle.BackColor = System.Drawing.Color.FromArgb(30, 40, 50);
            headerStyle.ForeColor = System.Drawing.Color.White;
            headerStyle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgvOdds.ColumnHeadersDefaultCellStyle = headerStyle;
            this.dgvOdds.EnableHeadersVisualStyles = false;
            this.dgvOdds.ColumnHeadersHeight = 35;

            rowStyle.BackColor = System.Drawing.Color.FromArgb(20, 25, 35);
            rowStyle.ForeColor = System.Drawing.Color.Cyan;
            rowStyle.SelectionBackColor = System.Drawing.Color.FromArgb(40, 50, 70);
            this.dgvOdds.DefaultCellStyle = rowStyle;

            // 列
            this.colMode.HeaderText = "玩法模式";
            this.colMode.Name = "colMode";
            this.colMode.ReadOnly = true; // 模式名不可改

            this.colValue.HeaderText = "赔率 (可编辑)";
            this.colValue.Name = "colValue";

            this.dgvOdds.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.colMode, this.colValue });

            // --- 主控件 ---
            this.Controls.Add(this.pnlFill);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pnlBottom);
            this.Size = new System.Drawing.Size(800, 500);
            this.BackColor = System.Drawing.Color.FromArgb(10, 15, 25);

            this.pnlLeft.ResumeLayout(false);
            this.pnlFill.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOdds)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.ListBox lbGames;
        private System.Windows.Forms.Panel pnlFill;
        private System.Windows.Forms.DataGridView dgvOdds;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
    }
}