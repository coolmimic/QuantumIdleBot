using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TL;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace QuantumIdleDesktop.Forms
{
    public partial class FormInput : Form
    {
        public string InputValue { get; private set; }


        public FormInput(string what , string defaultValue = "")
        {
            InitializeComponent();           
            ConfigureFor(what);
            this.txtInput.Text = defaultValue;
            this.StartPosition = FormStartPosition.CenterScreen;
        }


        private void ConfigureFor(string what)
        {
            txtInput.UseSystemPasswordChar = false;

            switch (what)
            {
                case "phone_number":
                    lblTitle.Text = "需要手机号";
                    lblPrompt.Text = "请输入手机号 (格式: +86...)";
                    this.Text = "登录 - 步骤 1/3";
                    break;
                case "verification_code":
                    lblTitle.Text = "需要验证码";
                    lblPrompt.Text = "验证码已发送，请输入:";
                    this.Text = "登录 - 步骤 2/3";
                    break;
                case "password":
                    lblTitle.Text = "两步验证";
                    lblPrompt.Text = "账号开启了2FA，请输入密码:";
                    txtInput.UseSystemPasswordChar = true; // 密码遮罩
                    this.Text = "登录 - 步骤 3/3";
                    break;
                default:
                    lblTitle.Text = "输入信息";
                    lblPrompt.Text = $"请输入 {what}:";
                    break;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                MessageBox.Show("输入不能为空！");
                return;
            }
            InputValue = txtInput.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }



        private void DragForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
        // Win32 API 用于无边框窗体拖动（只在标题栏和背景可拖动）
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

    }
}
