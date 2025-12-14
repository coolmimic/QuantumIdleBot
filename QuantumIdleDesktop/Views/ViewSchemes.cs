using QuantumIdleDesktop.Forms;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Utils;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views
{
    public partial class ViewSchemes : UserControl
    {
        // 声明新增的按钮字段
        private Button btnClearAll;

        public ViewSchemes()
        {
            InitializeComponent();
            ApplyExchangeLevelStyle();
            SetupToolbarButtons();
            SetupDataGridViewColumns();
            RefreshGrid();

            dgvSchemes.CellContentClick += dgvSchemes_CellContentClick;
            dgvSchemes.CellMouseEnter += (s, e) => { if (e.RowIndex >= 0) dgvSchemes.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(30, 40, 60); };
            dgvSchemes.CellMouseLeave += (s, e) => { if (e.RowIndex >= 0) RefreshRowColor(dgvSchemes.Rows[e.RowIndex]); };
        }

        private void ApplyExchangeLevelStyle()
        {
            this.BackColor = Color.FromArgb(15, 20, 32);
            this.Font = new Font("微软雅黑", 9F);
            this.Dock = DockStyle.Fill;
            // 工具栏：48px 纯黑底
            pnlToolbar.Height = 48;
            pnlToolbar.BackColor = Color.FromArgb(10, 15, 28);
            pnlToolbar.Padding = new Padding(0);


            // DataGridView：极致紧凑、专业
            dgvSchemes.Dock = DockStyle.Fill;
            dgvSchemes.BackgroundColor = Color.FromArgb(15, 20, 32);
            dgvSchemes.BorderStyle = BorderStyle.None;
            dgvSchemes.GridColor = Color.FromArgb(40, 50, 70);
            dgvSchemes.EnableHeadersVisualStyles = false;
            dgvSchemes.RowHeadersVisible = false;
            dgvSchemes.AllowUserToAddRows = false;
            dgvSchemes.AllowUserToResizeColumns = false;
            dgvSchemes.AllowUserToResizeRows = false;
            dgvSchemes.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvSchemes.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvSchemes.MultiSelect = false;
            dgvSchemes.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvSchemes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(15, 20, 32);  // 完全不明显
            dgvSchemes.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvSchemes.RowTemplate.Height = 36;  // 紧凑！
            dgvSchemes.ColumnHeadersHeight = 40;

            // 表头：深色底 + 白粗体 + 下边框线
            var header = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(10, 15, 28),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9.5F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(0, 0, 0, 2)
            };
            dgvSchemes.ColumnHeadersDefaultCellStyle = header;
            dgvSchemes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // 数据行样式
            dgvSchemes.RowsDefaultCellStyle.BackColor = Color.FromArgb(18, 25, 40);
            dgvSchemes.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(22, 28, 44);
            dgvSchemes.RowsDefaultCellStyle.ForeColor = Color.FromArgb(200, 220, 255);
            dgvSchemes.RowsDefaultCellStyle.Font = new Font("微软雅黑", 9F);
            dgvSchemes.RowsDefaultCellStyle.Padding = new Padding(4);

            // 禁止排序
            foreach (DataGridViewColumn c in dgvSchemes.Columns)
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
        }


        private void SetupToolbarButtons()
        {
            // --- 1. 配置原有的 btnAdd (新建按钮) ---
            btnAdd.Size = new Size(108, 30);
            btnAdd.Text = "＋ 新建方案";
            btnAdd.BackColor = Color.FromArgb(0, 170, 90); // 绿色
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Font = new Font("微软雅黑", 11F, FontStyle.Bold);
            btnAdd.Cursor = Cursors.Hand;

            // 移除旧的事件，防止重复绑定 (如果有的话)
            btnAdd.MouseEnter += BtnAdd_MouseEnter;
            btnAdd.MouseLeave += BtnAdd_MouseLeave;

            // --- 2. 创建并配置 btnCopy (复制按钮) ---
            if (btnCopy == null)
            {
                btnCopy = new Button();
                pnlToolbar.Controls.Add(btnCopy); // 加入到面板中
                btnCopy.Click += BtnCopy_Click;   // 绑定点击事件
            }

            btnCopy.Size = new Size(108, 30);
            btnCopy.Text = "❐ 复制方案";
            btnCopy.BackColor = Color.FromArgb(255, 140, 0); // 橙色，与绿色区分明显
            btnCopy.ForeColor = Color.White;
            btnCopy.FlatStyle = FlatStyle.Flat;
            btnCopy.FlatAppearance.BorderSize = 0;
            btnCopy.Font = new Font("微软雅黑", 11F, FontStyle.Bold);
            btnCopy.Cursor = Cursors.Hand;

            // 简单的悬停变色效果
            btnCopy.MouseEnter += (s, e) => btnCopy.BackColor = Color.FromArgb(255, 160, 50);
            btnCopy.MouseLeave += (s, e) => btnCopy.BackColor = Color.FromArgb(255, 140, 0);

            // --- 3. 创建并配置 btnClearAll (清空所有方案按钮) ---
            if (btnClearAll == null)
            {
                btnClearAll = new Button();
                pnlToolbar.Controls.Add(btnClearAll); // 加入到面板中
                btnClearAll.Click += BtnClearAll_Click; // 绑定点击事件
            }

            btnClearAll.Size = new Size(130, 30);
            btnClearAll.Text = "🗑️ 清空所有方案";
            btnClearAll.BackColor = Color.FromArgb(200, 40, 60); // 红色，警示色
            btnClearAll.ForeColor = Color.White;
            btnClearAll.FlatStyle = FlatStyle.Flat;
            btnClearAll.FlatAppearance.BorderSize = 0;
            btnClearAll.Font = new Font("微软雅黑", 11F, FontStyle.Bold);
            btnClearAll.Cursor = Cursors.Hand;

            // 简单的悬停变色效果
            btnClearAll.MouseEnter += (s, e) => btnClearAll.BackColor = Color.FromArgb(220, 60, 80);
            btnClearAll.MouseLeave += (s, e) => btnClearAll.BackColor = Color.FromArgb(200, 40, 60);


            // --- 4. 计算位置：让三个按钮在中间并排 ---
            RepositionButtons();

            // 绑定面板调整大小事件，保持居中
            pnlToolbar.Resize -= PnlToolbar_Resize;
            pnlToolbar.Resize += PnlToolbar_Resize;
        }

        private async void BtnCopy_Click(object sender, EventArgs e)
        {
            // 1. 实例化批量复制窗体
            // 使用 using 确保窗体关闭后资源被立即释放
            using (FormCopySchemes form = new FormCopySchemes())
            {
                // 2. 判断是否点击了“确认”按钮 (我们在 FormCopySchemes 里设置了 DialogResult.OK)
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // 3. 刷新列表，显示新生成的方案
                    RefreshGrid();
                }
            }
        }

        private async void BtnClearAll_Click(object sender, EventArgs e)
        {
            if (CacheData.Schemes == null || CacheData.Schemes.Count == 0)
            {
                MessageBox.Show("当前没有方案。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 弹窗警告用户
            var result = MessageBox.Show(
                $"您确定要**永久删除**所有 {CacheData.Schemes.Count} 个方案吗？此操作不可撤销！",
                "⚠️ 确认清空所有方案",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2 // 默认选中“否”
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    // 清空列表
                    CacheData.Schemes.Clear();
                    // 保存空列表到文件
                    await SchemeFileHelper.SaveListAsync(CacheData.Schemes);
                    // 刷新界面
                    RefreshGrid();
                    MessageBox.Show("所有方案已成功清空。", "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"清空方案时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void BtnAdd_MouseEnter(object sender, EventArgs e) => btnAdd.BackColor = Color.FromArgb(0, 200, 110);
        private void BtnAdd_MouseLeave(object sender, EventArgs e) => btnAdd.BackColor = Color.FromArgb(0, 170, 90);
        private void PnlToolbar_Resize(object sender, EventArgs e) => RepositionButtons();

        // 独立的定位逻辑，方便 Resize 调用
        private void RepositionButtons()
        {
            int gap = 20; // 按钮之间的间距
            // 计算三个按钮的总宽度
            int totalWidth = btnAdd.Width + gap + btnCopy.Width + gap + btnClearAll.Width;

            // 计算起始 X 坐标
            int startX = (pnlToolbar.Width - totalWidth) / 2;
            int y = (pnlToolbar.Height - btnAdd.Height) / 2;

            // 定位三个按钮
            btnAdd.Location = new Point(startX, y);
            btnCopy.Location = new Point(startX + btnAdd.Width + gap, y);
            btnClearAll.Location = new Point(startX + btnAdd.Width + gap + btnCopy.Width + gap, y);
        }


        private void SetupDataGridViewColumns()
        {
            dgvSchemes.Columns.Clear();

            var cols = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn   { Name = "Id",      HeaderText = "ID",      Visible = false },
                new DataGridViewTextBoxColumn   { Name = "Status",  HeaderText = "状态",    Width = 60,  DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter } },
                new DataGridViewTextBoxColumn   { Name = "Name",    HeaderText = "方案名称", FillWeight = 180 },
                new DataGridViewTextBoxColumn   { Name = "Group",   HeaderText = "群组",      FillWeight = 130 },
                new DataGridViewTextBoxColumn   { Name = "Game",    HeaderText = "游戏",      FillWeight = 90 },
                new DataGridViewTextBoxColumn   { Name = "Play",    HeaderText = "玩法",      FillWeight = 90 },
                new DataGridViewTextBoxColumn   { Name = "Draw",    HeaderText = "出号",      FillWeight = 100 },
                new DataGridViewTextBoxColumn   { Name = "Odds",    HeaderText = "倍投",      FillWeight = 100 },
                new DataGridViewTextBoxColumn   { Name = "Profit",  HeaderText = "实际盈亏", FillWeight = 95,  DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn   { Name = "Sim",     HeaderText = "模拟盈亏", FillWeight = 95,  DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewButtonColumn    { Name = "Edit",    HeaderText = "",          Width = 46,  Text = "编辑", UseColumnTextForButtonValue = true },
                new DataGridViewButtonColumn    { Name = "Delete",  HeaderText = "",          Width = 46,  Text = "删除", UseColumnTextForButtonValue = true }
            };

            dgvSchemes.Columns.AddRange(cols);

            // 操作按钮
            var edit = (DataGridViewButtonColumn)dgvSchemes.Columns["Edit"];
            edit.FlatStyle = FlatStyle.Flat;
            edit.DefaultCellStyle.BackColor = Color.FromArgb(0, 120, 200);
            edit.DefaultCellStyle.ForeColor = Color.White;
            edit.DefaultCellStyle.Font = new Font("微软雅黑", 8.5F);

            var del = (DataGridViewButtonColumn)dgvSchemes.Columns["Delete"];
            del.FlatStyle = FlatStyle.Flat;
            del.DefaultCellStyle.BackColor = Color.FromArgb(200, 40, 60);
            del.DefaultCellStyle.ForeColor = Color.White;
            del.DefaultCellStyle.Font = new Font("微软雅黑", 8.5F);

            dgvSchemes.ReadOnly = true;
        }

        private void RefreshGrid()
        {
            dgvSchemes.Rows.Clear();
            if (CacheData.Schemes == null) return;

            int index = 0;
            foreach (var s in CacheData.Schemes)
            {
                string gameName = EnumHelper.GetDescription(s.GameType);
                string playName = EnumHelper.GetDescription(s.PlayMode);
                string drawRule = EnumHelper.GetDescription(s.DrawRule);
                string oddsType = EnumHelper.GetDescription(s.OddsType);

                int rowIdx = dgvSchemes.Rows.Add(
                    s.Id,
                    s.IsEnabled ? "✔" : "✖",
                    s.Name,
                    s.TgGroupName,
                    gameName,
                    playName,
                    drawRule,
                    oddsType,
                    FormatProfit(s.RealProfit),
                    FormatProfit(s.SimulatedProfit),
                    "编辑",
                    "删除"
                );

                var row = dgvSchemes.Rows[rowIdx];
                row.Tag = s;

                // 状态符号颜色
                var status = row.Cells["Status"];
                status.Style.ForeColor = s.IsEnabled ? Color.LimeGreen : Color.FromArgb(255, 85, 100);
                status.Style.Font = new Font("Segoe UI Symbol", 12F, FontStyle.Bold);

                // 盈亏颜色
                SetProfitColor(row.Cells["Profit"], s.RealProfit);
                SetProfitColor(row.Cells["Sim"], s.SimulatedProfit);

                // 交替行色
                row.DefaultCellStyle.BackColor = (index++ % 2 == 0)
                    ? Color.FromArgb(18, 25, 40)
                    : Color.FromArgb(22, 28, 44);
            }
            dgvSchemes.ClearSelection();
        }

        private void RefreshRowColor(DataGridViewRow row)
        {
            int index = row.Index;
            row.DefaultCellStyle.BackColor = (index % 2 == 0)
                ? Color.FromArgb(18, 25, 40)
                : Color.FromArgb(22, 28, 44);
        }

        private string FormatProfit(decimal v) => v >= 0 ? $"+{v:F2}" : v.ToString("F2");
        private void SetProfitColor(DataGridViewCell cell, decimal v)
        {
            cell.Style.ForeColor = v > 0 ? Color.FromArgb(255, 85, 100) : (v < 0 ? Color.LimeGreen : Color.Gray);
        }

        private async void dgvSchemes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvSchemes.Rows[e.RowIndex];
            var scheme = row.Tag as SchemeModel;
            if (scheme == null) return;

            // 点击状态符号：切换
            if (e.ColumnIndex == dgvSchemes.Columns["Status"].Index)
            {
                scheme.IsEnabled = !scheme.IsEnabled;
                await SchemeFileHelper.SaveListAsync(CacheData.Schemes);
                row.Cells["Status"].Value = scheme.IsEnabled ? "✔" : "✖";
                row.Cells["Status"].Style.ForeColor = scheme.IsEnabled ? Color.LimeGreen : Color.FromArgb(255, 85, 100);
                return;
            }

            if (dgvSchemes.Columns[e.ColumnIndex].Name == "Edit")
            {
                if (CacheData.tgService == null || !CacheData.tgService.IsOnline)
                {
                    MessageBox.Show("请先登录 Telegram。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                using (var f = new FormSchemeEditor(scheme))
                    if (f.ShowDialog() == DialogResult.OK) RefreshGrid();
            }
            else if (dgvSchemes.Columns[e.ColumnIndex].Name == "Delete")
            {
                if (MessageBox.Show($"确定删除方案 [{scheme.Name}] 吗？", "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    CacheData.Schemes.Remove(scheme);
                    await SchemeFileHelper.SaveListAsync(CacheData.Schemes);
                    RefreshGrid();
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (CacheData.tgService == null || !CacheData.tgService.IsOnline)
            {
                MessageBox.Show("请先登录 Telegram 才能获取群组信息。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var f = new FormSchemeEditor())
                if (f.ShowDialog() == DialogResult.OK) RefreshGrid();
        }
    }
}