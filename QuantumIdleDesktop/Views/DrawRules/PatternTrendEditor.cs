using QuantumIdleDesktop.Models.DrawRules;
using QuantumIdleDesktop.Views.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views.DrawRules
{
    public partial class PatternTrendEditor : DrawRuleEditorBase
    {
        private PatternTrendRuleConfig _currentConfig = new PatternTrendRuleConfig();

        // --- 控件 ---
        private TextBox _txtCodeZero;
        private TextBox _txtCodeOne;

        private Panel _panelAdd;
        private TextBox _txtInMonitor;
        private TextBox _txtInBet;
        private CheckBox _chkInStop;
        private Button _btnAdd;

        private DataGridView _dgv;

        // --- 美化配色常量 ---
        private readonly Font _fontText = new Font("微软雅黑", 9F);
        private readonly Font _fontBold = new Font("微软雅黑", 9F, FontStyle.Bold);
        private readonly Color _colBlue = Color.FromArgb(0, 120, 215);   // 标题蓝
        private readonly Color _colGreen = Color.FromArgb(40, 167, 69);  // 按钮绿
        private readonly Color _colRed = Color.FromArgb(220, 53, 69);    // 删除红
        private readonly Color _colGrayBg = Color.FromArgb(248, 249, 250); // 面板灰背景
        private readonly Color _colLabel = Color.FromArgb(100, 100, 100); // 标签深灰
        private readonly Color _colBorder = Color.FromArgb(220, 220, 220); // 边框淡灰

        public PatternTrendEditor()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.BackColor = Color.White;
            this.Size = new Size(420, 300);
            this.AutoScaleMode = AutoScaleMode.None;

            // ================================================================
            // 1. 顶部：全局定义
            // ================================================================
            var lblTitle1 = CreateTitle("1. 基础语义 (Definitions)", 10, 8);
            this.Controls.Add(lblTitle1);

            // 0 代表
            this.Controls.Add(CreateLabel("0 代表:", 15, 38));
            _txtCodeZero = CreateInput(65, 35, 130);
            _txtCodeZero.PlaceholderText = "如: 大";

            // 1 代表
            this.Controls.Add(CreateLabel("1 代表:", 210, 38));
            _txtCodeOne = CreateInput(260, 35, 130);
            _txtCodeOne.PlaceholderText = "如: 小";

            this.Controls.Add(_txtCodeZero);
            this.Controls.Add(_txtCodeOne);

            // ================================================================
            // 2. 中间：添加策略面板 (两行布局)
            // ================================================================
            var lblTitle2 = CreateTitle("2. 策略配置 (Config)", 10, 68);
            this.Controls.Add(lblTitle2);

            _panelAdd = new Panel
            {
                Location = new Point(0, 92),
                Size = new Size(420, 75), // 高度增加，容纳两行
                BackColor = _colGrayBg
            };

            // --- 第一行：监控 和 下注 ---
            var lblMon = CreateLabel("监控:", 15, 13);
            _txtInMonitor = CreateInput(55, 10, 100);
            _txtInMonitor.PlaceholderText = "000";

            var lblBet = CreateLabel("下注:", 170, 13);
            _txtInBet = CreateInput(210, 10, 100);
            _txtInBet.PlaceholderText = "111";

            // --- 第二行：赢即停 和 按钮 ---
            _chkInStop = new CheckBox
            {
                Text = "赢即停 (Win Stop)",
                Location = new Point(55, 43), // 换行
                AutoSize = true,
                Font = new Font("微软雅黑", 8.5F),
                ForeColor = _colLabel,
                Checked = true,
                Cursor = Cursors.Hand
            };

            _btnAdd = new Button
            {
                Text = "添加形态",
                Location = new Point(215, 38), // 靠右放
                Size = new Size(90, 28),
                BackColor = _colGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = _fontBold,
                Cursor = Cursors.Hand
            };
            _btnAdd.FlatAppearance.BorderSize = 0;
            _btnAdd.Click += _btnAdd_Click;

            _panelAdd.Controls.Add(lblMon); _panelAdd.Controls.Add(_txtInMonitor);
            _panelAdd.Controls.Add(lblBet); _panelAdd.Controls.Add(_txtInBet);
            _panelAdd.Controls.Add(_chkInStop);
            _panelAdd.Controls.Add(_btnAdd);
            this.Controls.Add(_panelAdd);

            // ================================================================
            // 3. 底部：美化后的 DataGridView
            // ================================================================
            _dgv = new DataGridView
            {
                Location = new Point(10, 175), // 紧接灰色面板下方
                Size = new Size(400, 115),     // 剩余空间
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None, // 去掉外框，更现代

                // 核心美化属性
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AllowUserToResizeColumns = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                ReadOnly = true,
                Font = _fontText,

                GridColor = Color.FromArgb(240, 240, 240), // 极淡的网格线
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, // 只显示横线
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            // 表头美化
            _dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(235, 238, 242), // 淡灰蓝表头
                ForeColor = Color.FromArgb(80, 80, 80),    // 深灰文字
                Font = _fontBold,
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(0, 6, 0, 6)
            };
            _dgv.ColumnHeadersHeight = 32;

            // 单元格美化
            _dgv.DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(60, 60, 60),
                SelectionBackColor = Color.FromArgb(230, 242, 255), // 选中变为清爽淡蓝
                SelectionForeColor = Color.Black // 选中文字保持黑色
            };
            _dgv.RowTemplate.Height = 28; // 行高

            // 列定义
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "监控", DataPropertyName = "Monitor", Width = 110 });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "投注", DataPropertyName = "Bet", Width = 110 });
            _dgv.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "赢即停", DataPropertyName = "StopWin", Width = 80 });

            // 删除按钮列美化
            var btnCol = new DataGridViewButtonColumn
            {
                HeaderText = "操作",
                Text = "删除",
                UseColumnTextForButtonValue = true,
                Width = 80,
                FlatStyle = FlatStyle.Flat
            };
            btnCol.DefaultCellStyle.ForeColor = _colRed;
            btnCol.DefaultCellStyle.SelectionForeColor = _colRed;
            _dgv.Columns.Add(btnCol);

            _dgv.CellContentClick += _dgv_CellContentClick;
            this.Controls.Add(_dgv);

            // 画一条淡灰线区分 DGV 和输入区
            var line = new Panel { Location = new Point(10, 174), Size = new Size(400, 1), BackColor = Color.FromArgb(230, 230, 230) };
            this.Controls.Add(line);
        }

        // ================================================================
        // 逻辑处理
        // ================================================================

        private void _btnAdd_Click(object sender, EventArgs e)
        {
            string m = CleanPattern(_txtInMonitor.Text);
            string b = CleanPattern(_txtInBet.Text);

            if (string.IsNullOrEmpty(m) || string.IsNullOrEmpty(b))
            {
                MessageBox.Show("请填写有效内容 (0/1)", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _currentConfig.StrategyList.Add(new TrendStrategyItem { MonitorPattern = m, BetPattern = b, StopOnWin = _chkInStop.Checked });
            RefreshGrid();

            _txtInMonitor.Clear(); _txtInBet.Clear(); _txtInMonitor.Focus();
        }

        private void _dgv_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && _dgv.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                _currentConfig.StrategyList.RemoveAt(e.RowIndex);
                RefreshGrid();
            }
        }

        private void RefreshGrid()
        {
            _dgv.Rows.Clear();
            if (_currentConfig.StrategyList == null) return;
            foreach (var item in _currentConfig.StrategyList)
                _dgv.Rows.Add(item.MonitorPattern, item.BetPattern, item.StopOnWin);
        }

        // ================================================================
        // 配置读写
        // ================================================================
        public override void SetConfiguration(DrawRuleConfigBase config)
        {
            if (config is PatternTrendRuleConfig c)
            {
                _currentConfig = new PatternTrendRuleConfig
                {
                    CodeZeroDefinition = c.CodeZeroDefinition,
                    CodeOneDefinition = c.CodeOneDefinition,
                    StrategyList = c.StrategyList.Select(x => new TrendStrategyItem { MonitorPattern = x.MonitorPattern, BetPattern = x.BetPattern, StopOnWin = x.StopOnWin }).ToList()
                };
                LoadFromConfig();
            }
        }

        public override DrawRuleConfigBase GetConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_txtCodeZero.Text) || string.IsNullOrWhiteSpace(_txtCodeOne.Text)) return null;
            _currentConfig.CodeZeroDefinition = _txtCodeZero.Text.Trim();
            _currentConfig.CodeOneDefinition = _txtCodeOne.Text.Trim();
            return _currentConfig;
        }

        private void LoadFromConfig()
        {
            _txtCodeZero.Text = _currentConfig.CodeZeroDefinition;
            _txtCodeOne.Text = _currentConfig.CodeOneDefinition;
            RefreshGrid();
        }

        private string CleanPattern(string s) => string.IsNullOrEmpty(s) ? "" : new string(s.Where(c => c == '0' || c == '1').ToArray());

        // ================================================================
        // UI 工厂方法
        // ================================================================
        private Label CreateTitle(string text, int x, int y)
        {
            return new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = _fontBold, ForeColor = _colBlue };
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label { Text = text, Location = new Point(x, y), AutoSize = true, Font = _fontText, ForeColor = _colLabel };
        }

        private TextBox CreateInput(int x, int y, int w)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 23),
                Font = _fontText,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
        }
    }
}