using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Models.DrawRules
{
    public class FixedNumberDrawRuleConfig : DrawRuleConfigBase
    {
        public override DrawRuleType Type => DrawRuleType.Fixed;
        /// <summary>
        /// 核心数据：要死跟的目标
        /// </summary>
        public List<string> TargetNumbers { get; set; } = new List<string>();
    }
}
