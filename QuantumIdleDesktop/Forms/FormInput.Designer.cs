namespace QuantumIdleDesktop.Forms
{
    partial class FormInput
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;

        private void InitializeComponent()
        {
            panel1 = new Panel();
            lblTitle = new Label();
            lblPrompt = new Label();
            txtInput = new TextBox();
            btnOk = new Button();
            btnCancel = new Button();

            panel1.SuspendLayout();
            SuspendLayout();

            //
            // panel1
            //
            panel1.BackColor = Color.FromArgb(15, 22, 32);
            panel1.Controls.Add(lblTitle);
            panel1.Controls.Add(lblPrompt);
            panel1.Controls.Add(txtInput);
            panel1.Controls.Add(btnOk);
            panel1.Controls.Add(btnCancel);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(1, 1);
            panel1.Name = "panel1";
            panel1.Size = new Size(498, 278);
            panel1.TabIndex = 0;
            panel1.MouseDown += DragForm_MouseDown;

            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.Cyan;
            lblTitle.Location = new Point(30, 30);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(54, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "标题";
            lblTitle.MouseDown += DragForm_MouseDown;

            //
            // lblPrompt
            //
            lblPrompt.AutoSize = true;
            lblPrompt.Font = new Font("Segoe UI", 10F);
            lblPrompt.ForeColor = Color.Silver;
            lblPrompt.Location = new Point(30, 70);
            lblPrompt.Name = "lblPrompt";
            lblPrompt.Size = new Size(60, 19);
            lblPrompt.TabIndex = 1;
            lblPrompt.Text = "提示语...";

            //
            // txtInput
            //
            txtInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtInput.BackColor = Color.FromArgb(30, 40, 50);
            txtInput.BorderStyle = BorderStyle.FixedSingle;
            txtInput.Font = new Font("Segoe UI", 12F);
            txtInput.ForeColor = Color.White;
            txtInput.Location = new Point(30, 110);
            txtInput.Name = "txtInput";
            txtInput.Size = new Size(438, 34);   // 几乎占满宽度
            txtInput.TabIndex = 2;

            //
            // btnOk
            //
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.BackColor = Color.FromArgb(20, 40, 25);
            btnOk.DialogResult = DialogResult.OK;
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.FlatStyle = FlatStyle.Flat;
            btnOk.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnOk.ForeColor = Color.LimeGreen;
            btnOk.Location = new Point(328, 208);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(140, 40);
            btnOk.TabIndex = 3;
            btnOk.Text = "确定";
            btnOk.UseVisualStyleBackColor = false;
            btnOk.Click += btnOk_Click;

            //
            // btnCancel
            //
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.BackColor = Color.FromArgb(35, 20, 20);
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);  // 也加粗，看着更统一
            btnCancel.ForeColor = Color.IndianRed;
            btnCancel.Location = new Point(158, 208);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(140, 40);   // 和确定一样宽
            btnCancel.TabIndex = 4;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;

            //
            // FormInput
            //
            this.AcceptButton = btnOk;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.CancelButton = btnCancel;
            this.ClientSize = new Size(500, 280);
            this.Controls.Add(panel1);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "FormInput";
            this.Padding = new Padding(1);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "FormInput";   // 可根据需要改

            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }
    }

}