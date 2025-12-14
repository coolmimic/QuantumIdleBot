using System;
using System.Drawing;
using System.Windows.Forms;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Forms;
using QuantumIdleDesktop.Utils;

namespace QuantumIdleDesktop.Views
{
    public partial class ViewSetting : UserControl
    {
        // --- 控件声明 ---
        private Panel pnlRisk, pnlDelay, pnlSchedule;
        private NumericUpDown numStopTurnover, numStopLoss, numStopProfit;
        private CheckBox chkRisk; // 虽然叫chk，但我们会把它画成按钮
        private NumericUpDown numDelayMax, numDelayMin;
        private CheckBox chkDelay;
        private DateTimePicker dtpEnd, dtpStart;
        private CheckBox chkSchedule;
        private Button btnSave, btnSchemeRotation, btnMultiplyStrategy;


        // --- 配色方案 (高对比度) ---
        private readonly Color _mainBg = Color.FromArgb(30, 30, 30);
        private readonly Color _blockBg = Color.FromArgb(43, 43, 46);

        // 状态颜色
        private readonly Color _textEnabled = Color.White;
        private readonly Color _textDisabled = Color.FromArgb(120, 120, 120); // 禁用时的文字颜色（亮灰，绝不是黑色）
        private readonly Color _inputBack = Color.FromArgb(60, 60, 65);

        // 强调色
        private readonly Color _accentBlue = Color.FromArgb(0, 120, 215);
        private readonly Color _accentYellow = Color.FromArgb(200, 140, 0); //稍微调暗一点黄，保证白字可见
        private readonly Color _accentPurple = Color.FromArgb(140, 60, 190);
        private readonly Color _btnOff = Color.FromArgb(80, 80, 80); // 开关关闭时的背景色

        public ViewSetting()
        {
            InitializeComponent();
            this.Size = new Size(960, 340);
            InitializeComponentCustom();
            InitControlStyles();
            LoadSettings();
            BindEvents();
        }

        private void InitializeComponentCustom()
        {
            this.SuspendLayout();
            this.BackColor = _mainBg;
            this.Font = new Font("Microsoft YaHei UI", 9F);
            this.ForeColor = Color.White;

            int topMargin = 10;
            int leftX = 20;
            int rightX = 500;
            int panelWidth = 460;
            int leftPanelHeight = 260;
            int rightPanelHeight = 125;
            int btnY = 285; // 底部按钮Y坐标

            // ====================== 1. 风控模块 ======================
            pnlRisk = CreateBlockPanel(new Point(leftX, topMargin), new Size(panelWidth, leftPanelHeight), _accentBlue);

            AddHeader(pnlRisk, "风控守护 / RISK CONTROL", Color.White);

            // 【核心修改】创建大号 Toggle 按钮，放在右上角
            chkRisk = CreateToggleButton(pnlRisk, 340, 10, _accentBlue);

            AddInputRow(pnlRisk, "止盈阈值", out numStopProfit, "CNY", 80);
            AddInputRow(pnlRisk, "止损阈值", out numStopLoss, "CNY", 135);
            AddInputRow(pnlRisk, "流水上限", out numStopTurnover, "CNY", 190);

            // ====================== 2. 延迟模块 ======================
            pnlDelay = CreateBlockPanel(new Point(rightX, topMargin), new Size(panelWidth, rightPanelHeight), _accentYellow);

            AddHeader(pnlDelay, "随机延迟 / LATENCY", Color.White);
            chkDelay = CreateToggleButton(pnlDelay, 340, 10, _accentYellow);

            int delayY = 60;
            var lblRange = new Label { Text = "延迟范围:", ForeColor = Color.LightGray, AutoSize = true, Location = new Point(25, delayY + 5) };
            numDelayMin = CreateGhostNumeric(110, delayY, 80);
            var lblTo = new Label { Text = "—", ForeColor = Color.LightGray, AutoSize = true, Location = new Point(200, delayY + 5) };
            numDelayMax = CreateGhostNumeric(230, delayY, 80);
            var lblSec = new Label { Text = "秒 (Seconds)", ForeColor = Color.Gray, AutoSize = true, Location = new Point(320, delayY + 5) };

            pnlDelay.Controls.AddRange(new Control[] { lblRange, numDelayMin, lblTo, numDelayMax, lblSec });

            // ====================== 3. 计划任务 ======================
            pnlSchedule = CreateBlockPanel(new Point(rightX, topMargin + rightPanelHeight + 10), new Size(panelWidth, rightPanelHeight), _accentPurple);

            AddHeader(pnlSchedule, "计划任务 / SCHEDULE", Color.White);
            chkSchedule = CreateToggleButton(pnlSchedule, 340, 10, _accentPurple);

            int schedY = 60;
            var lblTime = new Label { Text = "运行时间:", ForeColor = Color.LightGray, AutoSize = true, Location = new Point(25, schedY + 5) };
            dtpStart = CreateGhostTimePicker(new Point(110, schedY));
            var lblTo2 = new Label { Text = "➜", ForeColor = Color.LightGray, AutoSize = true, Location = new Point(230, schedY + 5) };
            dtpEnd = CreateGhostTimePicker(new Point(260, schedY));

            pnlSchedule.Controls.AddRange(new Control[] { lblTime, dtpStart, lblTo2, dtpEnd });

            // ====================== 4. 底部按钮 ======================
            int splitBtnWidth = 225;
            int gap = 10;

            // 4.1 方案轮换按钮 (左下-左)
            btnSchemeRotation = CreateFlatButton("⚡ 方案轮换", new Point(leftX, btnY), new Size(splitBtnWidth, 45), Color.FromArgb(50, 50, 55), Color.White);
            // 绘制自定义边框颜色 (蓝色)
            btnSchemeRotation.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, btnSchemeRotation.ClientRectangle, _accentBlue, ButtonBorderStyle.Solid);
            btnSchemeRotation.Click += (s, e) => { using (var frm = new SchemeRotationForm()) frm.ShowDialog(); };

            // 4.2 【新增】全局倍率按钮 (左下-右)
            // 坐标 x = leftX + 按钮宽 + 间距
            btnMultiplyStrategy = CreateFlatButton("全局倍率", new Point(leftX + splitBtnWidth + gap, btnY), new Size(splitBtnWidth, 45), Color.FromArgb(50, 50, 55), Color.White);
            // 绘制自定义边框颜色 (使用黄色或紫色区分，这里用紫色)
            btnMultiplyStrategy.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, btnMultiplyStrategy.ClientRectangle, _accentPurple, ButtonBorderStyle.Solid);
            // 绑定点击事件，打开之前的 MultiplyStrategyForm
            btnMultiplyStrategy.Click += (s, e) =>
            {
                using (var frm = new MultiplyStrategyForm())
                {
                    frm.ShowDialog();
                }
            };

            // 4.3 保存按钮 (右下，保持不变)
            btnSave = CreateFlatButton("💾 保存全局配置", new Point(rightX, btnY), new Size(panelWidth, 45), _accentBlue, Color.White);

            // 将所有控件加入 Controls
            this.Controls.AddRange(new Control[] { pnlRisk, pnlDelay, pnlSchedule, btnSchemeRotation, btnMultiplyStrategy, btnSave });
            this.ResumeLayout(false);
        }

        // ====================== 组件工厂 ======================

        // 【新】大号 Toggle 按钮
        private CheckBox CreateToggleButton(Panel parent, int x, int y, Color activeColor)
        {
            var chk = new CheckBox
            {
                Appearance = Appearance.Button, // 关键：变成按钮样式
                Location = new Point(x, y),
                Size = new Size(100, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Text = "⚪ 未启用", // 默认文字
                AutoCheck = true
            };
            chk.FlatAppearance.BorderSize = 0;
            chk.BackColor = _btnOff; // 默认背景

            // 绑定点击变色逻辑
            chk.CheckedChanged += (s, e) =>
            {
                if (chk.Checked)
                {
                    chk.BackColor = activeColor;
                    chk.Text = "✅ 已启用";
                }
                else
                {
                    chk.BackColor = _btnOff;
                    chk.Text = "⚪ 未启用";
                }
            };

            parent.Controls.Add(chk);
            return chk;
        }

        private Panel CreateBlockPanel(Point loc, Size size, Color accent)
        {
            var p = new Panel { Location = loc, Size = size, BackColor = _blockBg, Padding = new Padding(0) };
            var bar = new Panel { Dock = DockStyle.Left, Width = 4, BackColor = accent };
            p.Controls.Add(bar);
            return p;
        }

        private void AddHeader(Panel parent, string text, Color color)
        {
            var lbl = new Label { Text = text, Location = new Point(15, 14), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = color };
            parent.Controls.Add(lbl);
        }

        private NumericUpDown CreateGhostNumeric(int x, int y, int width)
        {
            var num = new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(width, 26),
                BackColor = _inputBack,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 11F),
                TextAlign = HorizontalAlignment.Center
            };
            if (num.Controls.Count > 0) num.Controls[0].BackColor = _inputBack;
            return num;
        }

        private DateTimePicker CreateGhostTimePicker(Point loc)
        {
            return new DateTimePicker
            {
                Location = loc,
                Size = new Size(110, 26),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm:ss",
                ShowUpDown = true,
                Font = new Font("Consolas", 10F),
                BackColor = _inputBack,
                ForeColor = Color.White,
                CalendarMonthBackground = _blockBg
            };
        }

        private void AddInputRow(Panel parent, string label, out NumericUpDown num, string unit, int y)
        {
            var lbl = new Label { Text = label, ForeColor = Color.LightGray, AutoSize = true, Location = new Point(25, y + 3) };
            num = CreateGhostNumeric(130, y, 190);
            num.Maximum = 99999999; num.DecimalPlaces = 2;
            var line = new Panel { Size = new Size(190, 1), Location = new Point(130, y + 27), BackColor = Color.Gray };
            var unitLbl = new Label { Text = unit, ForeColor = Color.Gray, AutoSize = true, Location = new Point(330, y + 3) };
            parent.Controls.AddRange(new Control[] { lbl, num, unitLbl, line });
        }

        private Button CreateFlatButton(string text, Point loc, Size size, Color bg, Color fore)
        {
            var btn = new Button
            {
                Text = text,
                Location = loc,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = fore,
                Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // --- 业务逻辑 ---
        private void InitControlStyles()
        {
            numDelayMin.DecimalPlaces = 0; numDelayMax.DecimalPlaces = 0;
        }

        private void LoadSettings()
        {
            if (CacheData.Settings == null) return;
            var s = CacheData.Settings;

            // 设置状态
            chkDelay.Checked = s.EnableDelay;
            chkRisk.Checked = s.EnableRiskControl;
            chkSchedule.Checked = s.EnableSchedule;

            // 触发一次 UI 刷新，确保按钮颜色和文字正确
            RefreshToggleState(chkRisk, _accentBlue);
            RefreshToggleState(chkDelay, _accentYellow);
            RefreshToggleState(chkSchedule, _accentPurple);

            numDelayMin.Value = s.DelayMinSeconds; numDelayMax.Value = s.DelayMaxSeconds;
            numStopProfit.Value = s.StopProfitAmount; numStopLoss.Value = s.StopLossAmount; numStopTurnover.Value = s.StopTurnoverAmount;

            if (s.ScheduleStartTime != DateTime.MinValue) dtpStart.Value = s.ScheduleStartTime;
            if (s.ScheduleEndTime != DateTime.MinValue) dtpEnd.Value = s.ScheduleEndTime;

            UpdateEnabledStates();
        }

        // 辅助方法：手动刷新一次 Toggle 状态 (防止加载时没触发事件)
        private void RefreshToggleState(CheckBox chk, Color activeColor)
        {
            if (chk.Checked) { chk.BackColor = activeColor; chk.Text = "✅ 已启用"; }
            else { chk.BackColor = _btnOff; chk.Text = "⚪ 未启用"; }
        }

        private void BindEvents()
        {
            chkDelay.CheckedChanged += (s, e) => UpdateEnabledStates();
            chkRisk.CheckedChanged += (s, e) => UpdateEnabledStates();
            chkSchedule.CheckedChanged += (s, e) => UpdateEnabledStates();
            btnSave.Click += BtnSave_Click;
        }

        // 核心修改：修复“视力测试”问题
        private void UpdateEnabledStates()
        {
            SetEnabled(pnlRisk, chkRisk.Checked, chkRisk);
            SetEnabled(pnlDelay, chkDelay.Checked, chkDelay);
            SetEnabled(pnlSchedule, chkSchedule.Checked, chkSchedule);
        }

        private void SetEnabled(Control parent, bool enabled, Control exclude)
        {
            foreach (Control c in parent.Controls)
            {
                if (c == exclude) continue; // 不改变开关本身

                c.Enabled = enabled;

                // 核心：禁用时使用 _textDisabled (亮灰)，而不是默认的 Black
                if (enabled)
                {
                    if (c is NumericUpDown num) num.ForeColor = Color.White;
                    else if (c is DateTimePicker dtp) dtp.ForeColor = Color.White;
                    else c.ForeColor = Color.LightGray;
                }
                else
                {
                    // 禁用状态统一用亮灰色，保证看得到！
                    c.ForeColor = _textDisabled;
                    if (c is NumericUpDown num) num.ForeColor = _textDisabled;
                    if (c is DateTimePicker dtp) dtp.ForeColor = _textDisabled;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            var s = CacheData.Settings;
            if (chkDelay.Checked && numDelayMin.Value > numDelayMax.Value) { MessageBox.Show("延迟设置错误"); return; }
            s.EnableDelay = chkDelay.Checked; s.DelayMinSeconds = (int)numDelayMin.Value; s.DelayMaxSeconds = (int)numDelayMax.Value;
            s.EnableRiskControl = chkRisk.Checked; s.StopProfitAmount = numStopProfit.Value; s.StopLossAmount = numStopLoss.Value; s.StopTurnoverAmount = numStopTurnover.Value;
            s.EnableSchedule = chkSchedule.Checked; s.ScheduleStartTime = dtpStart.Value; s.ScheduleEndTime = dtpEnd.Value;
            try { JsonHelper.Save("Data\\Settings.json", s); MessageBox.Show("保存成功"); } catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}