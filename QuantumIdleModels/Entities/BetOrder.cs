using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumIdleModels.Entities
{
    /// <summary>
    /// 注单信息表
    /// <para>记录所有下注历史，作为核心账本数据。用于统计盈亏、流水和历史回顾。</para>
    /// </summary>
    [Table("BetOrders")]
    public class BetOrder
    {
        // ==========================================
        // 基础主键
        // ==========================================

        /// <summary>
        /// 数据库主键 ID
        /// <para>自增 BigInt，保证海量数据下的性能。</para>
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// 关联的用户 ID
        /// <para>对应 Users 表的 Id。表示该注单归属哪个软件账号。</para>
        /// </summary>
        public int AppUserId { get; set; }

        /// <summary>
        /// Telegram 消息 ID
        /// <para>下注时发送的消息ID，用于匹配机器人回复确认下注结果</para>
        /// </summary>
        public int TgMsgId { get; set; }

        // ==========================================
        // 来源追踪
        // ==========================================

        /// <summary>
        /// 来源引用 ID (外部ID)
        /// <para>通常存储客户端的 Telegram MessageId 或 平台方的原始订单号。</para>
        /// </summary>
        [StringLength(50)]
        public string SourceRefId { get; set; }

        /// <summary>
        /// 游戏期号
        /// <para>例如: "20231121-088"。</para>
        /// </summary>
        [StringLength(50)]
        public string IssueNumber { get; set; }

        // ==========================================
        // 游戏参数
        // ==========================================

        /// <summary>
        /// 游戏类型 (Enum -> int)
        /// <para>对应客户端 GameType 枚举 (如: 0=重庆时时彩, 1=赛车)。</para>
        /// </summary>
        public int GameType { get; set; }

        /// <summary>
        /// 玩法模式 (Enum -> int)
        /// <para>对应客户端 PlayMode 枚举 (如: 0=模拟, 1=实盘)。</para>
        /// </summary>
        public int PlayMode { get; set; }

        /// <summary>
        /// 方案 ID
        /// <para>记录该注单是由哪个策略/方案生成的。</para>
        /// </summary>
        [StringLength(50)]
        public string SchemeId { get; set; }

        // ==========================================
        // 资金核心数据
        // ==========================================

        /// <summary>
        /// 下注内容
        /// <para>文本描述，例如: "第一球 大 100" 或 "龙虎 200"。</para>
        /// </summary>
        [StringLength(500)]
        public string BetContent { get; set; }

        /// <summary>
        /// 投入本金
        /// <para>精度: 18位总长，4位小数。</para>
        /// </summary>
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Amount { get; set; }

        // ==========================================
        // 结算数据 (包含结果与金额)
        // ==========================================

        /// <summary>
        /// 开奖号码 / 开奖结果
        /// <para>记录该期号最终开出的结果，例如: "3,5,9,2,1"。</para>
        /// <para>未开奖时通常为空。</para>
        /// </summary>
        [StringLength(100)]
        public string OpenResult { get; set; }

        /// <summary>
        /// 派彩金额 (返还金额)
        /// <para>通常包含本金。例如投100赢了返198，则此字段为 198.00。</para>
        /// <para>如果是未结算或输了，此字段通常为 0。</para>
        /// </summary>
        [Column(TypeName = "decimal(18, 4)")]
        public decimal PayoutAmount { get; set; }

        /// <summary>
        /// 净盈亏 (Net Profit)
        /// <para>计算公式: PayoutAmount - Amount。</para>
        /// <para>赢: 正数; 输: 负数; 平: 0。数据库直接存储方便 SQL 聚合统计。</para>
        /// </summary>
        [Column(TypeName = "decimal(18, 4)")]
        public decimal Profit { get; set; }

        // ==========================================
        // 状态标识
        // ==========================================

        /// <summary>
        /// 订单状态
        /// <para>0: Pending (未结算/进行中)</para>
        /// <para>1: Settled (已结算)</para>
        /// <para>-1: Cancelled (已取消/无效)</para>
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 是否盈利 (胜负结果)
        /// <para>true: 赢; false: 输。仅在 Status=1 时有意义。</para>
        /// </summary>
        public bool IsWin { get; set; }

        /// <summary>
        /// 是否为模拟单
        /// <para>true: 模拟测试数据; false: 真实资金数据。</para>
        /// </summary>
        public bool IsSimulation { get; set; }

        // ==========================================
        // 时间记录
        // ==========================================

        /// <summary>
        /// 下注时间
        /// </summary>
        public DateTime BetTime { get; set; }

        /// <summary>
        /// 结算时间
        /// <para>开奖并计算出盈亏的时间。未结算时可为 null。</para>
        /// </summary>
        public DateTime? SettleTime { get; set; }
    }
}