using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleModels.DTOs
{
    public class CreateBetOrderDto
    {
        public string SourceRefId { get; set; } // 对应 TgMsgId
        public string IssueNumber { get; set; }
        public int GameType { get; set; }
        public int PlayMode { get; set; }
        public string SchemeId { get; set; }
        public string BetContent { get; set; }
        public decimal Amount { get; set; }
        public bool IsSimulation { get; set; }
        public DateTime BetTime { get; set; }
    }
}
