using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Models.DrawRules
{
    public class FollowLastDrawRuleConfig : DrawRuleConfigBase
    {
        public override DrawRuleType Type => DrawRuleType.FollowLast;

        public Dictionary<string, List<string>> DrawRuleDic { get; set; } = new Dictionary<string, List<string>>();
    }
}
