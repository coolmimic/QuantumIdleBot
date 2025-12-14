using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace QuantumIdleDesktop.Models.DrawRules
{
    public class SlayDragonFollowDragonRuleConfig : DrawRuleConfigBase
    {
        public override DrawRuleType Type => DrawRuleType.SlayDragonFollowDragon;

        /// <summary>
        /// 所有监控规则（每条规则完全独立配置）
        /// </summary>
        public SlayDragonMonitorRule MonitorRule { get; set; } = new();
    }

    /// <summary>
    /// 单条监控规则（每条规则独立控制所有行为）← 核心！
    /// </summary>
    public class SlayDragonMonitorRule
    {
        /// <summary>
        /// 监控的标签（支持组合：大,单 / 龙 / 7）
        /// </summary>
        public string MonitorTags { get; set; } = string.Empty;

        /// <summary>
        /// 需要连续出现几期才触发
        /// </summary>
        public int RequiredConsecutiveCount { get; set; } = 1;

        /// <summary>
        /// 触发方式：每期都检查，还是满足条件后才开始投
        /// </summary>
        public TriggerMode TriggerMode { get; set; } = TriggerMode.CheckEveryIssue;

        /// <summary>
        /// 投注模式：正投 / 反投 / 固定号码
        /// </summary>
        public BetMode BetMode { get; set; } = BetMode.Follow;

        /// <summary>
        /// 触发后连投几期（仅当 TriggerMode = ContinueBet 时有效）
        /// </summary>
        public int ContinueBetCount { get; set; } = 1;

        /// <summary>
        /// 固定投注内容（仅当 BetMode = Fixed 时有效）
        /// </summary>
        public string FixedBetContent { get; set; } = string.Empty;


        /// <summary>
        /// 【新增】标记当前是否处于连投状态中
        /// </summary>
        [JsonIgnore]
        public bool IsBetting { get; set; } = false;


        /// <summary>
        /// 连投剩余期数
        /// </summary>
        [JsonIgnore]
        public int RemainingBetCount { get; set; } = 0;

        /// <summary>
        /// 锁定当前触发连投的那个标签（例如是"大"触发的，就锁定"大"，防止连投期间跳去投"单"）
        /// </summary>
        [JsonIgnore]
        public string LockedTargetTag { get; set; } = null;
    }

    /// <summary>
    /// 触发方式
    /// </summary>
    public enum TriggerMode
    {
        [Description("每期检查")] CheckEveryIssue,   // 每期都判断是否满足条件
        [Description("触发后连投")] ContinueBet       // 满足条件后，开始连投N期
    }

    /// <summary>
    /// 投注模式
    /// </summary>
    public enum BetMode
    {
        [Description("正投")] Follow,
        [Description("反投")] Reverse,
        [Description("固定号码")] Fixed
    }
}
