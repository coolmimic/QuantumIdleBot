using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuantumIdleDesktop.Services;
using TL;

namespace QuantumIdleDesktop.Views
{
    public partial class ViewLogin : UserControl
    {
        private TelegramService _tgService;
        private TaskCompletionSource<string> _userInputTcs;   // 用来把用户输入传回 Service

        public event Action<User> OnLoginSuccess;

        public ViewLogin()
        {
            InitializeComponent();
        }

        public void StartLogin(TelegramService service)
        {
            _tgService = service;

            _tgService.OnLogMessage += Service_OnLogMessage;
            _tgService.OnLoginRequired += Service_OnLoginRequired;     // 需要用户输入
            _tgService.OnLoggedIn += Service_OnLoggedIn;               // 登录成功
            _tgService.OnStatusChanged += Service_OnStatusChanged;

            // 后台启动登录流程（不 await，让 UI 保持响应）
            Task.Run(() => _tgService.LoginAsync());
        }

        private void Service_OnLogMessage(string msg)
        {
            // 可选：显示日志
        }

        private void Service_OnStatusChanged(TelegramService.ConnectionStatus status)
        {
            this.UIThread(() =>
            {
                if (status == TelegramService.ConnectionStatus.Connecting)
                    SetStateLoading("正在连接服务器...");
            });
        }

        // ------------------------------------------------------------
        // 【核心修复】正确处理登录时需要用户输入（手机号/验证码/密码）
        // ------------------------------------------------------------
        private Task<string> Service_OnLoginRequired(string what)
        {
            // 防止上一次的 TCS 没被清理（理论上不会，但保险）
            _userInputTcs?.TrySetCanceled();

            _userInputTcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            // 必须在 UI 线程更新界面，使用同步 Invoke 防止更新还没完成就返回 Task
            this.UIThreadSync(() =>
            {
                txtInput.UseSystemPasswordChar = false; // 每次都先恢复明文
                UpdateUiForInput(what);
            });

            return _userInputTcs.Task;
        }

        private void Service_OnLoggedIn(User user)
        {
            this.UIThread(() =>
            {
                lblInstruction.Text = $"登录成功！欢迎 {user.username ?? user.first_name}";
                lblError.Text = "";
                txtInput.Visible = false;
                btnSubmit.Enabled = false;

                OnLoginSuccess?.Invoke(user);
            });
        }

        // ------------------------------------------------------------
        // 提交按钮
        // ------------------------------------------------------------
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (_userInputTcs == null || _userInputTcs.Task.IsCompleted)
                return;

            string val = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(val))
            {
                lblError.Text = "输入不能为空";
                return;
            }

            SetStateLoading("正在验证...");

            // 把用户输入传回给 TelegramService（继续 await OnLoginRequired）
            _userInputTcs.TrySetResult(val);
            _userInputTcs = null; // 清理，准备下一次可能的输入（比如先输入手机号 → 再输入验证码）
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSubmit_Click(sender, e);
                e.SuppressKeyPress = true;
            }
        }

        // ------------------------------------------------------------
        // UI 辅助方法
        // ------------------------------------------------------------
        private void UpdateUiForInput(string what)
        {
            txtInput.Visible = true;
            txtInput.Enabled = true;
            txtInput.Text = "";
            txtInput.Focus();
            btnSubmit.Enabled = true;
            lblError.Text = "";

            switch (what)
            {
                case "phone_number":
                    lblInstruction.Text = "请输入手机号（+86...）：";
                    btnSubmit.Text = "发送验证码";
                    break;
                case "verification_code":
                    lblInstruction.Text = "验证码已发送，请输入：";
                    btnSubmit.Text = "登录";
                    break;
                case "password":
                    lblInstruction.Text = "请输入两步验证密码：";
                    txtInput.UseSystemPasswordChar = true;
                    btnSubmit.Text = "验证密码";
                    break;
                default:
                    lblInstruction.Text = $"请输入 {what}：";
                    btnSubmit.Text = "提交";
                    break;
            }
        }

        private void SetStateLoading(string msg)
        {
            lblInstruction.Text = msg;
            txtInput.Enabled = false;
            btnSubmit.Enabled = false;
            btnSubmit.Text = "处理中...";
        }
    }

    // ------------------------------------------------------------
    // 扩展方法（建议放在项目公共位置）
    // ------------------------------------------------------------
    public static class ControlExtensions
    {
        // 异步版本（最常用）
        public static void UIThread(this Control control, Action action)
        {
            if (control.InvokeRequired)
                control.BeginInvoke(action);
            else
                action();
        }

        // 同步版本（需要阻塞调用线程时使用，比如在 Task 中等待 UI 更新完成）
        public static void UIThreadSync(this Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(action);
            else
                action();
        }
    }
}