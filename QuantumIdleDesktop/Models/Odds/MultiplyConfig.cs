using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace QuantumIdleDesktop.Models.Odds
{
    /// <summary>
    /// 用户最终配置实体 —— 极简版，只留用户真正会填的字段
    /// </summary>
    public record MultiplyConfig
    {
        /// <summary>
        /// 选择的模式：不启用 / 盈利加倍 / 亏损加倍
        /// </summary>
        [JsonPropertyName("mode")]
        public MultiplyMode Mode { get; set; } = MultiplyMode.None;

        /// <summary>
        /// 默认倍率（没达到任何触发金额时用的倍率）
        /// </summary>
        [JsonPropertyName("defaultMultiplier")]
        public int DefaultMultiplier { get; set; } = 1;

        /// <summary>
        /// 用户添加的触发规则列表
        /// </summary>
        [JsonPropertyName("items")]
        public List<MultiplyItem> Items { get; set; } = new();
    }
}
