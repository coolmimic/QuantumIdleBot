using Microsoft.VisualBasic.Logging;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Utils;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace QuantumIdleDesktop.Views
{
    public partial class ViewOrderList : UserControl
    {
        private readonly Font _mainFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font _boldFont = new Font("Segoe UI", 10F, FontStyle.Bold);

        // 高亮颜色
        private readonly Color _colorWin = Color.FromArgb(255, 107, 107);   // 亮红
        private readonly Color _colorLoss = Color.FromArgb(81, 207, 102);  // 亮绿
        private readonly Color _colorWait = Color.FromArgb(252, 196, 25);   // 亮黄
        private readonly Color _colorNormal = Color.FromArgb(230, 230, 230);

        public ViewOrderList()
        {
            InitializeComponent();
            EnableDoubleBuffered(dgvOrders);
            SetupDataGridView();

            dgvOrders.CellValueNeeded += DgvOrders_CellValueNeeded;
            dgvOrders.CellFormatting += DgvOrders_CellFormatting;

            RefreshData();
        }

        private void SetupDataGridView()
        {
            dgvOrders.AutoGenerateColumns = false;
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None; // 彻底禁止自动调整
            dgvOrders.VirtualMode = true;
            dgvOrders.Columns.Clear();

            // =========================================================
            // 2. 视觉样式
            // =========================================================
            Color bgColor = Color.FromArgb(30, 35, 45);
            // 奇数行：保持您满意的斑马纹亮度
            Color altColor = Color.FromArgb(55, 65, 80);
            Color headerColor = Color.FromArgb(45, 50, 65);

            dgvOrders.BackgroundColor = bgColor;
            dgvOrders.BorderStyle = BorderStyle.None;
            dgvOrders.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvOrders.GridColor = Color.FromArgb(60, 65, 80);

            dgvOrders.DefaultCellStyle.BackColor = bgColor;
            dgvOrders.DefaultCellStyle.ForeColor = _colorNormal;
            dgvOrders.DefaultCellStyle.Font = _mainFont;
            dgvOrders.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 100, 160);
            dgvOrders.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvOrders.DefaultCellStyle.Padding = new Padding(5, 0, 5, 0);

            dgvOrders.AlternatingRowsDefaultCellStyle.BackColor = altColor;
            dgvOrders.AlternatingRowsDefaultCellStyle.ForeColor = _colorNormal;
            dgvOrders.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 100, 160);
            dgvOrders.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

            dgvOrders.EnableHeadersVisualStyles = false;
            dgvOrders.ColumnHeadersDefaultCellStyle.BackColor = headerColor;
            dgvOrders.ColumnHeadersDefaultCellStyle.ForeColor = Color.WhiteSmoke;
            dgvOrders.ColumnHeadersDefaultCellStyle.Font = _boldFont;
            dgvOrders.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // 全局表头居中，更协调
            dgvOrders.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgvOrders.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvOrders.ColumnHeadersHeight = 40;
            dgvOrders.RowTemplate.Height = 38;

            // =========================================================
            // 3. 列定义 (修复倍率显示)
            // =========================================================
            var cols = new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { HeaderText = "时间", Width = 75, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } },
                new DataGridViewTextBoxColumn { HeaderText = "期号", Width = 70, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } },

                new DataGridViewTextBoxColumn { 
                    HeaderText = "方案", Width = 110, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, DefaultCellStyle = 
                    { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell =
                    { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } },
                new DataGridViewTextBoxColumn {
                    HeaderText = "群名", Width = 130, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, 
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, 
                    HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } },

                new DataGridViewTextBoxColumn { HeaderText = "内容", Width = 60, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } },       
                
                // 倍率：修复为 60 (50太窄会显示省略号)，表头居中，数字靠右
                new DataGridViewTextBoxColumn {
                    HeaderText = "倍率",
                    Width = 60,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                    HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } // 表头居中
                },       
                
                // 金额：表头居中，数字靠右
                new DataGridViewTextBoxColumn {
                    HeaderText = "金额",
                    Width = 90,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                    HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } // 表头居中
                },

                new DataGridViewTextBoxColumn { HeaderText = "开奖", Width = 60, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }, HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } },       
                
                // 盈亏/状态：保持 Fill 填充，表头居中，内容靠右
                new DataGridViewTextBoxColumn {
                    HeaderText = "盈亏/状态",
                    Width = 160,
                    MinimumWidth = 160,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter },
                    HeaderCell = { Style = { Alignment = DataGridViewContentAlignment.MiddleCenter } } // 表头居中
                }
            };

            dgvOrders.Columns.AddRange(cols);

            dgvOrders.RowHeadersVisible = false;
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.AllowUserToResizeRows = false;
            dgvOrders.ReadOnly = true;
        }

        private void EnableDoubleBuffered(DataGridView dgv)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi?.SetValue(dgv, true, null);
        }

        public void RefreshData()
        {
            lock (CacheData.OrderLock)
            {
                int currentCount = CacheData.Orders.Count;
                if (dgvOrders.RowCount != currentCount)
                {
                    dgvOrders.RowCount = currentCount;
                }
            }
            dgvOrders.Invalidate();
        }

        private void DgvOrders_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            OrderModel order = null;
            lock (CacheData.OrderLock)
            {
                if (CacheData.Orders == null || CacheData.Orders.Count == 0) return;
                int realIndex = CacheData.Orders.Count - 1 - e.RowIndex;
                if (realIndex < 0 || realIndex >= CacheData.Orders.Count) return;
                order = CacheData.Orders[realIndex];
            }

            if (order == null) return;

            switch (e.ColumnIndex)
            {
                case 0: e.Value = order.BetTime.ToString("HH:mm:ss"); break;
                case 1: e.Value = FormatIssueNumber(order.IssueNumber); break;
                case 2: e.Value = order.SchemeName; break;
                case 3: e.Value = FormatGroupName(order.GroupName); break;
                case 4: e.Value = order.BetContent; break;
                case 5: e.Value = order.BetMultiplier.ToString("0.##"); break;
                case 6: e.Value = order.Amount.ToString("N2"); break;
                case 7: e.Value = FormatOpenResult(order.OpenResult); break;
                case 8: e.Value = GetProfitOrStatusText(order); break;
            }
        }


        public string FormatOpenResult(string OpenResult)
        {
            if (string.IsNullOrEmpty(OpenResult))
            {
                return "-";
            }
            else if (OpenResult.Contains("="))
            {
                return OpenResult.Split('=').ToList()[1];
            }
            return OpenResult;
        }


        private void DgvOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.Value == null) return;

            if (e.ColumnIndex == 8) // 盈亏/状态
            {
                string text = e.Value.ToString();
                if (text.Contains("中") || text.StartsWith("+"))
                {
                    e.CellStyle.ForeColor = _colorWin;
                    e.CellStyle.Font = _boldFont;
                }
                else if (text.Contains("挂") || text.StartsWith("-"))
                {
                    e.CellStyle.ForeColor = _colorLoss;
                }
                else if (text.Contains("平"))
                {
                    e.CellStyle.ForeColor = Color.DeepSkyBlue;
                }
                else if (text.Contains("待"))
                {
                    e.CellStyle.ForeColor = _colorWait;
                    e.CellStyle.Font = _boldFont;
                }
                else if (text.Contains("失败") || text.Contains("取消"))
                {
                    e.CellStyle.ForeColor = Color.Orange;
                }
            }
            else if (e.ColumnIndex == 6) // 金额
            {
                e.CellStyle.ForeColor = Color.White;
                e.CellStyle.Font = _boldFont;
            }
            else if (e.ColumnIndex == 4) // 内容
            {
                e.CellStyle.ForeColor = Color.FromArgb(200, 200, 255);
            }
        }

        private string FormatGroupName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return fullName;
            string name = fullName.Trim();
            var regex = new System.Text.RegularExpressions.Regex(@"^公群\s*\d+", System.Text.RegularExpressions.RegexOptions.Compiled);
            var match = regex.Match(name);
            return match.Success ? match.Value.Trim() : fullName;
        }

        private string FormatIssueNumber(string fullIssueNumber)
        {
            if (string.IsNullOrWhiteSpace(fullIssueNumber)) return string.Empty;
            if (fullIssueNumber.Length >= 6)
            {
                return fullIssueNumber.Substring(fullIssueNumber.Length - 4);
            }
            return fullIssueNumber;
        }

        private string GetProfitOrStatusText(OrderModel order)
        {
            string baseText = "";

            if (order.Status == OrderStatus.PendingSettlement)
                baseText = "⏳ 待开奖";
            else if (order.Status == OrderStatus.BetFailed)
                baseText = "❌ 失败";
            else if (order.Status == OrderStatus.Cancelled)
                baseText = "🚫 取消";
            else
            {
                decimal profit = order.PayoutAmount > 0 ? order.PayoutAmount : -order.Amount;
                if (profit > 0) baseText = $"+{profit:N2} (中)";
                else if (profit < 0) baseText = $"{profit:N2} (挂)";
                else baseText = "0 (平)";
            }

            if (!string.IsNullOrWhiteSpace(order.Remark))
            {
                baseText += $" | {order.Remark}";
            }

            return baseText;
        }
    }
}