using QuantumIdleDesktop.Forms;
using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Services;
using QuantumIdleDesktop.Views;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using TL;

namespace QuantumIdleDesktop
{
    public partial class FrmMain : Form
    {
        private UserControl _currentView;
        private readonly GameContextService _gameService = GameContextService.Instance;

        // 配色（高级深灰蓝）
        private readonly Color Bg = Color.FromArgb(30, 35, 45);
        private readonly Color NavBg = Color.FromArgb(38, 45, 58);
        private readonly Color CardBg = Color.FromArgb(45, 52, 68);
        private readonly Color BtnNormal = Color.FromArgb(65, 75, 95);
        private readonly Color BtnHover = Color.FromArgb(85, 100, 130);
        private readonly Color BtnActive = Color.FromArgb(0, 115, 255);
        private readonly Color Success = Color.FromArgb(16, 185, 129);
        private readonly Color Danger = Color.FromArgb(239, 80, 80);
        private readonly Color Text = Color.FromArgb(230, 235, 245);
        private readonly Color TextMuted = Color.FromArgb(140, 150, 180);

        public FrmMain()
        {
            InitializeComponent();
            this.FormClosing += FrmMain_FormClosing;
            this.Load += FrmMain_Load;
            this.FormClosing += (s, e) => CacheData.tgService?.Dispose();
        }

        private void FrmMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private async void FrmMain_Load(object sender, EventArgs e)
        {
            ApplyAbsoluteLayout();
            InitLogic();
            await CacheData.OnLoad();

            UpdateSelectedButton(btnScheme);
            SwitchView("Schemes");
            AppendLog("QuantumIdle 已启动");


            base.Text = $"量子挂机 v{AppGlobal.localVer}  重塑彩票公正性 3D 排列5  哈希可线上线下查询   公开公正 千万无忧 12年信誉全网可验证 飞机aobai11000";
        }

        private void ApplyAbsoluteLayout()
        {
            this.ClientSize = new Size(1000, 600);
            this.FormBorderStyle = FormBorderStyle.Sizable;   // 加这行
            this.MaximizeBox = true;
            this.BackColor = Bg;
            this.StartPosition = FormStartPosition.CenterScreen;

            // 顶部导航栏
            panelTopNav.Dock = DockStyle.Top;
            panelTopNav.Height = 56;
            panelTopNav.BackColor = NavBg;

            int x = 15;
            int y = 10;

            SetNavBtn(btnTgLogin, "TG登录", x, y); x += 100;
            SetNavBtn(btnScheme, "方案", x, y); x += 94;
            SetNavBtn(btnSettings, "设置", x, y); x += 94;
            SetNavBtn(btnOrderLog, "注单", x, y); x += 94;
            SetNavBtn(btnOdds, "赔率", x, y); x += 94;
            SetNavBtn(btnHistory, "开奖历史", x, y); x += 94;
            // 模拟模式 CheckBox
            chkSimulation.Location = new Point(x + 25, y + 8);
            chkSimulation.AutoSize = true;
            chkSimulation.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
            chkSimulation.Cursor = Cursors.Hand;
            UpdateSimulationBtnState();

            // 开始按钮（右对齐）
            btnStartAll.Size = new Size(118, 36);
            btnStartAll.Location = new Point(1000 - 118 - 20, y + 2);
            btnStartAll.BackColor = Success;
            btnStartAll.ForeColor = Color.White;
            btnStartAll.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            btnStartAll.FlatStyle = FlatStyle.Flat;
            btnStartAll.FlatAppearance.BorderSize = 0;
            btnStartAll.Text = "Start 开始挂机";
            btnStartAll.Cursor = Cursors.Hand;

            // 主内容区
            splitContainer1.SplitterDistance = 370;
            splitContainer1.Panel1.BackColor = CardBg;
            splitContainer1.Panel2.BackColor = Color.FromArgb(20, 22, 28);
            splitContainer1.SplitterWidth = 4;
            splitContainer1.Panel1.Padding = new Padding(15);
            splitContainer1.Panel2.Padding = new Padding(12, 8, 12, 12);

            panelMainContent.BackColor = CardBg;

            // 日志区
            richTextBoxLog.BackColor = Color.FromArgb(20, 22, 28);
            richTextBoxLog.ForeColor = Color.FromArgb(100, 255, 150);
            richTextBoxLog.Font = new Font("Cascadia Mono", 10F);
            richTextBoxLog.BorderStyle = BorderStyle.None;
            richTextBoxLog.ReadOnly = true;
            richTextBoxLog.Padding = new Padding(12);
            richTextBoxLog.WordWrap = false;

            // 底部状态栏（绝对定位，强制一行）
            panelBottomBar.Dock = DockStyle.Bottom;
            panelBottomBar.Height = 44;
            panelBottomBar.BackColor = NavBg;

            // 2. 替换底部状态栏调用代码（统一间距 80px，超级紧凑）
            int sy = 12;
            int startX = 20;

            // 每项之间固定 80px 间距，数值自动紧贴冒号
            CreateStatus("余额: ", statusLabelBalance, startX, sy, Color.FromArgb(46, 204, 113)); startX += 130;
            CreateStatus("盈亏: ", statusLabelProfit, startX, sy, Color.FromArgb(241, 196, 15)); startX += 130;
            CreateStatus("流水: ", statusLabelTurnover, startX, sy, Color.FromArgb(149, 165, 166)); startX += 130;
            CreateStatus("模拟盈亏: ", statusLabelSimProfit, startX, sy, Color.FromArgb(231, 76, 60)); startX += 140;
            CreateStatus("模拟流水: ", statusLabelSimTurnover, startX, sy, Color.FromArgb(155, 89, 182));

            // 有效期靠右（同样紧贴）
            var lblExpireTitle = new Label
            {
                Text = "有效期: ",
                ForeColor = TextMuted,
                Font = new Font("Microsoft YaHei UI", 9F),
                AutoSize = true,
                Location = new Point(1000 - 220, sy + 2)
            };
            statusLabelExpireTime.Location = new Point(1000 - 220 + MeasureTextWidth("有效期: "), sy);
            statusLabelExpireTime.ForeColor = TextMuted;
            statusLabelExpireTime.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);

            panelBottomBar.Controls.Add(lblExpireTitle);
            panelBottomBar.Controls.Add(statusLabelExpireTime);

            timerState.Interval = 1000;
            timerState.Start();
        }

        private void SetNavBtn(Button btn, string text, int x, int y)
        {
            btn.Location = new Point(x, y);
            btn.Size = new Size(90, 36);
            btn.Text = text;                          
            btn.BackColor = BtnNormal;
            btn.ForeColor = Text;
            btn.Font = new Font("Microsoft YaHei UI", 9F);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = BtnHover;
            btn.TextAlign = ContentAlignment.MiddleCenter;  
            btn.Cursor = Cursors.Hand;
        }



        private void CreateStatus(string title, Label valueLabel, int x, int y, Color valueColor)
        {
            // 标题（包含冒号）
            var lblTitle = new Label
            {
                Text = title,
                ForeColor = TextMuted,
                Font = new Font("Microsoft YaHei UI", 9F),
                AutoSize = true,
                Location = new Point(x, y + 2)
            };

            // 数值紧贴在冒号后面（核心！）
            valueLabel.ForeColor = valueColor;
            valueLabel.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            valueLabel.AutoSize = true;
            valueLabel.Location = new Point(x + MeasureTextWidth(title) + 3, y);  // 关键：动态计算标题宽度
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;

            panelBottomBar.Controls.Add(lblTitle);
            panelBottomBar.Controls.Add(valueLabel);
        }
        private int MeasureTextWidth(string text)
        {
            using var g = panelBottomBar.CreateGraphics();
            return (int)Math.Ceiling(g.MeasureString(text, new Font("Microsoft YaHei UI", 9F)).Width);
        }
        private void UpdateSelectedButton(Button btn)
        {
            foreach (var b in new[] { btnTgLogin, btnScheme, btnSettings, btnOrderLog, btnOdds })
            {
                b.BackColor = BtnNormal;
                b.ForeColor = Text;
            }
            btn.BackColor = BtnActive;
            btn.ForeColor = Color.White;
        }
        private void UpdateSimulationBtnState()
        {
            chkSimulation.Checked = AppGlobal.IsSimulation;
            chkSimulation.Text = AppGlobal.IsSimulation ? "模拟模式" : "真实模式";
            chkSimulation.ForeColor = AppGlobal.IsSimulation ? Color.FromArgb(255, 90, 90) : Color.FromArgb(100, 220, 100);
        }
        private void InitLogic()
        {
            _gameService.OnLog += AppendLog;
            BotGuardService.LogTriggered += AppendLog;
            BettingService.Instance.OnLog += RefreshOrderIfVisible;
            SettlementService.Instance.OnLog += RefreshOrderIfVisible;
            SettlementService.Instance.OnIsRunning += Instance_OnIsRunning;


            btnTgLogin.Click += async (s, e) => await PerformTgLoginAsync();
            //btnScheme.Click += (s, e) => { UpdateSelectedButton(btnScheme); SwitchView("TGLogin"); };
            btnStartAll.Click += BtnStartAll_Click;
            btnScheme.Click += (s, e) => { UpdateSelectedButton(btnScheme); SwitchView("Schemes"); };
            btnSettings.Click += (s, e) => { UpdateSelectedButton(btnSettings); SwitchView("Settings"); };
            btnOrderLog.Click += (s, e) => { UpdateSelectedButton(btnOrderLog); SwitchView("OrderLog"); };
            btnOdds.Click += (s, e) => { UpdateSelectedButton(btnOdds); SwitchView("Odds"); };
            btnHistory.Click += (s, e) => { UpdateSelectedButton(btnOdds); SwitchView("History"); };
            chkSimulation.CheckedChanged += (s, e) =>
            {
                AppGlobal.IsSimulation = chkSimulation.Checked;
                UpdateSimulationBtnState();
            };

            timerState.Tick += (s, e) => UpdateExpireDisplay();
        }
        private void Instance_OnIsRunning(bool isRunning)
        {
            // ==================================================
            // 1. 线程安全检查 (Thread Safety)
            // ==================================================
            // 如果当前线程不是创建控件的 UI 线程，则使用 Invoke 将操作封送回 UI 线程
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(Instance_OnIsRunning), isRunning);
                return;
            }

            // ==================================================
            // 2. 更新全局状态
            // ==================================================
            AppGlobal.IsRunning = isRunning;

            // ==================================================
            // 3. 根据状态更新 UI (修正了原本的逻辑倒置)
            // ==================================================
            if (isRunning)
            {
                // 状态变成了【运行中】 -> 按钮应该变成【停止】
                btnStartAll.Text = "Stop 停止运行";
                btnStartAll.BackColor = Danger; // 红色
                AppendLog($">>> 开始挂机 ({(AppGlobal.IsSimulation ? "模拟" : "实盘")})");
            }
            else
            {
                // 状态变成了【已停止】 -> 按钮应该变成【开始】
                btnStartAll.Text = "Start 开始挂机";
                btnStartAll.BackColor = Success; // 绿色/蓝色
                AppendLog(">>> 挂机已停止");
            }
        }
        private void SwitchView(string view)
        {
            // 如果是同一个视图，直接返回（避免重复创建）
            if (_currentView?.Tag as string == view)
                return;

            panelMainContent.Controls.Clear();

            UserControl newView = view switch
            {
                "History" => new ViewLotteryHistory(),
                "Schemes" => new ViewSchemes(),
                "Settings" => new ViewSetting(),
                "OrderLog" => new ViewOrderList(),
                "Odds" => new ViewOddsSetting(),
                _ => null
            };

            if (newView != null)
            {
                newView.Dock = DockStyle.Fill;
                newView.Tag = view; // 标记是什么视图，方便判断
                panelMainContent.Controls.Add(newView);

                // 【关键】替换当前视图引用
                _currentView = newView;

               
            }
        }
        public void RefreshOrderIfVisible(string msg) 
        {
            AppendLog(msg);

            if ( _currentView is ViewOrderList orderListView)
            {
                orderListView.RefreshData();
            }
        }
        private void BtnStartAll_Click(object sender, EventArgs e)
        {
            if (AppGlobal.IsRunning)
            {
                AppGlobal.IsRunning = false;
                btnStartAll.Text = "Start 开始挂机";
                btnStartAll.BackColor = Success;
                AppendLog(">>> 挂机已停止");
            }
            else
            {
                AppGlobal.IsRunning = true;
                btnStartAll.Text = "Stop 停止运行";
                btnStartAll.BackColor = Danger;
                AppendLog($">>> 开始挂机 ({(AppGlobal.IsSimulation ? "模拟" : "实盘")})");
            }
        }
        private void AppendLog(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return;
            void Add() { richTextBoxLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n"); richTextBoxLog.ScrollToCaret(); }
            if (richTextBoxLog.InvokeRequired) richTextBoxLog.BeginInvoke(Add); else Add();
        }
        private async Task PerformTgLoginAsync()
        {
            btnTgLogin.Enabled = false;
            try
            {
                string phone = await ShowInputDialog("请输入手机号", "+86");
                if (string.IsNullOrWhiteSpace(phone)) return;
         
                if (CacheData.tgService == null)
                {
                    CacheData.tgService = new TelegramService(phone);
                    CacheData.tgService.OnLoginRequired += async what => await ShowInputDialog(what == "verification_code" ? "请输入验证码" : "请输入密码");
                    CacheData.tgService.onNewMessage += _tgService_onNewMessage;
                    CacheData.tgService.OnLogMessage += AppendLog;
                }
           
                AppendLog($"正在登录 {phone}...");
                await CacheData.tgService.LoginAsync();
            }
            catch (Exception ex) { AppendLog($"登录失败: {ex.Message}"); }
            finally { btnTgLogin.Enabled = true; }
        }
        private Task<string> ShowInputDialog(string title, string def = "")
        {
            var tcs = new TaskCompletionSource<string>();
            this.Invoke((MethodInvoker)(() =>
            {
                using var f = new FormInput(title, def);
                tcs.SetResult(f.ShowDialog() == DialogResult.OK ? f.InputValue : null);
            }));
            return tcs.Task;
        }
        private void _tgService_onNewMessage(TL.Update obj)
        {
            if (!AppGlobal.IsRunning) return;
            TL.Message msg = null;
            if (obj is TL.UpdateNewMessage unm) msg = unm.message as TL.Message;
            else if (obj is TL.UpdateNewChannelMessage uncm) msg = uncm.message as TL.Message;
            if (msg?.message != null) _gameService.ProcessGroupMessage(msg, msg.message);
        }
        private void UpdateExpireDisplay()
        {
            if (CacheData.SoftwareUser == null) return;
            var remain = CacheData.SoftwareUser.ExpireTime - DateTime.Now;
            this.Invoke((MethodInvoker)(() =>
            {
                statusLabelExpireTime.Text = remain.TotalSeconds <= 0
                    ? "已过期" : $"{remain.Days}.{remain.Hours}.{remain.Minutes}.{remain.Seconds}s";


                statusLabelExpireTime.ForeColor = remain.TotalDays < 3 ? Danger : TextMuted;

                statusLabelBalance.Text = $"{AppGlobal.Balance:N2}";
                statusLabelProfit.Text = AppGlobal.Profit >= 0 ? $"+{AppGlobal.Profit:N2}" : $"{AppGlobal.Profit:N2}";
                statusLabelTurnover.Text = $"{AppGlobal.Turnover:N2}";
                statusLabelSimProfit.Text = $"{AppGlobal.SimProfit:N2}";
                statusLabelSimTurnover.Text = $"{AppGlobal.SimTurnover:N2}";
            }));
        }
    }
}