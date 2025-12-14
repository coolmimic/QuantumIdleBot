using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace QuantumIdleDesktop.Models.Odds
{
    /// <summary>
    /// 每一行规则：触发金额 → 倍率
    /// </summary>
    public record MultiplyItem
    {
        /// <summary>
        /// 触发金额（单位：账户货币，如 100 表示 100 USDT）
        /// </summary>
        [JsonPropertyName("amount")]
        public decimal TriggerAmount { get; set; }

        /// <summary>
        /// 倍率（整数）
        /// </summary>
        [JsonPropertyName("multiplier")]
        public int Multiplier { get; set; }
    }

    /// <summary>
    /// 倍率模式
    /// </summary>
    public enum MultiplyMode
    {
        None = 0,  // 关闭
        
        Profit = 1,  // 盈利加倍
        Loss = 2   // 亏损加倍
    }
}
