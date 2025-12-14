using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QuantumIdleDesktop.Models
{
    /// <summary>
    /// 订单状态枚举
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// 待开奖 (下注成功，等待开奖结果)
        /// </summary>
        [Description("待开奖")]
        PendingSettlement,

        /// <summary>
        /// 已结算 (已拿到结果，无论输赢)
        /// </summary>
        [Description("已结算")]
        Settled,

        /// <summary>
        /// 下注失败 (余额不足、网络错误、盘口关闭等)
        /// </summary>
        [Description("下注失败")]
        BetFailed = 2,

        /// <summary>
        /// 已取消 (比如该期作废)
        /// </summary>
        [Description("已取消")]
        Cancelled,

        /// <summary>
        /// 待确认 
        /// </summary>
        [Description("待确认")]
        Confirmed

    }
}
