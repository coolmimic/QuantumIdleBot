using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuantumIdleModels.Entities
{
    [Table("CardKeys")]
    public class CardKey
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 卡密字符串 (例如: QM-8888-XXXX)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string KeyCode { get; set; }

        /// <summary>
        /// 时长类型 (天数，例如 30 表示月卡，365 表示年卡)
        /// </summary>
        public int DurationDays { get; set; }

        /// <summary>
        /// 批次/备注 (方便你后台管理，知道是哪一批生成的)
        /// </summary>
        [StringLength(100)]
        public string BatchName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 是否已被消耗
        /// </summary>
        public bool IsRedeemed { get; set; } = false;
        public DateTime UsedTime { get; set; }
        public long UsedByAppUserId { get; set; }
    }
}