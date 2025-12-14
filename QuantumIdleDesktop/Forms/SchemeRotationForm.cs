using QuantumIdleDesktop.Models; // 引用你的实体类
using QuantumIdleDesktop.Utils;  // 引用工具类
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Forms
{
    public partial class SchemeRotationForm : Form
    {
        // ==========================================
        // 1. 控件定义
        // ==========================================
        private Panel pnlInput;
        private Panel pnlList;

        private ComboBox cmbSourceScheme, cmbTargetScheme, cmbCondition;
        private Button btnAdd;
        private DataGridView dgvRotationRules;
        private ContextMenuStrip ctxMenu;

        // ==========================================
        // 2. 配色方案 (Dark Theme)
        // ==========================================
        private readonly Color _mainBg = Color.FromArgb(30, 30, 30);
        private readonly Color _cardBg = Color.FromArgb(45, 45, 48);
        private readonly Color _borderColor = Color.FromArgb(60, 60, 60);
        private readonly Color _inputBg = Color.FromArgb(60, 60, 60);
        private readonly Color _accentColor = Color.FromArgb(0, 122, 204);
        private readonly Color _textPrimary = Color.FromArgb(220, 220, 220);
        private readonly Color _textSecondary = Color.FromArgb(160, 160, 160);
        private readonly Color _dangerColor = Color.FromArgb(220, 80, 80); // 删除按钮红色

        // ==========================================
        // 3. 数据源
        // ==========================================
        private BindingList<SchemeRotationConfig> _rotationRules;

        public SchemeRotationForm()
        {
            InitializeCustomUI();
            InitializeData();
        }

        // ==========================================
        // 4. UI 构建
        // ==========================================
        private void InitializeCustomUI()
        {
            // 窗体设置
            this.Text = "方案轮换策略配置";
            this.Size = new Size(960, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Font = new Font("Microsoft YaHei UI", 9F);
            this.BackColor = _mainBg;
            this.ForeColor = _textPrimary;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // --- 顶部输入卡片 ---
            pnlInput = CreateCardPanel(new Point(20, 20), new Size(905, 110));

            var lblTitle = new Label { Text = "添加新规则", Location = new Point(20, 15), AutoSize = true, Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold), ForeColor = _textPrimary };
            pnlInput.Controls.Add(lblTitle);

            int yPos = 55;

            AddLabel(pnlInput, "当前方案", 30, yPos - 22);
            cmbSourceScheme = CreateDarkComboBox(30, yPos, 220);

            AddLabel(pnlInput, "触发条件", 280, yPos - 22);
            cmbCondition = CreateDarkComboBox(280, yPos, 160);

            var lblArrow = new Label { Text = "➜", Location = new Point(470, yPos), AutoSize = true, ForeColor = _accentColor, Font = new Font("Segoe UI", 12F, FontStyle.Bold), BackColor = Color.Transparent };
            pnlInput.Controls.Add(lblArrow);

            AddLabel(pnlInput, "切换至目标方案", 520, yPos - 22);
            cmbTargetScheme = CreateDarkComboBox(520, yPos, 220);

            btnAdd = CreateFlatButton("+ 添加规则", new Point(780, yPos - 1), new Size(100, 32), _accentColor);
            btnAdd.Click += BtnAdd_Click;

            pnlInput.Controls.AddRange(new Control[] { cmbSourceScheme, cmbCondition, cmbTargetScheme, btnAdd });


            // --- 底部列表卡片 ---
            pnlList = CreateCardPanel(new Point(20, 150), new Size(905, 390));

            var lblListTitle = new Label { Text = "策略队列", Location = new Point(20, 15), AutoSize = true, Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold), ForeColor = _textPrimary };

            // Grid 初始化
            dgvRotationRules = new DataGridView();
            StyleDataGridView(dgvRotationRules);
            dgvRotationRules.Location = new Point(2, 50);
            dgvRotationRules.Size = new Size(901, 338);
            dgvRotationRules.Dock = DockStyle.None;
            dgvRotationRules.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            dgvRotationRules.AutoGenerateColumns = false;

            // 事件绑定
            dgvRotationRules.CellFormatting += DgvRotationRules_CellFormatting; // 处理枚举显示中文
            dgvRotationRules.CellContentClick += DgvRotationRules_CellContentClick; // 处理删除按钮点击

            // 右键菜单 (保留作为备用)
            ctxMenu = new ContextMenuStrip();
            ToolStripMenuItem delItem = new ToolStripMenuItem("删除选中规则");
            delItem.Click += (s, e) => DeleteSelectedRule();
            ctxMenu.Items.Add(delItem);
            dgvRotationRules.ContextMenuStrip = ctxMenu;

            pnlList.Controls.Add(lblListTitle);
            pnlList.Controls.Add(dgvRotationRules);

            this.Controls.Add(pnlInput);
            this.Controls.Add(pnlList);
        }

        // ==========================================
        // 5. 数据逻辑
        // ==========================================
        private void InitializeData()
        {
            if (CacheData.Settings.SchemeRotations == null)
                CacheData.Settings.SchemeRotations = new List<SchemeRotationConfig>();

            _rotationRules = new BindingList<SchemeRotationConfig>(CacheData.Settings.SchemeRotations);
            dgvRotationRules.DataSource = _rotationRules;

            // 1. 当前方案列
            dgvRotationRules.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SourceSchemeName",
                HeaderText = "当前方案",
                FillWeight = 30,
                SortMode = DataGridViewColumnSortMode.NotSortable // 禁止排序
            });

            // 2. 触发条件列 (绑定 Enum)
            dgvRotationRules.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ConditionType",
                HeaderText = "触发条件",
                FillWeight = 25,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });

            // 3. 目标方案列
            dgvRotationRules.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TargetSchemeName",
                HeaderText = "目标方案",
                FillWeight = 30,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });

            // 4. 操作列 (删除按钮)
            var btnCol = new DataGridViewButtonColumn
            {
                HeaderText = "操作",
                Text = "删除",
                UseColumnTextForButtonValue = true, // 让按钮显示 Text 属性的值
                FillWeight = 15,
                FlatStyle = FlatStyle.Flat,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            // 设置按钮单元格样式 (红色字)
            btnCol.DefaultCellStyle.ForeColor = _dangerColor;
            btnCol.DefaultCellStyle.SelectionForeColor = _dangerColor;
            dgvRotationRules.Columns.Add(btnCol);

            LoadComboBoxes();
        }

        // 处理 DataGridView 格式化 (Enum 转 Description)
        private void DgvRotationRules_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // 确保是 "ConditionType" 列 (根据列索引或列名判断，这里假设它是第2列，Index=1)
            // 更稳健的方式是判断列绑定的属性名
            if (dgvRotationRules.Columns[e.ColumnIndex].DataPropertyName == "ConditionType" && e.Value != null)
            {
                if (e.Value is Enum enumValue)
                {
                    e.Value = GetEnumDescription(enumValue); // 显示中文描述
                    e.FormattingApplied = true; // 告诉 DGV 我们已经处理了格式化
                }
            }
        }

        // 处理 DataGridView 按钮点击
        private void DgvRotationRules_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 确保点击的是按钮列，且不是表头
            if (e.RowIndex >= 0 && dgvRotationRules.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                var item = dgvRotationRules.Rows[e.RowIndex].DataBoundItem as SchemeRotationConfig;
                if (item != null)
                {
                    if (MessageBox.Show($"确定要删除从 [{item.SourceSchemeName}] 到 [{item.TargetSchemeName}] 的规则吗?",
                        "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        _rotationRules.Remove(item);
                        SaveData();
                    }
                }
            }
        }

        private void LoadComboBoxes()
        {
            if (CacheData.Schemes == null) return;
            var listSource = new List<SchemeModel>(CacheData.Schemes);
            var listTarget = new List<SchemeModel>(CacheData.Schemes);

            cmbSourceScheme.DataSource = listSource;
            cmbSourceScheme.DisplayMember = "Name";
            cmbSourceScheme.ValueMember = "Id";

            cmbTargetScheme.DataSource = listTarget;
            cmbTargetScheme.DisplayMember = "Name";
            cmbTargetScheme.ValueMember = "Id";

            var conditions = Enum.GetValues(typeof(RotationConditionType))
                .Cast<RotationConditionType>()
                .Select(e => new { Name = GetEnumDescription(e), Value = e })
                .ToList();

            cmbCondition.DataSource = conditions;
            cmbCondition.DisplayMember = "Name";
            cmbCondition.ValueMember = "Value";
        }

        private string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbSourceScheme.SelectedIndex < 0 || cmbTargetScheme.SelectedIndex < 0) return;

            string sId = cmbSourceScheme.SelectedValue.ToString();
            string tId = cmbTargetScheme.SelectedValue.ToString();
            string sName = ((SchemeModel)cmbSourceScheme.SelectedItem).Name;
            string tName = ((SchemeModel)cmbTargetScheme.SelectedItem).Name;
            var cond = (RotationConditionType)cmbCondition.SelectedValue;

            if (sId == tId)
            {
                MessageBox.Show("源方案和目标方案不能相同！", "逻辑错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool exists = _rotationRules.Any(r => r.SourceSchemeId == sId && r.TargetSchemeId == tId && r.ConditionType == cond);
            if (exists)
            {
                MessageBox.Show("该规则已存在。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _rotationRules.Add(new SchemeRotationConfig
            {
                SourceSchemeId = sId,
                SourceSchemeName = sName,
                TargetSchemeId = tId,
                TargetSchemeName = tName,
                ConditionType = cond
            });

            SaveData();
        }

        private void DeleteSelectedRule()
        {
            // 保留此方法供右键菜单使用
            if (dgvRotationRules.SelectedRows.Count > 0)
            {
                var item = dgvRotationRules.SelectedRows[0].DataBoundItem as SchemeRotationConfig;
                if (item != null)
                {
                    _rotationRules.Remove(item);
                    SaveData();
                }
            }
        }

        private void SaveData()
        {
            try { JsonHelper.Save("Data\\Settings.json", CacheData.Settings); }
            catch (Exception ex) { MessageBox.Show($"保存失败: {ex.Message}"); }
        }

        // ==========================================
        // 6. 样式工厂
        // ==========================================

        private Panel CreateCardPanel(Point loc, Size size)
        {
            var p = new Panel
            {
                Location = loc,
                Size = size,
                BackColor = _cardBg,
                Padding = new Padding(0)
            };
            var bar = new Panel { Dock = DockStyle.Left, Width = 3, BackColor = _accentColor };
            p.Paint += (s, e) => { e.Graphics.DrawRectangle(new Pen(_borderColor), 0, 0, p.Width - 1, p.Height - 1); };
            p.Controls.Add(bar);
            return p;
        }

        private void AddLabel(Panel p, string text, int x, int y)
        {
            var lbl = new Label { Text = text, Location = new Point(x, y), AutoSize = true, ForeColor = _textSecondary, Font = new Font("Microsoft YaHei UI", 9F) };
            p.Controls.Add(lbl);
        }

        private ComboBox CreateDarkComboBox(int x, int y, int w)
        {
            return new ComboBox
            {
                Location = new Point(x, y),
                Width = w,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = _inputBg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft YaHei UI", 9F)
            };
        }

        private Button CreateFlatButton(string text, Point loc, Size size, Color bg)
        {
            var btn = new Button
            {
                Text = text,
                Location = loc,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = Color.White,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = _cardBg;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.EnableHeadersVisualStyles = false;
            dgv.GridColor = Color.FromArgb(60, 60, 60);
            dgv.RowHeadersVisible = false;

            // 3. 彻底锁定表格交互
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly = true;                 // 禁止编辑
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToOrderColumns = false; // 禁止拖拽列
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 40);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = _textPrimary;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(40, 40, 40);
            dgv.ColumnHeadersHeight = 40;

            dgv.DefaultCellStyle.BackColor = _cardBg;
            dgv.DefaultCellStyle.ForeColor = _textPrimary;
            dgv.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9.5F);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 80, 140);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.RowTemplate.Height = 35;
        }
    }
}