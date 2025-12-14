using QuantumIdleDesktop.Models.Odds;
using QuantumIdleDesktop.Views.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views.Odds
{
    public partial class LinearOddsEditor : OddsEditorBase
    {
        private Label lblSeq;
        private TextBox txtSequence;
        private Label lblExample;

        public LinearOddsEditor()
        {
            InitializeUI(); // 初始化界面
        }

        /// <summary>
        /// 实现基类方法：创建对象
        /// </summary>
        public override OddsConfigBase GetConfiguration()
        {
            var config = new LinearOddsConfig();

            // 处理用户输入的字符串 "10, 20, 50"
            string input = txtSequence.Text.Trim();
            if (string.IsNullOrEmpty(input))
                throw new Exception("倍投序列不能为空");

            try
            {
                // 逗号、空格、中文逗号都作为分隔符
                config.Sequence = input
                    .Split(new[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();
            }
            catch
            {
                throw new Exception("倍投序列格式错误：只能包含数字，用逗号分隔。");
            }

            return config;
        }

        /// <summary>
        /// 实现基类方法：展示对象
        /// </summary>
        public override void SetConfiguration(OddsConfigBase config)
        {
            if (config is LinearOddsConfig linear)
            {
                // 将 List<decimal> 转换回字符串显示
                txtSequence.Text = string.Join(", ", linear.Sequence);
            }
        }

        // --- 简单的代码布局 (免去 Designer 文件) ---
        private void InitializeUI()
        {
            this.Size = new Size(400, 150);
            this.BackColor = Color.FromArgb(20, 30, 45); // 深色背景
            this.ForeColor = Color.White;

            lblSeq = new Label { Text = "倍投序列 (逗号分隔):", Location = new Point(10, 10), AutoSize = true, ForeColor = Color.Silver };
            txtSequence = new TextBox { Location = new Point(10, 35), Width = 360, BackColor = Color.FromArgb(30, 40, 55), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            lblExample = new Label { Text = "例如: 10, 20, 50, 100 (输了怎么投)", Location = new Point(10, 60), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 8) };
    
            this.Controls.Add(lblSeq);
            this.Controls.Add(txtSequence);
            this.Controls.Add(lblExample);
        }
    }
}
