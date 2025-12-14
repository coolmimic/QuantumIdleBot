namespace QuantumIdleWEB.GameCore
{
    /// <summary>
    /// 游戏消息状态
    /// </summary>
    public enum GameMessageState
    {
        Unknown,        // 未知/无关消息
        StartBetting,   // 开始下注/正在销售
        StopBetting,    // 封盘/停止下注
        LotteryResult,  // 开奖结果
        BetResult       // 下注结果
    }
}
