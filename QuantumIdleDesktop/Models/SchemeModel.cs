using QuantumIdleDesktop.Models.DrawRules;
using QuantumIdleDesktop.Models.Odds;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json.Serialization;

namespace QuantumIdleDesktop.Models
{
    /// <summary>
    /// 投注/挂机方案实体
    /// </summary>
    public class SchemeModel
    {
        /// <summary>
        /// 是否启用 
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        // --- 基础信息 ---

        /// <summary>
        /// 方案ID (建议使用 Guid.NewGuid().ToString())
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 方案名称
        /// </summary>
        public string Name { get; set; }

        // --- Telegram 群组信息 ---

        /// <summary>
        /// Telegram 游戏群名称
        /// </summary>
        public string TgGroupName { get; set; }

        /// <summary>
        /// Telegram 游戏群ID (注意：TG群组ID通常是 long 类型，如 -100xxxx)
        /// </summary>
        public long TgGroupId { get; set; }

        // --- 游戏设定 ---

        /// <summary>
        /// 游戏类型 (枚举)
        /// </summary>
        public GameType GameType { get; set; }

        /// <summary>
        /// 游戏玩法 (枚举)
        /// </summary>
        public GamePlayMode PlayMode { get; set; }

        // --- 倍率设定 ---

        /// <summary>
        /// 倍率类型 (枚举)
        /// </summary>
        public OddsType OddsType { get; set; }


        public List<int> PositionLst { get; set; }

        /// <summary>
        /// 倍率对象 (存储具体的倍率配置，例如：大:1.98, 小:1.98)
        /// </summary>
        public OddsConfigBase OddsConfig { get; set; }

        // --- 出号/投注规则 ---

        /// <summary>
        /// 出号/决策规则 (枚举)
        /// </summary>
        public DrawRuleType DrawRule { get; set; }

        /// <summary>
        /// 出号规则对象 (存储规则的具体参数，例如：长龙5期后反打)
        /// </summary>
        public DrawRuleConfigBase DrawRuleConfig { get; set; }





        /// <summary>
        /// 实际盈亏 
        /// </summary>
        [JsonIgnore]
        public decimal RealProfit { get; set; }

        /// <summary>
        /// 实际流水 
        /// </summary>
        [JsonIgnore]
        public decimal RealTurnover { get; set; }

        /// <summary>
        /// 模拟盈亏
        /// </summary>
        [JsonIgnore]
        public decimal SimulatedProfit { get; set; }

        /// <summary>
        /// 模拟流水 
        /// </summary>
        [JsonIgnore]
        public decimal SimulatedTurnover { get; set; }

        // --- 资金管理 (风控) ---

        /// <summary>
        /// 是否启用止盈止损
        /// </summary>
        public bool EnableStopProfitLoss { get; set; }

        /// <summary>
        /// 止盈金额 (使用 decimal 保证金额精度)
        /// </summary>
        public decimal StopProfitAmount { get; set; }

        /// <summary>
        /// 止损金额
        /// </summary>
        public decimal StopLossAmount { get; set; }
    }
}
