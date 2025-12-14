using System.Drawing;
using System.Windows.Forms;

namespace QuantumIdleUpdater
{
    partial class FrmUpdater
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            // 定义控件
            this.pnlTitleBar = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblPercent = new System.Windows.Forms.Label();
            this.pnlTrack = new System.Windows.Forms.Panel(); // 进度条槽
            this.pnlBar = new System.Windows.Forms.Panel();   // 进度条实体

            this.pnlTitleBar.SuspendLayout();
            this.pnlTrack.SuspendLayout();
            this.SuspendLayout();

            // 
            // FrmUpdater (主窗体设置)
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30))))); // 深灰背景
            this.ClientSize = new System.Drawing.Size(450, 180);
            this.Controls.Add(this.pnlTrack);
            this.Controls.Add(this.lblPercent);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pnlTitleBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None; // 无边框！
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Name = "FrmUpdater";
            this.Text = "Update";
            this.Load += new System.EventHandler(this.FrmUpdater_Load);

            // 
            // pnlTitleBar (顶部拖动条)
            // 
            this.pnlTitleBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.pnlTitleBar.Controls.Add(this.btnClose);
            this.pnlTitleBar.Controls.Add(this.lblTitle);
            this.pnlTitleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitleBar.Height = 35;
            this.pnlTitleBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlTitleBar_MouseDown); // 绑定拖动事件

            // 
            // lblTitle (标题文字)
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.Silver;
            this.lblTitle.Location = new System.Drawing.Point(10, 8);
            this.lblTitle.Text = "Quantum Update";

            // 
            // btnClose (右上角关闭 X)
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.Gray;
            this.btnClose.Location = new System.Drawing.Point(415, 0);
            this.btnClose.Size = new System.Drawing.Size(35, 35);
            this.btnClose.Text = "X";
            this.btnClose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            //this.btnClose.MouseEnter += (s, e) => { btnClose.ForeColor = Color.Red; };
            //this.btnClose.MouseLeave += (s, e) => { btnClose.ForeColor = Color.Gray; };

            // 
            // lblStatus (状态提示)
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Location = new System.Drawing.Point(25, 60);
            this.lblStatus.Text = "正在连接服务器...";

            // 
            // lblPercent (百分比数字)
            // 
            this.lblPercent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPercent.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblPercent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192))))); // 青色
            this.lblPercent.Location = new System.Drawing.Point(350, 55);
            this.lblPercent.Text = "0%";
            this.lblPercent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            // 
            // pnlTrack (进度条底槽)
            // 
            this.pnlTrack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.pnlTrack.Controls.Add(this.pnlBar);
            this.pnlTrack.Location = new System.Drawing.Point(25, 95);
            this.pnlTrack.Size = new System.Drawing.Size(400, 10); // 细长的条

            // 
            // pnlBar (进度条填充物 - 核心)
            // 
            this.pnlBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192))))); // 青色/量子色
            this.pnlBar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlBar.Size = new System.Drawing.Size(0, 10); // 初始宽度为0

            this.pnlTitleBar.ResumeLayout(false);
            this.pnlTitleBar.PerformLayout();
            this.pnlTrack.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Panel pnlTitleBar;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label btnClose;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblPercent;
        private System.Windows.Forms.Panel pnlTrack;
        private System.Windows.Forms.Panel pnlBar;
    }
}