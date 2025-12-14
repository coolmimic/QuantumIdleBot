using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views
{
    public partial class ViewLotteryHistory : UserControl
    {
        // 控件定义
        private ComboBox cmbGroups;
        private Label lblSelectGroup;
        private DataGridView dgvHistory;
        private Panel panelTop;

        public ViewLotteryHistory()
        {
            // 1. 初始化界面
            InitializeCustomUI();

            // 2. 加载数据
            if (!DesignMode) // 防止设计器模式下报错
            {
                LoadGroupData();
            }
        }

        private void InitializeCustomUI()
        {
            this.Size = new Size(970, 340);
            this.BackColor = Color.White; // 整体背景白
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point); // 使用更现代的字体

            // ===========================
            // 1. 顶部操作栏美化
            // ===========================
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60, // 增加高度，更宽松
                Padding = new Padding(20, 15, 20, 15), // 内边距
                BackColor = Color.White
            };

            lblSelectGroup = new Label
            {
                Text = "选择群组：",
                AutoSize = true,
                Location = new Point(20, 20),
                ForeColor = Color.FromArgb(64, 64, 64), // 深灰色文字
                Font = new Font("微软雅黑", 10f, FontStyle.Bold)
            };

            cmbGroups = new ComboBox
            {
                Location = new Point(100, 17),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("微软雅黑", 10f),
                FlatStyle = FlatStyle.System // 系统原生样式，比默认的3D好看一点
            };
            cmbGroups.SelectedIndexChanged += CmbGroups_SelectedIndexChanged;

            panelTop.Controls.Add(lblSelectGroup);
            panelTop.Controls.Add(cmbGroups);

            // ===========================
            // 2. 表格深度美化 (DataGridView)
            // ===========================
            dgvHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White, // 背景纯白
                BorderStyle = BorderStyle.None, // 去掉边框

                // --- 核心功能限制 ---
                ReadOnly = true,                    // 禁止编辑
                AllowUserToAddRows = false,         // 禁止添加
                AllowUserToDeleteRows = false,      // 禁止删除
                AllowUserToResizeColumns = false,   // 禁止调整列宽
                AllowUserToResizeRows = false,      // 禁止调整行高
                RowHeadersVisible = false,          // 隐藏左侧行头
                MultiSelect = false,                // 禁止多选
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, // 整行选中

                // --- 样式设置 ---
                EnableHeadersVisualStyles = false,  // 允许自定义表头样式
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // 自动填充
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal, // 只有横向分割线
                GridColor = Color.FromArgb(230, 230, 230), // 分割线颜色很淡
                RowTemplate = { Height = 40 }       // **增加行高**，看起来不拥挤
            };

            // 设置表头样式
            dgvHistory.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvHistory.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(245, 247, 250), // 浅灰蓝色表头背景
                ForeColor = Color.FromArgb(100, 100, 100), // 表头文字颜色
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter, // 表头居中
                Padding = new Padding(0, 10, 0, 10) // 表头上下留白
            };
            dgvHistory.ColumnHeadersHeight = 45; // 表头高度
            dgvHistory.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing; // 禁止调整表头高度

            // 设置单元格默认样式
            dgvHistory.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.FromArgb(50, 50, 50),
                Font = new Font("Segoe UI", 10F),
                SelectionBackColor = Color.FromArgb(232, 240, 254), // 选中时的背景色（淡蓝）
                SelectionForeColor = Color.FromArgb(50, 50, 50),    // 选中时的文字颜色（不变黑）
                Alignment = DataGridViewContentAlignment.MiddleCenter, // 内容居中
                Padding = new Padding(5, 0, 5, 0)
            };

            // 添加列
            AddColumn("GroupName", "群名称", 25);
            AddColumn("OpenTime", "开奖时间", 25);
            AddColumn("IssueNumber", "期号", 20);
            AddColumn("Result", "开奖号码", 30);

            // 特殊列样式：开奖号码
            dgvHistory.Columns["Result"].DefaultCellStyle.Font = new Font("Consolas", 11F, FontStyle.Bold); // 等宽字体
            dgvHistory.Columns["Result"].DefaultCellStyle.ForeColor = Color.FromArgb(0, 120, 215); // 蓝色高亮

            this.Controls.Add(dgvHistory);
            this.Controls.Add(panelTop);
        }

        // 辅助方法：添加列并设置禁止排序
        private void AddColumn(string name, string headerText, float fillWeight)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = headerText,
                DataPropertyName = name,
                SortMode = DataGridViewColumnSortMode.NotSortable, // **核心：禁止排序**
                FillWeight = fillWeight
            };
            dgvHistory.Columns.Add(col);
        }

        private void LoadGroupData()
        {
            try
            {
                cmbGroups.Items.Clear();
                if (GameContextService.Instance?._groupContexts == null) return;

                foreach (var context in GameContextService.Instance._groupContexts.Values)
                {
                    cmbGroups.Items.Add(new ComboBoxItem
                    {
                        Text = context.GroupModel.Name,
                        Value = context.GroupModel.Id
                    });
                }

                if (cmbGroups.Items.Count > 0) cmbGroups.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"数据加载异常: {ex.Message}");
            }
        }

        private void CmbGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbGroups.SelectedItem is ComboBoxItem selectedItem)
            {
                RefreshHistoryGrid((long)selectedItem.Value);
            }
        }

        private void RefreshHistoryGrid(long groupId)
        {
            dgvHistory.Rows.Clear(); // 清空

            if (GameContextService.Instance._groupContexts.TryGetValue(groupId, out var context))
            {
                if (context.History == null) return;

                // 倒序显示（最新的在最上面）
                var displayList = context.History.ToList();
                // 如果本来就是最新的在最前面，就不用Reverse，根据你实际数据调整
                // displayList.Reverse(); 

                foreach (var record in displayList)
                {
                    dgvHistory.Rows.Add(
                        context.GroupModel.Name,
                        record.OpenTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        record.IssueNumber,
                        record.Result
                    );
                }

                // 刷新后自动取消选中第一行，看起来更干净
                dgvHistory.ClearSelection();
            }
        }

        private class ComboBoxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }
            public override string ToString() => Text;
        }
    }
}