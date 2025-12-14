using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace QuantumIdleModels.Entities
{
    [Table("PaymentOrders")]
    public class PaymentOrder
    {
        [Key]
        public long Id { get; set; }

        public long TelegramId { get; set; }

        public int DurationDays { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal BaseAmount { get; set; }

        [Column(TypeName = "decimal(18, 4)")]
        public decimal RealAmount { get; set; }

        public int Status { get; set; } // 0:待支付, 1:已完成, -1:已过期

        public string? TxId { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        // ==========================================
        // 👇 新增字段：过期时间
        // ==========================================
        public DateTime ExpireTime { get; set; }
    }
}
