using QuantumIdleDesktop.Models;   // 引用模型 (ApiResult)
using QuantumIdleDesktop.Services; // 引用服务
using QuantumIdleDesktop.Utils;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks; // 必须引用
using System.Windows.Forms;

namespace QuantumIdleDesktop.Forms
{
    public partial class FormLogin : Form
    {
        // 1. 实例化 API 服务
        private readonly AuthApiService _authService = new AuthApiService();

        // 用于保存登录后的用户信息 (建议放到全局静态类中，这里暂时定义在这)
        // public static UserLoginResponse CurrentUser { get; private set; }

        public FormLogin()
        {
            InitializeComponent();

            // ============== 在这里加入修正 Tab 顺序的代码 ==============

            // 1. 修正登录面板 (pnlLogin)
            txtLoginUser.TabIndex = 0;   // 光标第一步：账号
            txtLoginPass.TabIndex = 1;   // 光标第二步：密码
            btnLoginAction.TabIndex = 2; // 光标第三步：登录按钮

            // 2. 修正注册面板 (pnlRegister) - 顺手也修了
            txtRegUser.TabIndex = 0;
            txtRegPass.TabIndex = 1;
            btnRegAction.TabIndex = 2;

            // 3. 修正重置密码面板 (pnlReset)
            txtResetUser.TabIndex = 0;
            txtResetOldPass.TabIndex = 1;
            txtResetNewPass.TabIndex = 2;
            txtResetConfirm.TabIndex = 3;
            btnResetAction.TabIndex = 4;

            // 4. 修正激活面板 (pnlActivate)
            txtActUser.TabIndex = 0;
            txtActCard.TabIndex = 1;
            btnActAction.TabIndex = 2;
            // =======================================================

            // ============== 【新增】自动填充账号密码 ==============
            try
            {
                var (savedUser, savedPass) = RegistryHelper.GetLoginInfo();
                if (!string.IsNullOrEmpty(savedUser))
                {
                    txtLoginUser.Text = savedUser;
                    txtLoginPass.Text = savedPass;
                }
            }
            catch { /* 忽略错误 */ }
            // ===================================================


            SwitchMode("Login"); // 默认显示登录
        }

        // ================== 侧边栏切换逻辑 (保持不变) ==================
        private void btnSideLogin_Click(object sender, EventArgs e) => SwitchMode("Login");
        private void btnSideRegister_Click(object sender, EventArgs e) => SwitchMode("Register");
        private void btnSideReset_Click(object sender, EventArgs e) => SwitchMode("Reset");
        private void btnSideActivate_Click(object sender, EventArgs e) => SwitchMode("Activate");

        private void SwitchMode(string mode)
        {
            pnlLogin.Visible = false;
            pnlRegister.Visible = false;
            pnlReset.Visible = false;
            pnlActivate.Visible = false;

            ResetSideBtnColor(btnSideLogin);
            ResetSideBtnColor(btnSideRegister);
            ResetSideBtnColor(btnSideReset);
            ResetSideBtnColor(btnSideActivate);

            switch (mode)
            {
                case "Login":
                    pnlLogin.Visible = true;
                    HighlightSideBtn(btnSideLogin);
                    break;
                case "Register":
                    pnlRegister.Visible = true;
                    HighlightSideBtn(btnSideRegister);
                    break;
                case "Reset":
                    pnlReset.Visible = true;
                    HighlightSideBtn(btnSideReset);
                    break;
                case "Activate":
                    pnlActivate.Visible = true;
                    HighlightSideBtn(btnSideActivate);
                    break;
            }
        }

        private void HighlightSideBtn(Button btn)
        {
            btn.ForeColor = Color.Cyan;
            btn.BackColor = Color.FromArgb(10, 15, 25);
        }

        private void ResetSideBtnColor(Button btn)
        {
            btn.ForeColor = Color.Silver;
            btn.BackColor = Color.FromArgb(20, 30, 45);
        }

        // ================== 业务逻辑区域 (已集成 API) ==================

        // 1. 登录
        private async void btnLoginAction_Click(object sender, EventArgs e)
        {
            string user = txtLoginUser.Text.Trim();
            string pass = txtLoginPass.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MsgError("请输入账号和密码");
                return;
            }

            btnLoginAction.Enabled = false; // 禁用按钮防止连点
            btnLoginAction.Text = "登录中...";

            try
            {
                // 调用 API
                var result = await _authService.LoginAsync(user, pass);

                if (result.Success)
                {
                    if (result.Data.IsActive == 0)
                    {
                        MessageBox.Show("账户未激活");
                    }
                    else
                    {
                        RegistryHelper.SaveLoginInfo(user, pass);
                        CacheData.SoftwareUser = result.Data;
                        FrmMain frmMain = new FrmMain();
                        frmMain.Show();
                        this.Hide();
                    }
                }
                else
                {
                    MsgError($"登录失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                MsgError("系统错误: " + ex.Message);
            }
            finally
            {
                btnLoginAction.Enabled = true;
                btnLoginAction.Text = "立即登录";
            }
        }

        // 2. 注册
        private async void btnRegAction_Click(object sender, EventArgs e)
        {
            string user = txtRegUser.Text.Trim();
            string pass = txtRegPass.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MsgError("账号或密码不能为空");
                return;
            }

            btnRegAction.Enabled = false;
            btnRegAction.Text = "注册中...";

            var result = await _authService.RegisterAsync(user, pass);

            btnRegAction.Enabled = true;
            btnRegAction.Text = "注册账户";

            if (result.Success)
            {
                MsgSuccess($"注册成功！请登录。");
                // 自动填入登录框并切换
                txtLoginUser.Text = user;
                txtLoginPass.Text = pass; // 可选：自动填密码
                SwitchMode("Login");
            }
            else
            {
                MsgError($"注册失败: {result.Message}");
            }
        }

        // 3. 重置密码
        private async void btnResetAction_Click(object sender, EventArgs e)
        {
            string user = txtResetUser.Text.Trim();
            string oldPass = txtResetOldPass.Text.Trim();
            string newPass = txtResetNewPass.Text.Trim();
            string confirm = txtResetConfirm.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(oldPass) || string.IsNullOrEmpty(newPass))
            {
                MsgError("请填写完整信息");
                return;
            }

            if (newPass != confirm)
            {
                MsgError("两次新密码输入不一致");
                return;
            }

            btnResetAction.Enabled = false;
            var result = await _authService.ResetPasswordAsync(user, oldPass, newPass);
            btnResetAction.Enabled = true;

            if (result.Success)
            {
                MsgSuccess("密码重置成功，请使用新密码登录");
                SwitchMode("Login");
            }
            else
            {
                MsgError($"重置失败: {result.Message}");
            }
        }

        // 4. 激活
        private async void btnActAction_Click(object sender, EventArgs e)
        {
            string user = txtActUser.Text.Trim();
            string card = txtActCard.Text.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(card))
            {
                MsgError("请输入账号和卡密");
                return;
            }

            btnActAction.Enabled = false;
            var result = await _authService.ActivateAsync(user, card);
            btnActAction.Enabled = true;

            if (result.Success)
            {
                // 如果 API 返回了 NewExpireTime，可以显示出来
                //string expireMsg = result.ExpireTime.HasValue
                //    ? $"有效期已延长至: {result.ExpireTime.Value:yyyy-MM-dd}"
                //    : "";

                //MsgSuccess($"激活成功！{expireMsg}");

                // 如果已经激活，可以选择跳转登录
                txtLoginUser.Text = user;
                SwitchMode("Login");
            }
            else
            {
                MsgError($"激活失败: {result.Message}");
            }
        }

        // ================== 窗体通用逻辑 ==================

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MsgError(string msg) => MessageBox.Show(msg, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        private void MsgSuccess(string msg) => MessageBox.Show(msg, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);

        // --- 拖动窗体代码 ---
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        private void panelTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }
    }
}