using QuantumIdleDesktop.Models.DrawRules;
using QuantumIdleDesktop.Views.Base;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views.DrawRules
{
    public partial class SlayDragonFollowDragonEditor : DrawRuleEditorBase
    {
        private SlayDragonFollowDragonRuleConfig _currentConfig = new SlayDragonFollowDragonRuleConfig();

        // UI 控件
        private TextBox _txtMonitorTags;
        private NumericUpDown _nudConsecutive;
        private ComboBox _cmbBetMode;
        private TextBox _txtFixedContent;
        private ComboBox _cmbTriggerMode;
        private NumericUpDown _nudContinueCount;
        private Label _lblContinueCount; // 需要引用的标签以便隐藏
        private TableLayoutPanel _layout; // 主布局引用

        public SlayDragonFollowDragonEditor()
        {
            InitializeComponent();
            InitializeUI();

            // 默认加载一次空数据，确保UI状态正确
            LoadFromConfig();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.White;
            this.Size = new Size(410, 260); //稍微加高一点点防止拥挤

            // === 紧凑布局 ===
            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 15, 10, 10),
                RowCount = 5, // 改为 5 行 (增加一个占位行)
                ColumnCount = 4,
                AutoSize = false
            };

            // 列宽定义：[标签70px] [控件30%] [标签70px] [控件30%]
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // --- 关键修改：行高定义 ---
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 0: 监控号码
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 1: 连开/模式
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // Row 2: 固定内容 (动态)
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 3: 触发/连投

            // Row 4: 占位行 (吃掉所有剩余高度，把上面的行顶上去)
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Helper: 创建紧凑标签
            Label CreateLbl(string text) => new Label
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleRight,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Margin = new Padding(0, 0, 3, 0)
            };

            // ========== Row 0: 监控号码 ==========
            _layout.Controls.Add(CreateLbl("监控号码:"), 0, 0);
            _txtMonitorTags = new TextBox { PlaceholderText = "如: 大 单", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Margin = new Padding(3, 4, 3, 3) };
            _layout.Controls.Add(_txtMonitorTags, 1, 0);
            _layout.SetColumnSpan(_txtMonitorTags, 3);

            // ========== Row 1: 连开期数 | 投注模式 ==========
            _layout.Controls.Add(CreateLbl("连开期数:"), 0, 1);
            _nudConsecutive = new NumericUpDown { Minimum = 1, Maximum = 99, Value = 2, Dock = DockStyle.Fill, TextAlign = HorizontalAlignment.Center, Margin = new Padding(3, 4, 3, 3) };
            _layout.Controls.Add(_nudConsecutive, 1, 1);

            _layout.Controls.Add(CreateLbl("投注模式:"), 2, 1);
            _cmbBetMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Margin = new Padding(3, 4, 3, 3) };
            _cmbBetMode.Items.AddRange(new[] { "正投 (跟)", "反投 (斩)", "固定号码" });
            _cmbBetMode.SelectedIndexChanged += (s, e) => UpdateVisibility();
            _layout.Controls.Add(_cmbBetMode, 3, 1);

            // ========== Row 2: 固定内容 (动态显隐) ==========
            var lblFixed = CreateLbl("固定内容:");
            _layout.Controls.Add(lblFixed, 0, 2); // 放到 Layout 里才能控制 Visible

            _txtFixedContent = new TextBox { PlaceholderText = "请输入下注内容", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Margin = new Padding(3, 4, 3, 3) };
            _layout.Controls.Add(_txtFixedContent, 1, 2);
            _layout.SetColumnSpan(_txtFixedContent, 3);

            // ========== Row 3: 触发方式 | 连投期数 ==========
            _layout.Controls.Add(CreateLbl("触发方式:"), 0, 3);

            _cmbTriggerMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Margin = new Padding(3, 4, 3, 3) };
            _cmbTriggerMode.Items.AddRange(new[] { "每期检查", "触发后连投" });
            _cmbTriggerMode.SelectedIndexChanged += (s, e) => UpdateVisibility();
            _layout.Controls.Add(_cmbTriggerMode, 1, 3);

            _lblContinueCount = CreateLbl("连投期数:");
            _layout.Controls.Add(_lblContinueCount, 2, 3);

            _nudContinueCount = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 3, Dock = DockStyle.Fill, TextAlign = HorizontalAlignment.Center, Margin = new Padding(3, 4, 3, 3) };
            _layout.Controls.Add(_nudContinueCount, 3, 3);

            this.Controls.Add(_layout);
        }

        // 动态调整显隐
        private void UpdateVisibility()
        {
            // 1. 固定内容逻辑
            bool isFixed = _cmbBetMode.SelectedIndex == 2;

            // 同时控制 Label 和 TextBox 的 Visible
            // TableLayoutPanel 的 RowStyle 为 AutoSize，当该行所有控件 Visible=false 时，行高变为0
            var lblFixed = _layout.GetControlFromPosition(0, 2);
            if (lblFixed != null) lblFixed.Visible = isFixed;
            _txtFixedContent.Visible = isFixed;

            // 2. 连投期数逻辑
            bool isContinue = _cmbTriggerMode.SelectedIndex == 1;
            _lblContinueCount.Visible = isContinue;
            _nudContinueCount.Visible = isContinue;
        }

        public override DrawRuleConfigBase GetConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_txtMonitorTags.Text)) return null;

            var rule = _currentConfig.MonitorRule;
            rule.MonitorTags = _txtMonitorTags.Text.Trim();
            rule.RequiredConsecutiveCount = (int)_nudConsecutive.Value;

            rule.BetMode = _cmbBetMode.SelectedIndex switch
            {
                0 => BetMode.Follow,
                1 => BetMode.Reverse,
                2 => BetMode.Fixed,
                _ => BetMode.Follow
            };
            rule.FixedBetContent = rule.BetMode == BetMode.Fixed ? _txtFixedContent.Text.Trim() : "";

            rule.TriggerMode = _cmbTriggerMode.SelectedIndex == 0 ? TriggerMode.CheckEveryIssue : TriggerMode.ContinueBet;
            rule.ContinueBetCount = rule.TriggerMode == TriggerMode.ContinueBet ? (int)_nudContinueCount.Value : 1;

            return _currentConfig;
        }

        public override void SetConfiguration(DrawRuleConfigBase config)
        {
            if (config is SlayDragonFollowDragonRuleConfig specificConfig)
            {
                _currentConfig = specificConfig;
                if (_currentConfig.MonitorRule == null) _currentConfig.MonitorRule = new SlayDragonMonitorRule();
                LoadFromConfig();
            }
        }

        private void LoadFromConfig()
        {
            var rule = _currentConfig.MonitorRule;
            _txtMonitorTags.Text = rule.MonitorTags;
            _nudConsecutive.Value = rule.RequiredConsecutiveCount < 1 ? 2 : rule.RequiredConsecutiveCount;

            _cmbBetMode.SelectedIndex = rule.BetMode switch { BetMode.Follow => 0, BetMode.Reverse => 1, BetMode.Fixed => 2, _ => 0 };
            _txtFixedContent.Text = rule.FixedBetContent;

            _cmbTriggerMode.SelectedIndex = rule.TriggerMode == TriggerMode.CheckEveryIssue ? 0 : 1;
            _nudContinueCount.Value = rule.ContinueBetCount < 1 ? 1 : rule.ContinueBetCount;

            UpdateVisibility();
        }
    }
}