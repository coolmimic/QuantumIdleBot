using System;
using System.ComponentModel;

namespace QuantumIdleDesktop.Models
{
    /// <summary>
    /// TG 账户状态视图模型，用于 DataGridView 绑定
    /// </summary>
    public class TgAccountVm
    {
        [DisplayName("TGid")]
        public long TgId { get; set; }

        [DisplayName("昵称")]
        public string Nickname { get; set; } = string.Empty;

        [DisplayName("状态")]
        public string Status { get; set; } = string.Empty; // 例如: "运行中", "已登录", "离线"

        [DisplayName("盈亏")]
        public decimal ProfitLoss { get; set; }

        [DisplayName("流水")]
        public decimal Turnover { get; set; }

        [DisplayName("模拟盈亏")]
        public decimal SimulatedProfitLoss { get; set; }

        [DisplayName("模拟流水")]
        public decimal SimulatedTurnover { get; set; }

        // 余额和今日盈亏按钮列不需要对应的字段，它们在 Designer 中作为 DataGridViewButtonColumn 实现。
    }
}