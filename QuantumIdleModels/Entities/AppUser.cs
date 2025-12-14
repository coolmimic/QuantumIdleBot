using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QuantumIdleModels.Entities
{

    [Table("Users")]
    public class AppUser
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 软件登录账号
        /// </summary>
        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        /// <summary>
        /// 登录密码 (加密存储)
        /// </summary>
        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// 软件过期时间 (由卡密激活后更新此字段)
        /// </summary>
        public DateTime ExpireTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 账号状态 (0:未激活/失效, 1:正常, 其他值可扩展)
        /// </summary>
        public int IsActive { get; set; }

        /// <summary>
        /// 绑定的 Telegram ID (用于接收通知或验证身份)
        /// </summary>
        public long TelegramId { get; set; }

        /// <summary>
        /// 服务机器人会话 ID (用于推送通知)
        /// </summary>
        public long TelegramChatId { get; set; }

        /// <summary>
        /// 账号创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        // ========== 盈亏统计字段 ==========

        /// <summary>
        /// 实盘盈亏
        /// </summary>
        public decimal Profit { get; set; } = 0;

        /// <summary>
        /// 实盘流水
        /// </summary>
        public decimal Turnover { get; set; } = 0;

        /// <summary>
        /// 模拟盈亏
        /// </summary>
        public decimal SimProfit { get; set; } = 0;

        /// <summary>
        /// 模拟流水
        /// </summary>
        public decimal SimTurnover { get; set; } = 0;
    }
}
