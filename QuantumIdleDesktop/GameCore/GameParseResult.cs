using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.GameCore
{
    public enum GameMessageState
    {
        Unknown,        // 未知/无关消息
        StartBetting,   // 开始下注/正在销售 (例如：123期 开始下注)
        StopBetting,    // 封盘/停止下注
        LotteryResult,   // 开奖结果 (例如：123期 开奖结果 3,4,5)
        BetResult
    
    }
}
