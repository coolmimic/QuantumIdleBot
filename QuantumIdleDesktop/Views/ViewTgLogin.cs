using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using QuantumIdleDesktop.Models; // 引入数据实体类命名空间

namespace QuantumIdleDesktop.Views
{
    public partial class ViewTgLogin : UserControl
    {
        public ViewTgLogin()
        {
            InitializeComponent();
            SetupDataBinding();
        }

        private void SetupDataBinding()
        {
            // 示例：将 DataGridView 的数据源设置为 TgAccountVm 列表
            List<TgAccountVm> accountList = new List<TgAccountVm>
            {
                new TgAccountVm { TgId = 1234567890, Nickname = "BotAccount1", Status = "运行中", ProfitLoss = 50.25m, Turnover = 1200.00m, SimulatedProfitLoss = 150.00m, SimulatedTurnover = 3500.00m },
                new TgAccountVm { TgId = 2345678901, Nickname = "TestUser", Status = "离线", ProfitLoss = -10.00m, Turnover = 500.00m, SimulatedProfitLoss = 20.00m, SimulatedTurnover = 800.00m }
            };

            dgvAccounts.DataSource = accountList;

            // 按钮点击事件处理 (需要在 DataGridView 事件中捕获，这里仅作注释说明)
            // dgvAccounts.CellContentClick += dgvAccounts_CellContentClick; 
        }

        // 示例：按钮点击处理逻辑 (如果需要实现)
        /*
        private void dgvAccounts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // 假设第 7 列是余额查询按钮，第 8 列是今日盈亏查询按钮
            if (e.ColumnIndex == 7) 
            {
                var account = dgvAccounts.Rows[e.RowIndex].DataBoundItem as TgAccountVm;
                if (account != null)
                {
                    MessageBox.Show($"查询账户 {account.Nickname} 的余额...");
                }
            }
            else if (e.ColumnIndex == 8)
            {
                // ... 查询今日盈亏逻辑
            }
        }
        */
    }
}