using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace QuantumIdleModels.DTOs
{
    // 1. 机器人存卡请求
    public class ActivateCardRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string CardCode { get; set; }
    }
}
