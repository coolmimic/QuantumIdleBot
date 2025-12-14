using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumIdleModels.Entities
{
    /// <summary>
    /// Telegram 群组/频道实体
    /// </summary>
    [Table("TelegramChats")]
    public class TelegramChat
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 用户ID（外键）
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Telegram 群组/频道 ID
        /// </summary>
        public long ChatId { get; set; }

        /// <summary>
        /// 群组/频道名称
        /// </summary>
        [StringLength(200)]
        public string Name { get; set; }

        /// <summary>
        /// 是否是频道
        /// </summary>
        public bool IsChannel { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        // 导航属性
        [ForeignKey("UserId")]
        public virtual AppUser User { get; set; }
    }
}
