using QuantumIdleDesktop.Models.DrawRules;
using QuantumIdleDesktop.Views.Base;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views.DrawRules
{
    /// <summary>
    /// 结果跟随规则编辑器 (ResultFollowEditor)
    /// 用于编辑 ResultFollowRuleConfig
    /// </summary>
    public partial class ResultFollowEditor : DrawRuleEditorBase
    {
        private ResultFollowRuleConfig _currentConfig = new ResultFollowRuleConfig();

        // UI 控件
        private TextBox _txtCodeZero;
        private TextBox _txtCodeOne;

        private TextBox _txtSequenceOnZero;
        private TextBox _txtSequenceOnOne;

        private CheckBox _chkStopOnWin;
        private TableLayoutPanel _layout;

        // 美化配色 (保持与 BranchTrendEditor 一致)
        private readonly Color _lblColor = Color.FromArgb(100, 100, 100);
        private readonly Color _headerColor = Color.FromArgb(0, 120, 215);
        private readonly Color _lineColor = Color.FromArgb(230, 230, 230);
        private readonly Font _mainFont = new Font("Segoe UI", 9F);
        private readonly Font _headerFont = new Font("Segoe UI", 9F, FontStyle.Bold);

        public ResultFollowEditor()
        {
            InitializeComponent();
            InitializeUI();
            LoadFromConfig();
        }

        private void InitializeComponent()
        {
            this.Name = "ResultFollowEditor";
            this.Size = new Size(450, 450);
        }

        private void InitializeUI()
        {
            this.BackColor = Color.White;

            // === 主布局 ===
            _layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                RowCount = 10,
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

            // 2. 触发序列配置
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25)); // Header
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Seq on Zero
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); // Seq on One
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 15)); // Sep

            // 3. 风控
            _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25)); // Header
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

            // ==================== 第二部分：跟投序列配置 ====================
            AddHeader("2. 跟投序列配置 (Follow Sequences)", 3);

            // 开0 触发
            _layout.Controls.Add(CreateLabel("开0(大)时:"), 0, 4);
            _txtSequenceOnZero = CreateTextBox("上期开0后执行的序列，如: 000111");
            _layout.Controls.Add(_txtSequenceOnZero, 1, 4);
            _layout.SetColumnSpan(_txtSequenceOnZero, 3);

            // 开1 触发
            _layout.Controls.Add(CreateLabel("开1(小)时:"), 0, 5);
            _txtSequenceOnOne = CreateTextBox("上期开1后执行的序列，如: 111000");
            _layout.Controls.Add(_txtSequenceOnOne, 1, 5);
            _layout.SetColumnSpan(_txtSequenceOnOne, 3);

            AddSeparator(6);

            // ==================== 第三部分：风控与逻辑 ====================
            AddHeader("3. 风控逻辑 (Control)", 7);

            _chkStopOnWin = new CheckBox
            {
                Text = " ✅ 中奖即停止 (Win Stop) - 推荐勾选，中奖后重新检测",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(60, 60, 60),
                Cursor = Cursors.Hand,
                AutoSize = true,
                Margin = new Padding(5, 5, 0, 0)
            };
            _layout.Controls.Add(_chkStopOnWin, 0, 8);
            _layout.SetColumnSpan(_chkStopOnWin, 4);

            this.Controls.Add(_layout);
        }

        // --- 辅助 UI 方法 (与模板保持一致) ---

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

        // --- 逻辑部分 (适配 ResultFollowRuleConfig) ---

        public override DrawRuleConfigBase GetConfiguration()
        {
            // 基础校验
            if (string.IsNullOrWhiteSpace(_txtCodeZero.Text) || string.IsNullOrWhiteSpace(_txtCodeOne.Text)) return null;

            // 至少需要填写一个序列
            bool hasSeqZero = !string.IsNullOrWhiteSpace(_txtSequenceOnZero.Text);
            bool hasSeqOne = !string.IsNullOrWhiteSpace(_txtSequenceOnOne.Text);

            if (!hasSeqZero && !hasSeqOne)
            {
                MessageBox.Show("请至少填写一种触发序列 (开0 或 开1)", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }

            _currentConfig.CodeZeroDefinition = _txtCodeZero.Text.Trim();
            _currentConfig.CodeOneDefinition = _txtCodeOne.Text.Trim();

            _currentConfig.SequenceOnZero = CleanPattern(_txtSequenceOnZero.Text);
            _currentConfig.SequenceOnOne = CleanPattern(_txtSequenceOnOne.Text);

            _currentConfig.StopOnWin = _chkStopOnWin.Checked;

            return _currentConfig;
        }

        public override void SetConfiguration(DrawRuleConfigBase config)
        {
            if (config is ResultFollowRuleConfig specificConfig)
            {
                _currentConfig = specificConfig;
                LoadFromConfig();
            }
        }

        private void LoadFromConfig()
        {
            _txtCodeZero.Text = _currentConfig.CodeZeroDefinition;
            _txtCodeOne.Text = _currentConfig.CodeOneDefinition;

            _txtSequenceOnZero.Text = _currentConfig.SequenceOnZero;
            _txtSequenceOnOne.Text = _currentConfig.SequenceOnOne;

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