using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.GameCore
{
    public class LotteryRecord
    {
        public string IssueNumber { get; set; } // 期号
        public string Result { get; set; }      // 开奖结果 (例如 "3,4,5" 或 "10")
        public DateTime OpenTime { get; set; }  // 开奖时间
    }
}
