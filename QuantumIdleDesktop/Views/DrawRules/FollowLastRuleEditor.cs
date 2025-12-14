using QuantumIdleDesktop.Models.DrawRules;
using QuantumIdleDesktop.Views.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views.DrawRules
{
    public partial class FollowLastRuleEditor : DrawRuleEditorBase
    {
        private TextBox txtTrigger;
        private TextBox txtTarget;
        private Button btnAdd;
        private DataGridView dgvRules;

        public FollowLastRuleEditor()
        {
            InitializeCustomComponent();
        }

        private void InitializeCustomComponent()
        {
            this.Size = new Size(400, 240);
            this.Padding = new Padding(12);
            this.BackColor = Color.FromArgb(40, 50, 70);
            this.Font = new Font("Microsoft YaHei UI", 9F);
            this.SuspendLayout();

            // === 输入区 ===
            var lblTrigger = new Label
            {
                Text = "开出号码（触发）",
                ForeColor = Color.FromArgb(200, 200, 220),
                AutoSize = true,
                Location = new Point(12, 14)
            };

            txtTrigger = new TextBox
            {
                Location = new Point(12, 36),
                Size = new Size(120, 28),
                Text = "大",
                BackColor = Color.FromArgb(35, 45, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTarget = new Label
            {
                Text = "投注号码（多个用逗号分隔）",
                ForeColor = Color.FromArgb(200, 200, 220),
                AutoSize = true,
                Location = new Point(145, 14)
            };

            txtTarget = new TextBox
            {
                Location = new Point(145, 36),
                Size = new Size(165, 28),
                Text = "大",
                BackColor = Color.FromArgb(35, 45, 65),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnAdd = new Button
            {
                Text = "添  加",
                Location = new Point(320, 35),
                Size = new Size(66, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold)
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            // === DataGridView ===
            dgvRules = new DataGridView
            {
                Location = new Point(12, 76),
                Size = new Size(374, 150),
                BackgroundColor = Color.FromArgb(45, 55, 75),
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,                    // 彻底禁止编辑
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 32,
                GridColor = Color.FromArgb(60, 70, 90),
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal
            };

            // 列定义
            var colTrigger = new DataGridViewTextBoxColumn
            {
                HeaderText = "开出号码（触发）",
                Name = "Trigger",
                FillWeight = 40
            };
            var colTarget = new DataGridViewTextBoxColumn
            {
                HeaderText = "投注号码（执行）",
                Name = "Target",
                FillWeight = 50
            };
            var colDelete = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Name = "Delete",
                Text = "×",
                UseColumnTextForButtonValue = true,
                Width = 40,
                FillWeight = 10
            };

            dgvRules.Columns.AddRange(colTrigger, colTarget, colDelete);

            // 美化表头
            dgvRules.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 60, 80);
            dgvRules.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRules.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            dgvRules.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 单元格样式
            dgvRules.DefaultCellStyle.BackColor = Color.FromArgb(45, 55, 75);
            dgvRules.DefaultCellStyle.ForeColor = Color.White;
            dgvRules.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvRules.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvRules.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

            // 删除按钮样式
            dgvRules.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == dgvRules.Columns["Delete"].Index)
                {
                    dgvRules.Rows.RemoveAt(e.RowIndex);
                }
            };

            dgvRules.CellMouseEnter += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == dgvRules.Columns["Delete"].Index)
                    dgvRules.Cursor = Cursors.Hand;
            };
            dgvRules.CellMouseLeave += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == dgvRules.Columns["Delete"].Index)
                    dgvRules.Cursor = Cursors.Default;
            };

            // === 添加控件 ===
            this.Controls.Add(lblTrigger);
            this.Controls.Add(txtTrigger);
            this.Controls.Add(lblTarget);
            this.Controls.Add(txtTarget);
            this.Controls.Add(btnAdd);
            this.Controls.Add(dgvRules);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            string trigger = txtTrigger.Text.Trim();
            string targetRaw = txtTarget.Text.Trim();

            if (string.IsNullOrWhiteSpace(trigger) || string.IsNullOrWhiteSpace(targetRaw))
            {
                MessageBox.Show("触发条件和投注号码不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 检查重复（以触发条件为 key）
            foreach (DataGridViewRow row in dgvRules.Rows)
            {
                if (row.Cells["Trigger"].Value?.ToString() == trigger)
                {
                    row.Cells["Target"].Value = targetRaw;
                    txtTrigger.Clear();
                    txtTarget.Clear();
                    txtTrigger.Focus();
                    return;
                }
            }

            // 新增
            dgvRules.Rows.Add(trigger, targetRaw);
            txtTrigger.Clear();
            txtTarget.Clear();
            txtTrigger.Focus();
        }

        public override DrawRuleConfigBase GetConfiguration()
        {
            var config = new FollowLastDrawRuleConfig
            {
                DrawRuleDic = new Dictionary<string, List<string>>()
            };

            foreach (DataGridViewRow row in dgvRules.Rows)
            {
                if (row.IsNewRow) continue;
                string trigger = row.Cells["Trigger"].Value?.ToString()?.Trim();
                string targetRaw = row.Cells["Target"].Value?.ToString()?.Trim();

                if (string.IsNullOrEmpty(trigger) || string.IsNullOrEmpty(targetRaw)) continue;

                var targets = targetRaw
                    .Split(new[] { ',', '，', ';', '；', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();

                if (targets.Count > 0)
                    config.DrawRuleDic[trigger] = targets;
            }

            return config;
        }

        public override void SetConfiguration(DrawRuleConfigBase config)
        {
            dgvRules.Rows.Clear();

            if (config is FollowLastDrawRuleConfig c && c.DrawRuleDic != null)
            {
                foreach (var kvp in c.DrawRuleDic)
                {
                    string trigger = kvp.Key;
                    string targetStr = string.Join(", ", kvp.Value);
                    dgvRules.Rows.Add(trigger, targetStr);
                }
            }
        }
    }
}