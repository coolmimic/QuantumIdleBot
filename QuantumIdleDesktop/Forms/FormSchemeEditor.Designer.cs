using System.Drawing;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Forms
{
    partial class FormSchemeEditor
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ==================== 控件声明 ====================
            this.grpRule = new GroupBox();   // 左侧上：出号规则
            this.cboRuleType = new ComboBox();
            this.pnlRuleContainer = new Panel();

            this.grpOdds = new GroupBox();   // 左侧下：资金策略
            this.cboOddsType = new ComboBox();
            this.pnlOddsContainer = new Panel();

            this.grpRight = new GroupBox();   // 右侧：所有设置 + 按钮
            this.txtName = new TextBox();

            // 【新增】搜索框
            this.txtGroupSearch = new TextBox();
            this.cboGroup = new ComboBox();
            this.btnRefreshGroup = new Button();

            this.cboGameType = new ComboBox();
            this.cboPlayMode = new ComboBox();
            this.chkRisk = new CheckBox();
            this.numStopProfit = new NumericUpDown();
            this.numStopLoss = new NumericUpDown();
            this.btnSave = new Button();
            this.btnCancel = new Button();

            ((System.ComponentModel.ISupportInitialize)numStopProfit).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numStopLoss).BeginInit();
            this.SuspendLayout();

            // ==================== 窗体 850×700 ====================
            this.ClientSize = new Size(850, 700);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "方案编辑器";
            this.BackColor = Color.FromArgb(15, 22, 34);
            this.Font = new Font("微软雅黑", 10F);

            // ==================== 左侧上：出号规则（宽度 450） ====================
            grpRule.Location = new Point(15, 15);
            grpRule.Size = new Size(450, 330);
            grpRule.Text = "出号规则（投注内容）";
            grpRule.ForeColor = Color.Cyan;

            var lblRule = new Label { Location = new Point(15, 30), Text = "规则类型：", ForeColor = Color.Silver, AutoSize = true };
            cboRuleType.Location = new Point(100, 26);
            cboRuleType.Size = new Size(330, 30);
            cboRuleType.DropDownStyle = ComboBoxStyle.DropDownList;

            pnlRuleContainer.Location = new Point(15, 70);
            pnlRuleContainer.Size = new Size(420, 245);
            pnlRuleContainer.BackColor = Color.FromArgb(20, 30, 50);
            pnlRuleContainer.BorderStyle = BorderStyle.FixedSingle;
            pnlRuleContainer.AutoScroll = true;

            grpRule.Controls.AddRange(new Control[] { lblRule, cboRuleType, pnlRuleContainer });

            // ==================== 左侧下：资金策略（宽度 450） ====================
            grpOdds.Location = new Point(15, 355);
            grpOdds.Size = new Size(450, 330);
            grpOdds.Text = "资金策略（倍率/追号）";
            grpOdds.ForeColor = Color.Cyan;

            var lblOdds = new Label { Location = new Point(15, 30), Text = "策略类型：", ForeColor = Color.Silver, AutoSize = true };
            cboOddsType.Location = new Point(100, 26);
            cboOddsType.Size = new Size(330, 30);
            cboOddsType.DropDownStyle = ComboBoxStyle.DropDownList;

            pnlOddsContainer.Location = new Point(15, 70);
            pnlOddsContainer.Size = new Size(420, 245);
            pnlOddsContainer.BackColor = Color.FromArgb(20, 30, 50);
            pnlOddsContainer.BorderStyle = BorderStyle.FixedSingle;
            pnlOddsContainer.AutoScroll = true;

            grpOdds.Controls.AddRange(new Control[] { lblOdds, cboOddsType, pnlOddsContainer });

            // ==================== 右侧：所有设置 + 按钮（宽度 355） ====================
            grpRight.Location = new Point(480, 15);
            grpRight.Size = new Size(355, 670);
            grpRight.Text = "方案设置";
            grpRight.ForeColor = Color.Cyan;

            int top = 30;

            // 1. 方案名称
            var l1 = new Label { Location = new Point(25, top), Text = "方案名称：", ForeColor = Color.Silver, AutoSize = true };
            txtName.Location = new Point(110, top - 4);
            txtName.Size = new Size(220, 30);
            top += 50;

            // 2. 群组搜索与刷新 (新增)
            // 搜索框
            txtGroupSearch.Location = new Point(25, top - 4);
            txtGroupSearch.Size = new Size(220, 30);

            // 刷新按钮 (放在搜索框右侧)
            btnRefreshGroup.Location = new Point(255, top - 5);
            btnRefreshGroup.Size = new Size(75, 32);
            btnRefreshGroup.Text = "刷新";
            btnRefreshGroup.Font = new Font("微软雅黑", 9F);

            top += 35; // 下移给下拉框

            // 3. 群组列表 (独占一行)
            cboGroup.Location = new Point(25, top - 4);
            cboGroup.Size = new Size(305, 30);
            cboGroup.DropDownStyle = ComboBoxStyle.DropDownList;

            top += 50;

            // 4. 游戏类型
            var l3 = new Label { Location = new Point(25, top), Text = "游戏类型：", ForeColor = Color.Silver, AutoSize = true };
            cboGameType.Location = new Point(110, top - 4);
            cboGameType.Size = new Size(220, 30);
            cboGameType.DropDownStyle = ComboBoxStyle.DropDownList;
            top += 50;

            // 5. 玩法模式
            var l4 = new Label { Location = new Point(25, top), Text = "玩法模式：", ForeColor = Color.Silver, AutoSize = true };
            cboPlayMode.Location = new Point(110, top - 4);
            cboPlayMode.Size = new Size(220, 30);
            cboPlayMode.DropDownStyle = ComboBoxStyle.DropDownList;
            top += 60;

            // 6. 止盈止损开关
            chkRisk.Location = new Point(25, top);
            chkRisk.Text = "启用止盈止损";
            chkRisk.Checked = true;
            chkRisk.ForeColor = Color.Silver;
            top += 50;

            // 7. 止盈金额
            var l5 = new Label { Location = new Point(45, top), Text = "止盈金额：", ForeColor = Color.Silver, AutoSize = true };
            numStopProfit.Location = new Point(130, top - 4);
            numStopProfit.Size = new Size(130, 30);
            numStopProfit.DecimalPlaces = 2;
            numStopProfit.Maximum = 999999;
            var u1 = new Label { Location = new Point(270, top), Text = "TON", ForeColor = Color.Gray, AutoSize = true };
            top += 50;

            // 8. 止损金额
            var l6 = new Label { Location = new Point(45, top), Text = "止损金额：", ForeColor = Color.Silver, AutoSize = true };
            numStopLoss.Location = new Point(130, top - 4);
            numStopLoss.Size = new Size(130, 30);
            numStopLoss.DecimalPlaces = 2;
            numStopLoss.Maximum = 999999;
            var u2 = new Label { Location = new Point(270, top), Text = "TON", ForeColor = Color.Gray, AutoSize = true };

            // 保存 & 取消按钮（固定在右侧底部）
            btnSave.Location = new Point(45, 670 - 90);
            btnSave.Size = new Size(130, 48);
            btnSave.Text = "保存方案";
            btnSave.BackColor = Color.FromArgb(0, 185, 95);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("微软雅黑", 11F, FontStyle.Bold);
            btnSave.Click += btnSave_Click;

            btnCancel.Location = new Point(195, 670 - 90);
            btnCancel.Size = new Size(130, 48);
            btnCancel.Text = "取消";
            btnCancel.BackColor = Color.FromArgb(220, 60, 80);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Font = new Font("微软雅黑", 11F, FontStyle.Bold);
            btnCancel.Click += btnCancel_Click;

            // 添加控件到 GroupBox
            grpRight.Controls.AddRange(new Control[] {
                l1, txtName,
                txtGroupSearch, btnRefreshGroup, cboGroup, // 调整顺序
                l3, cboGameType,
                l4, cboPlayMode,
                chkRisk, l5, numStopProfit, u1, l6, numStopLoss, u2,
                btnSave, btnCancel
            });

            // ==================== 添加到窗体 ====================
            this.Controls.Add(grpRule);
            this.Controls.Add(grpOdds);
            this.Controls.Add(grpRight);

            ((System.ComponentModel.ISupportInitialize)numStopProfit).EndInit();
            ((System.ComponentModel.ISupportInitialize)numStopLoss).EndInit();
            this.ResumeLayout(false);
        }

        // ==================== 控件字段 ====================
        private GroupBox grpRule;
        private ComboBox cboRuleType;
        private Panel pnlRuleContainer;

        private GroupBox grpOdds;
        private ComboBox cboOddsType;
        private Panel pnlOddsContainer;

        private GroupBox grpRight;
        private TextBox txtName;

        // 【新增】搜索框字段
        private TextBox txtGroupSearch;
        private ComboBox cboGroup;
        private Button btnRefreshGroup;

        private ComboBox cboGameType;
        private ComboBox cboPlayMode;
        private CheckBox chkRisk;
        private NumericUpDown numStopProfit;
        private NumericUpDown numStopLoss;
        private Button btnSave;
        private Button btnCancel;
    }
}