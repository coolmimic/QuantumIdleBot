using System;
using System.Text.Json.Serialization;

namespace QuantumIdleDesktop.Models.DrawRules
{
    /// <summary>
    /// 结果跟随规则配置 (ResultFollowRule)
    /// 特性：根据上期开奖结果(0或1)，触发对应的投注序列。
    /// </summary>
    public class ResultFollowRuleConfig : DrawRuleConfigBase
    {
        public override DrawRuleType Type => DrawRuleType.ResultFollow;

        #region 1. 基础定义 (0和1代表什么)

        /// <summary>
        /// 定义代码 "0" 的实际含义
        /// 例如："大"、"单"、"1"
        /// </summary>
        public string CodeZeroDefinition { get; set; } = "大";

        /// <summary>
        /// 定义代码 "1" 的实际含义
        /// 例如："小"、"双"、"2"
        /// </summary>
        public string CodeOneDefinition { get; set; } = "小";

        #endregion

        #region 2. 触发与序列配置

        /// <summary>
        /// 【开0（大）触发的序列】
        /// 当上期开奖结果为 "0" 时，启动此投注序列。
        /// 留空则表示开 "0" 不进行投注。
        /// 示例："000111" (即：大大小小...)
        /// </summary>
        public string SequenceOnZero { get; set; } = "000111";

        /// <summary>
        /// 【开1（小）触发的序列】
        /// 当上期开奖结果为 "1" 时，启动此投注序列。
        /// 留空则表示开 "1" 不进行投注。
        /// 示例："111000" (即：小小大大...)
        /// </summary>
        public string SequenceOnOne { get; set; } = "111000";

        #endregion

        #region 3. 输赢控制

        /// <summary>
        /// 【中奖即停】
        /// True = 中奖后立即停止当前序列，重新检测上期结果以开启新一轮。
        /// False = 即使中奖，也必须把当前序列（如 "000111"）全部跑完才重新检测。
        /// </summary>
        public bool StopOnWin { get; set; } = true;

        #endregion

        #region 4. 运行时状态 (Runtime State) - JsonIgnore

        /// <summary>
        /// 是否正在跟随序列中
        /// </summary>
        [JsonIgnore]
        public bool IsBetting { get; set; } = false;

        /// <summary>
        /// 当前正在执行哪一个序列 (开0序列 或 开1序列)
        /// </summary>
        [JsonIgnore]
        public ResultFollowState CurrentState { get; set; } = ResultFollowState.None;

        /// <summary>
        /// 当前序列执行到的索引
        /// </summary>
        [JsonIgnore]
        public int CurrentStepIndex { get; set; } = 0;

        #endregion
    }

    /// <summary>
    /// 结果跟随规则的运行时状态枚举
    /// </summary>
    public enum ResultFollowState
    {
        None,
        FollowingZero, // 正在执行“开0”后的序列
        FollowingOne   // 正在执行“开1”后的序列
    }
}