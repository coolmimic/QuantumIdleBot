using QuantumIdleDesktop.Models.Odds;
using QuantumIdleDesktop.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Forms
{
    public partial class MultiplyStrategyForm : Form
    {
        // 临时数据绑定源，用于界面操作，点击保存后再写回 CacheData
        private readonly BindingList<MultiplyItem> _tempItems;
        private readonly BindingSource _bindingSource;

        public MultiplyStrategyForm()
        {
            InitializeComponent(); // 初始化基础窗体属性
            InitializeModernLayout(); // 初始化现代化布局

            // 1. 直接读取 CacheData.Settings.MultiplyConfig
            var sourceConfig = CacheData.Settings.MultiplyConfig;

            // 2. 创建临时列表用于绑定（防止未点保存就修改了原始数据引用）
            // 注意：这里做了一个浅拷贝列表，如果Item是引用类型且需修改属性，可能需要更深层的拷贝
            // 但对于当前的 int/decimal 结构，重新生成 List 足够
            _tempItems = new BindingList<MultiplyItem>(sourceConfig.Items.ToList());
            _bindingSource = new BindingSource { DataSource = _tempItems };

            // 初始值加载
            LoadConfigToUI(sourceConfig);


            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void LoadConfigToUI(MultiplyConfig config)
        {
            switch (config.Mode)
            {
                case MultiplyMode.Profit: rbProfit.Checked = true; break;
                case MultiplyMode.Loss: rbLoss.Checked = true; break;
                default: rbNone.Checked = true; break;
            }

            nudDefaultMultiplier.Value = Math.Clamp(config.DefaultMultiplier, nudDefaultMultiplier.Minimum, nudDefaultMultiplier.Maximum);
            dgvItems.DataSource = _bindingSource;

            UpdateUiState();
        }

        // ==================== 现代化 UI 组件声明 ====================
        private RadioButton rbNone;
        private RadioButton rbProfit;
        private RadioButton rbLoss;
        private NumericUpDown nudDefaultMultiplier;
        private DataGridView dgvItems;
        private Button btnAdd;
        private Button btnSave;
        private Button btnCancel;
        private Panel panelRules; // 用于控制启用/禁用的容器


        private NumericUpDown nudInputAmount;
        private NumericUpDown nudInputMultiplier;

        // ==================== 事件处理 ====================

        private void Mode_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUiState();
        }

        private void UpdateUiState()
        {
            // 只有在非 None 模式下才启用规则编辑
            bool enableRules = !rbNone.Checked;
            panelRules.Enabled = enableRules;

            // 视觉反馈：禁用时稍微变灰
            dgvItems.DefaultCellStyle.ForeColor = enableRules ? Color.Black : Color.Gray;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            var amount = (int)nudInputAmount.Value;
            var mult = nudInputMultiplier.Value;

            // 简单校验：查重
            if (_tempItems.Any(x => x.TriggerAmount == amount))
            {
                MessageBox.Show($"金额 {amount} 的规则已存在，请先删除旧规则或修改金额。", "重复规则", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 添加到列表
            _tempItems.Add(new MultiplyItem { TriggerAmount = amount, Multiplier = (int)mult });

            // 排序（可选：如果你希望添加后自动按金额排序）
            // 注意：BindingList 不支持直接 Sort，通常需要重新绑定或在保存时排序。
            // 这里为了视觉反馈，我们暂时仅仅是追加到末尾，保存时会统一排序。

            // 滚动到底部
            if (dgvItems.Rows.Count > 0)
                dgvItems.FirstDisplayedScrollingRowIndex = dgvItems.Rows.Count - 1;
        }

        private void DgvItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // 处理删除按钮 (现在的删除按钮是最后一列)
            if (dgvItems.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                // 也可以加个确认框
                // if (MessageBox.Show("删除此规则？", "确认", MessageBoxButtons.YesNo) != DialogResult.Yes) return;

                _tempItems.RemoveAt(e.RowIndex);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // 校验
            if (!rbNone.Checked)
            {
                if (_tempItems.Any(x => x.TriggerAmount <= 0))
                {
                    MessageBox.Show("所有触发金额必须大于 0", "配置错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // 1. 更新 CacheData
            var config = CacheData.Settings.MultiplyConfig;



            if (rbNone.Checked) config.Mode = MultiplyMode.None;
            else if (rbProfit.Checked) config.Mode = MultiplyMode.Profit;
            else config.Mode = MultiplyMode.Loss;

            config.DefaultMultiplier = (int)nudDefaultMultiplier.Value;

            // 保存列表并排序 (金额小的在前)
            config.Items = _tempItems.OrderBy(x => x.TriggerAmount).ToList();

            // 2. 保存到文件 (按要求)
            try
            {
                JsonHelper.Save("Data\\Settings.json", CacheData.Settings);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ==================== 界面构建与美化 (重构核心) ====================

        private void InitializeModernLayout()
        {
            // --- 窗体基础设置 ---
            this.Text = "全局倍率设置";
            this.Size = new Size(650, 700); // 稍微宽一点
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(25)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F)); // 标题
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // 模式
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // 规则区域
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // 底部按钮

            // 1. 标题
            var lblTitle = new Label
            {
                Text = "全局倍率设置 / Global Settings",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            mainLayout.Controls.Add(lblTitle, 0, 0);

            // 2. 模式选择
            var flowMode = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 10, 0, 0)
            };
            rbNone = CreateStyledRadioButton("禁用");
            rbProfit = CreateStyledRadioButton("盈利加倍");
            rbLoss = CreateStyledRadioButton("亏损加倍");

            // 重新绑定事件
            rbNone.CheckedChanged += Mode_CheckedChanged;
            rbProfit.CheckedChanged += Mode_CheckedChanged;
            rbLoss.CheckedChanged += Mode_CheckedChanged;

            flowMode.Controls.AddRange(new Control[] { rbNone, rbProfit, rbLoss });
            mainLayout.Controls.Add(flowMode, 0, 1);

            // 3. 规则区域容器
            panelRules = new Panel { Dock = DockStyle.Fill };
            var grpLayout = new GroupBox
            {
                Text = " 规则配置 (Rule Config) ", // 加空格美观一点
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            var rulesInnerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            rulesInnerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F)); // 工具栏增高，放输入框
            rulesInnerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // ==================== 修改重点：顶部输入工具栏 ====================
            var topBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };

            // 全局默认倍率
            var lblDef = new Label { Text = "默认倍率:", AutoSize = true, Margin = new Padding(0, 8, 5, 0) };
            nudDefaultMultiplier = new NumericUpDown { Minimum = 1, Maximum = 999, Width = 60, Margin = new Padding(0, 5, 30, 0) };

            // 分割线（视觉上的）
            var lblSep = new Label { Text = "|", AutoSize = true, ForeColor = Color.LightGray, Margin = new Padding(0, 8, 30, 0) };

            // 新增：触发金额输入
            var lblAmt = new Label { Text = "触发金额 >=", AutoSize = true, Margin = new Padding(0, 8, 5, 0) };
            nudInputAmount = new NumericUpDown { Minimum = 1, Maximum = 99999999, Width = 80, Value = 100, Margin = new Padding(0, 5, 15, 0) };

            // 新增：倍率输入
            var lblMul = new Label { Text = "倍率:", AutoSize = true, Margin = new Padding(0, 8, 5, 0) };
            nudInputMultiplier = new NumericUpDown { Minimum = 0, Maximum = 999, DecimalPlaces = 1, Width = 60, Value = 2, Margin = new Padding(0, 5, 15, 0) };

            // 修改：添加按钮
            btnAdd = new Button
            {
                Text = "添加规则",
                Width = 90,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 3, 0, 0)
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;

            // 将所有控件加入 TopBar
            topBar.Controls.AddRange(new Control[] {
                lblDef, nudDefaultMultiplier,
                lblSep,
                lblAmt, nudInputAmount,
                lblMul, nudInputMultiplier,
                btnAdd
            });

            rulesInnerLayout.Controls.Add(topBar, 0, 0);
            // ==================== 结束输入工具栏修改 ====================

            // DataGridView 设置 (设为只读)
            dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                ReadOnly = true, // 【关键修改】表格变为只读，只能通过上方添加，点删除按钮移除
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 45,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowTemplate = { Height = 40 }
            };

            // 样式设置
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(73, 80, 87);
            dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            dgvItems.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgvItems.ColumnHeadersDefaultCellStyle.Padding = new Padding(15, 0, 0, 0);
            dgvItems.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            dgvItems.DefaultCellStyle.BackColor = Color.White;
            dgvItems.DefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
            dgvItems.DefaultCellStyle.SelectionBackColor = Color.FromArgb(231, 241, 255);
            dgvItems.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvItems.DefaultCellStyle.Padding = new Padding(15, 0, 0, 0);
            dgvItems.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
            dgvItems.GridColor = Color.FromArgb(233, 236, 239);

            // 列定义
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(MultiplyItem.TriggerAmount),
                HeaderText = "触发金额 (Amount)",
                Width = 200
            });
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(MultiplyItem.Multiplier),
                HeaderText = "倍率 (Multiplier)",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // 删除按钮列
            var btnCol = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Text = "删除",
                UseColumnTextForButtonValue = true,
                Width = 80,
                FlatStyle = FlatStyle.Flat
            };
            btnCol.DefaultCellStyle.ForeColor = Color.Crimson;
            btnCol.DefaultCellStyle.SelectionForeColor = Color.Crimson;
            dgvItems.Columns.Add(btnCol);

            dgvItems.CellClick += DgvItems_CellClick;

            rulesInnerLayout.Controls.Add(dgvItems, 0, 1);
            grpLayout.Controls.Add(rulesInnerLayout);
            panelRules.Controls.Add(grpLayout);
            mainLayout.Controls.Add(panelRules, 0, 2);

            // 4. 底部按钮
            var footerPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };
            btnCancel = CreateStyledButton("取消", Color.White, Color.FromArgb(108, 117, 125), true);
            btnSave = CreateStyledButton("保存配置", Color.FromArgb(40, 167, 69), Color.White, false);

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;

            footerPanel.Controls.Add(btnCancel);
            footerPanel.Controls.Add(btnSave);
            mainLayout.Controls.Add(footerPanel, 0, 3);

            this.Controls.Add(mainLayout);
        }

        // 辅助方法：创建样式统一的单选框
        private RadioButton CreateStyledRadioButton(string text)
        {
            return new RadioButton
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(0, 0, 20, 0),
                Cursor = Cursors.Hand
            };
        }

        // 辅助方法：创建样式统一的按钮
        private Button CreateStyledButton(string text, Color backColor, Color foreColor, bool hasBorder)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(100, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Cursor = Cursors.Hand,
                Margin = new Padding(10, 0, 0, 0)
            };
            if (!hasBorder) btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}