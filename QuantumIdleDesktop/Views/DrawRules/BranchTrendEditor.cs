using QuantumIdleDesktop.Models.DrawRules;
using QuantumIdleDesktop.Views.Base;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views.DrawRules
{
    /// <summary>
    /// 分支走势规则编辑器
    /// 用于编辑 BranchTrendRuleConfig
    /// </summary>
    public partial class BranchTrendEditor : DrawRuleEditorBase
    {
        private BranchTrendRuleConfig _currentConfig = new BranchTrendRuleConfig();

        // UI 控件
        private TextBox _txtCodeZero;
        private TextBox _txtCodeOne;

        private TextBox _txtMonitorPattern;
        private ComboBox _cmbInitialBet; // 首投只有0或1，用下拉框更严谨

        private TextBox _txtWinPattern;
        private TextBox _txtLossPattern;

        private CheckBox _chkStopOnWin;
        private TableLayoutPanel _layout;

        // 美化配色
        private readonly Color _lblColor = Color.FromArgb(100, 100, 100); // 柔和灰
        private readonly Color _headerColor = Color.FromArgb(0, 120, 215); // 科技蓝标题
        private readonly Color _lineColor = Color.FromArgb(230, 230, 230); // 分割线颜色
        private readonly Font _mainFont = new Font("Segoe UI", 9F);
        private readonly Font _headerFont = new Font("Segoe UI", 9F, FontStyle.Bold);

        public BranchTrendEditor()
        {
            InitializeComponent();
            InitializeUI();
            LoadFromConfig();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.White;
            this.Size = new Size(450, 450); // 稍微加高以容纳更多字段

            // === 主布局 ===
            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                RowCount = 12,              // 行数增加以适应新字段
                ColumnCount = 4,
                AutoSize = false
            };

            // 列宽：[标签70] [输入框35%] [标签70] [输入框35%]
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // 行高规划
            // 1. 定义部分
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25)); // Header
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // 0/1 Def
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 15)); // Sep

            // 2. 监控与首投
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25)); // Header
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Monitor
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Initial
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 15)); // Sep

            // 3. 分支策略
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25)); // Header
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Win Pattern
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Loss Pattern
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 15)); // Sep

            // 4. 风控
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // StopOnWin

            // ==================== 第一部分：语义定义 ====================
            AddHeader("1. 基础语义定义 (Definition)", 0);

            _layout.Controls.Add(CreateLabel("0 代表:"), 0, 1);
            _txtCodeZero = CreateTextBox("如: 大,单");
            _layout.Controls.Add(_txtCodeZero, 1, 1);

            _layout.Controls.Add(CreateLabel("1 代表:"), 2, 1);
            _txtCodeOne = CreateTextBox("如: 小,双");
            _layout.Controls.Add(_txtCodeOne, 3, 1);

            AddSeparator(2);

            // ==================== 第二部分：监控与首投 ====================
            AddHeader("2. 监控与首投 (Monitor & Initial)", 3);

            _layout.Controls.Add(CreateLabel("监控形态:"), 0, 4);
            _txtMonitorPattern = CreateTextBox("历史出现此形态触发，如: 000111");
            _layout.Controls.Add(_txtMonitorPattern, 1, 4);
            _layout.SetColumnSpan(_txtMonitorPattern, 3);

            _layout.Controls.Add(CreateLabel("触发首投:"), 0, 5);
            _cmbInitialBet = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Font = _mainFont,
                Margin = new Padding(5, 6, 5, 5)
            };
            _cmbInitialBet.Items.Add("0");
            _cmbInitialBet.Items.Add("1");
            _layout.Controls.Add(_cmbInitialBet, 1, 5);

            // 右侧提示
            var lblHint = CreateLabel("(监控满足后，第一手投什么)");
            lblHint.TextAlign = ContentAlignment.MiddleLeft;
            _layout.Controls.Add(lblHint, 2, 5);
            _layout.SetColumnSpan(lblHint, 2);

            AddSeparator(6);

            // ==================== 第三部分：分支策略 ====================
            AddHeader("3. 分支形态策略 (Branch Logic)", 7);

            _layout.Controls.Add(CreateLabel("赢后形态:"), 0, 8);
            _txtWinPattern = CreateTextBox("赢了之后接着投，如: 111000");
            _layout.Controls.Add(_txtWinPattern, 1, 8);
            _layout.SetColumnSpan(_txtWinPattern, 3);

            _layout.Controls.Add(CreateLabel("挂后形态:"), 0, 9);
            _txtLossPattern = CreateTextBox("输了之后接着投，如: 000111");
            _layout.Controls.Add(_txtLossPattern, 1, 9);
            _layout.SetColumnSpan(_txtLossPattern, 3);

            AddSeparator(10);

            // ==================== 第四部分：风控 ====================
            _chkStopOnWin = new CheckBox
            {
                Text = " ✅ 中奖即停止 (Win Stop) - 勾选后忽略'赢后形态'",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 60, 60),
                Cursor = Cursors.Hand,
                AutoSize = true,
                Margin = new Padding(5, 5, 0, 0)
            };
            _layout.Controls.Add(_chkStopOnWin, 0, 11);
            _layout.SetColumnSpan(_chkStopOnWin, 4);

            this.Controls.Add(_layout);
        }

        // --- 辅助 UI 方法 ---

        private void AddHeader(string text, int row)
        {
            var lbl = new Label
            {
                Text = text,
                Font = _headerFont,
                ForeColor = _headerColor,
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.BottomLeft,
                Margin = new Padding(0, 0, 0, 3)
            };
            _layout.Controls.Add(lbl, 0, row);
            _layout.SetColumnSpan(lbl, 4);
        }

        private void AddSeparator(int row)
        {
            var panel = new Panel
            {
                Height = 1,
                Dock = DockStyle.Top,
                BackColor = _lineColor,
                Margin = new Padding(0, 7, 0, 7)
            };
            _layout.Controls.Add(panel, 0, row);
            _layout.SetColumnSpan(panel, 4);
        }

        private Label CreateLabel(string text) => new Label
        {
            Text = text,
            TextAlign = ContentAlignment.MiddleRight,
            Dock = DockStyle.Fill,
            Font = _mainFont,
            ForeColor = _lblColor,
            Margin = new Padding(0)
        };

        private TextBox CreateTextBox(string placeholder) => new TextBox
        {
            PlaceholderText = placeholder,
            Dock = DockStyle.Fill,
            Font = _mainFont,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(5, 6, 5, 5)
        };

        // --- 逻辑部分 (适配 BranchTrendRuleConfig) ---

        public override DrawRuleConfigBase GetConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_txtCodeZero.Text) || string.IsNullOrWhiteSpace(_txtCodeOne.Text)) return null;
            if (string.IsNullOrWhiteSpace(_txtMonitorPattern.Text)) return null;
            if (_cmbInitialBet.SelectedIndex == -1) return null; // 必须选首投

            _currentConfig.CodeZeroDefinition = _txtCodeZero.Text.Trim();
            _currentConfig.CodeOneDefinition = _txtCodeOne.Text.Trim();

            _currentConfig.MonitorPattern = CleanPattern(_txtMonitorPattern.Text);
            _currentConfig.InitialBet = _cmbInitialBet.SelectedItem.ToString();

            _currentConfig.WinPattern = CleanPattern(_txtWinPattern.Text);
            _currentConfig.LossPattern = CleanPattern(_txtLossPattern.Text);

            _currentConfig.StopOnWin = _chkStopOnWin.Checked;

            return _currentConfig;
        }

        public override void SetConfiguration(DrawRuleConfigBase config)
        {
            if (config is BranchTrendRuleConfig specificConfig)
            {
                _currentConfig = specificConfig;
                LoadFromConfig();
            }
        }

        private void LoadFromConfig()
        {
            _txtCodeZero.Text = _currentConfig.CodeZeroDefinition;
            _txtCodeOne.Text = _currentConfig.CodeOneDefinition;

            _txtMonitorPattern.Text = _currentConfig.MonitorPattern;

            // 设置下拉框选中项
            if (_currentConfig.InitialBet == "1")
                _cmbInitialBet.SelectedIndex = 1;
            else
                _cmbInitialBet.SelectedIndex = 0; // 默认 0

            _txtWinPattern.Text = _currentConfig.WinPattern;
            _txtLossPattern.Text = _currentConfig.LossPattern;

            _chkStopOnWin.Checked = _currentConfig.StopOnWin;
        }

        private string CleanPattern(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            // 只保留 0 和 1
            char[] arr = input.Where(c => c == '0' || c == '1').ToArray();
            return new string(arr);
        }
    }
}