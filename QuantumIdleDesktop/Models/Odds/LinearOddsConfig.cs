using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace QuantumIdleDesktop.Models.Odds
{
    /// <summary>
    /// 直线倍率配置 (具体实现)
    /// </summary>
    public class LinearOddsConfig : OddsConfigBase
    {
        // 重写类型为 Linear
        public override OddsType Type => OddsType.Linear;

        /// <summary>
        /// 必填：倍投序列 (例如: 10, 20, 50, 100)
        /// </summary>
        public List<int> Sequence { get; set; } = new List<int>();

        /// <summary>
        /// 当前进行到序列的第几个下标 (0代表第1关)
        /// [JsonIgnore] 标记告诉序列化器：保存文件时跳过我！
        /// </summary>
        [JsonIgnore]
        public int CurrentIndex { get; set; } = 0;

    }
}
