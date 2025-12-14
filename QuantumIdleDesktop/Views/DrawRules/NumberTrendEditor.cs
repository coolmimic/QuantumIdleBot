using QuantumIdleDesktop.Models.DrawRules; // 确保引用了新的 Config 命名空间
using QuantumIdleDesktop.Views.Base;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views.DrawRules
{
    public partial class NumberTrendEditor : DrawRuleEditorBase
    {
        // 引用新的配置类
        private NumberTrendRuleConfig _currentConfig = new NumberTrendRuleConfig();

        // UI 控件
        private TextBox _txtMonitorNumbers;
        private ComboBox _cmbMonitorMode; // 新增：监控模式 (遗漏/连开)
        private CheckBox _chkFullMatch;   // 新增：全匹配开关
        private NumericUpDown _nudThreshold;
        private Label _lblThreshold;      // 需要引用它来修改文字 (遗漏期数/连开期数)
        private ComboBox _cmbBetMode;
        private TextBox _txtFixedContent;
        private ComboBox _cmbTriggerMode;
        private NumericUpDown _nudContinueCount;
        private Label _lblContinueCount;
        private TableLayoutPanel _layout;

        public NumberTrendEditor()
        {
            InitializeComponent();
            InitializeUI();

            // 默认加载一次空数据
            LoadFromConfig();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.White;
            // 因为增加了一行配置，高度稍微增加一点，保持舒适
            this.Size = new Size(420, 300);

            // === 紧凑布局 ===
            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 15, 10, 10),
                RowCount = 6, // 增加到 6 行
                ColumnCount = 4,
                AutoSize = false
            };

            // 列宽定义：[标签75px] [控件30%] [标签75px] [控件30%]
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 75));
            _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // --- 行高定义 ---
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 0: 监控号码
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 1: 【新增】监控模式 & 全匹配
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 2: 阈值 & 投注模式
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));     // Row 3: 固定内容 (动态)
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 4: 触发/连投
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Row 5: 占位

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
            _txtMonitorNumbers = new TextBox { PlaceholderText = "如: 1,2,3 或 大,单", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Margin = new Padding(3, 4, 3, 3) };
            _layout.Controls.Add(_txtMonitorNumbers, 1, 0);
            _layout.SetColumnSpan(_txtMonitorNumbers, 3);

            // ========== Row 1: 【新增】监控模式 | 全匹配 ==========
            _layout.Controls.Add(CreateLbl("监控模式:"), 0, 1);

            _cmbMonitorMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Margin = new Padding(3, 4, 3, 3) };
            _cmbMonitorMode.Items.AddRange(new[] { "监控遗漏 (冷)", "监控连开 (热)" });
            // 事件：切换模式时更新下一行的标签文字
            _cmbMonitorMode.SelectedIndexChanged += (s, e) => UpdateThresholdLabel();
            _layout.Controls.Add(_cmbMonitorMode, 1, 1);

            _layout.Controls.Add(CreateLbl("匹配规则:"), 2, 1);

            _chkFullMatch = new CheckBox { Text = "必须全满足", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Margin = new Padding(3, 0, 0, 0), AutoSize = true };
            _layout.Controls.Add(_chkFullMatch, 3, 1);

            // ========== Row 2: 阈值 (遗漏/连开) | 投注模式 ==========
            _lblThreshold = CreateLbl("遗漏期数:"); // 默认文字
            _layout.Controls.Add(_lblThreshold, 0, 2);

            _nudThreshold = new NumericUpDown { Minimum = 1, Maximum = 999, Value = 10, Dock = DockStyle.Fill, TextAlign = HorizontalAlignment.Center, Margin = new Padding(3, 4, 3, 3) };
            _layout.Controls.Add(_nudThreshold, 1, 2);

            _layout.Controls.Add(CreateLbl("投注模式:"), 2, 2);
            _cmbBetMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Margin = new Padding(3, 4, 3, 3) };
            _cmbBetMode.Items.AddRange(new[] { "正投 (跟)", "反投 (杀/斩)", "固定号码" });
            _cmbBetMode.SelectedIndexChanged += (s, e) => UpdateVisibility();
            _layout.Controls.Add(_cmbBetMode, 3, 2);

            // ========== Row 3: 固定内容 (动态显隐) ==========
            var lblFixed = CreateLbl("固定内容:");
            _layout.Controls.Add(lblFixed, 0, 3);

            _txtFixedContent = new TextBox { PlaceholderText = "请输入下注内容", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Margin = new Padding(3, 4, 3, 3) };
            _layout.Controls.Add(_txtFixedContent, 1, 3);
            _layout.SetColumnSpan(_txtFixedContent, 3);

            // ========== Row 4: 触发方式 | 连投期数 ==========
            _layout.Controls.Add(CreateLbl("触发方式:"), 0, 4);

            _cmbTriggerMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9F), Margin = new Padding(3, 4, 3, 3) };
            _cmbTriggerMode.Items.AddRange(new[] { "每期检查", "触发后连投" });
            _cmbTriggerMode.SelectedIndexChanged += (s, e) => UpdateVisibility();
            _layout.Controls.Add(_cmbTriggerMode, 1, 4);

            _lblContinueCount = CreateLbl("连投期数:");
            _layout.Controls.Add(_lblContinueCount, 2, 4);

            _nudContinueCount = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 1, Dock = DockStyle.Fill, TextAlign = HorizontalAlignment.Center, Margin = new Padding(3, 4, 3, 3) };
            _layout.Controls.Add(_nudContinueCount, 3, 4);

            this.Controls.Add(_layout);
        }

        // 动态调整显隐
        private void UpdateVisibility()
        {
            // 1. 固定内容逻辑
            bool isFixed = _cmbBetMode.SelectedIndex == 2;
            var lblFixed = _layout.GetControlFromPosition(0, 3); // 注意 Row 索引变为了 3
            if (lblFixed != null) lblFixed.Visible = isFixed;
            _txtFixedContent.Visible = isFixed;

            // 2. 连投期数逻辑
            bool isContinue = _cmbTriggerMode.SelectedIndex == 1;
            _lblContinueCount.Visible = isContinue;
            _nudContinueCount.Visible = isContinue;
        }

        // 动态更新标签文字
        private void UpdateThresholdLabel()
        {
            bool isOmission = _cmbMonitorMode.SelectedIndex == 0;
            _lblThreshold.Text = isOmission ? "遗漏期数:" : "连开期数:";
        }

        public override DrawRuleConfigBase GetConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_txtMonitorNumbers.Text)) return null;

            _currentConfig.MonitorNumbers = _txtMonitorNumbers.Text.Trim();

            // 获取新字段
            _currentConfig.IsOmissionMode = _cmbMonitorMode.SelectedIndex == 0;
            _currentConfig.IsFullMatch = _chkFullMatch.Checked;
            _currentConfig.ThresholdCount = (int)_nudThreshold.Value;

            _currentConfig.BetMode = _cmbBetMode.SelectedIndex switch
            {
                0 => BetMode.Follow,
                1 => BetMode.Reverse,
                2 => BetMode.Fixed,
                _ => BetMode.Follow
            };
            _currentConfig.FixedBetContent = _currentConfig.BetMode == BetMode.Fixed ? _txtFixedContent.Text.Trim() : "";

            _currentConfig.TriggerMode = _cmbTriggerMode.SelectedIndex == 0 ? TriggerMode.CheckEveryIssue : TriggerMode.ContinueBet;
            _currentConfig.ContinueBetCount = _currentConfig.TriggerMode == TriggerMode.ContinueBet ? (int)_nudContinueCount.Value : 1;

            return _currentConfig;
        }

        public override void SetConfiguration(DrawRuleConfigBase config)
        {
            if (config is NumberTrendRuleConfig specificConfig)
            {
                _currentConfig = specificConfig;
                LoadFromConfig();
            }
        }

        private void LoadFromConfig()
        {
            _txtMonitorNumbers.Text = _currentConfig.MonitorNumbers;

            // 加载新字段
            _cmbMonitorMode.SelectedIndex = _currentConfig.IsOmissionMode ? 0 : 1;
            _chkFullMatch.Checked = _currentConfig.IsFullMatch;
            _nudThreshold.Value = _currentConfig.ThresholdCount < 1 ? 5 : _currentConfig.ThresholdCount;

            _cmbBetMode.SelectedIndex = _currentConfig.BetMode switch { BetMode.Follow => 0, BetMode.Reverse => 1, BetMode.Fixed => 2, _ => 0 };
            _txtFixedContent.Text = _currentConfig.FixedBetContent;

            _cmbTriggerMode.SelectedIndex = _currentConfig.TriggerMode == TriggerMode.CheckEveryIssue ? 0 : 1;
            _nudContinueCount.Value = _currentConfig.ContinueBetCount < 1 ? 1 : _currentConfig.ContinueBetCount;

            UpdateVisibility();
            UpdateThresholdLabel(); // 确保标签文字正确
        }
    }
}