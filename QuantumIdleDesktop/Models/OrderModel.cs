using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Models
{
    public class OrderModel
    {
        // --- 基础信息 ---

        /// <summary>
        /// 订单ID (唯一标识)
        /// </summary>
        public string Id { get; set; }
        public long TgUserId { get; set;  }
        public long TgMsgId { get;set;  }
        /// <summary>
        /// 注单期号
        /// </summary>
        public string IssueNumber { get; set;  }

        /// <summary>
        /// 下注时间
        /// </summary>
        public DateTime BetTime { get; set; }

        // --- 群组信息 ---

        /// <summary>
        /// 来源群组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 来源群组ID
        /// </summary>
        public long GroupId { get; set; }

        // --- 方案/游戏信息 ---

        /// <summary>
        /// 方案ID
        /// </summary>
        public string SchemeId { get; set; }

        /// <summary>
        /// 方案名称 (例如: "挂机方案A")
        /// </summary>
        public string SchemeName { get; set; }

        /// <summary>
        /// 游戏类型 (枚举)
        /// </summary>
        public GameType GameType { get; set; }

        /// <summary>
        /// 玩法模式 (枚举)
        /// </summary>
        public GamePlayMode PlayMode { get; set; }

        // --- 投注详情 ---

        /// <summary>
        /// 下注内容 (例如: "大", "单", "3,4,5")
        /// </summary>
        public string BetContent { get; set; }

        /// <summary>
        /// 投注倍数 (例如: 1倍, 2倍)
        /// </summary>
        public int BetMultiplier { get; set; }

        /// <summary>
        /// 下注本金
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 位置信息
        /// </summary>
        public List<int> PositionLst { get; set; }

        // --- 结算信息 ---

        /// <summary>
        /// 开奖号码 (例如: "3,4,5")
        /// </summary>
        public string? OpenResult { get; set; }

        /// <summary>
        /// 中奖金额 / 派彩金额
        /// (即本局游戏获得的奖金，未减去本金)
        /// </summary>
        public decimal PayoutAmount { get; set; }

        /// <summary>
        /// 订单状态 (核心字段)
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.PendingSettlement;




        public bool IsWin { get; set; }


        /// <summary>
        /// 备注/错误信息 (如果 Status 是 Failed，这里记录原因，比如 "余额不足")
        /// </summary>
        public string Remark { get; set; }


        public bool IsSimulation { get; set; }


        /// <summary>
        /// 获取下注结果 (中/挂/平)
        /// </summary>
        public BetResult ResultState
        {
            get
            {
                // 1. 如果还没结算，或者下注失败，就没有输赢之说
                if (Status != OrderStatus.Settled) return BetResult.None;

                // 2. 根据金额判断
                // 注意：这里假设 PayoutAmount 是包含本金的派彩金额
                if (PayoutAmount > Amount) return BetResult.Win;
                if (PayoutAmount < Amount) return BetResult.Loss;

                // 3. 相等就是平
                return BetResult.Draw;
            }
        }

        /// <summary>
        /// 净盈亏 (派彩 - 本金)
        /// </summary>
        public decimal Profit => PayoutAmount - Amount;
    }
}
