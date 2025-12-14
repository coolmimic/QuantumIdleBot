using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumIdleModels.Entities
{
    /// <summary>
    /// 投注/挂机方案实体（数据库表）
    /// </summary>
    [Table("Schemes")]
    public class Scheme
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 用户ID（外键）
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 方案唯一标识（GUID字符串，用于与桌面版兼容）
        /// </summary>
        [Required]
        [StringLength(50)]
        public string SchemeId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 方案名称
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Telegram 游戏群名称
        /// </summary>
        [StringLength(200)]
        public string TgGroupName { get; set; }

        /// <summary>
        /// Telegram 游戏群ID
        /// </summary>
        public long TgGroupId { get; set; }

        /// <summary>
        /// 游戏类型（枚举值）
        /// </summary>
        public int GameType { get; set; }

        /// <summary>
        /// 游戏玩法（枚举值）
        /// </summary>
        public int PlayMode { get; set; }

        /// <summary>
        /// 倍率类型（枚举值）
        /// </summary>
        public int OddsType { get; set; }

        /// <summary>
        /// 位置列表（JSON数组，例如：[0,1,2]）
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string PositionLst { get; set; }

        /// <summary>
        /// 倍率配置（JSON对象）
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string OddsConfig { get; set; }

        /// <summary>
        /// 出号规则类型（枚举值）
        /// </summary>
        public int DrawRule { get; set; }

        /// <summary>
        /// 出号规则配置（JSON对象）
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string DrawRuleConfig { get; set; }

        /// <summary>
        /// 是否启用止盈止损
        /// </summary>
        public bool EnableStopProfitLoss { get; set; }

        /// <summary>
        /// 止盈金额
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal StopProfitAmount { get; set; }

        /// <summary>
        /// 止损金额
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal StopLossAmount { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 实盘盈亏
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Profit { get; set; }

        /// <summary>
        /// 实盘流水
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal Turnover { get; set; }

        /// <summary>
        /// 模拟盈亏
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal SimProfit { get; set; }

        /// <summary>
        /// 模拟流水
        /// </summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal SimTurnover { get; set; }

        // 导航属性
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }
    }
}

