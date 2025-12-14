namespace QuantumIdleDesktop.Views
{
    partial class ViewSchemes
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlToolbar = new Panel();
            btnAdd = new Button();
            dgvSchemes = new DataGridView();
            pnlToolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSchemes).BeginInit();
            SuspendLayout();
            // 
            // pnlToolbar
            // 
            pnlToolbar.BackColor = Color.FromArgb(28, 38, 58);
            pnlToolbar.Controls.Add(btnAdd);
            pnlToolbar.Dock = DockStyle.Top;
            pnlToolbar.Location = new Point(0, 0);
            pnlToolbar.Name = "pnlToolbar";
            pnlToolbar.Size = new Size(980, 76);
            pnlToolbar.TabIndex = 1;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(20, 18);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(128, 42);
            btnAdd.TabIndex = 0;
            btnAdd.Text = "＋ 新建方案";
            btnAdd.Click += btnAdd_Click;
            // 
            // dgvSchemes
            // 
            dgvSchemes.AllowUserToAddRows = false;
            dgvSchemes.BackgroundColor = Color.FromArgb(15, 22, 34);
            dgvSchemes.BorderStyle = BorderStyle.None;
            dgvSchemes.Dock = DockStyle.Fill;
            dgvSchemes.EnableHeadersVisualStyles = false;
            dgvSchemes.GridColor = Color.FromArgb(45, 55, 75);
            dgvSchemes.Location = new Point(0, 76);
            dgvSchemes.MultiSelect = false;
            dgvSchemes.Name = "dgvSchemes";
            dgvSchemes.RowHeadersVisible = false;
            dgvSchemes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSchemes.Size = new Size(980, 444);
            dgvSchemes.TabIndex = 0;
            // 
            // ViewSchemes
            // 
            BackColor = Color.FromArgb(15, 22, 34);
            Controls.Add(dgvSchemes);
            Controls.Add(pnlToolbar);
            Font = new Font("微软雅黑", 9.5F);
            Name = "ViewSchemes";
            Size = new Size(980, 520);
            pnlToolbar.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvSchemes).EndInit();
            ResumeLayout(false);
        }

        private Panel pnlToolbar;
        private Button btnAdd;
        private DataGridView dgvSchemes;
        private Button btnCopy;
    }
}
