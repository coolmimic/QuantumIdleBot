using QuantumIdleDesktop.GameCore;
using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Models.DrawRules;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Strategies.DrawRules
{
    public class FixedNumberRule : IDrawRule
    {
        public List<string> GetNextBet(SchemeModel scheme, GroupGameContext context)
        {
            if (scheme.DrawRuleConfig is FixedNumberDrawRuleConfig fiexd)
            {
                return fiexd.TargetNumbers;
            }
            return null;
        }
    }
}
