using System.Drawing;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views
{
    partial class ViewTgLogin
    {
        private System.ComponentModel.IContainer components = null;
        private Button btnLogin;
        private DataGridView dgvAccounts;
        private Panel pnlToolbar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // 控件声明
            this.pnlToolbar = new Panel();
            this.btnLogin = new Button();
            this.dgvAccounts = new DataGridView();

            // DataGridView 列声明 (简化后的名称)
            DataGridViewTextBoxColumn colTgId = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colNickname = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colStatus = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colProfitLoss = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colTurnover = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colSimProfitLoss = new DataGridViewTextBoxColumn();
            DataGridViewTextBoxColumn colSimTurnover = new DataGridViewTextBoxColumn();
            DataGridViewButtonColumn colBalanceQuery = new DataGridViewButtonColumn();
            DataGridViewButtonColumn colTodayProfitQuery = new DataGridViewButtonColumn();

            // 样式设置
            this.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.dgvAccounts).BeginInit();


            // ==================== UserControl 主体 (970x340) ====================
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(24, 34, 48); // 采用暗色主题背景
            this.Size = new Size(970, 340);
            this.Controls.Add(this.dgvAccounts);
            this.Controls.Add(this.pnlToolbar);

            // ==================== 顶部工具栏 (TG登录按钮) ====================
            this.pnlToolbar.Dock = DockStyle.Top;
            this.pnlToolbar.Size = new Size(970, 40);
            this.pnlToolbar.BackColor = Color.FromArgb(30, 40, 55);
            this.pnlToolbar.Controls.Add(this.btnLogin);

            // TG登录按钮
            this.btnLogin.Location = new Point(10, 5);
            this.btnLogin.Size = new Size(120, 30);
            this.btnLogin.Text = "TG 登录";
            this.btnLogin.BackColor = Color.FromArgb(0, 150, 255);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.Font = new Font("微软雅黑", 10F, FontStyle.Bold);


            // ==================== DataGridView 配置 ====================
            this.dgvAccounts.AllowUserToAddRows = false;
            this.dgvAccounts.AllowUserToDeleteRows = false;
            this.dgvAccounts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAccounts.Dock = DockStyle.Fill;
            this.dgvAccounts.BackgroundColor = this.BackColor;
            this.dgvAccounts.BorderStyle = BorderStyle.None;
            this.dgvAccounts.RowHeadersVisible = false;

            // 列定义
            this.dgvAccounts.Columns.AddRange(new DataGridViewColumn[] {
                colTgId,
                colNickname,
                colStatus,
                colProfitLoss,
                colTurnover,
                colSimProfitLoss,
                colSimTurnover,
                colBalanceQuery,
                colTodayProfitQuery
            });

            // 1. TGid
            colTgId.DataPropertyName = "TgId";
            colTgId.HeaderText = "TGid";
            colTgId.Width = 100;
            colTgId.ReadOnly = true;

            // 2. 昵称
            colNickname.DataPropertyName = "Nickname";
            colNickname.HeaderText = "昵称";
            colNickname.Width = 120;

            // 3. 状态
            colStatus.DataPropertyName = "Status";
            colStatus.HeaderText = "状态";
            colStatus.Width = 90;

            // 4. 盈亏 (右对齐)
            colProfitLoss.DataPropertyName = "ProfitLoss";
            colProfitLoss.HeaderText = "盈亏";
            colProfitLoss.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colProfitLoss.Width = 100;

            // 5. 流水 (右对齐)
            colTurnover.DataPropertyName = "Turnover";
            colTurnover.HeaderText = "流水";
            colTurnover.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colTurnover.Width = 100;

            // 6. 模拟盈亏 (右对齐)
            colSimProfitLoss.DataPropertyName = "SimulatedProfitLoss";
            colSimProfitLoss.HeaderText = "模拟盈亏";
            colSimProfitLoss.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colSimProfitLoss.Width = 100;

            // 7. 模拟流水 (右对齐)
            colSimTurnover.DataPropertyName = "SimulatedTurnover";
            colSimTurnover.HeaderText = "模拟流水";
            colSimTurnover.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colSimTurnover.Width = 100;

            // 8. 余额查询按钮
            colBalanceQuery.HeaderText = "余额查询";
            colBalanceQuery.Text = "查询余额";
            colBalanceQuery.UseColumnTextForButtonValue = true;
            colBalanceQuery.Width = 80;

            // 9. 今日盈亏查询按钮
            colTodayProfitQuery.HeaderText = "今日盈亏查询";
            colTodayProfitQuery.Text = "查询今日";
            colTodayProfitQuery.UseColumnTextForButtonValue = true;
            colTodayProfitQuery.Width = 90;

            // 结束布局
            ((System.ComponentModel.ISupportInitialize)this.dgvAccounts).EndInit();
            this.pnlToolbar.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}