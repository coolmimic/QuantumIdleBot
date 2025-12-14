using System;
using System.Text.Json.Serialization;

namespace QuantumIdleDesktop.Models.DrawRules
{
    /// <summary>
    /// 号码走势规则配置 (包含：遗漏/冷号、连开/热号)
    /// </summary>
    public class NumberTrendRuleConfig : DrawRuleConfigBase
    {
        public override DrawRuleType Type => DrawRuleType.NumberTrend;

        /// <summary>
        /// 需要监控的号码/标签（支持组合，用逗号分隔）
        /// 例如："大,双" 或 "1,2,3,4,5"
        /// </summary>
        public string MonitorNumbers { get; set; } = string.Empty;

        /// <summary>
        /// 【新增】触发逻辑是否需要全匹配 (AND vs OR)
        /// True = 严苛模式：必须 MonitorNumbers 里所有项都同时达到阈值才触发 (逻辑与 AND)
        /// False = 宽松模式：只要 MonitorNumbers 里任意一项达到阈值就触发 (逻辑或 OR)
        /// </summary>
        public bool IsFullMatch { get; set; } = false;

        /// <summary>
        /// 监控模式
        /// True = 监控遗漏 (比如 N 期没开)
        /// False = 监控连开 (比如连续开了 N 期)
        /// </summary>
        public bool IsOmissionMode { get; set; } = true;

        /// <summary>
        /// 触发阈值 (遗漏期数 或 连开期数)
        /// </summary>
        public int ThresholdCount { get; set; } = 10;

        /// <summary>
        /// 触发方式：每期检查 / 触发后连投
        /// </summary>
        public TriggerMode TriggerMode { get; set; } = TriggerMode.CheckEveryIssue;

        /// <summary>
        /// 投注模式：正投 / 反投 / 固定号码
        /// </summary>
        public BetMode BetMode { get; set; } = BetMode.Follow;

        /// <summary>
        /// 触发后连投期数
        /// </summary>
        public int ContinueBetCount { get; set; } = 1;

        /// <summary>
        /// 固定投注内容 (BetMode=Fixed时有效)
        /// </summary>
        public string FixedBetContent { get; set; } = string.Empty;

        #region 运行时状态 (JsonIgnore)

        /// <summary>
        /// 标记当前是否处于连投状态中
        /// </summary>
        [JsonIgnore]
        public bool IsBetting { get; set; } = false;

        /// <summary>
        /// 连投剩余期数
        /// </summary>
        [JsonIgnore]
        public int RemainingBetCount { get; set; } = 0;

        /// <summary>
        /// 锁定当前触发的那个(或那组)号码
        /// 注意：
        /// 1. 若 IsFullMatch=False，这里存储触发的那一个号码 (如 "大")
        /// 2. 若 IsFullMatch=True，这里建议存储组合字符串 (如 "大,双")，具体看你业务逻辑怎么投
        /// </summary>
        [JsonIgnore]
        public string LockedTargetNumber { get; set; } = null;

        #endregion
    }
}