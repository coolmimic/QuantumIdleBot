using System;
using System.Collections.Generic;
using System.Linq; // 用于 .Any()
using System.Text.Json.Serialization;

namespace QuantumIdleDesktop.Models.DrawRules
{
    /// <summary>
    /// 01形态/走势规则配置 (主类)
    /// </summary>
    public class PatternTrendRuleConfig : DrawRuleConfigBase
    {
        public override DrawRuleType Type => DrawRuleType.PatternTrend;

        #region 全局定义 (0和1代表什么)

        public string CodeZeroDefinition { get; set; } = "大";
        public string CodeOneDefinition { get; set; } = "小";

        #endregion

        #region 策略集合

        /// <summary>
        /// 包含多个监控策略
        /// </summary>
        public List<TrendStrategyItem> StrategyList { get; set; } = new List<TrendStrategyItem>();

        #endregion

        #region 运行时快捷状态 (JsonIgnore)

        /// <summary>
        /// 【核心回答】快捷判断：当前是否任意一个子策略正在运行中？
        /// 使用 Computed Property (=>) 确保数据永远是最新的，不用担心状态不同步
        /// </summary>
        [JsonIgnore]
        public bool IsAnyStrategyExecuting => StrategyList != null && StrategyList.Any(x => x.IsExecuting);

        /// <summary>
        /// (可选) 快速获取当前那个正在运行的策略对象
        /// </summary>
        [JsonIgnore]
        public TrendStrategyItem CurrentActiveStrategy => StrategyList?.FirstOrDefault(x => x.IsExecuting);

        #endregion
    }

    /// <summary>
    /// 具体的监控策略项 (子类)
    /// </summary>
    public class TrendStrategyItem
    {
        #region 配置数据
        /// <summary>
        /// 监控形态 (如：000)
        /// </summary>
        public string MonitorPattern { get; set; }

        /// <summary>
        /// 投注形态 (如：111)
        /// </summary>
        public string BetPattern { get; set; }

        /// <summary>
        /// 中奖即停
        /// </summary>
        public bool StopOnWin { get; set; } = true;

        #endregion

        #region 运行时状态 (JsonIgnore)

        /// <summary>
        /// 是否正在执行
        /// </summary>
        [JsonIgnore]
        public bool IsExecuting { get; set; }

        /// <summary>
        /// 当前执行索引
        /// </summary>
        [JsonIgnore]
        public int CurrentStepIndex { get; set; }

        #endregion
    }
}