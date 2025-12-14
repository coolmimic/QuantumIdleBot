using System;
using System.Text.Json.Serialization;

namespace QuantumIdleDesktop.Models.DrawRules
{
    /// <summary>
    /// 分支走势规则 (BranchTrend)
    /// 特性：监控历史形态 -> 触发首投 -> 根据中/挂走不同的投注序列
    /// </summary>
    public class BranchTrendRuleConfig : DrawRuleConfigBase
    {
        // 请记得在您的 DrawRuleType 枚举中添加 BranchTrend
        public override DrawRuleType Type => DrawRuleType.BranchTrend;

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

        #region 2. 监控条件

        /// <summary>
        /// 监控历史形态 (0/1 字符串)
        /// 必须完全匹配此后缀才触发投注
        /// 例如 "0001111"
        /// </summary>
        public string MonitorPattern { get; set; } = "0001111";

        #endregion

        #region 3. 投注分支策略

        /// <summary>
        /// 【首投】
        /// 监控达成后，第一手投什么？("0" 或 "1")
        /// </summary>
        public string InitialBet { get; set; } = "0";//这里改成 让用户选择正投还是反投

        /// <summary>
        /// 【挂了（输）后执行的形态】
        /// 如果上一手挂了，按此序列继续
        /// 例如 "000001111"
        /// </summary>
        public string LossPattern { get; set; } = "000001111";

        /// <summary>
        /// 【中了（赢）后执行的形态】
        /// 如果上一手中了，按此序列继续
        /// 例如 "1110000"
        /// </summary>
        public string WinPattern { get; set; } = "1110000";

        /// <summary>
        /// 【中奖即停】
        /// True = 中奖后立即停止本轮，重新监控 (忽略 WinPattern)
        /// False = 中奖后进入 WinPattern 继续执行
        /// </summary>
        public bool StopOnWin { get; set; } = true;

        #endregion

        #region 4. 运行时状态 (JsonIgnore)

        /// <summary>
        /// 是否正在投注中
        /// </summary>
        [JsonIgnore]
        public bool IsBetting { get; set; } = false;

        /// <summary>
        /// 当前的投注阶段 (首投 / 赢序列 / 输序列)
        /// </summary>
        [JsonIgnore]
        public BranchBettingState CurrentState { get; set; } = BranchBettingState.None;

        /// <summary>
        /// 当前序列执行到的索引
        /// </summary>
        [JsonIgnore]
        public int CurrentStepIndex { get; set; } = 0;

        /// <summary>
        /// 获取当前应投内容
        /// </summary>
        public string GetCurrentBetContent()
        {
            if (!IsBetting) return null;

            string codeToBet = null;

            // 1. 首投
            if (CurrentState == BranchBettingState.Initial)
            {
                codeToBet = InitialBet;
            }
            // 2. 分支序列
            else
            {
                string activePattern = (CurrentState == BranchBettingState.WinSequence) ? WinPattern : LossPattern;

                if (!string.IsNullOrEmpty(activePattern) && CurrentStepIndex < activePattern.Length)
                {
                    codeToBet = activePattern[CurrentStepIndex].ToString();
                }
            }

            // 转换 0/1 为定义
            if (codeToBet == "0") return CodeZeroDefinition;
            if (codeToBet == "1") return CodeOneDefinition;

            return null;
        }

        #endregion
    }

    /// <summary>
    /// 分支投注的状态枚举
    /// </summary>
    public enum BranchBettingState
    {
        None,           // 监控中
        Initial,        // 首投
        WinSequence,    // 中奖分支
        LossSequence    // 挂单分支
    }
}