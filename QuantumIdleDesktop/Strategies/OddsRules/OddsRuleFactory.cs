using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Strategies.OddsRules
{
    public static class OddsRuleFactory
    {
        // 缓存策略实例 (单例模式)
        private static readonly Dictionary<OddsType, IOddsRule> _strategies = new Dictionary<OddsType, IOddsRule>
        {
            { OddsType.Linear, new LinearOddsRule() }
            // 后期如果要加 "斐波那契倍投"，在这里加一行即可
        };

        public static IOddsRule GetRule(OddsType type)
        {
            if (_strategies.TryGetValue(type, out var rule))
            {
                return rule;
            }
            // 默认返回固定倍率，防止空指针
            return _strategies[OddsType.Linear];
        }

    }
}
