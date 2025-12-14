namespace QuantumIdleWEB.GameCore
{
    /// <summary>
    /// 群游戏上下文基类
    /// </summary>
    public abstract class GroupGameContext
    {
        public long GroupId { get; private set; }
        public int GameType { get; private set; }
        public string GroupName { get; set; } = string.Empty;
        
        public Dictionary<long, decimal> UserBalances { get; private set; } = new();
        
        // 当前销售期号
        public string CurrentIssue { get; protected set; } = string.Empty;
        
        // 历史记录
        public List<LotteryRecord> History { get; private set; } = new();
        
        // 已开盘期号列表
        public List<string> SaleLst { get; set; } = new();

        protected GroupGameContext(long groupId, int gameType)
        {
            GroupId = groupId;
            GameType = gameType;
        }

        /// <summary>
        /// 核心消息处理方法
        /// </summary>
        public GameMessageState ProcessMessage(string message)
        {
            var msgType = GetMessageType(message);
            if (msgType == GameMessageState.Unknown) return GameMessageState.Unknown;

            switch (msgType)
            {
                case GameMessageState.StartBetting:
                    return HandleStartBetting(message);
                case GameMessageState.LotteryResult:
                    return HandleLotteryResult(message);
                case GameMessageState.StopBetting:
                    return GameMessageState.StopBetting;
                default:
                    return GameMessageState.Unknown;
            }
        }

        /// <summary>
        /// 处理开始销售逻辑
        /// </summary>
        private GameMessageState HandleStartBetting(string message)
        {
            string saleIssue = GetSaleIssue(message);
            if (string.IsNullOrEmpty(saleIssue)) return GameMessageState.Unknown;

            if (SaleLst.Contains(saleIssue))
            {
                return GameMessageState.Unknown; // 已处理过
            }

            CurrentIssue = saleIssue;
            SaleLst.Add(saleIssue);

            // 维护列表长度
            if (SaleLst.Count > 50)
            {
                SaleLst.RemoveAt(0);
            }

            return GameMessageState.StartBetting;
        }

        /// <summary>
        /// 处理开奖结果逻辑
        /// </summary>
        private GameMessageState HandleLotteryResult(string message)
        {
            var openData = GetLotteryData(message);
            if (openData == null || string.IsNullOrEmpty(openData.IssueNumber))
                return GameMessageState.Unknown;

            var existingRecord = History.Find(x => x.IssueNumber == openData.IssueNumber);
            if (existingRecord != null)
            {
                return GameMessageState.Unknown; // 已存在
            }

            openData.OpenTime = DateTime.Now;
            History.Insert(0, openData);

            if (History.Count > 50)
            {
                History.RemoveAt(History.Count - 1);
            }

            return GameMessageState.LotteryResult;
        }

        // 抽象方法 - 子类实现
        protected abstract GameMessageState GetMessageType(string message);
        protected abstract string GetSaleIssue(string message);
        protected abstract LotteryRecord? GetLotteryData(string message);
        public abstract string FormatOrderBets(List<OrderBet> orders);
        public abstract (decimal Balance, bool IsSuccess, string ErrorMessage) ParseBotReply(string message);
    }

    /// <summary>
    /// 订单投注信息
    /// </summary>
    public class OrderBet
    {
        public string BetContent { get; set; } = string.Empty;
        public int BetMultiplier { get; set; }
        public int PlayMode { get; set; }
    }
}
