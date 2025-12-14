using System.Text.RegularExpressions;

namespace QuantumIdleWEB.GameCore
{
    /// <summary>
    /// 扫雷游戏上下文
    /// </summary>
    public class MinesweeperContext : GroupGameContext
    {
        private readonly Random _random = new();
        
        private readonly Dictionary<string, List<string>> _replacements = new()
        {
            {"大", new List<string> { "大", "da", "DA" }},
            {"小", new List<string> { "小", "x", "X" }},
            {"单", new List<string> { "单", "dan", "Dan" }},
            {"双", new List<string> { "双", "s", "S" }}
        };

        public MinesweeperContext(long groupId) : base(groupId, 0) // GameType.Minesweeper = 0
        {
        }

        protected override LotteryRecord? GetLotteryData(string message)
        {
            // 提取期号
            var issueMatch = Regex.Match(message, @"第([a-zA-Z0-9]+)期");
            
            // 提取开奖结果
            var resultMatch = Regex.Match(message, @"骰子为:\s*(\S+)");
            
            // 提取用户余额
            string balancePattern = @"\[(\d+)\].*?余额:([0-9.]+)";
            UserBalances.Clear();
            var balanceMatches = Regex.Matches(message, balancePattern);
            
            foreach (Match match in balanceMatches)
            {
                if (long.TryParse(match.Groups[1].Value, out long userId) &&
                    decimal.TryParse(match.Groups[2].Value, out decimal balance))
                {
                    UserBalances[userId] = balance;
                }
            }

            if (issueMatch.Success && resultMatch.Success)
            {
                return new LotteryRecord
                {
                    IssueNumber = issueMatch.Groups[1].Value,
                    Result = resultMatch.Groups[1].Value
                };
            }

            return null;
        }

        protected override GameMessageState GetMessageType(string message)
        {
            if (message.Contains("骰子为"))
            {
                return GameMessageState.LotteryResult;
            }
            else if (message.Contains("期号:") && message.Contains("发包手ID"))
            {
                return GameMessageState.StartBetting;
            }
            return GameMessageState.Unknown;
        }

        protected override string GetSaleIssue(string message)
        {
            var match = Regex.Match(message, @"期号:\s*([a-zA-Z0-9]+)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        public override string FormatOrderBets(List<OrderBet> orders)
        {
            var formattedBets = new List<string>();

            foreach (var order in orders)
            {
                if (string.IsNullOrEmpty(order.BetContent)) continue;
                
                var betContents = order.BetContent.Split(',').ToList();

                switch (order.PlayMode)
                {
                    case 0: // BigSmallOddEven
                        foreach (var content in betContents)
                        {
                            string finalContent = content;
                            if (_replacements.TryGetValue(content, out var replacementList) && replacementList.Count > 0)
                            {
                                int index = _random.Next(replacementList.Count);
                                finalContent = replacementList[index];
                            }
                            formattedBets.Add($"{finalContent}{order.BetMultiplier}");
                        }
                        break;

                    case 1: // Digital
                        foreach (var content in betContents)
                        {
                            formattedBets.Add($"{content}y{order.BetMultiplier}");
                        }
                        break;
                }
            }
            return string.Join(" ", formattedBets);
        }

        public override (decimal Balance, bool IsSuccess, string ErrorMessage) ParseBotReply(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return (0, false, "回复内容为空");
            }

            if (message.Contains("下注成功"))
            {
                decimal balance = 0;
                var match = Regex.Match(message, @"余额[:：]?\s*(\d+(\.\d+)?)");
                if (match.Success)
                {
                    decimal.TryParse(match.Groups[1].Value, out balance);
                }
                return (balance, true, string.Empty);
            }

            string reason = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];
            return (0, false, reason);
        }
    }
}
