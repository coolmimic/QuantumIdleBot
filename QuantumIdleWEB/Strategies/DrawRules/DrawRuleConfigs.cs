using System.Text.Json.Serialization;

namespace QuantumIdleWEB.Strategies.DrawRules
{
    /// <summary>
    /// 出号规则配置基类
    /// </summary>
    public abstract class DrawRuleConfigBase
    {
        public abstract DrawRuleType Type { get; }
    }

    /// <summary>
    /// 触发方式
    /// </summary>
    public enum TriggerMode
    {
        CheckEveryIssue = 0,  // 每期检查
        ContinueBet = 1       // 触发后连投
    }

    /// <summary>
    /// 投注模式
    /// </summary>
    public enum BetMode
    {
        Follow = 0,   // 正投
        Reverse = 1,  // 反投
        Fixed = 2     // 固定号码
    }

    #region 固定号码规则配置

    /// <summary>
    /// 固定号码规则配置
    /// </summary>
    public class FixedNumberConfig
    {
        public List<string> BetNumbers { get; set; } = new();
    }

    #endregion

    #region 开某投某规则配置

    /// <summary>
    /// 开某投某规则配置
    /// Key=触发条件(如"大"), Value=下注内容列表(如["小"])
    /// </summary>
    public class FollowLastConfig
    {
        public Dictionary<string, List<string>> DrawRuleDic { get; set; } = new();
    }

    #endregion

    #region 斩龙跟龙规则配置

    /// <summary>
    /// 斩龙跟龙规则配置
    /// </summary>
    public class SlayDragonConfig
    {
        public SlayDragonMonitorRule MonitorRule { get; set; } = new();
    }

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
        /// 触发方式
        /// </summary>
        public TriggerMode TriggerMode { get; set; } = TriggerMode.CheckEveryIssue;

        /// <summary>
        /// 投注模式
        /// </summary>
        public BetMode BetMode { get; set; } = BetMode.Follow;

        /// <summary>
        /// 触发后连投几期
        /// </summary>
        public int ContinueBetCount { get; set; } = 1;

        /// <summary>
        /// 固定投注内容
        /// </summary>
        public string FixedBetContent { get; set; } = string.Empty;

        // 运行时状态
        [JsonIgnore] public bool IsBetting { get; set; } = false;
        [JsonIgnore] public int RemainingBetCount { get; set; } = 0;
        [JsonIgnore] public string? LockedTargetTag { get; set; } = null;
    }

    #endregion

    #region 号码走势规则配置

    /// <summary>
    /// 号码走势规则配置 (遗漏/连开)
    /// </summary>
    public class NumberTrendConfig
    {
        /// <summary>
        /// 需要监控的号码/标签（支持组合，用逗号分隔）
        /// </summary>
        public string MonitorNumbers { get; set; } = string.Empty;

        /// <summary>
        /// 是否全匹配模式 (AND vs OR)
        /// </summary>
        public bool IsFullMatch { get; set; } = false;

        /// <summary>
        /// 监控模式：True=遗漏, False=连开
        /// </summary>
        public bool IsOmissionMode { get; set; } = true;

        /// <summary>
        /// 触发阈值
        /// </summary>
        public int ThresholdCount { get; set; } = 10;

        /// <summary>
        /// 触发方式
        /// </summary>
        public TriggerMode TriggerMode { get; set; } = TriggerMode.CheckEveryIssue;

        /// <summary>
        /// 投注模式
        /// </summary>
        public BetMode BetMode { get; set; } = BetMode.Follow;

        /// <summary>
        /// 触发后连投期数
        /// </summary>
        public int ContinueBetCount { get; set; } = 1;

        /// <summary>
        /// 固定投注内容
        /// </summary>
        public string FixedBetContent { get; set; } = string.Empty;

        // 运行时状态
        [JsonIgnore] public bool IsBetting { get; set; } = false;
        [JsonIgnore] public int RemainingBetCount { get; set; } = 0;
        [JsonIgnore] public string? LockedTargetNumber { get; set; } = null;
    }

    #endregion

    #region 01形态规则配置

    /// <summary>
    /// 01形态规则配置
    /// </summary>
    public class PatternTrendConfig
    {
        public string CodeZeroDefinition { get; set; } = "大";
        public string CodeOneDefinition { get; set; } = "小";
        public List<TrendStrategyItem> StrategyList { get; set; } = new();

        [JsonIgnore]
        public bool IsAnyStrategyExecuting => StrategyList?.Any(x => x.IsExecuting) ?? false;
    }

    public class TrendStrategyItem
    {
        public string MonitorPattern { get; set; } = string.Empty;
        public string BetPattern { get; set; } = string.Empty;
        public bool StopOnWin { get; set; } = true;

        [JsonIgnore] public bool IsExecuting { get; set; }
        [JsonIgnore] public int CurrentStepIndex { get; set; }
    }

    #endregion

    #region 分支走势规则配置

    /// <summary>
    /// 分支走势规则配置
    /// </summary>
    public class BranchTrendConfig
    {
        public string CodeZeroDefinition { get; set; } = "大";
        public string CodeOneDefinition { get; set; } = "小";
        public string MonitorPattern { get; set; } = "0001111";
        public string InitialBet { get; set; } = "0";
        public string LossPattern { get; set; } = "000001111";
        public string WinPattern { get; set; } = "1110000";
        public bool StopOnWin { get; set; } = true;

        // 运行时状态
        [JsonIgnore] public bool IsBetting { get; set; } = false;
        [JsonIgnore] public BranchBettingState CurrentState { get; set; } = BranchBettingState.None;
        [JsonIgnore] public int CurrentStepIndex { get; set; } = 0;
    }

    public enum BranchBettingState
    {
        None,
        Initial,
        WinSequence,
        LossSequence
    }

    #endregion

    #region 结果跟随规则配置

    /// <summary>
    /// 结果跟随规则配置
    /// </summary>
    public class ResultFollowConfig
    {
        public string CodeZeroDefinition { get; set; } = "大";
        public string CodeOneDefinition { get; set; } = "小";
        public string SequenceOnZero { get; set; } = "000111";
        public string SequenceOnOne { get; set; } = "111000";
        public bool StopOnWin { get; set; } = true;

        // 运行时状态
        [JsonIgnore] public bool IsBetting { get; set; } = false;
        [JsonIgnore] public ResultFollowState CurrentState { get; set; } = ResultFollowState.None;
        [JsonIgnore] public int CurrentStepIndex { get; set; } = 0;
    }

    public enum ResultFollowState
    {
        None,
        FollowingZero,
        FollowingOne
    }

    #endregion
}
