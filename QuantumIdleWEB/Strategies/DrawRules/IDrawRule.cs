using QuantumIdleWEB.GameCore;

namespace QuantumIdleWEB.Strategies.DrawRules
{
    /// <summary>
    /// 出号规则接口
    /// </summary>
    public interface IDrawRule
    {
        /// <summary>
        /// 获取下一期下注内容
        /// </summary>
        List<string> GetNextBet(SchemeContext scheme, GroupGameContext context);
    }

    /// <summary>
    /// 方案上下文（简化版）
    /// </summary>
    public class SchemeContext
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DrawRule { get; set; }
        public object? DrawRuleConfig { get; set; }
        public int OddsType { get; set; }
        public object? OddsConfig { get; set; }
        public int GameType { get; set; }
        public int PlayMode { get; set; }
        public long TgGroupId { get; set; }
        public string TgGroupName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public bool EnableStopProfitLoss { get; set; }
        public decimal StopProfitAmount { get; set; }
        public decimal StopLossAmount { get; set; }
        
        // 运行时状态
        public decimal RealProfit { get; set; }
        public decimal RealTurnover { get; set; }
        public decimal SimProfit { get; set; }
        public decimal SimTurnover { get; set; }
    }
}
