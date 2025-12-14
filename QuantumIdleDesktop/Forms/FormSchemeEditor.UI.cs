using QuantumIdleDesktop.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Forms
{
    // 使用 partial 关键字，表示这是 FormSchemeEditor 的一部分
    public partial class FormSchemeEditor
    {
        // ==================== 控件声明 ====================

        // --- 左侧：出号规则 ---
        private GroupBox grpRule;
        private ComboBox cboRuleType;
        private Panel pnlRuleContainer;

        // --- 左侧：资金策略 ---
        private GroupBox grpOdds;
        private ComboBox cboOddsType;
        private Panel pnlOddsContainer;

        // --- 右侧：设置 ---
        private GroupBox grpRight;

        // 基础设置
        private TextBox txtName;
        private TextBox txtGroupSearch;
        private Button btnRefreshGroup;
        private ComboBox cboGroup;
        private ComboBox cboGameType;
        private ComboBox cboPlayMode;

        // 【新增】位置选择面板 (HashLottery 专用)
        private Panel pnlPositions;
        private List<CheckBox> _posCheckBoxes; // 存储5个复选框以便逻辑调用

        // 止盈止损
        private CheckBox chkRisk;
        private NumericUpDown numStopProfit;
        private NumericUpDown numStopLoss;

        // 底部按钮
        private Button btnSave;
        private Button btnCancel;

        /// <summary>
        /// 初始化所有 UI 组件 (替代 Designer 的 InitializeComponent)
        /// </summary>
        private void InitUI()
        {
            // 1. 窗体基础属性
            this.ClientSize = new Size(880, 720);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "方案编辑器";
            this.BackColor = Color.FromArgb(15, 22, 34);
            this.Font = new Font("微软雅黑", 10F);

            // 2. 初始化三大区域
            InitLeftRuleArea();
            InitLeftOddsArea();
            InitRightSettingsArea();

            // 3. 将区域加入窗体
            this.Controls.Add(grpRule);
            this.Controls.Add(grpOdds);
            this.Controls.Add(grpRight);
        }

        private void InitLeftRuleArea()
        {
            grpRule = CreateGroupBox("出号规则（投注内容）", 15, 15, 450, 330);

            var lbl = CreateLabel("规则类型：", 15, 30);
            cboRuleType = CreateComboBox(100, 26, 330);

            pnlRuleContainer = new Panel
            {
                Location = new Point(15, 70),
                Size = new Size(420, 245),
                BackColor = Color.FromArgb(20, 30, 50),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };

            grpRule.Controls.Add(lbl);
            grpRule.Controls.Add(cboRuleType);
            grpRule.Controls.Add(pnlRuleContainer);
        }

        private void InitLeftOddsArea()
        {
            grpOdds = CreateGroupBox("资金策略（倍率/追号）", 15, 355, 450, 330);

            var lbl = CreateLabel("策略类型：", 15, 30);
            cboOddsType = CreateComboBox(100, 26, 330);

            pnlOddsContainer = new Panel
            {
                Location = new Point(15, 70),
                Size = new Size(420, 245),
                BackColor = Color.FromArgb(20, 30, 50),
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };

            grpOdds.Controls.Add(lbl);
            grpOdds.Controls.Add(cboOddsType);
            grpOdds.Controls.Add(pnlOddsContainer);
        }

        private void InitRightSettingsArea()
        {
            grpRight = CreateGroupBox("方案设置", 480, 15, 380, 670);

            int top = 30; // 用于动态计算 Y 轴坐标

            // 1. 方案名称
            grpRight.Controls.Add(CreateLabel("方案名称：", 25, top));
            txtName = CreateTextBox(110, top - 4, 240);
            grpRight.Controls.Add(txtName);
            top += 50;

            // 2. 群组搜索
            txtGroupSearch = CreateTextBox(25, top - 4, 240);
            btnRefreshGroup = new Button
            {
                Text = "刷新",
                Location = new Point(275, top - 5),
                Size = new Size(75, 32),
                BackColor = Color.FromArgb(45, 55, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            grpRight.Controls.Add(txtGroupSearch);
            grpRight.Controls.Add(btnRefreshGroup);
            top += 35;

            // 3. 群组下拉
            cboGroup = CreateComboBox(25, top - 4, 325);
            grpRight.Controls.Add(cboGroup);
            top += 50;

            // 4. 游戏类型
            grpRight.Controls.Add(CreateLabel("游戏类型：", 25, top));
            cboGameType = CreateComboBox(110, top - 4, 240);
            grpRight.Controls.Add(cboGameType);
            top += 50;

            // 5. 玩法模式
            grpRight.Controls.Add(CreateLabel("玩法模式：", 25, top));
            cboPlayMode = CreateComboBox(110, top - 4, 240);
            grpRight.Controls.Add(cboPlayMode);
            top += 40; // 稍微紧凑一点，给下面留空间

            // ==================== 【重点】动态位置面板构建 ====================
            // 默认隐藏，只有 HashLottery 且特定玩法时显示
            pnlPositions = new Panel
            {
                Location = new Point(25, top),
                Size = new Size(325, 30),
                Visible = false
            };

            _posCheckBoxes = new List<CheckBox>();
            string[] posNames = { "万", "千", "百", "十", "个" };

            for (int i = 0; i < posNames.Length; i++)
            {
                var chk = new CheckBox
                {
                    Text = posNames[i],
                    Location = new Point(i * 65, 3), // 均匀分布：0, 65, 130...
                    Size = new Size(50, 24),
                    ForeColor = Color.Silver,
                    AutoSize = true,
                    Tag = i, // 关键：绑定索引 (0=万, 1=千...)，逻辑层会用到
                    Cursor = Cursors.Hand
                };
                _posCheckBoxes.Add(chk);
                pnlPositions.Controls.Add(chk);
            }
            grpRight.Controls.Add(pnlPositions);

            // 预留空间给位置面板 (即使隐藏，也占位，保持布局稳定)
            top += 45;
            // ================================================================

            // 6. 止盈止损开关
            chkRisk = new CheckBox { Text = "启用止盈止损", Location = new Point(25, top), ForeColor = Color.Silver, AutoSize = true, Cursor = Cursors.Hand };
            grpRight.Controls.Add(chkRisk);
            top += 50;

            // 7. 止盈金额
            grpRight.Controls.Add(CreateLabel("止盈金额：", 45, top));
            numStopProfit = CreateNum(130, top - 4);
            grpRight.Controls.Add(numStopProfit);
            grpRight.Controls.Add(CreateLabel("TON", 270, top)); // 单位
            top += 50;

            // 8. 止损金额
            grpRight.Controls.Add(CreateLabel("止损金额：", 45, top));
            numStopLoss = CreateNum(130, top - 4);
            grpRight.Controls.Add(numStopLoss);
            grpRight.Controls.Add(CreateLabel("TON", 270, top));

            // 9. 底部按钮 (固定在底部)
            int btnY = 670 - 80;
            btnSave = CreateButton("保存方案", 45, btnY, Color.FromArgb(0, 185, 95));
            btnCancel = CreateButton("取消", 205, btnY, Color.FromArgb(220, 60, 80));

            grpRight.Controls.Add(btnSave);
            grpRight.Controls.Add(btnCancel);
        }

        // ==================== 私有工厂方法 (简化代码) ====================

        private GroupBox CreateGroupBox(string text, int x, int y, int w, int h)
        {
            return new GroupBox
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                ForeColor = Color.Cyan
            };
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label { Text = text, Location = new Point(x, y), ForeColor = Color.Silver, AutoSize = true };
        }

        private TextBox CreateTextBox(int x, int y, int w)
        {
            return new TextBox { Location = new Point(x, y), Size = new Size(w, 30), BackColor = Color.FromArgb(35, 45, 65), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
        }

        private ComboBox CreateComboBox(int x, int y, int w)
        {
            return new ComboBox { Location = new Point(x, y), Size = new Size(w, 30), DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(35, 45, 65), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        }

        private NumericUpDown CreateNum(int x, int y)
        {
            return new NumericUpDown { Location = new Point(x, y), Size = new Size(130, 30), DecimalPlaces = 2, Maximum = 999999, BackColor = Color.FromArgb(35, 45, 65), ForeColor = Color.White };
        }

        private Button CreateButton(string text, int x, int y, Color bg)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(140, 45),
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }
    }
}