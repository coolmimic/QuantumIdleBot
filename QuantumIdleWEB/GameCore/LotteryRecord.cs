namespace QuantumIdleWEB.GameCore
{
    /// <summary>
    /// 开奖记录
    /// </summary>
    public class LotteryRecord
    {
        public string IssueNumber { get; set; } = string.Empty; // 期号
        public string Result { get; set; } = string.Empty;      // 开奖结果
        public DateTime OpenTime { get; set; }                   // 开奖时间
    }
}
