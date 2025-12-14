using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QuantumIdleDesktop.GameCore
{
    public class MinesweeperContext : GroupGameContext
    {
        public MinesweeperContext(long groupId, GameType gameType, TelegramGroupModel groupModel) : base(groupId, gameType, groupModel)
        {
        }

        protected override LotteryRecord GetLotteryData(string message)
        {
          
            // ==========================================
            // 1. 提取期号 (保持原逻辑)
            // ==========================================
            var issueMatch = Regex.Match(message, @"第([a-zA-Z0-9]+)期");

            // ==========================================
            // 2. 提取开奖结果 (保持原逻辑)
            // ==========================================
            var resultMatch = Regex.Match(message, @"骰子为:\s*(\S+)");

            // ==========================================
            // 3. 提取用户余额 (新增逻辑)
            // ==========================================
            string balancePattern = @"\[(\d+)\].*?余额:([0-9.]+)";

            // 使用 Matches 获取所有匹配项
            base.UserBalances.Clear();
            var balanceMatches = Regex.Matches(message, balancePattern);

            foreach (Match match in balanceMatches)
            {
                if (long.TryParse(match.Groups[1].Value, out long userId) &&
                    decimal.TryParse(match.Groups[2].Value, out decimal balance))
                {
                    // 字典赋值特性：如果 Key 不存在则添加，如果存在则覆盖（更新为最新）
                    // 完美解决代码中提到的“可能有多个相同ID”的问题
                    if (base.UserBalances.ContainsKey(userId))
                    {
                        base.UserBalances[userId] = balance;
                    }
                    else
                    {
                        base.UserBalances.Add(userId, balance);
                    }
                }
            }

            // ==========================================
            // 4. 返回结果
            // ==========================================
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
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        private readonly Random _random = new Random();
        public override string FormatOrderBets(List<OrderModel> orders)
        {
            List<string> formattedBets = new List<string>();

            foreach (var order in orders)
            {
                // 防止 BetContent 为空导致异常
                if (string.IsNullOrEmpty(order.BetContent)) continue;

                List<string> betContents = order.BetContent.Split(',').ToList();

                switch (order.PlayMode)
                {
                    case GamePlayMode.BigSmallOddEven:
                        foreach (var content in betContents)
                        {
                            // 默认为原始内容
                            string finalContent = content;

                            // 2. 检查字典中是否存在该键，并获取对应的列表
                            if (_replacements.TryGetValue(content, out var replacementList) && replacementList.Count > 0)
                            {
                                // 3. 核心逻辑：生成一个 0 到 Count-1 之间的随机索引
                                int index = _random.Next(replacementList.Count);
                                finalContent = replacementList[index];
                            }

                            // 4. 必须将处理后的结果添加到列表（您原来的代码漏了这一步）
                            // 格式：内容 + 空格 + 倍数
                            formattedBets.Add($"{finalContent}{order.BetMultiplier}");
                        }
                        break;

                    case GamePlayMode.Digital: // 数字玩法
                        foreach (var content in betContents)
                        {
                            // 格式：内容 + y + 倍数
                            formattedBets.Add($"{content}y{order.BetMultiplier}");
                        }
                        break;
                }
            }
            return string.Join(" ", formattedBets);
        }
        private readonly Dictionary<string, List<string>> _replacements = new Dictionary<string, List<string>>()
        {
            {"大", new List<string>() { "大", "da", "DA" }},
            {"小", new List<string>() { "小", "x", "X" }},
            {"单", new List<string>() { "单", "dan", "Dan" }},
            {"双", new List<string>() { "双", "s", "S" }}
        };

        public override (decimal Balance, bool IsSuccess, string ErrorMessage) ParseBotReply(string message)
        {
            // 1. 空值检查
            if (string.IsNullOrWhiteSpace(message))
            {
                return (0, false, "回复内容为空");
            }

            // 2. 判断是否成功
            // 只要包含 "下注成功" 四个字，即视为成功
            if (message.Contains("下注成功"))
            {
                // --- 提取余额逻辑 ---
                decimal balance = 0;

                // 使用正则匹配余额
                // 匹配规则：寻找 "余额" 后面跟着 "冒号(可选)" 和 "数字(包含小数点)"
                // 兼容格式："余额: 43.66" 或 "余额：43.66"
                var match = Regex.Match(message, @"余额[:：]?\s*(\d+(\.\d+)?)");

                if (match.Success)
                {
                    // Groups[1] 是捕获到的数字部分
                    decimal.TryParse(match.Groups[1].Value, out balance);
                }

                // 返回：余额, true, 无错误信息
                return (balance, true, string.Empty);
            }

            // 3. 处理失败情况
            // 如果没有 "下注成功"，则视为失败
            // 提取第一行作为简短的错误原因（通常第一行是 "余额不足" 或 "封盘"）
            string reason = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0];

            // 返回：0, false, 失败原因
            return (0, false, reason);
        }


    }
}
