using QuantumIdleDesktop.Models.DrawRules;
using QuantumIdleDesktop.Views.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views.DrawRules
{
    public partial class FixedRuleEditor : DrawRuleEditorBase
    {
        private Label lblTarget;
        private TextBox txtTarget;
        private Label lblExample;

        public FixedRuleEditor()
        {
            InitializeUI();
        }

        /// <summary>
        /// 实现基类方法：创建对象
        /// </summary>
        public override DrawRuleConfigBase GetConfiguration()
        {
            var config = new FixedNumberDrawRuleConfig();

            string input = txtTarget.Text.Trim();
            if (string.IsNullOrEmpty(input))
                throw new Exception("投注内容不能为空");

            // 将 "Big, Odd" 解析为 List
            config.TargetNumbers = input
                .Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            return config;
        }

        /// <summary>
        /// 实现基类方法：展示对象
        /// </summary>
        public override void SetConfiguration(DrawRuleConfigBase config)
        {
            if (config is FixedNumberDrawRuleConfig fixedRule)
            {
                txtTarget.Text = string.Join(", ", fixedRule.TargetNumbers);
            }
        }

        // --- 代码布局 ---
        private void InitializeUI()
        {
            this.Size = new Size(400, 120);
            this.BackColor = Color.FromArgb(20, 30, 45);
            this.ForeColor = Color.White;

            lblTarget = new Label { Text = "固定投注内容 (多个用逗号分隔):", Location = new Point(10, 10), AutoSize = true, ForeColor = Color.Silver };
            txtTarget = new TextBox { Location = new Point(10, 35), Width = 360, BackColor = Color.FromArgb(30, 40, 55), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            lblExample = new Label { Text = "例如: Big, Small (压大小) 或 7, 8 (压特码)", Location = new Point(10, 60), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 8) };

            this.Controls.Add(lblTarget);
            this.Controls.Add(txtTarget);
            this.Controls.Add(lblExample);
        }
    }
}
