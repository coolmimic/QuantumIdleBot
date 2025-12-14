using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Forms
{
    public partial class FormCopySchemes : Form
    {
        // 模拟 CacheData 中的群组缓存 (建议你把这个 List 放进 CacheData 类里)
        // public static List<TelegramGroupModel> CachedGroups = new List<TelegramGroupModel>();
        // 这里为了演示方便，我先引用 CacheData.CachedGroups，如果没有请自行添加。
        // 如果不想改 CacheData，可以暂且用这个静态变量：

        // UI 控件
        private TableLayoutPanel mainLayout;
        private Panel pnlBottom;
        private ListBox lstSchemes;
        private CheckedListBox chkGroups;
        private Label lblTitleScheme, lblTitleGroup;
        private Button btnRefreshGroups, btnConfirm;
        private CheckBox chkSelectAll;
        private TextBox txtSearch; // 新增搜索框

        // 临时存储当前显示的完整列表，用于搜索过滤
        private List<TelegramGroupModel> _currentFullList = new List<TelegramGroupModel>();

        public FormCopySchemes()
        {
            InitializeComponent();
            SetupUI();
            SetupEvents();

            this.Load += async (s, e) => await LoadDataAsync();
        }

        // --- 1. 数据加载 (优化缓存与搜索) ---
        private async Task LoadDataAsync()
        {
            // 左侧：加载方案
            lstSchemes.Items.Clear();
            if (CacheData.Schemes != null)
            {
                foreach (var s in CacheData.Schemes) lstSchemes.Items.Add(s);
            }

            // 右侧：加载群组 (优先读缓存)
            // 假设 CacheData.CachedGroups 是你存放群组的地方，如果还没创建，请使用 _localGroupCache
            var cache = CacheData.GroupLst;

            if (cache.Count > 0)
            {
                // 有缓存，直接用
                _currentFullList = new List<TelegramGroupModel>(cache);
                UpdateGroupListUI(_currentFullList);
            }
            else
            {
                // 没缓存，请求 API
                await FetchGroupsFromApi();
            }
        }

        private async Task FetchGroupsFromApi()
        {
            chkGroups.Items.Clear();
            btnRefreshGroups.Enabled = false;
            btnRefreshGroups.Text = "...";

            try
            {
                if (CacheData.tgService == null || !CacheData.tgService.IsOnline)
                {
                    chkGroups.Items.Add("未登录 Telegram");
                    return;
                }

                var groups = await CacheData.tgService.GetAllChats();

                // 更新缓存
                CacheData.GroupLst = groups.OrderBy(g => g.Name).ToList();
                _currentFullList = new List<TelegramGroupModel>(CacheData.GroupLst);

                // 更新界面
                UpdateGroupListUI(_currentFullList);
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取失败: " + ex.Message);
            }
            finally
            {
                btnRefreshGroups.Enabled = true;
                btnRefreshGroups.Text = "⟳";
            }
        }

        // 辅助方法：更新列表显示（支持搜索过滤）
        private void UpdateGroupListUI(List<TelegramGroupModel> list)
        {
            chkGroups.BeginUpdate();
            chkGroups.Items.Clear();
            foreach (var g in list)
            {
                chkGroups.Items.Add(g);
            }
            chkGroups.EndUpdate();

            // 如果是在搜索状态，取消全选勾选，避免误操作
            chkSelectAll.Checked = false;
        }

        // --- 2. 核心：确认复制 ---
        private async void btnConfirm_Click(object sender, EventArgs e)
        {

            if (CacheData.GroupLst.Count <= 0)
            {
                MessageBox.Show("Telegram未登录");
                return;
            }

            if (lstSchemes.SelectedItem == null)
            {
                MessageBox.Show("请选择左侧的一个方案模板！");
                return;
            }
            if (chkGroups.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择右侧的目标群组！");
                return;
            }

            var sourceScheme = lstSchemes.SelectedItem as SchemeModel;
            int count = 1;
            int successTotal = 0;

            btnConfirm.Enabled = false;
            btnConfirm.Text = "处理中...";

            try
            {
                // 仅遍历选中的项
                foreach (var item in chkGroups.CheckedItems)
                {
                    var targetGroup = item as TelegramGroupModel;
                    if (targetGroup == null) continue;

                    var json = System.Text.Json.JsonSerializer.Serialize(sourceScheme);
                    var newScheme = System.Text.Json.JsonSerializer.Deserialize<SchemeModel>(json);

                    newScheme.Id = Guid.NewGuid().ToString();
                    newScheme.Name = $"复制【{sourceScheme.Name}】{count}";
                    newScheme.TgGroupId = targetGroup.Id;
                    newScheme.TgGroupName = targetGroup.Name;
                    newScheme.IsEnabled = false;
                    newScheme.RealProfit = 0;
                    newScheme.SimulatedProfit = 0;

                    CacheData.Schemes.Add(newScheme);
                    count++;
                    successTotal++;
                }

                await SchemeFileHelper.SaveListAsync(CacheData.Schemes);

                MessageBox.Show($"成功！已生成 {successTotal} 个新方案。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误：" + ex.Message);
                btnConfirm.Enabled = true;
                btnConfirm.Text = "确认批量复制";
            }
        }

        // --- 3. UI 布局 (调整字体与搜索框) ---
        private void SetupUI()
        {
            this.Text = "批量复制方案";
            this.Size = new Size(900, 600); // 整体变大
            this.BackColor = Color.FromArgb(15, 20, 32);
            this.ForeColor = Color.White;
            this.StartPosition = FormStartPosition.CenterParent;
            // 【改动】全局字体调大
            this.Font = new Font("微软雅黑", 10.5F);

            // 底部面板
            pnlBottom = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(10, 15, 28),
                Padding = new Padding(15)
            };

            btnConfirm = new Button
            {
                Text = "确认批量复制",
                Dock = DockStyle.Right,
                Width = 160,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 170, 90),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("微软雅黑", 11F, FontStyle.Bold) // 按钮字体更大
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            pnlBottom.Controls.Add(btnConfirm);
            this.Controls.Add(pnlBottom);

            // 主布局
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F)); // 左侧稍窄
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F)); // 右侧稍宽（为了放搜索框）
            this.Controls.Add(mainLayout);
            mainLayout.BringToFront();

            // --- 左侧内容 ---
            Panel pnlLeftContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            lblTitleScheme = new Label
            {
                Text = "1. 选择方案模板",
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = Color.FromArgb(0, 190, 255),
                Font = new Font("微软雅黑", 11F, FontStyle.Bold)
            };

            lstSchemes = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(22, 28, 44),
                ForeColor = Color.White,
                ItemHeight = 38, // 行高增加
                DrawMode = DrawMode.OwnerDrawFixed
            };

            pnlLeftContainer.Controls.Add(lstSchemes);
            pnlLeftContainer.Controls.Add(lblTitleScheme);
            mainLayout.Controls.Add(pnlLeftContainer, 0, 0);

            // --- 右侧内容 ---
            Panel pnlRightContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            // 右侧顶部工具栏 (改用 Panel 组合)
            Panel pnlRightTools = new Panel { Dock = DockStyle.Top, Height = 40 }; // 高度增加

            lblTitleGroup = new Label
            {
                Text = "2. 目标群组",
                Dock = DockStyle.Left,
                Width = 100,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(0, 190, 255),
                Font = new Font("微软雅黑", 11F, FontStyle.Bold)
            };

            // 【新增】搜索框
            txtSearch = new TextBox
            {
                Dock = DockStyle.Fill, // 填充中间区域
                BackColor = Color.FromArgb(30, 35, 50),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("微软雅黑", 11F)
            };
            // 为了让 TextBox 垂直居中，套个 Panel 或者简单的 Margin 处理
            // 这里为了简单，直接加 padding
            Panel pnlSearchWrapper = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 4, 10, 0) };
            pnlSearchWrapper.Controls.Add(txtSearch);

            // 按钮区
            chkSelectAll = new CheckBox { Text = "全选", Dock = DockStyle.Right, Width = 65, Cursor = Cursors.Hand, Font = new Font("微软雅黑", 10F) };
            btnRefreshGroups = new Button
            {
                Text = "⟳",
                Dock = DockStyle.Right,
                Width = 45,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 50, 70),
                Cursor = Cursors.Hand
            };
            btnRefreshGroups.FlatAppearance.BorderSize = 0;

            pnlRightTools.Controls.Add(pnlSearchWrapper); // 中间：搜索
            pnlRightTools.Controls.Add(lblTitleGroup);    // 左边：标题
            pnlRightTools.Controls.Add(chkSelectAll);     // 右边：全选
            pnlRightTools.Controls.Add(btnRefreshGroups); // 右边：刷新

            chkGroups = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(22, 28, 44),
                ForeColor = Color.White,
                CheckOnClick = true
            };

            pnlRightContainer.Controls.Add(chkGroups);
            pnlRightContainer.Controls.Add(pnlRightTools);
            mainLayout.Controls.Add(pnlRightContainer, 1, 0);
        }

        private void SetupEvents()
        {
            lstSchemes.DisplayMember = "Name";
            chkGroups.DisplayMember = "Name";

            // 强制刷新：按钮点击时才去 API
            btnRefreshGroups.Click += async (s, e) => await FetchGroupsFromApi();

            btnConfirm.Click += btnConfirm_Click;

            // 全选
            chkSelectAll.CheckedChanged += (s, e) =>
            {
                bool check = chkSelectAll.Checked;
                for (int i = 0; i < chkGroups.Items.Count; i++) chkGroups.SetItemChecked(i, check);
            };

            // 【新增】搜索框逻辑：模糊查找
            txtSearch.TextChanged += (s, e) =>
            {
                string keyword = txtSearch.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(keyword))
                {
                    UpdateGroupListUI(_currentFullList);
                }
                else
                {
                    // 模糊匹配
                    var filtered = _currentFullList
                        .Where(g => g.Name.ToLower().Contains(keyword))
                        .ToList();
                    UpdateGroupListUI(filtered);
                }
            };

            // 自绘 ListBox
            lstSchemes.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                Color backColor = isSelected ? Color.FromArgb(0, 120, 200) : Color.FromArgb(22, 28, 44);
                using (SolidBrush bgBrush = new SolidBrush(backColor)) e.Graphics.FillRectangle(bgBrush, e.Bounds);

                var item = lstSchemes.Items[e.Index] as SchemeModel;
                string text = item != null ? item.Name : "null";

                // 调整文字位置，适应更大的行高
                TextRenderer.DrawText(e.Graphics, text, e.Font, new Point(e.Bounds.X + 8, e.Bounds.Y + 8), Color.White, TextFormatFlags.Left);
            };
        }
    }
}