using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumIdleModels.Entities
{
    [Table("CardUsageLogs")]
    public class CardUsageLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 使用者ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 使用的卡密ID
        /// </summary>
        public int CardKeyId { get; set; }

        /// <summary>
        /// 冗余记录卡号 (方便不查表也能看)
        /// </summary>
        [StringLength(50)]
        public string CardCodeSnapshot { get; set; }

        /// <summary>
        /// 激活前的过期时间 (保留历史，方便排查)
        /// </summary>
        public DateTime PreviousExpireTime { get; set; }

        /// <summary>
        /// 激活后的过期时间
        /// </summary>
        public DateTime NewExpireTime { get; set; }

        /// <summary>
        /// 使用时间
        /// </summary>
        public DateTime UseTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 激活时的IP地址 (防盗用)
        /// </summary>
        [StringLength(50)]
        public string IpAddress { get; set; }
    }
}