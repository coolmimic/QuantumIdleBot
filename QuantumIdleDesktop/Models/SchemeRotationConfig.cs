using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QuantumIdleDesktop.Models
{
    public class SchemeRotationConfig
    {
        // 核心逻辑：使用 ID
        public string SourceSchemeId { get; set; }
        public string TargetSchemeId { get; set; }

        // 界面显示：保留名称方便查看 (在添加时记录快照)
        public string SourceSchemeName { get; set; }
        public string TargetSchemeName { get; set; }

        public RotationConditionType ConditionType { get; set; }
        public decimal ThresholdValue { get; set; }
    }
    public enum RotationConditionType
    {
        [Description("止盈")]
        TakeProfit,
        [Description("止损")]
        StopLoss
    }
}
