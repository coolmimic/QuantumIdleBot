namespace QuantumIdleWEB.Strategies.OddsRules
{
    /// <summary>
    /// 倍率规则接口
    /// </summary>
    public interface IOddsRule
    {
        /// <summary>
        /// 获取下一期的倍数
        /// </summary>
        int GetNextMultiplier(OddsContext context);

        /// <summary>
        /// 结算后更新状态
        /// </summary>
        void UpdateState(OddsContext context, bool isWin);
    }

    /// <summary>
    /// 倍率上下文
    /// </summary>
    public class OddsContext
    {
        public object? OddsConfig { get; set; }
        public int CurrentIndex { get; set; }
    }
}
