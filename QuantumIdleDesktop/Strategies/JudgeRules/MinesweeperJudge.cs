using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Strategies.JudgeRules
{
    public class MinesweeperJudge : IJudgeStrategy
    {
        /// <summary>
        /// 判断注单中奖个数（支持多标签、包选、组合投注）
        /// </summary>
        /// <param name="order">注单对象</param>
        /// <returns>中奖个数（每中一个标签算一个）</returns>
        public int Judge(OrderModel order)
        {
            // 1. 解析开奖号码（扫雷是单个数字）
            if (!int.TryParse(order.OpenResult, out int openCode) || openCode < 0 || openCode > 9)
            {
                return 0; // 非法开奖号码，不中
            }

            // 2. 获取该玩法的开奖标签集合（核心！）
            List<string> openTags = GetOpenTags(order.PlayMode, openCode);

            var betItems = order.BetContent?
                .Split(new[] { ',', '，', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList() ?? new List<string>();

            if (betItems.Count == 0)
                return 0;

            // 4. 计算交集个数 → 这就是中奖个数！
            int winCount = betItems.Count(bet => openTags.Contains(bet, StringComparer.OrdinalIgnoreCase));

            return winCount;
        }

        /// <summary>
        /// 根据玩法和开奖号码，返回该号码拥有的所有标签
        /// 这是你整个系统的“开奖标签引擎”，只改这里就行，处处生效
        /// </summary>
        private static List<string> GetOpenTags(GamePlayMode playMode, int num)
        {
            var tags = new List<string>();

            switch (playMode)
            {
                case GamePlayMode.BigSmallOddEven:
                   
                    tags.Add(num > 3 ? "大" : "小");
                    // 单双规则
                    tags.Add(num % 2 == 0 ? "双" : "单");
                    break;

                case GamePlayMode.Digital:
                    // 数字玩法：直接就是号码本身
                    tags.Add(num.ToString());
                    break;

                default:
                    // 未知玩法，保守处理
                    tags.Add(num.ToString());
                    break;
            }

            return tags;
        }

    }
}
