using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QuantumIdleDesktop.Models.DrawRules;

namespace QuantumIdleDesktop.Views.DrawRules
{
    public partial class SlayDragonFollowDragonSettingForm : Form
    {
        // 数据源
        private BindingList<SlayDragonMonitorRule> _ruleList;
        private SlayDragonFollowDragonRuleConfig _workingConfig;
        public SlayDragonFollowDragonRuleConfig ResultConfig { get; private set; }

        // 控件引用 (需要动态控制可见性的控件)
        private TextBox txtMonitorTags;
        private NumericUpDown nudConsecutive;
        private ComboBox cmbBetMode;
        private TextBox txtFixedContent;      // 动态显示
        private Label lblFixedContent;        // 动态显示
        private ComboBox cmbTriggerMode;
        private NumericUpDown nudContinueCount; // 动态显示
        private Label lblContinueCount;         // 动态显示
        private DataGridView dgvRules;

        public SlayDragonFollowDragonSettingForm(SlayDragonFollowDragonRuleConfig config = null)
        {
            _workingConfig = config ?? new SlayDragonFollowDragonRuleConfig();
            //_ruleList = new BindingList<SlayDragonMonitorRule>(_workingConfig.MonitorRules ?? new List<SlayDragonMonitorRule>());

            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            // 1. 窗体设置
            this.Text = "斩龙跟龙 - 规则配置";
            this.Size = new Size(650, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.BackColor = Color.FromArgb(243, 244, 246);

            // 主容器：上部分是输入区，中间是表格，底部是按钮
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                RowCount = 3,
                ColumnCount = 1
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // 输入区自适应
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // 表格填充
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // 底部按钮

            // 2. 添加模块
            mainLayout.Controls.Add(CreateInputArea(), 0, 0);
            mainLayout.Controls.Add(CreateGridArea(), 0, 1);
            mainLayout.Controls.Add(CreateBottomButtons(), 0, 2);

            this.Controls.Add(mainLayout);
        }

        // ==================== 输入区域 (核心逻辑) ====================
        private GroupBox CreateInputArea()
        {
            var gb = new GroupBox
            {
                Text = "添加新规则",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(10)
            };

            // 使用 TableLayout 规整排列输入项 (3行 X 4列)
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(0, 5, 0, 0)
            };
            // 列宽比例：标签列固定宽度，输入列自适应
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); // Label
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));  // Input
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90)); // Label
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));  // Input

            // --- 第一行：监控号码 + 连开期数 ---
            table.Controls.Add(new Label { Text = "监控号码:", TextAlign = ContentAlignment.MiddleRight, AutoSize = true, Anchor = AnchorStyles.Right }, 0, 0);
            txtMonitorTags = new TextBox { PlaceholderText = "如: 大,小", Anchor = AnchorStyles.Left | AnchorStyles.Right };
            table.Controls.Add(txtMonitorTags, 1, 0);

            table.Controls.Add(new Label { Text = "连开期数:", TextAlign = ContentAlignment.MiddleRight, AutoSize = true, Anchor = AnchorStyles.Right }, 2, 0);
            nudConsecutive = new NumericUpDown { Minimum = 1, Value = 2, Maximum = 50, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            table.Controls.Add(nudConsecutive, 3, 0);

            // --- 第二行：投注模式 + (动态)固定内容 ---
            table.Controls.Add(new Label { Text = "投注模式:", TextAlign = ContentAlignment.MiddleRight, AutoSize = true, Anchor = AnchorStyles.Right }, 0, 1);
            cmbBetMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cmbBetMode.Items.AddRange(new[] { "正投 (跟)", "反投 (斩)", "固定号码" });
            cmbBetMode.SelectedIndex = 0;
            cmbBetMode.SelectedIndexChanged += CmbBetMode_SelectedIndexChanged; // 绑定事件
            table.Controls.Add(cmbBetMode, 1, 1);

            lblFixedContent = new Label { Text = "号码内容:", TextAlign = ContentAlignment.MiddleRight, AutoSize = true, Anchor = AnchorStyles.Right, Visible = false };
            table.Controls.Add(lblFixedContent, 2, 1);
            txtFixedContent = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Visible = false };
            table.Controls.Add(txtFixedContent, 3, 1);

            // --- 第三行：触发方式 + (动态)连投期数 ---
            table.Controls.Add(new Label { Text = "触发方式:", TextAlign = ContentAlignment.MiddleRight, AutoSize = true, Anchor = AnchorStyles.Right }, 0, 2);
            cmbTriggerMode = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            cmbTriggerMode.Items.AddRange(new[] { "每期检查", "触发后连投" });
            cmbTriggerMode.SelectedIndex = 0;
            cmbTriggerMode.SelectedIndexChanged += CmbTriggerMode_SelectedIndexChanged; // 绑定事件
            table.Controls.Add(cmbTriggerMode, 1, 2);

            lblContinueCount = new Label { Text = "连投期数:", TextAlign = ContentAlignment.MiddleRight, AutoSize = true, Anchor = AnchorStyles.Right, Visible = false };
            table.Controls.Add(lblContinueCount, 2, 2);
            nudContinueCount = new NumericUpDown { Minimum = 1, Value = 3, Maximum = 100, Anchor = AnchorStyles.Left | AnchorStyles.Right, Visible = false };
            table.Controls.Add(nudContinueCount, 3, 2);

            // --- 添加按钮 (单独放下面或者放在右侧，这里放在最下面一行占满) ---
            var btnAdd = new Button
            {
                Text = "＋ 添加规则",
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height = 35,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 15, 0, 5)
            };
            btnAdd.Click += BtnAdd_Click;

            var pnl = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Dock = DockStyle.Top };
            pnl.Controls.Add(table);
            pnl.Controls.Add(btnAdd); // 布局稍微调整，将Table放入，Button在下面

            var container = new Panel { Dock = DockStyle.Top, AutoSize = true };
            table.Dock = DockStyle.Top;
            btnAdd.Dock = DockStyle.Bottom;
            container.Controls.Add(btnAdd);
            container.Controls.Add(table);

            gb.Controls.Add(container);
            return gb;
        }

        // ==================== 表格区域 ====================
        private Control CreateGridArea()
        {
            dgvRules = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeight = 32
            };

            // 样式
            dgvRules.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 230, 230);
            dgvRules.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvRules.EnableHeadersVisualStyles = false;

            // 列定义
            dgvRules.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "监控号码", DataPropertyName = "MonitorTags", FillWeight = 30 });
            dgvRules.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "连开", DataPropertyName = "RequiredConsecutiveCount", FillWeight = 15 });

            // 模式列 (格式化显示)
            var colMode = new DataGridViewTextBoxColumn { HeaderText = "投注模式", DataPropertyName = "BetMode", FillWeight = 20 };
            dgvRules.Columns.Add(colMode);

            // 触发详情列
            var colTrigger = new DataGridViewTextBoxColumn { HeaderText = "触发/参数", DataPropertyName = "TriggerMode", FillWeight = 25 };
            dgvRules.Columns.Add(colTrigger);

            // 删除按钮
            var btnCol = new DataGridViewButtonColumn
            {
                HeaderText = "操作",
                Text = "删除",
                UseColumnTextForButtonValue = true,
                FillWeight = 10,
                FlatStyle = FlatStyle.Popup
            };
            btnCol.DefaultCellStyle.ForeColor = Color.Red;
            dgvRules.Columns.Add(btnCol);

            dgvRules.DataSource = _ruleList;
            dgvRules.CellFormatting += DgvRules_CellFormatting;
            dgvRules.CellContentClick += DgvRules_CellContentClick;

            return dgvRules;
        }

        // ==================== 底部按钮 ====================
        private Control CreateBottomButtons()
        {
            var pnl = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 10, 0, 0) };
            var btnCancel = new Button { Text = "取消", Size = new Size(80, 32), DialogResult = DialogResult.Cancel };
            var btnOk = new Button { Text = "确定保存", Size = new Size(100, 32), DialogResult = DialogResult.OK, BackColor = Color.FromArgb(37, 99, 235), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            pnl.Controls.Add(btnOk);
            pnl.Controls.Add(btnCancel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
            return pnl;
        }

        // ==================== 事件逻辑 ====================

        // 动态控制：投注模式变化
        private void CmbBetMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 假设索引 2 是 "固定号码"
            bool isFixed = cmbBetMode.SelectedIndex == 2;
            lblFixedContent.Visible = isFixed;
            txtFixedContent.Visible = isFixed;
        }

        // 动态控制：触发方式变化
        private void CmbTriggerMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 假设索引 1 是 "触发后连投"
            bool isContinue = cmbTriggerMode.SelectedIndex == 1;
            lblContinueCount.Visible = isContinue;
            nudContinueCount.Visible = isContinue;
        }
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // 基础校验
            if (string.IsNullOrWhiteSpace(txtMonitorTags.Text))
            {
                MessageBox.Show("请输入监控号码");
                return;
            }

            var betMode = cmbBetMode.SelectedIndex switch
            {
                0 => BetMode.Follow,
                1 => BetMode.Reverse,
                2 => BetMode.Fixed,
                _ => BetMode.Follow
            };

            // 如果选择了固定号码，校验内容是否为空
            if (betMode == BetMode.Fixed && string.IsNullOrWhiteSpace(txtFixedContent.Text))
            {
                MessageBox.Show("请填写固定号码内容");
                return;
            }

            var triggerMode = cmbTriggerMode.SelectedIndex == 0 ? TriggerMode.CheckEveryIssue : TriggerMode.ContinueBet;

            // 创建对象
            var rule = new SlayDragonMonitorRule
            {
                MonitorTags = txtMonitorTags.Text.Trim(),
                RequiredConsecutiveCount = (int)nudConsecutive.Value,
                BetMode = betMode,
                FixedBetContent = betMode == BetMode.Fixed ? txtFixedContent.Text.Trim() : "",
                TriggerMode = triggerMode,
                ContinueBetCount = triggerMode == TriggerMode.ContinueBet ? (int)nudContinueCount.Value : 0
            };

            _ruleList.Add(rule);

            // 清空输入以便再次添加
            txtMonitorTags.Clear();
            txtMonitorTags.Focus();
        }
        private void DgvRules_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _ruleList.Count) return;
            var rule = _ruleList[e.RowIndex];

            // 格式化“投注模式”
            if (dgvRules.Columns[e.ColumnIndex].DataPropertyName == "BetMode")
            {
                e.Value = rule.BetMode switch
                {
                    BetMode.Follow => "正投",
                    BetMode.Reverse => "反投",
                    BetMode.Fixed => $"固定: {rule.FixedBetContent}", // 直接在这里显示固定内容，一目了然
                    _ => rule.BetMode.ToString()
                };
            }
            // 格式化“触发/参数”
            else if (dgvRules.Columns[e.ColumnIndex].DataPropertyName == "TriggerMode")
            {
                e.Value = rule.TriggerMode == TriggerMode.ContinueBet
                    ? $"连投 {rule.ContinueBetCount} 期"
                    : "每期检查";
            }
        }
        private void DgvRules_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvRules.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                _ruleList.RemoveAt(e.RowIndex);
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (DialogResult == DialogResult.OK)
            {
                //_workingConfig.MonitorRules = _ruleList.ToList();
                // 移除了全局冷却的赋值
                ResultConfig = _workingConfig;
            }
        }
    }
}