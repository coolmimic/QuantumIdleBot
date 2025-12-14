using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleModels.DTOs
{
    public class UpdateBetResultDto
    {
        public string SourceRefId { get; set; } // 根据这个 ID 找订单
        public string OpenResult { get; set; }
        public decimal PayoutAmount { get; set; }
        public bool IsWin { get; set; }
    }
}
