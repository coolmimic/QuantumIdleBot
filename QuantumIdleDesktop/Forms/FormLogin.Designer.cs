namespace QuantumIdleDesktop.Forms
{
    partial class FormLogin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLogin));
            panelSideMenu = new Panel();
            btnSideActivate = new Button();
            btnSideReset = new Button();
            btnSideRegister = new Button();
            btnSideLogin = new Button();
            panelLogo = new Panel();
            lblLogo = new Label();
            panelTop = new Panel();
            btnClose = new Button();
            lblTitle = new Label();
            panelContainer = new Panel();
            pnlLogin = new Panel();
            btnLoginAction = new Button();
            txtLoginPass = new TextBox();
            lblLoginPass = new Label();
            txtLoginUser = new TextBox();
            lblLoginUser = new Label();
            lblHeaderLogin = new Label();
            pnlRegister = new Panel();
            btnRegAction = new Button();
            txtRegPass = new TextBox();
            lblRegPass = new Label();
            txtRegUser = new TextBox();
            lblRegUser = new Label();
            lblHeaderReg = new Label();
            pnlReset = new Panel();
            txtResetConfirm = new TextBox();
            lblResetConfirm = new Label();
            btnResetAction = new Button();
            txtResetNewPass = new TextBox();
            lblResetNewPass = new Label();
            txtResetOldPass = new TextBox();
            lblResetOldPass = new Label();
            txtResetUser = new TextBox();
            lblResetUser = new Label();
            lblHeaderReset = new Label();
            pnlActivate = new Panel();
            btnActAction = new Button();
            txtActCard = new TextBox();
            lblActCard = new Label();
            txtActUser = new TextBox();
            lblActUser = new Label();
            lblHeaderAct = new Label();
            panelSideMenu.SuspendLayout();
            panelLogo.SuspendLayout();
            panelTop.SuspendLayout();
            panelContainer.SuspendLayout();
            pnlLogin.SuspendLayout();
            pnlRegister.SuspendLayout();
            pnlReset.SuspendLayout();
            pnlActivate.SuspendLayout();
            SuspendLayout();
            // 
            // panelSideMenu
            // 
            panelSideMenu.BackColor = Color.FromArgb(20, 30, 45);
            panelSideMenu.Controls.Add(btnSideActivate);
            panelSideMenu.Controls.Add(btnSideReset);
            panelSideMenu.Controls.Add(btnSideRegister);
            panelSideMenu.Controls.Add(btnSideLogin);
            panelSideMenu.Controls.Add(panelLogo);
            panelSideMenu.Dock = DockStyle.Left;
            panelSideMenu.Location = new Point(0, 30);
            panelSideMenu.Name = "panelSideMenu";
            panelSideMenu.Size = new Size(160, 420);
            panelSideMenu.TabIndex = 0;
            // 
            // btnSideActivate
            // 
            btnSideActivate.Dock = DockStyle.Top;
            btnSideActivate.FlatAppearance.BorderSize = 0;
            btnSideActivate.FlatStyle = FlatStyle.Flat;
            btnSideActivate.Font = new Font("Segoe UI", 10F);
            btnSideActivate.ForeColor = Color.Silver;
            btnSideActivate.Location = new Point(0, 210);
            btnSideActivate.Name = "btnSideActivate";
            btnSideActivate.Size = new Size(160, 50);
            btnSideActivate.TabIndex = 4;
            btnSideActivate.Text = "激活账户";
            btnSideActivate.UseVisualStyleBackColor = true;
            btnSideActivate.Click += btnSideActivate_Click;
            // 
            // btnSideReset
            // 
            btnSideReset.Dock = DockStyle.Top;
            btnSideReset.FlatAppearance.BorderSize = 0;
            btnSideReset.FlatStyle = FlatStyle.Flat;
            btnSideReset.Font = new Font("Segoe UI", 10F);
            btnSideReset.ForeColor = Color.Silver;
            btnSideReset.Location = new Point(0, 160);
            btnSideReset.Name = "btnSideReset";
            btnSideReset.Size = new Size(160, 50);
            btnSideReset.TabIndex = 3;
            btnSideReset.Text = "重置密码";
            btnSideReset.UseVisualStyleBackColor = true;
            btnSideReset.Click += btnSideReset_Click;
            // 
            // btnSideRegister
            // 
            btnSideRegister.Dock = DockStyle.Top;
            btnSideRegister.FlatAppearance.BorderSize = 0;
            btnSideRegister.FlatStyle = FlatStyle.Flat;
            btnSideRegister.Font = new Font("Segoe UI", 10F);
            btnSideRegister.ForeColor = Color.Silver;
            btnSideRegister.Location = new Point(0, 110);
            btnSideRegister.Name = "btnSideRegister";
            btnSideRegister.Size = new Size(160, 50);
            btnSideRegister.TabIndex = 2;
            btnSideRegister.Text = "注册";
            btnSideRegister.UseVisualStyleBackColor = true;
            btnSideRegister.Click += btnSideRegister_Click;
            // 
            // btnSideLogin
            // 
            btnSideLogin.Dock = DockStyle.Top;
            btnSideLogin.FlatAppearance.BorderSize = 0;
            btnSideLogin.FlatStyle = FlatStyle.Flat;
            btnSideLogin.Font = new Font("Segoe UI", 10F);
            btnSideLogin.ForeColor = Color.Cyan;
            btnSideLogin.Location = new Point(0, 60);
            btnSideLogin.Name = "btnSideLogin";
            btnSideLogin.Size = new Size(160, 50);
            btnSideLogin.TabIndex = 1;
            btnSideLogin.Text = "登录";
            btnSideLogin.UseVisualStyleBackColor = true;
            btnSideLogin.Click += btnSideLogin_Click;
            // 
            // panelLogo
            // 
            panelLogo.Controls.Add(lblLogo);
            panelLogo.Dock = DockStyle.Top;
            panelLogo.Location = new Point(0, 0);
            panelLogo.Name = "panelLogo";
            panelLogo.Size = new Size(160, 60);
            panelLogo.TabIndex = 0;
            // 
            // lblLogo
            // 
            lblLogo.Dock = DockStyle.Fill;
            lblLogo.Font = new Font("Impact", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblLogo.ForeColor = Color.FromArgb(40, 167, 69);
            lblLogo.Location = new Point(0, 0);
            lblLogo.Name = "lblLogo";
            lblLogo.Size = new Size(160, 60);
            lblLogo.TabIndex = 0;
            lblLogo.Text = "QUANTUM";
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(20, 30, 45);
            panelTop.Controls.Add(btnClose);
            panelTop.Controls.Add(lblTitle);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(700, 30);
            panelTop.TabIndex = 1;
            panelTop.MouseDown += panelTop_MouseDown;
            // 
            // btnClose
            // 
            btnClose.Dock = DockStyle.Right;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.ForeColor = Color.IndianRed;
            btnClose.Location = new Point(655, 0);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(45, 30);
            btnClose.TabIndex = 1;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.ForeColor = Color.Gray;
            lblTitle.Location = new Point(12, 8);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(141, 17);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Quantum Security Gate";
            // 
            // panelContainer
            // 
            panelContainer.Controls.Add(pnlLogin);
            panelContainer.Controls.Add(pnlRegister);
            panelContainer.Controls.Add(pnlReset);
            panelContainer.Controls.Add(pnlActivate);
            panelContainer.Dock = DockStyle.Fill;
            panelContainer.Location = new Point(160, 30);
            panelContainer.Name = "panelContainer";
            panelContainer.Size = new Size(540, 420);
            panelContainer.TabIndex = 2;
            // 
            // pnlLogin
            // 
            pnlLogin.BackColor = Color.FromArgb(10, 15, 25);
            pnlLogin.Controls.Add(btnLoginAction);
            pnlLogin.Controls.Add(txtLoginPass);
            pnlLogin.Controls.Add(lblLoginPass);
            pnlLogin.Controls.Add(txtLoginUser);
            pnlLogin.Controls.Add(lblLoginUser);
            pnlLogin.Controls.Add(lblHeaderLogin);
            pnlLogin.Dock = DockStyle.Fill;
            pnlLogin.Location = new Point(0, 0);
            pnlLogin.Name = "pnlLogin";
            pnlLogin.Size = new Size(540, 420);
            pnlLogin.TabIndex = 10;
            // 
            // btnLoginAction
            // 
            btnLoginAction.BackColor = Color.FromArgb(40, 167, 69);
            btnLoginAction.FlatAppearance.BorderSize = 0;
            btnLoginAction.FlatStyle = FlatStyle.Flat;
            btnLoginAction.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnLoginAction.ForeColor = Color.White;
            btnLoginAction.Location = new Point(46, 250);
            btnLoginAction.Name = "btnLoginAction";
            btnLoginAction.Size = new Size(300, 40);
            btnLoginAction.TabIndex = 0;
            btnLoginAction.Text = "立即登录";
            btnLoginAction.UseVisualStyleBackColor = false;
            btnLoginAction.Click += btnLoginAction_Click;
            // 
            // txtLoginPass
            // 
            txtLoginPass.BackColor = Color.FromArgb(30, 40, 50);
            txtLoginPass.BorderStyle = BorderStyle.FixedSingle;
            txtLoginPass.Font = new Font("Segoe UI", 11F);
            txtLoginPass.ForeColor = Color.White;
            txtLoginPass.Location = new Point(46, 183);
            txtLoginPass.Name = "txtLoginPass";
            txtLoginPass.PasswordChar = '●';
            txtLoginPass.Size = new Size(300, 27);
            txtLoginPass.TabIndex = 1;
            // 
            // lblLoginPass
            // 
            lblLoginPass.AutoSize = true;
            lblLoginPass.ForeColor = Color.Silver;
            lblLoginPass.Location = new Point(43, 160);
            lblLoginPass.Name = "lblLoginPass";
            lblLoginPass.Size = new Size(109, 17);
            lblLoginPass.TabIndex = 2;
            lblLoginPass.Text = "密码 / Password";
            // 
            // txtLoginUser
            // 
            txtLoginUser.BackColor = Color.FromArgb(30, 40, 50);
            txtLoginUser.BorderStyle = BorderStyle.FixedSingle;
            txtLoginUser.Font = new Font("Segoe UI", 11F);
            txtLoginUser.ForeColor = Color.White;
            txtLoginUser.Location = new Point(46, 113);
            txtLoginUser.Name = "txtLoginUser";
            txtLoginUser.Size = new Size(300, 27);
            txtLoginUser.TabIndex = 3;
            // 
            // lblLoginUser
            // 
            lblLoginUser.AutoSize = true;
            lblLoginUser.ForeColor = Color.Silver;
            lblLoginUser.Location = new Point(43, 90);
            lblLoginUser.Name = "lblLoginUser";
            lblLoginUser.Size = new Size(99, 17);
            lblLoginUser.TabIndex = 4;
            lblLoginUser.Text = "账号 / Account";
            // 
            // lblHeaderLogin
            // 
            lblHeaderLogin.AutoSize = true;
            lblHeaderLogin.Font = new Font("Segoe UI", 18F);
            lblHeaderLogin.ForeColor = Color.Cyan;
            lblHeaderLogin.Location = new Point(40, 30);
            lblHeaderLogin.Name = "lblHeaderLogin";
            lblHeaderLogin.Size = new Size(114, 32);
            lblHeaderLogin.TabIndex = 5;
            lblHeaderLogin.Text = "用户登录";
            // 
            // pnlRegister
            // 
            pnlRegister.BackColor = Color.FromArgb(10, 15, 25);
            pnlRegister.Controls.Add(btnRegAction);
            pnlRegister.Controls.Add(txtRegPass);
            pnlRegister.Controls.Add(lblRegPass);
            pnlRegister.Controls.Add(txtRegUser);
            pnlRegister.Controls.Add(lblRegUser);
            pnlRegister.Controls.Add(lblHeaderReg);
            pnlRegister.Dock = DockStyle.Fill;
            pnlRegister.Location = new Point(0, 0);
            pnlRegister.Name = "pnlRegister";
            pnlRegister.Size = new Size(540, 420);
            pnlRegister.TabIndex = 11;
            pnlRegister.Visible = false;
            // 
            // btnRegAction
            // 
            btnRegAction.BackColor = Color.FromArgb(40, 167, 69);
            btnRegAction.FlatAppearance.BorderSize = 0;
            btnRegAction.FlatStyle = FlatStyle.Flat;
            btnRegAction.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnRegAction.ForeColor = Color.White;
            btnRegAction.Location = new Point(46, 250);
            btnRegAction.Name = "btnRegAction";
            btnRegAction.Size = new Size(300, 40);
            btnRegAction.TabIndex = 0;
            btnRegAction.Text = "注册账户";
            btnRegAction.UseVisualStyleBackColor = false;
            btnRegAction.Click += btnRegAction_Click;
            // 
            // txtRegPass
            // 
            txtRegPass.BackColor = Color.FromArgb(30, 40, 50);
            txtRegPass.BorderStyle = BorderStyle.FixedSingle;
            txtRegPass.Font = new Font("Segoe UI", 11F);
            txtRegPass.ForeColor = Color.White;
            txtRegPass.Location = new Point(46, 183);
            txtRegPass.Name = "txtRegPass";
            txtRegPass.PasswordChar = '●';
            txtRegPass.Size = new Size(300, 27);
            txtRegPass.TabIndex = 1;
            // 
            // lblRegPass
            // 
            lblRegPass.AutoSize = true;
            lblRegPass.ForeColor = Color.Silver;
            lblRegPass.Location = new Point(43, 160);
            lblRegPass.Name = "lblRegPass";
            lblRegPass.Size = new Size(141, 17);
            lblRegPass.TabIndex = 2;
            lblRegPass.Text = "设置密码 / Password";
            // 
            // txtRegUser
            // 
            txtRegUser.BackColor = Color.FromArgb(30, 40, 50);
            txtRegUser.BorderStyle = BorderStyle.FixedSingle;
            txtRegUser.Font = new Font("Segoe UI", 11F);
            txtRegUser.ForeColor = Color.White;
            txtRegUser.Location = new Point(46, 113);
            txtRegUser.Name = "txtRegUser";
            txtRegUser.Size = new Size(300, 27);
            txtRegUser.TabIndex = 3;
            // 
            // lblRegUser
            // 
            lblRegUser.AutoSize = true;
            lblRegUser.ForeColor = Color.Silver;
            lblRegUser.Location = new Point(43, 90);
            lblRegUser.Name = "lblRegUser";
            lblRegUser.Size = new Size(145, 17);
            lblRegUser.TabIndex = 4;
            lblRegUser.Text = "新账号 / New Account";
            // 
            // lblHeaderReg
            // 
            lblHeaderReg.AutoSize = true;
            lblHeaderReg.Font = new Font("Segoe UI", 18F);
            lblHeaderReg.ForeColor = Color.Cyan;
            lblHeaderReg.Location = new Point(40, 30);
            lblHeaderReg.Name = "lblHeaderReg";
            lblHeaderReg.Size = new Size(114, 32);
            lblHeaderReg.TabIndex = 5;
            lblHeaderReg.Text = "注册账户";
            // 
            // pnlReset
            // 
            pnlReset.BackColor = Color.FromArgb(10, 15, 25);
            pnlReset.Controls.Add(txtResetConfirm);
            pnlReset.Controls.Add(lblResetConfirm);
            pnlReset.Controls.Add(btnResetAction);
            pnlReset.Controls.Add(txtResetNewPass);
            pnlReset.Controls.Add(lblResetNewPass);
            pnlReset.Controls.Add(txtResetOldPass);
            pnlReset.Controls.Add(lblResetOldPass);
            pnlReset.Controls.Add(txtResetUser);
            pnlReset.Controls.Add(lblResetUser);
            pnlReset.Controls.Add(lblHeaderReset);
            pnlReset.Dock = DockStyle.Fill;
            pnlReset.Location = new Point(0, 0);
            pnlReset.Name = "pnlReset";
            pnlReset.Size = new Size(540, 420);
            pnlReset.TabIndex = 12;
            pnlReset.Visible = false;
            // 
            // txtResetConfirm
            // 
            txtResetConfirm.BackColor = Color.FromArgb(30, 40, 50);
            txtResetConfirm.BorderStyle = BorderStyle.FixedSingle;
            txtResetConfirm.Font = new Font("Segoe UI", 10F);
            txtResetConfirm.ForeColor = Color.White;
            txtResetConfirm.Location = new Point(46, 255);
            txtResetConfirm.Name = "txtResetConfirm";
            txtResetConfirm.PasswordChar = '●';
            txtResetConfirm.Size = new Size(300, 25);
            txtResetConfirm.TabIndex = 0;
            // 
            // lblResetConfirm
            // 
            lblResetConfirm.AutoSize = true;
            lblResetConfirm.ForeColor = Color.Silver;
            lblResetConfirm.Location = new Point(43, 235);
            lblResetConfirm.Name = "lblResetConfirm";
            lblResetConfirm.Size = new Size(147, 17);
            lblResetConfirm.TabIndex = 1;
            lblResetConfirm.Text = "确认新密码 / Confirm";
            // 
            // btnResetAction
            // 
            btnResetAction.BackColor = Color.FromArgb(40, 167, 69);
            btnResetAction.FlatAppearance.BorderSize = 0;
            btnResetAction.FlatStyle = FlatStyle.Flat;
            btnResetAction.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnResetAction.ForeColor = Color.White;
            btnResetAction.Location = new Point(46, 300);
            btnResetAction.Name = "btnResetAction";
            btnResetAction.Size = new Size(300, 40);
            btnResetAction.TabIndex = 2;
            btnResetAction.Text = "重置密码";
            btnResetAction.UseVisualStyleBackColor = false;
            btnResetAction.Click += btnResetAction_Click;
            // 
            // txtResetNewPass
            // 
            txtResetNewPass.BackColor = Color.FromArgb(30, 40, 50);
            txtResetNewPass.BorderStyle = BorderStyle.FixedSingle;
            txtResetNewPass.Font = new Font("Segoe UI", 10F);
            txtResetNewPass.ForeColor = Color.White;
            txtResetNewPass.Location = new Point(46, 200);
            txtResetNewPass.Name = "txtResetNewPass";
            txtResetNewPass.PasswordChar = '●';
            txtResetNewPass.Size = new Size(300, 25);
            txtResetNewPass.TabIndex = 3;
            // 
            // lblResetNewPass
            // 
            lblResetNewPass.AutoSize = true;
            lblResetNewPass.ForeColor = Color.Silver;
            lblResetNewPass.Location = new Point(43, 180);
            lblResetNewPass.Name = "lblResetNewPass";
            lblResetNewPass.Size = new Size(155, 17);
            lblResetNewPass.TabIndex = 4;
            lblResetNewPass.Text = "新密码 / New Password";
            // 
            // txtResetOldPass
            // 
            txtResetOldPass.BackColor = Color.FromArgb(30, 40, 50);
            txtResetOldPass.BorderStyle = BorderStyle.FixedSingle;
            txtResetOldPass.Font = new Font("Segoe UI", 10F);
            txtResetOldPass.ForeColor = Color.White;
            txtResetOldPass.Location = new Point(46, 145);
            txtResetOldPass.Name = "txtResetOldPass";
            txtResetOldPass.PasswordChar = '●';
            txtResetOldPass.Size = new Size(300, 25);
            txtResetOldPass.TabIndex = 5;
            // 
            // lblResetOldPass
            // 
            lblResetOldPass.AutoSize = true;
            lblResetOldPass.ForeColor = Color.Silver;
            lblResetOldPass.Location = new Point(43, 125);
            lblResetOldPass.Name = "lblResetOldPass";
            lblResetOldPass.Size = new Size(150, 17);
            lblResetOldPass.TabIndex = 6;
            lblResetOldPass.Text = "旧密码 / Old Password";
            // 
            // txtResetUser
            // 
            txtResetUser.BackColor = Color.FromArgb(30, 40, 50);
            txtResetUser.BorderStyle = BorderStyle.FixedSingle;
            txtResetUser.Font = new Font("Segoe UI", 10F);
            txtResetUser.ForeColor = Color.White;
            txtResetUser.Location = new Point(46, 90);
            txtResetUser.Name = "txtResetUser";
            txtResetUser.Size = new Size(300, 25);
            txtResetUser.TabIndex = 7;
            // 
            // lblResetUser
            // 
            lblResetUser.AutoSize = true;
            lblResetUser.ForeColor = Color.Silver;
            lblResetUser.Location = new Point(43, 70);
            lblResetUser.Name = "lblResetUser";
            lblResetUser.Size = new Size(99, 17);
            lblResetUser.TabIndex = 8;
            lblResetUser.Text = "账号 / Account";
            // 
            // lblHeaderReset
            // 
            lblHeaderReset.AutoSize = true;
            lblHeaderReset.Font = new Font("Segoe UI", 18F);
            lblHeaderReset.ForeColor = Color.Cyan;
            lblHeaderReset.Location = new Point(40, 20);
            lblHeaderReset.Name = "lblHeaderReset";
            lblHeaderReset.Size = new Size(114, 32);
            lblHeaderReset.TabIndex = 9;
            lblHeaderReset.Text = "重置密码";
            // 
            // pnlActivate
            // 
            pnlActivate.BackColor = Color.FromArgb(10, 15, 25);
            pnlActivate.Controls.Add(btnActAction);
            pnlActivate.Controls.Add(txtActCard);
            pnlActivate.Controls.Add(lblActCard);
            pnlActivate.Controls.Add(txtActUser);
            pnlActivate.Controls.Add(lblActUser);
            pnlActivate.Controls.Add(lblHeaderAct);
            pnlActivate.Dock = DockStyle.Fill;
            pnlActivate.Location = new Point(0, 0);
            pnlActivate.Name = "pnlActivate";
            pnlActivate.Size = new Size(540, 420);
            pnlActivate.TabIndex = 13;
            pnlActivate.Visible = false;
            // 
            // btnActAction
            // 
            btnActAction.BackColor = Color.FromArgb(40, 167, 69);
            btnActAction.FlatAppearance.BorderSize = 0;
            btnActAction.FlatStyle = FlatStyle.Flat;
            btnActAction.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnActAction.ForeColor = Color.White;
            btnActAction.Location = new Point(46, 250);
            btnActAction.Name = "btnActAction";
            btnActAction.Size = new Size(300, 40);
            btnActAction.TabIndex = 0;
            btnActAction.Text = "激 活";
            btnActAction.UseVisualStyleBackColor = false;
            btnActAction.Click += btnActAction_Click;
            // 
            // txtActCard
            // 
            txtActCard.BackColor = Color.FromArgb(30, 40, 50);
            txtActCard.BorderStyle = BorderStyle.FixedSingle;
            txtActCard.Font = new Font("Segoe UI", 11F);
            txtActCard.ForeColor = Color.White;
            txtActCard.Location = new Point(46, 183);
            txtActCard.Name = "txtActCard";
            txtActCard.Size = new Size(300, 27);
            txtActCard.TabIndex = 1;
            // 
            // lblActCard
            // 
            lblActCard.AutoSize = true;
            lblActCard.ForeColor = Color.Silver;
            lblActCard.Location = new Point(43, 160);
            lblActCard.Name = "lblActCard";
            lblActCard.Size = new Size(133, 17);
            lblActCard.TabIndex = 2;
            lblActCard.Text = "卡密 / Card Number";
            // 
            // txtActUser
            // 
            txtActUser.BackColor = Color.FromArgb(30, 40, 50);
            txtActUser.BorderStyle = BorderStyle.FixedSingle;
            txtActUser.Font = new Font("Segoe UI", 11F);
            txtActUser.ForeColor = Color.White;
            txtActUser.Location = new Point(46, 113);
            txtActUser.Name = "txtActUser";
            txtActUser.Size = new Size(300, 27);
            txtActUser.TabIndex = 3;
            // 
            // lblActUser
            // 
            lblActUser.AutoSize = true;
            lblActUser.ForeColor = Color.Silver;
            lblActUser.Location = new Point(43, 90);
            lblActUser.Name = "lblActUser";
            lblActUser.Size = new Size(99, 17);
            lblActUser.TabIndex = 4;
            lblActUser.Text = "账号 / Account";
            // 
            // lblHeaderAct
            // 
            lblHeaderAct.AutoSize = true;
            lblHeaderAct.Font = new Font("Segoe UI", 18F);
            lblHeaderAct.ForeColor = Color.Cyan;
            lblHeaderAct.Location = new Point(40, 30);
            lblHeaderAct.Name = "lblHeaderAct";
            lblHeaderAct.Size = new Size(114, 32);
            lblHeaderAct.TabIndex = 5;
            lblHeaderAct.Text = "账户激活";
            // 
            // FormLogin
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(10, 15, 25);
            ClientSize = new Size(700, 450);
            Controls.Add(panelContainer);
            Controls.Add(panelSideMenu);
            Controls.Add(panelTop);
            Font = new Font("Segoe UI", 9.75F);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormLogin";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FormLogin";
            panelSideMenu.ResumeLayout(false);
            panelLogo.ResumeLayout(false);
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelContainer.ResumeLayout(false);
            pnlLogin.ResumeLayout(false);
            pnlLogin.PerformLayout();
            pnlRegister.ResumeLayout(false);
            pnlRegister.PerformLayout();
            pnlReset.ResumeLayout(false);
            pnlReset.PerformLayout();
            pnlActivate.ResumeLayout(false);
            pnlActivate.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelSideMenu;
        private System.Windows.Forms.Panel panelLogo;
        private System.Windows.Forms.Label lblLogo;
        private System.Windows.Forms.Button btnSideLogin;
        private System.Windows.Forms.Button btnSideRegister;
        private System.Windows.Forms.Button btnSideReset;
        private System.Windows.Forms.Button btnSideActivate;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panelContainer;

        // Login Controls
        private System.Windows.Forms.Panel pnlLogin;
        private System.Windows.Forms.Label lblHeaderLogin;
        private System.Windows.Forms.TextBox txtLoginUser;
        private System.Windows.Forms.Label lblLoginUser;
        private System.Windows.Forms.TextBox txtLoginPass;
        private System.Windows.Forms.Label lblLoginPass;
        private System.Windows.Forms.Button btnLoginAction;

        // Register Controls
        private System.Windows.Forms.Panel pnlRegister;
        private System.Windows.Forms.Label lblHeaderReg;
        private System.Windows.Forms.TextBox txtRegUser;
        private System.Windows.Forms.Label lblRegUser;
        private System.Windows.Forms.TextBox txtRegPass;
        private System.Windows.Forms.Label lblRegPass;
        private System.Windows.Forms.Button btnRegAction;

        // Reset Controls
        private System.Windows.Forms.Panel pnlReset;
        private System.Windows.Forms.Label lblHeaderReset;
        private System.Windows.Forms.TextBox txtResetUser;
        private System.Windows.Forms.Label lblResetUser;
        private System.Windows.Forms.TextBox txtResetOldPass;
        private System.Windows.Forms.Label lblResetOldPass;
        private System.Windows.Forms.TextBox txtResetNewPass;
        private System.Windows.Forms.Label lblResetNewPass;
        private System.Windows.Forms.TextBox txtResetConfirm;
        private System.Windows.Forms.Label lblResetConfirm;
        private System.Windows.Forms.Button btnResetAction;

        // Activate Controls
        private System.Windows.Forms.Panel pnlActivate;
        private System.Windows.Forms.Label lblHeaderAct;
        private System.Windows.Forms.TextBox txtActUser;
        private System.Windows.Forms.Label lblActUser;
        private System.Windows.Forms.TextBox txtActCard;
        private System.Windows.Forms.Label lblActCard;
        private System.Windows.Forms.Button btnActAction;
    }
}