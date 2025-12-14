using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Services;
using QuantumIdleDesktop.Utils;
using System;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views
{
    public partial class ViewOddsSetting : UserControl
    {
        private GameType _currentGame;

        public ViewOddsSetting()
        {
            InitializeComponent();
            InitGameList();
            lbGames.SelectedIndexChanged += LbGames_SelectedIndexChanged;
            btnSave.Click += BtnSave_Click;
        }

        private void InitGameList()
        {
            lbGames.Items.Clear();
            // 使用 UIHelper 扩展方法绑定，自动显示中文
            lbGames.BindEnum<GameType>();

            if (lbGames.Items.Count > 0) lbGames.SelectedIndex = 0;
        }

        private void LbGames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbGames.SelectedItem == null) return;
            // 使用 UIHelper 扩展方法获取选中的枚举
            _currentGame = lbGames.GetSelectedEnum<GameType>();
            RefreshGrid(_currentGame);
        }

        private void RefreshGrid(GameType gameType)
        {
            dgvOdds.Rows.Clear();

            foreach (GamePlayMode mode in Enum.GetValues(typeof(GamePlayMode)))
            {
                decimal odds = GlobalOddsManager.Instance.GetOdds(gameType, mode);

                int index = dgvOdds.Rows.Add();

                // 【核心修改】这里使用 GetDescription() 显示中文 "大小" 而不是 "BigSmall"
                dgvOdds.Rows[index].Cells[0].Value = mode.GetDescription();

                dgvOdds.Rows[index].Cells[1].Value = odds;
                dgvOdds.Rows[index].Tag = mode;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            dgvOdds.EndEdit();

            try
            {
                foreach (DataGridViewRow row in dgvOdds.Rows)
                {
                    if (row.Tag is GamePlayMode mode)
                    {
                        if (decimal.TryParse(row.Cells[1].Value?.ToString(), out decimal val))
                        {
                            GlobalOddsManager.Instance.UpdateOdds(_currentGame, mode, val);
                        }
                    }
                }

                GlobalOddsManager.Instance.SaveConfig();
                MessageBox.Show("赔率配置保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}