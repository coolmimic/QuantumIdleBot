using System.Text.RegularExpressions;

namespace QuantumIdleWEB.GameCore
{
    /// <summary>
    /// 哈希彩游戏上下文
    /// </summary>
    public class HashLotteryContext : GroupGameContext
    {
        private readonly Random _random = new();

        private readonly Dictionary<string, List<string>> _replacements = new()
        {
            {"大", new List<string> { "大" }},
            {"小", new List<string> { "小" }},
            {"单", new List<string> { "单" }},
            {"双", new List<string> { "双" }},
            {"龙", new List<string> { "龙" }},
            {"虎", new List<string> { "虎" }},
            {"和", new List<string> { "和" }}
        };

        public HashLotteryContext(long groupId) : base(groupId, 4) // GameType.ShiShiCai = 4
        {
        }

        protected override LotteryRecord? GetLotteryData(string message)
        {
            // 提取期号: 📢 哈希分分彩 第 202512110159 期开奖结果
            var issueMatch = Regex.Match(message, @"第\s*(\d+)\s*期");

            // 提取开奖号码: 🎲 号码：3  6  1  0  0
            var resultMatch = Regex.Match(message, @"号码[:：]\s*((?:\d\s*)+)");

            if (issueMatch.Success && resultMatch.Success)
            {
                string rawNumbers = resultMatch.Groups[1].Value;
                string formattedResult = string.Join(",", Regex.Split(rawNumbers.Trim(), @"\s+"));

                return new LotteryRecord
                {
                    IssueNumber = issueMatch.Groups[1].Value,
                    Result = formattedResult
                };
            }

            return null;
        }

        protected override GameMessageState GetMessageType(string message)
        {
            if (message.Contains("开奖结果") && message.Contains("号码"))
            {
                return GameMessageState.LotteryResult;
            }
            else if (message.Contains("新期数开启") || (message.Contains("当前期号") && message.Contains("赔率")))
            {
                return GameMessageState.StartBetting;
            }

            return GameMessageState.Unknown;
        }

        protected override string GetSaleIssue(string message)
        {
            var match = Regex.Match(message, @"当前期号[:：]\s*(\d+)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        public override string FormatOrderBets(List<OrderBet> orders)
        {
            var formattedBets = new List<string>();

            foreach (var order in orders)
            {
                if (string.IsNullOrEmpty(order.BetContent)) continue;

                var betContents = order.BetContent.Split(',').ToList();

                foreach (var content in betContents)
                {
                    string finalContent = content;

                    if (_replacements.TryGetValue(content, out var replacementList) && replacementList.Count > 0)
                    {
                        int index = _random.Next(replacementList.Count);
                        finalContent = replacementList[index];
                    }

                    // 如果是数字，使用定位胆格式
                    if (Regex.IsMatch(finalContent, @"^\d+$"))
                    {
                        formattedBets.Add($"{finalContent}/{order.BetMultiplier}");
                    }
                    else
                    {
                        formattedBets.Add($"{finalContent}{order.BetMultiplier}");
                    }
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
                var match = Regex.Match(message, @"余额[:：]\s*(\d+(\.\d+)?)");
                if (match.Success)
                {
                    decimal.TryParse(match.Groups[1].Value, out balance);
                }
                return (balance, true, string.Empty);
            }

            string reason = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "未知错误";
            return (0, false, reason);
        }
    }
}
