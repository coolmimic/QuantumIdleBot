using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QuantumIdleDesktop.Models
{
    // ===========================
    // 1. 枚举定义
    // ===========================

    /// <summary>
    /// 资金/倍率管理类型
    /// </summary>
    public enum OddsType
    {
        /// <summary>
        /// 直线倍率 (固定序列/阶梯)
        /// 含义：按照预设的金额列表顺序投注，如 [10, 20, 50, 100]
        /// </summary>
        [Description("直线倍率")]
        Linear
    }

    /// <summary>
    /// 出号/下注规则类型
    /// </summary>
    public enum DrawRuleType
    {
        /// <summary>
        /// 固定号码 (死跟)
        /// 含义：永远只买设定的那个号码/选项
        /// </summary>
        [Description("固定号码")]
        Fixed,
        [Description("开某投某")]
        FollowLast,
        [Description("斩龙跟龙")]
        SlayDragonFollowDragon,
        [Description("号码走势")]
        NumberTrend,
        [Description("01形态")]
        PatternTrend,
        [Description("分支走势")]
        BranchTrend,
        [Description("结果跟随")]
        ResultFollow

    }
}
