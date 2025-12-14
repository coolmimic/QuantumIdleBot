using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QuantumIdleModels.DTOs
{
    // 2. 生成卡密请求
    public class GenerateCardRequest
    {
        /// <summary>
        /// 天数 (例如 30)
        /// </summary>
        [Required]
        [Range(1, 3650)]
        public int DurationDays { get; set; }

        /// <summary>
        /// 管理员密码
        /// </summary>
        [Required]
        public string AdminPassword { get; set; }

        /// <summary>
        /// 备注 (记录来源)
        /// </summary>
        public string Remark { get; set; }
    }
}
