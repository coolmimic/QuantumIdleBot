using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QuantumIdleDesktop.GameCore
{
    /// <summary>
    /// 哈希彩游戏上下文 (原时时彩/分分彩)
    /// 处理逻辑：针对哈希分分彩的开奖、下注和开盘逻辑
    /// </summary>
    public class HashLotteryContext : GroupGameContext
    {
        // 建议将枚举 GameType.shishicai 重构为 GameType.HashLottery 以保持专业性
        public HashLotteryContext(long groupId, GameType gameType, TelegramGroupModel groupModel)
            : base(groupId, gameType, groupModel)
        {
        }

        /// <summary>
        /// 从开奖广播消息中提取期号和开奖结果
        /// </summary>
        protected override LotteryRecord GetLotteryData(string message)
        {
            // ==========================================
            // 1. 提取期号
            // 样本: 📢 哈希分分彩 第 202512110159 期开奖结果
            // ==========================================
            var issueMatch = Regex.Match(message, @"第\s*(\d+)\s*期");

            // ==========================================
            // 2. 提取开奖号码
            // 样本: 🎲 号码：3  6  1  0  0
            // 正则说明：匹配 "号码" 后面的数字序列，允许数字间有空格
            // ==========================================
            var resultMatch = Regex.Match(message, @"号码[:：]\s*((?:\d\s*)+)");

            /* * 注意：与扫雷不同，哈希彩的开奖广播通常不包含所有玩家的余额列表。
             * 所以这里只返回期号和结果，不处理 UserBalances。
             */

            if (issueMatch.Success && resultMatch.Success)
            {
                // 处理开奖号码格式：将 "3  6  1" 这种带空格的格式转换为 "3,6,1,0,0" 标准存储格式
                string rawNumbers = resultMatch.Groups[1].Value;
                // 使用正则分割空格并用逗号连接
                string formattedResult = string.Join(",", Regex.Split(rawNumbers.Trim(), @"\s+"));

                return new LotteryRecord
                {
                    IssueNumber = issueMatch.Groups[1].Value,
                    Result = formattedResult
                };
            }

            return null;
        }

        /// <summary>
        /// 判断群消息类型 (是开奖结果，还是开始下注)
        /// </summary>
        protected override GameMessageState GetMessageType(string message)
        {
            // 样本: 📢 哈希分分彩 第 ... 期开奖结果 ... 号码：...
            if (message.Contains("开奖结果") && message.Contains("号码"))
            {
                return GameMessageState.LotteryResult;
            }
            // 样本: 🟢 哈希分分彩 新期数开启 ... 当前期号：...
            else if (message.Contains("新期数开启") || (message.Contains("当前期号") && message.Contains("赔率")))
            {
                return GameMessageState.StartBetting;
            }

            return GameMessageState.Unknown;
        }

        /// <summary>
        /// 从开盘消息中提取当前可下注的期号
        /// </summary>
        protected override string GetSaleIssue(string message)
        {
            // 样本: 📌 当前期号：202512110157
            var match = Regex.Match(message, @"当前期号[:：]\s*(\d+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }

        private readonly Random _random = new Random();

        /// <summary>
        /// 格式化下注指令
        /// 根据玩法说明将订单转换为具体的文本指令
        /// </summary>
        public override string FormatOrderBets(List<OrderModel> orders)
        {
            List<string> formattedBets = new List<string>();

            foreach (var order in orders)
            {
                // 防止空内容
                if (string.IsNullOrEmpty(order.BetContent)) continue;

                List<string> betContents = order.BetContent.Split(',').ToList();

                /* * * 玩法指令逻辑 (根据你的样本):
                 * 1. 文字玩法 (双面/龙虎/和值): 格式为 "内容+金额" (例如: 大10, 龙10)
                 * 2. 数字玩法 (定位胆): 格式为 "号码/金额" (例如: 5/10)
                 */

                foreach (var content in betContents)
                {
                    string finalContent = content;

                    // 1. 同义词随机替换 (模拟真人)
                    // 如果字典里有 "大" -> ["大", "da"]，则随机选一个
                    if (_replacements.TryGetValue(content, out var replacementList) && replacementList.Count > 0)
                    {
                        int index = _random.Next(replacementList.Count);
                        finalContent = replacementList[index];
                    }

                    // 2. 根据内容判断格式
                    // 如果内容全是数字 (例如 "5")，视为【定位胆】玩法
                    if (Regex.IsMatch(finalContent, @"^\d+$"))
                    {
                        // 格式: 号码/金额 (如: 5/10)
                        formattedBets.Add($"{finalContent}/{order.BetMultiplier}");
                    }
                    else
                    {
                        // 如果内容包含文字 (例如 "大", "单", "龙")，视为【双面/龙虎】玩法
                        // 格式: 内容金额 (如: 大10)
                        formattedBets.Add($"{finalContent}{order.BetMultiplier}");
                    }
                }
            }
            // 将多个下注内容用空格连接
            return string.Join(" ", formattedBets);
        }

        // 同义词替换字典 (根据需要可扩展拼音等)
        private readonly Dictionary<string, List<string>> _replacements = new Dictionary<string, List<string>>()
        {
            {"大", new List<string>() { "大" }},
            {"小", new List<string>() { "小" }},
            {"单", new List<string>() { "单" }},
            {"双", new List<string>() { "双" }},
            {"龙", new List<string>() { "龙" }},
            {"虎", new List<string>() { "虎" }},
            {"和", new List<string>() { "和" }}
        };

        /// <summary>
        /// 解析机器人的回复结果 (用于判断下注是否成功及更新余额)
        /// </summary>
        public override (decimal Balance, bool IsSuccess, string ErrorMessage) ParseBotReply(string message)
        {
            // 1. 空值检查
            if (string.IsNullOrWhiteSpace(message))
            {
                return (0, false, "回复内容为空");
            }

            // 2. 成功判断
            // 样本: ✅ 下注成功
            if (message.Contains("下注成功"))
            {
                decimal balance = 0;

                // 3. 提取余额
                // 样本: 💳 余额：9.5
                // 匹配 "余额" 后面跟着 冒号(可选) 和 数字
                var match = Regex.Match(message, @"余额[:：]\s*(\d+(\.\d+)?)");

                if (match.Success)
                {
                    decimal.TryParse(match.Groups[1].Value, out balance);
                }

                return (balance, true, string.Empty);
            }

            // 3. 失败处理
            // 如果没有"下注成功"，通常第一行就是错误原因 (例如: 封盘、余额不足)
            string reason = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return (0, false, reason ?? "未知错误");
        }
    }
}