using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Strategies.DrawRules
{
    public class DrawRuleFactory
    {
        private static readonly Dictionary<DrawRuleType, IDrawRule> _strategies = new Dictionary<DrawRuleType, IDrawRule>
        {
            { DrawRuleType.Fixed, new FixedNumberRule() },
            { DrawRuleType.FollowLast, new FollowLastRule() },
            { DrawRuleType.SlayDragonFollowDragon, new SlayDragonFollowDragonRule() },
            { DrawRuleType.NumberTrend, new NumberTrendRule()  },
            { DrawRuleType.PatternTrend,new PatternTrendRule()},
            { DrawRuleType.BranchTrend, new BranchTrendRule() },
            { DrawRuleType.ResultFollow,new ResultFollowRule() }
        };

        public static IDrawRule GetRule(DrawRuleType type)
        {
            if (_strategies.TryGetValue(type, out var rule))
            {
                return rule;
            }
            // 默认返回个空的，或者抛异常
            return new FixedNumberRule();
        }
    }
}
