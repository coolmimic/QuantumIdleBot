namespace QuantumIdleWEB.Strategies.DrawRules
{
    /// <summary>
    /// 出号规则类型
    /// </summary>
    public enum DrawRuleType
    {
        Fixed = 0,                    // 固定号码
        FollowLast = 1,               // 开某投某
        SlayDragonFollowDragon = 2,   // 斩龙跟龙
        NumberTrend = 3,              // 号码走势 (遗漏/连开)
        PatternTrend = 4,             // 01形态
        BranchTrend = 5,              // 分支走势
        ResultFollow = 6              // 结果跟随
    }

    /// <summary>
    /// 出号规则工厂
    /// </summary>
    public static class DrawRuleFactory
    {
        private static readonly Dictionary<DrawRuleType, IDrawRule> _rules = new()
        {
            { DrawRuleType.Fixed, new FixedNumberRule() },
            { DrawRuleType.FollowLast, new FollowLastRule() },
            { DrawRuleType.SlayDragonFollowDragon, new SlayDragonRule() },
            { DrawRuleType.NumberTrend, new NumberTrendRule() },
            { DrawRuleType.PatternTrend, new PatternTrendRule() },
            { DrawRuleType.BranchTrend, new BranchTrendRule() },
            { DrawRuleType.ResultFollow, new ResultFollowRule() },
        };

        public static IDrawRule GetRule(int type)
        {
            var ruleType = (DrawRuleType)type;
            return _rules.TryGetValue(ruleType, out var rule) ? rule : new FixedNumberRule();
        }
    }
}
